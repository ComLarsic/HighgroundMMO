using System.Text.Json.Serialization;

namespace HGSocketManager;

/// <summary>
/// A message from the socket.
/// </summary>
[Serializable]
public class Message
{
    /// <summary>
    /// The type of message.
    /// </summary>
    [JsonPropertyName("type")]
    public required string MessageType { get; set; }

    /// <summary>
    /// The content of the message.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}
