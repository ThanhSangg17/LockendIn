using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;

namespace LockedIn.BusinessObject.Interfaces;

public interface IPtProfileService
{
    Task<ApiResponse<string>> GetMyPtProfileAsync();
    Task<ApiResponse<string>> UpdateMyPtProfileAsync();
    Task<ApiResponse<string>> UploadDocumentAsync();
    Task<ApiResponse<string>> GetMyDocumentsAsync();
    Task<ApiResponse<string>> DeleteDocumentAsync(Guid documentId);
}
