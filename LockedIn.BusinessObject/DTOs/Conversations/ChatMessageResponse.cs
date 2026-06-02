using System;

namespace LockedIn.BusinessObject.DTOs.Conversations;

public class ChatMessageResponse
{
    public string Id { get; set; } = null!;
    public Guid ConversationId { get; set; }
    public Guid SenderUserId { get; set; }
    public string SenderName { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string MessageType { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
