using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using OtpNet;
using RFIDP2P3_API.Helpers;
using RFIDP2P3_API.Models;
using RFIDP2P3_API.Services.Interfaces;

namespace RFIDP2P3_API.Services.Implementations;

public class MfaService : IMfaService
{
    private readonly string? _connectionString;
    private readonly ILogger<MfaService> _logger;
    private readonly IEmailService _emailService;
    
    public MfaService(IConfiguration config, ILogger<MfaService> logger, IEmailService emailService)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
        _logger = logger;
        _emailService = emailService;
    }
    
    public async Task<string> GenerateAndSaveOtpAsync(string userId)
    {
        var otp = GenerateSecureOTP();
        MfaLogHelper.Info("Generating secure OTP", new { userId, otp });
        
        try 
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_Submit_User_Auth_Email", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@U_PIC_ID", userId);
            cmd.Parameters.AddWithValue("@OTP", otp);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            MfaLogHelper.Info("OTP successfully saved to DB", new { userId });
        }
        catch (Exception ex)
        {
            MfaLogHelper.Error("Failed to save OTP to DB", new { userId, error = ex.Message });
            throw;
        }
        return otp;
    }
    
    public async Task<string?> GetUserEmailAsync(string userId)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("sp_Inq_Login_MFA", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@Ftype", "GetUserData");
        cmd.Parameters.AddWithValue("@userLogin", userId);

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? reader["Email"]?.ToString() : null;
    }
    
    private string GenerateSecureOTP()
    {
        var buffer = new byte[5];
        RandomNumberGenerator.Fill(buffer);
        var otp = new StringBuilder();
        foreach (var byteValue in buffer)
        {
            otp.Append(byteValue % 10);
        }
        return otp.ToString();
    }

    private string EncryptOtp(string otp)
    {
        using (var sha256 = SHA256.Create())
        {
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(otp));
            return Convert.ToBase64String(hash);
        }
    }
    
    public async Task<bool> SendOtpEmailAsync(string userId)
    {
        try
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_Inq_Login_MFA", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Ftype", "GetUserData");
            cmd.Parameters.AddWithValue("@userLogin", userId);
            
            await conn.OpenAsync();
            
            UserData? userData = null;
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    userData = new UserData
                    {
                        Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : null,
                        OTP = reader["OTP"] != DBNull.Value ? reader["OTP"].ToString() : null,
                        U_PIC_Name = reader["U_PIC_Name"] != DBNull.Value ? reader["U_PIC_Name"].ToString() : null
                    };
                }
            }
            
            if (userData == null || string.IsNullOrWhiteSpace(userData.Email) || string.IsNullOrWhiteSpace(userData.OTP))
            {
                MfaLogHelper.Info("User data not found or incomplete for email sending", new { userId });
                return false;
            }
            
            string subject = "Your OTP Code";
            string htmlBody = $@"
                <p>Hi {userData.U_PIC_Name},</p>
                <p>Your OTP is: <strong>{userData.OTP}</strong></p>
                <p>Please do not share this code with anyone.</p>";
            
            var emailList = new List<string> { userData.Email };
            bool emailSent = await _emailService.SendEmailAsync(emailList, subject, htmlBody);
            
            if (emailSent)
                MfaLogHelper.Info("OTP email successfully sent", new { userId, Email = userData.Email });
            else
                MfaLogHelper.Error("Failed to send OTP email", new { userId, Email = userData.Email });
            
            return emailSent;
        }
        catch (Exception ex)
        {
            MfaLogHelper.Error("An error occurred while sending OTP email", new { userId, error = ex.Message });
            return false; 
        }
    }

    public async Task<string?> GetOtpRemarkAsync(string userId, string otp)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("sp_Inq_Auth_Otp", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@U_PIC_ID", userId);
        cmd.Parameters.AddWithValue("@OTP", otp);
        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? reader["Remarks"]?.ToString() : null;
    }

    public async Task<string> GetOrCreateSecretAsync(string userId)
    {
        var existing = NormalizeSecret(await GetSecretAsync(userId));
        if (!string.IsNullOrWhiteSpace(existing))
        {
            MfaLogHelper.Info("Returning existing Secret Key", new { userId });
            return existing;
        }
        
        var keyBytes = KeyGeneration.GenerateRandomKey(20);
        var base32   = NormalizeSecret(Base32Encoding.ToString(keyBytes));
        
        using var conn = new SqlConnection(_connectionString);
        using var cmd  = new SqlCommand("sp_Submit_Secret_MFA", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@Secret", base32);
        cmd.Parameters.AddWithValue("@userLogin", userId);
        
        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();

        // Log aman (tanpa membuka secret)
        MfaLogHelper.Info("Secret created", new {
            userId,
            secretLen  = base32.Length,
            secretHash8 = ShortHash8(base32)
        });

        return base32;
    }

    public async Task<string?> GetSecretAsync(string userId)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd  = new SqlCommand("sp_Inq_Login_MFA", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@Ftype", "GetSecretKey");
        cmd.Parameters.AddWithValue("@userLogin", userId);

        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();
        var raw = await reader.ReadAsync() ? reader["Secret"]?.ToString() : null;

        var norm = NormalizeSecret(raw);
        return string.IsNullOrWhiteSpace(norm) ? null : norm;
    }

    public async Task<bool> VerifyTotpAsync(string userId, string otp)
    {
        const int period = 30;
        const int digits = 6;
        const OtpHashMode algo = OtpHashMode.Sha1;
        const int verifyWindow = 2;
        const int DRIFT_PROBE_STEPS = 10;
        
        try
        {
            otp ??= "";
            otp = Regex.Replace(otp, "[^0-9]", "");
            var maskedOtp = otp.Length <= 2 ? new string('*', otp.Length)
                : new string('*', otp.Length - 2) + otp[^2..];
            
            MfaLogHelper.Info("VerifyTotp: start", new { userId, otp, otpLen = otp.Length });
            
            var secretRaw  = await GetSecretAsync(userId);
            if (string.IsNullOrEmpty(secretRaw ))
            {
                MfaLogHelper.Info("VerifyTotp: end", new { userId, ok = false, reason = "secret_not_found" });
                return false;
            }
            var secret = NormalizeSecret(secretRaw);
            
            if (otp.Length != digits)
            {
                MfaLogHelper.Info("VerifyTotp: end", new {
                    userId, ok = false, reason = "otp_format_invalid", otpLen = otp.Length, digitsExpected = digits
                });
                return false;
            }
            
            if (!Regex.IsMatch(secret, "^[A-Z2-7]+$"))
            {
                MfaLogHelper.Info("VerifyTotp: end", new {
                    userId, ok = false, reason = "secret_invalid_chars", secretLen = secret.Length
                });
                return false;
            }
            
            byte[] keyBytes;
            string roundtrip;
            try
            {
                keyBytes = Base32Encoding.ToBytes(secret);
                roundtrip = NormalizeSecret(Base32Encoding.ToString(keyBytes));
            }
            catch (Exception ex)
            {
                MfaLogHelper.Info("VerifyTotp: end", new {
                    userId, ok = false, reason = "secret_base32_decode_throw", ex.Message
                });
                return false;
            }
            
            if (roundtrip != secret)
            {
                MfaLogHelper.Info("VerifyTotp: end", new {
                    userId, ok = false, reason = "secret_base32_roundtrip_fail",
                    secretLen = secret.Length, secretHash8 = ShortHash8(secret)
                });
                return false;
            }

            var totp = new Totp(keyBytes, step: period, mode: algo, totpSize: digits);
            var unix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var serverStep = unix / period;
            
            bool ok = totp.VerifyTotp(otp, out long matchedStep, new VerificationWindow(verifyWindow, verifyWindow));
            if (ok)
            {
                MfaLogHelper.Info("VerifyTotp: end", new {
                    userId, ok = true, reason = "ok",
                    matchedStep, serverStep, stepDelta = matchedStep - serverStep,
                    period, digits, algo = algo.ToString(),
                    secretLen = secret.Length, secretHash8 = ShortHash8(secret),
                    otp = maskedOtp
                });
                return true;
            }
            
            int? driftOffset = null;
            for (int off = -DRIFT_PROBE_STEPS; off <= DRIFT_PROBE_STEPS; off++)
            {
                var ts = DateTime.UtcNow.AddSeconds(off * period);
                var cand = totp.ComputeTotp(ts);
                if (cand == otp) { driftOffset = off; break; }
            }

            if (driftOffset.HasValue && driftOffset.Value != 0)
            {
                MfaLogHelper.Info("VerifyTotp: end", new {
                    userId, ok = false, reason = "clock_skew_detected",
                    driftOffsetSteps = driftOffset, approxDriftSeconds = driftOffset * period,
                    serverStep, period, digits, algo = algo.ToString(),
                    secretLen = secret.Length, secretHash8 = ShortHash8(secret),
                    otp = maskedOtp
                });
            }
            else
            {
                // Benar-benar tidak ada kecocokan di ±5 menit → sangat mungkin SECRET mismatch
                MfaLogHelper.Info("VerifyTotp: end", new {
                    userId, ok = false, reason = "no_match_window",
                    matchedStep, serverStep,
                    period, digits, algo = algo.ToString(),
                    secretLen = secret.Length, secretHash8 = ShortHash8(secret),
                    otp = maskedOtp
                });
            }
            
            MfaLogHelper.Info("VerifyTotp: end", new {
                userId, ok = false, reason = "no_match_window",
                matchedStep, serverStep,
                period, digits, algo = algo.ToString(),
                secretLen = secret.Length, secretHash8 = ShortHash8(secret)
            });
            
            // var totp = new Totp(Base32Encoding.ToBytes(secret));
            // bool ok = totp.VerifyTotp(otp, out long step, new VerificationWindow(2, 2));

            // MfaLogHelper.Info("VerifyTotp: end", new { userId, ok, step });
            return false;
        }
        catch (Exception ex)
        {
            MfaLogHelper.Error("VerifyTotp exception", new { userId, ex.Message });
            return false;
        }
    }

    public async Task UpdateTokenAndLoginStatusAsync(string userId, string token)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("UPDATE M_UserAuth SET exp_token = @token, is_Logged_In = 1 WHERE UserID = @UserId", conn);
        cmd.Parameters.AddWithValue("@token", token);
        cmd.Parameters.AddWithValue("@UserId", userId);
        await conn.OpenAsync();
        await cmd.ExecuteNonQueryAsync();
    }
    
    private static string NormalizeSecret(string? s) =>
        (s ?? "").Replace(" ", "").ToUpperInvariant();

    private static string ShortHash8(string s)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(s));
        return Convert.ToHexString(bytes)[..8];
    }
}