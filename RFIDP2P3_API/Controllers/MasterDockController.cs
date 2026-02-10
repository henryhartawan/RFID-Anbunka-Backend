using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data.SqlClient;
using System.Data;

namespace RFIDP2P3_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class MasterDockController : Controller
	{
		private readonly string _configuration;
		private string? remarks = "";

		public MasterDockController(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterDock>> INQ()
		{
			List<MasterDock> Dock = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Dock_Sel", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				conn.Open();
				SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
				{
					Dock.Add(new MasterDock
					{
                        PlantCode = sdr["PlantCode"].ToString(),
                        DockCode = sdr["DockCode"].ToString(),
                        DockName = sdr["DockName"].ToString(),
                        GapToSupply = sdr["GapToSupply"].ToString(),
                        DockStatus = sdr["DockStatus"].ToString(),
						LastUpdate = sdr["DateUpdate"].ToString(),
						UserUpdate = sdr["UserUpdate"].ToString(),
						Remarks = sdr["Remarks"].ToString()
                    });
				}
				conn.Close();
			}
			return Dock;
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterDock>> INS(MasterDock dock)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Dock_Ins", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@IUType", dock.IUType));
				cmd.Parameters.Add(new("@PlantCode", dock.PlantCode));
                cmd.Parameters.Add(new("@DockCode", dock.DockCode));
                cmd.Parameters.Add(new("@DockName", dock.DockName));
                cmd.Parameters.Add(new("@GapToSupply", dock.GapToSupply));
                cmd.Parameters.Add(new("@UserLogin", dock.UserLogin));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterDock>> DEL(MasterDock dock)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Dock_Del", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@DockCode", dock.DockCode));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterDock>> ACT(MasterDock dock)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_Dock_Act", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@DockCode", dock.DockCode));
                cmd.Parameters.Add(new("@UserLogin", dock.UserLogin));

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
