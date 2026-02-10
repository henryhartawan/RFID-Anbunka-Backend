using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data.SqlClient;
using System.Data;
using OfficeOpenXml;

namespace RFIDP2P3_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class MasterGIP2Controller : Controller
	{
		private readonly string _configuration;
		private string? remarks = "";

		public MasterGIP2Controller(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterGIP2>> INQ()
		{
			List<MasterGIP2> ContainerObj = new();

			using (SqlConnection conn = new(_configuration))
			{
				conn.Open();
				SqlCommand cmd = new("exec sp_M_GIP2_Sel", conn);
				cmd.CommandType = CommandType.Text;

				SqlDataReader sdr = cmd.ExecuteReader();
				while (sdr.Read())
				{
					ContainerObj.Add(new MasterGIP2
					{
						GIP2Id  = sdr["GIP2Id"].ToString(),
						PlantId  = sdr["PlantId"].ToString(),
						PlantName  = sdr["PlantName"].ToString(),
						ShopId  = sdr["ShopId"].ToString(),
						ShopName  = sdr["ShopName"].ToString(),
						DeptId  = sdr["DeptId"].ToString(),
						DeptName  = sdr["DeptName"].ToString(),
						CostCenter  = sdr["CostCenter"].ToString(),
						UserUpdate  = sdr["UserUpdate"].ToString(),
						DateUpdate = sdr["DateUpdate"].ToString()
					});
				}
				conn.Close();
			}
			return ContainerObj;
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterGIP2>> INS(MasterGIP2 paramObj)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_GIP2_Ins", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;

				cmd.Parameters.Add(new("@IUType", paramObj.IUType));
				cmd.Parameters.Add(new("@GIP2Id", paramObj.GIP2Id));
				cmd.Parameters.Add(new("@DeptId", paramObj.DeptId));
				cmd.Parameters.Add(new("@CostCenter", paramObj.CostCenter));
				cmd.Parameters.Add(new("@UserLogin", paramObj.UserLogin));

				conn.Open();
				remarks = cmd.ExecuteScalar().ToString();
				conn.Close();
			}

			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterGIP2>> DEL(MasterGIP2 paramObj)
		{
			using (SqlConnection conn = new(_configuration))
			{
				conn.Open();
				SqlCommand cmd = new("exec sp_M_GIP2_Del @GIP2ID", conn);
				cmd.CommandType = CommandType.Text;
				cmd.Parameters.Add(new("@GIP2ID", paramObj.GIP2Id));
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
					if("success" != remarks)
					{
						b.WriteLog(remarks, "XLSRemarks");
					}
					else
					{
						using (SqlConnection conn = new(_configuration))
						{
							conn.Open();
							SqlCommand cmd = new("exec sp_M_GIP2_Upload @EntryUser", conn);
							cmd.CommandType = CommandType.Text;
							cmd.Parameters.Add(new("@EntryUser", UID));
							cmd.ExecuteScalar();
							conn.Close();
						}

					}
					list.Add(new RemarksNote { Remarks = remarks});
					return list;
				}
			}
		}
	}
}
