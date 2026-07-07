using System;

namespace LockedIn.DataAccess.Models;

public partial class MealPlanQuotaCounter
{
    public Guid Id { get; set; }
    public Guid PtProfileId { get; set; }
    public DateOnly QuotaDate { get; set; }
    public int ConsumedCount { get; set; }
    public int ReservedCount { get; set; }
    public int DailyLimit { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual PtProfile PtProfile { get; set; } = null!;
}
