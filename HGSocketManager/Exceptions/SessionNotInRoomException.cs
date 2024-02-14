namespace HGSocketManager.Exceptions;

/// <summary>
/// Exception thrown when a session is not in a room.
/// </summary>
public class SessionNotInRoomException(Guid sessionId) : Exception($"Session with id {sessionId} is not in a room.")
{ }
