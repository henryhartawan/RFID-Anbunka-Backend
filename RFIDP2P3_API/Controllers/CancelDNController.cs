using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CancelDNController : ControllerBase
    {
        private readonly string _configuration;
        private string? remarks = "";

        public CancelDNController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<DN>> CheckDN(DN dn)
        {
            object result;
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Inq_Cancel_DN_Check", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@DN_No", dn.DN_No));

                conn.Open();
                result = cmd.ExecuteScalar();
                conn.Close();
            }
            remarks = result.ToString();
            return Ok(remarks);
        }

        [HttpPost]
        public ActionResult<IEnumerable<Dictionary<string,object>>> DNDetail(DN dn)
        {
            var dt = new DataTable();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_Inq_Cancel_DN_Detail", conn))
			{
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@DN_No", dn.DN_No));
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
        public ActionResult<IEnumerable<GR>> INS(GR dn)
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Submit_Cancel_DN", conn))
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@DN_No", SqlDbType.VarChar, 50));
                cmd.Parameters.Add(new("@PartNo", SqlDbType.VarChar, 50));
                cmd.Parameters.Add(new("@EntryDate", SqlDbType.VarChar, 20));
                cmd.Parameters.Add(new("@UserLogin", SqlDbType.VarChar, 50));

                foreach (var dns in dn.DN)
                {
                    cmd.Parameters["@DN_No"].Value = dn.DN_No;
                    cmd.Parameters["@PartNo"].Value = dns.PartNo;
                    cmd.Parameters["@EntryDate"].Value = now;
                    cmd.Parameters["@UserLogin"].Value = dn.UserLogin;

                    cmd.ExecuteNonQuery();
                    remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);

                    if (remarks != "")
                    {
                        conn.Close();
                        return BadRequest(remarks);
                    }
                }
                conn.Close();
            }
            if (remarks != "") return BadRequest(remarks);
            else return Ok("success");
        }


        [HttpPost]
        public async Task<List<RemarksNote>> Upload(IFormFile file, string? UID)
        {
            var list = new List<RemarksNote>();
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    BusinessObject b = new();
                    string remarks = b.UploadXLS(package, UID, _configuration);
                    if ("success" != remarks)
                    {
                        b.WriteLog(remarks, "XLSRemarks");
                    }
                    else
                    {
                        object result;
                        using (SqlConnection conn = new(_configuration))
                        {
                            conn.Open();
                            SqlCommand cmd = new("exec sp_Upload_Cancel_DN @EntryUser", conn);
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.Add(new("@EntryUser", UID));
                            result = cmd.ExecuteScalar();
                            conn.Close();
                        }
                        remarks = result.ToString();
                    }
                    list.Add(new RemarksNote { Remarks = remarks });
                    return list;
                }
            }
        }
    }
}
