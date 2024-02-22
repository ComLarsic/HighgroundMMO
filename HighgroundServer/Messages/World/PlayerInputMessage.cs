using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using HGSocketManager;
using HGSocketManager.Exceptions;
using HGWorld;
using HighgroundRooms.Exceptions;

namespace HighgroundServer;

/// <summary>
/// A message to represent the input from a player.
/// This message is sent from the client to the server to update the input of a player.
/// </summary>
[Serializable]
public struct PlayerInputMessage
{
    /// <summary>
    /// The horizontal vector of the input.
    /// </summary>
    [JsonPropertyName("x")]
    public float X { get; set; }

    /// <summary>
    /// The vertical vector of the input.
    /// </summary>
    [JsonPropertyName("y")]
    public float Y { get; set; }

    /// <summary>
    /// Whether the player is jumping.
    /// </summary>
    [JsonPropertyName("jumping")]
    public bool Jumping { get; set; }
}

/// <summary>
/// A message to represent the input from a player.
/// </summary>

public class PlayerInputMessageHandler(IWorldManager worldManager) : IMessageHandler
{
    public string MessageType => "player-input";

    public Task Handle(Session session, string? message)
    {
        if (message == null)
            return Task.CompletedTask;

        try
        {
            var inputMessage = JsonSerializer.Deserialize<PlayerInputMessage>(message);
            if (session.PlayerId == null)
                throw new PlayerNotInSessionException(session.Id);
            if (session.RoomId == null)
                throw new SessionNotInRoomException(session.Id);
            var world = worldManager.GetWorld(session.RoomId.Value) ?? throw new RoomNotFoundException(session.RoomId.Value);
            world.SetPlayerInput(session.PlayerId.Value, new Vector2(inputMessage.X, inputMessage.Y), inputMessage.Jumping);
        }
        catch { }
        return Task.CompletedTask;
    }
}
