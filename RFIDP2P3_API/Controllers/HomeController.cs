using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly string _configuration;
        private string? remarks = "";

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<Home>> INQ(Home home)
        {
            object result;
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Inq_Current_Trip", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@PIC_ID", home.PIC_ID));

                conn.Open();
                result = cmd.ExecuteScalar();
                conn.Close();
            }
            remarks = result.ToString();
            return Ok(remarks);
        }
    }
}
