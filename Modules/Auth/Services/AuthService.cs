using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using backend.Models;
using backend.Modules.Auth.DTOs;
using backend.Modules.Auth.Interfaces;
using System.Text.Json;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace backend.Modules.Auth.Services;

public class AuthService : IAuthService
{
    private readonly FirebaseAuth _firebaseAuth;
    private readonly FirestoreDb _firestore;
    private readonly IEmailService _emailService;
    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;

    public AuthService(Infrastructure.Firebase.FirebaseService firebaseService, IEmailService emailService)
    {
        _firebaseAuth = firebaseService.GetAuth();
        _firestore = firebaseService.GetFirestore();
        _emailService = emailService;
        _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new Exception("JWT_SECRET not configured");
        _jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "MosaicBackend";
        _jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "MosaicApp";
    }

    private string GenerateJwtToken(string userId, string email, string? displayName, string? photoUrl)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim("displayName", displayName ?? ""),
            new Claim("photoUrl", photoUrl ?? ""),
            new Claim("firebase_uid", userId) // Keep for compatibility
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // 1. Check if email already exists
        UserRecord? existingUser = null;
        try 
        {
            existingUser = await _firebaseAuth.GetUserByEmailAsync(request.Email);
        }
        catch (FirebaseAuthException ex) when (ex.AuthErrorCode == AuthErrorCode.UserNotFound)
        {
            // Email available
        }

        if (existingUser != null)
        {
            var existingDoc = await _firestore.Collection("users").Document(existingUser.Uid).GetSnapshotAsync();
            if (existingDoc.Exists && existingDoc.GetValue<bool>("isEmailVerified"))
            {
                throw new Exception("This email is already registered. Try logging in!");
            }
            
            var newCode = new Random().Next(100000, 999999).ToString();
            await existingDoc.Reference.UpdateAsync(new Dictionary<string, object>
            {
                { "verificationCode", newCode }
            });

            await _emailService.SendVerificationEmailAsync(request.Email, newCode);
            return new AuthResponse("", request.Email, existingUser.DisplayName, existingUser.Uid, existingUser.PhotoUrl);
        }

        // 2. Create user in Firebase Auth
        var userArgs = new UserRecordArgs
        {
            Email = request.Email,
            Password = request.Password,
            DisplayName = request.DisplayName,
        };

        var userRecord = await _firebaseAuth.CreateUserAsync(userArgs);
        var verificationCode = new Random().Next(100000, 999999).ToString();

        // 3. Create user in Firestore
        var userDoc = _firestore.Collection("users").Document(userRecord.Uid);
        var userData = new Dictionary<string, object>
        {
            { "email", request.Email },
            { "displayName", request.DisplayName ?? "" },
            { "firebaseUid", userRecord.Uid },
            { "isEmailVerified", false },
            { "verificationCode", verificationCode },
            { "createdAt", Timestamp.FromDateTime(DateTime.UtcNow) }
        };

        await userDoc.SetAsync(userData);
        await _emailService.SendVerificationEmailAsync(request.Email, verificationCode);

        // 4. Generate Local JWT
        var token = GenerateJwtToken(userRecord.Uid, request.Email, request.DisplayName, userRecord.PhotoUrl);

        return new AuthResponse(token, request.Email, request.DisplayName, userRecord.Uid, userRecord.PhotoUrl);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // 1. Check Firebase first
        UserRecord firebaseUser;
        try 
        {
            firebaseUser = await _firebaseAuth.GetUserByEmailAsync(request.Email);
        }
        catch
        {
            throw new Exception("User not found in Firebase.");
        }

        // 2. Fetch from Firestore
        var userDoc = _firestore.Collection("users").Document(firebaseUser.Uid);
        var snapshot = await userDoc.GetSnapshotAsync();

        string displayName = firebaseUser.DisplayName ?? firebaseUser.Email;
        string? photoUrl = firebaseUser.PhotoUrl;

