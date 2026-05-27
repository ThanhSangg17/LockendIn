using System;
using System.Collections.Generic;

namespace LockedIn.DataAccess.Models;

public partial class Package
{
    public Guid Id { get; set; }

    public Guid PtProfileId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int SessionCount { get; set; }

    public decimal Price { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual User? DeletedByNavigation { get; set; }

    public virtual PtProfile PtProfile { get; set; } = null!;
}
