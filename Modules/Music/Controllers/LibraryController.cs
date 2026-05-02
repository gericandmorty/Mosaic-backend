using backend.Models;
using backend.Modules.Music.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Modules.Music.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LibraryController : ControllerBase
{
    private readonly ILibraryService _libraryService;

    public LibraryController(ILibraryService libraryService)
    {
        _libraryService = libraryService;
    }

    [HttpGet("liked")]
    public async Task<IActionResult> GetLikedSongs([FromHeader(Name = "X-Firebase-Uid")] string firebaseUid)
    {
        if (string.IsNullOrEmpty(firebaseUid)) return Unauthorized("Firebase UID header is missing.");
        
        var songs = await _libraryService.GetLikedSongsAsync(firebaseUid);
        return Ok(songs);
    }

    [HttpPost("liked/toggle")]
    public async Task<IActionResult> ToggleLikedSong([FromHeader(Name = "X-Firebase-Uid")] string firebaseUid, [FromBody] LikedSong song)
    {
        if (string.IsNullOrEmpty(firebaseUid)) return Unauthorized("Firebase UID header is missing.");
        
        await _libraryService.ToggleLikedSongAsync(firebaseUid, song);
        return Ok(new { success = true });
    }

    [HttpGet("liked/check/{youtubeId}")]
    public async Task<IActionResult> CheckIsLiked([FromHeader(Name = "X-Firebase-Uid")] string firebaseUid, string youtubeId)
    {
        if (string.IsNullOrEmpty(firebaseUid)) return Unauthorized("Firebase UID header is missing.");
        
        var isLiked = await _libraryService.IsSongLikedAsync(firebaseUid, youtubeId);
        return Ok(new { isLiked });
    }

    [HttpGet("playlists")]
    public async Task<IActionResult> GetPlaylists([FromHeader(Name = "X-Firebase-Uid")] string firebaseUid)
    {
        if (string.IsNullOrEmpty(firebaseUid)) return Unauthorized("Firebase UID header is missing.");
        
        var playlists = await _libraryService.GetPlaylistsAsync(firebaseUid);
        return Ok(playlists);
    }

    [HttpPost("playlists")]
    public async Task<IActionResult> CreatePlaylist([FromHeader(Name = "X-Firebase-Uid")] string firebaseUid, [FromBody] Playlist request)
    {
        if (string.IsNullOrEmpty(firebaseUid)) return Unauthorized("Firebase UID header is missing.");
        
        var playlist = await _libraryService.CreatePlaylistAsync(firebaseUid, request.Name, request.Description, request.CoverUrl);
        return Ok(playlist);
    }

    [HttpDelete("playlists/{playlistId}")]
    public async Task<IActionResult> DeletePlaylist([FromHeader(Name = "X-Firebase-Uid")] string firebaseUid, string playlistId)
    {
        if (string.IsNullOrEmpty(firebaseUid)) return Unauthorized("Firebase UID header is missing.");
        
        await _libraryService.DeletePlaylistAsync(firebaseUid, playlistId);
        return Ok(new { success = true });
    }

    [HttpGet("playlists/{playlistId}/tracks")]
    public async Task<IActionResult> GetPlaylistTracks([FromHeader(Name = "X-Firebase-Uid")] string firebaseUid, string playlistId)
    {
        if (string.IsNullOrEmpty(firebaseUid)) return Unauthorized("Firebase UID header is missing.");
        
        var tracks = await _libraryService.GetPlaylistTracksAsync(firebaseUid, playlistId);
        return Ok(tracks);
    }

    [HttpPost("playlists/{playlistId}/tracks")]
    public async Task<IActionResult> AddTrackToPlaylist([FromHeader(Name = "X-Firebase-Uid")] string firebaseUid, string playlistId, [FromBody] PlaylistTrack track)
    {
        if (string.IsNullOrEmpty(firebaseUid)) return Unauthorized("Firebase UID header is missing.");
        
        await _libraryService.AddTrackToPlaylistAsync(firebaseUid, playlistId, track);
        return Ok(new { success = true });
    }

    [HttpDelete("playlists/{playlistId}/tracks/{youtubeId}")]
    public async Task<IActionResult> RemoveTrackFromPlaylist([FromHeader(Name = "X-Firebase-Uid")] string firebaseUid, string playlistId, string youtubeId)
    {
        if (string.IsNullOrEmpty(firebaseUid)) return Unauthorized("Firebase UID header is missing.");
        
        await _libraryService.RemoveTrackFromPlaylistAsync(firebaseUid, playlistId, youtubeId);
        return Ok(new { success = true });
    }
}
