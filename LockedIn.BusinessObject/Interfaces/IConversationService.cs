using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.Conversations;

namespace LockedIn.BusinessObject.Interfaces;

public interface IConversationService
{
    Task<ApiResponse<ConversationResponse>> GetConversationByWorkspaceAsync(Guid workspaceId);
    Task<ApiResponse<ConversationResponse>> CreateConversationByBookingAsync(Guid bookingId);
}
