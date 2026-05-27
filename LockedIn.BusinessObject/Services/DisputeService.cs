using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.BusinessObject.DTOs.Disputes;

namespace LockedIn.BusinessObject.Services;

public class DisputeService : IDisputeService
{
    private readonly IUnitOfWork _unitOfWork;

    public DisputeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<DisputeResponse>> CreateDisputeAsync(CreateDisputeRequest request)
    {
        return await Task.FromResult(ApiResponse<DisputeResponse>.Ok(new DisputeResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<IReadOnlyList<DisputeResponse>>> GetMyDisputesAsync()
    {
        return await Task.FromResult(ApiResponse<IReadOnlyList<DisputeResponse>>.Ok(new List<DisputeResponse>(), "Not implemented yet"));
    }

    public async Task<ApiResponse<DisputeResponse>> GetDisputeByIdAsync(Guid disputeId)
    {
        return await Task.FromResult(ApiResponse<DisputeResponse>.Ok(new DisputeResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<DisputeEvidenceResponse>> UploadEvidenceAsync(Guid disputeId, UploadDisputeEvidenceRequest request)
    {
        return await Task.FromResult(ApiResponse<DisputeEvidenceResponse>.Ok(new DisputeEvidenceResponse(), "Not implemented yet"));
    }
}
