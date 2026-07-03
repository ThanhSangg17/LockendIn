using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.Bookings;
using LockedIn.DataAccess.Models;

namespace LockedIn.BusinessObject.Interfaces;

public record BookingCompletionResult(
    Guid BookingId,
    Guid CustomerUserId,
    Guid PtUserId,
    decimal TotalAmount,
    Guid SettlementId
);

public interface IBookingService
{
    Task<ApiResponse<BookingResponse>> CreateBookingAsync(CreateBookingRequest request);
    Task<ApiResponse<PagedResult<BookingHistoryResponse>>> GetMyBookingsAsync(PaginationRequest request, int? bookingStatus, int? paymentStatus, DateTime? startDate, DateTime? endDate);
    Task<ApiResponse<BookingDetailResponse>> GetBookingByIdAsync(Guid bookingId);
    Task<ApiResponse<BookingResponse>> CancelBookingAsync(Guid bookingId);
    Task<ApiResponse<BookingResponse>> AcceptBookingAsync(Guid bookingId);
    Task<ApiResponse<BookingResponse>> RejectBookingAsync(Guid bookingId);
    Task<ApiResponse<BookingResponse>> CompleteBookingAsync(Guid bookingId);
    Task<BookingCompletionResult> CompleteBookingCoreAsync(Booking booking);
}
