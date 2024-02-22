using System.Net.WebSockets;

namespace HGSocketManager;

/// <summary>
/// The service for the session server.
/// </summary>
public interface ISessionManager
{
    /// <summary>
    /// The list of active sessions.
    /// </summary>
    public List<Session> HandledSessions { get; }

    /// <summary>
    /// The unhandles sessions.
    /// </summary>
    /// <remarks>
    /// This is used to store sessions that have not been assigned to a thread yet.
    /// </remarks>
    public List<Session> UnhandledSessions { get; }

    /// <summary>
    /// Keeps track of sessions that have been removed.
    /// </summary>
    public List<Session> RemovedSessions { get; }

    /// <summary>
    /// Connect a websocket.
    /// </summary>
    /// <param name="webSocket"></param>
    /// <param name="taskCompletionSource"></param>
    /// <returns>The session id</returns>
    public Task<Guid> Connect(WebSocket webSocket, TaskCompletionSource<object> taskCompletionSource);

    /// <summary>
    /// Disconnect a websocket.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task Disconnect(Guid id);

    /// <summary>
    /// Get a session by player id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<Session?> GetSessionByPlayerIdAsync(Guid id);

    /// <summary>
    /// Send a message to a session.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public Task SendMessageAsync(Guid id, string type, string message);

    /// <summary>
    /// Send a message to all sessions.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public Task SendMessageToAllAsync(string type, string message);

    /// <summary>
    /// Send a message to all sessions in a room.
    /// </summary>
    /// <param name="roomId"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public Task SendMessageToRoomAsync(Guid roomId, string type, string message);

    /// <summary>
    /// Send a message to a player.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public Task SendMessageToPlayerAsync(Guid playerId, string type, string message);
}