        if (!snapshot.Exists)
        {
            var userData = new Dictionary<string, object>
            {
                { "email", firebaseUser.Email },
                { "displayName", displayName },
                { "firebaseUid", firebaseUser.Uid },
                { "createdAt", Timestamp.FromDateTime(DateTime.UtcNow) }
            };
            await userDoc.SetAsync(userData);
        }
        else 
        {
            displayName = snapshot.GetValue<string>("displayName");
            photoUrl = snapshot.ContainsField("photoUrl") ? snapshot.GetValue<string>("photoUrl") : firebaseUser.PhotoUrl;
        }

        // 3. Generate Local JWT
        var token = GenerateJwtToken(firebaseUser.Uid, firebaseUser.Email, displayName, photoUrl);

        return new AuthResponse(token, firebaseUser.Email, displayName, firebaseUser.Uid, photoUrl);
    }

    public async Task RequestPasswordResetAsync(string email)
    {
        var userRecord = await _firebaseAuth.GetUserByEmailAsync(email);
        if (userRecord == null) return;

        var code = new Random().Next(100000, 999999).ToString();
        var expiresAt = DateTime.UtcNow.AddMinutes(15);

        var userDoc = _firestore.Collection("users").Document(userRecord.Uid);
        await userDoc.UpdateAsync(new Dictionary<string, object>
        {
            { "resetPasswordToken", code },
            { "resetTokenExpiresAt", Timestamp.FromDateTime(expiresAt) }
        });

        await _emailService.SendPasswordResetEmailAsync(email, code);
    }

    public async Task ResetPasswordAsync(string code, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new Exception("Reset code is required.");

        var query = _firestore.Collection("users").WhereEqualTo("resetPasswordToken", code.Trim());
        var snapshot = await query.GetSnapshotAsync();
        
        if (snapshot.Documents.Count == 0)
        {
            throw new Exception("Invalid reset code. Please check your email.");
        }

        var userDoc = snapshot.Documents[0];
        var expiresAt = userDoc.GetValue<Timestamp>("resetTokenExpiresAt").ToDateTime();
        if (expiresAt < DateTime.UtcNow) throw new Exception("This code has expired. Please request a new one.");

        var firebaseUid = userDoc.Id;
        await _firebaseAuth.UpdateUserAsync(new UserRecordArgs
        {
            Uid = firebaseUid,
            Password = newPassword
        });

        await userDoc.Reference.UpdateAsync(new Dictionary<string, object>
        {
            { "resetPasswordToken", FieldValue.Delete },
            { "resetTokenExpiresAt", FieldValue.Delete }
        });
    }

    public async Task VerifyEmailAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new Exception("Verification code is required.");

        var query = _firestore.Collection("users").WhereEqualTo("verificationCode", code.Trim());
        var snapshot = await query.GetSnapshotAsync();
        
        if (snapshot.Documents.Count == 0)
        {
            throw new Exception("Invalid or expired verification code. Please check your email.");
        }

        var userDoc = snapshot.Documents[0];

        await userDoc.Reference.UpdateAsync(new Dictionary<string, object>
        {
            { "isEmailVerified", true },
            { "verificationCode", FieldValue.Delete }
        });
    }

    public async Task ResendVerificationCodeAsync(string email)
    {
        var userRecord = await _firebaseAuth.GetUserByEmailAsync(email);
        if (userRecord == null) throw new Exception("User not found.");

        var userDoc = _firestore.Collection("users").Document(userRecord.Uid);
        var snapshot = await userDoc.GetSnapshotAsync();
        
        if (snapshot.Exists && snapshot.GetValue<bool>("isEmailVerified"))
        {
            throw new Exception("This account is already verified.");
        }

        var newCode = new Random().Next(100000, 999999).ToString();
        await userDoc.UpdateAsync(new Dictionary<string, object>
        {
            { "verificationCode", newCode }
        });

        await _emailService.SendVerificationEmailAsync(email, newCode);
    }
}
