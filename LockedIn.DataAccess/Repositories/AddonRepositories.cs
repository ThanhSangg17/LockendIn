using LockedIn.DataAccess.Models;
using LockedIn.DataAccess.Interfaces;

namespace LockedIn.DataAccess.Repositories;

public class AddonProductRepository : GenericRepository<AddonProduct>, IAddonProductRepository
{
    public AddonProductRepository(LockedInDbContext context) : base(context) {}
}

public class AddonProductPriceRepository : GenericRepository<AddonProductPrice>, IAddonProductPriceRepository
{
    public AddonProductPriceRepository(LockedInDbContext context) : base(context) {}
}

public class AddonOrderRepository : GenericRepository<AddonOrder>, IAddonOrderRepository
{
    public AddonOrderRepository(LockedInDbContext context) : base(context) {}
}

public class AddonOrderItemRepository : GenericRepository<AddonOrderItem>, IAddonOrderItemRepository
{
    public AddonOrderItemRepository(LockedInDbContext context) : base(context) {}
}

public class AddonPaymentAttemptRepository : GenericRepository<AddonPaymentAttempt>, IAddonPaymentAttemptRepository
{
    public AddonPaymentAttemptRepository(LockedInDbContext context) : base(context) {}
}

public class AddonWebhookLogRepository : GenericRepository<AddonWebhookLog>, IAddonWebhookLogRepository
{
    public AddonWebhookLogRepository(LockedInDbContext context) : base(context) {}
}

public class AddonEntitlementRepository : GenericRepository<AddonEntitlement>, IAddonEntitlementRepository
{
    public AddonEntitlementRepository(LockedInDbContext context) : base(context) {}
}

public class AddonQuotaReservationRepository : GenericRepository<AddonQuotaReservation>, IAddonQuotaReservationRepository
{
    public AddonQuotaReservationRepository(LockedInDbContext context) : base(context) {}
}

public class MealPlanQuotaCounterRepository : GenericRepository<MealPlanQuotaCounter>, IMealPlanQuotaCounterRepository
{
    public MealPlanQuotaCounterRepository(LockedInDbContext context) : base(context) {}
}
