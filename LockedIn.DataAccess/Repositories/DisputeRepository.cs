using LockedIn.DataAccess.Models;
using LockedIn.DataAccess.Interfaces;

namespace LockedIn.DataAccess.Repositories;

public class DisputeRepository : GenericRepository<Dispute>, IDisputeRepository
{
    public DisputeRepository(LockedInDbContext context) : base(context)
    {
    }
}
