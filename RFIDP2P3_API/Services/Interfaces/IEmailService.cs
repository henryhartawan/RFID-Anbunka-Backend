namespace RFIDP2P3_API.Services.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailAsync(List<string> toEmail, string subject, string htmlBody);
}