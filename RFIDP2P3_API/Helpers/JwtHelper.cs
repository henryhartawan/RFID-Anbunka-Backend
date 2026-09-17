using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using RFIDP2P3_API.Models;

namespace RFIDP2P3_API.Helpers
{
    public static class JwtHelper
    {
        public static string GenerateToken(User user, IConfiguration config)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = config["JWT:Issuer"],
                Audience = config["JWT:Audience"],
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.PIC_ID ?? ""),
                    new Claim(JwtRegisteredClaimNames.Name, user.PIC_Name ?? ""),
                    new Claim("UserGroup_Id", user.UserGroup_Id ?? ""),
                    new Claim("PlantId", user.PlantId ?? "")
                }),
                Expires = DateTime.UtcNow.AddHours(config.GetValue<int>("JWT:ExpireJwtMinutes")),
                SigningCredentials = creds
            };

            var handler = new JsonWebTokenHandler();
            return handler.CreateToken(descriptor);
        }

        public static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public static ClaimsPrincipal? GetPrincipalFromExpiredToken(string token, IConfiguration config)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = config["JWT:Audience"],
                ValidateIssuer = true,
                ValidIssuer = config["JWT:Issuer"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Key"]!)),
                ValidateLifetime = false
            };

            var tokenHandler = new JsonWebTokenHandler();
            var result = tokenHandler.ValidateToken(token, tokenValidationParameters);

            if (!result.IsValid) return null;

            return new ClaimsPrincipal(result.ClaimsIdentity);
        }
    }
}