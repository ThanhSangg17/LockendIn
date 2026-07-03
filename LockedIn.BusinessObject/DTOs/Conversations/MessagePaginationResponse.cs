using System.Collections.Generic;

namespace LockedIn.BusinessObject.DTOs.Conversations;

public class MessagePaginationResponse
{
    public IReadOnlyList<ChatMessageResponse> Messages { get; set; } = new List<ChatMessageResponse>();
    public string? NextCursor { get; set; }
    public bool HasMore { get; set; }
}
