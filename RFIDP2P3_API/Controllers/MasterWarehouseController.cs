using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class MasterWarehouseController : Controller
	{
		private readonly string _configuration;
		private string? remarks = "";

		public MasterWarehouseController(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterWarehouse>> INQ()
		{
			List<MasterWarehouse> warehouse = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Warehouse_Sel", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				conn.Open();
				SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
				{
                    warehouse.Add(new MasterWarehouse
                    {
                        WarehouseID = sdr["WarehouseID"].ToString(),
                        PlantCode = sdr["PlantCode"].ToString(),
                        LineOrderCode = sdr["LineOrderCode"].ToString(),
                        SLoc = sdr["SLoc"].ToString(),
                        Warehouse = sdr["Warehouse"].ToString(),
                        Description = sdr["Description"].ToString(),
                        WarehouseStatus = sdr["WarehouseStatus"].ToString(),
						LastUpdate = sdr["DateUpdate"].ToString(),
						UserUpdate = sdr["UserUpdate"].ToString(),
						Remarks = sdr["Remarks"].ToString()
                    });
				}
				conn.Close();
			}
			return warehouse;
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterWarehouse>> INS(MasterWarehouse warehouse)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Warehouse_Ins", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@IUType", warehouse.IUType));
				cmd.Parameters.Add(new("@WarehouseID", warehouse.WarehouseID));
                cmd.Parameters.Add(new("@PlantCode", warehouse.PlantCode));
                cmd.Parameters.Add(new("@LineOrderCode", warehouse.LineOrderCode));
                cmd.Parameters.Add(new("@SLoc", warehouse.SLoc));
                cmd.Parameters.Add(new("@Warehouse", warehouse.Warehouse));
                cmd.Parameters.Add(new("@Description", warehouse.Description));
                cmd.Parameters.Add(new("@UserLogin", warehouse.UserLogin));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterWarehouse>> DEL(MasterWarehouse warehouse)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Warehouse_Del", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@WarehouseID", warehouse.WarehouseID));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterWarehouse>> ACT(MasterWarehouse warehouse)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_Warehouse_Act", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@WarehouseID", warehouse.WarehouseID));
                cmd.Parameters.Add(new("@UserLogin", warehouse.UserLogin));

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
