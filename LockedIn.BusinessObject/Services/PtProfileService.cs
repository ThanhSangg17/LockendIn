using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.BusinessObject.DTOs.PtProfiles;

namespace LockedIn.BusinessObject.Services;

public class PtProfileService : IPtProfileService
{
    private readonly IUnitOfWork _unitOfWork;

    public PtProfileService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PtProfileResponse>> GetMyPtProfileAsync()
    {
        return await Task.FromResult(ApiResponse<PtProfileResponse>.Ok(new PtProfileResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<PtProfileResponse>> UpdateMyPtProfileAsync(UpdatePtProfileRequest request)
    {
        return await Task.FromResult(ApiResponse<PtProfileResponse>.Ok(new PtProfileResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<PtDocumentResponse>> UploadDocumentAsync(UploadPtDocumentRequest request)
    {
        return await Task.FromResult(ApiResponse<PtDocumentResponse>.Ok(new PtDocumentResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<IReadOnlyList<PtDocumentResponse>>> GetMyDocumentsAsync()
    {
        return await Task.FromResult(ApiResponse<IReadOnlyList<PtDocumentResponse>>.Ok(new List<PtDocumentResponse>(), "Not implemented yet"));
    }

    public async Task<ApiResponse<string>> DeleteDocumentAsync(Guid documentId)
    {
        return await Task.FromResult(ApiResponse<string>.Ok(string.Empty, "Not implemented yet"));
    }
}
