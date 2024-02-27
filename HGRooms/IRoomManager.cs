namespace HGRooms;

/// <summary>
/// The room manager.
/// </summary>
public interface IRoomManager
{
    /// <summary>
    /// Creates a room.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="capacity"></param>
    /// <returns></returns>
    public Task<Room> CreateRoomAsync(string name, int capacity);

    /// <summary>
    /// Gets a room by Guid.
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="RoomNotFoundException"></exception>
    /// <returns></returns>
    public Task<Room> GetRoomAsync(Guid id);

    /// <summary>
    /// Gets all rooms.
    /// </summary>
    /// <returns></returns>
    public Task<List<Room>> GetRoomsAsync();

    /// <summary>
    /// Adds a player to a room.
    /// </summary>
    /// <param name="roomId"></param>
    /// <param name="playerId"></param>
    /// <returns></returns>
    /// <exception cref="RoomNotFoundException"></exception>
    /// <exception cref="RoomFullException"></exception>
    /// <exception cref="PlayerAlreadyInRoomException"></exception>
    /// <exception cref="PlayerNotFoundException"></exception>
    public Task AddPlayerToRoomAsync(Guid roomId, Guid playerId);

    /// <summary>
    /// Removes a player from a room.
    /// </summary>
    /// <param name="roomId"></param>
    /// <param name="playerId"></param>
    /// <returns></returns>
    /// <exception cref="RoomNotFoundException"></exception>
    /// <exception cref="PlayerNotFoundException"></exception>
    /// <exception cref="PlayerNotInRoomException"></exception>
    public Task RemovePlayerFromRoomAsync(Guid roomId, Guid playerId);

    /// <summary>
    /// Deletes a room.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="RoomNotFoundException"></exception>
    public Task DeleteRoomAsync(Guid id);

    /// <summary>
    /// Deletes all rooms.
    /// </summary>
    /// <returns></returns>
    public Task DeleteAllRoomsAsync();

    /// <summary>
    /// Removes all players from a room.
    /// </summary>
    /// <param name="roomId"></param>
    /// <returns></returns>
    /// <exception cref="RoomNotFoundException"></exception>
    public Task RemoveAllPlayersFromRoomAsync(Guid roomId);

    /// <summary>
    /// Removes all players from all rooms.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="RoomNotFoundException"></exception>
    public Task RemoveAllPlayersFromAllRoomsAsync();

    /// <summary>
    /// Gets all players in a room.
    /// </summary>
    /// <param name="roomId"></param>
    /// <returns></returns>
    /// <exception cref="RoomNotFoundException"></exception>
    public Task<List<Guid>> GetPlayersInRoomAsync(Guid roomId);

    /// <summary>
    /// Gets the room a player is in.
    /// </summary>
    /// <param name="playerId"></param>
    /// <returns></returns>
    /// <exception cref="PlayerNotFoundException"></exception>
    public Task<Room?> GetRoomByPlayerAsync(Guid playerId);

    /// <summary>
    /// Check if all rooms are empty.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="RoomNotFoundException"></exception>
    public Task<bool> AreAllRoomsEmptyAsync();
}
