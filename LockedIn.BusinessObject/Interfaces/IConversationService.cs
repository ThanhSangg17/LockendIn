using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;

namespace LockedIn.BusinessObject.Interfaces;

public interface IConversationService
{
    Task<ApiResponse<string>> GetConversationByWorkspaceAsync(Guid workspaceId);
    Task<ApiResponse<string>> CreateConversationByBookingAsync(Guid bookingId);
}
