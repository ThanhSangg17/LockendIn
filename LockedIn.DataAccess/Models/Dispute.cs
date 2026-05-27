using System;
using System.Collections.Generic;

namespace LockedIn.DataAccess.Models;

public partial class Dispute
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }

    public Guid CustomerId { get; set; }

    public Guid PtProfileId { get; set; }

    public string Reason { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int Status { get; set; }

    public string? ResolutionNote { get; set; }

    public Guid? ResolvedByAdminId { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual CustomerProfile Customer { get; set; } = null!;

    public virtual ICollection<DisputeEvidence> DisputeEvidences { get; set; } = new List<DisputeEvidence>();

    public virtual PtProfile PtProfile { get; set; } = null!;

    public virtual User? ResolvedByAdmin { get; set; }
}
