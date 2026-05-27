using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.Disputes;

namespace LockedIn.BusinessObject.Interfaces;

public interface IDisputeService
{
    Task<ApiResponse<DisputeResponse>> CreateDisputeAsync(CreateDisputeRequest request);
    Task<ApiResponse<IReadOnlyList<DisputeResponse>>> GetMyDisputesAsync();
    Task<ApiResponse<DisputeResponse>> GetDisputeByIdAsync(Guid disputeId);
    Task<ApiResponse<DisputeEvidenceResponse>> UploadEvidenceAsync(Guid disputeId, UploadDisputeEvidenceRequest request);
}
