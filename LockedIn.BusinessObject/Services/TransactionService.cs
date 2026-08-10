using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.Transactions;
using LockedIn.BusinessObject.Enums;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.Models;
using LockedIn.DataAccess.UnitOfWork;

namespace LockedIn.BusinessObject.Services;

public class TransactionService : ITransactionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public TransactionService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<PagedResult<AdminTransactionResponse>>> GetAdminTransactionsAsync(TransactionSearchRequest request)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<PagedResult<AdminTransactionResponse>>.Fail("Only Admins can access all system transactions.");
        }

        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        bool includePayments = string.IsNullOrWhiteSpace(request.TransactionType) || 
                              string.Equals(request.TransactionType, "PAYMENT", StringComparison.OrdinalIgnoreCase);
        bool includeSettlements = string.IsNullOrWhiteSpace(request.TransactionType) || 
                                 string.Equals(request.TransactionType, "SETTLEMENT", StringComparison.OrdinalIgnoreCase);

        List<AdminTransactionResponse> allResults = new List<AdminTransactionResponse>();
        int totalItems = 0;

        if (includePayments)
        {
            var pQuery = _unitOfWork.Payments.Query()
                .Include(p => p.Booking).ThenInclude(b => b.Package)
                .Include(p => p.Booking).ThenInclude(b => b.Customer).ThenInclude(c => c.User)
                .Include(p => p.Booking).ThenInclude(b => b.PtProfile).ThenInclude(pt => pt.User)
                .AsQueryable();

            if (request.Status.HasValue)
            {
                pQuery = pQuery.Where(p => p.Status == request.Status.Value);
            }
            if (request.StartDate.HasValue)
            {
                pQuery = pQuery.Where(p => p.CreatedAt >= request.StartDate.Value);
            }
            if (request.EndDate.HasValue)
            {
                pQuery = pQuery.Where(p => p.CreatedAt <= request.EndDate.Value);
            }
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.Trim().ToLower();
                pQuery = pQuery.Where(p => p.OrderCode.ToLower().Contains(s) ||
                                           (p.Booking.Customer != null && p.Booking.Customer.User != null && p.Booking.Customer.User.Email.ToLower().Contains(s)) ||
                                           (p.Booking.Customer != null && p.Booking.Customer.User != null && p.Booking.Customer.User.FullName.ToLower().Contains(s)) ||
                                           (p.Booking.PtProfile != null && p.Booking.PtProfile.User != null && p.Booking.PtProfile.User.Email.ToLower().Contains(s)) ||
                                           (p.Booking.PtProfile != null && p.Booking.PtProfile.User != null && p.Booking.PtProfile.User.FullName.ToLower().Contains(s)));
            }

            if (!includeSettlements)
            {
                // Pure Payment query pagination
                totalItems = await pQuery.CountAsync();
                var pagedPayments = await pQuery
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                allResults = pagedPayments.Select(MapPaymentToAdminResponse).ToList();
            }
            else
            {
                var payments = await pQuery.ToListAsync();
                allResults.AddRange(payments.Select(MapPaymentToAdminResponse));
            }
        }

        if (includeSettlements)
        {
            var sQuery = _unitOfWork.Settlements.Query()
                .Include(s => s.Booking).ThenInclude(b => b.Package)
                .Include(s => s.Booking).ThenInclude(b => b.Customer).ThenInclude(c => c.User)
                .Include(s => s.PtProfile).ThenInclude(pt => pt.User)
                .AsQueryable();

            if (request.Status.HasValue)
            {
                sQuery = sQuery.Where(s => s.Status == request.Status.Value);
            }
            if (request.StartDate.HasValue)
            {
                sQuery = sQuery.Where(s => s.CreatedAt >= request.StartDate.Value);
            }
            if (request.EndDate.HasValue)
            {
                sQuery = sQuery.Where(s => s.CreatedAt <= request.EndDate.Value);
            }
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var searchStr = request.Search.Trim().ToLower();
                sQuery = sQuery.Where(s => (s.Booking.Customer != null && s.Booking.Customer.User != null && s.Booking.Customer.User.Email.ToLower().Contains(searchStr)) ||
                                           (s.Booking.Customer != null && s.Booking.Customer.User != null && s.Booking.Customer.User.FullName.ToLower().Contains(searchStr)) ||
                                           (s.PtProfile != null && s.PtProfile.User != null && s.PtProfile.User.Email.ToLower().Contains(searchStr)) ||
                                           (s.PtProfile != null && s.PtProfile.User != null && s.PtProfile.User.FullName.ToLower().Contains(searchStr)));
            }

            if (!includePayments)
            {
                // Pure Settlement query pagination
                totalItems = await sQuery.CountAsync();
                var pagedSettlements = await sQuery
                    .OrderByDescending(s => s.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                allResults = pagedSettlements.Select(MapSettlementToAdminResponse).ToList();
            }
            else
            {
                var settlements = await sQuery.ToListAsync();
                allResults.AddRange(settlements.Select(MapSettlementToAdminResponse));
            }
        }

        // Merge case (includePayments && includeSettlements)
        if (includePayments && includeSettlements)
        {
            totalItems = allResults.Count;
            allResults = allResults
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        var pagedResult = new PagedResult<AdminTransactionResponse>
        {
            Items = allResults,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };

        return ApiResponse<PagedResult<AdminTransactionResponse>>.Ok(pagedResult, "Admin transactions retrieved successfully.");
    }

    public async Task<ApiResponse<PagedResult<UserTransactionResponse>>> GetMyTransactionsAsync(TransactionSearchRequest request)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<PagedResult<UserTransactionResponse>>.Fail("User is not authenticated.");
        }

        var userId = _currentUserService.UserId.Value;
        var role = _currentUserService.Role;
        var pageNumber = request.PageNumber <= 0 ? 1 : request.PageNumber;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        List<UserTransactionResponse> items = new List<UserTransactionResponse>();
        int totalItems = 0;

        if (role == (int)UserRole.Customer)
        {
            var customerProfile = await _unitOfWork.CustomerProfiles.Query()
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);

            if (customerProfile == null)
            {
                return ApiResponse<PagedResult<UserTransactionResponse>>.Fail("Customer profile not found.");
            }

            var pQuery = _unitOfWork.Payments.Query()
                .Include(p => p.Booking).ThenInclude(b => b.Package)
                .Include(p => p.Booking).ThenInclude(b => b.PtProfile).ThenInclude(pt => pt.User)
                .Where(p => p.Booking.CustomerId == customerProfile.Id);

            if (request.Status.HasValue)
            {
                pQuery = pQuery.Where(p => p.Status == request.Status.Value);
            }
            if (request.StartDate.HasValue)
            {
                pQuery = pQuery.Where(p => p.CreatedAt >= request.StartDate.Value);
            }
            if (request.EndDate.HasValue)
            {
                pQuery = pQuery.Where(p => p.CreatedAt <= request.EndDate.Value);
            }

            totalItems = await pQuery.CountAsync();
            var payments = await pQuery
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            items = payments.Select(MapPaymentToUserResponse).ToList();
        }
        else if (role == (int)UserRole.PersonalTrainer)
        {
            var ptProfile = await _unitOfWork.PtProfiles.Query()
                .FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);

            if (ptProfile == null)
            {
                return ApiResponse<PagedResult<UserTransactionResponse>>.Fail("Personal trainer profile not found.");
            }

            var sQuery = _unitOfWork.Settlements.Query()
                .Include(s => s.Booking).ThenInclude(b => b.Package)
                .Include(s => s.Booking).ThenInclude(b => b.Customer).ThenInclude(c => c.User)
                .Where(s => s.PtProfileId == ptProfile.Id);

            if (request.Status.HasValue)
            {
                sQuery = sQuery.Where(s => s.Status == request.Status.Value);
            }
            if (request.StartDate.HasValue)
            {
                sQuery = sQuery.Where(s => s.CreatedAt >= request.StartDate.Value);
            }
            if (request.EndDate.HasValue)
            {
                sQuery = sQuery.Where(s => s.CreatedAt <= request.EndDate.Value);
            }

            totalItems = await sQuery.CountAsync();
            var settlements = await sQuery
                .OrderByDescending(s => s.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            items = settlements.Select(MapSettlementToUserResponse).ToList();
        }
        else
        {
            return ApiResponse<PagedResult<UserTransactionResponse>>.Fail("Access denied: Invalid user role.");
        }

        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        var pagedResult = new PagedResult<UserTransactionResponse>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };

        return ApiResponse<PagedResult<UserTransactionResponse>>.Ok(pagedResult, "Transaction history retrieved successfully.");
    }

    #region Private Mapping Helpers

    private static AdminTransactionResponse MapPaymentToAdminResponse(Payment p)
    {
        var customerUser = p.Booking?.Customer?.User;
        var ptUser = p.Booking?.PtProfile?.User;

        return new AdminTransactionResponse
        {
            Id = p.Id,
            TransactionType = "PAYMENT",
            ReferenceCode = p.OrderCode ?? p.Id.ToString()[..8],
            BookingId = p.BookingId,
            PackageName = p.Booking?.Package?.Name ?? "N/A",
            CustomerId = p.Booking?.CustomerId ?? Guid.Empty,
            CustomerName = customerUser?.FullName ?? "N/A",
            CustomerEmail = customerUser?.Email ?? "N/A",
            PtProfileId = p.Booking?.PtProfileId ?? Guid.Empty,
            PtName = ptUser?.FullName ?? "N/A",
            PtEmail = ptUser?.Email ?? "N/A",
            GrossAmount = p.Amount,
            PlatformFee = 0m,
            NetAmount = p.Amount,
            Status = p.Status,
            StatusName = ((PaymentStatus)p.Status).ToString(),
            CreatedAt = p.CreatedAt,
            ProcessedAt = p.PaidAt
        };
    }

    private static AdminTransactionResponse MapSettlementToAdminResponse(Settlement s)
    {
        var customerUser = s.Booking?.Customer?.User;
        var ptUser = s.PtProfile?.User;

        return new AdminTransactionResponse
        {
            Id = s.Id,
            TransactionType = "SETTLEMENT",
            ReferenceCode = s.Id.ToString()[..8].ToUpper(),
            BookingId = s.BookingId,
            PackageName = s.Booking?.Package?.Name ?? "N/A",
            CustomerId = s.Booking?.CustomerId ?? Guid.Empty,
            CustomerName = customerUser?.FullName ?? "N/A",
            CustomerEmail = customerUser?.Email ?? "N/A",
            PtProfileId = s.PtProfileId,
            PtName = ptUser?.FullName ?? "N/A",
            PtEmail = ptUser?.Email ?? "N/A",
            GrossAmount = s.GrossAmount,
            PlatformFee = s.PlatformFee,
            NetAmount = s.NetAmount,
            Status = s.Status,
            StatusName = ((SettlementStatus)s.Status).ToString(),
            CreatedAt = s.CreatedAt,
            ProcessedAt = s.SettledAt
        };
    }

    private static UserTransactionResponse MapPaymentToUserResponse(Payment p)
    {
        var ptUser = p.Booking?.PtProfile?.User;

        return new UserTransactionResponse
        {
            Id = p.Id,
            TransactionType = "PAYMENT",
            ReferenceCode = p.OrderCode ?? p.Id.ToString()[..8],
            BookingId = p.BookingId,
            PackageName = p.Booking?.Package?.Name ?? "N/A",
            CounterpartyName = ptUser?.FullName ?? "HLV",
            Amount = p.Amount,
            GrossAmount = null,
            PlatformFee = null,
            Status = p.Status,
            StatusName = ((PaymentStatus)p.Status).ToString(),
            CreatedAt = p.CreatedAt,
            ProcessedAt = p.PaidAt
        };
    }

    private static UserTransactionResponse MapSettlementToUserResponse(Settlement s)
    {
        var customerUser = s.Booking?.Customer?.User;

        return new UserTransactionResponse
        {
            Id = s.Id,
            TransactionType = "SETTLEMENT",
            ReferenceCode = s.Id.ToString()[..8].ToUpper(),
            BookingId = s.BookingId,
            PackageName = s.Booking?.Package?.Name ?? "N/A",
            CounterpartyName = customerUser?.FullName ?? "Khách hàng",
            Amount = s.NetAmount,
            GrossAmount = s.GrossAmount,
            PlatformFee = s.PlatformFee,
            Status = s.Status,
            StatusName = ((SettlementStatus)s.Status).ToString(),
            CreatedAt = s.CreatedAt,
            ProcessedAt = s.SettledAt
        };
    }

    #endregion
}
