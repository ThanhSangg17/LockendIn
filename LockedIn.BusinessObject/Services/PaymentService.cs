using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

    public PaymentService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
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

        var orderCode = "ORDER-" + DateTime.UtcNow.Ticks;
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            Provider = "PayOS",
            OrderCode = orderCode,
            Amount = booking.TotalAmount,
            Status = (int)PaymentStatus.Pending,
            CheckoutUrl = "https://pay.payos.vn/checkout/" + orderCode,
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

    public async Task<ApiResponse<string>> HandlePayOsWebhookAsync(PayOsWebhookRequest request)
    {
        var receivedAt = DateTime.UtcNow;
        var rawPayload = System.Text.Json.JsonSerializer.Serialize(request);
        var isValidSignature = !string.IsNullOrWhiteSpace(request.Signature);

        var webhookLog = new PaymentWebhookLog
        {
            Id = Guid.NewGuid(),
            Provider = "PayOS",
            EventType = "Webhook",
            EventId = request.Data,
            RawPayload = rawPayload,
            IsValidSignature = isValidSignature,
            ReceivedAt = receivedAt
        };

        if (!isValidSignature)
        {
            await _unitOfWork.PaymentWebhookLogs.AddAsync(webhookLog);
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<string>.Fail("Invalid webhook signature");
        }

        var orderCode = request.Data;
        var payment = await _unitOfWork.Payments.Query()
            .Include(p => p.Booking)
            .FirstOrDefaultAsync(p => p.OrderCode == orderCode);

        if (payment == null)
        {
            await _unitOfWork.PaymentWebhookLogs.AddAsync(webhookLog);
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<string>.Fail("Payment not found");
        }

        webhookLog.PaymentId = payment.Id;

        if (payment.Status == (int)PaymentStatus.Success)
        {
            webhookLog.ProcessedAt = DateTime.UtcNow;
            await _unitOfWork.PaymentWebhookLogs.AddAsync(webhookLog);
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<string>.Ok("Webhook already processed", "Webhook already processed");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (request.Code == "00")
            {
                payment.Status = (int)PaymentStatus.Success;
                payment.PaidAt = DateTime.UtcNow;
                payment.ProviderTransactionId = "MOCK-" + payment.OrderCode;

                var booking = payment.Booking;
                booking.Status = (int)BookingStatus.PaidPendingAcceptance;
                booking.PaidAt = DateTime.UtcNow;
                booking.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.Bookings.Update(booking);

                try
                {
                    var ptProfile = await _unitOfWork.PtProfiles.Query()
                        .FirstOrDefaultAsync(pt => pt.Id == booking.PtProfileId);
                    if (ptProfile != null)
                    {
                        var notification = new Notification
                        {
                            Id = Guid.NewGuid(),
                            UserId = ptProfile.UserId,
                            Title = "New paid booking",
                            Content = "A customer has paid for a booking and is waiting for your acceptance.",
                            Type = (int)NotificationType.Booking,
                            IsRead = false,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _unitOfWork.Notifications.AddAsync(notification);
                    }
                }
                catch
                {
                    // silently ignore
                }
            }
            else
            {
                payment.Status = (int)PaymentStatus.Failed;
            }

            _unitOfWork.Payments.Update(payment);

            webhookLog.ProcessedAt = DateTime.UtcNow;
            await _unitOfWork.PaymentWebhookLogs.AddAsync(webhookLog);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return ApiResponse<string>.Ok("Webhook processed successfully", "Webhook processed successfully");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ApiResponse<string>.Fail($"Error processing webhook: {ex.Message}");
        }
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

