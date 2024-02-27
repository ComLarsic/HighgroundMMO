using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HGRooms;
using HGSocketManager;

namespace HighgroundServer.Messages.Rooms;

/// <summary>
/// The get rooms response.
/// </summary>
[Serializable]
public struct GetRoomResponse
{
    /// <summary>
    /// The rooms.
    /// </summary>
    [JsonPropertyName("rooms")]
    public List<Room>? Rooms { get; set; }
}


/// <summary>
/// The handler for the get room message.
/// </summary>
public class GetRoomHandler(IRoomManager roomManager, ISessionManager sessionManager) : IMessageHandler
{

    /// <inheritdoc />
    public string MessageType => "get-room-list";

    /// <inheritdoc />
    public async Task Handle(Session session, string? message)
    {
        if (message == null)
            return;
        var rooms = await roomManager.GetRoomsAsync();
        await SendGetRoomResponse(session, rooms);
        return;
    }

    /// <summary>
    /// Send the get room response.
    /// </summary>
    /// <param name="socket"></param>
    /// <param name="rooms"></param>
    /// <returns></returns>
    private Task SendGetRoomResponse(Session session, List<Room> rooms)
        => sessionManager.SendMessageAsync(session.Id, "get-room-list-response", JsonSerializer.Serialize(new GetRoomResponse
        {
            Rooms = rooms
        }));
}
