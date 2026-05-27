using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;

namespace LockedIn.BusinessObject.Interfaces;

public interface IWorkspaceService
{
    Task<ApiResponse<string>> GetWorkspaceByBookingAsync(Guid bookingId);
    Task<ApiResponse<string>> GetWorkspaceByIdAsync(Guid workspaceId);
    Task<ApiResponse<string>> UpdateCourseNoteAsync(Guid workspaceId);
}
