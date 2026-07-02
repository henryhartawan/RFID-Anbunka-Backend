using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using RFIDP2P3_API.Models.Request;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MonthlyPlanResultsController : ControllerBase
    {
        private readonly string _configuration;

        public MonthlyPlanResultsController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult GetResultList([FromBody] JsonElement body)
        {
            try
            {
                string periode = body.TryGetProperty("Periode", out var p) ? p.GetString() ?? "" : "";
                int revisionNo = body.TryGetProperty("RevisionNo", out var r) ? r.GetInt32() : -1;

                if (string.IsNullOrEmpty(periode))
                    return BadRequest("Period is required.");

                if (!DateTime.TryParseExact(periode, "yyyyMM", null, System.Globalization.DateTimeStyles.None, out _))
                    return BadRequest("Invalid period format. Expected YYYYMM.");

                List<Dictionary<string, object>> resultList = new List<Dictionary<string, object>>();

                using (SqlConnection conn = new SqlConnection(_configuration))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_Inq_Monthly_Plan_Results", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Periode", periode);
                        cmd.Parameters.AddWithValue("@RevisionNo", revisionNo);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var row = new Dictionary<string, object>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row[reader.GetName(i)] = reader.GetValue(i) == DBNull.Value ? null : reader.GetValue(i);
                                }
                                resultList.Add(row);
                            }
                        }
                    }
                }

                return Ok(resultList);
            }
            catch (Exception ex)
            {
                return BadRequest($"A system error occurred while fetching the monthly plan results. {ex.Message}");
            }
        }
        
        [HttpPost]
        public IActionResult GetRevisions([FromBody] JsonElement body)
        {
            string periode = body.TryGetProperty("Periode", out var p) ? p.GetString() ?? "" : "";
            
            if (periode.Length == 6 && !periode.Contains("-"))
            {
                periode = periode.Substring(0, 4) + "-" + periode.Substring(4, 2);
            }

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
        
        [HttpPost]
        public async Task<ActionResult> UpdateDetailOutput([FromBody] JsonElement body)
        {
            var requests = System.Text.Json.JsonSerializer.Deserialize<List<UpdateDetailOutputRequest>>(body.GetRawText());

            if (requests == null || !requests.Any()) 
                return BadRequest("No data to update");

            string periodeId = requests.First().Periode;
            int revisionNo = requests.First().RevisionNo;
            
            string jsonData = System.Text.Json.JsonSerializer.Serialize(requests);
            var userLogin = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? "Admin";

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Update_Monthly_Plan_Unique", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@Periode", SqlDbType.VarChar, 6) { Value = periodeId });
                cmd.Parameters.Add(new SqlParameter("@RevisionNo", SqlDbType.Int) { Value = revisionNo });
                cmd.Parameters.Add(new SqlParameter("@JsonData", SqlDbType.NVarChar, -1) { Value = jsonData });
                cmd.Parameters.Add(new SqlParameter("@User_Login", SqlDbType.NVarChar, 50) { Value = userLogin });

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }

            return Ok(new { success = true });
        }
    }
}
