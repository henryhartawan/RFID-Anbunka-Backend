using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using RFIDP2P3_API.Helpers;
using RFIDP2P3_API.Models;
using RFIDP2P3_API.Services;
using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.RateLimiting;
using OtpNet;
using RFIDP2P3_API.Services.Interfaces;
using AttemptMap = System.Collections.Concurrent.ConcurrentDictionary<
    string, (int Count, System.DateTime WindowStartUtc, System.DateTime LastHitUtc)>;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LoginMFAController : ControllerBase
    {
        private readonly IMfaService _mfaService;
        private readonly ILogger<LoginMFAController> _logger;
        private readonly IConfiguration _config;
        
        public LoginMFAController(IMfaService mfaService, ILogger<LoginMFAController> logger, IConfiguration config)
        {
            _mfaService = mfaService;
            _logger = logger;
            _config = config;
        }

        [HttpPost] [AllowAnonymous]
        public async Task<IActionResult> ReadEmail([FromBody] LoginMFA login)
        {
            MfaLogHelper.Info("ReadEmail requested", new { login.UserId });
            
            var email = await _mfaService.GetUserEmailAsync(login.UserId);
            string maskedEmail = string.IsNullOrWhiteSpace(email)
                ? GenerateDummyMaskedEmail(login.UserId)
                : MaskEmail(email);
            
            MfaLogHelper.Info("ReadEmail result", new { login.UserId, emailFound = !string.IsNullOrWhiteSpace(email) });
            
            return Ok(new
            {
                status = 1,
                message = "If the user exists, their email is shown below.",
                data = new { email = maskedEmail }
            });
        }
        
        [HttpPost] [AllowAnonymous]
        public async Task<IActionResult> SendEmail(
            [FromBody] LoginMFA login,
            [FromServices] AttemptMap sendEmailTracker)
        {
            MfaLogHelper.Info("SendEmail requested", new { login.UserId });
            
            const int MAX_SENDS = 3;
            var WINDOW = TimeSpan.FromMinutes(1);

            var key = EndpointThrottle.BuildKey(HttpContext, login.UserId);

            if (EndpointThrottle.ShouldThrottle(sendEmailTracker, key, MAX_SENDS, WINDOW,
                    out var retryAfter, out var current, out var left))
            {
                MfaLogHelper.Info("SendEmail throttled", new { login.UserId, retryAfter });
                Response.Headers["Retry-After"] = retryAfter.ToString();
                return StatusCode(429, new {
                    error = $"Terlalu sering meminta OTP. Coba lagi dalam {retryAfter} detik.",
                    countInWindow = current, limit = MAX_SENDS, windowSecondsLeft = left, retryAfter
                });
            }

            EndpointThrottle.RegisterHit(sendEmailTracker, key, WINDOW);

            await _mfaService.GenerateAndSaveOtpAsync(login.UserId);
            var sent = await _mfaService.SendOtpEmailAsync(login.UserId);
            
            MfaLogHelper.Info("SendEmail process finished", new { login.UserId, isSent = sent });
            
            return Ok(new { validation = 0, message = "If the user exists, an OTP has been sent." });
        }
        
        [HttpPost] [AllowAnonymous]
        public async Task<IActionResult> CheckOTPEmail(
            [FromBody] LoginMFA login,
            [FromServices] AttemptMap checkOtpEmailTracker)
        {
            MfaLogHelper.Info("CheckOTPEmail requested", new { login.UserId });
            
            const int MAX_FAILS = 3;
            var WINDOW = TimeSpan.FromMinutes(1); 

            var key = EndpointThrottle.BuildKey(HttpContext, login.UserId);

            if (EndpointThrottle.ShouldThrottle(checkOtpEmailTracker, key, MAX_FAILS, WINDOW,
                out var retryAfter, out var current, out var left))
            {
                MfaLogHelper.Info("CheckOTPEmail throttled", new { login.UserId, retryAfter });
                Response.Headers["Retry-After"] = retryAfter.ToString();
                return StatusCode(429, new {
                    error = $"Terlalu banyak OTP salah. Coba lagi dalam {retryAfter} detik.",
                    attemptsInWindow = current, limit = MAX_FAILS, windowSecondsLeft = left, retryAfter
                });
            }

            var remark = await _mfaService.GetOtpRemarkAsync(login.UserId, login.OTP);
            if (remark != "1")
            {
                MfaLogHelper.Info("CheckOTPEmail failed (Invalid OTP)", new { login.UserId, remark });
                EndpointThrottle.RegisterFailure(checkOtpEmailTracker, key, WINDOW);

                var (cnt, left2) = EndpointThrottle.GetState(checkOtpEmailTracker, key, WINDOW);
                if (cnt >= MAX_FAILS)
                {
                    Response.Headers["Retry-After"] = left2.ToString();
                    return StatusCode(429, new {
                        error = $"Terlalu banyak OTP salah. Coba lagi dalam {left2} detik.",
                        attemptsInWindow = cnt, limit = MAX_FAILS, windowSecondsLeft = left2, retryAfter = left2
                    });
                }
                
                return Unauthorized(new {
                    message = "Invalid or expired OTP.",
                    attemptsInWindow = cnt,
                    limit = MAX_FAILS,
                    windowSecondsLeft = left2,
                    remainingBeforeLock = Math.Max(0, MAX_FAILS - cnt)
                });
            }

            MfaLogHelper.Info("CheckOTPEmail success, proceeding to generate QR", new { login.UserId });
            EndpointThrottle.Reset(checkOtpEmailTracker, key);

            var secret = await _mfaService.GetOrCreateSecretAsync(login.UserId);
            var issuer = _config["MFA:Issuer"] ?? "DefaultIssuer";
            
            var otpUrl = GenerateOtpAuthUrl(secret, login.UserName, issuer);
            var qrCodeBase64 = GenerateQRCodeBase64(otpUrl);
            
            return Ok(new { status = 1, data = new { imgUrl = "data:image/png;base64," + qrCodeBase64 } });
        }
        
        [HttpPost][AllowAnonymous]
        public async Task<IActionResult> CheckAuth(
            [FromBody] LoginMFA login,
            [FromServices] AttemptMap checkAuthTracker)
        {
            const int MAX_FAILS = 3;
            var WINDOW = TimeSpan.FromSeconds(60);
            
            login.OTP = Regex.Replace(login.OTP ?? "", "[^0-9]", "");
            var correlationId = Activity.Current?.Id ?? HttpContext.TraceIdentifier ?? Guid.NewGuid().ToString("N");
            Response.Headers["X-Correlation-ID"] = correlationId;
            
            var key = EndpointThrottle.BuildKey(HttpContext, login.UserId);

            MfaLogHelper.Info("Incoming MFA verify request", new { 
                correlationId, login.UserId, otpLen = login.OTP?.Length ?? 0, Ip = HttpContext.Connection.RemoteIpAddress?.ToString() 
            });
            
            if (EndpointThrottle.ShouldThrottle(checkAuthTracker, key, MAX_FAILS, WINDOW,
                out var retryAfter, out var current, out var left))
            {
                Response.Headers["Retry-After"] = retryAfter.ToString();
                MfaLogHelper.Info("MFA verify throttled", new { correlationId, login.UserId, current, limit = MAX_FAILS, left, retryAfter });
                return StatusCode(429, new {
                    error = $"Terlalu banyak kode TOTP salah. Coba lagi dalam {retryAfter} detik.",
                    attemptsInWindow = current, limit = MAX_FAILS, windowSecondsLeft = left, retryAfter
                });
            }

            MfaLogHelper.Info("Incoming MFA verify request", new {
                correlationId,
                login.UserId,
                login.UserName,
                otpLen = login.OTP?.Length ?? 0,
                Ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UA = Request.Headers.UserAgent.ToString()
            });

            var isValid = await _mfaService.VerifyTotpAsync(login.UserId, login.OTP);

            MfaLogHelper.Info("Verification result", new { correlationId, login.UserId, isValid });
            
            if (!isValid)
            {
                EndpointThrottle.RegisterFailure(checkAuthTracker, key, WINDOW);

                var (cnt, left2) = EndpointThrottle.GetState(checkAuthTracker, key, WINDOW);
                
                if (cnt >= MAX_FAILS)
                {
                    Response.Headers["Retry-After"] = left2.ToString();
                    return StatusCode(429, new {
                        error = $"Terlalu banyak kode TOTP salah. Coba lagi dalam {left2} detik.",
                        attemptsInWindow = cnt, limit = MAX_FAILS, windowSecondsLeft = left2, retryAfter = left2
                    });
                }

                MfaLogHelper.Info("MFA invalid response", new { correlationId, login.UserId, attempts = cnt });
                return Ok(new {
                    status = 0,
                    data = new {
                        remark = "Invalid MFA code",
                        attemptsInWindow = cnt,
                        limit = MAX_FAILS,
                        windowSecondsLeft = left2,
                        remainingBeforeLock = Math.Max(0, MAX_FAILS - cnt)
                    }
                });
            }

            EndpointThrottle.Reset(checkAuthTracker, key);

            string token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            await _mfaService.UpdateTokenAndLoginStatusAsync(login.UserId, token);

            MfaLogHelper.Info("MFA success, token issued", new { correlationId, login.UserId });

            return Ok(new { status = 1, data = new { url = "/Home/Index" } });
        }
        
        private string GenerateDummyMaskedEmail(string userId)
        {
            var local = !string.IsNullOrWhiteSpace(userId) ? userId.Substring(0, Math.Min(3, userId.Length)) : "usr";
            return $"{local}...@example.com";
        }
        
        private string MaskEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                return "unknown...@domain.com";
            
            var parts = email.Split('@');
            var local = parts[0].Length > 3 ? parts[0].Substring(0, 3) : parts[0];
            return $"{local}...@{parts[1]}";
        }
        
        private string GenerateOtpAuthUrl(string secret, string username, string issuer)
        {
            if (string.IsNullOrWhiteSpace(issuer) ||
                    issuer.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Invalid MFA issuer configuration.");

            var accountName = $"{username} (Astra Daihatsu Motor)";
            
            var uri = new OtpUri(
                OtpType.Totp,
                secret,
                accountName,
                issuer
            );

            return uri.ToString();
        }
    
        private string GenerateQRCodeBase64(string otpAuthUrl)
        {
            using var generator = new QRCodeGenerator();
            using var qrCodeData = generator.CreateQrCode(otpAuthUrl, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new Base64QRCode(qrCodeData);
            return qrCode.GetGraphic(5);
        }
    }
}