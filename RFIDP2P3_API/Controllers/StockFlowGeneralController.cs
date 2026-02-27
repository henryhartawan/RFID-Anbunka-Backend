using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class StockFlowGeneralController : ControllerBase
    {
        private readonly string _configuration;
        private string? remarks = "";

        public StockFlowGeneralController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<Dictionary<string,object>>> INQ(StockFlowGeneral sfg)
        {
            var dt = new DataTable();

			using (SqlConnection conn = new SqlConnection(_configuration))
            {
                conn.Open();

                SqlCommand cmd  = new SqlCommand("sp_Inq_Stock_Flow_General", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@From", sfg.From));
                cmd.Parameters.Add(new("@To", sfg.To));
                cmd.Parameters.Add(new("@LineOrderCode", sfg.LineOrderCode));
                cmd.Parameters.Add(new("@SupplierCode", sfg.SupplierCode));

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
