using LockedIn.DataAccess.Models;
using LockedIn.DataAccess.Interfaces;

namespace LockedIn.DataAccess.Repositories;

public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(LockedInDbContext context) : base(context)
    {
    }
}
