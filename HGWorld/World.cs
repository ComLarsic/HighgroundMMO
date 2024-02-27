using System.Numerics;
using System.Runtime.CompilerServices;
using HGPhysics;
using HGWorld.Player;
using HGRooms;
using HGScript;

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
    public PhyscisWorld PhysicsWorld = new();

    /// <summary>
    /// The updates since the last tick.
    /// </summary>
    public WorldUpdates Updates = WorldUpdates.Empty;

    /// <summary>
    /// The script manager for the world.
    /// </summary>
    /// <value></value>
    public ScriptManager ScriptManager { get; } = new();

    public World(Room room, string? initScript)
    {
        RoomId = room.Id;

        // Add the libraries to the script manager.
        ScriptManager.Libraries.Add("HGScript");
        ScriptManager.Libraries.Add("HGPhysics");
        ScriptManager.Libraries.Add("HGWorld");
        ScriptManager.Libraries.Add("HGRooms");
        ScriptManager.Libraries.Add("HGPlayers");
        ScriptManager.Libraries.Add("HGChat");
        ScriptManager.Libraries.Add("System.Numerics");
        ScriptManager.Libraries.Add("System");

        // Load the init script.
        if (initScript != null)
        {
            var script = ScriptManager.LoadScript(initScript);
            script.Call("Init", this);
        }
    }

    /// <summary>
    /// Initialize the world.
    /// </summary>
    public void Initialize()
    {
        // Initialize the scripts.
        ScriptManager.CallMethodOnAllScripts("Init", this);
    }

    /// <summary>
    /// Spawn a new entity in the world.
    /// </summary>
    public Entity SpawnEntity(Entity entity)
    {
        Entities.Add(entity);
        Updates.AddedEntities.Add(entity);
        return entity;
    }

    /// <summary>
    /// Spawn a player in the world.
    /// </summary>
    /// <param name="playerId">The id of the player.</param>
    public Entity SpawnPlayer(Guid playerId)
    {
        // Create a new physics object for the player.
        var physicsObject = PhysicsWorld.AddObject(new Vector2(0, 0), PhysicsObjectType.Dynamic, new Vector2(0, 0));
        var collider = new BoxCollider
        {
            Position = new Vector2(0, 0),
            Size = new Vector2(96, 96),
            IsTrigger = false
        };
        PhysicsWorld.Colliders.Add(collider);

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
        Updates.AddedEntities.Add(entity);

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
    /// <param name="x">The x position of the entity.</param>
    /// <param name="y">The y position of the entity.</param>
    /// <param name="width">The width of the entity.</param>
    /// <param name="height">The height of the entity.</param>
    /// <returns>The entity that was spawned.</returns>
    public Entity SpawnStaticEntity(float x, float y, float width, float height)
    {
        var position = new Vector2(x, y);
        var size = new Vector2(width, height);
        var physicsObject = PhysicsWorld.AddObject(position, PhysicsObjectType.Static, new Vector2(0, 0));
        var collider = new BoxCollider
        {
            Position = position,
            Size = size,
            IsTrigger = false
        };
        PhysicsWorld.Colliders.Add(collider);
        physicsObject.ColliderId = collider.Id;
        var entity = new Entity
        {
            Id = Guid.NewGuid(),
            PhysicsObject = physicsObject,
            Collider = collider,
            EntityData = []
        };
        Entities.Add(entity);
        Updates.AddedEntities.Add(entity);
        return entity;
    }

    /// <summary>
    /// Spawn a dynamic entity in the world.
    /// </summary>
    /// <param name="x">The x position of the entity.</param>
    /// <param name="y">The y position of the entity.</param>
    /// <param name="width">The width of the entity.</param>
    /// <param name="height">The height of the entity.</param>
    /// <returns>The entity that was spawned.</returns>
    public Entity SpawnDynamicEntity(float x, float y, float width, float height)
    {
        var position = new Vector2(x, y);
        var size = new Vector2(width, height);
        var physicsObject = PhysicsWorld.AddObject(position, PhysicsObjectType.Dynamic, new Vector2(0, 0));
        var collider = new BoxCollider
        {
            Position = position,
            Size = size,
            IsTrigger = false
        };
        PhysicsWorld.Colliders.Add(collider);
        physicsObject.ColliderId = collider.Id;
        var entity = new Entity
        {
            Id = Guid.NewGuid(),
            PhysicsObject = physicsObject,
            Collider = collider,
            EntityData = []
        };
        Entities.Add(entity);
        Updates.AddedEntities.Add(entity);
        return entity;
    }

    /// <summary>
    /// Get an entity by id.
    /// </summary>
    /// <param name="entityId">The id of the entity.</param>
    /// <returns>The entity with the id.</returns>
    public Entity? GetEntity(Guid entityId)
        => Entities.FirstOrDefault(e => e.Id == entityId);

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
        Updates.RemovedEntities.Add(entityId);

        if (entity.PhysicsObject != null)
            PhysicsWorld.RemoveObject(entity.PhysicsObject);
        if (entity.PhysicsObject?.ColliderId != null)
            PhysicsWorld.Colliders.Remove(PhysicsWorld.Colliders.First(c => c.Id == entity.PhysicsObject.ColliderId));
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
}
