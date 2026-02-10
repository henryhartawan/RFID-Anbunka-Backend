using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class MasterCPLController : Controller
	{
		private readonly string _configuration;
		private string? remarks = "";

		public MasterCPLController(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterCPL>> INQ()
		{
			List<MasterCPL> cpl = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_CPL_Sel", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				conn.Open();
				SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
				{
                    cpl.Add(new MasterCPL
                    {
                        CPLID = sdr["CPLID"].ToString(),
                        ExCore = sdr["ExCore"].ToString(),
                        Core = sdr["Core"].ToString(),
                        Part = sdr["Part"].ToString(),
                        Supplier = sdr["Supplier"].ToString(),
                        Depth = sdr["Depth"].ToString(),
                        UniqueLineID = sdr["UniqueLineID"].ToString(),
                        UniqueLine = sdr["UniqueLine"].ToString(),
                        QtyPerUnit = sdr["QtyPerUnit"].ToString(),
                        LastUpdate = sdr["DateUpdate"].ToString(),
						UserUpdate = sdr["UserUpdate"].ToString(),
                        CPLStatus = sdr["CPLStatus"].ToString(),
                        Remarks = sdr["Remarks"].ToString()
                    });
				}
				conn.Close();
			}
			return cpl;
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterCPL>> INS(MasterCPL cpl)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_CPL_Ins", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@IUType", cpl.IUType));
                cmd.Parameters.Add(new("@CPLID", cpl.CPLID));
                cmd.Parameters.Add(new("@ExCore", cpl.ExCore));
                cmd.Parameters.Add(new("@UniqueLineID", cpl.UniqueLineID));
                cmd.Parameters.Add(new("@QtyPerUnit", cpl.QtyPerUnit));
                cmd.Parameters.Add(new("@UserLogin", cpl.UserLogin));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterCPL>> DEL(MasterCPL cpl)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_CPL_Del", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@CPLID", cpl.CPLID));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterCPL>> ACT(MasterCPL cpl)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_CPL_Act", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@CPLID", cpl.CPLID));
                cmd.Parameters.Add(new("@UserLogin", cpl.UserLogin));

                conn.Open();
                cmd.ExecuteNonQuery();
                remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
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
                            SqlCommand cmd = new("exec sp_M_CPL_Upload @EntryUser", conn);
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
