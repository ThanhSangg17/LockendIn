using System;
using System.Threading.Tasks;
using LockedIn.DataAccess.Interfaces;

namespace LockedIn.DataAccess.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    ICustomerProfileRepository CustomerProfiles { get; }
    IPtProfileRepository PtProfiles { get; }
    IPtProfileEditRequestRepository PtProfileEditRequests { get; }
    IPtDocumentRepository PtDocuments { get; }
    IPackageRepository Packages { get; }
    IBookingRepository Bookings { get; }
    IPaymentRepository Payments { get; }
    IPaymentWebhookLogRepository PaymentWebhookLogs { get; }
    IWorkspaceRepository Workspaces { get; }
    IConversationRepository Conversations { get; }
    IMealPlanRepository MealPlans { get; }
    IAiUsageLogRepository AiUsageLogs { get; }
    IReviewRepository Reviews { get; }
    IDisputeRepository Disputes { get; }
    IDisputeEvidenceRepository DisputeEvidences { get; }
    ISettlementRepository Settlements { get; }
    INotificationRepository Notifications { get; }
    IAuditLogRepository AuditLogs { get; }
    IWorkspaceSessionRepository WorkspaceSessions { get; }
    ISessionProposalRepository SessionProposals { get; }


    IAddonProductRepository AddonProducts { get; }
    IAddonProductPriceRepository AddonProductPrices { get; }
    IAddonOrderRepository AddonOrders { get; }
    IAddonOrderItemRepository AddonOrderItems { get; }
    IAddonPaymentAttemptRepository AddonPaymentAttempts { get; }
    IAddonWebhookLogRepository AddonWebhookLogs { get; }
    IAddonEntitlementRepository AddonEntitlements { get; }
    IAddonQuotaReservationRepository AddonQuotaReservations { get; }
    IMealPlanQuotaCounterRepository MealPlanQuotaCounters { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task BeginTransactionAsync(System.Data.IsolationLevel isolationLevel);
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
