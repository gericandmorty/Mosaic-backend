using backend.Modules.Music.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Modules.Music.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MusicController : ControllerBase
    {
        private readonly IMusicService _musicService;

        public MusicController(IMusicService musicService)
        {
            _musicService = musicService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int limit = 20)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest("Search query cannot be empty.");
            }

            var results = await _musicService.SearchTracksAsync(q, limit);
            return Ok(results);
        }

        [HttpGet("stream/{id}")]
        public async Task<IActionResult> GetStream(string id)
        {
            var url = await _musicService.GetAudioStreamUrlAsync(id);
            if (string.IsNullOrEmpty(url))
            {
                return NotFound("Stream not found.");
            }

            return Ok(new { url });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetails(string id)
        {
            var result = await _musicService.GetTrackDetailsAsync(id);
            if (result == null)
            {
                return NotFound("Track not found.");
            }

            return Ok(result);
        }
    }
}
