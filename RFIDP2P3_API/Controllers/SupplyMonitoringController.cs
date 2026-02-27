using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data.SqlClient;
using System.Data;
using System.Net.NetworkInformation;
using OfficeOpenXml;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SupplyMonitoringController : Controller
    {
        private readonly string _configuration;
		private string? remarks = "";

		public SupplyMonitoringController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<Dictionary<string, object>>> INQ(SupplyMonitoring sm)
        {
            var dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Inq_T_Scan_Supply_Monitoring", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@From", sm.From));
                cmd.Parameters.Add(new("@To", sm.To));
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
		public ActionResult<IEnumerable<ReportLog>> Resync(ReportLog paramObj)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_Submit_T_Scan_Supply_Resync", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@ID", paramObj.ID));
				cmd.Parameters.Add(new("@UserLogin", paramObj.UserLogin));

				conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
		}
	}
}
