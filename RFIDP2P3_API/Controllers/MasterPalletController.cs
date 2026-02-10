using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Identity;
using OfficeOpenXml;

namespace RFIDP2P3_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class MasterPalletController : Controller
	{
		private readonly string _configuration;
		private string remarks = "";

		public MasterPalletController(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterPallet>> INQ()
		{
			List<MasterPallet> ContainerObj = new();

			using (SqlConnection conn = new(_configuration))
			{
				conn.Open();
				SqlCommand cmd = new("exec sp_M_Pallet_Sel", conn);
				cmd.CommandType = CommandType.Text;

				SqlDataReader sdr = cmd.ExecuteReader();
				while (sdr.Read())
				{
					ContainerObj.Add(new MasterPallet
					{
						RFIDNo= sdr["RFIDNo"].ToString(),
						PalletTypeID= sdr["PalletTypeID"].ToString(),
						PalletTypeName= sdr["PalletTypeName"].ToString(),
						UserUpdate= sdr["UserUpdate"].ToString(),
						DateUpdate= sdr["DateUpdate"].ToString(),
						Remarks = sdr["Remarks"].ToString()
					});
				}
				conn.Close();
			}
			return ContainerObj;
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterPallet>> INS(MasterPallet paramObj)
		{
			using (SqlConnection conn = new(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Pallet_Ins", conn))
			{

				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@IUType", paramObj.IUType));
				cmd.Parameters.Add(new("@RFIDNo", paramObj.RFIDNo));
				cmd.Parameters.Add(new("@PalletType", paramObj.PalletTypeID));
				cmd.Parameters.Add(new("@UserLogin", paramObj.UserLogin));


				conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}

			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterPallet>> DEL(MasterPallet paramObj)
		{
			using (SqlConnection conn = new(_configuration))
			{
				conn.Open();
				SqlCommand cmd = new("exec sp_M_Pallet_Del @RFIDNo", conn);
				cmd.CommandType = CommandType.Text;
				cmd.Parameters.Add(new("@RFIDNo", paramObj.RFIDNo));
				remarks = cmd.ExecuteScalar().ToString();
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
						using (SqlConnection conn = new(_configuration))
						{
							conn.Open();
							SqlCommand cmd = new("exec sp_M_Pallet_Upload @EntryUser", conn);
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
	}
}
