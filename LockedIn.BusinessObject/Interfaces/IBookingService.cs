using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.Bookings;

namespace LockedIn.BusinessObject.Interfaces;

public interface IBookingService
{
    Task<ApiResponse<BookingResponse>> CreateBookingAsync(CreateBookingRequest request);
    Task<ApiResponse<IReadOnlyList<BookingResponse>>> GetMyBookingsAsync();
    Task<ApiResponse<BookingDetailResponse>> GetBookingByIdAsync(Guid bookingId);
    Task<ApiResponse<BookingResponse>> CancelBookingAsync(Guid bookingId);
    Task<ApiResponse<BookingResponse>> AcceptBookingAsync(Guid bookingId);
    Task<ApiResponse<BookingResponse>> RejectBookingAsync(Guid bookingId);
    Task<ApiResponse<BookingResponse>> CompleteBookingAsync(Guid bookingId);
}
