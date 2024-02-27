namespace HGRooms.Exceptions;

/// <summary>
/// An exception thrown when a player is not found.
/// </summary>
/// <param name="playerId"></param>
/// <param name="roomId"></param>
public class PlayerNotFoundException(Guid playerId) : Exception($"Player with id {playerId} not found.")
{
    /// <summary>
    /// The id of the player.
    /// </summary>
    public Guid PlayerId { get; } = playerId;
}
