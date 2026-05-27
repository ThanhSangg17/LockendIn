using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;

namespace LockedIn.BusinessObject.Services;

public class WorkspaceService : IWorkspaceService
{
    private readonly IUnitOfWork _unitOfWork;

    public WorkspaceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<string>> GetWorkspaceByBookingAsync(Guid bookingId)
    {
        return await Task.FromResult(ApiResponse<string>.Ok("Not implemented yet"));
    }

    public async Task<ApiResponse<string>> GetWorkspaceByIdAsync(Guid workspaceId)
    {
        return await Task.FromResult(ApiResponse<string>.Ok("Not implemented yet"));
    }

    public async Task<ApiResponse<string>> UpdateCourseNoteAsync(Guid workspaceId)
    {
        return await Task.FromResult(ApiResponse<string>.Ok("Not implemented yet"));
    }

}
