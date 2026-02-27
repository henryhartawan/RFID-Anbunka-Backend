using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ScanSupplyController : ControllerBase
    {
        private readonly string _configuration;
        private string? remarks = "";

        public ScanSupplyController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<ScanSupply>> CheckKanban(ScanSupply ss)
        {
            object result;
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Inq_T_Scan_Supply", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@Type", SqlDbType.VarChar, 50));
                cmd.Parameters["@Type"].Value = "Kanban";
                cmd.Parameters.Add(new("@Kanban_No", ss.Kanban_No));
                cmd.Parameters.Add(new("@ExCore", ss.ExCore));
                cmd.Parameters.Add(new("@UserLogin", ss.UserLogin));

                conn.Open();
                result = cmd.ExecuteScalar();
                conn.Close();
            }
            remarks = result.ToString();
            return Ok(remarks);
        }

        [HttpPost]
        public ActionResult<IEnumerable<ScanSupply>> CheckExcore(ScanSupply ss)
        {
            object result;
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Inq_T_Scan_Supply", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@Type", SqlDbType.VarChar, 50));
                cmd.Parameters["@Type"].Value = "ExCore";
                cmd.Parameters.Add(new("@Kanban_No", ss.Kanban_No));
                cmd.Parameters.Add(new("@ExCore", ss.ExCore));
                cmd.Parameters.Add(new("@UserLogin", ss.UserLogin));

                conn.Open();
                result = cmd.ExecuteScalar();
                conn.Close();
            }
            remarks = result.ToString();
            return Ok(remarks);
        }

        [HttpPost]
        public ActionResult<IEnumerable<ScanSupply>> INS(ScanSupply ss)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Submit_T_Scan_Supply", conn))
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@Kanban_No", ss.Kanban_No));
                cmd.Parameters.Add(new("@ExCore", ss.ExCore));
                cmd.Parameters.Add(new("@UserLogin", ss.UserLogin));

                cmd.ExecuteNonQuery();
                remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
                conn.Close();
            }
            if (remarks != "") return BadRequest(remarks);
            else return Ok("success");
        }
    }
}
