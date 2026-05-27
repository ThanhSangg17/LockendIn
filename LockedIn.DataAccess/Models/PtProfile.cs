using System;
using System.Collections.Generic;

namespace LockedIn.DataAccess.Models;

public partial class PtProfile
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string? Bio { get; set; }

    public string? Specialization { get; set; }

    public int ExperienceYears { get; set; }

    public int VerificationStatus { get; set; }

    public decimal AverageRating { get; set; }

    public int TotalReviews { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AiUsageLog> AiUsageLogs { get; set; } = new List<AiUsageLog>();

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();

    public virtual User? DeletedByNavigation { get; set; }

    public virtual ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();

    public virtual ICollection<MealPlan> MealPlans { get; set; } = new List<MealPlan>();

    public virtual ICollection<Package> Packages { get; set; } = new List<Package>();

    public virtual ICollection<PtDocument> PtDocuments { get; set; } = new List<PtDocument>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Settlement> Settlements { get; set; } = new List<Settlement>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<Workspace> Workspaces { get; set; } = new List<Workspace>();
}
