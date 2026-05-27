using LockedIn.DataAccess.Models;
using LockedIn.DataAccess.Interfaces;

namespace LockedIn.DataAccess.Repositories;

public class CustomerProfileRepository : GenericRepository<CustomerProfile>, ICustomerProfileRepository
{
    public CustomerProfileRepository(LockedInDbContext context) : base(context)
    {
    }
}
