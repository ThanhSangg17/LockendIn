using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.DataAccess.Models;
using LockedIn.BusinessObject.DTOs.Payments;
using LockedIn.BusinessObject.Enums;

namespace LockedIn.BusinessObject.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly PayOS.PayOSClient _payOSClient;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
    private readonly Microsoft.Extensions.Logging.ILogger<PaymentService> _logger;
    private readonly IAddonPaymentWebhookService _addonPaymentWebhookService;

    public PaymentService(
        IUnitOfWork unitOfWork, 
        ICurrentUserService currentUserService,
        PayOS.PayOSClient payOSClient,
        Microsoft.Extensions.Configuration.IConfiguration configuration,
        Microsoft.Extensions.Logging.ILogger<PaymentService> logger,
        IAddonPaymentWebhookService addonPaymentWebhookService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _payOSClient = payOSClient;
        _configuration = configuration;
        _logger = logger;
        _addonPaymentWebhookService = addonPaymentWebhookService;
    }

    public async Task<ApiResponse<PaymentResponse>> CreatePaymentLinkAsync(CreatePaymentLinkRequest request)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<PaymentResponse>.Fail("User is not authenticated.");
        }

        if (_currentUserService.Role != (int)UserRole.Customer)
        {
            return ApiResponse<PaymentResponse>.Fail("Only customers can create payment links.");
        }

        var userId = _currentUserService.UserId.Value;
        var customerProfile = await _unitOfWork.CustomerProfiles.Query()
            .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);

        if (customerProfile == null)
        {
            return ApiResponse<PaymentResponse>.Fail("Customer profile not found.");
        }

        var booking = await _unitOfWork.Bookings.Query()
            .FirstOrDefaultAsync(b => b.Id == request.BookingId);

        if (booking == null)
        {
            return ApiResponse<PaymentResponse>.Fail("Booking not found.");
        }

        if (booking.CustomerId != customerProfile.Id)
        {
            return ApiResponse<PaymentResponse>.Fail("You do not own this booking.");
        }

        if (booking.Status != (int)BookingStatus.PendingPayment)
        {
            return ApiResponse<PaymentResponse>.Fail("Booking is not in pending payment status.");
        }

        // Check if booking already has successful payment
        var successfulPayment = await _unitOfWork.Payments.Query()
            .FirstOrDefaultAsync(p => p.BookingId == booking.Id && p.Status == (int)PaymentStatus.Success);

        if (successfulPayment != null)
        {
            return ApiResponse<PaymentResponse>.Fail("Booking already paid");
        }

        // Check if booking already has active pending payment
        var pendingPayment = await _unitOfWork.Payments.Query()
            .FirstOrDefaultAsync(p => p.BookingId == booking.Id && p.Status == (int)PaymentStatus.Pending && p.ExpiredAt > DateTime.UtcNow);

        if (pendingPayment != null)
        {
            var existingResponse = MapToPaymentResponse(pendingPayment);
            return ApiResponse<PaymentResponse>.Ok(existingResponse, "Pending payment link already exists.");
        }

        var returnUrl = _configuration["PayOS:ReturnUrl"];
        var cancelUrl = _configuration["PayOS:CancelUrl"];

        if (string.IsNullOrEmpty(returnUrl) || string.IsNullOrEmpty(cancelUrl))
        {
            return ApiResponse<PaymentResponse>.Fail("PayOS ReturnUrl or CancelUrl is not configured.");
        }

        long payOsOrderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 100 + Random.Shared.Next(0, 100);
        long payOsAmount = Convert.ToInt64(booking.TotalAmount);

        var payOsRequest = new PayOS.Models.V2.PaymentRequests.CreatePaymentLinkRequest
        {
            OrderCode = payOsOrderCode,
            Amount = payOsAmount,
            Description = $"Booking {payOsOrderCode}",
            CancelUrl = cancelUrl,
            ReturnUrl = returnUrl
        };

        PayOS.Models.V2.PaymentRequests.CreatePaymentLinkResponse paymentLink;
        try
        {
            paymentLink = await _payOSClient.PaymentRequests.CreateAsync(payOsRequest);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PAYOS ERROR] Failed to create payment link: {ex}");
            return ApiResponse<PaymentResponse>.Fail($"Failed to create payment link with PayOS: {ex.Message}");
        }

        var orderCodeStr = payOsOrderCode.ToString();
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            Provider = "PayOS",
            OrderCode = orderCodeStr,
            Amount = booking.TotalAmount,
            Status = (int)PaymentStatus.Pending,
            CheckoutUrl = paymentLink.CheckoutUrl,
            ExpiredAt = DateTime.UtcNow.AddMinutes(15),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Payments.AddAsync(payment);
        await _unitOfWork.SaveChangesAsync();

        var response = MapToPaymentResponse(payment);
        return ApiResponse<PaymentResponse>.Ok(response, "Payment link created successfully.");
    }

    public async Task<ApiResponse<PaymentResponse>> GetPaymentByBookingAsync(Guid bookingId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<PaymentResponse>.Fail("User is not authenticated.");
        }

        var booking = await _unitOfWork.Bookings.Query()
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
            return ApiResponse<PaymentResponse>.Fail("Booking not found.");
        }

        var (allowed, error) = await CheckBookingAccessAsync(booking);
        if (!allowed)
        {
            return ApiResponse<PaymentResponse>.Fail(error ?? "Access denied.");
        }

        var latestPayment = await _unitOfWork.Payments.Query()
            .Where(p => p.BookingId == bookingId)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();

        if (latestPayment == null)
        {
            return ApiResponse<PaymentResponse>.Fail("Payment not found");
        }

        var response = MapToPaymentResponse(latestPayment);
        return ApiResponse<PaymentResponse>.Ok(response, "Payment retrieved successfully.");
    }

    public async Task<ApiResponse<PaymentResponse>> GetPaymentByIdAsync(Guid paymentId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<PaymentResponse>.Fail("User is not authenticated.");
        }

        var payment = await _unitOfWork.Payments.Query()
            .Include(p => p.Booking)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null)
        {
            return ApiResponse<PaymentResponse>.Fail("Payment not found.");
        }

        var (allowed, error) = await CheckBookingAccessAsync(payment.Booking);
        if (!allowed)
        {
            return ApiResponse<PaymentResponse>.Fail(error ?? "Access denied.");
        }

        var response = MapToPaymentResponse(payment);
        return ApiResponse<PaymentResponse>.Ok(response, "Payment retrieved successfully.");
    }

    public async Task<ApiResponse<string>> HandlePayOsWebhookAsync(PayOS.Models.Webhooks.Webhook request)
    {
        var receivedAt = DateTime.UtcNow;
        var rawPayload = System.Text.Json.JsonSerializer.Serialize(request);
        _logger.LogInformation("PayOS Webhook received. Raw payload: {Payload}", rawPayload);

        // PayOS returnUrl/cancelUrl are only browser redirects and must not be trusted as payment confirmation.
        // This webhook with signature verification is the trusted source for payment status.

        PayOS.Models.Webhooks.WebhookData verifiedData;
        try
        {
            verifiedData = await _payOSClient.Webhooks.VerifyAsync(request);
            _logger.LogInformation("PayOS Webhook signature verified successfully. OrderCode: {OrderCode}", verifiedData.OrderCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PayOS Webhook signature verification failed.");
            
            var failedWebhookLog = new PaymentWebhookLog
            {
                Id = Guid.NewGuid(),
                Provider = "PayOS",
                EventType = "Webhook",
                EventId = "InvalidSignature",
                RawPayload = rawPayload,
                IsValidSignature = false,
                ReceivedAt = receivedAt
            };
            try
            {
                await _unitOfWork.PaymentWebhookLogs.AddAsync(failedWebhookLog);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "Failed to save failed webhook log to database.");
            }

            return ApiResponse<string>.Fail($"Webhook signature verification failed: {ex.Message}");
        }

        var orderCodeStr = verifiedData.OrderCode.ToString();

        var payment = await _unitOfWork.Payments.Query()
            .Include(p => p.Booking)
            .FirstOrDefaultAsync(p => p.OrderCode == orderCodeStr);

        var webhookLog = new PaymentWebhookLog
        {
            Id = Guid.NewGuid(),
            Provider = "PayOS",
            EventType = "Webhook",
            EventId = orderCodeStr,
            RawPayload = rawPayload,
            IsValidSignature = true,
            ReceivedAt = receivedAt,
            PaymentId = payment?.Id
        };

        if (payment == null)
        {
            // Phase 6D: Check if this is an Add-on payment
            var addonAttempt = await _unitOfWork.AddonPaymentAttempts.Query()
                .FirstOrDefaultAsync(a => a.OrderCode == orderCodeStr);
            if (addonAttempt != null)
            {
                return await _addonPaymentWebhookService.HandleWebhookAsync(verifiedData, request.Code, rawPayload, receivedAt);
            }

            _logger.LogWarning("Payment record not found for OrderCode: {OrderCode}", orderCodeStr);
            
            try
            {
                await _unitOfWork.PaymentWebhookLogs.AddAsync(webhookLog);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "Failed to save webhook log for missing payment.");
            }

            return ApiResponse<string>.Ok("Payment not found but processed safely.", "Payment not found");
        }

        if (payment.Status == (int)PaymentStatus.Success)
        {
            _logger.LogInformation("Webhook for OrderCode {OrderCode} already processed. Payment is already in SUCCESS status.", orderCodeStr);
            
            webhookLog.ProcessedAt = DateTime.UtcNow;
            try
            {
                await _unitOfWork.PaymentWebhookLogs.AddAsync(webhookLog);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "Failed to save webhook log for already processed payment.");
            }

            return ApiResponse<string>.Ok("Webhook already processed", "Webhook already processed");
        }

        var oldPaymentStatus = payment.Status;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (request.Code == "00")
            {
                payment.Status = (int)PaymentStatus.Success;
                payment.PaidAt = DateTime.UtcNow;
                payment.ProviderTransactionId = verifiedData.Reference ?? ("MOCK-" + payment.OrderCode);

                var booking = payment.Booking;
                
                if (booking.Status != (int)BookingStatus.Active)
                {
                    booking.Status = (int)BookingStatus.Active;
                    booking.PaidAt = DateTime.UtcNow;
                    booking.UpdatedAt = DateTime.UtcNow;

                    _unitOfWork.Bookings.Update(booking);

                    _logger.LogInformation("Payment for OrderCode {OrderCode} updated from status {OldStatus} to Success. Booking updated to Active.", orderCodeStr, oldPaymentStatus);

                    var existingWorkspace = await _unitOfWork.Workspaces.Query()
                        .FirstOrDefaultAsync(w => w.BookingId == booking.Id);

                    if (existingWorkspace == null)
                    {
                        var workspace = new Workspace
                        {
                            Id = Guid.NewGuid(),
                            BookingId = booking.Id,
                            CustomerId = booking.CustomerId,
                            PtProfileId = booking.PtProfileId,
                            Status = 1,
                            CourseNote = null,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _unitOfWork.Workspaces.AddAsync(workspace);
                    }

                    var existingConversation = await _unitOfWork.Conversations.Query()
                        .FirstOrDefaultAsync(c => c.BookingId == booking.Id);

                    if (existingConversation == null)
                    {
                        var conversation = new Conversation
                        {
                            Id = Guid.NewGuid(),
                            BookingId = booking.Id,
                            CustomerId = booking.CustomerId,
                            PtProfileId = booking.PtProfileId,
                            FirebaseConversationId = "firebase-" + booking.Id.ToString(),
                            CreatedAt = DateTime.UtcNow
                        };
                        await _unitOfWork.Conversations.AddAsync(conversation);
                    }

                    try
                    {
                        var ptProfile = await _unitOfWork.PtProfiles.Query()
                            .FirstOrDefaultAsync(pt => pt.Id == booking.PtProfileId);
                        var customerProfile = await _unitOfWork.CustomerProfiles.Query()
                            .FirstOrDefaultAsync(c => c.Id == booking.CustomerId);

                        if (ptProfile != null)
                        {
                            var notification = new Notification
                            {
                                Id = Guid.NewGuid(),
                                UserId = ptProfile.UserId,
                                Title = "Booking paid",
                                Content = "A customer has successfully paid. The workspace and conversation are now active.",
                                Type = (int)NotificationType.Booking,
                                IsRead = false,
                                IsDeleted = false,
                                CreatedAt = DateTime.UtcNow
                            };
                            await _unitOfWork.Notifications.AddAsync(notification);
                            _logger.LogInformation("Created notification for PT User {PtUserId} regarding payment success.", ptProfile.UserId);
                        }

                        if (customerProfile != null)
                        {
                            var notification = new Notification
                            {
                                Id = Guid.NewGuid(),
                                UserId = customerProfile.UserId,
                                Title = "Booking activated",
                                Content = "Your payment was successful and the booking is now active. Start training!",
                                Type = (int)NotificationType.Booking,
                                IsRead = false,
                                IsDeleted = false,
                                CreatedAt = DateTime.UtcNow
                            };
                            await _unitOfWork.Notifications.AddAsync(notification);
                            
                            var auditLog = new AuditLog
                            {
                                Id = Guid.NewGuid(),
                                ActorUserId = customerProfile.UserId,
                                Action = "ActivateBooking",
                                EntityName = "Booking",
                                EntityId = booking.Id,
                                MetadataJson = System.Text.Json.JsonSerializer.Serialize(new
                                {
                                    bookingId = booking.Id,
                                    paymentId = payment.Id,
                                    orderCode = orderCodeStr
                                }),
                                CreatedAt = DateTime.UtcNow
                            };
                            await _unitOfWork.AuditLogs.AddAsync(auditLog);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to create payment success notifications or audit log.");
                    }
                }
                else
                {
                    _logger.LogInformation("Payment for OrderCode {OrderCode} updated to Success. Booking was already Active.", orderCodeStr);
                }
            }
            else
            {
                payment.Status = (int)PaymentStatus.Failed;
                _logger.LogInformation("Payment for OrderCode {OrderCode} updated to Failed (status: {Status}, description: {Desc}).", orderCodeStr, request.Code, request.Description);
            }

            _unitOfWork.Payments.Update(payment);

            webhookLog.ProcessedAt = DateTime.UtcNow;
            await _unitOfWork.PaymentWebhookLogs.AddAsync(webhookLog);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Successfully updated Payment status from {OldStatus} to {NewStatus} for OrderCode {OrderCode}.", 
                oldPaymentStatus, payment.Status, orderCodeStr);

            return ApiResponse<string>.Ok("Webhook processed successfully", "Webhook processed successfully");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error occurred while processing PayOS Webhook for OrderCode {OrderCode}.", orderCodeStr);
            return ApiResponse<string>.Fail($"Error processing webhook: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> CancelPaymentAsync(Guid paymentId)
    {
        _logger.LogInformation("CancelPaymentAsync: Received request to cancel payment {PaymentId}.", paymentId);

        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<string>.Fail("User is not authenticated.");
        }

        var payment = await _unitOfWork.Payments.Query()
            .Include(p => p.Booking)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null)
        {
            return ApiResponse<string>.Fail("Payment not found.");
        }

        var userId = _currentUserService.UserId.Value;

        // Validate auth / ownership
        if (_currentUserService.Role == (int)UserRole.Customer)
        {
            var customerProfile = await _unitOfWork.CustomerProfiles.Query()
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);
            if (customerProfile == null || payment.Booking.CustomerId != customerProfile.Id)
            {
                _logger.LogWarning("CancelPaymentAsync: Customer {UserId} does not own the booking for payment {PaymentId}.", userId, paymentId);
                return ApiResponse<string>.Fail("You do not own this payment/booking.");
            }
        }
        else if (_currentUserService.Role != (int)UserRole.Admin)
        {
            _logger.LogWarning("CancelPaymentAsync: User {UserId} with role {Role} tried to cancel payment {PaymentId}.", userId, _currentUserService.Role, paymentId);
            return ApiResponse<string>.Fail("Only customers and admins can cancel payment links.");
        }

        // Idempotency check: If already cancelled
        if (payment.Status == (int)PaymentStatus.Failed && payment.Booking.Status == (int)BookingStatus.Cancelled)
        {
            _logger.LogInformation("CancelPaymentAsync: Payment {PaymentId} and Booking {BookingId} are already cancelled (Idempotent).", paymentId, payment.BookingId);
            return ApiResponse<string>.Ok("Payment already cancelled.", "Payment already cancelled.");
        }

        // Check if already completed
        if (payment.Status == (int)PaymentStatus.Success)
        {
            _logger.LogWarning("CancelPaymentAsync: Attempt to cancel a completed payment. PaymentId: {PaymentId}", paymentId);
            return ApiResponse<string>.Fail("Payment already completed and cannot be cancelled directly.");
        }

        // Require Pending and PendingPayment states
        if (payment.Status != (int)PaymentStatus.Pending || payment.Booking.Status != (int)BookingStatus.PendingPayment)
        {
            _logger.LogWarning("CancelPaymentAsync: Invalid state for cancellation. PaymentId: {PaymentId}, PaymentStatus: {PaymentStatus}, BookingStatus: {BookingStatus}", 
                paymentId, payment.Status, payment.Booking.Status);
            return ApiResponse<string>.Fail("Payment or booking is not in a cancellable state.");
        }

        long orderCode = long.Parse(payment.OrderCode);
        _logger.LogInformation("CancelPaymentAsync: Attempting to cancel PayOS payment link for OrderCode {OrderCode}.", orderCode);
        try
        {
            var payOsResult = await _payOSClient.PaymentRequests.CancelAsync(orderCode, "Cancelled by user or admin");
            _logger.LogInformation("CancelPaymentAsync: PayOS cancellation API response for OrderCode {OrderCode}: {Response}", 
                orderCode, System.Text.Json.JsonSerializer.Serialize(payOsResult));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CancelPaymentAsync: Failed to cancel PayOS payment link for OrderCode {OrderCode}. Continuing with local database updates.", orderCode);
        }

        // Local DB Updates
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var oldPaymentStatus = payment.Status;
            var oldBookingStatus = payment.Booking.Status;

            payment.Status = (int)PaymentStatus.Failed;
            payment.Booking.Status = (int)BookingStatus.Cancelled;
            payment.Booking.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Payments.Update(payment);
            _unitOfWork.Bookings.Update(payment.Booking);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("CancelPaymentAsync: Successfully updated Payment {PaymentId} status from {OldPaymentStatus} to Failed ({NewPaymentStatus}), and Booking {BookingId} status from {OldBookingStatus} to Cancelled ({NewBookingStatus}).",
                payment.Id, oldPaymentStatus, payment.Status, payment.Booking.Id, oldBookingStatus, payment.Booking.Status);

            return ApiResponse<string>.Ok("Payment cancelled successfully.", "Payment cancelled successfully.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "CancelPaymentAsync: Exception occurred during database updates for Payment {PaymentId}.", paymentId);
            return ApiResponse<string>.Fail($"Failed to cancel payment: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PaymentResponse>> ConfirmAndGetPaymentStatusAsync(long orderCode, string forceStatus = null)
    {
        _logger.LogInformation("ConfirmAndGetPaymentStatusAsync: Querying/Forcing status for OrderCode {OrderCode}, forceStatus: {ForceStatus}", orderCode, forceStatus);
        
        try
        {
            var orderCodeStr = orderCode.ToString();
            var payment = await _unitOfWork.Payments.Query()
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.OrderCode == orderCodeStr);

            if (payment == null)
            {
                return ApiResponse<PaymentResponse>.Fail("Payment record not found.");
            }

            if (payment.Status == (int)PaymentStatus.Success || payment.Status == (int)PaymentStatus.Failed)
            {
                return ApiResponse<PaymentResponse>.Ok(MapToPaymentResponse(payment));
            }

            string paymentStatusStr = forceStatus;
            string providerTransactionId = null;

            if (string.IsNullOrEmpty(paymentStatusStr))
            {
                var paymentInfo = await _payOSClient.PaymentRequests.GetAsync(orderCode);
                paymentStatusStr = paymentInfo.Status.ToString();
                providerTransactionId = paymentInfo.Transactions?.LastOrDefault()?.Reference;
            }

            if (string.Equals(paymentStatusStr, "PAID", StringComparison.OrdinalIgnoreCase))
            {
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    payment.Status = (int)PaymentStatus.Success;
                    payment.PaidAt = DateTime.UtcNow;
                    payment.ProviderTransactionId = providerTransactionId ?? ("MOCK-" + payment.OrderCode);

                    var booking = payment.Booking;
                    if (booking.Status != (int)BookingStatus.Active)
                    {
                        booking.Status = (int)BookingStatus.Active;
                        booking.PaidAt = DateTime.UtcNow;
                        booking.UpdatedAt = DateTime.UtcNow;
                        _unitOfWork.Bookings.Update(booking);

                        var existingWorkspace = await _unitOfWork.Workspaces.Query()
                            .FirstOrDefaultAsync(w => w.BookingId == booking.Id);

                        if (existingWorkspace == null)
                        {
                            var workspace = new Workspace
                            {
                                Id = Guid.NewGuid(),
                                BookingId = booking.Id,
                                CustomerId = booking.CustomerId,
                                PtProfileId = booking.PtProfileId,
                                Status = 1,
                                CreatedAt = DateTime.UtcNow
                            };
                            await _unitOfWork.Workspaces.AddAsync(workspace);
                        }

                        var existingConversation = await _unitOfWork.Conversations.Query()
                            .FirstOrDefaultAsync(c => c.BookingId == booking.Id);

                        if (existingConversation == null)
                        {
                            var conversation = new Conversation
                            {
                                Id = Guid.NewGuid(),
                                BookingId = booking.Id,
                                CustomerId = booking.CustomerId,
                                PtProfileId = booking.PtProfileId,
                                FirebaseConversationId = "firebase-" + booking.Id.ToString(),
                                CreatedAt = DateTime.UtcNow
                            };
                            await _unitOfWork.Conversations.AddAsync(conversation);
                        }
                    }

                    _unitOfWork.Payments.Update(payment);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();
                    
                    _logger.LogInformation("ConfirmAndGetPaymentStatusAsync: Successfully updated payment status to Success for OrderCode {OrderCode}", orderCode);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "Error updating payment to Success on redirect for OrderCode {OrderCode}", orderCode);
                }
            }
            else if (paymentStatusStr == "CANCELLED" || paymentStatusStr == "EXPIRED")
            {
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    payment.Status = (int)PaymentStatus.Failed;
                    payment.Booking.Status = (int)BookingStatus.Cancelled;
                    payment.Booking.UpdatedAt = DateTime.UtcNow;

                    _unitOfWork.Payments.Update(payment);
                    _unitOfWork.Bookings.Update(payment.Booking);

                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();

                    _logger.LogInformation("ConfirmAndGetPaymentStatusAsync: Updated payment status to Failed for OrderCode {OrderCode}", orderCode);
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    _logger.LogError(ex, "Error updating payment to Failed on redirect for OrderCode {OrderCode}", orderCode);
                }
            }

            return ApiResponse<PaymentResponse>.Ok(MapToPaymentResponse(payment));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ConfirmAndGetPaymentStatusAsync: Failed to retrieve or process payment from PayOS for OrderCode {OrderCode}", orderCode);
            return ApiResponse<PaymentResponse>.Fail($"Failed to retrieve payment from PayOS: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IReadOnlyList<PaymentResponse>>> GetMyPaymentsAsync()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<IReadOnlyList<PaymentResponse>>.Fail("User is not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;
        var role = _currentUserService.Role;

        List<Payment> payments = new List<Payment>();

        if (role == (int)UserRole.Customer)
        {
            var customerProfile = await _unitOfWork.CustomerProfiles.Query()
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);

            if (customerProfile != null)
            {
                payments = await _unitOfWork.Payments.Query()
                    .Include(p => p.Booking)
                    .Where(p => p.Booking.CustomerId == customerProfile.Id)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
        }
        else if (role == (int)UserRole.PersonalTrainer)
        {
            var ptProfile = await _unitOfWork.PtProfiles.Query()
                .FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);

            if (ptProfile != null)
            {
                payments = await _unitOfWork.Payments.Query()
                    .Include(p => p.Booking)
                    .Where(p => p.Booking.PtProfileId == ptProfile.Id)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }
        }
        else if (role == (int)UserRole.Admin)
        {
            payments = await _unitOfWork.Payments.Query()
                .Include(p => p.Booking)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
        else
        {
            return ApiResponse<IReadOnlyList<PaymentResponse>>.Fail("Access denied: Invalid user role.");
        }

        var response = payments.Select(MapToPaymentResponse).ToList();
        return ApiResponse<IReadOnlyList<PaymentResponse>>.Ok(response, "Payments retrieved successfully.");
    }

    #region Helper Methods

    private async Task<(bool Allowed, string? Error)> CheckBookingAccessAsync(Booking booking)
    {
        var userId = _currentUserService.UserId!.Value;

        if (_currentUserService.Role == (int)UserRole.Customer)
        {
            var customerProfile = await _unitOfWork.CustomerProfiles.Query()
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);
            if (customerProfile == null || booking.CustomerId != customerProfile.Id)
            {
                return (false, "Access denied to this booking.");
            }
        }
        else if (_currentUserService.Role == (int)UserRole.PersonalTrainer)
        {
            var ptProfile = await _unitOfWork.PtProfiles.Query()
                .FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);
            if (ptProfile == null || booking.PtProfileId != ptProfile.Id)
            {
                return (false, "Access denied to this booking.");
            }
        }
        else if (_currentUserService.Role != (int)UserRole.Admin)
        {
            return (false, "Access denied to this booking.");
        }

        return (true, null);
    }

    private PaymentResponse MapToPaymentResponse(Payment payment)
    {
        return new PaymentResponse
        {
            Id = payment.Id,
            BookingId = payment.BookingId,
            Provider = payment.Provider,
            OrderCode = payment.OrderCode,
            Amount = payment.Amount,
            Status = payment.Status,
            CheckoutUrl = payment.CheckoutUrl,
            ProviderTransactionId = payment.ProviderTransactionId,
            PaidAt = payment.PaidAt,
            ExpiredAt = payment.ExpiredAt,
            CreatedAt = payment.CreatedAt
        };
    }

    #endregion
}

