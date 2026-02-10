using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class MasterOEETTController : Controller
	{
		private readonly string _configuration;
		private string? remarks = "";

		public MasterOEETTController(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterOEETT>> INQ()
		{
			List<MasterOEETT> oeett = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_OEE_TT_Sel", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				conn.Open();
				SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
				{
                    oeett.Add(new MasterOEETT
                    {
                        ExCore = sdr["ExCore"].ToString(),
                        LineOrderCode = sdr["LineOrderCode"].ToString(),
                        Line = sdr["Line"].ToString(),
                        OEE = sdr["OEE"].ToString(),
                        CT = sdr["CT"].ToString(),
                        ImplementDate = sdr["ImplementDate"].ToString(),
                        DateView = sdr["DateView"].ToString(),
                        OEETTStatus = sdr["OEETTStatus"].ToString(),
						LastUpdate = sdr["DateUpdate"].ToString(),
						UserUpdate = sdr["UserUpdate"].ToString(),
						Remarks = sdr["Remarks"].ToString()
                    });
				}
				conn.Close();
			}
			return oeett;
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterOEETT>> INS(MasterOEETT oeett)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_OEE_TT_Ins", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@IUType", oeett.IUType));
				cmd.Parameters.Add(new("@ExCore", oeett.ExCore));
                cmd.Parameters.Add(new("@LineOrderCode", oeett.LineOrderCode));
                cmd.Parameters.Add(new("@OEE", oeett.OEE));
                cmd.Parameters.Add(new("@CT", oeett.CT));
                cmd.Parameters.Add(new("@ImplementDate", oeett.ImplementDate));
                cmd.Parameters.Add(new("@UserLogin", oeett.UserLogin));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterOEETT>> DEL(MasterOEETT oeett)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_OEE_TT_Del", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@ExCore", oeett.ExCore));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterOEETT>> ACT(MasterOEETT oeett)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_OEE_TT_Act", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@ExCore", oeett.ExCore));
                cmd.Parameters.Add(new("@UserLogin", oeett.UserLogin));

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
