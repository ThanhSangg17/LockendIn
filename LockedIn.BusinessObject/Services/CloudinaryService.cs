using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.Uploads;
using LockedIn.BusinessObject.Interfaces;

namespace LockedIn.BusinessObject.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private static readonly string[] AllowedFileExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };

    public CloudinaryService(IConfiguration configuration)
    {
        var cloudName = configuration["Cloudinary:CloudName"] 
            ?? throw new ArgumentNullException("Cloudinary:CloudName", "Cloudinary CloudName configuration is missing.");
        var apiKey = configuration["Cloudinary:ApiKey"] 
            ?? throw new ArgumentNullException("Cloudinary:ApiKey", "Cloudinary ApiKey configuration is missing.");
        var apiSecret = configuration["Cloudinary:ApiSecret"] 
            ?? throw new ArgumentNullException("Cloudinary:ApiSecret", "Cloudinary ApiSecret configuration is missing.");

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<ApiResponse<FileUploadResponse>> UploadImageAsync(IFormFile file, string folder)
    {
        // 1. Validation
        if (file == null || file.Length == 0)
        {
            return ApiResponse<FileUploadResponse>.Fail("No file provided or the file is empty.");
        }

        if (file.Length > MaxFileSize)
        {
            return ApiResponse<FileUploadResponse>.Fail("File size exceeds the 10MB limit.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(extension))
        {
            return ApiResponse<FileUploadResponse>.Fail($"Invalid image format. Allowed extensions: {string.Join(", ", AllowedImageExtensions)}");
        }

        // 2. Upload
        try
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                return ApiResponse<FileUploadResponse>.Fail(uploadResult.Error.Message);
            }

            var response = new FileUploadResponse
            {
                PublicId = uploadResult.PublicId,
                Url = uploadResult.Url?.ToString() ?? string.Empty,
                SecureUrl = uploadResult.SecureUrl?.ToString() ?? string.Empty,
                Bytes = uploadResult.Bytes,
                Format = uploadResult.Format ?? string.Empty,
                ResourceType = uploadResult.ResourceType ?? string.Empty
            };

            return ApiResponse<FileUploadResponse>.Ok(response, "Image uploaded successfully.");
        }
        catch (Exception ex)
        {
            return ApiResponse<FileUploadResponse>.Fail($"An error occurred during upload: {ex.Message}");
        }
    }

    public async Task<ApiResponse<FileUploadResponse>> UploadFileAsync(IFormFile file, string folder)
    {
        // 1. Validation
        if (file == null || file.Length == 0)
        {
            return ApiResponse<FileUploadResponse>.Fail("No file provided or the file is empty.");
        }

        if (file.Length > MaxFileSize)
        {
            return ApiResponse<FileUploadResponse>.Fail("File size exceeds the 10MB limit.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedFileExtensions.Contains(extension))
        {
            return ApiResponse<FileUploadResponse>.Fail($"Invalid file format. Allowed extensions: {string.Join(", ", AllowedFileExtensions)}");
        }

        // 2. Upload
        try
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                return ApiResponse<FileUploadResponse>.Fail(uploadResult.Error.Message);
            }

            var response = new FileUploadResponse
            {
                PublicId = uploadResult.PublicId,
                Url = uploadResult.Url?.ToString() ?? string.Empty,
                SecureUrl = uploadResult.SecureUrl?.ToString() ?? string.Empty,
                Bytes = uploadResult.Bytes,
                Format = uploadResult.Format ?? string.Empty,
                ResourceType = uploadResult.ResourceType ?? string.Empty
            };

            return ApiResponse<FileUploadResponse>.Ok(response, "File uploaded successfully.");
        }
        catch (Exception ex)
        {
            return ApiResponse<FileUploadResponse>.Fail($"An error occurred during upload: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> DeleteFileAsync(string publicId)
    {
        if (string.IsNullOrWhiteSpace(publicId))
        {
            return ApiResponse<string>.Fail("Public ID cannot be empty.");
        }

        try
        {
            // Try deleting as an image first
            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };
            var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

            if (deletionResult.Result == "ok")
            {
                return ApiResponse<string>.Ok(publicId, "Image deleted successfully.");
            }

            // If not successful, try deleting as raw resource type
            deletionParams.ResourceType = ResourceType.Raw;
            deletionResult = await _cloudinary.DestroyAsync(deletionParams);

            if (deletionResult.Result == "ok")
            {
                return ApiResponse<string>.Ok(publicId, "File deleted successfully.");
            }

            return ApiResponse<string>.Fail($"Failed to delete resource from Cloudinary. Result: {deletionResult.Result}");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Fail($"An error occurred during deletion: {ex.Message}");
        }
    }
}
