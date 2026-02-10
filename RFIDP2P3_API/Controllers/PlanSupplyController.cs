using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PlanSupplyController : ControllerBase
    {
        private readonly string _configuration;
        private string? remarks = "";

        public PlanSupplyController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<Dictionary<string,object>>> INQ(PlanSupply ps)
        {
            var dt = new DataTable();

			using (SqlConnection conn = new SqlConnection(_configuration))
            {
                conn.Open();

                SqlCommand cmd;
                if (ps.Type == "PL") cmd = new SqlCommand("sp_Inq_T_Calc_Plan_Supply_PL", conn);
                else cmd = new SqlCommand("sp_Inq_T_Calc_Plan_Supply_NonPL", conn);

                cmd.CommandTimeout = 300;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@Periode_ID", ps.Prod_Date));
                cmd.Parameters.Add(new("@LineOrderCode", ps.Line));

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
    }
}
