using System;
using System.Collections.Generic;

namespace LockedIn.DataAccess.Models;

public partial class AddonEntitlement
{
    public Guid Id { get; set; }
    public Guid PtProfileId { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? OrderItemId { get; set; }
    public string ProductCode { get; set; } = null!;
    public string FulfillmentType { get; set; } = null!;
    public int QuantityGranted { get; set; }
    public int QuantityRemaining { get; set; }
    public int Status { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual PtProfile PtProfile { get; set; } = null!;
    public virtual AddonOrder? Order { get; set; }
    public virtual AddonOrderItem? OrderItem { get; set; }
    public virtual ICollection<AddonQuotaReservation> AddonQuotaReservations { get; set; } = new List<AddonQuotaReservation>();
}
