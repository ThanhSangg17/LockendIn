using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using LockedIn.BusinessObject.Common;
using LockedIn.BusinessObject.Interfaces;

namespace LockedIn.BusinessObject.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<ApiResponse<string>> SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var emailSection = _configuration.GetSection("Email");
            var host = emailSection["SmtpHost"];
            var portStr = emailSection["SmtpPort"];
            var user = emailSection["SmtpUser"];
            var password = emailSection["SmtpPassword"];
            var fromEmail = emailSection["FromEmail"];
            var fromName = emailSection["FromName"] ?? "LockedIn";

            if (string.IsNullOrEmpty(host))
            {
                return ApiResponse<string>.Fail("SMTP host is not configured.");
            }

            if (string.IsNullOrEmpty(fromEmail))
            {
                return ApiResponse<string>.Fail("SMTP FromEmail is not configured.");
            }

            int port = 587;
            if (int.TryParse(portStr, out var parsedPort))
            {
                port = parsedPort;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail!));
            message.To.Add(new MailboxAddress(toEmail, toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                try
                {
                    // MailKit SMTP with STARTTLS
                    await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);

                    if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
                    {
                        await client.AuthenticateAsync(user, password);
                    }

                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
                catch (Exception ex)
                {
                    // Return ApiResponse instead of throwing normal SMTP exceptions
                    return ApiResponse<string>.Fail($"SMTP sending failed: {ex.Message}");
                }
            }

            return ApiResponse<string>.Ok("Email sent successfully.", "Email sent successfully.");
        }

        public async Task<ApiResponse<string>> SendVerificationEmailAsync(Guid userId, string email, string fullName)
        {
            var expirationUtc = DateTime.UtcNow.AddHours(24);

            // Generate secure token using HMACSHA256 + Jwt:SecretKey
            var secretKey = _configuration["Jwt:SecretKey"]!;
            var payload = $"{userId}|{email}|{expirationUtc.Ticks}";
            string token;
            using (var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secretKey)))
            {
                var hashBytes = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
                var hashStr = Convert.ToBase64String(hashBytes);
                var tokenRaw = $"{payload}||{hashStr}";
                var bytes = System.Text.Encoding.UTF8.GetBytes(tokenRaw);
                token = Microsoft.AspNetCore.WebUtilities.Base64UrlTextEncoder.Encode(bytes);
            }

            var verificationLink = $"https://localhost:7072/api/auth/verify-email?userId={userId}&token={token}";

            var subject = "LockedIn - Verify Your Email Address";
            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eeeeee; border-radius: 5px;'>
                    <h2 style='color: #333333;'>Welcome to LockedIn, {fullName}!</h2>
                    <p style='color: #666666;'>Please verify your email address by clicking the button below:</p>
                    <p style='margin: 30px 0;'>
                        <a href='{verificationLink}' style='background-color: #4CAF50; color: white; padding: 12px 24px; text-decoration: none; display: inline-block; border-radius: 4px; font-weight: bold;'>Verify Email Address</a>
                    </p>
                    <p style='color: #666666;'>Or copy and paste this link into your browser:</p>
                    <p style='color: #888888; font-size: 13px;'><a href='{verificationLink}'>{verificationLink}</a></p>
                    <hr style='border: none; border-top: 1px solid #eeeeee; margin: 30px 0;'/>
                    <p style='color: #999999; font-size: 12px;'>This verification link will expire in 24 hours.</p>
                    <p style='color: #999999; font-size: 12px;'>Best regards,<br/>The LockedIn Team</p>
                </div>";

            return await SendEmailAsync(email, subject, htmlBody);
        }
    }
}
