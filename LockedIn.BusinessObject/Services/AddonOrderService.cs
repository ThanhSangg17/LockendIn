using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.AddonOrders;
using LockedIn.BusinessObject.Enums;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.Models;
using LockedIn.DataAccess.UnitOfWork;

namespace LockedIn.BusinessObject.Services;

public class AddonOrderService : IAddonOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly PayOS.PayOSClient _payOSClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AddonOrderService> _logger;

    public AddonOrderService(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        PayOS.PayOSClient payOSClient,
        IConfiguration configuration,
        ILogger<AddonOrderService> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _payOSClient = payOSClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ApiResponse<AddonOrderPaymentLinkResponse>> CreateOrderAsync(CreateAddonOrderRequest request)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<AddonOrderPaymentLinkResponse>.Fail("User is not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;
        var ptProfile = await _unitOfWork.PtProfiles.Query()
            .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);

        if (ptProfile == null)
        {
            return ApiResponse<AddonOrderPaymentLinkResponse>.Fail("Only Personal Trainers can create Add-on orders.");
        }

        var productCode = request.ProductCode?.Trim().ToUpper();
        if (string.IsNullOrEmpty(productCode))
        {
            return ApiResponse<AddonOrderPaymentLinkResponse>.Fail("Product code is required.");
        }

        var product = await _unitOfWork.AddonProducts.Query()
            .Include(p => p.AddonProductPrices)
            .FirstOrDefaultAsync(p => p.Code == productCode);

        if (product == null || !product.IsActive)
        {
            return ApiResponse<AddonOrderPaymentLinkResponse>.Fail("Product not found or inactive.");
        }

        var activePrices = product.AddonProductPrices.Where(p => p.IsActive).ToList();
        if (activePrices.Count != 1)
        {
            return ApiResponse<AddonOrderPaymentLinkResponse>.Fail("Product does not have exactly one active price.");
        }

        var activePrice = activePrices.First();

        var orderCodeValue = await GenerateUniqueOrderCodeAsync();
        var orderCodeStr = orderCodeValue.ToString();

        var orderId = Guid.NewGuid();
        var order = new AddonOrder
        {
            Id = orderId,
            PtProfileId = ptProfile.Id,
            Status = (int)AddonOrderStatus.Pending,
            TotalAmount = activePrice.UnitAmount, // Quantity is always 1 for MVP
            Currency = activePrice.Currency,
            CreatedAt = DateTime.UtcNow
        };

        var orderItem = new AddonOrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = product.Id,
            PriceId = activePrice.Id,
            ProductCode = product.Code,
            ProductName = product.Name,
            UnitAmount = activePrice.UnitAmount,
            Currency = activePrice.Currency,
            Quantity = 1,
            TotalAmount = activePrice.UnitAmount,
            FulfillmentTypeSnapshot = ((AddonProductType)product.ProductType).ToString(),
            GrantQuantitySnapshot = product.GrantQuantity,
            DurationDaysSnapshot = product.DurationDays,
            CreatedAt = DateTime.UtcNow
        };

        var attemptId = Guid.NewGuid();
        var attempt = new AddonPaymentAttempt
        {
            Id = attemptId,
            OrderId = orderId,
            Provider = "PayOS",
            OrderCode = orderCodeStr,
            Amount = activePrice.UnitAmount,
            Currency = activePrice.Currency,
            Status = (int)AddonPaymentAttemptStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _unitOfWork.AddonOrders.AddAsync(order);
            await _unitOfWork.AddonOrderItems.AddAsync(orderItem);
            await _unitOfWork.AddonPaymentAttempts.AddAsync(attempt);
            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }

        await LogAuditActionAsync(userId, "CreateAddonOrder", order, ptProfile, orderItem, attempt);
        await LogAuditActionAsync(userId, "CreateAddonPaymentAttempt", order, ptProfile, orderItem, attempt);

        // Call PayOS
        var cancelUrl = _configuration["PayOS:CancelUrl"] ?? "http://localhost:5173/#/payment-failed";
        var returnUrl = _configuration["PayOS:ReturnUrl"] ?? "http://localhost:5173/#/payment-success";

        var payOsRequest = new PayOS.Models.V2.PaymentRequests.CreatePaymentLinkRequest
        {
            OrderCode = orderCodeValue,
            Amount = Convert.ToInt32(activePrice.UnitAmount), // PayOS uses int for VND usually, ensure safe casting
            Description = $"Addon {orderCodeValue}",
            CancelUrl = cancelUrl,
            ReturnUrl = returnUrl
        };

        PayOS.Models.V2.PaymentRequests.CreatePaymentLinkResponse paymentLink;
        try
        {
            paymentLink = await _payOSClient.PaymentRequests.CreateAsync(payOsRequest);
            
            // Re-fetch attempt from context
            var attemptToUpdate = await _unitOfWork.AddonPaymentAttempts.Query().FirstOrDefaultAsync(a => a.Id == attemptId);
            if (attemptToUpdate != null)
            {
                attemptToUpdate.CheckoutUrl = paymentLink.CheckoutUrl;
                // PayOS uses int/long for expiresAt, but it is returned in some versions? Fallback to +15 mins if not available
                attemptToUpdate.ExpiredAt = DateTime.UtcNow.AddMinutes(15);
                await _unitOfWork.SaveChangesAsync();
                
                attempt.CheckoutUrl = attemptToUpdate.CheckoutUrl;
                attempt.ExpiredAt = attemptToUpdate.ExpiredAt;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"[PAYOS ERROR] Failed to create payment link: {ex}");
            var attemptToUpdate = await _unitOfWork.AddonPaymentAttempts.Query().FirstOrDefaultAsync(a => a.Id == attemptId);
            if (attemptToUpdate != null)
            {
                attemptToUpdate.Status = (int)AddonPaymentAttemptStatus.Failed;
                await _unitOfWork.SaveChangesAsync();
            }
            return ApiResponse<AddonOrderPaymentLinkResponse>.Fail($"Order created but failed to generate payment link: {ex.Message}");
        }

        return ApiResponse<AddonOrderPaymentLinkResponse>.Ok(new AddonOrderPaymentLinkResponse
        {
            OrderId = order.Id,
            OrderStatus = (AddonOrderStatus)order.Status,
            ProductCode = product.Code,
            ProductName = product.Name,
            UnitAmount = activePrice.UnitAmount,
            Currency = activePrice.Currency,
            Quantity = 1,
            TotalAmount = activePrice.UnitAmount,
            PaymentAttemptId = attempt.Id,
            PaymentAttemptStatus = (AddonPaymentAttemptStatus)attempt.Status,
            OrderCode = attempt.OrderCode,
            CheckoutUrl = attempt.CheckoutUrl,
            ExpiredAt = attempt.ExpiredAt,
            CreatedAt = order.CreatedAt
        });
    }

    public async Task<ApiResponse<AddonOrderPaymentLinkResponse>> GetOrRecreatePaymentLinkAsync(Guid orderId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<AddonOrderPaymentLinkResponse>.Fail("User is not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;
        var ptProfile = await _unitOfWork.PtProfiles.Query()
            .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);

        if (ptProfile == null)
        {
            return ApiResponse<AddonOrderPaymentLinkResponse>.Fail("Only Personal Trainers can request Add-on payment links.");
        }

        var order = await _unitOfWork.AddonOrders.Query()
            .Include(o => o.AddonOrderItems)
            .Include(o => o.AddonPaymentAttempts)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return ApiResponse<AddonOrderPaymentLinkResponse>.Fail("Order not found.");
        }

        if (order.PtProfileId != ptProfile.Id)
        {
            return ApiResponse<AddonOrderPaymentLinkResponse>.Fail("You do not own this order.");
        }

        if (order.Status == (int)AddonOrderStatus.Paid)
        {
            return ApiResponse<AddonOrderPaymentLinkResponse>.Fail("Order is already paid.");
        }

        if (order.Status == (int)AddonOrderStatus.Cancelled)
        {
            return ApiResponse<AddonOrderPaymentLinkResponse>.Fail("Order is cancelled.");
        }

        var orderItem = order.AddonOrderItems.FirstOrDefault();
        if (orderItem == null)
        {
            return ApiResponse<AddonOrderPaymentLinkResponse>.Fail("Order item not found.");
        }

        // Expire old pending attempts
        var now = DateTime.UtcNow;
        bool hasChanges = false;
        foreach (var att in order.AddonPaymentAttempts.Where(a => a.Status == (int)AddonPaymentAttemptStatus.Pending))
        {
            if (att.ExpiredAt.HasValue && att.ExpiredAt.Value <= now)
            {
                att.Status = (int)AddonPaymentAttemptStatus.Expired;
                hasChanges = true;
            }
        }
        if (hasChanges)
        {
            await _unitOfWork.SaveChangesAsync();
        }

        // Find valid pending attempt
        var pendingAttempt = order.AddonPaymentAttempts
            .Where(a => a.Status == (int)AddonPaymentAttemptStatus.Pending && !string.IsNullOrEmpty(a.CheckoutUrl))
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefault();

        if (pendingAttempt != null && pendingAttempt.ExpiredAt.HasValue && pendingAttempt.ExpiredAt.Value > now)
        {
            await LogAuditActionAsync(userId, "ReuseAddonPaymentLink", order, ptProfile, orderItem, pendingAttempt);
            
            return ApiResponse<AddonOrderPaymentLinkResponse>.Ok(new AddonOrderPaymentLinkResponse
            {
                OrderId = order.Id,
                OrderStatus = (AddonOrderStatus)order.Status,
                ProductCode = orderItem.ProductCode,
                ProductName = orderItem.ProductName,
                UnitAmount = orderItem.UnitAmount,
                Currency = orderItem.Currency,
                Quantity = orderItem.Quantity,
                TotalAmount = orderItem.TotalAmount,
                PaymentAttemptId = pendingAttempt.Id,
                PaymentAttemptStatus = (AddonPaymentAttemptStatus)pendingAttempt.Status,
                OrderCode = pendingAttempt.OrderCode,
                CheckoutUrl = pendingAttempt.CheckoutUrl,
                ExpiredAt = pendingAttempt.ExpiredAt,
                CreatedAt = order.CreatedAt
            });
        }

        // Generate new attempt
        var orderCodeValue = await GenerateUniqueOrderCodeAsync();
        var orderCodeStr = orderCodeValue.ToString();

        var attemptId = Guid.NewGuid();
        var newAttempt = new AddonPaymentAttempt
        {
            Id = attemptId,
            OrderId = order.Id,
            Provider = "PayOS",
            OrderCode = orderCodeStr,
            Amount = order.TotalAmount,
            Currency = order.Currency,
            Status = (int)AddonPaymentAttemptStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _unitOfWork.AddonPaymentAttempts.AddAsync(newAttempt);
            await _unitOfWork.CommitTransactionAsync();
        }
        catch (DbUpdateException)
        {
            await _unitOfWork.RollbackTransactionAsync();
            // Handle concurrent insert unique index conflict (if we added such an index in EF).
            // Since we use OrderId and there is no strict unique constraint on (OrderId, Status=Pending) in DB, 
            // it shouldn't conflict on unique index unless OrderCode collides.
            return ApiResponse<AddonOrderPaymentLinkResponse>.Fail("A conflict occurred while creating a new payment attempt. Please try again.");
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }

        await LogAuditActionAsync(userId, "RetryAddonPaymentLink", order, ptProfile, orderItem, newAttempt);
        await LogAuditActionAsync(userId, "CreateAddonPaymentAttempt", order, ptProfile, orderItem, newAttempt);


        // Call PayOS
        var cancelUrl = _configuration["PayOS:CancelUrl"] ?? "http://localhost:5173/#/payment-failed";
        var returnUrl = _configuration["PayOS:ReturnUrl"] ?? "http://localhost:5173/#/payment-success";

        var payOsRequest = new PayOS.Models.V2.PaymentRequests.CreatePaymentLinkRequest
        {
            OrderCode = orderCodeValue,
            Amount = Convert.ToInt32(order.TotalAmount),
            Description = $"Addon {orderCodeValue}",
            CancelUrl = cancelUrl,
            ReturnUrl = returnUrl
        };

        PayOS.Models.V2.PaymentRequests.CreatePaymentLinkResponse paymentLink;
        try
        {
            paymentLink = await _payOSClient.PaymentRequests.CreateAsync(payOsRequest);
            
            var attemptToUpdate = await _unitOfWork.AddonPaymentAttempts.Query().FirstOrDefaultAsync(a => a.Id == attemptId);
            if (attemptToUpdate != null)
            {
                attemptToUpdate.CheckoutUrl = paymentLink.CheckoutUrl;
                attemptToUpdate.ExpiredAt = DateTime.UtcNow.AddMinutes(15);
                await _unitOfWork.SaveChangesAsync();
                
                newAttempt.CheckoutUrl = attemptToUpdate.CheckoutUrl;
                newAttempt.ExpiredAt = attemptToUpdate.ExpiredAt;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"[PAYOS ERROR] Failed to create retry payment link: {ex}");
            var attemptToUpdate = await _unitOfWork.AddonPaymentAttempts.Query().FirstOrDefaultAsync(a => a.Id == attemptId);
            if (attemptToUpdate != null)
            {
                attemptToUpdate.Status = (int)AddonPaymentAttemptStatus.Failed;
                await _unitOfWork.SaveChangesAsync();
            }
            return ApiResponse<AddonOrderPaymentLinkResponse>.Fail($"Failed to generate payment link: {ex.Message}");
        }

        return ApiResponse<AddonOrderPaymentLinkResponse>.Ok(new AddonOrderPaymentLinkResponse
        {
            OrderId = order.Id,
            OrderStatus = (AddonOrderStatus)order.Status,
            ProductCode = orderItem.ProductCode,
            ProductName = orderItem.ProductName,
            UnitAmount = orderItem.UnitAmount,
            Currency = orderItem.Currency,
            Quantity = orderItem.Quantity,
            TotalAmount = orderItem.TotalAmount,
            PaymentAttemptId = newAttempt.Id,
            PaymentAttemptStatus = (AddonPaymentAttemptStatus)newAttempt.Status,
            OrderCode = newAttempt.OrderCode,
            CheckoutUrl = newAttempt.CheckoutUrl,
            ExpiredAt = newAttempt.ExpiredAt,
            CreatedAt = order.CreatedAt
        });
    }

    public async Task<ApiResponse<PagedResult<AddonOrderListItemResponse>>> GetMyOrdersAsync(PaginationRequest pagination, AddonOrderStatus? status, DateTime? startDate, DateTime? endDate)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<PagedResult<AddonOrderListItemResponse>>.Fail("User is not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;
        var ptProfile = await _unitOfWork.PtProfiles.Query()
            .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);

        if (ptProfile == null)
        {
            return ApiResponse<PagedResult<AddonOrderListItemResponse>>.Fail("Only Personal Trainers can view Add-on orders.");
        }

        var query = _unitOfWork.AddonOrders.Query()
            .Include(o => o.AddonOrderItems)
            .Include(o => o.AddonPaymentAttempts)
            .Where(o => o.PtProfileId == ptProfile.Id);

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == (int)status.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= endDate.Value);
        }

        query = query.OrderByDescending(o => o.CreatedAt);

        var totalRecords = await query.CountAsync();
        var pagedOrders = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        var items = pagedOrders.Select(o =>
        {
            var item = o.AddonOrderItems.FirstOrDefault();
            var latestAttempt = o.AddonPaymentAttempts.OrderByDescending(a => a.CreatedAt).FirstOrDefault();
            
            return new AddonOrderListItemResponse
            {
                OrderId = o.Id,
                Status = (AddonOrderStatus)o.Status,
                TotalAmount = o.TotalAmount,
                Currency = o.Currency,
                CreatedAt = o.CreatedAt,
                PaidAt = o.PaidAt,
                ProductCode = item?.ProductCode ?? string.Empty,
                ProductName = item?.ProductName ?? string.Empty,
                LatestPaymentAttemptStatus = latestAttempt != null ? (AddonPaymentAttemptStatus)latestAttempt.Status : AddonPaymentAttemptStatus.Pending,
                LatestCheckoutUrl = (latestAttempt != null && latestAttempt.Status == (int)AddonPaymentAttemptStatus.Pending && latestAttempt.ExpiredAt > DateTime.UtcNow) ? latestAttempt.CheckoutUrl : null,
                LatestExpiredAt = latestAttempt?.ExpiredAt
            };
        }).ToList();

        var pagedResult = new PagedResult<AddonOrderListItemResponse>
        {
            Items = items,
            TotalItems = totalRecords,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pagination.PageSize)
        };

        return ApiResponse<PagedResult<AddonOrderListItemResponse>>.Ok(pagedResult);
    }

    private async Task<long> GenerateUniqueOrderCodeAsync()
    {
        long payOsOrderCode = 0;
        int retries = 0;
        bool isUnique = false;

        while (!isUnique && retries < 10)
        {
            payOsOrderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000 + Random.Shared.Next(500, 600);
            var orderCodeStr = payOsOrderCode.ToString();

            bool paymentExists = await _unitOfWork.Payments.Query().AnyAsync(p => p.OrderCode == orderCodeStr);
            bool addonPaymentExists = await _unitOfWork.AddonPaymentAttempts.Query().AnyAsync(a => a.OrderCode == orderCodeStr);

            if (!paymentExists && !addonPaymentExists)
            {
                isUnique = true;
            }
            retries++;
        }

        if (!isUnique)
        {
            throw new Exception("Could not generate a unique order code for PayOS after multiple attempts.");
        }

        return payOsOrderCode;
    }

    private async Task LogAuditActionAsync(Guid userId, string action, AddonOrder order, PtProfile ptProfile, AddonOrderItem orderItem, AddonPaymentAttempt attempt)
    {
        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                ActorUserId = userId,
                Action = action,
                EntityName = "AddonOrder",
                EntityId = order.Id,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    OrderId = order.Id,
                    PtProfileId = ptProfile.Id,
                    ProductId = orderItem.ProductId,
                    ProductCode = orderItem.ProductCode,
                    PriceId = orderItem.PriceId,
                    PaymentAttemptId = attempt.Id,
                    OrderCode = attempt.OrderCode,
                    TotalAmount = orderItem.TotalAmount,
                    Currency = orderItem.Currency
                }),
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Failed to write audit log for action {action} on AddonOrder {order.Id}: {ex.Message}");
        }
    }
}
