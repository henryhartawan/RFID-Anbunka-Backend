using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data.SqlClient;
using System.Data;

namespace RFIDP2P3_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class MasterLineOrderController : Controller
	{
		private readonly string _configuration;
		private string? remarks = "";

		public MasterLineOrderController(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterLineOrder>> INQ()
		{
			List<MasterLineOrder> LineOrder = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Line_Order_Sel", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				conn.Open();
				SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
				{
                    LineOrder.Add(new MasterLineOrder
                    {
                        DockCode = sdr["DockCode"].ToString(),
                        Dock = sdr["Dock"].ToString(),
                        LineOrderCode = sdr["LineOrderCode"].ToString(),
                        LineOrderName = sdr["LineOrderName"].ToString(),
                        BuildingID = sdr["BuildingID"].ToString(),
                        Building = sdr["Building"].ToString(),
                        LineOrderStatus = sdr["LineOrderStatus"].ToString(),
						LastUpdate = sdr["DateUpdate"].ToString(),
						UserUpdate = sdr["UserUpdate"].ToString(),
						Remarks = sdr["Remarks"].ToString()
                    });
				}
				conn.Close();
			}
			return LineOrder;
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterLineOrder>> INS(MasterLineOrder lineOrder)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Line_Order_Ins", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@IUType", lineOrder.IUType));
                cmd.Parameters.Add(new("@DockCode", lineOrder.DockCode));
                cmd.Parameters.Add(new("@LineOrderCode", lineOrder.LineOrderCode));
                cmd.Parameters.Add(new("@LineOrderName", lineOrder.LineOrderName));
                cmd.Parameters.Add(new("@BuildingID", lineOrder.BuildingID));
                cmd.Parameters.Add(new("@UserLogin", lineOrder.UserLogin));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterLineOrder>> DEL(MasterLineOrder lineOrder)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Line_Order_Del", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@LineOrderCode", lineOrder.LineOrderCode));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterLineOrder>> ACT(MasterLineOrder lineOrder)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_Line_Order_Act", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@LineOrderCode", lineOrder.LineOrderCode));
                cmd.Parameters.Add(new("@UserLogin", lineOrder.UserLogin));

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
