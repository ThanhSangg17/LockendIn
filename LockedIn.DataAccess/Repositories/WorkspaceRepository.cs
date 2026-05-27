using LockedIn.DataAccess.Models;
using LockedIn.DataAccess.Interfaces;

namespace LockedIn.DataAccess.Repositories;

public class WorkspaceRepository : GenericRepository<Workspace>, IWorkspaceRepository
{
    public WorkspaceRepository(LockedInDbContext context) : base(context)
    {
    }
}
