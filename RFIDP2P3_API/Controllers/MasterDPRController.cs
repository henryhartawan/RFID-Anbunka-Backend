using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class MasterDPRController : Controller
	{
		private readonly string _configuration;
		private string? remarks = "";

		public MasterDPRController(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterDPR>> INQ()
		{
			List<MasterDPR> dpr = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_DPR_Sel", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				conn.Open();
				SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
				{
                    dpr.Add(new MasterDPR
                    {
                        DPRID = sdr["DPRID"].ToString(),
                        PartOrderID = sdr["PartOrderID"].ToString(),
                        Part = sdr["Part"].ToString(),
                        Qty = sdr["Qty"].ToString(),
                        ImplementDate = sdr["ImplementDate"].ToString(),
                        DateView = sdr["DateView"].ToString(),
                        DPRStatus = sdr["DPRStatus"].ToString(),
						LastUpdate = sdr["DateUpdate"].ToString(),
						UserUpdate = sdr["UserUpdate"].ToString(),
						Remarks = sdr["Remarks"].ToString()
                    });
				}
				conn.Close();
			}
			return dpr;
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterDPR>> INS(MasterDPR dpr)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_DPR_Ins", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@IUType", dpr.IUType));
				cmd.Parameters.Add(new("@DPRID", dpr.DPRID));
                cmd.Parameters.Add(new("@PartOrderID", dpr.PartOrderID));
                cmd.Parameters.Add(new("@Qty", dpr.Qty));
                cmd.Parameters.Add(new("@ImplementDate", dpr.ImplementDate));
                cmd.Parameters.Add(new("@UserLogin", dpr.UserLogin));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterDPR>> DEL(MasterDPR dpr)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_DPR_Del", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@DPRID", dpr.DPRID));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterDPR>> ACT(MasterDPR dpr)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_DPR_Act", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@DPRID", dpr.DPRID));
                cmd.Parameters.Add(new("@UserLogin", dpr.UserLogin));

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
