using LockedIn.DataAccess.Models;
using LockedIn.DataAccess.Interfaces;

namespace LockedIn.DataAccess.Repositories;

public class PtDocumentRepository : GenericRepository<PtDocument>, IPtDocumentRepository
{
    public PtDocumentRepository(LockedInDbContext context) : base(context)
    {
    }
}
