using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;
using RFIDP2P3_API.Models.Request;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SummaryOrderController : ControllerBase
    {
        private readonly string _configuration;
        private string? remarks = "";

        public SummaryOrderController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<Dictionary<string, object>>> INQ([FromBody] FirmOrder request)
        {
            var dt = new DataTable();
            var result = new List<Dictionary<string, object>>();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Inq_T_Summary_Order", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Periode_ID", request.UploadDate);
                
                conn.Open();
                using (var da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            if (dt.Columns.Contains("Message") && dt.Rows.Count > 0)
            {
                return Ok(new List<object>());
            }

            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    dict[col.ColumnName] = row[col] == DBNull.Value ? 0 : row[col];
                }
                result.Add(dict);
            }

            return Ok(result);
        }
    }
}
