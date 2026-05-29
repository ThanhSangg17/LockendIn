using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.Reviews;

namespace LockedIn.BusinessObject.Interfaces;

public interface IReviewService
{
    Task<ApiResponse<ReviewResponse>> CreateReviewAsync(CreateReviewRequest request);
    Task<ApiResponse<IReadOnlyList<ReviewResponse>>> GetMyReviewsAsync();
    Task<ApiResponse<IReadOnlyList<ReviewResponse>>> GetReviewsByPtAsync(Guid ptProfileId);
    Task<ApiResponse<ReviewResponse>> UpdateReviewAsync(Guid reviewId, UpdateReviewRequest request);
    Task<ApiResponse<string>> DeleteReviewAsync(Guid reviewId);
    Task<ApiResponse<ReviewResponse>> ReplyReviewAsync(Guid reviewId, string reply);
    Task<ApiResponse<bool>> HideReviewAsync(Guid reviewId);
    Task<ApiResponse<ReviewResponse>> GetReviewByIdAsync(Guid reviewId);
}
