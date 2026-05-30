using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.DataAccess.Models;
using LockedIn.BusinessObject.DTOs.Admin;
using LockedIn.BusinessObject.DTOs.Disputes;
using LockedIn.BusinessObject.DTOs.PtProfiles;
using LockedIn.BusinessObject.Enums;

namespace LockedIn.BusinessObject.Services;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AdminService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<DashboardResponse>> GetDashboardAsync()
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<DashboardResponse>.Fail("Only Admins can perform this action.");
        }

        var totalUsers = await _unitOfWork.Users.Query().CountAsync(u => !u.IsDeleted);
        var totalCustomers = await _unitOfWork.Users.Query().CountAsync(u => u.Role == (int)UserRole.Customer && !u.IsDeleted);
        var totalPts = await _unitOfWork.Users.Query().CountAsync(u => u.Role == (int)UserRole.PersonalTrainer && !u.IsDeleted);
        var totalBookings = await _unitOfWork.Bookings.Query().CountAsync();
        var totalRevenue = await _unitOfWork.Payments.Query().Where(p => p.Status == (int)PaymentStatus.Success).SumAsync(p => (decimal?)p.Amount) ?? 0m;
        var openDisputes = await _unitOfWork.Disputes.Query().CountAsync(d => d.Status == (int)DisputeStatus.Open || d.Status == (int)DisputeStatus.UnderReview);

        var response = new DashboardResponse
        {
            TotalUsers = totalUsers,
            TotalCustomers = totalCustomers,
            TotalPts = totalPts,
            TotalBookings = totalBookings,
            TotalRevenue = totalRevenue,
            OpenDisputes = openDisputes
        };

        return ApiResponse<DashboardResponse>.Ok(response, "Dashboard retrieved successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<AdminUserResponse>>> GetUsersAsync()
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<IReadOnlyList<AdminUserResponse>>.Fail("Only Admins can perform this action.");
        }

        var users = await _unitOfWork.Users.Query()
            .Where(u => !u.IsDeleted)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        var response = users.Select(u => new AdminUserResponse
        {
            Id = u.Id,
            Email = u.Email,
            FullName = u.FullName,
            Phone = u.Phone,
            Role = u.Role,
            Status = u.Status,
            EmailVerified = u.EmailVerified,
            CreatedAt = u.CreatedAt
        }).ToList();

        return ApiResponse<IReadOnlyList<AdminUserResponse>>.Ok(response, "Users retrieved successfully.");
    }

    public async Task<ApiResponse<AdminUserResponse>> GetUserByIdAsync(Guid userId)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<AdminUserResponse>.Fail("Only Admins can perform this action.");
        }

        var user = await _unitOfWork.Users.Query()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
        {
            return ApiResponse<AdminUserResponse>.Fail("User not found.");
        }

        var response = new AdminUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            Role = user.Role,
            Status = user.Status,
            EmailVerified = user.EmailVerified,
            CreatedAt = user.CreatedAt
        };

        return ApiResponse<AdminUserResponse>.Ok(response, "User retrieved successfully.");
    }

    public async Task<ApiResponse<AdminUserResponse>> BanUserAsync(Guid userId)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<AdminUserResponse>.Fail("Only Admins can perform this action.");
        }

        var currentUserId = _currentUserService.UserId!.Value;
        if (userId == currentUserId)
        {
            return ApiResponse<AdminUserResponse>.Fail("You cannot ban yourself.");
        }

        var user = await _unitOfWork.Users.Query()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
        {
            return ApiResponse<AdminUserResponse>.Fail("User not found.");
        }

        user.Status = (int)UserStatus.Banned;
        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);

        // Audit log
        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                ActorUserId = currentUserId,
                Action = "BanUser",
                EntityName = "User",
                EntityId = userId,
                MetadataJson = System.Text.Json.JsonSerializer.Serialize(new { Email = user.Email }),
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AuditLogs.AddAsync(auditLog);
        }
        catch {}

        await _unitOfWork.SaveChangesAsync();

        var response = new AdminUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            Role = user.Role,
            Status = user.Status,
            EmailVerified = user.EmailVerified,
            CreatedAt = user.CreatedAt
        };

        return ApiResponse<AdminUserResponse>.Ok(response, "User banned successfully.");
    }

    public async Task<ApiResponse<AdminUserResponse>> UnbanUserAsync(Guid userId)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<AdminUserResponse>.Fail("Only Admins can perform this action.");
        }

        var user = await _unitOfWork.Users.Query()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
        {
            return ApiResponse<AdminUserResponse>.Fail("User not found.");
        }

        user.Status = (int)UserStatus.Active;
        user.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);

        // Audit log
        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                ActorUserId = _currentUserService.UserId!.Value,
                Action = "UnbanUser",
                EntityName = "User",
                EntityId = userId,
                MetadataJson = System.Text.Json.JsonSerializer.Serialize(new { Email = user.Email }),
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AuditLogs.AddAsync(auditLog);
        }
        catch {}

        await _unitOfWork.SaveChangesAsync();

        var response = new AdminUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            Role = user.Role,
            Status = user.Status,
            EmailVerified = user.EmailVerified,
            CreatedAt = user.CreatedAt
        };

        return ApiResponse<AdminUserResponse>.Ok(response, "User unbanned successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<PtProfileResponse>>> GetPtVerificationsAsync()
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<IReadOnlyList<PtProfileResponse>>.Fail("Only Admins can perform this action.");
        }

        var ptProfiles = await _unitOfWork.PtProfiles.Query()
            .Include(pt => pt.User)
            .Where(pt => pt.VerificationStatus != (int)PtVerificationStatus.Approved && !pt.IsDeleted)
            .OrderByDescending(pt => pt.CreatedAt)
            .ToListAsync();

        var response = ptProfiles.Select(pt => new PtProfileResponse
        {
            Id = pt.Id,
            UserId = pt.UserId,
            FullName = pt.User?.FullName ?? string.Empty,
            Bio = pt.Bio,
            Specialization = pt.Specialization,
            ExperienceYears = pt.ExperienceYears,
            VerificationStatus = pt.VerificationStatus,
            AverageRating = pt.AverageRating,
            TotalReviews = pt.TotalReviews
        }).ToList();

        return ApiResponse<IReadOnlyList<PtProfileResponse>>.Ok(response, "PT verifications retrieved successfully.");
    }

    public async Task<ApiResponse<PtProfileResponse>> ApprovePtAsync(Guid ptProfileId)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<PtProfileResponse>.Fail("Only Admins can perform this action.");
        }

        var ptProfile = await _unitOfWork.PtProfiles.Query()
            .Include(pt => pt.User)
            .FirstOrDefaultAsync(pt => pt.Id == ptProfileId && !pt.IsDeleted);

        if (ptProfile == null)
        {
            return ApiResponse<PtProfileResponse>.Fail("PT profile not found.");
        }

        ptProfile.VerificationStatus = (int)PtVerificationStatus.Approved;
        ptProfile.ApprovedAt = DateTime.UtcNow;
        ptProfile.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.PtProfiles.Update(ptProfile);

        // Audit log
        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                ActorUserId = _currentUserService.UserId!.Value,
                Action = "ApprovePT",
                EntityName = "PtProfile",
                EntityId = ptProfileId,
                MetadataJson = System.Text.Json.JsonSerializer.Serialize(new { Email = ptProfile.User?.Email }),
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AuditLogs.AddAsync(auditLog);
        }
        catch {}

        // Notification
        try
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = ptProfile.UserId,
                Title = "PT verification approved",
                Content = "Your PT profile has been approved.",
                Type = (int)NotificationType.System,
                IsRead = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Notifications.AddAsync(notification);
        }
        catch {}

        await _unitOfWork.SaveChangesAsync();

        var response = new PtProfileResponse
        {
            Id = ptProfile.Id,
            UserId = ptProfile.UserId,
            FullName = ptProfile.User?.FullName ?? string.Empty,
            Bio = ptProfile.Bio,
            Specialization = ptProfile.Specialization,
            ExperienceYears = ptProfile.ExperienceYears,
            VerificationStatus = ptProfile.VerificationStatus,
            AverageRating = ptProfile.AverageRating,
            TotalReviews = ptProfile.TotalReviews
        };

        return ApiResponse<PtProfileResponse>.Ok(response, "PT profile approved successfully.");
    }

    public async Task<ApiResponse<PtProfileResponse>> RejectPtAsync(Guid ptProfileId)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<PtProfileResponse>.Fail("Only Admins can perform this action.");
        }

        var ptProfile = await _unitOfWork.PtProfiles.Query()
            .Include(pt => pt.User)
            .FirstOrDefaultAsync(pt => pt.Id == ptProfileId && !pt.IsDeleted);

        if (ptProfile == null)
        {
            return ApiResponse<PtProfileResponse>.Fail("PT profile not found.");
        }

        ptProfile.VerificationStatus = (int)PtVerificationStatus.Rejected;
        ptProfile.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.PtProfiles.Update(ptProfile);

        // Audit log
        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                ActorUserId = _currentUserService.UserId!.Value,
                Action = "RejectPT",
                EntityName = "PtProfile",
                EntityId = ptProfileId,
                MetadataJson = System.Text.Json.JsonSerializer.Serialize(new { Email = ptProfile.User?.Email }),
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AuditLogs.AddAsync(auditLog);
        }
        catch {}

        // Notification
        try
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = ptProfile.UserId,
                Title = "PT verification rejected",
                Content = "Your PT profile verification has been rejected.",
                Type = (int)NotificationType.System,
                IsRead = false,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Notifications.AddAsync(notification);
        }
        catch {}

        await _unitOfWork.SaveChangesAsync();

        var response = new PtProfileResponse
        {
            Id = ptProfile.Id,
            UserId = ptProfile.UserId,
            FullName = ptProfile.User?.FullName ?? string.Empty,
            Bio = ptProfile.Bio,
            Specialization = ptProfile.Specialization,
            ExperienceYears = ptProfile.ExperienceYears,
            VerificationStatus = ptProfile.VerificationStatus,
            AverageRating = ptProfile.AverageRating,
            TotalReviews = ptProfile.TotalReviews
        };

        return ApiResponse<PtProfileResponse>.Ok(response, "PT profile rejected successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<AdminPaymentResponse>>> GetPaymentsAsync()
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<IReadOnlyList<AdminPaymentResponse>>.Fail("Only Admins can perform this action.");
        }

        var payments = await _unitOfWork.Payments.Query()
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var response = payments.Select(p => new AdminPaymentResponse
        {
            Id = p.Id,
            BookingId = p.BookingId,
            Provider = p.Provider,
            OrderCode = p.OrderCode,
            Amount = p.Amount,
            Status = p.Status,
            CreatedAt = p.CreatedAt
        }).ToList();

        return ApiResponse<IReadOnlyList<AdminPaymentResponse>>.Ok(response, "Payments retrieved successfully.");
    }

    public async Task<ApiResponse<AdminPaymentResponse>> GetPaymentByIdAsync(Guid paymentId)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<AdminPaymentResponse>.Fail("Only Admins can perform this action.");
        }

        var p = await _unitOfWork.Payments.Query()
            .FirstOrDefaultAsync(pay => pay.Id == paymentId);

        if (p == null)
        {
            return ApiResponse<AdminPaymentResponse>.Fail("Payment not found.");
        }

        var response = new AdminPaymentResponse
        {
            Id = p.Id,
            BookingId = p.BookingId,
            Provider = p.Provider,
            OrderCode = p.OrderCode,
            Amount = p.Amount,
            Status = p.Status,
            CreatedAt = p.CreatedAt
        };

        return ApiResponse<AdminPaymentResponse>.Ok(response, "Payment retrieved successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<AdminDisputeResponse>>> GetDisputesAsync()
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<IReadOnlyList<AdminDisputeResponse>>.Fail("Only Admins can perform this action.");
        }

        var disputes = await _unitOfWork.Disputes.Query()
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        var response = disputes.Select(d => new AdminDisputeResponse
        {
            Id = d.Id,
            BookingId = d.BookingId,
            CustomerId = d.CustomerId,
            PtProfileId = d.PtProfileId,
            Reason = d.Reason,
            Status = d.Status,
            CreatedAt = d.CreatedAt
        }).ToList();

        return ApiResponse<IReadOnlyList<AdminDisputeResponse>>.Ok(response, "Disputes retrieved successfully.");
    }

    public async Task<ApiResponse<AdminDisputeResponse>> MarkDisputeUnderReviewAsync(Guid disputeId)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Only Admins can perform this action.");
        }

        var dispute = await _unitOfWork.Disputes.Query()
            .FirstOrDefaultAsync(d => d.Id == disputeId);

        if (dispute == null)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Dispute not found.");
        }

        if (dispute.Status != (int)DisputeStatus.Open)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Dispute must be Open to be marked as Under Review.");
        }

        dispute.Status = (int)DisputeStatus.UnderReview;
        dispute.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Disputes.Update(dispute);
        await _unitOfWork.SaveChangesAsync();

        var response = new AdminDisputeResponse
        {
            Id = dispute.Id,
            BookingId = dispute.BookingId,
            CustomerId = dispute.CustomerId,
            PtProfileId = dispute.PtProfileId,
            Reason = dispute.Reason,
            Status = dispute.Status,
            CreatedAt = dispute.CreatedAt
        };

        return ApiResponse<AdminDisputeResponse>.Ok(response, "Dispute marked under review successfully.");
    }

    public async Task<ApiResponse<AdminDisputeResponse>> ResolveRefundCustomerAsync(Guid disputeId, ResolveDisputeRequest request)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Only Admins can perform this action.");
        }

        var dispute = await _unitOfWork.Disputes.Query()
            .FirstOrDefaultAsync(d => d.Id == disputeId);

        if (dispute == null)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Dispute not found.");
        }

        if (dispute.Status != (int)DisputeStatus.Open && dispute.Status != (int)DisputeStatus.UnderReview)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Dispute is already resolved.");
        }

        var booking = await _unitOfWork.Bookings.Query()
            .FirstOrDefaultAsync(b => b.Id == dispute.BookingId);

        if (booking == null)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Associated booking not found.");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            dispute.Status = (int)DisputeStatus.ResolvedRefundCustomer;
            dispute.ResolutionNote = !string.IsNullOrWhiteSpace(request?.ResolutionNote) ? request.ResolutionNote : "Refund customer";
            dispute.ResolvedByAdminId = _currentUserService.UserId!.Value;
            dispute.ResolvedAt = DateTime.UtcNow;
            dispute.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Disputes.Update(dispute);

            booking.Status = (int)BookingStatus.Refunded;
            booking.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Bookings.Update(booking);

            var payment = await _unitOfWork.Payments.Query()
                .FirstOrDefaultAsync(p => p.BookingId == booking.Id && p.Status == (int)PaymentStatus.Success);

            if (payment != null)
            {
                payment.Status = (int)PaymentStatus.Refunded;
                _unitOfWork.Payments.Update(payment);
            }

            var settlement = await _unitOfWork.Settlements.Query()
                .FirstOrDefaultAsync(s => s.BookingId == booking.Id);

            if (settlement != null)
            {
                settlement.Status = (int)SettlementStatus.BlockedByDispute;
                settlement.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Settlements.Update(settlement);
            }

            // Notify Customer: Title = "Dispute resolved", Content = "Your dispute has been resolved with a refund."
            try
            {
                var customerProfile = await _unitOfWork.CustomerProfiles.Query()
                    .FirstOrDefaultAsync(c => c.Id == booking.CustomerId);
                if (customerProfile != null)
                {
                    var notification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = customerProfile.UserId,
                        Title = "Dispute resolved",
                        Content = "Your dispute has been resolved with a refund.",
                        Type = (int)NotificationType.Dispute,
                        IsRead = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Notifications.AddAsync(notification);
                }
            }
            catch {}

            // Audit log
            try
            {
                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    ActorUserId = _currentUserService.UserId!.Value,
                    Action = "ResolveDisputeRefund",
                    EntityName = "Dispute",
                    EntityId = dispute.Id,
                    MetadataJson = System.Text.Json.JsonSerializer.Serialize(new { BookingId = booking.Id, ResolutionNote = dispute.ResolutionNote }),
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.AuditLogs.AddAsync(auditLog);
            }
            catch {}

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ApiResponse<AdminDisputeResponse>.Fail($"Failed to resolve dispute: {ex.Message}");
        }

        var response = new AdminDisputeResponse
        {
            Id = dispute.Id,
            BookingId = dispute.BookingId,
            CustomerId = dispute.CustomerId,
            PtProfileId = dispute.PtProfileId,
            Reason = dispute.Reason,
            Status = dispute.Status,
            CreatedAt = dispute.CreatedAt
        };

        return ApiResponse<AdminDisputeResponse>.Ok(response, "Dispute resolved and customer refunded successfully.");
    }

    public async Task<ApiResponse<AdminDisputeResponse>> ResolveReleaseToPtAsync(Guid disputeId, ResolveDisputeRequest request)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Only Admins can perform this action.");
        }

        var dispute = await _unitOfWork.Disputes.Query()
            .FirstOrDefaultAsync(d => d.Id == disputeId);

        if (dispute == null)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Dispute not found.");
        }

        if (dispute.Status != (int)DisputeStatus.Open && dispute.Status != (int)DisputeStatus.UnderReview)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Dispute is already resolved.");
        }

        var booking = await _unitOfWork.Bookings.Query()
            .FirstOrDefaultAsync(b => b.Id == dispute.BookingId);

        if (booking == null)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Associated booking not found.");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            dispute.Status = (int)DisputeStatus.ResolvedReleaseToPT;
            dispute.ResolutionNote = !string.IsNullOrWhiteSpace(request?.ResolutionNote) ? request.ResolutionNote : "Release to PT";
            dispute.ResolvedByAdminId = _currentUserService.UserId!.Value;
            dispute.ResolvedAt = DateTime.UtcNow;
            dispute.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Disputes.Update(dispute);

            booking.Status = (int)BookingStatus.CompletedPendingSettlement;
            booking.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Bookings.Update(booking);

            var settlement = await _unitOfWork.Settlements.Query()
                .FirstOrDefaultAsync(s => s.BookingId == booking.Id);

            if (settlement != null)
            {
                settlement.Status = (int)SettlementStatus.Pending;
                settlement.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Settlements.Update(settlement);
            }
            else
            {
                var platformFee = booking.TotalAmount * 0.10m;
                var netAmount = booking.TotalAmount - platformFee;

                settlement = new Settlement
                {
                    Id = Guid.NewGuid(),
                    BookingId = booking.Id,
                    PtProfileId = booking.PtProfileId,
                    GrossAmount = booking.TotalAmount,
                    PlatformFee = platformFee,
                    NetAmount = netAmount,
                    Status = (int)SettlementStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Settlements.AddAsync(settlement);
            }

            // Notify PT: Title = "Dispute resolved", Content = "The dispute has been resolved and funds will be released."
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
                        Title = "Dispute resolved",
                        Content = "The dispute has been resolved and funds will be released.",
                        Type = (int)NotificationType.Dispute,
                        IsRead = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Notifications.AddAsync(notification);
                }
            }
            catch {}

            // Audit log
            try
            {
                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    ActorUserId = _currentUserService.UserId!.Value,
                    Action = "ResolveDisputeRelease",
                    EntityName = "Dispute",
                    EntityId = dispute.Id,
                    MetadataJson = System.Text.Json.JsonSerializer.Serialize(new { BookingId = booking.Id, ResolutionNote = dispute.ResolutionNote }),
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.AuditLogs.AddAsync(auditLog);
            }
            catch {}

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ApiResponse<AdminDisputeResponse>.Fail($"Failed to resolve dispute: {ex.Message}");
        }

        var response = new AdminDisputeResponse
        {
            Id = dispute.Id,
            BookingId = dispute.BookingId,
            CustomerId = dispute.CustomerId,
            PtProfileId = dispute.PtProfileId,
            Reason = dispute.Reason,
            Status = dispute.Status,
            CreatedAt = dispute.CreatedAt
        };

        return ApiResponse<AdminDisputeResponse>.Ok(response, "Dispute resolved and funds released to PT successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<AdminSettlementResponse>>> GetSettlementsAsync()
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<IReadOnlyList<AdminSettlementResponse>>.Fail("Only Admins can perform this action.");
        }

        var settlements = await _unitOfWork.Settlements.Query()
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        var response = settlements.Select(s => new AdminSettlementResponse
        {
            Id = s.Id,
            BookingId = s.BookingId,
            PtProfileId = s.PtProfileId,
            GrossAmount = s.GrossAmount,
            PlatformFee = s.PlatformFee,
            NetAmount = s.NetAmount,
            Status = s.Status,
            CreatedAt = s.CreatedAt
        }).ToList();

        return ApiResponse<IReadOnlyList<AdminSettlementResponse>>.Ok(response, "Settlements retrieved successfully.");
    }

    public async Task<ApiResponse<AdminSettlementResponse>> ApproveSettlementAsync(Guid settlementId)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<AdminSettlementResponse>.Fail("Only Admins can perform this action.");
        }

        var settlement = await _unitOfWork.Settlements.Query()
            .FirstOrDefaultAsync(s => s.Id == settlementId);

        if (settlement == null)
        {
            return ApiResponse<AdminSettlementResponse>.Fail("Settlement not found.");
        }

        if (settlement.Status != (int)SettlementStatus.Pending)
        {
            return ApiResponse<AdminSettlementResponse>.Fail("Settlement must be Pending to be approved.");
        }

        var hasOpenDispute = await _unitOfWork.Disputes.Query()
            .AnyAsync(d => d.BookingId == settlement.BookingId && 
                           (d.Status == (int)DisputeStatus.Open || d.Status == (int)DisputeStatus.UnderReview));

        if (hasOpenDispute)
        {
            settlement.Status = (int)SettlementStatus.BlockedByDispute;
            settlement.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Settlements.Update(settlement);
            await _unitOfWork.SaveChangesAsync();

            var blockedResponse = MapToAdminSettlementResponse(settlement);
            return ApiResponse<AdminSettlementResponse>.Fail("Settlement blocked by dispute.");
        }

        settlement.Status = (int)SettlementStatus.Approved;
        settlement.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Settlements.Update(settlement);

        // Notify PT: Title = "Settlement approved", Content = "Your settlement has been approved."
        try
        {
            var ptProfile = await _unitOfWork.PtProfiles.Query()
                .FirstOrDefaultAsync(pt => pt.Id == settlement.PtProfileId);
            if (ptProfile != null)
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = ptProfile.UserId,
                    Title = "Settlement approved",
                    Content = "Your settlement has been approved.",
                    Type = (int)NotificationType.Settlement,
                    IsRead = false,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Notifications.AddAsync(notification);
            }
        }
        catch {}

        // Audit log
        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                ActorUserId = _currentUserService.UserId!.Value,
                Action = "ApproveSettlement",
                EntityName = "Settlement",
                EntityId = settlementId,
                MetadataJson = System.Text.Json.JsonSerializer.Serialize(new { BookingId = settlement.BookingId, NetAmount = settlement.NetAmount }),
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AuditLogs.AddAsync(auditLog);
        }
        catch {}

        await _unitOfWork.SaveChangesAsync();

        var response = MapToAdminSettlementResponse(settlement);
        return ApiResponse<AdminSettlementResponse>.Ok(response, "Settlement approved successfully.");
    }

    public async Task<ApiResponse<AdminSettlementResponse>> MarkSettlementAsSettledAsync(Guid settlementId)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<AdminSettlementResponse>.Fail("Only Admins can perform this action.");
        }

        var settlement = await _unitOfWork.Settlements.Query()
            .FirstOrDefaultAsync(s => s.Id == settlementId);

        if (settlement == null)
        {
            return ApiResponse<AdminSettlementResponse>.Fail("Settlement not found.");
        }

        if (settlement.Status != (int)SettlementStatus.Approved)
        {
            return ApiResponse<AdminSettlementResponse>.Fail("Settlement must be Approved before it can be settled.");
        }

        var booking = await _unitOfWork.Bookings.Query()
            .FirstOrDefaultAsync(b => b.Id == settlement.BookingId);

        if (booking == null)
        {
            return ApiResponse<AdminSettlementResponse>.Fail("Associated booking not found.");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            settlement.Status = (int)SettlementStatus.Settled;
            settlement.SettledAt = DateTime.UtcNow;
            settlement.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Settlements.Update(settlement);

            booking.Status = (int)BookingStatus.Settled;
            booking.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Bookings.Update(booking);

            // Notify PT: Title = "Settlement paid", Content = "Your settlement has been marked as settled."
            try
            {
                var ptProfile = await _unitOfWork.PtProfiles.Query()
                    .FirstOrDefaultAsync(pt => pt.Id == settlement.PtProfileId);
                if (ptProfile != null)
                {
                    var notification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = ptProfile.UserId,
                        Title = "Settlement paid",
                        Content = "Your settlement has been marked as settled.",
                        Type = (int)NotificationType.Settlement,
                        IsRead = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Notifications.AddAsync(notification);
                }
            }
            catch {}

            // Audit log
            try
            {
                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    ActorUserId = _currentUserService.UserId!.Value,
                    Action = "MarkSettlementSettled",
                    EntityName = "Settlement",
                    EntityId = settlementId,
                    MetadataJson = System.Text.Json.JsonSerializer.Serialize(new { BookingId = settlement.BookingId, NetAmount = settlement.NetAmount }),
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.AuditLogs.AddAsync(auditLog);
            }
            catch {}

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ApiResponse<AdminSettlementResponse>.Fail($"Failed to settle: {ex.Message}");
        }

        var response = MapToAdminSettlementResponse(settlement);
        return ApiResponse<AdminSettlementResponse>.Ok(response, "Settlement marked as settled successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<AuditLogResponse>>> GetAuditLogsAsync()
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<IReadOnlyList<AuditLogResponse>>.Fail("Only Admins can perform this action.");
        }

        var logs = await _unitOfWork.AuditLogs.Query()
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        var response = logs.Select(l => new AuditLogResponse
        {
            Id = l.Id,
            ActorUserId = l.ActorUserId,
            Action = l.Action,
            EntityName = l.EntityName,
            EntityId = l.EntityId,
            MetadataJson = l.MetadataJson,
            CreatedAt = l.CreatedAt
        }).ToList();

        return ApiResponse<IReadOnlyList<AuditLogResponse>>.Ok(response, "Audit logs retrieved successfully.");
    }

    #region Helper Methods

    private AdminSettlementResponse MapToAdminSettlementResponse(Settlement settlement)
    {
        return new AdminSettlementResponse
        {
            Id = settlement.Id,
            BookingId = settlement.BookingId,
            PtProfileId = settlement.PtProfileId,
            GrossAmount = settlement.GrossAmount,
            PlatformFee = settlement.PlatformFee,
            NetAmount = settlement.NetAmount,
            Status = settlement.Status,
            CreatedAt = settlement.CreatedAt
        };
    }

    #endregion
}
