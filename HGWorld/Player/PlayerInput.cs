using System.Numerics;
using System.Text.Json.Serialization;
using HGSerializationUtils;

namespace HGWorld.Player;


/// <summary>
/// The input from the player.
/// </summary>
[Serializable]
public struct PlayerInput
{
    /// <summary>
    /// The direction the player is moving.
    /// </summary>
    [JsonPropertyName("direction")]
    [JsonConverter(typeof(Vector2Converter))]
    public Vector2 Direction { get; set; }

    /// <summary>
    /// Whether the player is jumping.
    /// </summary>
    [JsonPropertyName("jumping")]
    public bool Jumping { get; set; }

    /// <summary>
    /// Check if the player was also jumping last frame.
    /// </summary>
    [JsonPropertyName("wasJumping")]
    public bool WasJumping { get; set; }
}
