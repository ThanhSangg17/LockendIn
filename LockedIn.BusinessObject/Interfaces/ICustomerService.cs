using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.Customers;

namespace LockedIn.BusinessObject.Interfaces;

public interface ICustomerService
{
    Task<ApiResponse<CustomerProfileResponse>> GetMyCustomerProfileAsync();
    Task<ApiResponse<CustomerProfileResponse>> UpdateMyCustomerProfileAsync(UpdateCustomerProfileRequest request);
}
