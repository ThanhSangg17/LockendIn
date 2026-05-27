using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;

namespace LockedIn.BusinessObject.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<string>> GetMyNotificationsAsync()
    {
        return await Task.FromResult(ApiResponse<string>.Ok("Not implemented yet"));
    }

    public async Task<ApiResponse<string>> MarkAsReadAsync(Guid notificationId)
    {
        return await Task.FromResult(ApiResponse<string>.Ok("Not implemented yet"));
    }

    public async Task<ApiResponse<string>> MarkAllAsReadAsync()
    {
        return await Task.FromResult(ApiResponse<string>.Ok("Not implemented yet"));
    }

    public async Task<ApiResponse<string>> DeleteNotificationAsync(Guid notificationId)
    {
        return await Task.FromResult(ApiResponse<string>.Ok("Not implemented yet"));
    }

}
