using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using HGSocketManager;
using HighgroundRooms;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using Microsoft.Extensions.Hosting;

namespace HGWorld;

/// <summary>
/// The background service for the game world that updates the world every game tick.
/// </summary>
public class GameWorldService(IWorldManager worldManager, IRoomManager roomManager, ISessionManager sessionManager) : BackgroundService
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
                    var world = new World(room);
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
            var worldUpdates = world.Tick(deltaTime);
            if (worldUpdates.IsEmpty)
                continue;

            await sessionManager.SendMessageToRoomAsync(room.Id, "world-update", JsonSerializer.Serialize(worldUpdates));
        };
    }
}


