
namespace HGChat;

/// <summary>
/// The service for chat.
/// </summary>
public class ChatService : IChatService
{
    /// <summary>
    /// The messages for the chat.
    /// </summary>
    private Dictionary<Guid, List<ChatMessage>> Messages { get; } = [];

    /// <inheritdoc />
    public List<ChatMessage> GetMessages(Guid room)
    {
        if (!Messages.ContainsKey(room))
            Messages[room] = [];
        return Messages[room];
    }

    /// <inheritdoc />
    public void SendMessage(Guid room, ChatMessage message)
    {
        if (!Messages.ContainsKey(room))
            Messages[room] = [];
        Messages[room].Add(message);
    }
}
