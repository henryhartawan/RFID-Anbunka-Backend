namespace RFIDP2P3_API.Services.Interfaces;

public interface IAuditService
{
    void Log(string action, string? entityName = null, string? entityId = null, object? payload = null, string status = "SUCCESS");
}