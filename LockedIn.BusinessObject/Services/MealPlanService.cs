using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.BusinessObject.DTOs.MealPlans;

namespace LockedIn.BusinessObject.Services;

public class MealPlanService : IMealPlanService
{
    private readonly IUnitOfWork _unitOfWork;

    public MealPlanService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<MealPlanResponse>> GenerateMealPlanAsync(GenerateMealPlanRequest request)
    {
        return await Task.FromResult(ApiResponse<MealPlanResponse>.Ok(new MealPlanResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<MealPlanResponse>> CreateMealPlanAsync(CreateMealPlanRequest request)
    {
        return await Task.FromResult(ApiResponse<MealPlanResponse>.Ok(new MealPlanResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<IReadOnlyList<MealPlanResponse>>> GetMealPlansByWorkspaceAsync(Guid workspaceId)
    {
        return await Task.FromResult(ApiResponse<IReadOnlyList<MealPlanResponse>>.Ok(new List<MealPlanResponse>(), "Not implemented yet"));
    }

    public async Task<ApiResponse<MealPlanResponse>> GetMealPlanByIdAsync(Guid mealPlanId)
    {
        return await Task.FromResult(ApiResponse<MealPlanResponse>.Ok(new MealPlanResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<MealPlanResponse>> ActivateMealPlanAsync(Guid mealPlanId)
    {
        return await Task.FromResult(ApiResponse<MealPlanResponse>.Ok(new MealPlanResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<string>> DeleteMealPlanAsync(Guid mealPlanId)
    {
        return await Task.FromResult(ApiResponse<string>.Ok(string.Empty, "Not implemented yet"));
    }
}
