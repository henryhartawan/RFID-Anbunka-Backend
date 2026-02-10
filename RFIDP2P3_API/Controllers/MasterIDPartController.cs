using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class MasterIDPartController : Controller
	{
		private readonly string _configuration;
		private string? remarks = "";

		public MasterIDPartController(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterIDPart>> INQ()
		{
			List<MasterIDPart> IDPart = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_ID_Part_Sel", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				conn.Open();
				SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
				{
                    IDPart.Add(new MasterIDPart
                    {
                        ExCore = sdr["ExCore"].ToString(),
                        DepthID = sdr["DepthID"].ToString(),
                        Depth = sdr["Depth"].ToString(),
                        PartOrderID = sdr["PartOrderID"].ToString(),
                        Part = sdr["Part"].ToString(),
                        DockCode = sdr["DockCode"].ToString(),
                        Dock = sdr["Dock"].ToString(),
                        MinCapacity = sdr["MinCapacity"].ToString(),
                        MaxCapacity = sdr["MaxCapacity"].ToString(),
                        OrderTime = sdr["OrderTime"].ToString(),
                        CycleIssueX = sdr["CycleIssueX"].ToString(),
                        CycleIssueY = sdr["CycleIssueY"].ToString(),
                        CycleIssueZ = sdr["CycleIssueZ"].ToString(),
                        EffectiveDate = sdr["EffectiveDate"].ToString(),
                        EffectiveDateView = sdr["EffectiveDateView"].ToString(),
                        EffectiveShift = sdr["EffectiveShift"].ToString(),
                        IDPartStatus = sdr["IDPartStatus"].ToString(),
						LastUpdate = sdr["DateUpdate"].ToString(),
						UserUpdate = sdr["UserUpdate"].ToString(),
						Remarks = sdr["Remarks"].ToString()
                    });
				}
				conn.Close();
			}
			return IDPart;
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterIDPart>> INS(MasterIDPart idPart)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_ID_Part_Ins", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@IUType", idPart.IUType));
				cmd.Parameters.Add(new("@ExCore", idPart.ExCore));
				cmd.Parameters.Add(new("@DepthID", idPart.DepthID));
				cmd.Parameters.Add(new("@PartOrderID", idPart.PartOrderID));
				cmd.Parameters.Add(new("@DockCode", idPart.DockCode));
				cmd.Parameters.Add(new("@MinCapacity", idPart.MinCapacity));
				cmd.Parameters.Add(new("@MaxCapacity", idPart.MaxCapacity));
                cmd.Parameters.Add(new("@OrderTime", idPart.OrderTime));
                cmd.Parameters.Add(new("@CycleIssueX", idPart.CycleIssueX));
				cmd.Parameters.Add(new("@CycleIssueY", idPart.CycleIssueY));
				cmd.Parameters.Add(new("@CycleIssueZ", idPart.CycleIssueZ));
				cmd.Parameters.Add(new("@EffectiveDate", idPart.EffectiveDate));
				cmd.Parameters.Add(new("@EffectiveShift", idPart.EffectiveShift));
				cmd.Parameters.Add(new("@UserLogin", idPart.UserLogin));

				conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterIDPart>> DEL(MasterIDPart idPart)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_ID_Part_Del", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@ExCore", idPart.ExCore));

				conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterIDPart>> ACT(MasterIDPart idPart)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_ID_Part_Act", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@ExCore", idPart.ExCore));
				cmd.Parameters.Add(new("@UserLogin", idPart.UserLogin));

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
                            SqlCommand cmd = new("exec sp_M_ID_Part_Upload @EntryUser", conn);
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
