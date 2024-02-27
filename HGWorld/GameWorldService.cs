using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using HGScript;
using HGSocketManager;
using HGRooms;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using Microsoft.Extensions.Hosting;
using HGWorld.Player;

namespace HGWorld;

/// <summary>
/// The background service for the game world that updates the world every game tick.
/// </summary>
public class GameWorldService(
    IWorldManager worldManager,
    IRoomManager roomManager,
    ISessionManager sessionManager
) : BackgroundService
{
    /// <summary>
    /// The tick rate for checking if a room has a world in milliseconds.
    /// </summary>
    public static readonly uint RoomCheckRateMs = 1000;

    /// <summary>
    /// The tick rate of the game world in milliseconds.
    /// </summary>
    public static readonly uint TickRateMs = 4;

    /// <summary>
    /// The threads that update each game world.
    /// </summary>
    private readonly Dictionary<Guid, Task> _worldThreads = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // The timer to check if a room has a world.
        using var roomCheckTimer = new PeriodicTimer(new TimeSpan(RoomCheckRateMs * TimeSpan.TicksPerMillisecond));

        // Check if each room has a world.
        while (await roomCheckTimer.WaitForNextTickAsync(stoppingToken))
        {
            foreach (var room in await roomManager.GetRoomsAsync())
            {
                if (!worldManager.HasWorld(room.Id))
                {
                    // Create a new world for the room.
                    var world = new World(room, "Assets/Packs/BaseHighground/_Load.lua");
                    world.Initialize();
                    worldManager.Worlds.Add(room.Id, world);

                    // Start a new thread to update the world.
                    var thread = new Task(async () => await UpdateLoopWorld(world, stoppingToken), stoppingToken);
                    _worldThreads.Add(room.Id, thread);
                    thread.Start();
                }
            }
        }
    }

    /// <summary>
    /// The game world update loop.
    /// </summary>
    /// <param name="world"></param>
    /// <returns></returns>
    private async Task UpdateLoopWorld(World world, CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(new TimeSpan(TickRateMs * TimeSpan.TicksPerMillisecond));

        var now = TimeOnly.FromDateTime(DateTime.Now);
        var lastTick = now;
        double deltaTime = 0;
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            now = TimeOnly.FromDateTime(DateTime.Now);
            deltaTime = (now - lastTick).TotalSeconds;
            lastTick = now;

            // Remove the player entities that are no longer in the room.
            var room = await roomManager.GetRoomAsync(world.RoomId);
            var removedPlayers = sessionManager.RemovedSessions.Where(s => s.RoomId == room.Id).Select(s => s.PlayerId).ToList();
            foreach (var playerId in removedPlayers)
                if (playerId != null && world.Entities.Any(e => e.PlayerId == playerId))
                    world.RemovePlayer(playerId.Value);

            // Update the world.
            var worldUpdates = TickWorld(world, deltaTime);
            if (worldUpdates.IsEmpty)
                continue;

            await sessionManager.SendMessageToRoomAsync(room.Id, "world-update", JsonSerializer.Serialize(worldUpdates));
        };
    }

    /// <summary>
    /// Update the world every game tick.
    /// Also returns the updates to the world.
    /// </summary>
    /// <returns>The entities that have moved.</returns>
    public static WorldUpdates TickWorld(World world, double delta)
    {
        // Process the user input.
        var players = world.Entities.Where(e => e.PlayerId != null).ToList();
        var updatedPlayers = UpdatePlayerData(world, players, delta);
        world.Updates.UpdatedEntities.AddRange(updatedPlayers);

        // Update the physics world.
        var movedObjects = world.PhysicsWorld.Update(delta);
        var movedEntities = world.Entities.Where(e => movedObjects.Contains(e.PhysicsObject)).ToList();
        foreach (var entity in movedEntities)
        {
            // Add the entity to the updates.
            if (!world.Updates.UpdatedEntities.Contains(entity))
                world.Updates.UpdatedEntities.Add(entity);
        }

        // Run the update scripts.
        RunUpdateScripts(world, delta);

        // Return the updates to the world.
        world.Updates.DeltaTime = delta;
        var updates = world.Updates;
        world.Updates = new WorldUpdates
        {
            AddedEntities = [],
            RemovedEntities = updates.RemovedEntities,
            UpdatedEntities = []
        };
        return updates;
    }

    /// <summary>
    /// Run the update methods for the scripts.
    /// </summary>
    /// <param name="delta">The time since the last tick.</param>
    public static void RunUpdateScripts(World world, double delta)
    {
        world.ScriptManager.CallMethodOnAllScripts("Update", world, delta);
    }

    /// <summary>
    /// Update the player data in the world.
    /// </summary>
    /// <param name="playerEntities">The player entities to update.</param>
    /// <returns>The updated player entities.</returns>
    public static List<Entity> UpdatePlayerData(World world, List<Entity> playerEntities, double delta)
    {
        var updatedEntities = new List<Entity>();
        foreach (var entity in playerEntities)
        {
            // Get the player data for the player.
            var data = entity.PlayerData;
            if (data == null)
                continue;
            var wasProcessed = ProcessPlayerInput(world, data.Id, data.Input, delta);
            if (wasProcessed)
                world.Updates.UpdatedEntities.Add(entity);

            // Update the air time of the player.
            if (!entity.PhysicsObject.IsOnGround)
                data.AirTime += (float)delta;
            else
                data.AirTime = 0;
        }
        return updatedEntities;
    }

    /// <summary>
    /// Process a player's input.
    /// </summary>
    /// <param name="playerId">The id of the player.</param>
    /// <param name="vector">The input vector from the player.</param>
    /// <param name="delta">The time since the last tick.</param>
    /// <returns>True if the input was processed, false otherwise.</returns>
    private static bool ProcessPlayerInput(World world, Guid playerId, PlayerInput input, double delta)
    {
        const float jumpForce = 180f;
        const float moveSpeed = 600f;
        const float jumpTime = 0.05f;

        // Find the entity associated with the player.
        var entity = world.Entities.FirstOrDefault(e => e.PlayerId == playerId);
        if (entity == null)
            return false;

        // Find the player data associated with the player.
        if (entity.PlayerData == null)
            return false;

        // Apply the input to the entity.
        var physicsObject = entity.PhysicsObject;
        if (physicsObject == null)
            return false;

        var velocity = physicsObject.Velocity;
        var position = physicsObject.Position;

        // Apply the input to the velocity.
        velocity.X = input.Direction.X * moveSpeed * (float)delta;

        // Apply the jump.
        if (input.Jumping && entity.PlayerData.AirTime < jumpTime)
        {
            // Determine the velocity of the jump by lowering the acceleration with the air time.
            var jumpAcceleration = jumpForce * (1 - entity.PlayerData.AirTime / jumpTime);
            velocity.Y -= jumpAcceleration * (float)delta;
        }
        // Apply higher gravity if the player is not jumping.
        entity.PhysicsObject.Mass = input.Jumping ? .8f : 1f;

        // Set the velocity of the physics object.
        physicsObject.Velocity = velocity;
        return true;
    }
}


