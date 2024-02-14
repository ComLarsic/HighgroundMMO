namespace HGPlayers;

/// <summary>
/// The player manager.
/// </summary>
public interface IPlayerManager
{
    /// <summary>
    /// Create a player.
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public Task<Player> CreatePlayerAsync(string username, Guid sessionId);

    /// <summary>
    /// Get a player by id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<Player?> GetPlayerByIdAsync(Guid id);

    /// <summary>
    /// Get all the players.
    /// </summary>
    /// <returns></returns>
    public Task<List<Player>> GetPlayersAsync();
}
