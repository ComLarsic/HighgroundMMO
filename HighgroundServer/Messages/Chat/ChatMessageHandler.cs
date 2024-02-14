using System.Text.Json;
using System.Text.Json.Serialization;
using HGChat;
using HGPlayers;
using HGSocketManager;
using HGSocketManager.Exceptions;

namespace HighgroundServer.Messages.Chat;

/// <summary>
/// The chat message.
/// </summary>
[Serializable]
public struct ChatSentMessage // Weird name, but it's the message for the chat message.
{
    [JsonPropertyName("content")]
    public string Content { get; set; }
}

/// <summary>
/// The chat message handler.
/// </summary>
public class ChatMessageHandler(IChatService chatService, ISessionManager sessionManager, IPlayerManager playerManager) : IMessageHandler
{
    public string MessageType => "chat";

    public async Task Handle(Session session, string? message)
    {
        if (message == null)
            return;

        // Get the room from the session.
        try
        {
            // Get the player from the player manager.
            var player = await playerManager.GetPlayerByIdAsync(session.PlayerId.Value);

            // Get the player from the session.
            if (session.PlayerId == null)
                throw new PlayerNotInSessionException(session.Id);

            // Get the chat message from the message.
            var messageObj = JsonSerializer.Deserialize<ChatSentMessage>(message);
            // Send the message to the chat service.
            var chatMessage = new ChatMessage
            {
                Content = messageObj.Content,
                Player = player,
            };
            if (session.RoomId == null)
                throw new SessionNotInRoomException(session.Id);
            chatService.SendMessage(session.RoomId.Value, chatMessage);

            // Broadcast the message to the room.
            var messageSent = new ChatMessage
            {
                Player = player,
                Content = chatMessage.Content,
            };
            var response = JsonSerializer.Serialize(messageSent);
            await sessionManager.SendMessageToRoomAsync(session.RoomId.Value, "chat-sent", response);
        }
        catch { }
    }
}