using System.Text.Json.Serialization;

namespace HGRooms;

/// <summary>
/// A room in the game.
/// </summary>
[Serializable]
public class Room
{
    /// <summary>
    /// The id of the room.
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The name of the room.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The capacity of the room.
    /// </summary>
    [JsonPropertyName("capacity")]
    public int Capacity { get; set; }

    /// <summary>
    /// The current players in the room.
    /// </summary>
    [JsonPropertyName("players")]
    public List<Guid> Players { get; set; } = [];
}
