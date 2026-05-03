using backend.Modules.Auth.DTOs;

namespace backend.Modules.Auth.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task RequestPasswordResetAsync(string email);
    Task ResetPasswordAsync(string code, string newPassword);
    Task VerifyEmailAsync(string code);
    Task ResendVerificationCodeAsync(string email);
}
