using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.Uploads;

namespace LockedIn.BusinessObject.Interfaces;

public interface ICloudinaryService
{
    Task<ApiResponse<FileUploadResponse>> UploadImageAsync(IFormFile file, string folder);
    Task<ApiResponse<FileUploadResponse>> UploadFileAsync(IFormFile file, string folder);
    Task<ApiResponse<string>> DeleteFileAsync(string publicId);
}
