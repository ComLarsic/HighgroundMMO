using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HGPlayers;
using HighgroundRooms;
using HGSocketManager;
using HGSocketManager.Exceptions;
using HighgroundRooms.Exceptions;
using HGWorld;

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

    [JsonPropertyName("playerList")]
    public List<Player> PlayerList { get; set; }
}

/// <summary>
/// The handler for the join message.
/// </summary>
public class JoinHandler(
    ISessionManager sessionManager,
    IRoomManager roomManager,
    IPlayerManager playerManager,
    IWorldManager worldManager
) : IMessageHandler
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

            // The player must be added to the world first in order to avoid mmodifying the room before sending a message.
            // This is because SendMessageToRoomAsync will iterate over the players in the room, which will include the player that just joined.
            // This will cause it to throw a "Collection was modified" exception.
            // This is a workaround for that.
            var room = await roomManager.GetRoomAsync(content.Room) ?? throw new RoomNotFoundException(content.Room);
            await SendPlayerJoinedRoom(room, player);

            // Add the player to the world.
            var world = worldManager.GetWorld(room.Id) ?? throw new RoomNotFoundException(room.Id);
            world.SpawnPlayer(player.Id);

            // Send the world updates to the player
            var update = world.GetWorldAsUpdate();
            await sessionManager.SendMessageAsync(session.Id, "world-update", JsonSerializer.Serialize(update));

            // Send the join response.
            await roomManager.AddPlayerToRoomAsync(content.Room, player.Id);
            session.RoomId = content.Room;
            session.PlayerId = player.Id;

            // Get the player list
            var playerIdList = await roomManager.GetPlayersInRoomAsync(content.Room) ?? throw new RoomNotFoundException(content.Room);
            var playerList = new List<Player>();
            foreach (var playerId in playerIdList)
            {
                var p = await playerManager.GetPlayerByIdAsync(playerId) ?? throw new PlayerNotFoundException(playerId);
                playerList.Add(p);
            }
            await SendJoinResponse(session, content.Username != null, player, playerList);
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
    private Task SendJoinResponse(Session session, bool success, Player? player = null, List<Player>? playerList = null)
        => sessionManager.SendMessageAsync(session.Id, "join-response", JsonSerializer.Serialize(new JoinResponse
        {
            Success = success,
            Player = player,
            Room = session.RoomId ?? throw new SessionNotInRoomException(session.Id),
            PlayerList = playerList ?? throw new RoomNotFoundException(session.RoomId.Value)
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