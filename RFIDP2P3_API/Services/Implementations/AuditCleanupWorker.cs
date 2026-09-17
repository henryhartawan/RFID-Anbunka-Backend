using System.Data.SqlClient;

namespace RFIDP2P3_API.Services.Implementations;

public class AuditCleanupWorker : BackgroundService
{
    private readonly string _connectionString;
    private readonly ILogger<AuditCleanupWorker> _logger;
    private readonly int _retentionDays = 180;

    public AuditCleanupWorker(IConfiguration configuration, ILogger<AuditCleanupWorker> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AuditCleanupWorker has started. Retention period is set to {Days} days.", _retentionDays);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupOldLogsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while cleaning up old audit logs.");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task CleanupOldLogsAsync(CancellationToken cancellationToken)
    {
        string sql = @"
            DELETE FROM dbo.AuditLogs 
            WHERE TimestampWib < DATEADD(DAY, -@RetentionDays, DATEADD(HOUR, 7, SYSUTCDATETIME()));";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@RetentionDays", _retentionDays);

        await conn.OpenAsync(cancellationToken);
        int rowsDeleted = await cmd.ExecuteNonQueryAsync(cancellationToken);

        if (rowsDeleted > 0)
        {
            _logger.LogInformation("Successfully deleted {RowCount} audit log rows older than {Days} days.", rowsDeleted, _retentionDays);
        }
    }
}