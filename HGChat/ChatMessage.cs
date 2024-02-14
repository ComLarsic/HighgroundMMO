using System.Text.Json.Serialization;
using HGPlayers;

namespace HGChat;

/// <summary>
/// A message from the chat.
/// </summary>
[Serializable]
public struct ChatMessage
{
    /// <summary>
    /// The player
    /// </summary>
    [JsonPropertyName("player")]
    public Player? Player { get; set; }

    /// <summary>
    /// The content of the message.s
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; }
}
