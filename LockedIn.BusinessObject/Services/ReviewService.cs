using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.BusinessObject.DTOs.Reviews;

namespace LockedIn.BusinessObject.Services;

public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReviewService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<ReviewResponse>> CreateReviewAsync(CreateReviewRequest request)
    {
        return await Task.FromResult(ApiResponse<ReviewResponse>.Ok(new ReviewResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<IReadOnlyList<ReviewResponse>>> GetMyReviewsAsync()
    {
        return await Task.FromResult(ApiResponse<IReadOnlyList<ReviewResponse>>.Ok(new List<ReviewResponse>(), "Not implemented yet"));
    }

    public async Task<ApiResponse<IReadOnlyList<ReviewResponse>>> GetReviewsByPtAsync(Guid ptProfileId)
    {
        return await Task.FromResult(ApiResponse<IReadOnlyList<ReviewResponse>>.Ok(new List<ReviewResponse>(), "Not implemented yet"));
    }

    public async Task<ApiResponse<ReviewResponse>> UpdateReviewAsync(Guid reviewId, UpdateReviewRequest request)
    {
        return await Task.FromResult(ApiResponse<ReviewResponse>.Ok(new ReviewResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<string>> DeleteReviewAsync(Guid reviewId)
    {
        return await Task.FromResult(ApiResponse<string>.Ok(string.Empty, "Not implemented yet"));
    }
}
