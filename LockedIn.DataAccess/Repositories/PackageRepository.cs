using LockedIn.DataAccess.Models;
using LockedIn.DataAccess.Interfaces;

namespace LockedIn.DataAccess.Repositories;

public class PackageRepository : GenericRepository<Package>, IPackageRepository
{
    public PackageRepository(LockedInDbContext context) : base(context)
    {
    }
}
