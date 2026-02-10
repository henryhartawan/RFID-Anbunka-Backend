using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data.SqlClient;
using System.Data;

namespace RFIDP2P3_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class MasterUniqueLineController : Controller
	{
		private readonly string _configuration;
		private string? remarks = "";

		public MasterUniqueLineController(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterUniqueLine>> INQ()
		{
			List<MasterUniqueLine> uniqueLines = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Unique_Line_Sel", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				conn.Open();
				SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
				{
                    uniqueLines.Add(new MasterUniqueLine
                    {
                        UniqueLineID = sdr["UniqueLineID"].ToString(),
                        LineOrderCode = sdr["LineOrderCode"].ToString(),
                        Line = sdr["Line"].ToString(),
                        UniqueNumber = sdr["UniqueNumber"].ToString(),
                        FinishGoods = sdr["FinishGoods"].ToString(),
                        LastUpdate = sdr["DateUpdate"].ToString(),
						UserUpdate = sdr["UserUpdate"].ToString(),
                        UniqueLineStatus = sdr["UniqueLineStatus"].ToString(),
                        Remarks = sdr["Remarks"].ToString()
                    });
				}
				conn.Close();
			}
			return uniqueLines;
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterUniqueLine>> INS(MasterUniqueLine uniqueLines)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Unique_Line_Ins", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@IUType", uniqueLines.IUType));
				cmd.Parameters.Add(new("@LineOrderCode", uniqueLines.LineOrderCode));
                cmd.Parameters.Add(new("@UniqueNumber", uniqueLines.UniqueNumber));
                cmd.Parameters.Add(new("@UserLogin", uniqueLines.UserLogin));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterUniqueLine>> DEL(MasterUniqueLine uniqueLines)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Unique_Line_Del", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@UniqueLineID", uniqueLines.UniqueLineID));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterUniqueLine>> ACT(MasterUniqueLine uniqueLines)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_Unique_Line_Act", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@UniqueLineID", uniqueLines.UniqueLineID));
                cmd.Parameters.Add(new("@UserLogin", uniqueLines.UserLogin));

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
