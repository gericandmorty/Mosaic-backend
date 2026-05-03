using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using backend.Modules.Users.Services;
using backend.Modules.Users.DTOs;
using System.Security.Claims;

namespace backend.Modules.Users.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    private string? GetCurrentUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    [HttpPut("display-name")]
    public async Task<IActionResult> UpdateDisplayName([FromBody] UpdateProfileDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(dto.DisplayName))
            return BadRequest(new { message = "Display name cannot be empty." });

        try
        {
            await _userService.UpdateDisplayNameAsync(userId, dto.DisplayName);
            return Ok(new { message = "Display name updated successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("password")]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
            return BadRequest(new { message = "Password must be at least 6 characters." });

        try
        {
            // Note: In a production app, you should verify the current password here.
            // Firebase Admin SDK doesn't verify passwords, so you'd need to use Firebase Client SDK 
            // or another method to verify the current credentials if security is a high priority.
            await _userService.UpdatePasswordAsync(userId, dto.NewPassword);
            return Ok(new { message = "Password updated successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("profile-picture")]
    public async Task<IActionResult> UpdateProfilePicture([FromForm] IFormFile file)
    {
        Console.WriteLine($"[API] Profile Picture Upload Request Received. File: {file?.FileName}, Size: {file?.Length}");
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        try
        {
            using var stream = file.OpenReadStream();
            var photoUrl = await _userService.UpdateProfilePictureAsync(userId, stream, file.FileName);
            return Ok(new { photoUrl, message = "Profile picture updated successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
