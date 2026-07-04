using LockedIn.DataAccess.Interfaces;
using LockedIn.DataAccess.Models;

namespace LockedIn.DataAccess.Repositories;

public class PtProfileEditRequestRepository : GenericRepository<PtProfileEditRequest>, IPtProfileEditRequestRepository
{
    public PtProfileEditRequestRepository(LockedInDbContext context) : base(context)
    {
    }
}
