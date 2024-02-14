using HighgroundRooms;
using Microsoft.AspNetCore.Mvc;

namespace HighgroundServer;

/// <summary>
/// A controller for rooms.
/// </summary>
[ApiController]
[Route("api/rooms")]
public class RoomController : Controller
{
    /// <summary>
    /// Get all the rooms.
    /// </summary>
    /// <param name="roomManager"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<List<Room>>> GetRooms([FromServices] IRoomManager roomManager)
        => (await roomManager.GetRoomsAsync()).ToList();

    /// <summary>
    /// Create a room.
    /// </summary>
    /// <param name="roomManager"></param>
    /// <param name="name"></param>
    /// <param name="capacity"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<Room>> CreateRoom([FromServices] IRoomManager roomManager, string name, int capacity)
        => await roomManager.CreateRoomAsync(name, capacity);
}
