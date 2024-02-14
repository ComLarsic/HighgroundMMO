using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HGPlayers;
using HighgroundRooms;
using HGSocketManager;
using HGSocketManager.Exceptions;
using HighgroundRooms.Exceptions;

namespace HighgroundServer.Messages.Rooms;

/// <summary>
/// The join message.
/// </summary>    
[Serializable]
public struct JoinMessage
{
    [JsonPropertyName("room")]
    public Guid Room { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }
}

/// <summary>
/// The join response.
/// </summary>
[Serializable]
public struct JoinResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("player")]
    public Player? Player { get; set; }

    [JsonPropertyName("room")]
    public Guid Room { get; set; }
}

/// <summary>
/// The handler for the join message.
/// </summary>
public class JoinHandler(ISessionManager sessionManager, IRoomManager roomManager, IPlayerManager playerManager) : IMessageHandler
{
    /// <inheritdoc />
    public string MessageType => "join";

    /// <inheritdoc />
    public async Task Handle(Session session, string? message)
    {
        if (message == null)
            return;
        try
        {
            var content = JsonSerializer.Deserialize<JoinMessage>(message);
            var player = await playerManager.CreatePlayerAsync(content.Username, session.Id);
            await roomManager.AddPlayerToRoomAsync(content.Room, player.Id);
            session.RoomId = content.Room;
            session.PlayerId = player.Id;
            await SendJoinResponse(session, content.Username != null, player);
            var room = await roomManager.GetRoomAsync(content.Room) ?? throw new RoomNotFoundException(content.Room);
            await SendPlayerJoinedRoom(room, player);
        }
        catch
        {
            await SendJoinResponse(session, false);
        }
        return;
    }

    /// <summary>
    /// Send the join response.
    /// </summary>
    /// <param name="socket">The websocket connection</param>
    /// <param name="success">Wether or not it was a success</param>
    /// <returns></returns>
    private Task SendJoinResponse(Session session, bool success, Player? player = null)
        => sessionManager.SendMessageAsync(session.Id, "join-response", JsonSerializer.Serialize(new JoinResponse
        {
            Success = success,
            Player = player,
            Room = session.RoomId ?? throw new SessionNotInRoomException(session.Id)
        }));

    /// <summary>
    /// Tell the room that a player has joined.
    /// </summary>
    /// <param name="room">The room</param>
    /// <param name="player">The player</param>
    /// <returns></returns>
    private Task SendPlayerJoinedRoom(Room room, Player player)
        => sessionManager.SendMessageToRoomAsync(room.Id, "player-joined", JsonSerializer.Serialize(player));
}