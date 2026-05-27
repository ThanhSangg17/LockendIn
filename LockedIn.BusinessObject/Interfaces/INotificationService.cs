using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;

namespace LockedIn.BusinessObject.Interfaces;

public interface INotificationService
{
    Task<ApiResponse<string>> GetMyNotificationsAsync();
    Task<ApiResponse<string>> MarkAsReadAsync(Guid notificationId);
    Task<ApiResponse<string>> MarkAllAsReadAsync();
    Task<ApiResponse<string>> DeleteNotificationAsync(Guid notificationId);
}
