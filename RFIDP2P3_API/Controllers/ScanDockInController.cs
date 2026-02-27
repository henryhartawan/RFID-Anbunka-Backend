using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ScanDockInController : ControllerBase
    {
        private readonly string _configuration;
        private string? remarks = "";

        public ScanDockInController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<Dictionary<string, object>>> INQ()
        {
            var dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Inq_T_TN_Scan_Dock_In", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();

                using (var da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }

                conn.Close();
            }

            var result = new List<Dictionary<string, object>>();

            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                result.Add(dict);
            }

            return result;
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

                conn.Open();
                result = cmd.ExecuteScalar();
                conn.Close();
            }
            remarks = result.ToString();
            return Ok(remarks);
        }

        [HttpPost]
        public ActionResult<IEnumerable<ScanTN>> INS(ScanTN sd)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Submit_T_TN_Scan_Dock_In", conn))
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@TN_No", sd.TN_No));
                cmd.Parameters.Add(new("@UserLogin", sd.UserLogin));

                cmd.ExecuteNonQuery();
                remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
                conn.Close();
            }
            if (remarks != "") return BadRequest(remarks);
            else return Ok("success");
        }
    }
}
