using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data.SqlClient;
using System.Data;

namespace RFIDP2P3_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class MasterGedungController : Controller
	{
		private readonly string _configuration;
		private string? remarks = "";

		public MasterGedungController(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterGedung>> INQ()
		{
			List<MasterGedung> ContainerObj = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Building_Sel", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				conn.Open();
				SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
				{
					ContainerObj.Add(new MasterGedung
					{
						PlantID = sdr["PlantID"].ToString(),
						PlantName = sdr["PlantName"].ToString(),
						BuildingId = sdr["Buildingid"].ToString(),
						BuildingName = sdr["BuildingName"].ToString(),
						LastUpdate = sdr["DateUpdate"].ToString(),
						UserUpdate = sdr["UserUpdate"].ToString(),
						Remarks = sdr["Remarks"].ToString()

					});
				}
				conn.Close();
			}
			return ContainerObj;
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterGedung>> INS(MasterGedung paramObj)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Building_Ins", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
				
				cmd.Parameters.Add(new("@IUType", paramObj.IUType));
				cmd.Parameters.Add(new("@PlantID", paramObj.PlantID));
				cmd.Parameters.Add(new("@BuildingId", paramObj.BuildingId));
				cmd.Parameters.Add(new("@BuildingName", paramObj.BuildingName));
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
		public ActionResult<IEnumerable<MasterGedung>> DEL(MasterGedung paramObj)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Building_Del", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@BuildingId", paramObj.BuildingId));

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
