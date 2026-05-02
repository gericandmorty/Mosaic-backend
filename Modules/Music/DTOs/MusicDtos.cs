namespace backend.Modules.Music.DTOs
{
    public class TrackResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
