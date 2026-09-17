using System.Threading.Channels;
using RFIDP2P3_API.Models;
using RFIDP2P3_API.Services.Interfaces;

namespace RFIDP2P3_API.Services.Implementations;

public class AuditQueue : IAuditQueue
{
    private readonly Channel<AuditLogRecord> _queue;

    public AuditQueue()
    {
        var options = new BoundedChannelOptions(10000)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        };
        _queue = Channel.CreateBounded<AuditLogRecord>(options);
    }

    public void Enqueue(AuditLogRecord record)
    {
        _queue.Writer.TryWrite(record);
    }

    public IAsyncEnumerable<AuditLogRecord> ReadAllAsync(CancellationToken cancellationToken)
    {
        return _queue.Reader.ReadAllAsync(cancellationToken);
    }
}