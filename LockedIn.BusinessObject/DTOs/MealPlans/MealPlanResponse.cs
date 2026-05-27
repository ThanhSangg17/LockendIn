using System;

namespace LockedIn.BusinessObject.DTOs.MealPlans;

public class MealPlanResponse
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid CreatedByPtId { get; set; }
    public string Title { get; set; } = null!;
    public string ContentJson { get; set; } = null!;
    public int Source { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
