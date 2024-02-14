using System.ComponentModel.Design;
using System.Net.WebSockets;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using HGPlayers;
using HGSocketManager.Exceptions;
using HighgroundRooms;
using HighgroundRooms.Exceptions;

namespace HGSocketManager;

/// <summary>
/// The service for session management
/// </summary>
public class SessionManager(IRoomManager roomManager, IPlayerManager playerManager) : ISessionManager
{
    /// <inheritdoc />
    public List<Session> HandledSessions { get; } = [];

    /// </inheritdoc>
    public List<Session> UnhandledSessions { get; } = [];

    /// <inheritdoc />
    public Task<Guid> Connect(WebSocket webSocket, TaskCompletionSource<object> taskCompletionSource)
    {
        var session = new Session
        {
            Id = Guid.NewGuid(),
            Socket = webSocket,
            TaskCompletionSource = taskCompletionSource
        };
        UnhandledSessions.Add(session);
        return Task.FromResult(session.Id);
    }

    /// <inheritdoc />
    public async Task Disconnect(Guid id)
    {
        // Because C# doesnt just return null if the session is not found, we have to use a try catch block.
        // Microsoft just has to be extra like that.
        try
        {
            var session = HandledSessions.First(s => s.Id == id);

            // If a player is in a room, remove them from the room.
            if (session.PlayerId != null)
            {
                var player = await playerManager.GetPlayerByIdAsync(session.PlayerId.Value);
                if (player != null && session.RoomId != null)
                {
                    var room = await roomManager.GetRoomByPlayerAsync(player.Value.Id);
                    if (room != null)
                    {
                        await roomManager.RemovePlayerFromRoomAsync(room.Id, player.Value.Id);
                        await SendMessageToRoomAsync(session.RoomId.Value, "player-left", JsonSerializer.Serialize(player));
                    }
                }
            }

            // Close the socket.
            if (session.Socket.State == WebSocketState.Open)
                await session.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the socket manager", CancellationToken.None);

            // Remove the session from the list of handled sessions.
            HandledSessions.Remove(session);
        }
        catch { }
    }

    /// <inheritdoc />
    public Task SendMessageAsync(Guid id, string type, string message)
    {
        try
        {
            var session = HandledSessions.First(s => s.Id == id);
            var buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Message
            {
                MessageType = type,
                Content = message
            }));
            return session.Socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (InvalidOperationException)
        {
            throw new SessionNotFoundException(id); // Again, Microsoft being extra.
        }
    }

    /// <inheritdoc />
    public Task SendMessageToAllAsync(string type, string message)
    {
        var tasks = HandledSessions.Select(async s => await SendMessageAsync(s.Id, type, message));
        return Task.WhenAll(tasks);
    }

    /// <inheritdoc />
    public Task SendMessageToRoomAsync(Guid roomId, string type, string message)
    {
        var room = roomManager.GetRoomAsync(roomId).Result ?? throw new RoomNotFoundException(roomId);
        var tasks = room.Players.Select(async player =>
        {
            var session = await GetSessionByPlayerIdAsync(player) ?? throw new PlayerNotInSessionException(player);
            await SendMessageAsync(session.Id, type, message);
        });
        return Task.WhenAll(tasks);
    }

    /// <inheritdoc />
    public Task<Session?> GetSessionByPlayerIdAsync(Guid id)
    {
        try
        {
            Session? session = HandledSessions.FirstOrDefault(s => s.PlayerId == id);
            return Task.FromResult(session);
        }
        catch (InvalidOperationException) // Microsoft, why?
        {
            return Task.FromResult<Session?>(null);
        }
    }
}
