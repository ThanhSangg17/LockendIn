using LockedIn.DataAccess.Models;
using LockedIn.DataAccess.Interfaces;

namespace LockedIn.DataAccess.Repositories;

public class MealPlanRepository : GenericRepository<MealPlan>, IMealPlanRepository
{
    public MealPlanRepository(LockedInDbContext context) : base(context)
    {
    }
}
