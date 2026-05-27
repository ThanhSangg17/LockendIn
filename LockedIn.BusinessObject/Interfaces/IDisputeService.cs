using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;

namespace LockedIn.BusinessObject.Interfaces;

public interface IDisputeService
{
    Task<ApiResponse<string>> CreateDisputeAsync();
    Task<ApiResponse<string>> GetMyDisputesAsync();
    Task<ApiResponse<string>> GetDisputeByIdAsync(Guid disputeId);
    Task<ApiResponse<string>> UploadEvidenceAsync(Guid disputeId);
}
