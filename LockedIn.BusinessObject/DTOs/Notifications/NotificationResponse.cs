using System;

namespace LockedIn.BusinessObject.DTOs.Notifications;

public class NotificationResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public int Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
