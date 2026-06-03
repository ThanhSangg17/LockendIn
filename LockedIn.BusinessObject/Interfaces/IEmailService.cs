using System;
using System.Threading.Tasks;
using LockedIn.BusinessObject.Common;

namespace LockedIn.BusinessObject.Interfaces
{
    public interface IEmailService
    {
        Task<ApiResponse<string>> SendEmailAsync(string toEmail, string subject, string htmlBody);
        Task<ApiResponse<string>> SendVerificationEmailAsync(Guid userId, string email, string fullName);
    }
}
