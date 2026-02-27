using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ScanDriverController : ControllerBase
    {
        private readonly string _configuration;
        private string? remarks = "";

        public ScanDriverController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<ScanTN>> CheckTN(ScanTN st)
        {
            object result;
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Inq_T_TN_Scan", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@Type", st.Type));
                cmd.Parameters.Add(new("@TN_No", st.TN_No));
                cmd.Parameters.Add(new("@UserLogin", st.UserLogin));

                conn.Open();
                result = cmd.ExecuteScalar();
                conn.Close();
            }
            remarks = result.ToString();
            return Ok(remarks);
        }

        [HttpPost]
        public ActionResult<IEnumerable<ScanTN>> INS(ScanTN st)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Submit_T_TN_Scan_Driver", conn))
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@TN_No", st.TN_No));
                cmd.Parameters.Add(new("@NoPol", st.NoPol));
                cmd.Parameters.Add(new("@UserLogin", st.UserLogin));

                cmd.ExecuteNonQuery();
                remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
                conn.Close();
            }
            if (remarks != "") return BadRequest(remarks);
            else return Ok("success");
        }
    }
}
