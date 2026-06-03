using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.Payments;

namespace LockedIn.BusinessObject.Interfaces;

public interface IPaymentService
{
    Task<ApiResponse<PaymentResponse>> CreatePaymentLinkAsync(CreatePaymentLinkRequest request);
    Task<ApiResponse<PaymentResponse>> GetPaymentByBookingAsync(Guid bookingId);
    Task<ApiResponse<PaymentResponse>> GetPaymentByIdAsync(Guid paymentId);
    Task<ApiResponse<string>> HandlePayOsWebhookAsync(PayOS.Models.Webhooks.Webhook request);
    Task<ApiResponse<string>> CancelPaymentAsync(Guid paymentId);
}
