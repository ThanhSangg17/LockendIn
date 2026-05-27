using System;

namespace LockedIn.BusinessObject.DTOs.MealPlans;

public class GenerateMealPlanRequest
{
    public Guid WorkspaceId { get; set; }
    public string Goal { get; set; } = null!;
    public string? Preference { get; set; }
    public string? AllergyNote { get; set; }
}
