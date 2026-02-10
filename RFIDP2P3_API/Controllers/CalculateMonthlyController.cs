using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CalculateMonthlyController : ControllerBase
    {
        private readonly string _configuration;
        private string? remarks = "";

        public CalculateMonthlyController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<Dictionary<string,object>>> INQ()
        {
            var dt = new DataTable();

			using (SqlConnection conn = new SqlConnection(_configuration))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM T_Status WHERE Calc_Groups IN ('Calc Monthly', 'Reset Calc Monthly') ORDER BY Calc_Date DESC", conn);
                cmd.CommandType = CommandType.Text;
                
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
        public ActionResult<IEnumerable<Calculate>> Calc(Calculate calc)
        {
            object result;
            using (SqlConnection conn = new SqlConnection(_configuration))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("sp_Jobs_Calc_Monthly", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@Periode_ID", calc.Periode_ID));
                cmd.Parameters.Add(new("@User_Login", calc.User_Login));

                result = cmd.ExecuteScalar();
                conn.Close();
            }
            remarks = result.ToString();
            if (remarks != "success") return BadRequest(remarks);
            else return Ok("success");
        }

        [HttpPost]
        public ActionResult<IEnumerable<Calculate>> Reset(Calculate calc)
        {
            object result;
            using (SqlConnection conn = new SqlConnection(_configuration))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("sp_Calc_Reset_Monthly", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@Periode_ID", calc.Periode_ID));
                cmd.Parameters.Add(new("@UserLogin", calc.User_Login));

                result = cmd.ExecuteScalar();
                conn.Close();
            }
            remarks = result.ToString();
            if (remarks != "success") return BadRequest(remarks);
            else return Ok("success");
        }
    }
}
