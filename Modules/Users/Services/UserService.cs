using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using backend.Infrastructure.Firebase;
using backend.Infrastructure.Cloudinary;
using backend.Modules.Users.DTOs;

namespace backend.Modules.Users.Services;

public class UserService
{
    private readonly FirebaseAuth _firebaseAuth;
    private readonly FirestoreDb _firestore;
    private readonly CloudinaryService _cloudinaryService;

    public UserService(FirebaseService firebaseService, CloudinaryService cloudinaryService)
    {
        _firebaseAuth = firebaseService.GetAuth();
        _firestore = firebaseService.GetFirestore();
        _cloudinaryService = cloudinaryService;
    }

    public async Task UpdateDisplayNameAsync(string userId, string displayName)
    {
        // Update Firebase Auth
        await _firebaseAuth.UpdateUserAsync(new UserRecordArgs
        {
            Uid = userId,
            DisplayName = displayName
        });

        // Update Firestore
        var userDoc = _firestore.Collection("users").Document(userId);
        await userDoc.UpdateAsync("displayName", displayName);
    }

    public async Task UpdatePasswordAsync(string userId, string newPassword)
    {
        // In a real app, you might want to re-authenticate the user first.
        // But for this request, we'll directly update the password via Firebase Admin.
        await _firebaseAuth.UpdateUserAsync(new UserRecordArgs
        {
            Uid = userId,
            Password = newPassword
        });
    }

    public async Task<string> UpdateProfilePictureAsync(string userId, Stream fileStream, string fileName)
    {
        var userDoc = _firestore.Collection("users").Document(userId);
        var snapshot = await userDoc.GetSnapshotAsync();
        
        string? oldPublicId = null;
        if (snapshot.Exists && snapshot.ContainsField("photoPublicId"))
        {
            oldPublicId = snapshot.GetValue<string>("photoPublicId");
        }

        // Upload to Cloudinary (handles deleting old one if oldPublicId is provided)
        var (url, publicId) = await _cloudinaryService.UploadProfilePictureAsync(fileStream, fileName, oldPublicId);

        // Update Firestore
        await userDoc.UpdateAsync(new Dictionary<string, object>
        {
            { "photoUrl", url },
            { "photoPublicId", publicId }
        });

        // Update Firebase Auth too for consistency
        await _firebaseAuth.UpdateUserAsync(new UserRecordArgs
        {
            Uid = userId,
            PhotoUrl = url
        });

        return url;
    }
}
