using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.DataAccess.Models;
using LockedIn.BusinessObject.DTOs.Workspaces;
using LockedIn.BusinessObject.Enums;

namespace LockedIn.BusinessObject.Services;

public class WorkspaceService : IWorkspaceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public WorkspaceService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<WorkspaceResponse>> GetWorkspaceByBookingAsync(Guid bookingId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<WorkspaceResponse>.Fail("User is not authenticated.");
        }

        var booking = await _unitOfWork.Bookings.Query()
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
            return ApiResponse<WorkspaceResponse>.Fail("Booking not found.");
        }

        var (allowed, error) = await CheckBookingAccessAsync(booking);
        if (!allowed)
        {
            return ApiResponse<WorkspaceResponse>.Fail(error ?? "Access denied.");
        }

        var workspace = await _unitOfWork.Workspaces.Query()
            .FirstOrDefaultAsync(w => w.BookingId == bookingId);

        if (workspace == null)
        {
            return ApiResponse<WorkspaceResponse>.Fail("Workspace not found.");
        }

        var response = MapToWorkspaceResponse(workspace);
        return ApiResponse<WorkspaceResponse>.Ok(response, "Workspace retrieved successfully.");
    }

    public async Task<ApiResponse<WorkspaceResponse>> GetWorkspaceByIdAsync(Guid workspaceId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<WorkspaceResponse>.Fail("User is not authenticated.");
        }

        var workspace = await _unitOfWork.Workspaces.Query()
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null)
        {
            return ApiResponse<WorkspaceResponse>.Fail("Workspace not found.");
        }

        var (allowed, error) = await CheckWorkspaceAccessAsync(workspace);
        if (!allowed)
        {
            return ApiResponse<WorkspaceResponse>.Fail(error ?? "Access denied.");
        }

        var response = MapToWorkspaceResponse(workspace);
        return ApiResponse<WorkspaceResponse>.Ok(response, "Workspace retrieved successfully.");
    }

    public async Task<ApiResponse<WorkspaceResponse>> UpdateCourseNoteAsync(Guid workspaceId, UpdateCourseNoteRequest request)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<WorkspaceResponse>.Fail("User is not authenticated.");
        }

        var workspace = await _unitOfWork.Workspaces.Query()
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null)
        {
            return ApiResponse<WorkspaceResponse>.Fail("Workspace not found.");
        }

        var userId = _currentUserService.UserId.Value;

        if (_currentUserService.Role == (int)UserRole.PersonalTrainer)
        {
            var ptProfile = await _unitOfWork.PtProfiles.Query()
                .FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);
            if (ptProfile == null || workspace.PtProfileId != ptProfile.Id)
            {
                return ApiResponse<WorkspaceResponse>.Fail("You are not the personal trainer assigned to this workspace.");
            }
        }
        else if (_currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<WorkspaceResponse>.Fail("Only personal trainers or admins can update the course note.");
        }

        workspace.CourseNote = request.CourseNote;
        workspace.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Workspaces.Update(workspace);
        await _unitOfWork.SaveChangesAsync();

        var response = MapToWorkspaceResponse(workspace);
        return ApiResponse<WorkspaceResponse>.Ok(response, "Course note updated successfully.");
    }

    #region Access Control Helpers & Mapping

    private async Task<(bool Allowed, string? Error)> CheckWorkspaceAccessAsync(Workspace workspace)
    {
        var userId = _currentUserService.UserId!.Value;

        if (_currentUserService.Role == (int)UserRole.Customer)
        {
            var customerProfile = await _unitOfWork.CustomerProfiles.Query()
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);
            if (customerProfile == null || workspace.CustomerId != customerProfile.Id)
            {
                return (false, "Access denied to this workspace.");
            }
        }
        else if (_currentUserService.Role == (int)UserRole.PersonalTrainer)
        {
            var ptProfile = await _unitOfWork.PtProfiles.Query()
                .FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);
            if (ptProfile == null || workspace.PtProfileId != ptProfile.Id)
            {
                return (false, "Access denied to this workspace.");
            }
        }
        else if (_currentUserService.Role != (int)UserRole.Admin)
        {
            return (false, "Access denied to this workspace.");
        }

        return (true, null);
    }

    private async Task<(bool Allowed, string? Error)> CheckBookingAccessAsync(Booking booking)
    {
        var userId = _currentUserService.UserId!.Value;

        if (_currentUserService.Role == (int)UserRole.Customer)
        {
            var customerProfile = await _unitOfWork.CustomerProfiles.Query()
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);
            if (customerProfile == null || booking.CustomerId != customerProfile.Id)
            {
                return (false, "Access denied to this booking.");
            }
        }
        else if (_currentUserService.Role == (int)UserRole.PersonalTrainer)
        {
            var ptProfile = await _unitOfWork.PtProfiles.Query()
                .FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);
            if (ptProfile == null || booking.PtProfileId != ptProfile.Id)
            {
                return (false, "Access denied to this booking.");
            }
        }
        else if (_currentUserService.Role != (int)UserRole.Admin)
        {
            return (false, "Access denied to this booking.");
        }

        return (true, null);
    }

    private WorkspaceResponse MapToWorkspaceResponse(Workspace workspace)
    {
        return new WorkspaceResponse
        {
            Id = workspace.Id,
            BookingId = workspace.BookingId,
            CustomerId = workspace.CustomerId,
            PtProfileId = workspace.PtProfileId,
            Status = workspace.Status,
            CourseNote = workspace.CourseNote,
            CreatedAt = workspace.CreatedAt
        };
    }

    #endregion
}

