namespace BosBelme.Controllers;

[Authorize]
public class GamesController(IRoomService roomService) : Controller
{
    // GET: Метод отображения игры
    [Route("Games/Bounce/{roomId}")]
    public async Task<IActionResult> Bounce(string roomId)
    {
        if (string.IsNullOrEmpty(roomId))
        {
            return RedirectToAction("Index", "Hub");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return RedirectToAction("Index", "Hub");
        }

        var playerRoomCode = await roomService.RoomCode(int.Parse(userId));
        if (playerRoomCode != roomId)
        {
            return RedirectToAction("Index", "Hub");
        }

        var roomDetails = await roomService.GetRoomDetailsAsync(roomId);
        if (roomDetails == null)
        {
            return RedirectToAction("Index", "Hub");
        }

        ViewData["RoomCode"] = roomId;
        ViewData["RoomName"] = roomDetails.RoomName;

        return View();
    }
}
