using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data.SqlClient;
using System.Data;
using System.Net.NetworkInformation;
using OfficeOpenXml;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GRProController : Controller
    {
        private readonly string _configuration;
        private string? remarks = "";

        public GRProController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<GRPro>> INQ(GRPro paramObj)
        {
            List<GRPro> ContainerObj = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_GRPro_Sel", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@GRPro", paramObj.GRProNo));
                cmd.Parameters.Add(new("@PostingDate", paramObj.PostingDate));
                conn.Open();
                SqlDataReader sdr = cmd.ExecuteReader();

                while (sdr.Read())
                {
                    ContainerObj.Add(new GRPro
                    {
                        PartNumber = sdr["PartNumber"].ToString(),
                        PartName = sdr["PartName"].ToString(),
                        PartDesc = sdr["PartDesc"].ToString(),
                        PartQty = sdr["PartQty"].ToString(),
                        PostingDate = sdr["PostingDate"].ToString(),
                        GRProNo = sdr["GRProNo"].ToString(),
                        SAPStatus = sdr["SAPStatus"].ToString(),
                        SAPOutFile = sdr["SAPOutFile"].ToString(),
                        SAPInFile = sdr["SAPInFile"].ToString(),
                        SAPGRMatDocNo = sdr["SAPGRMatDocNo"].ToString(),
                        ErrorMessage = sdr["ErrorMessage"].ToString()
                    });
                }
                conn.Close();
            }
            return ContainerObj;
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
                        using (SqlConnection conn = new(_configuration))
                        {
                            conn.Open();
                            SqlCommand cmd = new("exec sp_M_GRPro_Upload @EntryUser", conn);
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.Add(new("@EntryUser", UID));
                            cmd.ExecuteScalar();
                            conn.Close();
                        }

                    }
                    list.Add(new RemarksNote { Remarks = remarks });
                    return list;
                }
            }
        }

        [HttpPost]
        public ActionResult<IEnumerable<GRPro>> Resync(GRPro grpro)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_GRPro_Resync", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@GRProNo", grpro.GRProNo));
                cmd.Parameters.Add(new("@PartNumber", grpro.PartNumber));
                cmd.Parameters.Add(new("@PostingDate", grpro.PostingDate));
                cmd.Parameters.Add(new("@UserLogin", grpro.UserLogin));

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
