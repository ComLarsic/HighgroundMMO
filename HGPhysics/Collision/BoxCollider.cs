using System.Numerics;
using System.Text.Json.Serialization;
using HGSerializationUtils;

namespace HGPhysics;

/// <summary>
/// An AABB box collider.s
/// </summary>
[Serializable]
public class BoxCollider
{
    /// <summary>
    /// The id of the collider.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// The position of the collider.
    /// </summary>
    [JsonPropertyName("position")]
    [JsonConverter(typeof(Vector2Converter))]
    public Vector2 Position { get; set; }

    /// <summary>
    /// The size of the collider.
    /// </summary>
    [JsonPropertyName("size")]
    [JsonConverter(typeof(Vector2Converter))]
    public Vector2 Size { get; set; }

    /// <summary>
    /// Whether the collider is a trigger.
    /// </summary>
    [JsonPropertyName("isTrigger")]
    public bool IsTrigger { get; set; }

    /// <summary>
    /// Check if the collider is colliding with another collider.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool IsCollidingWith(BoxCollider other)
    {
        return Position.X < other.Position.X + other.Size.X &&
               Position.X + Size.X > other.Position.X &&
               Position.Y < other.Position.Y + other.Size.Y &&
               Position.Y + Size.Y > other.Position.Y;
    }
}
