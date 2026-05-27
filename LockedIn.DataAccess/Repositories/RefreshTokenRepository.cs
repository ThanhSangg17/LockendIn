using LockedIn.DataAccess.Models;
using LockedIn.DataAccess.Interfaces;

namespace LockedIn.DataAccess.Repositories;

public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(LockedInDbContext context) : base(context)
    {
    }
}
