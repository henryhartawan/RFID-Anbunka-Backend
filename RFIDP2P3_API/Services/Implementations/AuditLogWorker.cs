using System.Data;
using System.Data.SqlClient;
using RFIDP2P3_API.Models;
using RFIDP2P3_API.Services.Interfaces;

namespace RFIDP2P3_API.Services.Implementations;

public class AuditLogWorker : BackgroundService
{
    private readonly IAuditQueue _queue;
    private readonly string _connectionString;
    private readonly ILogger<AuditLogWorker> _logger;

    public AuditLogWorker(IAuditQueue queue, IConfiguration configuration, ILogger<AuditLogWorker> logger)
    {
        _queue = queue;
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var log in _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await SaveLogToDatabaseAsync(log, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write audit log to database. TraceId: {TraceId}", log.TraceId);
            }
        }
    }

    private async Task SaveLogToDatabaseAsync(AuditLogRecord log, CancellationToken cancellationToken)
    {
        const string sql = @"
            INSERT INTO dbo.AuditLogs 
            (TimestampWib, TraceId, UserId, UserName, ClientType, IpAddress, UserAgent, Action, EntityName, EntityId, Status, Details)
            VALUES 
            (@TimestampWib, @TraceId, @UserId, @UserName, @ClientType, @IpAddress, @UserAgent, @Action, @EntityName, @EntityId, @Status, @Details);";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.Add("@TimestampWib", SqlDbType.DateTime2).Value = log.TimestampWib;
        cmd.Parameters.Add("@TraceId", SqlDbType.NVarChar, 100).Value = (object?)log.TraceId ?? DBNull.Value;
        cmd.Parameters.Add("@UserId", SqlDbType.NVarChar, 100).Value = (object?)log.UserId ?? DBNull.Value;
        cmd.Parameters.Add("@UserName", SqlDbType.NVarChar, 150).Value = (object?)log.UserName ?? DBNull.Value;
        cmd.Parameters.Add("@ClientType", SqlDbType.NVarChar, 20).Value = log.ClientType;
        cmd.Parameters.Add("@IpAddress", SqlDbType.NVarChar, 50).Value = (object?)log.IpAddress ?? DBNull.Value;
        cmd.Parameters.Add("@UserAgent", SqlDbType.NVarChar, 500).Value = (object?)log.UserAgent ?? DBNull.Value;
        cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 100).Value = log.Action;
        cmd.Parameters.Add("@EntityName", SqlDbType.NVarChar, 100).Value = (object?)log.EntityName ?? DBNull.Value;
        cmd.Parameters.Add("@EntityId", SqlDbType.NVarChar, 100).Value = (object?)log.EntityId ?? DBNull.Value;
        cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = log.Status;
        cmd.Parameters.Add("@Details", SqlDbType.NVarChar, -1).Value = (object?)log.Details ?? DBNull.Value;

        await conn.OpenAsync(cancellationToken);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}