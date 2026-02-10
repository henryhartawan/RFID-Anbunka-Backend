using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MasterParamJamController : ControllerBase
    {
        private readonly string _configuration;
        private string? remarks = "";

        public MasterParamJamController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<Dictionary<string, object>>> INQ()
        {
            var dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_Param_Jam_Sel", conn))
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;

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
        public ActionResult<IEnumerable<MasterParamJam>> INS(MasterParamJam pr)
        {
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Param_Jam_Ins", conn))
			{
                cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@Param_ID", pr.Param_ID));
                cmd.Parameters.Add(new("@Jam_Awal_Day", pr.Jam_Awal_Day));
				cmd.Parameters.Add(new("@Jam_Awal_Night", pr.Jam_Awal_Night));
                cmd.Parameters.Add(new("@Jam_Kerja_1_Shift", pr.Jam_Kerja_1_Shift));
                cmd.Parameters.Add(new("@Jam_Kerja_2_Shift", pr.Jam_Kerja_2_Shift));
                cmd.Parameters.Add(new("@Gap_To_Supply", pr.Gap_To_Supply));
                cmd.Parameters.Add(new("@UserLogin", pr.UserLogin));

				conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
            if (remarks != "") return BadRequest(remarks);
            else return Ok("success");

		}
    }
}
