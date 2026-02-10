using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data.SqlClient;
using System.Data;
using static IronOcr.OcrResult;
using System;
using OfficeOpenXml;

namespace RFIDP2P3_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class MasterApprovalLDKController : Controller
	{
		private readonly string _configuration;
		private string? remarks = "";

		public MasterApprovalLDKController(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterApprovalLDK>> INQ()
		{
			List<MasterApprovalLDK> ContainerObj = new();

			using (SqlConnection conn = new(_configuration))
			{
				conn.Open();
				SqlCommand cmd = new("exec sp_M_Approval_LDK_Sel", conn);
				cmd.CommandType = CommandType.Text;

				SqlDataReader sdr = cmd.ExecuteReader();
				while (sdr.Read())
				{
					ContainerObj.Add(new MasterApprovalLDK
					{
						AppLDKId  = sdr["AppLDKId"].ToString(),
						ShopId  = sdr["ShopId"].ToString(),
						ShopName  = sdr["ShopName"].ToString(),
						DeptId  = sdr["DeptId"].ToString(),
						DeptName  = sdr["DeptName"].ToString(),
                        Approval1_ID = sdr["Approval1_ID"].ToString(),
                        Approval1_Name = sdr["Approval1_Name"].ToString(),
                        Approval2_ID = sdr["Approval2_ID"].ToString(),
                        Approval2_Name = sdr["Approval2_Name"].ToString(),
                        Approval3_ID = sdr["Approval3_ID"].ToString(),
                        Approval3_Name = sdr["Approval3_Name"].ToString(),
                        Approval4_ID = sdr["Approval4_ID"].ToString(),
                        Approval4_Name = sdr["Approval4_Name"].ToString(),
                        UserUpdate  = sdr["UserUpdate"].ToString(),
						DateUpdate = sdr["DateUpdate"].ToString()
					});
				}
				conn.Close();
			}
			return ContainerObj;
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterApprovalLDK>> INS(MasterApprovalLDK paramObj)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Approval_LDK_Ins", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;

				cmd.Parameters.Add(new("@IUType", paramObj.IUType));
				cmd.Parameters.Add(new("@AppLDKId", paramObj.AppLDKId));
				cmd.Parameters.Add(new("@ShopId", paramObj.ShopId));
				cmd.Parameters.Add(new("@DeptId", paramObj.DeptId));
				cmd.Parameters.Add(new("@Approval1", paramObj.Approval1_ID));
				cmd.Parameters.Add(new("@Approval2", paramObj.Approval2_ID));
				cmd.Parameters.Add(new("@Approval3", paramObj.Approval3_ID));
				cmd.Parameters.Add(new("@Approval4", paramObj.Approval4_ID));
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
		public ActionResult<IEnumerable<MasterApprovalLDK>> DEL(MasterApprovalLDK paramObj)
		{
			using (SqlConnection conn = new(_configuration))
			{
				conn.Open();
				SqlCommand cmd = new("exec sp_M_Approval_LDK_Del @AppLDKid", conn);
				cmd.CommandType = CommandType.Text;
				cmd.Parameters.Add(new("@AppLDKid", paramObj.AppLDKId));
				remarks = cmd.ExecuteScalar().ToString();
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
		}
	}
}
