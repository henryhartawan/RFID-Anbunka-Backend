using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using RFIDP2P3_API.Models;
using RFIDP2P3_API.Services.Interfaces;

namespace RFIDP2P3_API.Services.Implementations;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    BusinessObject b = new BusinessObject();
    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;

    }

    public async Task<bool> SendEmailAsync(List<string> toEmails, string subject, string htmlBody)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            using var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
            {
                EnableSsl = _settings.UseSSL,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = string.IsNullOrWhiteSpace(_settings.SmtpUser),
                Credentials = string.IsNullOrWhiteSpace(_settings.SmtpUser)
                    ? null
                    : new NetworkCredential(_settings.SmtpUser, _settings.SmtpPass)
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            AddRangeSafe(mail.To, toEmails);
            AddRangeSafe(mail.CC, _settings.Cc);
            AddRangeSafe(mail.Bcc, _settings.Bcc);

            if (!string.IsNullOrWhiteSpace(_settings.ReplyEmail))
                mail.ReplyToList.Add(new MailAddress(_settings.ReplyEmail, _settings.ReplyName));

            var t0 = sw.ElapsedMilliseconds;
            await client.SendMailAsync(mail);
            var t1 = sw.ElapsedMilliseconds;
            
            b.WriteLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} SMTP accepted. Build={t0}ms, Send={t1 - t0}ms", "Email_log");
            b.WriteLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Email sent to: {string.Join(",", toEmails)}, Subject: {subject}", "Email_log");
            return true;
        }
        catch (SmtpException smtpEx)
        {
            b.WriteLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} SMTP error: {smtpEx.Message}, Inner: {smtpEx.InnerException?.Message}", "Email_log");
            return false;
        }
        catch (Exception ex)
        {
            b.WriteLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}General email error:  {ex.Message}, Inner: {ex.InnerException?.Message}", "Email_log");
            return false;
        }
    }

    private void AddRangeSafe(MailAddressCollection collection, IEnumerable<string>? emails)
    {
        if (emails == null) return;

        foreach (var email in emails.Distinct())
        {
            if (!string.IsNullOrWhiteSpace(email))
            {
                try
                {
                    collection.Add(email);
                }
                catch (FormatException)
                {
                    b.WriteLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} Invalid email skipped:  {email}", "Email_log");
                }
            }
        }
    }
}