using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.BusinessObject.DTOs.Bookings;

namespace LockedIn.BusinessObject.Services;

public class BookingService : IBookingService
{
    private readonly IUnitOfWork _unitOfWork;

    public BookingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<BookingResponse>> CreateBookingAsync(CreateBookingRequest request)
    {
        return await Task.FromResult(ApiResponse<BookingResponse>.Ok(new BookingResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<IReadOnlyList<BookingResponse>>> GetMyBookingsAsync()
    {
        return await Task.FromResult(ApiResponse<IReadOnlyList<BookingResponse>>.Ok(new List<BookingResponse>(), "Not implemented yet"));
    }

    public async Task<ApiResponse<BookingDetailResponse>> GetBookingByIdAsync(Guid bookingId)
    {
        return await Task.FromResult(ApiResponse<BookingDetailResponse>.Ok(new BookingDetailResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<BookingResponse>> CancelBookingAsync(Guid bookingId)
    {
        return await Task.FromResult(ApiResponse<BookingResponse>.Ok(new BookingResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<BookingResponse>> AcceptBookingAsync(Guid bookingId)
    {
        return await Task.FromResult(ApiResponse<BookingResponse>.Ok(new BookingResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<BookingResponse>> RejectBookingAsync(Guid bookingId)
    {
        return await Task.FromResult(ApiResponse<BookingResponse>.Ok(new BookingResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<BookingResponse>> CompleteBookingAsync(Guid bookingId)
    {
        return await Task.FromResult(ApiResponse<BookingResponse>.Ok(new BookingResponse(), "Not implemented yet"));
    }
}
