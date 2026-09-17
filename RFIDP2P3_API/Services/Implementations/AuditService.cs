using System.IdentityModel.Tokens.Jwt;
using RFIDP2P3_API.Helpers;
using RFIDP2P3_API.Models;
using RFIDP2P3_API.Services.Interfaces;

namespace RFIDP2P3_API.Services.Implementations;

public class AuditService : IAuditService
{
    private readonly IAuditQueue _queue;
    private readonly IHttpContextAccessor _contextAccessor;

    public AuditService(IAuditQueue queue, IHttpContextAccessor contextAccessor)
    {
        _queue = queue;
        _contextAccessor = contextAccessor;
    }

    public void Log(string action, string? entityName = null, string? entityId = null, object? payload = null, string status = "SUCCESS")
    {
        var context = _contextAccessor.HttpContext;
        var user = context?.User;

        string clientType = "Web";
        var userAgent = context?.Request.Headers["User-Agent"].ToString() ?? null;

        if (context?.Request.Headers.TryGetValue("X-Client-Type", out var typeHeader) == true)
        {
            clientType = typeHeader.ToString();
        }
        else if (!string.IsNullOrEmpty(userAgent))
        {
            var uaLower = userAgent.ToLower();

            if (uaLower.Contains("okhttp") || uaLower.Contains("dart") ||
                uaLower.Contains("cfnetwork") || uaLower.Contains("postman") ||
                uaLower.Contains("insomnia"))
            {
                clientType = "Mobile/API Tools";
            }
        }

        var ipAddress = context?.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                        ?? context?.Connection?.RemoteIpAddress?.ToString();

        var logRecord = new AuditLogRecord
        {
            TimestampWib = DateTime.UtcNow.AddHours(7),
            TraceId = context?.TraceIdentifier,
            UserId = user?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? "ANONYMOUS",
            UserName = user?.FindFirst(JwtRegisteredClaimNames.Name)?.Value ?? "ANONYMOUS",
            ClientType = clientType,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Action = action.ToUpperInvariant(),
            EntityName = entityName,
            EntityId = entityId,
            Status = status.ToUpperInvariant(),
            Details = AuditSanitizer.SanitizePayload(payload)
        };

        _queue.Enqueue(logRecord);
    }
}