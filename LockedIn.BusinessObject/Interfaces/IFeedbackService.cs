using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.Feedbacks;

namespace LockedIn.BusinessObject.Interfaces;

public interface IFeedbackService
{
    Task<ApiResponse<FeedbackResponse>> CreateFeedbackAsync(CreateFeedbackRequest request);

    Task<ApiResponse<IReadOnlyList<FeedbackResponse>>> GetMyFeedbacksAsync();

    Task<ApiResponse<FeedbackResponse>> UpdateFeedbackAsync(Guid id, UpdateFeedbackRequest request);

    Task<ApiResponse<bool>> DeleteMyFeedbackAsync(Guid id);

    Task<ApiResponse<IReadOnlyList<FeedbackResponse>>> GetAllFeedbacksAsync();

    Task<ApiResponse<bool>> DeleteFeedbackAsync(Guid id);
}
