using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data.SqlClient;
using System.Data;

namespace RFIDP2P3_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class MasterProgressLaneController : Controller
	{
		private readonly string _configuration;
		private string? remarks = "";

		public MasterProgressLaneController(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterProgressLane>> INQ()
		{
			List<MasterProgressLane> pl = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Progress_Lane_Sel", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				conn.Open();
				SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
				{
                    pl.Add(new MasterProgressLane
                    {
                        ProgressLaneID = sdr["ProgressLaneID"].ToString(),
                        LineOrderCode = sdr["LineOrderCode"].ToString(),
                        Line = sdr["Line"].ToString(),
                        QtyUnitPL = sdr["QtyUnitPL"].ToString(),
                        ProgressLaneStatus = sdr["ProgressLaneStatus"].ToString(),
						LastUpdate = sdr["DateUpdate"].ToString(),
						UserUpdate = sdr["UserUpdate"].ToString(),
						Remarks = sdr["Remarks"].ToString()
                    });
				}
				conn.Close();
			}
			return pl;
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterProgressLane>> INS(MasterProgressLane pl)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Progress_Lane_Ins", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@IUType", pl.IUType));
				cmd.Parameters.Add(new("@ProgressLaneID", pl.ProgressLaneID));
                cmd.Parameters.Add(new("@LineOrderCode", pl.LineOrderCode));
                cmd.Parameters.Add(new("@QtyUnitPL", pl.QtyUnitPL));
				cmd.Parameters.Add(new("@UserLogin", pl.UserLogin));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterProgressLane>> DEL(MasterProgressLane pl)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Progress_Lane_Del", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@ProgressLaneID", pl.ProgressLaneID));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterProgressLane>> ACT(MasterProgressLane pl)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_Progress_Lane_Act", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@ProgressLaneID", pl.ProgressLaneID));
                cmd.Parameters.Add(new("@UserLogin", pl.UserLogin));

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
