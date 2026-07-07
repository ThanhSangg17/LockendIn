using System;

namespace LockedIn.DataAccess.Models;

public partial class AddonQuotaReservation
{
    public Guid Id { get; set; }
    public Guid PtProfileId { get; set; }
    public string ReservationType { get; set; } = null!;
    public Guid? EntitlementId { get; set; }
    public DateOnly? QuotaDate { get; set; }
    public Guid GenerationRequestId { get; set; }
    public int Status { get; set; }
    public DateTime ReservedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? FinalizedAt { get; set; }
    public DateTime? ReleasedAt { get; set; }

    public virtual PtProfile PtProfile { get; set; } = null!;
    public virtual AddonEntitlement? Entitlement { get; set; }
}
