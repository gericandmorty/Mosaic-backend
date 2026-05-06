using backend.Modules.Music.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Modules.Music.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MusicController : ControllerBase
    {
        private readonly IMusicService _musicService;
        private static readonly HttpClient _httpClient = new HttpClient(new HttpClientHandler 
        { 
            UseCookies = false,
            CheckCertificateRevocationList = false // Optional: slight speed up
        });

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
                return NotFound("Could not extract stream URL.");
            }

            return Ok(new { url = url });
        }

        [HttpGet("play/{id}")]
        public async Task ProxyStream(string id)
        {
            Console.WriteLine($"[MusicProxy] Received request for ID: {id}");
            var url = await _musicService.GetAudioStreamUrlAsync(id);
            if (string.IsNullOrEmpty(url)) {
                Response.StatusCode = 404;
                return;
            }

            try {
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                
                // Forward Range header from the mobile device to YouTube
                if (Request.Headers.ContainsKey("Range")) {
                    request.Headers.TryAddWithoutValidation("Range", Request.Headers["Range"].ToString());
                }

                // Add YouTube Cookie to bypass bot detection
                var cookie = Environment.GetEnvironmentVariable("YOUTUBE_COOKIE");
                if (!string.IsNullOrEmpty(cookie))
                {
                    request.Headers.TryAddWithoutValidation("Cookie", cookie);
                }

                // Set a realistic User-Agent
                request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36");

                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                
                // Copy Status Code (important for 206 Partial Content)
                Response.StatusCode = (int)response.StatusCode;
                
                // Forward all content headers
                foreach (var header in response.Content.Headers) {
                    Response.Headers[header.Key] = header.Value.ToArray();
                }

                // Forward essential response headers
                if (response.Headers.Contains("Accept-Ranges")) {
                    Response.Headers["Accept-Ranges"] = "bytes";
                }

                // Ensure Content-Type is set if missing
                if (!Response.Headers.ContainsKey("Content-Type")) {
                    Response.ContentType = "audio/mpeg";
                }

                await response.Content.CopyToAsync(Response.Body);
            } catch (Exception ex) {
                Console.WriteLine($"Streaming error for {id}: {ex.Message}");
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
