using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityController : ControllerBase
    {
        private readonly string _configuration;
        private string? remarks = "";

        public SecurityController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult Index(MasterUser user)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(user.password);

            using (SqlConnection conn = new(_configuration))
            {
                conn.Open();
                SqlCommand cmd = new("exec sp_UserPass_Ins @PIC_ID, @Passwords", conn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new("@PIC_ID", user.PIC_ID));
                cmd.Parameters.Add(new("@Passwords", passwordHash));
                remarks = cmd.ExecuteScalar().ToString();
                conn.Close();
            }
            if (remarks != "success") return BadRequest(remarks);
            else return Ok(remarks);
        }
    }
}
