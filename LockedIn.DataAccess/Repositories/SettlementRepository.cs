using LockedIn.DataAccess.Models;
using LockedIn.DataAccess.Interfaces;

namespace LockedIn.DataAccess.Repositories;

public class SettlementRepository : GenericRepository<Settlement>, ISettlementRepository
{
    public SettlementRepository(LockedInDbContext context) : base(context)
    {
    }
}
