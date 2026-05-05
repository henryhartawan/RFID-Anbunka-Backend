using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
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
                Expires = DateTime.UtcNow.AddHours(config.GetValue<int>("JWT:ExpireHours")),
                SigningCredentials = creds
            };

            var handler = new JsonWebTokenHandler();
            return handler.CreateToken(descriptor);
        }
    }
}