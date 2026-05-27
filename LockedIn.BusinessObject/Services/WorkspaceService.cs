using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.BusinessObject.DTOs.Workspaces;

namespace LockedIn.BusinessObject.Services;

public class WorkspaceService : IWorkspaceService
{
    private readonly IUnitOfWork _unitOfWork;

    public WorkspaceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<WorkspaceResponse>> GetWorkspaceByBookingAsync(Guid bookingId)
    {
        return await Task.FromResult(ApiResponse<WorkspaceResponse>.Ok(new WorkspaceResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<WorkspaceResponse>> GetWorkspaceByIdAsync(Guid workspaceId)
    {
        return await Task.FromResult(ApiResponse<WorkspaceResponse>.Ok(new WorkspaceResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<WorkspaceResponse>> UpdateCourseNoteAsync(Guid workspaceId, UpdateCourseNoteRequest request)
    {
        return await Task.FromResult(ApiResponse<WorkspaceResponse>.Ok(new WorkspaceResponse(), "Not implemented yet"));
    }
}
