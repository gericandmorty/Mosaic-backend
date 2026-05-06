using backend.Modules.Music.DTOs;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Search;
using YoutubeExplode.Videos.Streams;

namespace backend.Modules.Music.Services
{
    public class YoutubeMusicService : IMusicService
    {
        private static readonly HttpClient _httpClient = CreateHttpClient();
        private readonly YoutubeClient _youtube;

        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler { UseCookies = false };
            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36");
            
            // Support for PO Tokens (Option 2)
            var poToken = Environment.GetEnvironmentVariable("YOUTUBE_PO_TOKEN");
            var visitorData = Environment.GetEnvironmentVariable("YOUTUBE_VISITOR_DATA");
            if (!string.IsNullOrEmpty(poToken)) client.DefaultRequestHeaders.Add("X-YouTube-PO-Token", poToken);
            if (!string.IsNullOrEmpty(visitorData)) client.DefaultRequestHeaders.Add("X-YouTube-Visitor-Data", visitorData);

            return client;
        }

        private static IReadOnlyList<System.Net.Cookie> ParseCookies(string cookieString)
        {
            var cookies = new List<System.Net.Cookie>();
            if (string.IsNullOrEmpty(cookieString)) return cookies;

            var pairs = cookieString.Split(';');
            foreach (var pair in pairs)
            {
                var parts = pair.Split('=');
                if (parts.Length >= 2)
                {
                    var name = parts[0].Trim();
                    var value = string.Join("=", parts.Skip(1)).Trim();
                    try {
                        cookies.Add(new System.Net.Cookie(name, value, "/", ".youtube.com"));
                    } catch { /* Skip invalid cookies */ }
                }
            }
            return cookies;
        }

        public YoutubeMusicService()
        {
            var cookieString = Environment.GetEnvironmentVariable("YOUTUBE_COOKIE") ?? "";
            var cookies = ParseCookies(cookieString);
            
            // Correct initialization for YoutubeExplode 6.6.0
            _youtube = new YoutubeClient(_httpClient, cookies);
        }

        public async Task<IEnumerable<TrackResponse>> SearchTracksAsync(string query, int limit = 20)
        {
            var results = new List<TrackResponse>();
            
            try
            {
                await foreach (var batch in _youtube.Search.GetResultBatchesAsync(query, SearchFilter.Video))
                {
                    foreach (var video in batch.Items)
                    {
                        if (video is VideoSearchResult videoResult)
                        {
                            results.Add(new TrackResponse
                            {
                                Id = videoResult.Id.Value,
                                Title = videoResult.Title,
                                Artist = videoResult.Author.ChannelTitle,
                                ThumbnailUrl = videoResult.Thumbnails.GetWithHighestResolution().Url,
                                Duration = videoResult.Duration?.ToString() ?? "00:00",
                                Url = videoResult.Url
                            });
                        }

                        if (results.Count >= limit) break;
                    }
                    
                    if (results.Count >= limit) break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Search Error: {ex.Message}");
            }

            return results;
        }

        public async Task<TrackResponse?> GetTrackDetailsAsync(string videoId)
        {
            try
            {
                var video = await _youtube.Videos.GetAsync(videoId);
                return new TrackResponse
                {
                    Id = video.Id.Value,
                    Title = video.Title,
                    Artist = video.Author.ChannelTitle,
                    ThumbnailUrl = video.Thumbnails.GetWithHighestResolution().Url,
                    Duration = video.Duration?.ToString() ?? "00:00",
                    Url = video.Url
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<string?> GetAudioStreamUrlAsync(string videoId)
        {
            try
            {
                // Try YoutubeExplode first (Highest quality)
                var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(videoId);
                
                // Prioritize mp4 (AAC/M4A) streams for iOS compatibility
                var m4aStreams = streamManifest.GetAudioOnlyStreams().Where(s => s.Container.Name == "mp4" || s.Container.Name == "m4a");
                var audioStreamInfo = m4aStreams.Any() 
                    ? m4aStreams.GetWithHighestBitrate() 
                    : streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                
                if (audioStreamInfo != null) return audioStreamInfo.Url;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"YoutubeExplode failed for {videoId}, trying Piped fallback... Error: {ex.Message}");
            }

            // FALLBACK 1: Piped Instance A
            var streamUrl = await GetPipedStreamUrlAsync(videoId, "https://pipedapi.kavin.rocks");
            if (streamUrl != null) return streamUrl;

            // FALLBACK 2: Piped Instance B
            Console.WriteLine($"Piped A failed for {videoId}, trying Piped B fallback...");
            return await GetPipedStreamUrlAsync(videoId, "https://piped-api.lunar.icu");
        }

        private async Task<string?> GetPipedStreamUrlAsync(string videoId, string pipedInstance)
        {
            try
            {
                string url = $"{pipedInstance}/streams/{videoId}";
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode) {
                    Console.WriteLine($"Piped ({pipedInstance}) responded with: {response.StatusCode}");
                    return null;
                }

                var data = await response.Content.ReadFromJsonAsync<PipedResponse>();
                var bestAudio = data?.AudioStreams?.OrderByDescending(s => s.Bitrate).FirstOrDefault();

                if (bestAudio != null) {
                    Console.WriteLine($"[MusicProxy] Piped fallback SUCCESS ({pipedInstance}) for {videoId}");
                }

                return bestAudio?.Url;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Piped Fallback Error ({pipedInstance}): {ex.Message}");
                return null;
            }
        }
    }

    // DTOs for Piped Response
    public class PipedResponse
    {
        public List<PipedAudioStream>? AudioStreams { get; set; }
    }

    public class PipedAudioStream
    {
        public string? Url { get; set; }
        public int Bitrate { get; set; }
        public string? Format { get; set; }
    }
}
