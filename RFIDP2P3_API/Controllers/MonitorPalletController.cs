using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data.SqlClient;
using System.Data;

namespace RFIDP2P3_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class MonitorPalletController : Controller
	{
		private readonly string _configuration;
		private string? remarks = "";

		public MonitorPalletController(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MonitorPallet>> INQ()
		{
			List<MonitorPallet> ContainerObj = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_T_MonitorPallet_Sel", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				conn.Open();
				SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
				{
					ContainerObj.Add(new MonitorPallet
					{
                        PalletID = sdr["PalletID"].ToString(),
						PalletType = sdr["PalletType"].ToString(),
						PartNumber = sdr["PartNumber"].ToString(),
						PalletStatus = sdr["PalletStatus"].ToString()
					});
				}
				conn.Close();
			}
			return ContainerObj;
		}


	}
}
