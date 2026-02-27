namespace RFIDP2P3_API.Models;

public class EmailSettings
{
    public string FromName { get; set; }
    public string FromEmail { get; set; }
    public string SmtpServer { get; set; }
    public int SmtpPort { get; set; }
    public string SmtpUser { get; set; }
    public string SmtpPass { get; set; }
    public List<string>? Cc { get; set; }
    public List<string>? Bcc { get; set; }
    public string ReplyEmail { get; set; }
    public string ReplyName { get; set; }
    public bool UseSSL { get; set; }
}