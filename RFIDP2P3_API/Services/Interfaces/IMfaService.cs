namespace RFIDP2P3_API.Services.Interfaces;

public interface IMfaService
{
    Task<string> GenerateAndSaveOtpAsync(string userId);
    Task<string?> GetUserEmailAsync(string userId);
    Task<bool> SendOtpEmailAsync(string userId);
    Task<string?> GetOtpRemarkAsync(string userId, string otp);
    Task<string> GetOrCreateSecretAsync(string userId);
    Task<bool> VerifyTotpAsync(string userId, string otp);
    Task<string?> GetSecretAsync(string userId);
    Task UpdateTokenAndLoginStatusAsync(string userId, string token);
}