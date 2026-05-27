using System;
using System.Collections.Generic;

namespace LockedIn.DataAccess.Models;

public partial class Booking
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public Guid PtProfileId { get; set; }

    public Guid PackageId { get; set; }

    public int Status { get; set; }

    public decimal TotalAmount { get; set; }

    public int SessionCount { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? SettlementDueAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Conversation? Conversation { get; set; }

    public virtual CustomerProfile Customer { get; set; } = null!;

    public virtual Dispute? Dispute { get; set; }

    public virtual Package Package { get; set; } = null!;

    public virtual Payment? Payment { get; set; }

    public virtual PtProfile PtProfile { get; set; } = null!;

    public virtual Review? Review { get; set; }

    public virtual Settlement? Settlement { get; set; }

    public virtual Workspace? Workspace { get; set; }
}
