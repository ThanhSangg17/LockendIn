using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.DataAccess.Models;
using LockedIn.BusinessObject.DTOs.Disputes;
using LockedIn.BusinessObject.Enums;

namespace LockedIn.BusinessObject.Services;

public class DisputeService : IDisputeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DisputeService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<DisputeResponse>> CreateDisputeAsync(CreateDisputeRequest request)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<DisputeResponse>.Fail("User is not authenticated.");
        }

        if (_currentUserService.Role != (int)UserRole.Customer)
        {
            return ApiResponse<DisputeResponse>.Fail("Only customers can create disputes.");
        }

        request.Reason = request.Reason?.Trim()!;
        request.Description = request.Description?.Trim()!;

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return ApiResponse<DisputeResponse>.Fail("Reason is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return ApiResponse<DisputeResponse>.Fail("Description is required.");
        }

        if (request.Reason.Length > 255)
        {
            return ApiResponse<DisputeResponse>.Fail("Reason cannot exceed 255 characters.");
        }

        if (request.Description.Length > 2000)
        {
            return ApiResponse<DisputeResponse>.Fail("Description cannot exceed 2000 characters.");
        }

        if (request.Evidences == null || !request.Evidences.Any())
        {
            return ApiResponse<DisputeResponse>.Fail("At least one evidence is required.");
        }

        foreach (var evReq in request.Evidences)
        {
            if (evReq == null || string.IsNullOrWhiteSpace(evReq.FileUrl))
            {
                return ApiResponse<DisputeResponse>.Fail("Evidence file URL cannot be null, empty or whitespace.");
            }
        }

        var customerProfile = await GetCurrentCustomerProfileAsync();
        if (customerProfile == null)
        {
            return ApiResponse<DisputeResponse>.Fail("Customer profile not found.");
        }

        var booking = await _unitOfWork.Bookings.Query()
            .FirstOrDefaultAsync(b => b.Id == request.BookingId);

        if (booking == null)
        {
            return ApiResponse<DisputeResponse>.Fail("Booking not found.");
        }

        if (booking.CustomerId != customerProfile.Id)
        {
            return ApiResponse<DisputeResponse>.Fail("You do not own this booking.");
        }

        if (booking.Status != (int)BookingStatus.Active && booking.Status != (int)BookingStatus.CompletedPendingSettlement)
        {
            return ApiResponse<DisputeResponse>.Fail("Disputes can only be created for active or completed bookings.");
        }

        if (booking.Status == (int)BookingStatus.CompletedPendingSettlement)
        {
            if (!booking.CompletedAt.HasValue)
            {
                return ApiResponse<DisputeResponse>.Fail("Booking completion timestamp is missing.");
            }
            if (DateTime.UtcNow > booking.CompletedAt.Value.AddDays(7))
            {
                return ApiResponse<DisputeResponse>.Fail("Disputes can only be created within 7 days of booking completion.");
            }
        }

        var existingDisputes = await _unitOfWork.Disputes.Query()
            .Where(d => d.BookingId == booking.Id)
            .ToListAsync();

        var hasFinalDispute = existingDisputes.Any(d => 
            d.Status == (int)DisputeStatus.ResolvedReleaseToPT || 
            d.Status == (int)DisputeStatus.ResolvedRefundCustomer);

        if (hasFinalDispute)
        {
            return ApiResponse<DisputeResponse>.Fail("A final decision has already been made for this booking's dispute.");
        }

        var hasActiveDispute = existingDisputes.Any(d => 
            d.Status == (int)DisputeStatus.Open || 
            d.Status == (int)DisputeStatus.UnderReview);

        if (hasActiveDispute)
        {
            return ApiResponse<DisputeResponse>.Fail("This booking already has an active dispute.");
        }

        var settlement = await _unitOfWork.Settlements.Query()
            .FirstOrDefaultAsync(s => s.BookingId == booking.Id);

        var dispute = new Dispute
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            CustomerId = booking.CustomerId,
            PtProfileId = booking.PtProfileId,
            Reason = request.Reason,
            Description = request.Description,
            Status = (int)DisputeStatus.Open,
            OriginalBookingStatus = booking.Status,
            OriginalSettlementStatus = settlement?.Status,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _unitOfWork.Disputes.AddAsync(dispute);

            foreach (var evReq in request.Evidences)
            {
                var evidence = new DisputeEvidence
                {
                    Id = Guid.NewGuid(),
                    DisputeId = dispute.Id,
                    FileUrl = evReq.FileUrl,
                    FileType = evReq.FileType ?? "Image",
                    UploadedAt = DateTime.UtcNow
                };
                await _unitOfWork.DisputeEvidences.AddAsync(evidence);
            }

            booking.Status = (int)BookingStatus.Disputed;
            booking.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Bookings.Update(booking);

            if (settlement != null && (settlement.Status == (int)SettlementStatus.Pending || settlement.Status == (int)SettlementStatus.Approved))
            {
                settlement.Status = (int)SettlementStatus.BlockedByDispute;
                settlement.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Settlements.Update(settlement);
            }

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
                        Title = "New dispute",
                        Content = "A customer has opened a dispute.",
                        Type = (int)NotificationType.Dispute,
                        IsRead = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Notifications.AddAsync(notification);
                }
            }
            catch
            {
                // silently ignore
            }

            try
            {
                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    ActorUserId = _currentUserService.UserId!.Value,
                    Action = "CreateDispute",
                    EntityName = "Dispute",
                    EntityId = dispute.Id,
                    MetadataJson = System.Text.Json.JsonSerializer.Serialize(new { BookingId = booking.Id, Reason = request.Reason }),
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.AuditLogs.AddAsync(auditLog);
            }
            catch
            {
                // silently ignore
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ApiResponse<DisputeResponse>.Fail($"Failed to create dispute: {ex.Message}");
        }

        var response = MapToDisputeResponse(dispute);
        return ApiResponse<DisputeResponse>.Ok(response, "Dispute created successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<DisputeResponse>>> GetMyDisputesAsync()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<IReadOnlyList<DisputeResponse>>.Fail("User is not authenticated.");
        }

        IQueryable<Dispute> query = _unitOfWork.Disputes.Query();

        if (_currentUserService.Role == (int)UserRole.Customer)
        {
            var customer = await GetCurrentCustomerProfileAsync();
            if (customer == null)
            {
                return ApiResponse<IReadOnlyList<DisputeResponse>>.Fail("Customer profile not found.");
            }
            query = query.Where(d => d.CustomerId == customer.Id);
        }
        else if (_currentUserService.Role == (int)UserRole.PersonalTrainer)
        {
            var pt = await GetCurrentPtProfileAsync();
            if (pt == null)
            {
                return ApiResponse<IReadOnlyList<DisputeResponse>>.Fail("PT profile not found.");
            }
            query = query.Where(d => d.PtProfileId == pt.Id);
        }
        else if (_currentUserService.Role != (int)UserRole.Admin)
        {
            return ApiResponse<IReadOnlyList<DisputeResponse>>.Fail("Access denied.");
        }

        var disputes = await query
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        var response = disputes.Select(MapToDisputeResponse).ToList();
        return ApiResponse<IReadOnlyList<DisputeResponse>>.Ok(response, "Disputes retrieved successfully.");
    }

    public async Task<ApiResponse<DisputeResponse>> GetDisputeByIdAsync(Guid disputeId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<DisputeResponse>.Fail("User is not authenticated.");
        }

        var dispute = await _unitOfWork.Disputes.Query()
            .FirstOrDefaultAsync(d => d.Id == disputeId);

        if (dispute == null)
        {
            return ApiResponse<DisputeResponse>.Fail("Dispute not found.");
        }

        bool hasAccess = false;

        if (_currentUserService.Role == (int)UserRole.Admin)
        {
            hasAccess = true;
        }
        else if (_currentUserService.Role == (int)UserRole.Customer)
        {
            var customer = await GetCurrentCustomerProfileAsync();
            if (customer != null && dispute.CustomerId == customer.Id)
            {
                hasAccess = true;
            }
        }
        else if (_currentUserService.Role == (int)UserRole.PersonalTrainer)
        {
            var pt = await GetCurrentPtProfileAsync();
            if (pt != null && dispute.PtProfileId == pt.Id)
            {
                hasAccess = true;
            }
        }

        if (!hasAccess)
        {
            return ApiResponse<DisputeResponse>.Fail("Access denied to this dispute.");
        }

        var response = MapToDisputeResponse(dispute);
        return ApiResponse<DisputeResponse>.Ok(response, "Dispute details retrieved successfully.");
    }

    public async Task<ApiResponse<DisputeEvidenceResponse>> UploadEvidenceAsync(Guid disputeId, UploadDisputeEvidenceRequest request)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<DisputeEvidenceResponse>.Fail("User is not authenticated.");
        }

        if (string.IsNullOrWhiteSpace(request.FileUrl))
        {
            return ApiResponse<DisputeEvidenceResponse>.Fail("FileUrl is required.");
        }

        var dispute = await _unitOfWork.Disputes.Query()
            .FirstOrDefaultAsync(d => d.Id == disputeId);

        if (dispute == null)
        {
            return ApiResponse<DisputeEvidenceResponse>.Fail("Dispute not found.");
        }

        bool hasAccess = false;

        if (_currentUserService.Role == (int)UserRole.Admin)
        {
            hasAccess = true;
        }
        else if (_currentUserService.Role == (int)UserRole.Customer)
        {
            var customer = await GetCurrentCustomerProfileAsync();
            if (customer != null && dispute.CustomerId == customer.Id)
            {
                hasAccess = true;
            }
        }
        else if (_currentUserService.Role == (int)UserRole.PersonalTrainer)
        {
            var pt = await GetCurrentPtProfileAsync();
            if (pt != null && dispute.PtProfileId == pt.Id)
            {
                hasAccess = true;
            }
        }

        if (!hasAccess)
        {
            return ApiResponse<DisputeEvidenceResponse>.Fail("Access denied to this dispute.");
        }

        if (dispute.Status != (int)DisputeStatus.Open && dispute.Status != (int)DisputeStatus.UnderReview)
        {
            return ApiResponse<DisputeEvidenceResponse>.Fail("Evidence can only be uploaded for open or under review disputes.");
        }

        var evidence = new DisputeEvidence
        {
            Id = Guid.NewGuid(),
            DisputeId = dispute.Id,
            FileUrl = request.FileUrl,
            FileType = request.FileType ?? "Image",
            UploadedAt = DateTime.UtcNow
        };

        await _unitOfWork.DisputeEvidences.AddAsync(evidence);
        await _unitOfWork.SaveChangesAsync();

        var response = MapToEvidenceResponse(evidence);
        return ApiResponse<DisputeEvidenceResponse>.Ok(response, "Evidence uploaded successfully.");
    }

    public async Task<ApiResponse<DisputeResponse>> WithdrawDisputeAsync(Guid disputeId)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<DisputeResponse>.Fail("User is not authenticated.");
        }

        if (_currentUserService.Role != (int)UserRole.Customer)
        {
            return ApiResponse<DisputeResponse>.Fail("Only customers can withdraw disputes.");
        }

        var customerProfile = await GetCurrentCustomerProfileAsync();
        if (customerProfile == null)
        {
            return ApiResponse<DisputeResponse>.Fail("Customer profile not found.");
        }

        var dispute = await _unitOfWork.Disputes.Query()
            .FirstOrDefaultAsync(d => d.Id == disputeId);

        if (dispute == null)
        {
            return ApiResponse<DisputeResponse>.Fail("Dispute not found.");
        }

        if (dispute.CustomerId != customerProfile.Id)
        {
            return ApiResponse<DisputeResponse>.Fail("You do not own this dispute.");
        }

        if (dispute.Status != (int)DisputeStatus.Open)
        {
            return ApiResponse<DisputeResponse>.Fail("Only open disputes can be withdrawn.");
        }

        var booking = await _unitOfWork.Bookings.Query()
            .FirstOrDefaultAsync(b => b.Id == dispute.BookingId);

        if (booking == null)
        {
            return ApiResponse<DisputeResponse>.Fail("Associated booking not found.");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            dispute.Status = (int)DisputeStatus.Withdrawn;
            dispute.WithdrawnAt = DateTime.UtcNow;
            dispute.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Disputes.Update(dispute);

            booking.Status = dispute.OriginalBookingStatus;
            booking.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Bookings.Update(booking);

            if (dispute.OriginalSettlementStatus.HasValue)
            {
                var settlement = await _unitOfWork.Settlements.Query()
                    .FirstOrDefaultAsync(s => s.BookingId == booking.Id);
                if (settlement != null)
                {
                    settlement.Status = dispute.OriginalSettlementStatus.Value;
                    settlement.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Settlements.Update(settlement);
                }
            }

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
                        Title = "Dispute withdrawn",
                        Content = "A customer has withdrawn their dispute.",
                        Type = (int)NotificationType.Dispute,
                        IsRead = false,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Notifications.AddAsync(notification);
                }
            }
            catch
            {
                // silently ignore
            }

            try
            {
                var auditLog = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    ActorUserId = _currentUserService.UserId!.Value,
                    Action = "WithdrawDispute",
                    EntityName = "Dispute",
                    EntityId = dispute.Id,
                    MetadataJson = System.Text.Json.JsonSerializer.Serialize(new { BookingId = booking.Id, DisputeId = dispute.Id }),
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.AuditLogs.AddAsync(auditLog);
            }
            catch
            {
                // silently ignore
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return ApiResponse<DisputeResponse>.Fail($"Failed to withdraw dispute: {ex.Message}");
        }

        var response = MapToDisputeResponse(dispute);
        return ApiResponse<DisputeResponse>.Ok(response, "Dispute withdrawn successfully.");
    }

    #region Helper Methods

    private async Task<CustomerProfile?> GetCurrentCustomerProfileAsync()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return null;
        }
        var userId = _currentUserService.UserId.Value;
        return await _unitOfWork.CustomerProfiles.Query()
            .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);
    }

    private async Task<PtProfile?> GetCurrentPtProfileAsync()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return null;
        }
        var userId = _currentUserService.UserId.Value;
        return await _unitOfWork.PtProfiles.Query()
            .FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);
    }

    private DisputeResponse MapToDisputeResponse(Dispute dispute)
    {
        return new DisputeResponse
        {
            Id = dispute.Id,
            BookingId = dispute.BookingId,
            CustomerId = dispute.CustomerId,
            PtProfileId = dispute.PtProfileId,
            Reason = dispute.Reason,
            Description = dispute.Description,
            Status = dispute.Status,
            ResolutionNote = dispute.ResolutionNote,
            ResolvedByAdminId = dispute.ResolvedByAdminId,
            ResolvedAt = dispute.ResolvedAt,
            CreatedAt = dispute.CreatedAt,
            OriginalBookingStatus = dispute.OriginalBookingStatus,
            OriginalSettlementStatus = dispute.OriginalSettlementStatus,
            WithdrawnAt = dispute.WithdrawnAt
        };
    }

    private DisputeEvidenceResponse MapToEvidenceResponse(DisputeEvidence evidence)
    {
        return new DisputeEvidenceResponse
        {
            Id = evidence.Id,
            DisputeId = evidence.DisputeId,
            FileUrl = evidence.FileUrl,
            FileType = evidence.FileType,
            UploadedAt = evidence.UploadedAt
        };
    }

    #endregion
}
