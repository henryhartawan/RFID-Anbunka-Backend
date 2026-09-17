using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Helpers;
using RFIDP2P3_API.Models;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using RFIDP2P3_API.Models.Request;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]

    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;

        public AuthController(IConfiguration config)
        {
            _config = config;
            _connectionString = config.GetConnectionString("DefaultConnection")!;
        }

        [HttpPost]
        public IActionResult Refresh([FromBody] TokenApiRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.AccessToken) || string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest("Invalid client request");

            var principal = JwtHelper.GetPrincipalFromExpiredToken(request.AccessToken, _config);
            if (principal == null) return BadRequest("Invalid access token");

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                         principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            var userName = principal.FindFirst(JwtRegisteredClaimNames.Name)?.Value ??
                           principal.FindFirst("name")?.Value;

            if (string.IsNullOrEmpty(userId)) return BadRequest("Invalid token claims");

            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            int tokenId = 0;
            DateTime expiresUtc = DateTime.MinValue;
            DateTime? revokedUtc = null;

            using (SqlCommand cmd = new SqlCommand(
                "SELECT Id, ExpiresUtc, RevokedUtc FROM UserRefreshTokens WHERE Token = @Token AND UserId = @UserId", conn))
            {
                cmd.Parameters.AddWithValue("@Token", request.RefreshToken);
                cmd.Parameters.AddWithValue("@UserId", userId);

                using SqlDataReader reader = cmd.ExecuteReader();
                if (!reader.Read())
                    return Unauthorized("Refresh token is invalid.");

                tokenId = Convert.ToInt32(reader["Id"]);
                expiresUtc = Convert.ToDateTime(reader["ExpiresUtc"]);
                revokedUtc = reader["RevokedUtc"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["RevokedUtc"]) : null;
            }

            if (revokedUtc != null || expiresUtc <= DateTime.UtcNow)
                return Unauthorized("Refresh token has expired or been revoked. Please login again.");

            var userMock = new User
            {
                PIC_ID = userId,
                PIC_Name = userName
            };

            var newAccessToken = JwtHelper.GenerateToken(userMock, _config);
            var newRefreshToken = JwtHelper.GenerateRefreshToken();
            var newExpiry = DateTime.UtcNow.AddDays(_config.GetValue<int>("JWT:RefreshTokenExpireDays", 7));

            using (SqlCommand cmdUpdate = new SqlCommand(
                @"UPDATE UserRefreshTokens SET RevokedUtc = GETUTCDATE(), ReplacedByToken = @NewToken WHERE Id = @Id;
                  INSERT INTO UserRefreshTokens (UserId, Token, ExpiresUtc, CreatedUtc) 
                  VALUES (@UserId, @NewToken, @ExpiresUtc, GETUTCDATE());", conn))
            {
                cmdUpdate.Parameters.AddWithValue("@NewToken", newRefreshToken);
                cmdUpdate.Parameters.AddWithValue("@Id", tokenId);
                cmdUpdate.Parameters.AddWithValue("@UserId", userId);
                cmdUpdate.Parameters.AddWithValue("@ExpiresUtc", newExpiry);

                cmdUpdate.ExecuteNonQuery();
            }

            return Ok(new
            {
                token = newAccessToken,
                refreshToken = newRefreshToken
            });
        }
    }
}