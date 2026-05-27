using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.BusinessObject.DTOs.Conversations;

namespace LockedIn.BusinessObject.Services;

public class ConversationService : IConversationService
{
    private readonly IUnitOfWork _unitOfWork;

    public ConversationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<ConversationResponse>> GetConversationByWorkspaceAsync(Guid workspaceId)
    {
        return await Task.FromResult(ApiResponse<ConversationResponse>.Ok(new ConversationResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<ConversationResponse>> CreateConversationByBookingAsync(Guid bookingId)
    {
        return await Task.FromResult(ApiResponse<ConversationResponse>.Ok(new ConversationResponse(), "Not implemented yet"));
    }
}
