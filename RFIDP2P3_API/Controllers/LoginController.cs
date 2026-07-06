using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using RFIDP2P3_API.Helpers;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly string _configuration;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private string? remarks = "";

        private static readonly ConcurrentDictionary<string, (int Attempts, DateTime LastAttempt)> _failedLoginTracker = new();

        public LoginController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _config = configuration;
            _configuration = configuration.GetConnectionString("DefaultConnection");
            _env = env;
        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterUser>> Index([FromBody] MasterUser Login, [FromQuery] bool isStressTest = false)
        {
            if (_env.IsDevelopment() && isStressTest) 
            {
                var mockUser = new User
                {
                    PIC_ID = Login.PIC_ID ?? "TESTER",
                    PIC_Name = "Stress Tester",
                    UserGroup_Id = "UG_TEST",
                    UserGroup_Name = "Tester Group",
                    PlantId = "PLANT_1",
                    MFAStatus = "0",
                    Privileges = new List<Privilege>()
                };

                string tokenString = JwtHelper.GenerateToken(mockUser, _config);
                return Ok(new { requireMfa = false, token = tokenString, user = mockUser });
            }
            
            string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "UnknownIP";
            var now = DateTime.UtcNow;

            if (_failedLoginTracker.TryGetValue(ip, out var entry))
            {
                if (entry.Attempts >= 2 && (now - entry.LastAttempt).TotalSeconds < 120)
                {
                    var wait = 120 - (int)(now - entry.LastAttempt).TotalSeconds;
                    return StatusCode(429, new { message = $"Terlalu banyak percobaan login gagal. Coba lagi dalam {wait} detik." });
                }
            }
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_UserLogin_Sel", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@UserId", Login.PIC_ID));

                conn.Open();
                SqlDataReader sdr = cmd.ExecuteReader();

                string? PIC_ID = "";
                string? PIC_Name = "";
                string? Pwd = "";
				string? UserGroup_Id = "";
				string? UserGroup_Name = "";
				string? PlantId = "";
                string? SupplierCode = "";
                string? MFAStatus = "";

                List<User> userLogin = new();

                while (sdr.Read())
                {
                    Pwd = sdr["Passwords"].ToString();
                    PIC_ID = sdr["UserID"].ToString();
                    PIC_Name = sdr["UserName"].ToString();
					UserGroup_Id = sdr["UserGroupID"].ToString();
					UserGroup_Name = sdr["UserGroupName"].ToString();
					PlantId = sdr["PlantId"].ToString();
                    SupplierCode = sdr["SupplierCode"].ToString();
                    MFAStatus = sdr["MFAStatus"].ToString();
                }

                if (!sdr.HasRows)
                {
                    sdr.Close();
                    conn.Close();

                    _failedLoginTracker.AddOrUpdate(ip, (1, now),
                        (key, old) => (now - old.LastAttempt).TotalSeconds > 120 ? (1, now) : (old.Attempts + 1, now));

                    return BadRequest("User not found/not active");
                }
                else if (!BCrypt.Net.BCrypt.Verify(Login.password, Pwd))
                {
                    sdr.Close();
                    conn.Close();

                    _failedLoginTracker.AddOrUpdate(ip, (1, now),
                        (key, old) => (now - old.LastAttempt).TotalSeconds > 120 ? (1, now) : (old.Attempts + 1, now));

                    return BadRequest("Incorrect login/password");
                }
                else
                {
                    _failedLoginTracker.TryRemove(ip, out _);

                    sdr.Close();

                    List<Privilege> privileges = new();
                    using (SqlCommand cmd1 = new SqlCommand("sp_UserAccess_Sel", conn))
                    {
                        cmd1.CommandType = CommandType.StoredProcedure;
                        cmd1.Parameters.Add(new("@UserId", Login.PIC_ID));
                        SqlDataReader sdr1 = cmd1.ExecuteReader();
                        if (sdr1.FieldCount > 0)
                        {
                            while (sdr1.Read())
                            {
                                privileges.Add(new Privilege
                                {
                                    Menu_Id = sdr1["MenuName"].ToString(),
                                    checkedbox_read = sdr1["AllowAccess"].ToString(),
                                    checkedbox_add = sdr1["AllowSubmit"].ToString(),
                                    checkedbox_edit = sdr1["AllowUpdate"].ToString(),
                                    checkedbox_del = sdr1["AllowDelete"].ToString()
                                });
                            }
                            userLogin.Add(new User
                            {
                                PIC_ID = PIC_ID,
                                PIC_Name = PIC_Name,
								UserGroup_Id = UserGroup_Id,
								UserGroup_Name = UserGroup_Name,
								PlantId = PlantId,
                                SupplierCode = SupplierCode,
                                Privileges = privileges,
                                MFAStatus = MFAStatus
                            });
                        }
                        sdr1.Close();
                        conn.Close();
                    }
                   
                    //List<User> userLogin = new();
                    //using (SqlCommand cmd2 = new SqlCommand("sp_UserLogin_Sel", conn))
                    //{
                    //    cmd2.Parameters.Add(new("@PIC_ID", Userlogin));
                    //    sdr = cmd2.ExecuteReader();
                    //    while (sdr.Read())
                    //    {
                    //        userLogin.Add(new User
                    //        {
                    //            PIC_ID = sdr["UserID"].ToString(),
                    //            PIC_Name = sdr["PIC_Name"].ToString(),
                    //            UserGroup_Id = sdr["UserGroupID"].ToString(),
                    //            Privileges = privileges
                    //        });
                    //    }
                    //    conn.Close();
                    //}
                    
                    var loggedUser = userLogin.FirstOrDefault();
                    bool requireMfa = loggedUser.MFAStatus?.ToLower() == "true" || loggedUser.MFAStatus == "1";

                    if (requireMfa)
                        return Ok(new { requireMfa = true, user = loggedUser });
                    else
                    {
                        string tokenString = JwtHelper.GenerateToken(loggedUser, _config);
                        return Ok(new { requireMfa = false, token = tokenString, user = loggedUser });
                    }

                    return Ok(userLogin);
                }
            }
        }
        
        [HttpPost]
        public IActionResult Logout([FromServices] IMemoryCache cache)
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
    
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
        
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(token))
                {
                    var jwtToken = handler.ReadJwtToken(token);
                    var exp = jwtToken.ValidTo;
                    var timeRemaining = exp - DateTime.UtcNow;

                    if (timeRemaining > TimeSpan.Zero)
                    {
                        cache.Set(token, "Revoked", timeRemaining);
                    }
                }
            }
            return Ok(new
            {
                success = true, 
                message = "Logout successful. Token invalidated."
            });
        }
    }
}
