using System.Text.Json.Serialization;

namespace HGWorld.Player;

/// <summary>
/// The data for the player.
/// </summary>
[Serializable]
public class PlayerData
{
    /// <summary>
    /// The id of the player.
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// The input from the player.
    /// </summary>
    [JsonPropertyName("input")]
    public PlayerInput Input { get; set; }

    /// <summary>
    /// The air time of the player.
    /// </summary>
    [JsonPropertyName("airTime")]
    public float AirTime { get; set; }
}
