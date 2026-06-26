using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;
using RFIDP2P3_API.Models.Request;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProcessMonthlyPlanController : ControllerBase
    {
        private readonly string _configuration;

        public ProcessMonthlyPlanController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        [HttpPost]
        public ActionResult<IEnumerable<Dictionary<string,object>>> INQ()
        {
            var dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_configuration))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM T_Status_Monthly_Plan ORDER BY Calc_Date DESC", conn);
                cmd.CommandType = CommandType.Text;
                
                using (var da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            var result = new List<Dictionary<string, object>>();

            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
                }
                result.Add(dict);
            }

            return Ok(result);
        }

        [HttpPost]
        public ActionResult<string> Calc([FromBody] ProcessMonthlyPlan request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Periode_ID))
                    return BadRequest("Period is required.");
                
                if (!DateTime.TryParseExact(request.Periode_ID, "yyyyMM", null, System.Globalization.DateTimeStyles.None, out DateTime parsedPeriode))
                    return BadRequest("Invalid period format. Expected YYYYMM.");

                DateTime currentMonthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                // if (parsedPeriode <= currentMonthStart)
                //     return BadRequest("Process Rejected: You can only calculate data for the next month or future periods.");
                
                string remarks = "";
                using (SqlConnection conn = new SqlConnection(_configuration))
                {
                    conn.Open();
                    
                    string spName = request.Calc_Type == "TargetStock" ? "sp_Calc_Target_Stock" : "sp_Calc_Monthly_Plan";
                    using (SqlCommand cmd = new SqlCommand(spName, conn))
                    {
                        var userId = User.FindFirst("PIC_ID")?.Value ?? "System";

                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@Periode", request.Periode_ID); 
                        cmd.Parameters.AddWithValue("@User_Login", userId);

                        object result = cmd.ExecuteScalar();
                        remarks = result != null ? result.ToString() : "error";
                    }
                }
                
                if (remarks.ToLower() != "success") return BadRequest(remarks);
                return Ok("success");
            }
            catch (Exception ex)
            {
                return BadRequest($"A system error occurred while processing the reset request. {ex.Message}");
            }
        }

        [HttpPost]
        public ActionResult<string> Reset([FromBody] ProcessMonthlyPlan request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Periode_ID))
                    return BadRequest("Period is required.");

                if (!DateTime.TryParseExact(request.Periode_ID, "yyyyMM", null, System.Globalization.DateTimeStyles.None, out DateTime parsedPeriode))
                    return BadRequest("Invalid period format. Expected YYYYMM.");
                
                DateTime currentMonthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                
                // if (parsedPeriode <= currentMonthStart)
                //     return BadRequest("Reset Rejected: You can only reset data for the next month or future periods.");
                

                string remarks = "";
                using (SqlConnection conn = new SqlConnection(_configuration))
                {
                    conn.Open();

                    string spName = request.Calc_Type == "TargetStock" ? "sp_Calc_Reset_Target_Stock" : "sp_Calc_Reset_Monthly_Plan";
                    using (SqlCommand cmd = new SqlCommand(spName, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Periode", request.Periode_ID);
                        cmd.Parameters.AddWithValue("@User_Login", request.User_Login ?? "System");

                        object result = cmd.ExecuteScalar();
                        remarks = result != null ? result.ToString() : "error";
                    }
                }
                
                if (remarks.ToLower() != "success") return BadRequest(remarks);
                return Ok("success");
            }
            catch (Exception ex)
            {
                return BadRequest("A system error occurred while processing the reset request.");
            }
        }
    }
}