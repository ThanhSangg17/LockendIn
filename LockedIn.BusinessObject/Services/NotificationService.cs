using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.BusinessObject.DTOs.Notifications;
using LockedIn.DataAccess.Models;

namespace LockedIn.BusinessObject.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public NotificationService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<IReadOnlyList<NotificationResponse>>> GetMyNotificationsAsync()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<IReadOnlyList<NotificationResponse>>.Fail("User is not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;

        var notifications = await _unitOfWork.Notifications.Query()
            .Where(n => n.UserId == userId && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        var response = notifications.Select(n => new NotificationResponse
        {
            Id = n.Id,
            UserId = n.UserId,
            Title = n.Title,
            Content = n.Content,
            Type = n.Type,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        }).ToList();

        return ApiResponse<IReadOnlyList<NotificationResponse>>.Ok(response, "Notifications retrieved successfully.");
    }

    public async Task<ApiResponse<NotificationResponse>> MarkAsReadAsync(Guid notificationId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<NotificationResponse>.Fail("User is not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;

        var notification = await _unitOfWork.Notifications.Query()
            .FirstOrDefaultAsync(n => n.Id == notificationId && !n.IsDeleted);

        if (notification == null)
        {
            return ApiResponse<NotificationResponse>.Fail("Notification not found.");
        }

        if (notification.UserId != userId)
        {
            return ApiResponse<NotificationResponse>.Fail("Access denied to this notification.");
        }

        notification.IsRead = true;
        _unitOfWork.Notifications.Update(notification);
        await _unitOfWork.SaveChangesAsync();

        var response = new NotificationResponse
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Title = notification.Title,
            Content = notification.Content,
            Type = notification.Type,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };

        return ApiResponse<NotificationResponse>.Ok(response, "Notification marked as read.");
    }

    public async Task<ApiResponse<string>> MarkAllAsReadAsync()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<string>.Fail("User is not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;

        var unreadNotifications = await _unitOfWork.Notifications.Query()
            .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
            .ToListAsync();

        if (unreadNotifications.Any())
        {
            foreach (var n in unreadNotifications)
            {
                n.IsRead = true;
                _unitOfWork.Notifications.Update(n);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        return ApiResponse<string>.Ok("All notifications marked as read.", "All notifications marked as read successfully.");
    }

    public async Task<ApiResponse<string>> DeleteNotificationAsync(Guid notificationId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<string>.Fail("User is not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;

        var notification = await _unitOfWork.Notifications.Query()
            .FirstOrDefaultAsync(n => n.Id == notificationId && !n.IsDeleted);

        if (notification == null)
        {
            return ApiResponse<string>.Fail("Notification not found.");
        }

        if (notification.UserId != userId)
        {
            return ApiResponse<string>.Fail("Access denied to this notification.");
        }

        notification.IsDeleted = true;
        notification.DeletedAt = DateTime.UtcNow;
        notification.DeletedBy = userId;

        _unitOfWork.Notifications.Update(notification);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<string>.Ok("Notification soft-deleted successfully.", "Notification soft-deleted successfully.");
    }
}
