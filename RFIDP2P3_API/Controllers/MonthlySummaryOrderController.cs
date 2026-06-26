using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Text.Json;
using RFIDP2P3_API.Models.Request;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MonthlySummaryOrderController : ControllerBase
    {
        private readonly string _configuration;
        private string? remarks = "";

        public MonthlySummaryOrderController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<Dictionary<string, object>>> INQ([FromBody] JsonElement body)
        {
            var dt = new DataTable();
            var result = new List<Dictionary<string, object>>();

            string uploadDate = body.TryGetProperty("UploadDate", out var p) ? p.GetString() ?? "" : "";
            int revisionNo = body.TryGetProperty("RevisionNo", out var r) ? r.GetInt32() : -1;
            
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Inq_T_Monthly_Summary_Order", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Periode_ID", uploadDate);
                cmd.Parameters.AddWithValue("@RevisionNo", revisionNo);
                
                conn.Open();
                using (var da = new SqlDataAdapter(cmd))
                    da.Fill(dt);
            }

            if (dt.Columns.Contains("Message") && dt.Rows.Count > 0)
                return Ok(new List<object>());

            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    string safeColName = int.TryParse(col.ColumnName, out _) ? "Day_" + col.ColumnName : col.ColumnName;
                    dict[safeColName] = row[col] == DBNull.Value ? 0 : row[col];
                }
                
                result.Add(dict);
            }

            return Ok(result);
        }
        
        [HttpPost]
        public ActionResult CheckMismatch([FromBody] JsonElement body)
        {
            string message = "";
            bool isMismatch = false;

            string uploadDate = body.TryGetProperty("UploadDate", out var p) ? p.GetString() ?? "" : "";
            int revisionNo = body.TryGetProperty("RevisionNo", out var r) ? r.GetInt32() : -1;
            
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Check_Mismatch_Monthly_Summary", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Periode_ID", uploadDate);
                cmd.Parameters.AddWithValue("@RevisionNo", revisionNo);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    List<string> mismatchDetails = new List<string>();
                    while (reader.Read())
                    {
                        string suffix = reader["SuffixCode"].ToString();
                        string monthlyQty = reader["Monthly_Qty"].ToString();
                        string summaryQty = reader["Summary_N_Qty"].ToString();
                
                        mismatchDetails.Add($"- Suffix <b>{suffix}</b> (Monthly: {monthlyQty} vs Summary: {summaryQty})");
                    }

                    if (mismatchDetails.Count > 0)
                    {
                        isMismatch = true;
                        message = "Data mismatch detected between Monthly Order and Summary Order!<br/><br/>" + 
                                  string.Join("<br/>", mismatchDetails);
                    }
                }
            }

            return Ok(new { isMismatch = isMismatch, message = message });
        }
        
        [HttpPost]
        public IActionResult GetRevisions([FromBody] JsonElement body)
        {
            string periode = body.TryGetProperty("UploadDate", out var p) ? p.GetString() ?? "" : "";
            
            if (periode.Length == 6 && !periode.Contains("-"))
                periode = periode.Substring(0, 4) + "-" + periode.Substring(4, 2);

            List<int> revisions = new List<int>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration))
                using (SqlCommand cmd = new SqlCommand("sp_M_Customer_Order_GetRevisions", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Periode", periode);

                    conn.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            revisions.Add(Convert.ToInt32(sdr["RevisionNo"]));
                        }
                    }
                    conn.Close();
                }
                return Ok(revisions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
