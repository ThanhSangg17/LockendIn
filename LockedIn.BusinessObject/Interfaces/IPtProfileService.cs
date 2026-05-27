using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.PtProfiles;

namespace LockedIn.BusinessObject.Interfaces;

public interface IPtProfileService
{
    Task<ApiResponse<PtProfileResponse>> GetMyPtProfileAsync();
    Task<ApiResponse<PtProfileResponse>> UpdateMyPtProfileAsync(UpdatePtProfileRequest request);
    Task<ApiResponse<PtDocumentResponse>> UploadDocumentAsync(UploadPtDocumentRequest request);
    Task<ApiResponse<IReadOnlyList<PtDocumentResponse>>> GetMyDocumentsAsync();
    Task<ApiResponse<string>> DeleteDocumentAsync(Guid documentId);
}
