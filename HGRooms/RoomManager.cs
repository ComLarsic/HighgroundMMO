
using HighgroundRooms.Exceptions;

namespace HighgroundRooms;

/// <summary>
/// A manager for rooms in the game.
/// </summary>
public class RoomManager : IRoomManager
{
    /// <summary>
    /// The rooms in the game.
    /// </summary>
    public List<Room> Rooms { get; } = [];

    /// </inheritdoc>
    public Task AddPlayerToRoomAsync(Guid roomId, Guid playerId)
    {
        var room = Rooms.First(r => r.Id == roomId) ?? throw new RoomNotFoundException(roomId);

        if (room.Players.Count >= room.Capacity)
            throw new RoomFullException(roomId, room.Capacity);
        if (room.Players.Contains(playerId))
            throw new PlayerAlreadyInRoomException(roomId, playerId);

        room.Players.Add(playerId);
        return Task.CompletedTask;
    }

    /// </inheritdoc>
    public Task<bool> AreAllRoomsEmptyAsync()
        => Task.FromResult(Rooms.All(r => r.Players.Count == 0));

    /// </inheritdoc>
    public Task<Room> CreateRoomAsync(string name, int capacity)
    {
        var room = new Room
        {
            Name = name,
            Capacity = capacity
        };
        Rooms.Add(room);
        return Task.FromResult(room);
    }

    /// </inheritdoc>
    public Task DeleteAllRoomsAsync()
    {
        Rooms.Clear();
        return Task.CompletedTask;
    }

    /// </inheritdoc>
    public Task DeleteRoomAsync(Guid id)
    {
        var room = Rooms.First(r => r.Id == id) ?? throw new RoomNotFoundException(id);
        Rooms.Remove(room);
        return Task.CompletedTask;
    }

    /// </inheritdoc>
    public Task<List<Guid>> GetPlayersInRoomAsync(Guid roomId)
    {
        var room = Rooms.First(r => r.Id == roomId) ?? throw new RoomNotFoundException(roomId);
        return Task.FromResult(room.Players);
    }

    /// </inheritdoc>
    public Task<Room> GetRoomAsync(Guid id)
    {
        var room = Rooms.First(r => r.Id == id) ?? throw new RoomNotFoundException(id);
        return Task.FromResult(room);
    }

    /// </inheritdoc>
    public Task<Room?> GetRoomByPlayerAsync(Guid playerId)
    {
        var room = Rooms.FirstOrDefault(r => r.Players.Contains(playerId));
        return Task.FromResult(room);
    }

    /// </inheritdoc>
    public Task<List<Room>> GetRoomsAsync()
        => Task.FromResult(Rooms);

    /// </inheritdoc>
    public Task RemoveAllPlayersFromAllRoomsAsync()
    {
        foreach (var room in Rooms)
            room.Players.Clear();
        return Task.CompletedTask;
    }

    /// </inheritdoc>
    public Task RemoveAllPlayersFromRoomAsync(Guid roomId)
    {
        var room = Rooms.First(r => r.Id == roomId) ?? throw new RoomNotFoundException(roomId);
        room.Players.Clear();
        return Task.CompletedTask;
    }

    /// </inheritdoc>
    public Task RemovePlayerFromRoomAsync(Guid roomId, Guid playerId)
    {
        var room = Rooms.First(r => r.Id == roomId) ?? throw new RoomNotFoundException(roomId);
        if (!room.Players.Contains(playerId))
            throw new PlayerNotInRoomException(playerId, roomId);

        room.Players.Remove(playerId);
        return Task.CompletedTask;
    }
}
