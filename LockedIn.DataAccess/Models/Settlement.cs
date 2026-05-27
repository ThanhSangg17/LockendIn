using System;
using System.Collections.Generic;

namespace LockedIn.DataAccess.Models;

public partial class Settlement
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }

    public Guid PtProfileId { get; set; }

    public decimal GrossAmount { get; set; }

    public decimal PlatformFee { get; set; }

    public decimal NetAmount { get; set; }

    public int Status { get; set; }

    public DateTime? SettledAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual PtProfile PtProfile { get; set; } = null!;
}
