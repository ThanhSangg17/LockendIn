using System;
using System.Collections.Generic;

namespace LockedIn.DataAccess.Models;

public partial class PaymentWebhookLog
{
    public Guid Id { get; set; }

    public Guid? PaymentId { get; set; }

    public string Provider { get; set; } = null!;

    public string? EventType { get; set; }

    public string? EventId { get; set; }

    public string RawPayload { get; set; } = null!;

    public bool IsValidSignature { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public DateTime ReceivedAt { get; set; }

    public virtual Payment? Payment { get; set; }
}
