using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using RFIDP2P3_API.Models.Request;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ResultCapacityController : ControllerBase
    {
        private readonly string _configuration;

        public ResultCapacityController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<Dictionary<string,object>>> INQ([FromBody] ResultCapacityRequest request)
        {
            var dt = new DataTable();
            string periodeId = request?.Periode ?? "";

            if (string.IsNullOrEmpty(periodeId))
                return Ok(new List<Dictionary<string, object>>());

            using (SqlConnection conn = new SqlConnection(_configuration))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_Inq_T_Calc_Capacity", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Periode_ID", periodeId);
                    
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
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

            return Ok(result);
        }
    }
}
