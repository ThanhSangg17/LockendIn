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

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
