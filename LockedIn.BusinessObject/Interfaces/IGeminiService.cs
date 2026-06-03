using System.Threading.Tasks;
using LockedIn.BusinessObject.DTOs.MealPlans;

namespace LockedIn.BusinessObject.Interfaces;

public interface IGeminiService
{
    Task<(string JsonContent, int TokensUsed)> GenerateMealPlanJsonAsync(GenerateMealPlanRequest request);
}
