using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GRController : ControllerBase
    {
        private readonly string _configuration;
        private string? remarks = "";

        public GRController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<Dictionary<string,object>>> DNDetail(GR gr)
        {
            var dt = new DataTable();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_Inq_GR", conn))
			{
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@Type", gr.Type));
                cmd.Parameters.Add(new("@DN_No", gr.DN_No));
                conn.Open();
                
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
        public ActionResult<IEnumerable<GR>> Vendor(GR gr)
        {
            object result;
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Inq_GR_Vendor", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@Type", gr.Type));
                cmd.Parameters.Add(new("@DN_No", gr.DN_No));

                conn.Open();
                result = cmd.ExecuteScalar();
                conn.Close();
            }
            remarks = result.ToString();
            return Ok(remarks);
        }

        [HttpPost]
        public ActionResult<IEnumerable<GR>> INS(GR gr)
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string idStr = "";

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Submit_GR", conn))
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@GRID", SqlDbType.VarChar, 50));
                cmd.Parameters.Add(new("@DN_No", SqlDbType.VarChar, 50));
                cmd.Parameters.Add(new("@PartNo", SqlDbType.VarChar, 50));
                cmd.Parameters.Add(new("@Qty", SqlDbType.VarChar));
                cmd.Parameters.Add(new("@EntryDate", SqlDbType.VarChar, 20));
                cmd.Parameters.Add(new("@UserLogin", SqlDbType.VarChar, 50));

                foreach (var grs in gr.DN)
                {
                    cmd.Parameters["@GRID"].Value = idStr;
                    cmd.Parameters["@DN_No"].Value = gr.DN_No;
                    cmd.Parameters["@PartNo"].Value = grs.PartNo;
                    cmd.Parameters["@Qty"].Value = grs.Qty;
                    cmd.Parameters["@EntryDate"].Value = now;
                    cmd.Parameters["@UserLogin"].Value = gr.UserLogin;

                    cmd.ExecuteNonQuery();
                    remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);

                    if (remarks.Substring(0,7) != "success")
                    {
                        conn.Close();
                        return BadRequest(remarks);
                    }

                    idStr = remarks.Substring(8);
                }
                conn.Close();
            }
            if (remarks.Substring(0,7) != "success") return BadRequest(remarks.Substring(6));
            else return Ok("success");
        }
    }
}
