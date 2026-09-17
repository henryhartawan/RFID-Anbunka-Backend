using RFIDP2P3_API.Models;

namespace RFIDP2P3_API.Services.Interfaces;

public interface IAuditQueue
{
    void Enqueue(AuditLogRecord record);
    IAsyncEnumerable<AuditLogRecord> ReadAllAsync(CancellationToken cancellationToken);
}