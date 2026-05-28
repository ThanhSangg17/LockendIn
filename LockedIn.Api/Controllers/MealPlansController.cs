using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.BusinessObject.DTOs.MealPlans;

namespace LockedIn.Api.Controllers;

[ApiController]
[Route("api/meal-plans")]
[Authorize]
public class MealPlansController : ControllerBase
{
    private readonly IMealPlanService _service;

    public MealPlansController(IMealPlanService service)
    {
        _service = service;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateMealPlanAsync([FromBody] GenerateMealPlanRequest request)
    {
        var result = await _service.GenerateMealPlanAsync(request);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMealPlanAsync([FromBody] CreateMealPlanRequest request)
    {
        var result = await _service.CreateMealPlanAsync(request);
        return Ok(result);
    }

    [HttpGet("workspace/{workspaceId}")]
    public async Task<IActionResult> GetMealPlansByWorkspaceAsync(Guid workspaceId)
    {
        var result = await _service.GetMealPlansByWorkspaceAsync(workspaceId);
        return Ok(result);
    }

    [HttpGet("{mealPlanId}")]
    public async Task<IActionResult> GetMealPlanByIdAsync(Guid mealPlanId)
    {
        var result = await _service.GetMealPlanByIdAsync(mealPlanId);
        return Ok(result);
    }

    [HttpPatch("{mealPlanId}/activate")]
    public async Task<IActionResult> ActivateMealPlanAsync(Guid mealPlanId)
    {
        var result = await _service.ActivateMealPlanAsync(mealPlanId);
        return Ok(result);
    }

    [HttpDelete("{mealPlanId}")]
    public async Task<IActionResult> DeleteMealPlanAsync(Guid mealPlanId)
    {
        var result = await _service.DeleteMealPlanAsync(mealPlanId);
        return Ok(result);
    }
}
