using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;

namespace LockedIn.BusinessObject.Interfaces;

public interface ICustomerService
{
    Task<ApiResponse<string>> GetMyCustomerProfileAsync();
    Task<ApiResponse<string>> UpdateMyCustomerProfileAsync();
}
