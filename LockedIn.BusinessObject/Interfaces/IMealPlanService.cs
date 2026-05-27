using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;

namespace LockedIn.BusinessObject.Interfaces;

public interface IMealPlanService
{
    Task<ApiResponse<string>> GenerateMealPlanAsync();
    Task<ApiResponse<string>> CreateMealPlanAsync();
    Task<ApiResponse<string>> GetMealPlansByWorkspaceAsync(Guid workspaceId);
    Task<ApiResponse<string>> GetMealPlanByIdAsync(Guid mealPlanId);
    Task<ApiResponse<string>> ActivateMealPlanAsync(Guid mealPlanId);
    Task<ApiResponse<string>> DeleteMealPlanAsync(Guid mealPlanId);
}
