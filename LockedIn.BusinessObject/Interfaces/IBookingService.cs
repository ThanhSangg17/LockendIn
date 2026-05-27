using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;

namespace LockedIn.BusinessObject.Interfaces;

public interface IBookingService
{
    Task<ApiResponse<string>> CreateBookingAsync();
    Task<ApiResponse<string>> GetMyBookingsAsync();
    Task<ApiResponse<string>> GetBookingByIdAsync(Guid bookingId);
    Task<ApiResponse<string>> CancelBookingAsync(Guid bookingId);
    Task<ApiResponse<string>> AcceptBookingAsync(Guid bookingId);
    Task<ApiResponse<string>> RejectBookingAsync(Guid bookingId);
    Task<ApiResponse<string>> CompleteBookingAsync(Guid bookingId);
}
