using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.BusinessObject.DTOs.Payments;

namespace LockedIn.BusinessObject.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PaymentResponse>> CreatePaymentLinkAsync(CreatePaymentLinkRequest request)
    {
        return await Task.FromResult(ApiResponse<PaymentResponse>.Ok(new PaymentResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<PaymentResponse>> GetPaymentByBookingAsync(Guid bookingId)
    {
        return await Task.FromResult(ApiResponse<PaymentResponse>.Ok(new PaymentResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<PaymentResponse>> GetPaymentByIdAsync(Guid paymentId)
    {
        return await Task.FromResult(ApiResponse<PaymentResponse>.Ok(new PaymentResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<string>> HandlePayOsWebhookAsync(PayOsWebhookRequest request)
    {
        return await Task.FromResult(ApiResponse<string>.Ok(string.Empty, "Not implemented yet"));
    }
}
