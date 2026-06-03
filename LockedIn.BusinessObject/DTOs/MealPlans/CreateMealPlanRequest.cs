using System;
using System.ComponentModel.DataAnnotations;

namespace LockedIn.BusinessObject.DTOs.MealPlans;

public class CreateMealPlanRequest
{
    public Guid WorkspaceId { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Title is required.")]
    [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
    public string Title { get; set; } = null!;

    [Required(AllowEmptyStrings = false, ErrorMessage = "ContentJson is required.")]
    public string ContentJson { get; set; } = null!;

    public int Source { get; set; }
}
