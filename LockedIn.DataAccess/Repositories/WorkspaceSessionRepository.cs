using LockedIn.DataAccess.Interfaces;
using LockedIn.DataAccess.Models;

namespace LockedIn.DataAccess.Repositories;

public class WorkspaceSessionRepository : GenericRepository<WorkspaceSession>, IWorkspaceSessionRepository
{
    public WorkspaceSessionRepository(LockedInDbContext context) : base(context)
    {
    }
}
