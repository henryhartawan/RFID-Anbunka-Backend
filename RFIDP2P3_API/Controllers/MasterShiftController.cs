using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data.SqlClient;
using System.Data;

namespace RFIDP2P3_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class MasterShiftController : Controller
	{
		private readonly string _configuration;
		private string? remarks = "";

		public MasterShiftController(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterShift>> INQ()
		{
			List<MasterShift> ContainerObj = new();

			using (SqlConnection conn = new(_configuration))
			{
				conn.Open();
				SqlCommand cmd = new("exec sp_M_Shift_Sel", conn);
				cmd.CommandType = CommandType.Text;
				//cmd.Parameters.Add(new("@UserGroup_Id", privilege.UserGroup_Id));

				SqlDataReader sdr = cmd.ExecuteReader();
				while (sdr.Read())
				{
					ContainerObj.Add(new MasterShift
					{
						PlantID = sdr["PlantID"].ToString(),
						ShiftID = sdr["ShiftID"].ToString(),
						DayShiftEnd = sdr["DayShiftEnd"].ToString(),
						DayShiftStart = sdr["DayShiftStart"].ToString(),
						NightShiftEnd = sdr["NightShiftEnd"].ToString(),
						NightShiftStart = sdr["NightShiftStart"].ToString(),
						LastUpdate = sdr["LastUpdate"].ToString(),
						UserUpdate = sdr["UserUpdate"].ToString(),
						Remarks = sdr["Remarks"].ToString()

					});
				}
				conn.Close();
			}
			return ContainerObj;
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterShift>> INS(MasterShift paramObj)
		{
			using (SqlConnection conn = new(_configuration))
			{
				conn.Open();
				SqlCommand cmd = new("exec sp_M_Shift_Ins @IUType, @PlantID, @ShiftID, @ShiftName, @DShiftStart, @DShiftEnd, @NShiftStart, @NShiftEnd, @UserLogin", conn);
				cmd.CommandType = CommandType.Text;
				cmd.Parameters.Add(new("@IUType", paramObj.IUType));
				cmd.Parameters.Add(new("@PlantID", paramObj.PlantID));
				cmd.Parameters.Add(new("@ShiftID", paramObj.ShiftID));
				cmd.Parameters.Add(new("@ShiftName", paramObj.ShiftName));
				cmd.Parameters.Add(new("@DShiftStart", paramObj.DayShiftStart));
				cmd.Parameters.Add(new("@DShiftEnd", paramObj.DayShiftEnd));
				cmd.Parameters.Add(new("@NShiftStart", paramObj.NightShiftStart));
				cmd.Parameters.Add(new("@NShiftEnd", paramObj.NightShiftEnd));
				cmd.Parameters.Add(new("@UserLogin", paramObj.UserLogin));
				remarks = cmd.ExecuteScalar().ToString();
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
		}


		[HttpPost]
		public ActionResult<IEnumerable<MasterShift>> DEL(MasterShift paramObj)
		{
			using (SqlConnection conn = new(_configuration))
			{
				conn.Open();
				SqlCommand cmd = new("exec sp_M_Shift_Del @ShiftID", conn);
				cmd.CommandType = CommandType.Text;
				cmd.Parameters.Add(new("@ShiftID", paramObj.ShiftID));
				remarks = cmd.ExecuteScalar().ToString();
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
		}

	}
}
