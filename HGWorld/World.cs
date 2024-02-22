using System.Numerics;
using System.Runtime.CompilerServices;
using HGPhysics;
using HGWorld.Player;
using HighgroundRooms;

namespace HGWorld;

/// <summary>
/// The game world.
/// </summary>
public class World
{
    /// <summary>
    /// The room id of the world.
    /// </summary>
    public Guid RoomId { get; private set; }

    /// <summary>
    /// The entities in the world.
    /// </summary>
    public List<Entity> Entities { get; private set; } = [];

    /// <summary>
    /// The playerdata for the world.
    /// </summary>
    public List<PlayerData> PlayerData { get; } = [];

    /// <summary>
    /// The physics world for the world.
    /// This handles the physics of the game.
    /// </summary>
    private PhyscisWorld _physicsWorld = new();

    /// <summary>
    /// The updates since the last tick.
    /// </summary>
    private WorldUpdates _updates = WorldUpdates.Empty;

    public World(Room room)
    {
        RoomId = room.Id;

        // Add the players as entities in the world.
        foreach (var player in room.Players)
            SpawnPlayer(player);

        // Spawn a massive platform for the players to stand on.
        SpawnStaticEntity(new Vector2(-10000 / 2, 500), new Vector2(10000, 100));
        SpawnStaticEntity(new Vector2(500 / 2, 200), new Vector2(500, 48));
        SpawnStaticEntity(new Vector2(500 / 2, 700), new Vector2(500, 48));
        SpawnStaticEntity(new Vector2(1000 / 2, 400), new Vector2(1000, 48));
    }

    /// <summary>
    /// Spawn a new entity in the world.
    /// </summary>
    public Entity SpawnEntity(Entity entity)
    {
        Entities.Add(entity);
        _updates.AddedEntities.Add(entity);
        return entity;
    }

    /// <summary>
    /// Spawn a player in the world.
    /// </summary>
    /// <param name="playerId">The id of the player.</param>
    public Entity SpawnPlayer(Guid playerId)
    {
        // Create a new physics object for the player.
        var physicsObject = _physicsWorld.AddObject(new Vector2(0, 0), PhysicsObjectType.Dynamic, new Vector2(0, 0));
        var collider = new BoxCollider
        {
            Position = new Vector2(0, 0),
            Size = new Vector2(96, 96),
            IsTrigger = false
        };
        _physicsWorld.Colliders.Add(collider);

        // Set the collider for the physics object.
        physicsObject.ColliderId = collider.Id;

        // Create a new entity for the player.
        var entity = new Entity
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            PhysicsObject = physicsObject,
            Collider = collider,
            EntityData = []
        };
        Entities.Add(entity);
        _updates.AddedEntities.Add(entity);

