using System;

namespace LockedIn.DataAccess.Models;

public partial class AddonWebhookLog
{
    public Guid Id { get; set; }
    public Guid? AttemptId { get; set; }
    public string Provider { get; set; } = null!;
    public string? EventType { get; set; }
    public string? EventId { get; set; }
    public string RawPayload { get; set; } = null!;
    public bool IsValidSignature { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime ReceivedAt { get; set; }

    public virtual AddonPaymentAttempt? Attempt { get; set; }
}
