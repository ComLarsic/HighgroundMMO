namespace HGRooms.Exceptions;

/// <summary>
/// An exception thrown when a room is full.
/// </summary>
/// <param name="roomId"></param>
/// <param name="Capacity"></param>
public class RoomFullException(Guid roomId, int Capacity) : Exception($"Room with id {roomId} is full. Capacity: {Capacity}.")
{
    /// <summary>
    /// The id of the room.
    /// </summary>
    public Guid RoomId { get; } = roomId;

    /// <summary>
    /// The capacity of the room.
    /// </summary>
    public int Capacity { get; } = Capacity;
}
