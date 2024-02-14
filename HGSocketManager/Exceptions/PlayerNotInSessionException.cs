namespace HGSocketManager.Exceptions;

/// <summary>
/// Exception thrown when a player is not in a session.
/// </summary>
/// <param name="playerId"></param>
public class PlayerNotInSessionException(Guid playerId) : Exception($"Player with id {playerId} is not in a session.")
{ }

