using System.Numerics;
using System.Text.Json.Serialization;
using HGSerializationUtils;

namespace HGPhysics;

/// <summary>
/// The type of the physics object.
/// </summary>
[Serializable]
public enum PhysicsObjectType
{
    Static,
    Dynamic
}

/// <summary>
/// The physics object in the game.
/// </summary>
[Serializable]
public class PhysicsObject
{
    /// <summary>
    /// The id of the object.
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// The collider of the object.
    /// </summary>
    /// <value></value>
    [JsonPropertyName("colliderId")]
    public Guid? ColliderId { get; set; }

    /// <summary>
    /// The type of the object.
    /// </summary>
    [JsonPropertyName("type")]
    public PhysicsObjectType Type { get; set; } = PhysicsObjectType.Dynamic;

    /// <summary>
    /// The position of the object.
    /// </summary>
    [JsonPropertyName("position")]
    [JsonConverter(typeof(Vector2Converter))]
    public Vector2 Position { get; set; }

    /// <summary>
    /// The velocity of the object.
    /// </summary>
    [JsonPropertyName("velocity")]
    [JsonConverter(typeof(Vector2Converter))]
    public Vector2 Velocity { get; set; }

    /// <summary>
    /// The mass of the object.
    /// </summary>
    [JsonPropertyName("mass")]
    public float Mass { get; set; } = 1;

    /// <summary>
    /// Checks if an object is on the ground.
    /// </summary>
    [JsonPropertyName("isOnGround")]
    public bool IsOnGround { get; set; }
}