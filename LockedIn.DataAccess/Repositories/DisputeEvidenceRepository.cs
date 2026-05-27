using LockedIn.DataAccess.Models;
using LockedIn.DataAccess.Interfaces;

namespace LockedIn.DataAccess.Repositories;

public class DisputeEvidenceRepository : GenericRepository<DisputeEvidence>, IDisputeEvidenceRepository
{
    public DisputeEvidenceRepository(LockedInDbContext context) : base(context)
    {
    }
}
