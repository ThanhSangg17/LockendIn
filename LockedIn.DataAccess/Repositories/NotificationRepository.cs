using LockedIn.DataAccess.Models;
using LockedIn.DataAccess.Interfaces;

namespace LockedIn.DataAccess.Repositories;

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(LockedInDbContext context) : base(context)
    {
    }
}
