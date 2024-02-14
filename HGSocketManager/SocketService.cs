namespace HGSocketManager;

using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

/// <summary>
/// The service for the socket server.
/// This runs in the background and listens for messages from the socket.
/// Runs at a time interval to check for new messages.
/// </summary>
public class SocketService(ISessionManager socketManager, IEnumerable<IMessageHandler> messageHandlers) : BackgroundService
{
    /// <summary>
    /// The buffer size for the socket.
    /// </summary>
    public const uint BufferSize = 1024 * 4;

    /// <summary>
    /// The time interval to check for new messages.
    /// </summary>
    public static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(50);

    /// <summary>
    /// The threads for each session.
    /// </summary>
    public Dictionary<Guid, Task> SessionThreads { get; } = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Checks for new sockets and starts a new thread for each one.
        using var timer = new PeriodicTimer(Interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var unhandledSessions = socketManager.UnhandledSessions.ToList();
            foreach (var session in unhandledSessions)
            {
                var thread = new Task(async () => await Receive(session), stoppingToken);
                SessionThreads.Add(session.Id, thread);
                thread.Start();
                socketManager.HandledSessions.Add(session);
                socketManager.UnhandledSessions.Remove(session);
            }

        }
        Console.WriteLine("Socket service stopped");
    }

    /// <summary>
    /// Receive messages from the client
    /// </summary>
    /// <param name="socket"></param>
    /// <returns></returns>
    private async Task Receive(Session session)
    {
        while (CheckSession(session))
        {
            try
            {
                var buffer = new byte[BufferSize];
                var result = await session.Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                // If the message is a close message, disconnect the session
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await socketManager.Disconnect(session.Id);
                    return;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var messageObject = JsonSerializer.Deserialize<Message>(message);
                if (messageObject != null)
                    await HandleMessage(session, messageObject);
            }
            catch
            {
                // If the session is closed, remove it from the session list and disconnect it.
                await socketManager.Disconnect(session.Id);
            }
        }

        // If the session is closed, remove it from the session list and disconnect it.
        await socketManager.Disconnect(session.Id);
    }

    /// <summary>
    /// Check if the session is still open.
    /// </summary>
    /// <param name="socket"></param>
    /// <returns>Whether or not the session is valid</returns>
    private static bool CheckSession(Session session)
    {
        if (session.Socket.State != WebSocketState.Open)
            return false;
        if (session.Socket.CloseStatus.HasValue)
            return false;

        return true;
    }

    /// <summary>
    /// Get the message handler services and handle the message
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private async Task HandleMessage(Session session, Message message)
    {
        var messageHandlerList = messageHandlers.ToList();
        var tasks = messageHandlerList.Select(async messageHandler =>
        {
            if (messageHandler.MessageType == message.MessageType)
            {
                try
                {
                    await messageHandler.Handle(session, message.Content);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        });
        await Task.WhenAll(tasks);
    }
}