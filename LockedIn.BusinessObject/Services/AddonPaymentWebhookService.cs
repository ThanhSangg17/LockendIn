using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.DataAccess.Models;
using LockedIn.BusinessObject.Enums;
using PayOS.Models.Webhooks;

namespace LockedIn.BusinessObject.Services;

public class AddonPaymentWebhookService : IAddonPaymentWebhookService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddonPaymentWebhookService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public AddonPaymentWebhookService(
        IUnitOfWork unitOfWork,
        ILogger<AddonPaymentWebhookService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task<ApiResponse<string>> HandleWebhookAsync(WebhookData verifiedData, string requestCode, string rawPayload, DateTime receivedAt)
    {
        string orderCodeStr = verifiedData.OrderCode.ToString();
        _logger.LogInformation("AddonPaymentWebhookService: Processing webhook for OrderCode {OrderCode}", orderCodeStr);

        ApiResponse<string>? webhookResult = null;
        DateTime? processedAt = null;

        try
        {
            await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            var attempt = await _unitOfWork.AddonPaymentAttempts.Query()
                .Include(a => a.Order)
                    .ThenInclude(o => o.AddonOrderItems)
                .FirstOrDefaultAsync(a => a.OrderCode == orderCodeStr);

            if (attempt == null)
            {
                await _unitOfWork.RollbackTransactionAsync();
                webhookResult = ApiResponse<string>.Ok("Attempt not found", "Attempt not found");
                return webhookResult;
            }

            var order = attempt.Order;

            // 1. Check idempotency
            if (attempt.Status == (int)AddonPaymentAttemptStatus.Success)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogInformation("AddonPaymentWebhookService: Webhook already processed (Attempt Success) for OrderCode {OrderCode}", orderCodeStr);
                processedAt = DateTime.UtcNow;
                webhookResult = ApiResponse<string>.Ok("Webhook already processed", "Webhook already processed");
                return webhookResult;
            }

            if (order.Status == (int)AddonOrderStatus.Paid)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogInformation("AddonPaymentWebhookService: Order already Paid by another attempt for OrderCode {OrderCode}", orderCodeStr);
                processedAt = DateTime.UtcNow;
                webhookResult = ApiResponse<string>.Ok("Webhook already processed", "Webhook already processed");
                return webhookResult;
            }

            if (order.Status == (int)AddonOrderStatus.Cancelled)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogWarning("AddonPaymentWebhookService: Order Cancelled for OrderCode {OrderCode}", orderCodeStr);
                webhookResult = ApiResponse<string>.Fail("Order cancelled");
                return webhookResult;
            }

            // 2. Validate amount mismatch
            decimal providerAmount = (decimal)verifiedData.Amount;
            if (providerAmount != attempt.Amount || providerAmount != order.TotalAmount)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError("AddonPaymentWebhookService: Amount mismatch for OrderCode {OrderCode}. Payload: {PayloadAmount}, Attempt: {AttemptAmount}, Order: {OrderAmount}", 
                    orderCodeStr, providerAmount, attempt.Amount, order.TotalAmount);
                webhookResult = ApiResponse<string>.Fail("Amount mismatch");
                return webhookResult;
            }

            if (requestCode != "00")
            {
                if (attempt.Status == (int)AddonPaymentAttemptStatus.Pending && order.Status == (int)AddonOrderStatus.Pending)
                {
                    attempt.Status = (int)AddonPaymentAttemptStatus.Failed;
                    _unitOfWork.AddonPaymentAttempts.Update(attempt);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();
                    _logger.LogInformation("AddonPaymentWebhookService: Attempt marked Failed for OrderCode {OrderCode} due to non-success code {Code}", orderCodeStr, requestCode);
                }
                else
                {
                    await _unitOfWork.RollbackTransactionAsync();
                }
                webhookResult = ApiResponse<string>.Ok("Webhook processed successfully (Failed)", "Webhook processed successfully (Failed)");
                return webhookResult;
            }

            // 3. Process Success
            DateTime successTime = DateTime.UtcNow;

            attempt.Status = (int)AddonPaymentAttemptStatus.Success;
            attempt.PaidAt = successTime;
            attempt.ProviderTransactionId = string.IsNullOrWhiteSpace(verifiedData.Reference) ? null : verifiedData.Reference;
            _unitOfWork.AddonPaymentAttempts.Update(attempt);

            order.Status = (int)AddonOrderStatus.Paid;
            order.PaidAt = successTime;
            order.UpdatedAt = successTime;
            _unitOfWork.AddonOrders.Update(order);

            // 4. Mark other pending attempts as Expired
            var otherAttempts = await _unitOfWork.AddonPaymentAttempts.Query()
                .Where(a => a.OrderId == order.Id && a.Id != attempt.Id && a.Status == (int)AddonPaymentAttemptStatus.Pending)
                .ToListAsync();

            foreach (var otherAttempt in otherAttempts)
            {
                otherAttempt.Status = (int)AddonPaymentAttemptStatus.Expired;
                _unitOfWork.AddonPaymentAttempts.Update(otherAttempt);
            }

            // 5. Grant Entitlements
            foreach (var item in order.AddonOrderItems)
            {
                var entitlement = new AddonEntitlement
                {
                    Id = Guid.NewGuid(),
                    PtProfileId = order.PtProfileId,
                    OrderId = order.Id,
                    OrderItemId = item.Id,
                    ProductCode = item.ProductCode,
                    FulfillmentType = item.FulfillmentTypeSnapshot,
                    Status = (int)AddonEntitlementStatus.Active,
                    CreatedAt = successTime,
                    UpdatedAt = null
                };

                if (item.FulfillmentTypeSnapshot == "Credit")
                {
                    if (item.GrantQuantitySnapshot == null || item.GrantQuantitySnapshot <= 0 || item.Quantity <= 0)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        _logger.LogError("AddonPaymentWebhookService: Invalid Credit snapshot for OrderCode {OrderCode}, OrderItem {ItemId}", orderCodeStr, item.Id);
                        webhookResult = ApiResponse<string>.Fail("Invalid fulfillment snapshot");
                        return webhookResult;
                    }
                    int granted = item.GrantQuantitySnapshot.Value * item.Quantity;
                    entitlement.QuantityGranted = granted;
                    entitlement.QuantityRemaining = granted;
                    entitlement.ActivatedAt = null;
                    entitlement.ExpiresAt = null;
                }
                else if (item.FulfillmentTypeSnapshot == "TimeBasedEntitlement")
                {
                    if (item.DurationDaysSnapshot == null || item.DurationDaysSnapshot <= 0)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        _logger.LogError("AddonPaymentWebhookService: Invalid TimeBased snapshot for OrderCode {OrderCode}, OrderItem {ItemId}", orderCodeStr, item.Id);
                        webhookResult = ApiResponse<string>.Fail("Invalid fulfillment snapshot");
                        return webhookResult;
                    }
                    entitlement.QuantityGranted = 0;
                    entitlement.QuantityRemaining = 0;
                    entitlement.ActivatedAt = successTime;
                    entitlement.ExpiresAt = successTime.AddDays(item.DurationDaysSnapshot.Value);
                }
                else
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError("AddonPaymentWebhookService: Unknown fulfillment type {Type} for OrderCode {OrderCode}", item.FulfillmentTypeSnapshot, orderCodeStr);
                    webhookResult = ApiResponse<string>.Fail("Invalid fulfillment snapshot");
                    return webhookResult;
                }

                await _unitOfWork.AddonEntitlements.AddAsync(entitlement);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            processedAt = successTime;
            webhookResult = ApiResponse<string>.Ok("Webhook processed successfully", "Webhook processed successfully");

            _logger.LogInformation("AddonPaymentWebhookService: Core transaction committed for OrderCode {OrderCode}", orderCodeStr);
            
            // Post-commit tasks
            await ExecutePostCommitTasksAsync(attempt, order, successTime);

            return webhookResult;
        }
        catch (DbUpdateException ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "AddonPaymentWebhookService: Concurrency conflict for OrderCode {OrderCode}", orderCodeStr);

            // Defect 1 Fix: Re-read state in a clean scope to recover gracefully if already processed
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var cleanUnitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var reReadAttempt = await cleanUnitOfWork.AddonPaymentAttempts.Query()
                    .Include(a => a.Order)
                    .FirstOrDefaultAsync(a => a.OrderCode == orderCodeStr);
                
                if (reReadAttempt != null && (reReadAttempt.Status == (int)AddonPaymentAttemptStatus.Success || reReadAttempt.Order.Status == (int)AddonOrderStatus.Paid))
                {
                    _logger.LogInformation("AddonPaymentWebhookService: Recovered from concurrency conflict. Webhook already processed by another thread for OrderCode {OrderCode}", orderCodeStr);
                    processedAt = DateTime.UtcNow;
                    webhookResult = ApiResponse<string>.Ok("Webhook already processed", "Webhook already processed");
                    return webhookResult;
                }
            }
            catch (Exception reReadEx)
            {
                _logger.LogError(reReadEx, "AddonPaymentWebhookService: Failed to re-read state after concurrency conflict for OrderCode {OrderCode}", orderCodeStr);
            }

            webhookResult = ApiResponse<string>.Fail("Concurrency conflict");
            return webhookResult;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "AddonPaymentWebhookService: Error processing webhook for OrderCode {OrderCode}", orderCodeStr);
            webhookResult = ApiResponse<string>.Fail($"Error processing webhook: {ex.Message}");
            return webhookResult;
        }
        finally
        {
            // Best-effort webhook log
            await SaveAddonWebhookLogAsync(orderCodeStr, verifiedData, rawPayload, receivedAt, processedAt, webhookResult);
        }
    }

    private async Task SaveAddonWebhookLogAsync(string orderCodeStr, WebhookData verifiedData, string rawPayload, DateTime receivedAt, DateTime? processedAt, ApiResponse<string>? result)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            Guid? attemptId = null;
            try
            {
                var attempt = await unitOfWork.AddonPaymentAttempts.Query().FirstOrDefaultAsync(a => a.OrderCode == orderCodeStr);
                attemptId = attempt?.Id;
            }
            catch { }

            var log = new AddonWebhookLog
            {
                Id = Guid.NewGuid(),
                AttemptId = attemptId,
                Provider = "PayOS",
                EventType = "PaymentSuccess",
                EventId = orderCodeStr,
                RawPayload = rawPayload,
                IsValidSignature = true,
                ReceivedAt = receivedAt,
                ProcessedAt = processedAt
            };

            await unitOfWork.AddonWebhookLogs.AddAsync(log);
            await unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddonPaymentWebhookService: Failed to save AddonWebhookLog for OrderCode {OrderCode}", orderCodeStr);
        }
    }

    private async Task ExecutePostCommitTasksAsync(AddonPaymentAttempt attempt, AddonOrder order, DateTime successTime)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<AddonPaymentWebhookService>>();

            // Notification
            var ptProfile = await unitOfWork.PtProfiles.Query().FirstOrDefaultAsync(p => p.Id == order.PtProfileId);
            if (ptProfile != null)
            {
                string content = "Thanh toán thành công. Dịch vụ đã được kích hoạt.";
                var firstItem = order.AddonOrderItems.FirstOrDefault();
                if (firstItem != null)
                {
                    if (firstItem.FulfillmentTypeSnapshot == "Credit" && firstItem.GrantQuantitySnapshot.HasValue)
                    {
                        int granted = firstItem.GrantQuantitySnapshot.Value * firstItem.Quantity;
                        content = $"Thanh toán thành công. Bạn đã nhận được {granted} Meal Plan Credit.";
                    }
                    else if (firstItem.FulfillmentTypeSnapshot == "TimeBasedEntitlement" && firstItem.DurationDaysSnapshot.HasValue)
                    {
                        var expiresAt = successTime.AddDays(firstItem.DurationDaysSnapshot.Value);
                        content = $"Thanh toán thành công. Gói dịch vụ của bạn đã được kích hoạt đến {expiresAt:dd/MM/yyyy HH:mm}.";
                    }
                }

                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = ptProfile.UserId,
                    Title = "Thanh toán Add-on thành công",
                    Content = content,
                    Type = (int)NotificationType.Payment,
                    IsRead = false,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                };
                await unitOfWork.Notifications.AddAsync(notification);
            }

            // AuditLog - CompleteAddonPayment
            var baseMetadata = new
            {
                OrderId = order.Id,
                PtProfileId = order.PtProfileId,
                PaymentAttemptId = attempt.Id,
                OrderCode = attempt.OrderCode,
                ProviderTransactionId = attempt.ProviderTransactionId,
                TotalAmount = order.TotalAmount,
                Currency = order.Currency
            };

            var paymentAudit = new AuditLog
            {
                Id = Guid.NewGuid(),
                ActorUserId = ptProfile?.UserId ?? Guid.Empty,
                Action = "CompleteAddonPayment",
                EntityName = "AddonOrder",
                EntityId = order.Id,
                MetadataJson = System.Text.Json.JsonSerializer.Serialize(baseMetadata),
                CreatedAt = DateTime.UtcNow
            };
            await unitOfWork.AuditLogs.AddAsync(paymentAudit);

            // AuditLog - GrantAddonEntitlement
            foreach (var item in order.AddonOrderItems)
            {
                int? quantityGranted = item.FulfillmentTypeSnapshot == "Credit" ? item.GrantQuantitySnapshot * item.Quantity : null;
                DateTime? activatedAt = item.FulfillmentTypeSnapshot == "TimeBasedEntitlement" ? successTime : null;
                DateTime? expiresAt = item.FulfillmentTypeSnapshot == "TimeBasedEntitlement" ? successTime.AddDays(item.DurationDaysSnapshot ?? 0) : null;

                var grantMetadata = new
                {
                    OrderId = order.Id,
                    PtProfileId = order.PtProfileId,
                    PaymentAttemptId = attempt.Id,
                    OrderCode = attempt.OrderCode,
                    ProductId = item.ProductId,
                    ProductCode = item.ProductCode,
                    OrderItemId = item.Id,
                    FulfillmentType = item.FulfillmentTypeSnapshot,
                    QuantityGranted = quantityGranted,
                    ActivatedAt = activatedAt,
                    ExpiresAt = expiresAt
                };

                var grantAudit = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    ActorUserId = ptProfile?.UserId ?? Guid.Empty,
                    Action = "GrantAddonEntitlement",
                    EntityName = "AddonOrderItem",
                    EntityId = item.Id,
                    MetadataJson = System.Text.Json.JsonSerializer.Serialize(grantMetadata),
                    CreatedAt = DateTime.UtcNow
                };
                await unitOfWork.AuditLogs.AddAsync(grantAudit);
            }

            await unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddonPaymentWebhookService: Post-commit tasks failed for OrderCode {OrderCode}", attempt.OrderCode);
        }
    }
}
