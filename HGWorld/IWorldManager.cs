namespace HGWorld;

/// <summary>
/// The manager for the game world.
/// </summary>
public interface IWorldManager
{
    /// <summary>
    /// The worlds associated with the rooms.
    /// </summary>
    public Dictionary<Guid, World> Worlds { get; }

    /// <summary>
    /// Get the world associated with a room.
    /// </summary>
    /// <param name="roomId">The id of the room.</param>
    /// <returns>The world associated with the room.</returns>
    public World? GetWorld(Guid roomId);

    /// <summary>
    /// Check if a world exists for a room.
    /// </summary>
    /// <param name="roomId">The id of the room.</param>
    /// <returns>True if a world exists for the room, false otherwise.</returns>
    public bool HasWorld(Guid roomId);
}
