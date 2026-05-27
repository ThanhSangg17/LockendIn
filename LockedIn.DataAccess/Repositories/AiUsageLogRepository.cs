using LockedIn.DataAccess.Models;
using LockedIn.DataAccess.Interfaces;

namespace LockedIn.DataAccess.Repositories;

public class AiUsageLogRepository : GenericRepository<AiUsageLog>, IAiUsageLogRepository
{
    public AiUsageLogRepository(LockedInDbContext context) : base(context)
    {
    }
}
