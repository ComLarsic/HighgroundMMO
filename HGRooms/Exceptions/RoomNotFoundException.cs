namespace HGRooms.Exceptions;

/// <summary>
/// An exception thrown when a room is not found.
/// </summary>
/// <param name="roomId"></param>
public class RoomNotFoundException(Guid roomId) : Exception($"Room with id {roomId} not found.")
{
    /// <summary>
    /// The id of the room.
    /// </summary>
    public Guid RoomId { get; } = roomId;
}
