namespace RFIDP2P3_API.Models;

public class ClientAuditRequest
{
    public string Action { get; set; } = string.Empty;
    public string Feature { get; set; } = string.Empty;
    public object? Metadata { get; set; }
}