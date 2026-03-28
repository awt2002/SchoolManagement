using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SMS.Application.Interfaces;

namespace SMS.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var host = _configuration["Smtp:Host"] ?? "localhost";
            var port = int.Parse(_configuration["Smtp:Port"] ?? "587");
            var user = _configuration["Smtp:User"] ?? "";
            var password = _configuration["Smtp:Password"] ?? "";
            var fromName = _configuration["Smtp:FromName"] ?? "SMS";

            using var client = new SmtpClient(host, port);
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(user, password);
            client.EnableSsl = true;

            var mailMessage = new MailMessage
            {
                From = new MailAddress(user, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
        }

        public async Task<bool> TrySendEmailAsync(string to, string subject, string body)
        {
            try
            {
                await SendEmailAsync(to, subject, body);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipient} with subject {Subject}", to, subject);
                return false;
            }
        }
    }
}
