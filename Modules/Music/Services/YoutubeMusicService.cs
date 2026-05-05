using backend.Modules.Music.DTOs;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Search;
using YoutubeExplode.Videos.Streams;

namespace backend.Modules.Music.Services
{
    public class YoutubeMusicService : IMusicService
    {
        private readonly YoutubeClient _youtube;

        public YoutubeMusicService()
        {
            var handler = new HttpClientHandler { UseCookies = true };
            var httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36");
            
            // Add YouTube Cookie to bypass bot detection
            var cookie = Environment.GetEnvironmentVariable("YOUTUBE_COOKIE");
            if (!string.IsNullOrEmpty(cookie))
            {
                httpClient.DefaultRequestHeaders.Add("Cookie", cookie);
            }

            _youtube = new YoutubeClient(httpClient);
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
                var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(videoId);
                
                // Prioritize mp4 (AAC/M4A) streams for iOS compatibility
                var m4aStreams = streamManifest.GetAudioOnlyStreams().Where(s => s.Container.Name == "mp4" || s.Container.Name == "m4a");
                var audioStreamInfo = m4aStreams.Any() 
                    ? m4aStreams.GetWithHighestBitrate() 
                    : streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                
                return audioStreamInfo?.Url;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"YoutubeExplode Error: {ex.Message}");
                return null;
            }
        }
    }
}
