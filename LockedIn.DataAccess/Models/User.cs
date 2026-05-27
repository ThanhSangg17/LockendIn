using System;
using System.Collections.Generic;

namespace LockedIn.DataAccess.Models;

public partial class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? AvatarUrl { get; set; }

    public int Role { get; set; }

    public int Status { get; set; }

    public bool EmailVerified { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<CustomerProfile> CustomerProfileDeletedByNavigations { get; set; } = new List<CustomerProfile>();

    public virtual CustomerProfile? CustomerProfileUser { get; set; }

    public virtual User? DeletedByNavigation { get; set; }

    public virtual ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();

    public virtual ICollection<User> InverseDeletedByNavigation { get; set; } = new List<User>();

    public virtual ICollection<MealPlan> MealPlans { get; set; } = new List<MealPlan>();

    public virtual ICollection<Notification> NotificationDeletedByNavigations { get; set; } = new List<Notification>();

    public virtual ICollection<Notification> NotificationUsers { get; set; } = new List<Notification>();

    public virtual ICollection<Package> Packages { get; set; } = new List<Package>();

    public virtual ICollection<PtProfile> PtProfileDeletedByNavigations { get; set; } = new List<PtProfile>();

    public virtual PtProfile? PtProfileUser { get; set; }

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
