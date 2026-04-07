using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ScanBoxLabelController : ControllerBase
    {
        private readonly string _configuration;
        private string? remarks = "";

        public ScanBoxLabelController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<Dictionary<string,object>>> CheckBox(ScanBoxLabel sbl)
        {
            var dt = new DataTable();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_Inq_Kanban", conn))
			{
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@KanbanNo", sbl.KanbanNo));
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
        public ActionResult<IEnumerable<ScanBoxLabel>> INS(ScanBoxLabel sbl)
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string idStr = "";

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Submit_SKID", conn))
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@SKID_ID", SqlDbType.VarChar, 50));
                cmd.Parameters.Add(new("@KanbanNo", SqlDbType.VarChar, 50));
                cmd.Parameters.Add(new("@PI_No", SqlDbType.VarChar, 50));
                cmd.Parameters.Add(new("@DN_No", SqlDbType.VarChar, 50));
                cmd.Parameters.Add(new("@Part_No", SqlDbType.VarChar, 50));
                cmd.Parameters.Add(new("@ExCore", SqlDbType.VarChar, 50));
                cmd.Parameters.Add(new("@Job_No", SqlDbType.VarChar, 50));
                cmd.Parameters.Add(new("@KanbanSeq", SqlDbType.VarChar, 3));
                cmd.Parameters.Add(new("@EntryDate", SqlDbType.VarChar, 20));
                cmd.Parameters.Add(new("@UserLogin", SqlDbType.VarChar, 50));

                foreach (var sbls in sbl.Kanban)
                {
                    cmd.Parameters["@SKID_ID"].Value = idStr;
                    cmd.Parameters["@KanbanNo"].Value = sbls.KanbanNo;
                    cmd.Parameters["@PI_No"].Value = sbls.PI_No;
                    cmd.Parameters["@DN_No"].Value = sbls.DN_No;
                    cmd.Parameters["@Part_No"].Value = sbls.Part_No;
                    cmd.Parameters["@ExCore"].Value = sbls.ExCore;
                    cmd.Parameters["@Job_No"].Value = sbls.Job_No;
                    cmd.Parameters["@KanbanSeq"].Value = sbls.KanbanSeq;
                    cmd.Parameters["@EntryDate"].Value = now;
                    cmd.Parameters["@UserLogin"].Value = sbl.UserLogin;

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
            else return Ok(remarks.Substring(8));
        }

        [HttpPost]
        public ActionResult<IEnumerable<Dictionary<string, object>>> PrintSKID(ScanBoxLabel sbl)
        {
            var dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Inq_T_SKID_Print", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@SKID_ID", sbl.SKID_ID));
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
    }
}
