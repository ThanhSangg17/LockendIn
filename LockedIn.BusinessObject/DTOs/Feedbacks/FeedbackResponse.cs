using System;

namespace LockedIn.BusinessObject.DTOs.Feedbacks;

public class FeedbackResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string UserFullName { get; set; } = null!;

    public string UserEmail { get; set; } = null!;

    public string? UserAvatarUrl { get; set; }

    public int UserRole { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
