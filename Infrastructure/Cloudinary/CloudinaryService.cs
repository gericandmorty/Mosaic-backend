using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace backend.Infrastructure.Cloudinary;

public class CloudinaryService
{
    private readonly CloudinaryDotNet.Cloudinary _cloudinary;
    private readonly string _folderName;

    public CloudinaryService()
    {
        var cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_ID");
        var apiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API");
        var apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");
        _folderName = Environment.GetEnvironmentVariable("CLOUDINARY_FOLDER_NAME") ?? "mosaic";

        if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
        {
            throw new Exception("Cloudinary environment variables are not properly set.");
        }

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new CloudinaryDotNet.Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<(string url, string publicId)> UploadProfilePictureAsync(Stream fileStream, string fileName, string? oldPublicId = null)
    {
        // Delete old image if it exists
        if (!string.IsNullOrEmpty(oldPublicId))
        {
            await DeleteImageAsync(oldPublicId);
        }

        var uploadParams = new ImageUploadParams()
        {
            File = new FileDescription(fileName, fileStream),
            Folder = $"{_folderName}/profiles",
            Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face"),
            PublicId = $"profile_{Guid.NewGuid()}"
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
        {
            throw new Exception(uploadResult.Error.Message);
        }

        return (uploadResult.SecureUrl.ToString(), uploadResult.PublicId);
    }

    public async Task DeleteImageAsync(string publicId)
    {
        var deletionParams = new DeletionParams(publicId);
        await _cloudinary.DestroyAsync(deletionParams);
    }
}
