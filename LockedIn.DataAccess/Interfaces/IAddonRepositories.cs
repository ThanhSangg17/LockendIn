using LockedIn.DataAccess.Models;

namespace LockedIn.DataAccess.Interfaces;

public interface IAddonProductRepository : IGenericRepository<AddonProduct>
{
}

public interface IAddonProductPriceRepository : IGenericRepository<AddonProductPrice>
{
}

public interface IAddonOrderRepository : IGenericRepository<AddonOrder>
{
}

public interface IAddonOrderItemRepository : IGenericRepository<AddonOrderItem>
{
}

public interface IAddonPaymentAttemptRepository : IGenericRepository<AddonPaymentAttempt>
{
}

public interface IAddonWebhookLogRepository : IGenericRepository<AddonWebhookLog>
{
}

public interface IAddonEntitlementRepository : IGenericRepository<AddonEntitlement>
{
}

public interface IAddonQuotaReservationRepository : IGenericRepository<AddonQuotaReservation>
{
}

public interface IMealPlanQuotaCounterRepository : IGenericRepository<MealPlanQuotaCounter>
{
}
