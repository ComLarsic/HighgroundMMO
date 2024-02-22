using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using HGPhysics;
using HGSerializationUtils;
using HGWorld.Player;

namespace HGWorld;

/// <summary>
/// Represents an entity in the game world.
/// </summary>
[Serializable]
public class Entity
{
    /// <summary>
    /// The id of the entity.
    /// </summary>
    [JsonPropertyName("id")]
    public required Guid Id { get; set; }

    /// <summary>
    /// The id of the player associated with the entity.
    /// </summary>
    [JsonPropertyName("playerId")]
    public Guid? PlayerId { get; set; }

    /// <summary>
    /// Check if the entity is a player.
    /// </summary>
    public bool IsPlayer => PlayerId.HasValue;

    /// <summary>
    /// The physics object of the entity.
    /// </summary>
    [JsonPropertyName("physicsObject")]
    public required PhysicsObject PhysicsObject { get; set; }

    /// <summary>
    /// The player data of the entity.
    /// </summary>
    [JsonPropertyName("playerData")]
    public PlayerData? PlayerData { get; set; }

    /// <summary>
    /// The additional data of the entity.
    /// </summary>
    [JsonPropertyName("entityData")]
    public required Dictionary<string, JsonElement> EntityData { get; set; }

    /// <summary>
    /// The collider of the entity.
    /// </summary>
    [JsonPropertyName("collider")]
    public BoxCollider? Collider { get; set; }

}
