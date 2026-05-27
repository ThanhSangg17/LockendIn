using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;

namespace LockedIn.BusinessObject.Interfaces;

public interface IPaymentService
{
    Task<ApiResponse<string>> CreatePaymentLinkAsync();
    Task<ApiResponse<string>> GetPaymentByBookingAsync(Guid bookingId);
    Task<ApiResponse<string>> GetPaymentByIdAsync(Guid paymentId);
    Task<ApiResponse<string>> HandlePayOsWebhookAsync();
}
