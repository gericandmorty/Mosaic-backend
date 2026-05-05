using backend.Modules.Music.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Modules.Music.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MusicController : ControllerBase
    {
        private readonly IMusicService _musicService;
        private static readonly HttpClient _httpClient = new HttpClient();

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
        public IActionResult GetStream(string id)
        {
            // Detect if we are behind a proxy (like Render) and use HTTPS if so
            var scheme = Request.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? Request.Scheme;
            
            // If we're on a production-like host and it's still http, force https for iOS compatibility
            if (!Request.Host.Host.Contains("localhost") && !Request.Host.Host.Contains("192.168") && scheme == "http")
            {
                scheme = "https";
            }

            var proxyUrl = $"{scheme}://{Request.Host}/api/music/play/{id}";
            return Ok(new { url = proxyUrl });
        }

        [HttpGet("play/{id}")]
        public async Task ProxyStream(string id)
        {
            var url = await _musicService.GetAudioStreamUrlAsync(id);
            if (string.IsNullOrEmpty(url)) {
                Response.StatusCode = 404;
                return;
            }

            try {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                
                if (Request.Headers.ContainsKey("Range")) {
                    request.Headers.Add("Range", Request.Headers["Range"].ToString());
                }

                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                
                Response.StatusCode = (int)response.StatusCode;
                foreach (var header in response.Content.Headers) {
                    Response.Headers[header.Key] = header.Value.ToArray();
                }
                
                if (!Response.Headers.ContainsKey("Content-Type")) {
                    Response.ContentType = "audio/mpeg";
                }

                await response.Content.CopyToAsync(Response.Body);
            } catch (Exception ex) {
                Console.WriteLine($"Streaming error: {ex.Message}");
                if (!Response.HasStarted) Response.StatusCode = 500;
            }
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
