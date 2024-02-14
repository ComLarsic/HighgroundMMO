namespace HGSocketManager;

/// <summary>
/// The handler for the socket messages.
/// </summary>
public interface IMessageHandler
{
    /// <summary>
    /// The type of message to handle.
    /// </summary>
    public string MessageType { get; }

    /// <summary>
    /// Handle the message.
    /// </summary>
    public Task Handle(Session session, string? message);
}
