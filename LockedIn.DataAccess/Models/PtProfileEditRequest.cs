using System;
using System.Collections.Generic;

namespace LockedIn.DataAccess.Models;

public partial class PtProfileEditRequest
{
    public Guid Id { get; set; }

    public Guid PtProfileId { get; set; }

    public string? CurrentBio { get; set; }

    public string? CurrentSpecialization { get; set; }

    public int CurrentExperienceYears { get; set; }

    public string? RequestedBio { get; set; }

    public string? RequestedSpecialization { get; set; }

    public int RequestedExperienceYears { get; set; }

    public int Status { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime RequestedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public Guid? ReviewedByAdminId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual PtProfile PtProfile { get; set; } = null!;

    public virtual User? ReviewedByAdmin { get; set; }
}
