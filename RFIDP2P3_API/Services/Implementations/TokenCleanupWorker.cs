using System.Data.SqlClient;

namespace RFIDP2P3_API.Services.Implementations
{
    public class TokenCleanupWorker : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<TokenCleanupWorker> _logger;

        public TokenCleanupWorker(IConfiguration config, ILogger<TokenCleanupWorker> logger)
        {
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TokenCleanupWorker service is starting.");

            TimeSpan loopDelay = TimeSpan.FromHours(24);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    string connectionString = _config.GetConnectionString("DefaultConnection")!;

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        await conn.OpenAsync(stoppingToken);

                        string query = "DELETE FROM UserRefreshTokens WHERE ExpiresUtc < GETUTCDATE()";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            int deletedCount = await cmd.ExecuteNonQueryAsync(stoppingToken);

                            if (deletedCount > 0)
                            {
                                _logger.LogInformation("[Token Cleanup] Successfully deleted {DeletedCount} expired token(s) from the database.", deletedCount);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Token Cleanup] An error occurred while deleting expired tokens.");
                }
                await Task.Delay(loopDelay, stoppingToken);
            }
        }
    }
}