using LockedIn.DataAccess.Models;
using LockedIn.DataAccess.Interfaces;

namespace LockedIn.DataAccess.Repositories;

public class PaymentWebhookLogRepository : GenericRepository<PaymentWebhookLog>, IPaymentWebhookLogRepository
{
    public PaymentWebhookLogRepository(LockedInDbContext context) : base(context)
    {
    }
}
