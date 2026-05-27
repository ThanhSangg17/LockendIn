using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.Marketplace;

namespace LockedIn.BusinessObject.Interfaces;

public interface IMarketplaceService
{
    Task<ApiResponse<PagedResult<MarketplacePtResponse>>> GetPtsAsync(PtSearchRequest request);
    Task<ApiResponse<MarketplacePtDetailResponse>> GetPtDetailAsync(Guid ptProfileId);
    Task<ApiResponse<IReadOnlyList<LockedIn.BusinessObject.DTOs.Packages.PackageResponse>>> GetPtPackagesAsync(Guid ptProfileId);
    Task<ApiResponse<IReadOnlyList<LockedIn.BusinessObject.DTOs.Reviews.ReviewResponse>>> GetPtReviewsAsync(Guid ptProfileId);
}
