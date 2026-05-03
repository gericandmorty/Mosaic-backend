namespace backend.Modules.Users.DTOs;

public class UpdateProfileDto
{
    public string? DisplayName { get; set; }
}

public class UpdatePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
