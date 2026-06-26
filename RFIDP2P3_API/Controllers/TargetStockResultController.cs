using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TargetStockResultController : ControllerBase
    {
        private readonly string _configuration;

        public TargetStockResultController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        [HttpPost]
        public ActionResult<IEnumerable<Dictionary<string, object>>> InqMachining([FromBody] Dictionary<string, string> request)
        {
            string periode = request.ContainsKey("Periode_ID") ? request["Periode_ID"] : "";
            string dbPeriode = periode.Substring(0, 4) + "-" + periode.Substring(4, 2);

            var dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_configuration))
            {
                conn.Open();
                string query = @"SELECT EngineBase, UniqueCode, PartName, PlanMonthly, PlanDaily, 
                                        MinDay, StdDay, MaxDay, MinUnit, StdUnit, MaxUnit 
                                 FROM T_Target_Stock_Machining 
                                 WHERE Periode = @Periode ORDER BY EngineBase, UniqueCode";
                                 
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Periode", dbPeriode);
                    using (var da = new SqlDataAdapter(cmd)) { da.Fill(dt); }
                }
            }
            return Ok(ConvertDataTable(dt));
        }

        [HttpPost]
        public ActionResult<IEnumerable<Dictionary<string, object>>> InqKLine([FromBody] Dictionary<string, string> request)
        {
            string periode = request.ContainsKey("Periode_ID") ? request["Periode_ID"] : "";
            string dbPeriode = periode.Substring(0, 4) + "-" + periode.Substring(4, 2);

            var dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_configuration))
            {
                conn.Open();
                string query = @"SELECT OrderFrom, UniqueCode, EngineType, EngineBase, PartName, 
                                        LoadingRatio, VolMonthly, VolDaily, Trip, BufferHours, OrderCycle, 
                                        TotalMinim, TotalMax 
                                 FROM T_Target_Stock_KLine 
                                 WHERE Periode = @Periode ORDER BY OrderFrom, UniqueCode";
                                 
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Periode", dbPeriode);
                    using (var da = new SqlDataAdapter(cmd)) { da.Fill(dt); }
                }
            }
            return Ok(ConvertDataTable(dt));
        }

        private List<Dictionary<string, object>> ConvertDataTable(DataTable dt)
        {
            var result = new List<Dictionary<string, object>>();
            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                    dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                result.Add(dict);
            }
            return result;
        }
    }
}