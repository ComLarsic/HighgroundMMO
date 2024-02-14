namespace HighgroundRooms;

/// <summary>
/// An exception thrown when a player is not in a room.
/// </summary>
/// <param name="playerId"></param>
/// <param name="roomId"></param>
public class PlayerNotInRoomException(Guid playerId, Guid roomId) : Exception($"Player with id {playerId} is not in room with id {roomId}.")
{
    public Guid PlayerId { get; } = playerId;
    public Guid RoomId { get; } = roomId;
}

