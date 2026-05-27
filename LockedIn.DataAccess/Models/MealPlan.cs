using System;
using System.Collections.Generic;

namespace LockedIn.DataAccess.Models;

public partial class MealPlan
{
    public Guid Id { get; set; }

    public Guid WorkspaceId { get; set; }

    public Guid CreatedByPtId { get; set; }

    public string Title { get; set; } = null!;

    public string ContentJson { get; set; } = null!;

    public int Source { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual PtProfile CreatedByPt { get; set; } = null!;

    public virtual User? DeletedByNavigation { get; set; }

    public virtual Workspace Workspace { get; set; } = null!;
}
