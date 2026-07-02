using System;
using System.Collections.Generic;

namespace LockedIn.DataAccess.Models;

public partial class Workspace
{
    public Guid Id { get; set; }

    public Guid BookingId { get; set; }

    public Guid CustomerId { get; set; }

    public Guid PtProfileId { get; set; }

    public int Status { get; set; }

    public string? CourseNote { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual CustomerProfile Customer { get; set; } = null!;

    public virtual ICollection<MealPlan> MealPlans { get; set; } = new List<MealPlan>();

    public virtual ICollection<WorkspaceSession> WorkspaceSessions { get; set; } = new List<WorkspaceSession>();

    public virtual PtProfile PtProfile { get; set; } = null!;
}
