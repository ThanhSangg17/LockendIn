using System;

namespace LockedIn.BusinessObject.DTOs.MealPlans;

public class CreateMealPlanRequest
{
    public Guid WorkspaceId { get; set; }
    public string Title { get; set; } = null!;
    public string ContentJson { get; set; } = null!;
    public int Source { get; set; }
}
