using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data.SqlClient;
using System.Data;
using static IronOcr.OcrResult;
using System.Security.Principal;
using OfficeOpenXml;

namespace RFIDP2P3_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class MasterGIP3Controller : Controller
	{
		private readonly string _configuration;
		private string? remarks = "";

		public MasterGIP3Controller(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}
		[HttpPost]
		public ActionResult<IEnumerable<MasterGIP3>> INQ()
		{
			List<MasterGIP3> ContainerObj = new();

			using (SqlConnection conn = new(_configuration))
			{
				conn.Open();
				SqlCommand cmd = new("exec sp_M_GIP3_Sel", conn);
				cmd.CommandType = CommandType.Text;

				SqlDataReader sdr = cmd.ExecuteReader();
				while (sdr.Read())
				{
					ContainerObj.Add(new MasterGIP3
					{
						GIP3Id= sdr["GIP3Id"].ToString(),
						PlantId= sdr["PlantId"].ToString(),
						PlantName= sdr["PlantName"].ToString(),
						BuildingId= sdr["BuildingId"].ToString(),
						BuildingName= sdr["BuildingName"].ToString(),
						PartNumber= sdr["PartNumber"].ToString(),
						PartDescription= sdr["PartDescription"].ToString(),
						PartName= sdr["PartName"].ToString(),
						GLAccount= sdr["GLAccount"].ToString(),
						CostCenter= sdr["CostCenter"].ToString(),
						MovementType= sdr["MovementType"].ToString(),
						PlantFrom= sdr["PlantFrom"].ToString(),
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
		public ActionResult<IEnumerable<MasterGIP3>> INS(MasterGIP3 paramObj)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_GIP3_Ins", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add(new("@IUType", paramObj.@IUType));
				cmd.Parameters.Add(new("@GIP3Id", paramObj.@GIP3Id));
				cmd.Parameters.Add(new("@PlantId", paramObj.PlantId));
				cmd.Parameters.Add(new("@BuildingId", paramObj.BuildingId));
				cmd.Parameters.Add(new("@PartNumber", paramObj.PartNumber));
				cmd.Parameters.Add(new("@GLAccount", paramObj.GLAccount));
				cmd.Parameters.Add(new("@CostCenter", paramObj.CostCenter));
				cmd.Parameters.Add(new("@Movement", paramObj.MovementType));
				cmd.Parameters.Add(new("@PlantFrom", paramObj.PlantFrom));
				cmd.Parameters.Add(new("@UserLogin", paramObj.UserLogin));

				conn.Open();
				cmd.ExecuteNonQuery();
				remarks = cmd.ExecuteScalar().ToString();
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterGIP3>> DEL(MasterGIP3 paramObj)
		{
			using (SqlConnection conn = new(_configuration))
			{
				conn.Open();
				SqlCommand cmd = new("exec sp_M_GIP3_Del @GIP3ID", conn);
				cmd.CommandType = CommandType.Text;
				cmd.Parameters.Add(new("@GIP3ID", paramObj.GIP3Id));
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
							SqlCommand cmd = new("exec sp_M_GIP3_Upload @EntryUser", conn);
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
