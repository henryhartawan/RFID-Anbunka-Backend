using System.Data;
using System.Data.SqlClient;
using System.Text.Json;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Helpers;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OrderDdmiHistoryController : Controller
    {
        private readonly string _connectionString;

        public OrderDdmiHistoryController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        
        [HttpPost]
        public IActionResult HistoryInq([FromBody] JsonElement body)
        {
            string periode = body.TryGetProperty("Periode", out JsonElement pEl) ? pEl.GetString() ?? "" : "";
            List<Dictionary<string, object>> historyList = new();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand("sp_Inq_T_Daily_Order_DDMI_History", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Periode", string.IsNullOrEmpty(periode) ? DateTime.Now.ToString("yyyy-MM") : periode);

                    conn.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            var dict = new Dictionary<string, object>();
                            for (int i = 0; i < sdr.FieldCount; i++)
                            {
                                string colName = sdr.GetName(i);
                                object val = sdr.GetValue(i);

                                if (val == DBNull.Value) dict[colName] = null;
                                else if (val is DateTime dt) dict[colName] = dt.ToString("yyyy-MM-dd HH:mm:ss");
                                else dict[colName] = val;
                            }
                            historyList.Add(dict);
                        }
                    }
                }
                return Ok(new { Data = historyList, Total = historyList.Count });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}