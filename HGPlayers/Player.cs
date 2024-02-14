using System.Text.Json.Serialization;

namespace HGPlayers;

/// <summary>
/// Represents a player.
/// </summary>
[Serializable]
public struct Player
{
    /// <summary>
    /// The id of the player.
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// The username of the player.
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; }
}
