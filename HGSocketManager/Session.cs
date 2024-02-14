using System.Net.WebSockets;

namespace HGSocketManager;

/// <summary>
/// A message from the socket.
/// </summary>
public class Session
{
    /// <summary>
    /// The session id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The room id.
    /// </summary>
    /// <value></value>
    public Guid? RoomId { get; set; }

    /// <summary>
    /// The player id.
    /// </summary>
    public Guid? PlayerId { get; set; }

    /// <summary>
    /// The socket
    /// </summary>
    public required WebSocket Socket { get; set; }

    /// <summary>
    /// The task completion source.
    /// </summary>
    public required TaskCompletionSource<object> TaskCompletionSource { get; set; }
}