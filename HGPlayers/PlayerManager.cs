namespace HGPlayers;

/// <summary>
/// The player manager.
/// </summary>
public class PlayerManager : IPlayerManager
{
    /// <inheritdoc />
    public List<Player> Players { get; } = [];

    /// <inheritdoc />
    public Task<Player> CreatePlayerAsync(string username, Guid sessionId)
    {
        var player = new Player
        {
            Id = Guid.NewGuid(),
            Username = username
        };
        Players.Add(player);
        return Task.FromResult(player);
    }

    /// <inheritdoc />
    public Task<Player?> GetPlayerByIdAsync(Guid id)
    {
        try
        {
            Player? player = Players.FirstOrDefault(p => p.Id == id);
            return Task.FromResult(player);
        }
        catch
        {
            return Task.FromResult<Player?>(null);
        }
    }

    public Task<List<Player>> GetPlayersAsync()
        => Task.FromResult(Players);


}
