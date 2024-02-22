namespace HGWorld;

/// <summary>
/// The manager for the game world.
/// </summary>
public class WorldManager : IWorldManager
{
    /// <inheritdoc/>
    public Dictionary<Guid, World> Worlds { get; } = [];

    /// </inheritdoc>
    public World? GetWorld(Guid roomId)
        => Worlds[roomId];


    /// </inheritdoc>
    public bool HasWorld(Guid roomId)
        => Worlds.ContainsKey(roomId);
}
