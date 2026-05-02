using backend.Models;
using backend.Modules.History.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Modules.History.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HistoryController : ControllerBase
{
    private readonly IHistoryService _historyService;

    public HistoryController(IHistoryService historyService)
    {
        _historyService = historyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetHistory([FromHeader(Name = "X-Firebase-Uid")] string firebaseUid, [FromQuery] int limit = 10)
    {
        if (string.IsNullOrEmpty(firebaseUid)) return Unauthorized("Firebase UID header is missing.");
        
        var history = await _historyService.GetRecentHistoryAsync(firebaseUid, limit);
        return Ok(history);
    }

    [HttpPost]
    public async Task<IActionResult> AddToHistory([FromHeader(Name = "X-Firebase-Uid")] string firebaseUid, [FromBody] HistoryTrack track)
    {
        if (string.IsNullOrEmpty(firebaseUid)) return Unauthorized("Firebase UID header is missing.");
        
        await _historyService.AddToHistoryAsync(firebaseUid, track);
        return Ok(new { success = true });
    }

    [HttpDelete]
    public async Task<IActionResult> ClearHistory([FromHeader(Name = "X-Firebase-Uid")] string firebaseUid)
    {
        if (string.IsNullOrEmpty(firebaseUid)) return Unauthorized("Firebase UID header is missing.");
        
        await _historyService.ClearHistoryAsync(firebaseUid);
        return Ok(new { success = true });
    }
}
