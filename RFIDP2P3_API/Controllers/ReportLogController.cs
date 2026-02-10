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
    public class ReportLogController : Controller
    {
        private readonly string _configuration;
		private string? remarks = "";

		public ReportLogController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<ReportLog>> INQ(ReportLog paramObj)
        {
            List<ReportLog> ContainerObj = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_ReportLog_Sel", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@From", paramObj.From));
                cmd.Parameters.Add(new("@To", paramObj.To));
                conn.Open();
                SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
                {
                    ContainerObj.Add(new ReportLog
					{
						ID = sdr["ID"].ToString(),
						ScanType = sdr["ScanType"].ToString(),
						ProcessArea = sdr["ProcessArea"].ToString(),
						ProcessDate = sdr["ProcessDate"].ToString(),
						PalletType = sdr["PalletType"].ToString(),
						RFIDNo = sdr["RFIDNo"].ToString(),
						LDK_No = sdr["LDK_No"].ToString(),
						PartNumber = sdr["PartNumber"].ToString(),
						PartDescription = sdr["PartDescription"].ToString(),
						Qty = sdr["Qty"].ToString(),
						KanbanNo = sdr["KanbanNo"].ToString(),
						RFIDStatus = sdr["RFIDStatus"].ToString(),
						RFIDErrMsg = sdr["RFIDErrMsg"].ToString(),
						SAPStatus = sdr["SAPStatus"].ToString(),
						SAPDocNo = sdr["SAPDocNo"].ToString(),
						SAPErrMsg = sdr["SAPErrMsg"].ToString(),
						AnbunkaStatus = sdr["AnbunkaStatus"].ToString(),
						AnbunkaDocNo = sdr["AnbunkaDocNo"].ToString(),
						AnbunkaErrMsg = sdr["AnbunkaErrMsg"].ToString(),
						SAPButton = sdr["SAPButton"].ToString(),
						AnbunkaButton = sdr["AnbunkaButton"].ToString(),
						UserLogin = sdr["UserLogin"].ToString(),
						BuildingName = sdr["BuildingName"].ToString(),
                        HeaderTxt = sdr["HeaderTxt"].ToString(),
						Cycle = sdr["Cycle"].ToString()
					});
                }
                conn.Close();
            }
            return ContainerObj;
		}

		[HttpPost]
		public ActionResult<IEnumerable<ReportLog>> Resync(ReportLog paramObj)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_ReportLog_Resync", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@ID", paramObj.ID));
				cmd.Parameters.Add(new("@ScanType", paramObj.ScanType));
				cmd.Parameters.Add(new("@ScanDate", paramObj.ProcessDate));
				cmd.Parameters.Add(new("@RFIDNo", paramObj.RFIDNo));
				cmd.Parameters.Add(new("@Type", paramObj.Type));
				cmd.Parameters.Add(new("@UserLogin", paramObj.UserLogin));

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
