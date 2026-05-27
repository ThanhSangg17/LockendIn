using LockedIn.DataAccess.Models;
using LockedIn.DataAccess.Interfaces;

namespace LockedIn.DataAccess.Repositories;

public class PtProfileRepository : GenericRepository<PtProfile>, IPtProfileRepository
{
    public PtProfileRepository(LockedInDbContext context) : base(context)
    {
    }
}
