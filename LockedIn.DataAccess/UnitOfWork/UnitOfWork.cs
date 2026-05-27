using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using LockedIn.DataAccess.Interfaces;
using LockedIn.DataAccess.Models;
using LockedIn.DataAccess.Repositories;

namespace LockedIn.DataAccess.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly LockedInDbContext _context;
    private IDbContextTransaction? _transaction;

    public IUserRepository Users { get; }
    public IRefreshTokenRepository RefreshTokens { get; }
    public ICustomerProfileRepository CustomerProfiles { get; }
    public IPtProfileRepository PtProfiles { get; }
    public IPtDocumentRepository PtDocuments { get; }
    public IPackageRepository Packages { get; }
    public IBookingRepository Bookings { get; }
    public IPaymentRepository Payments { get; }
    public IPaymentWebhookLogRepository PaymentWebhookLogs { get; }
    public IWorkspaceRepository Workspaces { get; }
    public IConversationRepository Conversations { get; }
    public IMealPlanRepository MealPlans { get; }
    public IAiUsageLogRepository AiUsageLogs { get; }
    public IReviewRepository Reviews { get; }
    public IDisputeRepository Disputes { get; }
    public IDisputeEvidenceRepository DisputeEvidences { get; }
    public ISettlementRepository Settlements { get; }
    public INotificationRepository Notifications { get; }
    public IAuditLogRepository AuditLogs { get; }

    public UnitOfWork(LockedInDbContext context)
    {
        _context = context;
        Users = new UserRepository(_context);
        RefreshTokens = new RefreshTokenRepository(_context);
        CustomerProfiles = new CustomerProfileRepository(_context);
        PtProfiles = new PtProfileRepository(_context);
        PtDocuments = new PtDocumentRepository(_context);
        Packages = new PackageRepository(_context);
        Bookings = new BookingRepository(_context);
        Payments = new PaymentRepository(_context);
        PaymentWebhookLogs = new PaymentWebhookLogRepository(_context);
        Workspaces = new WorkspaceRepository(_context);
        Conversations = new ConversationRepository(_context);
        MealPlans = new MealPlanRepository(_context);
        AiUsageLogs = new AiUsageLogRepository(_context);
        Reviews = new ReviewRepository(_context);
        Disputes = new DisputeRepository(_context);
        DisputeEvidences = new DisputeEvidenceRepository(_context);
        Settlements = new SettlementRepository(_context);
        Notifications = new NotificationRepository(_context);
        AuditLogs = new AuditLogRepository(_context);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        if (_transaction == null)
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
