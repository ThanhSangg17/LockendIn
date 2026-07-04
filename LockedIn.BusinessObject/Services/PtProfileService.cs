using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.BusinessObject.DTOs.PtProfiles;
using LockedIn.BusinessObject.DTOs.PtProfile;
using LockedIn.DataAccess.Models;
using LockedIn.BusinessObject.Enums;

namespace LockedIn.BusinessObject.Services;

public class PtProfileService : IPtProfileService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public PtProfileService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    private async Task<(PtProfile? Profile, string? Error)> GetCurrentPtProfileAsync()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return (null, "User is not authenticated.");
        }

        if (_currentUserService.Role != 2) // PersonalTrainer = 2
        {
            return (null, "Only personal trainers can perform this action.");
        }

        var userId = _currentUserService.UserId.Value;
        var ptProfile = await _unitOfWork.PtProfiles.Query()
            .Include(pt => pt.User)
            .FirstOrDefaultAsync(pt => pt.UserId == userId && !pt.IsDeleted);

        if (ptProfile == null)
        {
            return (null, "PT profile not found.");
        }

        return (ptProfile, null);
    }

    public async Task<ApiResponse<PtProfileResponse>> GetMyPtProfileAsync()
    {
        var (ptProfile, error) = await GetCurrentPtProfileAsync();
        if (error != null) return ApiResponse<PtProfileResponse>.Fail(error);

        return ApiResponse<PtProfileResponse>.Ok(MapToProfileResponse(ptProfile!), "Profile retrieved successfully.");
    }

    public async Task<ApiResponse<PtProfileResponse>> UpdateMyPtProfileAsync(UpdatePtProfileRequest request)
    {
        var (ptProfile, error) = await GetCurrentPtProfileAsync();
        if (error != null) return ApiResponse<PtProfileResponse>.Fail(error);

        if (ptProfile!.VerificationStatus == (int)PtVerificationStatus.Approved || 
            ptProfile.VerificationStatus == (int)PtVerificationStatus.Submitted)
        {
            return ApiResponse<PtProfileResponse>.Fail("Cannot update profile while it is submitted or approved.");
        }

        ptProfile!.Bio = request.Bio;
        ptProfile.Specialization = request.Specialization;
        ptProfile.ExperienceYears = request.ExperienceYears;
        ptProfile.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.PtProfiles.Update(ptProfile);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<PtProfileResponse>.Ok(MapToProfileResponse(ptProfile), "Profile updated successfully.");
    }

    public async Task<ApiResponse<PtDocumentResponse>> UploadDocumentAsync(UploadPtDocumentRequest request)
    {
        var (ptProfile, error) = await GetCurrentPtProfileAsync();
        if (error != null) return ApiResponse<PtDocumentResponse>.Fail(error);

        if (ptProfile!.VerificationStatus == (int)PtVerificationStatus.Approved || 
            ptProfile.VerificationStatus == (int)PtVerificationStatus.Submitted)
        {
            return ApiResponse<PtDocumentResponse>.Fail("Cannot upload documents while profile is submitted or approved.");
        }

        if (string.IsNullOrWhiteSpace(request.FileUrl))
        {
            return ApiResponse<PtDocumentResponse>.Fail("File URL cannot be empty.");
        }

        var document = new PtDocument
        {
            Id = Guid.NewGuid(),
            PtProfileId = ptProfile!.Id,
            DocumentType = request.DocumentType,
            FileUrl = request.FileUrl,
            Status = 1, // Pending by default
            UploadedAt = DateTime.UtcNow
        };

        await _unitOfWork.PtDocuments.AddAsync(document);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<PtDocumentResponse>.Ok(MapToDocumentResponse(document), "Document uploaded successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<PtDocumentResponse>>> GetMyDocumentsAsync()
    {
        var (ptProfile, error) = await GetCurrentPtProfileAsync();
        if (error != null) return ApiResponse<IReadOnlyList<PtDocumentResponse>>.Fail(error);

        var documents = await _unitOfWork.PtDocuments.Query()
            .Where(d => d.PtProfileId == ptProfile!.Id)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();

        var response = documents.Select(MapToDocumentResponse).ToList();
        return ApiResponse<IReadOnlyList<PtDocumentResponse>>.Ok(response, "Documents retrieved successfully.");
    }

    public async Task<ApiResponse<string>> DeleteDocumentAsync(Guid documentId)
    {
        var (ptProfile, error) = await GetCurrentPtProfileAsync();
        if (error != null) return ApiResponse<string>.Fail(error);

        if (ptProfile!.VerificationStatus == (int)PtVerificationStatus.Approved)
        {
            return ApiResponse<string>.Fail("Cannot delete documents after profile is approved.");
        }

        if (ptProfile.VerificationStatus == (int)PtVerificationStatus.Submitted)
        {
            return ApiResponse<string>.Fail("Cannot delete documents while profile is under review.");
        }

        var document = await _unitOfWork.PtDocuments.Query()
            .FirstOrDefaultAsync(d => d.Id == documentId && d.PtProfileId == ptProfile.Id);

        if (document == null)
        {
            return ApiResponse<string>.Fail("Document not found.");
        }

        _unitOfWork.PtDocuments.Delete(document);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<string>.Ok(string.Empty, "Document deleted successfully.");
    }

    public async Task<ApiResponse<string>> SubmitVerificationAsync()
    {
        var (ptProfile, error) = await GetCurrentPtProfileAsync();
        if (error != null) return ApiResponse<string>.Fail(error);

        if (ptProfile!.VerificationStatus == (int)PtVerificationStatus.Submitted || 
            ptProfile.VerificationStatus == (int)PtVerificationStatus.Approved)
        {
            return ApiResponse<string>.Fail("Profile has already been submitted or approved.");
        }

        if (string.IsNullOrWhiteSpace(ptProfile.Bio) || 
            string.IsNullOrWhiteSpace(ptProfile.Specialization) || 
            ptProfile.ExperienceYears <= 0)
        {
            return ApiResponse<string>.Fail("Bio, Specialization, and valid Experience Years are required to submit.");
        }

        var documents = await _unitOfWork.PtDocuments.Query()
            .Where(d => d.PtProfileId == ptProfile.Id)
            .ToListAsync();

        var hasCertificate = documents.Any(d => d.DocumentType == 1);
        var hasIdentityCard = documents.Any(d => d.DocumentType == 2);

        if (!hasCertificate || !hasIdentityCard)
        {
            return ApiResponse<string>.Fail("You must upload at least 1 Certificate and 1 Identity Card to submit.");
        }

        ptProfile.VerificationStatus = (int)PtVerificationStatus.Submitted;
        ptProfile.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.PtProfiles.Update(ptProfile);

        try
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                ActorUserId = _currentUserService.UserId!.Value,
                Action = "SubmitPtVerification",
                EntityName = "PtProfile",
                EntityId = ptProfile.Id,
                MetadataJson = "{}",
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.AuditLogs.AddAsync(auditLog);
        }
        catch {}

        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<string>.Ok(string.Empty, "Profile submitted for verification successfully.");
    }

    private PtProfileResponse MapToProfileResponse(PtProfile pt)
    {
        return new PtProfileResponse
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
        };
    }

    private PtDocumentResponse MapToDocumentResponse(PtDocument doc)
    {
        return new PtDocumentResponse
        {
            Id = doc.Id,
            PtProfileId = doc.PtProfileId,
            DocumentType = doc.DocumentType,
            FileUrl = doc.FileUrl,
            Status = doc.Status,
            UploadedAt = doc.UploadedAt
        };
    }
    public async Task<ApiResponse<ProfileEditRequestResponse>> SubmitProfileEditRequestAsync(SubmitProfileEditRequest request)
    {
        var (ptProfile, error) = await GetCurrentPtProfileAsync();
        if (error != null) return ApiResponse<ProfileEditRequestResponse>.Fail(error);

        if (ptProfile!.VerificationStatus != (int)PtVerificationStatus.Approved)
        {
            return ApiResponse<ProfileEditRequestResponse>.Fail("Only approved PTs can submit a profile edit request.");
        }

        request.Bio = request.Bio?.Trim();
        request.Specialization = request.Specialization?.Trim();

        if (string.IsNullOrWhiteSpace(request.Bio))
            return ApiResponse<ProfileEditRequestResponse>.Fail("Bio cannot be empty.");

        if (string.IsNullOrWhiteSpace(request.Specialization))
            return ApiResponse<ProfileEditRequestResponse>.Fail("Specialization cannot be empty.");

        if (request.ExperienceYears <= 0)
            return ApiResponse<ProfileEditRequestResponse>.Fail("Experience years must be greater than 0.");

        var currentBio = ptProfile.Bio?.Trim();
        var currentSpecialization = ptProfile.Specialization?.Trim();

        if (request.Bio == currentBio && 
            request.Specialization == currentSpecialization && 
            request.ExperienceYears == ptProfile.ExperienceYears)
        {
            return ApiResponse<ProfileEditRequestResponse>.Fail("No changes detected in your profile data.");
        }

        var existingPendingRequest = await _unitOfWork.PtProfileEditRequests.Query()
            .FirstOrDefaultAsync(r => r.PtProfileId == ptProfile.Id && r.Status == (int)PtProfileEditRequestStatus.Pending);

        if (existingPendingRequest != null)
        {
            return ApiResponse<ProfileEditRequestResponse>.Fail("You already have a pending profile edit request.");
        }

        var editRequest = new PtProfileEditRequest
        {
            Id = Guid.NewGuid(),
            PtProfileId = ptProfile.Id,
            CurrentBio = ptProfile.Bio,
            CurrentSpecialization = ptProfile.Specialization,
            CurrentExperienceYears = ptProfile.ExperienceYears,
            RequestedBio = request.Bio,
            RequestedSpecialization = request.Specialization,
            RequestedExperienceYears = request.ExperienceYears,
            Status = (int)PtProfileEditRequestStatus.Pending,
            RequestedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.PtProfileEditRequests.AddAsync(editRequest);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<ProfileEditRequestResponse>.Ok(MapToProfileEditRequestResponse(editRequest), "Profile edit request submitted successfully.");
    }

    public async Task<ApiResponse<IReadOnlyList<ProfileEditRequestResponse>>> GetMyProfileEditRequestsAsync()
    {
        var (ptProfile, error) = await GetCurrentPtProfileAsync();
        if (error != null) return ApiResponse<IReadOnlyList<ProfileEditRequestResponse>>.Fail(error);

        var requests = await _unitOfWork.PtProfileEditRequests.Query()
            .Where(r => r.PtProfileId == ptProfile!.Id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var response = requests.Select(MapToProfileEditRequestResponse).ToList();
        return ApiResponse<IReadOnlyList<ProfileEditRequestResponse>>.Ok(response, "Profile edit requests retrieved successfully.");
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
}
