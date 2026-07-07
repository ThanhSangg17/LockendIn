using System;
using System.Collections.Generic;

namespace LockedIn.DataAccess.Models;

public partial class AddonOrder
{
    public Guid Id { get; set; }
    public Guid PtProfileId { get; set; }
    public int Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = null!;
    public DateTime? PaidAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual PtProfile PtProfile { get; set; } = null!;
    public virtual ICollection<AddonOrderItem> AddonOrderItems { get; set; } = new List<AddonOrderItem>();
    public virtual ICollection<AddonPaymentAttempt> AddonPaymentAttempts { get; set; } = new List<AddonPaymentAttempt>();
    public virtual ICollection<AddonEntitlement> AddonEntitlements { get; set; } = new List<AddonEntitlement>();
}
