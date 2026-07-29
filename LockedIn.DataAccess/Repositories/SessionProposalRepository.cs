using LockedIn.DataAccess.Interfaces;
using LockedIn.DataAccess.Models;

namespace LockedIn.DataAccess.Repositories;

public class SessionProposalRepository : GenericRepository<SessionProposal>, ISessionProposalRepository
{
    public SessionProposalRepository(LockedInDbContext context) : base(context)
    {
    }
}
