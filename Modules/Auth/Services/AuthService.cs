using FirebaseAdmin.Auth;
using backend.Data;
using backend.Models;
using backend.Modules.Auth.DTOs;
using backend.Modules.Auth.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Modules.Auth.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly FirebaseAuth _firebaseAuth;

    public AuthService(AppDbContext context, Infrastructure.Firebase.FirebaseService firebaseService)
    {
        _context = context;
        _firebaseAuth = firebaseService.GetAuth();
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // 1. Create user in Firebase
        var userArgs = new UserRecordArgs
        {
            Email = request.Email,
            Password = request.Password,
            DisplayName = request.DisplayName,
        };

        var userRecord = await _firebaseAuth.CreateUserAsync(userArgs);

        // 2. Create user in our local DB
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirebaseUid = userRecord.Uid,
            Email = request.Email,
            DisplayName = request.DisplayName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // 3. Generate a real Firebase Custom Token (JWT)
        var token = await _firebaseAuth.CreateCustomTokenAsync(user.FirebaseUid);

        return new AuthResponse(token, user.Email, user.DisplayName, user.FirebaseUid);
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

        // 2. Sync with local DB
        var user = await _context.Users.FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUser.Uid);
        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                FirebaseUid = firebaseUser.Uid,
                Email = firebaseUser.Email,
                DisplayName = firebaseUser.DisplayName ?? firebaseUser.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        // 3. Generate a real Firebase Custom Token (JWT)
        var token = await _firebaseAuth.CreateCustomTokenAsync(user.FirebaseUid);

        return new AuthResponse(token, user.Email, user.DisplayName, user.FirebaseUid);
    }
}
