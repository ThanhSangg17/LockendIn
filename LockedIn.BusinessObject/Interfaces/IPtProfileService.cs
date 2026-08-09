using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.PtProfiles;
using LockedIn.BusinessObject.DTOs.PtProfile;

namespace LockedIn.BusinessObject.Interfaces;

public interface IPtProfileService
{
    Task<ApiResponse<PtProfileResponse>> GetMyPtProfileAsync();
    Task<ApiResponse<PtProfileResponse>> UpdateMyPtProfileAsync(UpdatePtProfileRequest request);
    Task<ApiResponse<PtProfileResponse>> UpdateMyQrCodeAsync(UpdatePtQrCodeRequest request);
    Task<ApiResponse<PtDocumentResponse>> UploadDocumentAsync(UploadPtDocumentRequest request);
    Task<ApiResponse<IReadOnlyList<PtDocumentResponse>>> GetMyDocumentsAsync();
    Task<ApiResponse<string>> DeleteDocumentAsync(Guid documentId);
    Task<ApiResponse<string>> SubmitVerificationAsync();
    Task<ApiResponse<ProfileEditRequestResponse>> SubmitProfileEditRequestAsync(SubmitProfileEditRequest request);
    Task<ApiResponse<IReadOnlyList<ProfileEditRequestResponse>>> GetMyProfileEditRequestsAsync();
}
