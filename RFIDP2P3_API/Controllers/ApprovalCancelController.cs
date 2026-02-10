using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data.SqlClient;
using System.Data;
using static IronOcr.OcrResult;
using System;
using OfficeOpenXml;

namespace RFIDP2P3_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class ApprovalCancelController : Controller
	{
		private readonly string _configuration;
		private string? remarks = "";

		public ApprovalCancelController(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}

		[HttpPost]
		public ActionResult<IEnumerable<ApprovalCancel>> INQ(ApprovalCancel paramObj)
		{
			List<ApprovalCancel> ContainerObj = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_ApprovalCancel_Sel", conn))
            {
				cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@From", paramObj.From));
                cmd.Parameters.Add(new("@To", paramObj.To));
                conn.Open();
                SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
				{
					ContainerObj.Add(new ApprovalCancel
                    {
                        ID = sdr["ID"].ToString(),
                        ScanType = sdr["ScanType"].ToString(),
                        SAP_Doc_No = sdr["SAP_Doc_No"].ToString(),
                        LDK_No = sdr["LDK_No"].ToString(),
                        RFIDNo = sdr["RFIDNo"].ToString(),
                        PalletTypeName = sdr["PalletTypeName"].ToString(),
                        PartNumber = sdr["PartNumber"].ToString(),
                        PartDescription = sdr["PartDescription"].ToString(),
                        KanbanNumber = sdr["KanbanNumber"].ToString(),
                        ProcessArea = sdr["ProcessArea"].ToString(),
                        ProcessDate = sdr["ProcessDate"].ToString(),
                        UserProposed = sdr["UserProposed"].ToString(),
                        DateProposed = sdr["DateProposed"].ToString(),
                        Reason = sdr["Reason"].ToString(),
                        CancelStatus = sdr["CancelStatus"].ToString(),
                        UserApprove = sdr["UserApprove"].ToString(),
						CancelDate = sdr["CancelDate"].ToString(),
						SAP_Cancel_Doc_No = sdr["SAP_Cancel_Doc_No"].ToString(),
						BuildingName = sdr["BuildingName"].ToString(),
						PlantId = sdr["PlantId"].ToString()
					});
				}
				conn.Close();
			}
			return ContainerObj;
        }

        [HttpPost]
        public ActionResult<IEnumerable<ApprovalCancel>> App(ApprovalCancel paramObj)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_ApprovalCancel_Ins", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@ID", paramObj.ID));
                cmd.Parameters.Add(new("@ScanType", paramObj.ScanType));
                cmd.Parameters.Add(new("@RFIDNo", paramObj.RFIDNo));
                cmd.Parameters.Add(new("@Approval", paramObj.Approval));
                cmd.Parameters.Add(new("@UserLogin", paramObj.UserLogin));
                cmd.Parameters.Add(new("@CancelNote", paramObj.Reason));

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
