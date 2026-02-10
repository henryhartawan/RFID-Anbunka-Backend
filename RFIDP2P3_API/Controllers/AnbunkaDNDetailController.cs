using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data.SqlClient;
using System.Data;
using System.Net.NetworkInformation;
using OfficeOpenXml;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AnbunkaDNDetailController : Controller
    {
        private readonly string _configuration;

        public AnbunkaDNDetailController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<AnbunkaDNDetail>> INQ(AnbunkaDNDetail paramObj)
        {
            List<AnbunkaDNDetail> ContainerObj = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_Anbunka_DNDetail_Sel", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@DNNo", paramObj.DNNo));
                cmd.Parameters.Add(new("@From", paramObj.From));
                cmd.Parameters.Add(new("@To", paramObj.To));
                conn.Open();
                SqlDataReader sdr = cmd.ExecuteReader();

                while (sdr.Read())
                {
                    ContainerObj.Add(new AnbunkaDNDetail
					{
                        PONo = sdr["PONo"].ToString(),
                        PartNumber = sdr["PartNumber"].ToString(),
                        PartDescription = sdr["PartDescription"].ToString(),
                        POItem = sdr["POItem"].ToString(),
                        DNNo = sdr["DNNo"].ToString(),
                        DelCycle = sdr["DelCycle"].ToString(),
                        QtyKanban = sdr["QtyKanban"].ToString(),
                        QtyPartOrder = sdr["QtyPartOrder"].ToString(),
                        DeliveryDate = sdr["DeliveryDate"].ToString(),
                        CreateDate = sdr["CreateDate"].ToString(),
                        SyncDate = sdr["SyncDate"].ToString(),
                        LastUpdate = sdr["LastUpdate"].ToString(),
                    });
                }
                conn.Close();
            }
            return ContainerObj;
        }
	}
}
