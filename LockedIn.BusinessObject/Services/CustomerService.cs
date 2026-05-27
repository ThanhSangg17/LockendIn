using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;
using LockedIn.DataAccess.UnitOfWork;
using LockedIn.BusinessObject.DTOs.Customers;

namespace LockedIn.BusinessObject.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;

    public CustomerService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<CustomerProfileResponse>> GetMyCustomerProfileAsync()
    {
        return await Task.FromResult(ApiResponse<CustomerProfileResponse>.Ok(new CustomerProfileResponse(), "Not implemented yet"));
    }

    public async Task<ApiResponse<CustomerProfileResponse>> UpdateMyCustomerProfileAsync(UpdateCustomerProfileRequest request)
    {
        return await Task.FromResult(ApiResponse<CustomerProfileResponse>.Ok(new CustomerProfileResponse(), "Not implemented yet"));
    }
}
