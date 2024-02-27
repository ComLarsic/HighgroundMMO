using System.ComponentModel.Design;
using System.Net.WebSockets;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using HGPlayers;
using HGSocketManager.Exceptions;
using HGRooms;
using HGRooms.Exceptions;

namespace HGSocketManager;

/// <summary>
/// The service for session management
/// </summary>
public class SessionManager(IRoomManager roomManager, IPlayerManager playerManager) : ISessionManager
{
    /// <inheritdoc />
    public List<Session> HandledSessions { get => GetHandledSessions(); }

    private List<Session> _handledSessions = [];
    private readonly object _handledSessionsLock = new();

    /// </inheritdoc>
    public List<Session> UnhandledSessions { get => GetUnhandledSessions(); }
    private List<Session> _unhandledSessions = [];
    private readonly object _unhandledSessionsLock = new();

    /// <inheritdoc />
    public List<Session> RemovedSessions { get => GetRemovedSessions(); }
    private List<Session> _removedSessions = [];
    private readonly object _removedSessionsLock = new();


    /// <summary>
    /// The getter for the handledSessions collection.
    /// </summary>
    /// <returns></returns>
    private List<Session> GetHandledSessions()
    {
        lock (_handledSessionsLock)
        {
            return _handledSessions;
        }
    }

    /// <summary>
    /// The getter for the unhandledSessions collection.
    /// </summary>
    /// <returns></returns>
    private List<Session> GetUnhandledSessions()
    {
        lock (_unhandledSessionsLock)
        {
            return _unhandledSessions;
        }
    }

    /// <summary>
    /// The getter for the removedSessions collection.
    /// </summary>
    /// <returns></returns>
    private List<Session> GetRemovedSessions()
    {
        lock (_removedSessionsLock)
        {
            return _removedSessions;
        }
    }

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

            // Close the socket.
            if (session.Socket.State == WebSocketState.Open)
                await session.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the socket manager", CancellationToken.None);

            // Remove the session from the list of handled sessions.
            HandledSessions.Remove(session);

            // Add the session to the list of removed sessions.
            RemovedSessions.Add(session);

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

        }
        catch { }
    }

    /// <inheritdoc />
    public Task SendMessageAsync(Guid id, string type, string message)
    {
        try
        {
            // Create a copy of the HandledSessions collection. This is because the collection may be modified while we are sending messages.
            var sessionsCopy = new List<Session>(HandledSessions);

            var session = sessionsCopy.First(s => s.Id == id);
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
        catch
        {
            return Task.CompletedTask;
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
        try
        {
            var room = roomManager.GetRoomAsync(roomId).Result ?? throw new RoomNotFoundException(roomId);
            var tasks = room.Players.Select(async player =>
            {
                try
                {
                    var session = await GetSessionByPlayerIdAsync(player) ?? throw new PlayerNotInSessionException(player);
                    await SendMessageAsync(session.Id, type, message);
                }
                catch { }
            });
            return Task.WhenAll(tasks);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            return Task.CompletedTask;
        }
    }

    /// <inheritdoc />
    public async Task SendMessageToPlayerAsync(Guid playerId, string type, string message)
    {
        var session = await GetSessionByPlayerIdAsync(playerId) ?? throw new PlayerNotInSessionException(playerId);
        await SendMessageAsync(session.Id, type, message);
    }

    /// <inheritdoc />
    public Task<Session?> GetSessionByPlayerIdAsync(Guid id)
    {
        try
        {
            Session? session = HandledSessions.FirstOrDefault(s => s.PlayerId == id);
            return Task.FromResult(session);
        }
        catch // Microsoft, why?
        {
            return Task.FromResult<Session?>(null);
        }
    }
}
