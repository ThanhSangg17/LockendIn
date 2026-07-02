using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.Workspaces;

namespace LockedIn.BusinessObject.Interfaces;

public interface IWorkspaceService
{
    Task<ApiResponse<WorkspaceResponse>> GetWorkspaceByBookingAsync(Guid bookingId);
    Task<ApiResponse<WorkspaceResponse>> GetWorkspaceByIdAsync(Guid workspaceId);
    Task<ApiResponse<WorkspaceResponse>> UpdateCourseNoteAsync(Guid workspaceId, UpdateCourseNoteRequest request);
    Task<ApiResponse<WorkspaceSessionResponse>> CreateSessionAsync(Guid workspaceId, CreateWorkspaceSessionRequest request);
    Task<ApiResponse<WorkspaceProgressResponse>> GetWorkspaceProgressAsync(Guid workspaceId);
}
