using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.DataAccess.Models;
using LockedIn.BusinessObject.DTOs.Conversations;
using LockedIn.BusinessObject.Enums;

namespace LockedIn.BusinessObject.Services;

public class ConversationService : IConversationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ConversationService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<ConversationResponse>> GetConversationByWorkspaceAsync(Guid workspaceId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<ConversationResponse>.Fail("User is not authenticated.");
        }

        var workspace = await _unitOfWork.Workspaces.Query()
            .FirstOrDefaultAsync(w => w.Id == workspaceId);

        if (workspace == null)
        {
            return ApiResponse<ConversationResponse>.Fail("Workspace not found.");
        }

        var (allowed, error) = await CheckWorkspaceAccessAsync(workspace);
        if (!allowed)
        {
            return ApiResponse<ConversationResponse>.Fail(error ?? "Access denied.");
        }

        var conversation = await _unitOfWork.Conversations.Query()
            .FirstOrDefaultAsync(c => c.BookingId == workspace.BookingId);

        if (conversation == null)
        {
            return ApiResponse<ConversationResponse>.Fail("Conversation not found.");
        }

        var response = MapToConversationResponse(conversation);
        return ApiResponse<ConversationResponse>.Ok(response, "Conversation retrieved successfully.");
    }

    public async Task<ApiResponse<ConversationResponse>> CreateConversationByBookingAsync(Guid bookingId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<ConversationResponse>.Fail("User is not authenticated.");
        }

        var booking = await _unitOfWork.Bookings.Query()
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
            return ApiResponse<ConversationResponse>.Fail("Booking not found.");
        }

        var userId = _currentUserService.UserId.Value;

        if (_currentUserService.Role == (int)UserRole.PersonalTrainer)
        {
            var ptProfile = await _unitOfWork.PtProfiles.Query()
                .FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);
            if (ptProfile == null || booking.PtProfileId != ptProfile.Id)
            {
                return ApiResponse<ConversationResponse>.Fail("You are not the personal trainer assigned to this booking.");
            }
        }
        else if (_currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<ConversationResponse>.Fail("Only personal trainers or admins can create conversations manually.");
        }

        if (booking.Status != (int)BookingStatus.Active)
        {
            return ApiResponse<ConversationResponse>.Fail("Booking is not active.");
        }

        var existingConversation = await _unitOfWork.Conversations.Query()
            .FirstOrDefaultAsync(c => c.BookingId == bookingId);

        if (existingConversation != null)
        {
            var existingResponse = MapToConversationResponse(existingConversation);
            return ApiResponse<ConversationResponse>.Ok(existingResponse, "Conversation already exists.");
        }

        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            CustomerId = booking.CustomerId,
            PtProfileId = booking.PtProfileId,
            FirebaseConversationId = "firebase-" + booking.Id.ToString(),
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            LastMessagePreview = null
        };

        await _unitOfWork.Conversations.AddAsync(conversation);
        await _unitOfWork.SaveChangesAsync();

        var response = MapToConversationResponse(conversation);
        return ApiResponse<ConversationResponse>.Ok(response, "Conversation created successfully.");
    }

    public async Task<ApiResponse<PagedResult<ConversationListResponse>>> GetMyConversationsAsync(PaginationRequest request)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<PagedResult<ConversationListResponse>>.Fail("User is not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;
        IQueryable<Conversation> query = _unitOfWork.Conversations.Query()
            .Include(c => c.Booking)
            .ThenInclude(b => b.Package)
            .Include(c => c.Customer)
            .ThenInclude(cu => cu.User)
            .Include(c => c.PtProfile)
            .ThenInclude(pt => pt.User);

        if (_currentUserService.Role == (int)UserRole.Customer)
        {
            var customerProfile = await _unitOfWork.CustomerProfiles.Query()
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);
            if (customerProfile == null)
            {
                return ApiResponse<PagedResult<ConversationListResponse>>.Fail("Customer profile not found.");
            }
            query = query.Where(c => c.CustomerId == customerProfile.Id);
        }
        else if (_currentUserService.Role == (int)UserRole.PersonalTrainer)
        {
            var ptProfile = await _unitOfWork.PtProfiles.Query()
                .FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);
            if (ptProfile == null)
            {
                return ApiResponse<PagedResult<ConversationListResponse>>.Fail("Personal trainer profile not found.");
            }
            query = query.Where(c => c.PtProfileId == ptProfile.Id);
        }
        else
        {
            return ApiResponse<PagedResult<ConversationListResponse>>.Fail("Only Customer and PT can view conversations.");
        }

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

        var conversations = await query
            .OrderByDescending(c => c.LastActivityAt ?? c.CreatedAt)
            .ThenByDescending(c => c.CreatedAt)
            .ThenByDescending(c => c.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var isCustomer = _currentUserService.Role == (int)UserRole.Customer;

        var items = conversations.Select(c => new ConversationListResponse
        {
            ConversationId = c.Id,
            BookingId = c.BookingId,
            PackageName = c.Booking?.Package?.Name ?? string.Empty,
            OtherUserId = isCustomer ? c.PtProfile.UserId : c.Customer.UserId,
            OtherUserName = isCustomer ? c.PtProfile?.User?.FullName ?? string.Empty : c.Customer?.User?.FullName ?? string.Empty,
            LastMessagePreview = c.LastMessagePreview,
            LastActivityAt = c.LastActivityAt,
            CreatedAt = c.CreatedAt
        }).ToList();

        var pagedResult = new PagedResult<ConversationListResponse>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };

        return ApiResponse<PagedResult<ConversationListResponse>>.Ok(pagedResult, "Conversations retrieved successfully.");
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

    private ConversationResponse MapToConversationResponse(Conversation conversation)
    {
        return new ConversationResponse
        {
            Id = conversation.Id,
            BookingId = conversation.BookingId,
            CustomerId = conversation.CustomerId,
            PtProfileId = conversation.PtProfileId,
            FirebaseConversationId = conversation.FirebaseConversationId,
            CreatedAt = conversation.CreatedAt
        };
    }

    #endregion
}

