using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.Feedbacks;
using LockedIn.BusinessObject.Enums;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.Models;
using LockedIn.DataAccess.UnitOfWork;

namespace LockedIn.BusinessObject.Services;

public class FeedbackService : IFeedbackService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public FeedbackService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<FeedbackResponse>> CreateFeedbackAsync(CreateFeedbackRequest request)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<FeedbackResponse>.Fail("User is not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;
        var user = await _unitOfWork.Users.Query()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
        {
            return ApiResponse<FeedbackResponse>.Fail("User not found.");
        }

        var content = request.Content?.Trim();
        if (string.IsNullOrWhiteSpace(content) || content.Length < 5)
        {
            return ApiResponse<FeedbackResponse>.Fail("Nội dung góp ý phải có ít nhất 5 ký tự.");
        }

        if (content.Length > 2000)
        {
            return ApiResponse<FeedbackResponse>.Fail("Nội dung góp ý không được vượt quá 2000 ký tự.");
        }

        var feedback = new Feedback
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Feedbacks.AddAsync(feedback);
        await _unitOfWork.SaveChangesAsync();

        var response = MapToFeedbackResponse(feedback, user);
        return ApiResponse<FeedbackResponse>.Ok(response, "Gửi góp ý thành công. Cảm ơn bạn đã đóng góp ý kiến!");
    }

    public async Task<ApiResponse<IReadOnlyList<FeedbackResponse>>> GetMyFeedbacksAsync()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<IReadOnlyList<FeedbackResponse>>.Fail("User is not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;
        var feedbacks = await _unitOfWork.Feedbacks.Query()
            .Include(f => f.User)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        var response = feedbacks.Select(f => MapToFeedbackResponse(f, f.User)).ToList();
        return ApiResponse<IReadOnlyList<FeedbackResponse>>.Ok(response, "Lấy danh sách góp ý thành công.");
    }

    public async Task<ApiResponse<FeedbackResponse>> UpdateFeedbackAsync(Guid id, UpdateFeedbackRequest request)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<FeedbackResponse>.Fail("User is not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;
        var feedback = await _unitOfWork.Feedbacks.Query()
            .Include(f => f.User)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (feedback == null)
        {
            return ApiResponse<FeedbackResponse>.Fail("Feedback not found.");
        }

        if (feedback.UserId != userId)
        {
            return ApiResponse<FeedbackResponse>.Fail("Bạn chỉ có quyền chỉnh sửa góp ý của chính mình.");
        }

        var content = request.Content?.Trim();
        if (string.IsNullOrWhiteSpace(content) || content.Length < 5)
        {
            return ApiResponse<FeedbackResponse>.Fail("Nội dung góp ý phải có ít nhất 5 ký tự.");
        }

        if (content.Length > 2000)
        {
            return ApiResponse<FeedbackResponse>.Fail("Nội dung góp ý không được vượt quá 2000 ký tự.");
        }

        feedback.Content = content;
        feedback.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Feedbacks.Update(feedback);
        await _unitOfWork.SaveChangesAsync();

        var response = MapToFeedbackResponse(feedback, feedback.User);
        return ApiResponse<FeedbackResponse>.Ok(response, "Cập nhật góp ý thành công.");
    }

    public async Task<ApiResponse<bool>> DeleteMyFeedbackAsync(Guid id)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<bool>.Fail("User is not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;
        var feedback = await _unitOfWork.Feedbacks.Query()
            .FirstOrDefaultAsync(f => f.Id == id);

        if (feedback == null)
        {
            return ApiResponse<bool>.Fail("Feedback not found.");
        }

        if (feedback.UserId != userId)
        {
            return ApiResponse<bool>.Fail("Bạn chỉ có quyền xóa góp ý của chính mình.");
        }

        _unitOfWork.Feedbacks.Delete(feedback);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Xóa góp ý thành công.");
    }

    public async Task<ApiResponse<IReadOnlyList<FeedbackResponse>>> GetAllFeedbacksAsync()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<IReadOnlyList<FeedbackResponse>>.Fail("User is not authenticated.");
        }

        var feedbacks = await _unitOfWork.Feedbacks.Query()
            .Include(f => f.User)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        var response = feedbacks.Select(f => MapToFeedbackResponse(f, f.User)).ToList();
        return ApiResponse<IReadOnlyList<FeedbackResponse>>.Ok(response, "Lấy danh sách tất cả góp ý thành công.");
    }

    public async Task<ApiResponse<bool>> DeleteFeedbackAsync(Guid id)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<bool>.Fail("User is not authenticated.");
        }

        if (_currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<bool>.Fail("Chỉ Admin mới có quyền xóa góp ý hệ thống.");
        }

        var feedback = await _unitOfWork.Feedbacks.Query()
            .FirstOrDefaultAsync(f => f.Id == id);

        if (feedback == null)
        {
            return ApiResponse<bool>.Fail("Feedback not found.");
        }

        _unitOfWork.Feedbacks.Delete(feedback);

        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                ActorUserId = _currentUserService.UserId!.Value,
                Action = "DeleteFeedback",
                EntityName = "Feedback",
                EntityId = id,
                MetadataJson = System.Text.Json.JsonSerializer.Serialize(new { FeedbackId = id, DeletedUserId = feedback.UserId }),
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AuditLogs.AddAsync(auditLog);
        }
        catch
        {
            // ignore audit log failure
        }

        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Xóa góp ý thành công.");
    }

    private static FeedbackResponse MapToFeedbackResponse(Feedback feedback, User user)
    {
        return new FeedbackResponse
        {
            Id = feedback.Id,
            UserId = feedback.UserId,
            UserFullName = user.FullName ?? "Người dùng",
            UserEmail = user.Email ?? "",
            UserAvatarUrl = user.AvatarUrl,
            UserRole = user.Role,
            Content = feedback.Content,
            CreatedAt = feedback.CreatedAt,
            UpdatedAt = feedback.UpdatedAt
        };
    }
}
