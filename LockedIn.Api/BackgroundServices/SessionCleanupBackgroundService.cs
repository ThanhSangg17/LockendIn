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

public class SessionCleanupBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SessionCleanupBackgroundService> _logger;
    private readonly IConfiguration _configuration;

    public SessionCleanupBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<SessionCleanupBackgroundService> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int intervalMinutes = _configuration.GetValue<int>("SessionScheduling:CleanupIntervalMinutes", 5);
        _logger.LogInformation("SessionCleanupBackgroundService running. Interval: {IntervalMinutes} minutes.", intervalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessMissedSessionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SessionCleanupBackgroundService.");
            }

            await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
        }
    }

    private async Task ProcessMissedSessionsAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var now = DateTime.UtcNow;

        var expiredSessionIds = await uow.WorkspaceSessions.Query()
            .Where(s => s.Status == (int)WorkspaceSessionStatus.Scheduled && s.ScheduledEnd.HasValue && s.ScheduledEnd.Value < now)
            .Select(s => s.Id)
            .Take(100)
            .ToListAsync(stoppingToken);

        if (!expiredSessionIds.Any())
        {
            return;
        }

        _logger.LogInformation("Found {Count} scheduled sessions past ScheduledEnd to process as Missed.", expiredSessionIds.Count);

        foreach (var sessionId in expiredSessionIds)
        {
            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                using var innerScope = _scopeFactory.CreateScope();
                var innerUow = innerScope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                await innerUow.BeginTransactionAsync();
                try
                {
                    var session = await innerUow.WorkspaceSessions.Query()
                        .FirstOrDefaultAsync(s => s.Id == sessionId, stoppingToken);

                    if (session != null && session.Status == (int)WorkspaceSessionStatus.Scheduled && session.ScheduledEnd.HasValue && session.ScheduledEnd.Value < now)
                    {
                        session.Status = (int)WorkspaceSessionStatus.Missed;
                        innerUow.WorkspaceSessions.Update(session);

                        await innerUow.SaveChangesAsync();
                        await innerUow.CommitTransactionAsync();
                        _logger.LogInformation("Successfully updated WorkspaceSession {SessionId} (Session #{SessionNumber}) to Missed.", session.Id, session.SessionNumber);
                    }
                    else
                    {
                        await innerUow.RollbackTransactionAsync();
                    }
                }
                catch (Exception ex)
                {
                    await innerUow.RollbackTransactionAsync();
                    _logger.LogError(ex, "Failed to mark WorkspaceSession {SessionId} as Missed.", sessionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating scope for marking WorkspaceSession {SessionId} as Missed.", sessionId);
            }
        }
    }
}
