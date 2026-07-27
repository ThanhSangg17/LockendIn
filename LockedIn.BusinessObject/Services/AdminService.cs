using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.DataAccess.Models;
using LockedIn.BusinessObject.DTOs.Admin;
using LockedIn.BusinessObject.DTOs.Disputes;
using LockedIn.BusinessObject.DTOs.PtProfile;
using LockedIn.BusinessObject.DTOs.PtProfiles;
using LockedIn.BusinessObject.Enums;

namespace LockedIn.BusinessObject.Services;

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AdminService> _logger;

    public AdminService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ILogger<AdminService> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<ApiResponse<DashboardAnalyticsResponse>> GetDashboardAnalyticsAsync()
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<DashboardAnalyticsResponse>.Fail("Only Admins can perform this action.");
        }

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var twelveMonthsAgo = now.AddMonths(-11);
        var twelveMonthsAgoStart = new DateTime(twelveMonthsAgo.Year, twelveMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // 1. KPIs
        var totalUsers = await _unitOfWork.Users.Query()
            .AsNoTracking()
            .CountAsync(u => !u.IsDeleted);

        var verifiedPTs = await _unitOfWork.PtProfiles.Query()
            .AsNoTracking()
            .CountAsync(pt => pt.VerificationStatus == (int)PtVerificationStatus.Approved && !pt.IsDeleted);

        var monthlyBookings = await _unitOfWork.Bookings.Query()
            .AsNoTracking()
            .CountAsync(b => b.CreatedAt >= startOfMonth);

        var monthlyRevenue = await _unitOfWork.Payments.Query()
            .AsNoTracking()
            .Where(p => p.Status == (int)PaymentStatus.Success && p.PaidAt >= startOfMonth)
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

        var pendingDisputes = await _unitOfWork.Disputes.Query()
            .AsNoTracking()
            .CountAsync(d => d.Status == (int)DisputeStatus.Open || d.Status == (int)DisputeStatus.UnderReview);

        var activeWorkspaces = await _unitOfWork.Workspaces.Query()
            .AsNoTracking()
            .CountAsync(w => w.Status == (int)WorkspaceStatus.Active);

        var kpis = new DashboardKpiDto
        {
            TotalUsers = totalUsers,
            VerifiedPTs = verifiedPTs,
            MonthlyBookings = monthlyBookings,
            MonthlyRevenue = monthlyRevenue,
            PendingDisputes = pendingDisputes,
            ActiveWorkspaces = activeWorkspaces
        };

        // 2. Booking Status Summary
        var bookingStatusCounts = await _unitOfWork.Bookings.Query()
            .AsNoTracking()
            .GroupBy(b => b.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var bookingStatusSummary = new BookingStatusSummaryDto
        {
            PendingPayment = bookingStatusCounts.FirstOrDefault(x => x.Status == (int)BookingStatus.PendingPayment)?.Count ?? 0,
            PendingTrainerAcceptance = bookingStatusCounts.FirstOrDefault(x => x.Status == (int)BookingStatus.PendingTrainerAcceptance)?.Count ?? 0,
            Active = bookingStatusCounts.FirstOrDefault(x => x.Status == (int)BookingStatus.Active)?.Count ?? 0,
            CompletedPendingSettlement = bookingStatusCounts.FirstOrDefault(x => x.Status == (int)BookingStatus.CompletedPendingSettlement)?.Count ?? 0,
            Settled = bookingStatusCounts.FirstOrDefault(x => x.Status == (int)BookingStatus.Settled)?.Count ?? 0,
            Cancelled = bookingStatusCounts.FirstOrDefault(x => x.Status == (int)BookingStatus.Cancelled)?.Count ?? 0,
            Refunded = bookingStatusCounts.FirstOrDefault(x => x.Status == (int)BookingStatus.Refunded)?.Count ?? 0,
            Disputed = bookingStatusCounts.FirstOrDefault(x => x.Status == (int)BookingStatus.Disputed)?.Count ?? 0
        };

        // 3. Dispute Summary
        var disputeStatusCounts = await _unitOfWork.Disputes.Query()
            .AsNoTracking()
            .GroupBy(d => d.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var disputeSummary = new DisputeSummaryDto
        {
            Open = disputeStatusCounts.FirstOrDefault(x => x.Status == (int)DisputeStatus.Open)?.Count ?? 0,
            UnderReview = disputeStatusCounts.FirstOrDefault(x => x.Status == (int)DisputeStatus.UnderReview)?.Count ?? 0,
            ResolvedRefundCustomer = disputeStatusCounts.FirstOrDefault(x => x.Status == (int)DisputeStatus.ResolvedRefundCustomer)?.Count ?? 0,
            ResolvedReleaseToPT = disputeStatusCounts.FirstOrDefault(x => x.Status == (int)DisputeStatus.ResolvedReleaseToPT)?.Count ?? 0,
            Withdrawn = disputeStatusCounts.FirstOrDefault(x => x.Status == (int)DisputeStatus.Withdrawn)?.Count ?? 0
        };

        // 4. New Users By Month
        var newUsersByMonthData = await _unitOfWork.Users.Query()
            .AsNoTracking()
            .Where(u => !u.IsDeleted && u.CreatedAt >= twelveMonthsAgoStart)
            .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Count = g.Count()
            })
            .ToListAsync();

        var newUsersByMonth = new List<MonthlyUserRegistrationDto>();
        for (int i = 11; i >= 0; i--)
        {
            var targetDate = now.AddMonths(-i);
            var year = targetDate.Year;
            var month = targetDate.Month;
            var monthStr = $"{year}-{month:D2}";

            var dbCount = newUsersByMonthData.FirstOrDefault(x => x.Year == year && x.Month == month)?.Count ?? 0;

            newUsersByMonth.Add(new MonthlyUserRegistrationDto
            {
                Month = monthStr,
                Count = dbCount
            });
        }

        // 5. Top Revenue Trainers
        var topRevenueTrainers = await _unitOfWork.Payments.Query()
            .AsNoTracking()
            .Where(p => p.Status == (int)PaymentStatus.Success && !p.Booking.PtProfile.IsDeleted && !p.Booking.PtProfile.User.IsDeleted)
            .GroupBy(p => new
            {
                PtId = p.Booking.PtProfileId,
                FullName = p.Booking.PtProfile.User.FullName,
                AvatarUrl = p.Booking.PtProfile.User.AvatarUrl
            })
            .Select(g => new TopRevenueTrainerDto
            {
                PtId = g.Key.PtId,
                FullName = g.Key.FullName,
                AvatarUrl = g.Key.AvatarUrl,
                Revenue = g.Sum(p => p.Amount),
                BookingCount = g.Select(p => p.BookingId).Distinct().Count()
            })
            .OrderByDescending(t => t.Revenue)
            .Take(10)
            .ToListAsync();

        // 6. Top Booked Packages
        var topBookedPackages = await _unitOfWork.Bookings.Query()
            .AsNoTracking()
            .Where(b => !b.Package.IsDeleted && !b.Package.PtProfile.IsDeleted && !b.Package.PtProfile.User.IsDeleted)
            .GroupBy(b => new
            {
                PackageId = b.PackageId,
                PackageName = b.Package.Name,
                TrainerName = b.Package.PtProfile.User.FullName
            })
            .Select(g => new TopBookedPackageDto
            {
                PackageId = g.Key.PackageId,
                PackageName = g.Key.PackageName,
                TrainerName = g.Key.TrainerName,
                BookingCount = g.Count()
            })
            .OrderByDescending(p => p.BookingCount)
            .Take(10)
            .ToListAsync();

        var response = new DashboardAnalyticsResponse
        {
            Kpis = kpis,
            BookingStatusSummary = bookingStatusSummary,
            DisputeSummary = disputeSummary,
            NewUsersByMonth = newUsersByMonth,
            TopRevenueTrainers = topRevenueTrainers,
            TopBookedPackages = topBookedPackages
        };

        return ApiResponse<DashboardAnalyticsResponse>.Ok(response, "Dashboard analytics retrieved successfully.");
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
            .Where(pt => pt.VerificationStatus == (int)PtVerificationStatus.Submitted && !pt.IsDeleted)
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

    public async Task<ApiResponse<PtVerificationDetailResponse>> GetPtVerificationByIdAsync(Guid ptProfileId)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<PtVerificationDetailResponse>.Fail("Only Admins can perform this action.");
        }

        var ptProfile = await _unitOfWork.PtProfiles.Query()
            .Include(pt => pt.User)
            .Include(pt => pt.PtDocuments)
            .FirstOrDefaultAsync(pt => pt.Id == ptProfileId && !pt.IsDeleted);

        if (ptProfile == null)
        {
            return ApiResponse<PtVerificationDetailResponse>.Fail("PT profile not found.");
        }

        var profileResponse = new PtProfileResponse
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

        var documentsResponse = ptProfile.PtDocuments.Select(d => new PtDocumentResponse
        {
            Id = d.Id,
            PtProfileId = d.PtProfileId,
            DocumentType = d.DocumentType,
            FileUrl = d.FileUrl,
            Status = d.Status,
            UploadedAt = d.UploadedAt
        }).ToList();

        var response = new PtVerificationDetailResponse
        {
            Profile = profileResponse,
            Email = ptProfile.User?.Email ?? string.Empty,
            Phone = ptProfile.User?.Phone ?? string.Empty,
            Documents = documentsResponse
        };

        return ApiResponse<PtVerificationDetailResponse>.Ok(response, "PT verification details retrieved successfully.");
    }

    public async Task<ApiResponse<PtProfileResponse>> ApprovePtAsync(Guid ptProfileId)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<PtProfileResponse>.Fail("Only Admins can perform this action.");
        }

        var ptProfile = await _unitOfWork.PtProfiles.Query()
            .Include(pt => pt.User)
            .Include(pt => pt.PtDocuments)
            .FirstOrDefaultAsync(pt => pt.Id == ptProfileId && !pt.IsDeleted);

        if (ptProfile == null)
        {
            return ApiResponse<PtProfileResponse>.Fail("PT profile not found.");
        }

        if (ptProfile.VerificationStatus != (int)PtVerificationStatus.Submitted)
        {
            return ApiResponse<PtProfileResponse>.Fail("Only submitted profiles can be approved.");
        }

        ptProfile.VerificationStatus = (int)PtVerificationStatus.Approved;
        ptProfile.ApprovedAt = DateTime.UtcNow;
        ptProfile.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.PtProfiles.Update(ptProfile);

        // Approve all documents
        foreach (var document in ptProfile.PtDocuments)
        {
            document.Status = 2; // Approved
            _unitOfWork.PtDocuments.Update(document);
        }

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
                Content = "Your PT profile has been approved. You can now create packages and receive bookings.",
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

    public async Task<ApiResponse<PtProfileResponse>> RejectPtAsync(Guid ptProfileId, RejectPtRequest request)
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

        if (ptProfile.VerificationStatus != (int)PtVerificationStatus.Submitted)
        {
            return ApiResponse<PtProfileResponse>.Fail("Only submitted profiles can be rejected.");
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
                MetadataJson = System.Text.Json.JsonSerializer.Serialize(new { Email = ptProfile.User?.Email, Reason = request.Reason }),
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
                Content = $"Your PT profile verification has been rejected. Reason: {request.Reason}",
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
            .Include(d => d.Customer).ThenInclude(c => c.User)
            .Include(d => d.PtProfile).ThenInclude(p => p.User)
            .Include(d => d.Booking).ThenInclude(b => b.Package)
            .Include(d => d.DisputeEvidences)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        var response = disputes.Select(MapToAdminDisputeResponse).ToList();
        return ApiResponse<IReadOnlyList<AdminDisputeResponse>>.Ok(response, "Disputes retrieved successfully.");
    }

    public async Task<ApiResponse<AdminDisputeResponse>> GetDisputeByIdAsync(Guid disputeId)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Only Admins can perform this action.");
        }

        var dispute = await _unitOfWork.Disputes.Query()
            .Include(d => d.Customer).ThenInclude(c => c.User)
            .Include(d => d.PtProfile).ThenInclude(p => p.User)
            .Include(d => d.Booking).ThenInclude(b => b.Package)
            .Include(d => d.DisputeEvidences)
            .FirstOrDefaultAsync(d => d.Id == disputeId);

        if (dispute == null)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Dispute not found.");
        }

        var response = MapToAdminDisputeResponse(dispute);
        return ApiResponse<AdminDisputeResponse>.Ok(response, "Dispute detail retrieved successfully.");
    }

    public async Task<ApiResponse<AdminDisputeResponse>> MarkDisputeUnderReviewAsync(Guid disputeId)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Only Admins can perform this action.");
        }

        var dispute = await _unitOfWork.Disputes.Query()
            .Include(d => d.Customer).ThenInclude(c => c.User)
            .Include(d => d.PtProfile).ThenInclude(p => p.User)
            .Include(d => d.Booking).ThenInclude(b => b.Package)
            .Include(d => d.DisputeEvidences)
            .FirstOrDefaultAsync(d => d.Id == disputeId);

        if (dispute == null)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Dispute not found.");
        }

        if (dispute.Status == (int)DisputeStatus.Withdrawn)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Dispute has been withdrawn and cannot be processed.");
        }

        if (dispute.Status != (int)DisputeStatus.Open)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Dispute must be Open to be marked as Under Review.");
        }

        dispute.Status = (int)DisputeStatus.UnderReview;
        dispute.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Disputes.Update(dispute);
        await _unitOfWork.SaveChangesAsync();

        var response = MapToAdminDisputeResponse(dispute);
        return ApiResponse<AdminDisputeResponse>.Ok(response, "Dispute marked under review successfully.");
    }

    public async Task<ApiResponse<AdminDisputeResponse>> ResolveRefundCustomerAsync(Guid disputeId, ResolveDisputeRequest request)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Only Admins can perform this action.");
        }

        var dispute = await _unitOfWork.Disputes.Query()
            .Include(d => d.Customer).ThenInclude(c => c.User)
            .Include(d => d.PtProfile).ThenInclude(p => p.User)
            .Include(d => d.Booking).ThenInclude(b => b.Package)
            .Include(d => d.DisputeEvidences)
            .FirstOrDefaultAsync(d => d.Id == disputeId);

        if (dispute == null)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Dispute not found.");
        }

        if (dispute.Status == (int)DisputeStatus.Withdrawn)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Dispute has been withdrawn and cannot be processed.");
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create refund resolution notification for dispute {DisputeId}", dispute.Id);
            }

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create audit log for refund resolution of dispute {DisputeId}", dispute.Id);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ApiResponse<AdminDisputeResponse>.Fail($"Failed to resolve dispute: {ex.Message}");
        }

        var response = MapToAdminDisputeResponse(dispute);
        return ApiResponse<AdminDisputeResponse>.Ok(response, "Dispute resolved and customer refunded successfully.");
    }

    public async Task<ApiResponse<AdminDisputeResponse>> ResolveReleaseToPtAsync(Guid disputeId, ResolveDisputeRequest request)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Only Admins can perform this action.");
        }

        var dispute = await _unitOfWork.Disputes.Query()
            .Include(d => d.Customer).ThenInclude(c => c.User)
            .Include(d => d.PtProfile).ThenInclude(p => p.User)
            .Include(d => d.Booking).ThenInclude(b => b.Package)
            .Include(d => d.DisputeEvidences)
            .FirstOrDefaultAsync(d => d.Id == disputeId);

        if (dispute == null)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Dispute not found.");
        }

        if (dispute.Status == (int)DisputeStatus.Withdrawn)
        {
            return ApiResponse<AdminDisputeResponse>.Fail("Dispute has been withdrawn and cannot be processed.");
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create PT release resolution notification for dispute {DisputeId}", dispute.Id);
            }

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create audit log for PT release resolution of dispute {DisputeId}", dispute.Id);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ApiResponse<AdminDisputeResponse>.Fail($"Failed to resolve dispute: {ex.Message}");
        }

        var response = MapToAdminDisputeResponse(dispute);
        return ApiResponse<AdminDisputeResponse>.Ok(response, "Dispute resolved and released to PT successfully.");
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

    public async Task<ApiResponse<IReadOnlyList<ProfileEditRequestResponse>>> GetPtProfileEditRequestsAsync(int? status = null)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
            return ApiResponse<IReadOnlyList<ProfileEditRequestResponse>>.Fail("Only Admins can perform this action.");

        var query = _unitOfWork.PtProfileEditRequests.Query();
        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        var requests = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        var response = requests.Select(MapToProfileEditRequestResponse).ToList();

        return ApiResponse<IReadOnlyList<ProfileEditRequestResponse>>.Ok(response, "PT profile edit requests retrieved successfully.");
    }

    public async Task<ApiResponse<ProfileEditRequestResponse>> GetPtProfileEditRequestByIdAsync(Guid requestId)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
            return ApiResponse<ProfileEditRequestResponse>.Fail("Only Admins can perform this action.");

        var request = await _unitOfWork.PtProfileEditRequests.Query()
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null)
            return ApiResponse<ProfileEditRequestResponse>.Fail("Request not found.");

        return ApiResponse<ProfileEditRequestResponse>.Ok(MapToProfileEditRequestResponse(request), "Request retrieved successfully.");
    }

    public async Task<ApiResponse<ProfileEditRequestResponse>> ApprovePtProfileEditRequestAsync(Guid requestId)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
            return ApiResponse<ProfileEditRequestResponse>.Fail("Only Admins can perform this action.");

        var request = await _unitOfWork.PtProfileEditRequests.Query()
            .Include(r => r.PtProfile)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null)
            return ApiResponse<ProfileEditRequestResponse>.Fail("Request not found.");

        if (request.Status != (int)PtProfileEditRequestStatus.Pending)
            return ApiResponse<ProfileEditRequestResponse>.Fail("Only pending requests can be approved.");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            request.Status = (int)PtProfileEditRequestStatus.Approved;
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedByAdminId = _currentUserService.UserId;
            request.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.PtProfileEditRequests.Update(request);

            var profile = request.PtProfile;
            profile.Bio = request.RequestedBio;
            profile.Specialization = request.RequestedSpecialization;
            profile.ExperienceYears = request.RequestedExperienceYears;
            profile.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.PtProfiles.Update(profile);

            try
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = profile.UserId,
                    Title = "Profile Edit Approved",
                    Content = "Your profile edit request has been approved.",
                    Type = (int)NotificationType.System,
                    IsRead = false,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Notifications.AddAsync(notification);

                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    ActorUserId = _currentUserService.UserId!.Value,
                    Action = "ApproveProfileEdit",
                    EntityName = "PtProfileEditRequest",
                    EntityId = requestId,
                    MetadataJson = "{}",
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.AuditLogs.AddAsync(auditLog);
            }
            catch {}

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return ApiResponse<ProfileEditRequestResponse>.Ok(MapToProfileEditRequestResponse(request), "Request approved successfully.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ApiResponse<ProfileEditRequestResponse>.Fail($"Failed to approve request: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ProfileEditRequestResponse>> RejectPtProfileEditRequestAsync(Guid requestId, RejectProfileEditRequest rejectRequest)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
            return ApiResponse<ProfileEditRequestResponse>.Fail("Only Admins can perform this action.");

        var request = await _unitOfWork.PtProfileEditRequests.Query()
            .Include(r => r.PtProfile)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null)
            return ApiResponse<ProfileEditRequestResponse>.Fail("Request not found.");

        if (request.Status != (int)PtProfileEditRequestStatus.Pending)
            return ApiResponse<ProfileEditRequestResponse>.Fail("Only pending requests can be rejected.");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            request.Status = (int)PtProfileEditRequestStatus.Rejected;
            request.RejectionReason = rejectRequest.Reason;
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedByAdminId = _currentUserService.UserId;
            request.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.PtProfileEditRequests.Update(request);

            try
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = request.PtProfile.UserId,
                    Title = "Profile Edit Rejected",
                    Content = $"Your profile edit request was rejected. Reason: {rejectRequest.Reason}",
                    Type = (int)NotificationType.System,
                    IsRead = false,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Notifications.AddAsync(notification);

                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    ActorUserId = _currentUserService.UserId!.Value,
                    Action = "RejectProfileEdit",
                    EntityName = "PtProfileEditRequest",
                    EntityId = requestId,
                    MetadataJson = System.Text.Json.JsonSerializer.Serialize(new { Reason = rejectRequest.Reason }),
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.AuditLogs.AddAsync(auditLog);
            }
            catch {}

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return ApiResponse<ProfileEditRequestResponse>.Ok(MapToProfileEditRequestResponse(request), "Request rejected successfully.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ApiResponse<ProfileEditRequestResponse>.Fail($"Failed to reject request: {ex.Message}");
        }
    }

    public async Task<ApiResponse<AddonProductResponse>> CreateAddonProductAsync(CreateAddonProductRequest request)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<AddonProductResponse>.Fail("Only Admins can perform this action.");
        }

        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return ApiResponse<AddonProductResponse>.Fail("Product code is required.");
        }
        var codeUpper = request.Code.Trim().ToUpper();
        if (!System.Text.RegularExpressions.Regex.IsMatch(codeUpper, "^[A-Z0-9_]+$"))
        {
            return ApiResponse<AddonProductResponse>.Fail("Product code can only contain uppercase letters (A-Z), numbers (0-9), and underscores (_).");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ApiResponse<AddonProductResponse>.Fail("Product name is required.");
        }
        var nameTrimmed = request.Name.Trim();

        if (request.ProductType != (int)AddonProductType.Credit && request.ProductType != (int)AddonProductType.TimeBasedEntitlement)
        {
            return ApiResponse<AddonProductResponse>.Fail("Invalid product type.");
        }

        if (request.ProductType == (int)AddonProductType.Credit)
        {
            if (!request.GrantQuantity.HasValue || request.GrantQuantity.Value <= 0)
            {
                return ApiResponse<AddonProductResponse>.Fail("Grant quantity must be greater than 0 for credit product type.");
            }
            if (request.DurationDays.HasValue)
            {
                return ApiResponse<AddonProductResponse>.Fail("Duration days must be null for credit product type.");
            }
        }
        else if (request.ProductType == (int)AddonProductType.TimeBasedEntitlement)
        {
            if (!request.DurationDays.HasValue || request.DurationDays.Value <= 0)
            {
                return ApiResponse<AddonProductResponse>.Fail("Duration days must be greater than 0 for time-based entitlement product type.");
            }
            if (request.GrantQuantity.HasValue)
            {
                return ApiResponse<AddonProductResponse>.Fail("Grant quantity must be null for time-based entitlement product type.");
            }
        }

        var exists = await _unitOfWork.AddonProducts.Query().AnyAsync(p => p.Code == codeUpper);
        if (exists)
        {
            return ApiResponse<AddonProductResponse>.Fail($"Add-on product with code '{codeUpper}' already exists.");
        }

        var product = new AddonProduct
        {
            Id = Guid.NewGuid(),
            Code = codeUpper,
            Name = nameTrimmed,
            Description = request.Description?.Trim(),
            ProductType = request.ProductType,
            GrantQuantity = request.ProductType == (int)AddonProductType.Credit ? request.GrantQuantity : null,
            DurationDays = request.ProductType == (int)AddonProductType.TimeBasedEntitlement ? request.DurationDays : null,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.AddonProducts.AddAsync(product);

        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                ActorUserId = _currentUserService.UserId!.Value,
                Action = "CreateAddonProduct",
                EntityName = "AddonProduct",
                EntityId = product.Id,
                MetadataJson = System.Text.Json.JsonSerializer.Serialize(new { Code = product.Code, Name = product.Name }),
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AuditLogs.AddAsync(auditLog);
        }
        catch {}

        await _unitOfWork.SaveChangesAsync();

        var createdProduct = await _unitOfWork.AddonProducts.Query()
            .Include(p => p.AddonProductPrices)
            .FirstAsync(p => p.Id == product.Id);

        return ApiResponse<AddonProductResponse>.Ok(MapToAddonProductResponse(createdProduct), "Add-on product created successfully.");
    }

    public async Task<ApiResponse<PagedResult<AddonProductResponse>>> GetAddonProductsAsync(PaginationRequest request, string? search = null, int? productType = null, bool? isActive = null)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<PagedResult<AddonProductResponse>>.Fail("Only Admins can perform this action.");
        }

        var query = _unitOfWork.AddonProducts.Query()
            .Include(p => p.AddonProductPrices)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lowerSearch = search.ToLower();
            query = query.Where(p => p.Code.ToLower().Contains(lowerSearch) || p.Name.ToLower().Contains(lowerSearch) || (p.Description != null && p.Description.ToLower().Contains(lowerSearch)));
        }
        if (productType.HasValue)
        {
            query = query.Where(p => p.ProductType == productType.Value);
        }
        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var response = products.Select(MapToAddonProductResponse).ToList();
        var pagedResult = new PagedResult<AddonProductResponse>
        {
            Items = response,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };

        return ApiResponse<PagedResult<AddonProductResponse>>.Ok(pagedResult, "Add-on products retrieved successfully.");
    }

    public async Task<ApiResponse<AddonProductResponse>> GetAddonProductByIdAsync(Guid productId)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<AddonProductResponse>.Fail("Only Admins can perform this action.");
        }

        var product = await _unitOfWork.AddonProducts.Query()
            .Include(p => p.AddonProductPrices)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null)
        {
            return ApiResponse<AddonProductResponse>.Fail("Add-on product not found.");
        }

        return ApiResponse<AddonProductResponse>.Ok(MapToAddonProductResponse(product), "Add-on product retrieved successfully.");
    }

    public async Task<ApiResponse<AddonProductResponse>> ActivateAddonProductAsync(Guid productId)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<AddonProductResponse>.Fail("Only Admins can perform this action.");
        }

        var product = await _unitOfWork.AddonProducts.Query()
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null)
        {
            return ApiResponse<AddonProductResponse>.Fail("Add-on product not found.");
        }

        if (product.IsActive)
        {
            var activeProduct = await _unitOfWork.AddonProducts.Query()
                .Include(p => p.AddonProductPrices)
                .FirstAsync(p => p.Id == productId);
            return ApiResponse<AddonProductResponse>.Ok(MapToAddonProductResponse(activeProduct), "Add-on product is already active.");
        }

        product.IsActive = true;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.AddonProducts.Update(product);

        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                ActorUserId = _currentUserService.UserId!.Value,
                Action = "ActivateAddonProduct",
                EntityName = "AddonProduct",
                EntityId = product.Id,
                MetadataJson = System.Text.Json.JsonSerializer.Serialize(new { Code = product.Code, Name = product.Name }),
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AuditLogs.AddAsync(auditLog);
        }
        catch {}

        await _unitOfWork.SaveChangesAsync();

        var updatedProduct = await _unitOfWork.AddonProducts.Query()
            .Include(p => p.AddonProductPrices)
            .FirstAsync(p => p.Id == productId);

        return ApiResponse<AddonProductResponse>.Ok(MapToAddonProductResponse(updatedProduct), "Add-on product activated successfully.");
    }

    public async Task<ApiResponse<AddonProductResponse>> DeactivateAddonProductAsync(Guid productId)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<AddonProductResponse>.Fail("Only Admins can perform this action.");
        }

        var product = await _unitOfWork.AddonProducts.Query()
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null)
        {
            return ApiResponse<AddonProductResponse>.Fail("Add-on product not found.");
        }

        if (!product.IsActive)
        {
            var inactiveProduct = await _unitOfWork.AddonProducts.Query()
                .Include(p => p.AddonProductPrices)
                .FirstAsync(p => p.Id == productId);
            return ApiResponse<AddonProductResponse>.Ok(MapToAddonProductResponse(inactiveProduct), "Add-on product is already inactive.");
        }

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.AddonProducts.Update(product);

        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                ActorUserId = _currentUserService.UserId!.Value,
                Action = "DeactivateAddonProduct",
                EntityName = "AddonProduct",
                EntityId = product.Id,
                MetadataJson = System.Text.Json.JsonSerializer.Serialize(new { Code = product.Code, Name = product.Name }),
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AuditLogs.AddAsync(auditLog);
        }
        catch {}

        await _unitOfWork.SaveChangesAsync();

        var updatedProduct = await _unitOfWork.AddonProducts.Query()
            .Include(p => p.AddonProductPrices)
            .FirstAsync(p => p.Id == productId);

        return ApiResponse<AddonProductResponse>.Ok(MapToAddonProductResponse(updatedProduct), "Add-on product deactivated successfully.");
    }

    public async Task<ApiResponse<AddonProductPriceResponse>> CreateAddonProductPriceAsync(Guid productId, CreateAddonProductPriceRequest request)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<AddonProductPriceResponse>.Fail("Only Admins can perform this action.");
        }

        var product = await _unitOfWork.AddonProducts.Query().FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null)
        {
            return ApiResponse<AddonProductPriceResponse>.Fail("Add-on product not found.");
        }

        if (request.UnitAmount < 0)
        {
            return ApiResponse<AddonProductPriceResponse>.Fail("Price unit amount cannot be negative.");
        }
        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            return ApiResponse<AddonProductPriceResponse>.Fail("Currency is required.");
        }

        var currentAdminId = _currentUserService.UserId!.Value;

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var activePrices = await _unitOfWork.AddonProductPrices.Query()
                .Where(p => p.ProductId == productId && p.IsActive)
                .ToListAsync();
            foreach (var ap in activePrices)
            {
                ap.IsActive = false;
                ap.DeactivatedAt = DateTime.UtcNow;
                _unitOfWork.AddonProductPrices.Update(ap);
            }

            var newPrice = new AddonProductPrice
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                UnitAmount = request.UnitAmount,
                Currency = request.Currency.Trim().ToUpper(),
                IsActive = true,
                CreatedByAdminId = currentAdminId,
                CreatedAt = DateTime.UtcNow,
                DeactivatedAt = null
            };

            await _unitOfWork.AddonProductPrices.AddAsync(newPrice);

            try
            {
                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    ActorUserId = currentAdminId,
                    Action = "CreateAddonProductPrice",
                    EntityName = "AddonProductPrice",
                    EntityId = newPrice.Id,
                    MetadataJson = System.Text.Json.JsonSerializer.Serialize(new { ProductId = productId, UnitAmount = newPrice.UnitAmount, Currency = newPrice.Currency }),
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.AuditLogs.AddAsync(auditLog);
            }
            catch {}

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return ApiResponse<AddonProductPriceResponse>.Ok(MapToAddonProductPriceResponse(newPrice), "Add-on product price created successfully.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ApiResponse<AddonProductPriceResponse>.Fail($"Failed to create price: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IReadOnlyList<AddonProductPriceResponse>>> GetAddonProductPricesAsync(Guid productId)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<IReadOnlyList<AddonProductPriceResponse>>.Fail("Only Admins can perform this action.");
        }

        var productExists = await _unitOfWork.AddonProducts.Query().AnyAsync(p => p.Id == productId);
        if (!productExists)
        {
            return ApiResponse<IReadOnlyList<AddonProductPriceResponse>>.Fail("Add-on product not found.");
        }

        var prices = await _unitOfWork.AddonProductPrices.Query()
            .Where(p => p.ProductId == productId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var response = prices.Select(MapToAddonProductPriceResponse).ToList();
        return ApiResponse<IReadOnlyList<AddonProductPriceResponse>>.Ok(response, "Add-on product prices retrieved successfully.");
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

    private ProfileEditRequestResponse MapToProfileEditRequestResponse(PtProfileEditRequest request)
    {
        return new ProfileEditRequestResponse
        {
            Id = request.Id,
            PtProfileId = request.PtProfileId,
            CurrentBio = request.CurrentBio,
            CurrentSpecialization = request.CurrentSpecialization,
            CurrentExperienceYears = request.CurrentExperienceYears,
            RequestedBio = request.RequestedBio,
            RequestedSpecialization = request.RequestedSpecialization,
            RequestedExperienceYears = request.RequestedExperienceYears,
            Status = request.Status,
            RejectionReason = request.RejectionReason,
            RequestedAt = request.RequestedAt,
            ReviewedAt = request.ReviewedAt,
            ReviewedByAdminId = request.ReviewedByAdminId
        };
    }

    private AddonProductResponse MapToAddonProductResponse(AddonProduct product)
    {
        return new AddonProductResponse
        {
            Id = product.Id,
            Code = product.Code,
            Name = product.Name,
            Description = product.Description,
            ProductType = product.ProductType,
            GrantQuantity = product.GrantQuantity,
            DurationDays = product.DurationDays,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            Prices = product.AddonProductPrices != null 
                ? product.AddonProductPrices.Select(MapToAddonProductPriceResponse).ToList()
                : new List<AddonProductPriceResponse>()
        };
    }

    private AddonProductPriceResponse MapToAddonProductPriceResponse(AddonProductPrice price)
    {
        return new AddonProductPriceResponse
        {
            Id = price.Id,
            ProductId = price.ProductId,
            UnitAmount = price.UnitAmount,
            Currency = price.Currency,
            IsActive = price.IsActive,
            CreatedByAdminId = price.CreatedByAdminId,
            CreatedAt = price.CreatedAt,
            DeactivatedAt = price.DeactivatedAt
        };
    }

    private AdminDisputeResponse MapToAdminDisputeResponse(Dispute d)
    {
        return new AdminDisputeResponse
        {
            Id = d.Id,
            BookingId = d.BookingId,
            CustomerId = d.CustomerId,
            PtProfileId = d.PtProfileId,
            Reason = d.Reason,
            Description = d.Description,
            Status = d.Status,
            ResolutionNote = d.ResolutionNote,
            ResolvedByAdminId = d.ResolvedByAdminId,
            ResolvedAt = d.ResolvedAt,
            CreatedAt = d.CreatedAt,
            CustomerName = d.Customer?.User?.FullName,
            CustomerEmail = d.Customer?.User?.Email,
            PtName = d.PtProfile?.User?.FullName,
            PtEmail = d.PtProfile?.User?.Email,
            PackageName = d.Booking?.Package?.Name,
            BookingAmount = d.Booking?.TotalAmount,
            BookingStatus = d.Booking?.Status,
            BookingStartDate = d.Booking?.StartedAt,
            BookingCompletedAt = d.Booking?.CompletedAt,
            Evidences = d.DisputeEvidences?.Select(e => new DisputeEvidenceResponse
            {
                Id = e.Id,
                DisputeId = e.DisputeId,
                FileUrl = e.FileUrl,
                FileType = e.FileType,
                UploadedAt = e.UploadedAt
            }).ToList() ?? new List<DisputeEvidenceResponse>()
        };
    }

    #endregion
}
