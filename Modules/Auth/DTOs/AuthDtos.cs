using System.Text.Json.Serialization;

namespace backend.Modules.Auth.DTOs;

public class RegisterRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;
}

public class LoginRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}
public record AuthResponse(
    [property: JsonPropertyName("token")] string Token, 
    [property: JsonPropertyName("email")] string Email, 
    [property: JsonPropertyName("displayName")] string? DisplayName, 
    [property: JsonPropertyName("firebaseUid")] string FirebaseUid
);
