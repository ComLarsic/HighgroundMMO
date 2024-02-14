namespace HGSocketManager.Exceptions;

/// <summary>
/// Exception thrown when a session is not found.
/// </summary>
/// <param name="sessionId"></param>
public class SessionNotFoundException(Guid sessionId) : Exception($"Session with id {sessionId} not found.")
{ }
