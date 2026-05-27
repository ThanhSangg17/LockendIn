using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.BusinessObject.DTOs.Marketplace;

namespace LockedIn.BusinessObject.Services;

public class MarketplaceService : IMarketplaceService
{
    private readonly IUnitOfWork _unitOfWork;

    public MarketplaceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResult<MarketplacePtResponse>>> GetPtsAsync(PtSearchRequest request)
    {
        return await Task.FromResult(ApiResponse<PagedResult<MarketplacePtResponse>>.Ok(new PagedResult<MarketplacePtResponse>(), "Not implemented yet"));
    }

    public async Task<ApiResponse<MarketplacePtDetailResponse>> GetPtDetailAsync(Guid ptProfileId)
    {
        return await Task.FromResult(ApiResponse<MarketplacePtDetailResponse>.Ok(new MarketplacePtDetailResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<IReadOnlyList<LockedIn.BusinessObject.DTOs.Packages.PackageResponse>>> GetPtPackagesAsync(Guid ptProfileId)
    {
        return await Task.FromResult(ApiResponse<IReadOnlyList<LockedIn.BusinessObject.DTOs.Packages.PackageResponse>>.Ok(new List<LockedIn.BusinessObject.DTOs.Packages.PackageResponse>(), "Not implemented yet"));
    }

    public async Task<ApiResponse<IReadOnlyList<LockedIn.BusinessObject.DTOs.Reviews.ReviewResponse>>> GetPtReviewsAsync(Guid ptProfileId)
    {
        return await Task.FromResult(ApiResponse<IReadOnlyList<LockedIn.BusinessObject.DTOs.Reviews.ReviewResponse>>.Ok(new List<LockedIn.BusinessObject.DTOs.Reviews.ReviewResponse>(), "Not implemented yet"));
    }

}
