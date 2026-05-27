using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.Notifications;

namespace LockedIn.BusinessObject.Interfaces;

public interface INotificationService
{
    Task<ApiResponse<IReadOnlyList<NotificationResponse>>> GetMyNotificationsAsync();
    Task<ApiResponse<NotificationResponse>> MarkAsReadAsync(Guid notificationId);
    Task<ApiResponse<string>> MarkAllAsReadAsync();
    Task<ApiResponse<string>> DeleteNotificationAsync(Guid notificationId);
}
