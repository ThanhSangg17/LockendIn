using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.DTOs.AddonOrders;
using LockedIn.BusinessObject.Enums;

namespace LockedIn.BusinessObject.Interfaces;

public interface IAddonOrderService
{
    Task<ApiResponse<AddonOrderPaymentLinkResponse>> CreateOrderAsync(CreateAddonOrderRequest request);
    Task<ApiResponse<AddonOrderPaymentLinkResponse>> GetOrRecreatePaymentLinkAsync(Guid orderId);
    Task<ApiResponse<PagedResult<AddonOrderListItemResponse>>> GetMyOrdersAsync(PaginationRequest pagination, AddonOrderStatus? status, DateTime? startDate, DateTime? endDate);
}
