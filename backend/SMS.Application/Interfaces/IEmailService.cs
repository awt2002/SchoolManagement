namespace SMS.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task<bool> TrySendEmailAsync(string to, string subject, string body);
    }
}
