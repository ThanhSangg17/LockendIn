using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.BusinessObject.Enums;

namespace LockedIn.Api.BackgroundServices;

public class QuotaCleanupBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<QuotaCleanupBackgroundService> _logger;
    private readonly IConfiguration _configuration;

    public QuotaCleanupBackgroundService(IServiceScopeFactory scopeFactory, ILogger<QuotaCleanupBackgroundService> logger, IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int intervalMinutes = _configuration.GetValue<int>("AIQuota:MealPlanReservationCleanupIntervalMinutes", 5);
        _logger.LogInformation("QuotaCleanupBackgroundService running. Interval: {IntervalMinutes} minutes.", intervalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupStaleReservationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in QuotaCleanupBackgroundService.");
            }

            await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
        }
    }

    private async Task CleanupStaleReservationsAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting cleanup of stale AddonQuotaReservations...");
        
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var staleIds = await uow.AddonQuotaReservations.Query()
            .Where(r => r.Status == (int)AddonQuotaReservationStatus.Pending && r.ExpiresAt < DateTime.UtcNow)
            .Select(r => r.Id)
            .Take(100)
            .ToListAsync(stoppingToken);

        if (!staleIds.Any())
        {
            return;
        }

        _logger.LogInformation("Found {Count} stale reservations to cleanup.", staleIds.Count);

        foreach (var id in staleIds)
        {
            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                using var innerScope = _scopeFactory.CreateScope();
                var innerUow = innerScope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                await innerUow.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
                try
                {
                    var reservation = await innerUow.AddonQuotaReservations.Query()
                        .Include(r => r.Entitlement)
                        .FirstOrDefaultAsync(r => r.Id == id, stoppingToken);

                    if (reservation != null && reservation.Status == (int)AddonQuotaReservationStatus.Pending && reservation.ExpiresAt < DateTime.UtcNow)
                    {
                        if (reservation.ReservationType == "FreeQuota")
                        {
                            var counter = await innerUow.MealPlanQuotaCounters.Query()
                                .FirstOrDefaultAsync(c => c.PtProfileId == reservation.PtProfileId && c.QuotaDate == reservation.QuotaDate, stoppingToken);
                            
                            if (counter != null && counter.ReservedCount > 0)
                            {
                                counter.ReservedCount--;
                            }
                        }
                        else if (reservation.ReservationType == "Credit" && reservation.EntitlementId.HasValue)
                        {
                            var entitlement = reservation.Entitlement;
                            if (entitlement != null)
                            {
                                entitlement.QuantityRemaining++;
                                if (entitlement.QuantityRemaining > entitlement.QuantityGranted)
                                {
                                    entitlement.QuantityRemaining = entitlement.QuantityGranted;
                                }
                                // Optional logic to flip status from Exhausted back to Active if needed, but per rule:
                                // "chỉ đổi về Active nếu entitlement chưa expired và source status semantic cho phép"
                                if (entitlement.Status == (int)AddonEntitlementStatus.Exhausted && (entitlement.ExpiresAt == null || entitlement.ExpiresAt > DateTime.UtcNow))
                                {
                                    entitlement.Status = (int)AddonEntitlementStatus.Active;
                                }
                            }
                        }

                        reservation.Status = (int)AddonQuotaReservationStatus.Released;
                        reservation.ReleasedAt = DateTime.UtcNow;

                        await innerUow.SaveChangesAsync();
                        await innerUow.CommitTransactionAsync();
                        _logger.LogInformation("Successfully cleaned up stale reservation {ReservationId}.", id);
                    }
                    else
                    {
                        await innerUow.RollbackTransactionAsync();
                    }
                }
                catch (Exception ex)
                {
                    await innerUow.RollbackTransactionAsync();
                    _logger.LogError(ex, "Failed to cleanup stale reservation {ReservationId}.", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating scope for reservation {ReservationId}.", id);
            }
        }
    }
}
