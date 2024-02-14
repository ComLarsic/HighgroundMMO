using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using HGChat;
using HGSocketManager;
using HGSocketManager.Exceptions;

namespace HighgroundServer;

/// <summary>
/// Represents a get chat message.
/// </summary>
[Serializable]
public struct GetChatResponse
{
    [JsonPropertyName("exception")]
    public string? Exception { get; set; }

    [JsonPropertyName("messages")]
    public List<ChatMessage>? Messages { get; set; }
}

/// <summary>
/// Handles the get chat message.
/// </summary>
public class GetChatHandler(ISessionManager sessionManager, IChatService chatService) : IMessageHandler
{
    public string MessageType => "get-chat";

    public async Task Handle(Session session, string? message)
    {
        try
        {
            if (session.RoomId == null)
                throw new SessionNotInRoomException(session.Id);
            var chatMessages = chatService.GetMessages(session.RoomId.Value);
            await SendGetChatResponse(session, chatMessages);
        }
        catch (Exception e)
        {
            await SendGetChatResponse(session, null, e);
            return;
        }
    }

    /// <summary>
    /// Sends the get chat response.
    /// </summary>
    /// <param name="session"></param>
    /// <param name="chatMessages"></param>
    /// <param name="exception"></param>
    /// <returns></returns>
    private async Task SendGetChatResponse(Session session, List<ChatMessage>? chatMessages, Exception? exception = null)
    {
        var response = JsonSerializer.Serialize(new GetChatResponse
        {
            Messages = chatMessages,
            Exception = exception?.Message,
        });
        await sessionManager.SendMessageAsync(session.Id, "get-chat-response", response);
    }
}