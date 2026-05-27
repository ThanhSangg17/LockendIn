using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;

namespace LockedIn.BusinessObject.Interfaces;

public interface IMarketplaceService
{
    Task<ApiResponse<string>> GetPtsAsync();
    Task<ApiResponse<string>> GetPtDetailAsync(Guid ptProfileId);
    Task<ApiResponse<string>> GetPtPackagesAsync(Guid ptProfileId);
    Task<ApiResponse<string>> GetPtReviewsAsync(Guid ptProfileId);
}
