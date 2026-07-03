using System;

namespace LockedIn.BusinessObject.DTOs.Conversations;

public class ConversationListResponse
{
    public Guid ConversationId { get; set; }
    public Guid BookingId { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public Guid OtherUserId { get; set; }
    public string OtherUserName { get; set; } = string.Empty;
    public string? LastMessagePreview { get; set; }
    public DateTime? LastActivityAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
