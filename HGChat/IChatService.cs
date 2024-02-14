namespace HGChat;

/// <summary>
/// The service for chat.
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Send a message to the chat.
    /// </summary>
    /// <param name="room"></param>
    /// <param name="message"></param>
    public void SendMessage(Guid room, ChatMessage message);

    /// <summary>
    /// Get the messages from the chat.
    /// </summary>
    /// <param name="room"></param>
    /// <returns></returns>
    public List<ChatMessage> GetMessages(Guid room);
}
