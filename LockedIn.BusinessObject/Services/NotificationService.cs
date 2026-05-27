using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.BusinessObject.DTOs.Notifications;

namespace LockedIn.BusinessObject.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<IReadOnlyList<NotificationResponse>>> GetMyNotificationsAsync()
    {
        return await Task.FromResult(ApiResponse<IReadOnlyList<NotificationResponse>>.Ok(new List<NotificationResponse>(), "Not implemented yet"));
    }

    public async Task<ApiResponse<NotificationResponse>> MarkAsReadAsync(Guid notificationId)
    {
        return await Task.FromResult(ApiResponse<NotificationResponse>.Ok(new NotificationResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<string>> MarkAllAsReadAsync()
    {
        return await Task.FromResult(ApiResponse<string>.Ok(string.Empty, "Not implemented yet"));
    }

    public async Task<ApiResponse<string>> DeleteNotificationAsync(Guid notificationId)
    {
        return await Task.FromResult(ApiResponse<string>.Ok(string.Empty, "Not implemented yet"));
    }
}
