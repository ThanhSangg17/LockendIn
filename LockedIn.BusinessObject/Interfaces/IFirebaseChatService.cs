using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.Conversations;

namespace LockedIn.BusinessObject.Interfaces;

public interface IFirebaseChatService
{
    Task<ApiResponse<ChatMessageResponse>> SendMessageAsync(SendMessageRequest request);
    Task<ApiResponse<IReadOnlyList<ChatMessageResponse>>> GetMessagesAsync(Guid conversationId);
    Task<ApiResponse<string>> MarkMessagesAsReadAsync(Guid conversationId);
}
