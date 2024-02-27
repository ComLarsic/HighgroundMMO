namespace HGRooms.Exceptions;

/// <summary>
/// An exception thrown when a player is already in a room.
/// </summary>
/// <param name="roomId"></param>
/// <param name="playerId"></param>
public class PlayerAlreadyInRoomException(Guid roomId, Guid playerId) : Exception($"Player with id {playerId} is already in room with id {roomId}.")
{
    /// <summary>
    /// The id of the room.
    /// </summary>
    public Guid RoomId { get; } = roomId;

    /// <summary>
    /// The id of the player.
    /// </summary>
    public Guid PlayerId { get; } = playerId;
}
