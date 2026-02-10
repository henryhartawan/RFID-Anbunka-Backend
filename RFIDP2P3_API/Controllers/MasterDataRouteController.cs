using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data.SqlClient;
using System.Data;

namespace RFIDP2P3_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class MasterDataRouteController : Controller
	{
		private readonly string _configuration;
		private string? remarks = "";

		public MasterDataRouteController(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterDataRoute>> INQ()
		{
			List<MasterDataRoute> DataRoute = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Data_Route_Sel", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				conn.Open();
				SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
				{
                    DataRoute.Add(new MasterDataRoute
                    {
                        PlantCode = sdr["PlantCode"].ToString(),
                        SupplierCode = sdr["SupplierCode"].ToString(),
                        Supplier = sdr["Supplier"].ToString(),
                        RouteCode = sdr["RouteCode"].ToString(),
                        RouteName = sdr["RouteName"].ToString(),
                        Capacity = sdr["Capacity"].ToString(),
                        FlagGRSAP = sdr["FlagGRSAP"].ToString(),
                        FlagView = sdr["FlagView"].ToString(),
                        LastUpdate = sdr["DateUpdate"].ToString(),
						UserUpdate = sdr["UserUpdate"].ToString(),
                        RouteStatus = sdr["RouteStatus"].ToString(),
                        Remarks = sdr["Remarks"].ToString()
                    });
				}
				conn.Close();
			}
			return DataRoute;
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterDataRoute>> INS(MasterDataRoute dataRoute)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Data_Route_Ins", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@IUType", dataRoute.IUType));
                cmd.Parameters.Add(new("@PlantCode", dataRoute.PlantCode));
                cmd.Parameters.Add(new("@SupplierCode", dataRoute.SupplierCode));
                cmd.Parameters.Add(new("@RouteCode", dataRoute.RouteCode));
                cmd.Parameters.Add(new("@RouteName", dataRoute.RouteName));
                cmd.Parameters.Add(new("@Capacity", dataRoute.Capacity));
                cmd.Parameters.Add(new("@FlagGRSAP", dataRoute.FlagGRSAP));
                cmd.Parameters.Add(new("@UserLogin", dataRoute.UserLogin));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterDataRoute>> DEL(MasterDataRoute dataRoute)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Data_Route_Del", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@RouteCode", dataRoute.RouteCode));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterDataRoute>> ACT(MasterDataRoute dataRoute)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_Data_Route_Act", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@RouteCode", dataRoute.RouteCode));
                cmd.Parameters.Add(new("@UserLogin", dataRoute.UserLogin));

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