        // Add the player data to the player data list.
        var playerData = new PlayerData
        {
            Id = playerId,
            Input = new PlayerInput()
        };
        PlayerData.Add(playerData);
        entity.PlayerData = playerData;
        return entity;
    }

    /// <summary>
    /// Spawn a static entity in the world.
    /// </summary>
    /// <param name="position">The position of the entity.</param>
    /// <param name="size">The size of the entity.</param>
    /// <returns>The entity that was spawned.</returns>
    public Entity SpawnStaticEntity(Vector2 position, Vector2 size)
    {
        var physicsObject = _physicsWorld.AddObject(position, PhysicsObjectType.Static, new Vector2(0, 0));
        var collider = new BoxCollider
        {
            Position = position,
            Size = size,
            IsTrigger = false
        };
        _physicsWorld.Colliders.Add(collider);
        physicsObject.ColliderId = collider.Id;
        var entity = new Entity
        {
            Id = Guid.NewGuid(),
            PhysicsObject = physicsObject,
            Collider = collider,
            EntityData = []
        };
        Entities.Add(entity);
        _updates.AddedEntities.Add(entity);
        return entity;
    }

    /// <summary>
    /// Remove an entity from the world.
    /// </summary>
    /// <param name="entityId">The id of the entity.</param>
    public void RemoveEntity(Guid entityId)
    {
        var entity = Entities.FirstOrDefault(e => e.Id == entityId);
        if (entity == null)
            return;
        Entities.Remove(entity);
        _updates.RemovedEntities.Add(entityId);

        if (entity.PhysicsObject != null)
            _physicsWorld.RemoveObject(entity.PhysicsObject);
        if (entity.PhysicsObject?.ColliderId != null)
            _physicsWorld.Colliders.Remove(_physicsWorld.Colliders.First(c => c.Id == entity.PhysicsObject.ColliderId));
    }

    /// <summary>
    /// Remove a player from the world.
    /// </summary>
    /// <param name="playerId">The id of the player.</param>
    /// <returns>The entity that was removed.</returns>
    public Entity? RemovePlayer(Guid playerId)
    {
        var entity = Entities.FirstOrDefault(e => e.PlayerId == playerId);
        RemoveEntity(entity?.Id ?? Guid.Empty);
        return entity;
    }

    /// <summary>
    /// Get a player entity by player id.
    /// </summary>
    /// <param name="playerId">The id of the player.</param>
    /// <returns>The player entity.</returns>
    public Entity? GetPlayerEntity(Guid playerId)
        => Entities.FirstOrDefault(e => e.PlayerId == playerId);


    /// <summary>
    /// Get a world update with all the entities in the world.
    /// This is to send the initial state of the world to a client.
    /// </summary>
    /// <returns>The world update.</returns>
    public WorldUpdates GetWorldAsUpdate()
    {
        var update = WorldUpdates.Empty;
        update.AddedEntities.AddRange(Entities);
        return update;
    }

    /// <summary>
    /// Update the world every game tick.
    /// Also returns the updates to the world.
    /// </summary>
    /// <returns>The entities that have moved.</returns>
    public WorldUpdates Tick(double delta)
    {
        // Process the user input.
        var updatedPlayers = UpdatePlayerData(Entities.Where(e => e.PlayerId != Guid.Empty).ToList(), delta);
        _updates.UpdatedEntities.AddRange(updatedPlayers);

        // Update the physics world.
        var movedObjects = _physicsWorld.Update(delta);
        var movedEntities = Entities.Where(e => movedObjects.Contains(e.PhysicsObject)).ToList();
        foreach (var entity in movedEntities)
        {
            // Add the entity to the updates.
            if (!_updates.UpdatedEntities.Contains(entity))
                _updates.UpdatedEntities.Add(entity);
        }

        // Return the updates to the world.
        _updates.DeltaTime = delta;
        var updates = _updates;
        // Clear the updates for the next tick.
        // Removed entities are not cleared because they are not updated every tick.
        // And the client might not have received the update yet.
        _updates = new WorldUpdates
        {
            AddedEntities = [],
            RemovedEntities = _updates.RemovedEntities,
            UpdatedEntities = []
        };
        return updates;
    }

    /// <summary>
    /// Set the input for a player.
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="vector"></param>
    public void SetPlayerInput(Guid playerId, Vector2 direction, bool jumping)
    {
        if (playerId == Guid.Empty)
            return;
        if (!PlayerData.Any(p => p.Id == playerId))
            throw new InvalidOperationException("Player does not exist in the world.");

        var playerData = PlayerData.FirstOrDefault(p => p.Id == playerId);
        if (playerData == null)
            return;
        playerData.Input = new PlayerInput
        {
            Direction = direction,
            Jumping = jumping,
            WasJumping = playerData.Input.Jumping
        };
    }

    /// <summary>
    /// Update the player data in the world.
    /// </summary>
    /// <param name="playerEntities">The player entities to update.</param>
    /// <returns>The updated player entities.</returns>
    public List<Entity> UpdatePlayerData(List<Entity> playerEntities, double delta)
    {
        var updatedEntities = new List<Entity>();
        foreach (var entity in playerEntities)
        {
            // Get the player data for the player.
            var data = entity.PlayerData;
            if (data == null)
                continue;
            var wasProcessed = ProcessPlayerInput(data.Id, data.Input, delta);
            if (wasProcessed)
                _updates.UpdatedEntities.Add(entity);

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
    private bool ProcessPlayerInput(Guid playerId, PlayerInput input, double delta)
    {
        const float jumpForce = 180f;
        const float moveSpeed = 600f;
        const float jumpTime = 0.05f;

        // Find the entity associated with the player.
        var entity = Entities.FirstOrDefault(e => e.PlayerId == playerId);
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
