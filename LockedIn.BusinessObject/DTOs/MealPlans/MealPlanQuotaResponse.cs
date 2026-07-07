using System;

namespace LockedIn.BusinessObject.DTOs.MealPlans;

public class MealPlanQuotaResponse
{
    public int DailyFreeQuota { get; set; }
    public int FreeQuotaUsedToday { get; set; }
    public int FreeQuotaReservedNow { get; set; }
    public int FreeQuotaRemaining { get; set; }
    public int PaidMealPlanCreditRemaining { get; set; }
    public int TotalGenerationsRemaining { get; set; }
    public DateOnly UsageDateUtc { get; set; }
    public DateTime ServerUtcNow { get; set; }
}
