namespace RFIDP2P3_API.Models;

public class AuditLogRecord
{
    public DateTime TimestampWib { get; set; }
    public string? TraceId { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string ClientType { get; set; } = "Web";
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }
    public string Status { get; set; } = "SUCCESS";
    public string? Details { get; set; }
}