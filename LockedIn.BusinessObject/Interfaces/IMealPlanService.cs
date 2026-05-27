using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.MealPlans;

namespace LockedIn.BusinessObject.Interfaces;

public interface IMealPlanService
{
    Task<ApiResponse<MealPlanResponse>> GenerateMealPlanAsync(GenerateMealPlanRequest request);
    Task<ApiResponse<MealPlanResponse>> CreateMealPlanAsync(CreateMealPlanRequest request);
    Task<ApiResponse<IReadOnlyList<MealPlanResponse>>> GetMealPlansByWorkspaceAsync(Guid workspaceId);
    Task<ApiResponse<MealPlanResponse>> GetMealPlanByIdAsync(Guid mealPlanId);
    Task<ApiResponse<MealPlanResponse>> ActivateMealPlanAsync(Guid mealPlanId);
    Task<ApiResponse<string>> DeleteMealPlanAsync(Guid mealPlanId);
}
