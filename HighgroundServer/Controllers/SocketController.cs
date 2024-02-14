using HGSocketManager;
using Microsoft.AspNetCore.Mvc;

namespace HighgroundServer.Controllers;

/// <summary>
/// The controller for the socket server.
/// </summary>
[ApiController]
public class SocketController : ControllerBase
{
    /// <summary>
    /// Handle the websocket request.
    /// </summary>
    /// <returns></returns>
    [HttpGet("/ws")]
    public async Task Get([FromServices] ISessionManager socketManager)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            var taskCompletionSource = new TaskCompletionSource<object>();
            var id = await socketManager.Connect(webSocket, taskCompletionSource);
            await taskCompletionSource.Task;
        }
        else
        {
            HttpContext.Response.StatusCode = 400;
        }
    }
}
