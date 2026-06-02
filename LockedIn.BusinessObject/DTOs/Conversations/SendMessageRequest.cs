using System;

namespace LockedIn.BusinessObject.DTOs.Conversations;

public class SendMessageRequest
{
    public Guid ConversationId { get; set; }
    public string Content { get; set; } = null!;
    public string MessageType { get; set; } = null!;
}
