using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using backend.Models;
using backend.Modules.Auth.DTOs;
using backend.Modules.Auth.Interfaces;

namespace backend.Modules.Auth.Services;

public class AuthService : IAuthService
{
    private readonly FirebaseAuth _firebaseAuth;
    private readonly FirestoreDb _firestore;

    public AuthService(Infrastructure.Firebase.FirebaseService firebaseService)
    {
        _firebaseAuth = firebaseService.GetAuth();
        _firestore = firebaseService.GetFirestore();
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // 1. Create user in Firebase Auth
        var userArgs = new UserRecordArgs
        {
            Email = request.Email,
            Password = request.Password,
            DisplayName = request.DisplayName,
        };

        var userRecord = await _firebaseAuth.CreateUserAsync(userArgs);

        // 2. Create user in Firestore
        var userDoc = _firestore.Collection("users").Document(userRecord.Uid);
        var userData = new Dictionary<string, object>
        {
            { "email", request.Email },
            { "displayName", request.DisplayName ?? "" },
            { "firebaseUid", userRecord.Uid },
            { "createdAt", Timestamp.FromDateTime(DateTime.UtcNow) }
        };

        await userDoc.SetAsync(userData);

        // 3. Generate a real Firebase Custom Token (JWT)
        var token = await _firebaseAuth.CreateCustomTokenAsync(userRecord.Uid);

        return new AuthResponse(token, request.Email, request.DisplayName, userRecord.Uid);
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

        if (!snapshot.Exists)
        {
            // Auto-sync: Create Firestore doc if it doesn't exist but Auth does
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
        }

        // 3. Generate a real Firebase Custom Token (JWT)
        var token = await _firebaseAuth.CreateCustomTokenAsync(firebaseUser.Uid);

        return new AuthResponse(token, firebaseUser.Email, displayName, firebaseUser.Uid);
    }

}
