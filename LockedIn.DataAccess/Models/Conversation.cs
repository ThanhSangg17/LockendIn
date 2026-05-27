using System;
using System.Collections.Generic;

namespace LockedIn.DataAccess.Models;

public partial class Conversation
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }

    public Guid CustomerId { get; set; }

    public Guid PtProfileId { get; set; }

    public string FirebaseConversationId { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual CustomerProfile Customer { get; set; } = null!;

    public virtual PtProfile PtProfile { get; set; } = null!;
}
