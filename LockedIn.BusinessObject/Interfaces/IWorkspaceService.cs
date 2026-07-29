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

    // Proposal & Scheduling APIs
    Task<ApiResponse<SessionProposalResponse>> CreateProposalAsync(Guid workspaceId, CreateSessionProposalRequest request);
    Task<ApiResponse<SessionProposalResponse>> GetActiveProposalAsync(Guid workspaceId);
    Task<ApiResponse<SessionProposalResponse>> GetProposalAvailabilityAsync(Guid workspaceId, Guid proposalId);
    Task<ApiResponse<bool>> RevokeProposalAsync(Guid workspaceId, Guid proposalId);
    Task<ApiResponse<WorkspaceSessionResponse>> SelectProposalSlotAsync(Guid workspaceId, Guid proposalId, SelectSlotRequest request);

    // Check-in & Complete APIs
    Task<ApiResponse<WorkspaceSessionResponse>> CheckInSessionAsync(Guid workspaceId, Guid sessionId);
    Task<ApiResponse<WorkspaceSessionResponse>> CompleteSessionAsync(Guid workspaceId, Guid sessionId);
}
