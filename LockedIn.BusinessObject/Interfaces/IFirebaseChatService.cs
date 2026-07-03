using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.Conversations;

namespace LockedIn.BusinessObject.Interfaces;

public interface IFirebaseChatService
{
    Task<ApiResponse<ChatMessageResponse>> SendMessageAsync(SendMessageRequest request);
    Task<ApiResponse<MessagePaginationResponse>> GetMessagesAsync(Guid conversationId, string? cursor, int limit);
    Task<ApiResponse<string>> MarkMessagesAsReadAsync(Guid conversationId);
}
