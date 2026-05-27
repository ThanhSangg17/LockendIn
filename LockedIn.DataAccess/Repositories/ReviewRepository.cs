using LockedIn.DataAccess.Models;
using LockedIn.DataAccess.Interfaces;

namespace LockedIn.DataAccess.Repositories;

public class ReviewRepository : GenericRepository<Review>, IReviewRepository
{
    public ReviewRepository(LockedInDbContext context) : base(context)
    {
    }
}
