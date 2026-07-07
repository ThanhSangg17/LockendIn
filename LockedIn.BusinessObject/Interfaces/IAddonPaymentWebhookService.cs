using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;
using PayOS.Models.Webhooks;

namespace LockedIn.BusinessObject.Interfaces;

public interface IAddonPaymentWebhookService
{
    Task<ApiResponse<string>> HandleWebhookAsync(WebhookData verifiedData, string requestCode, string rawPayload, DateTime receivedAt);
}