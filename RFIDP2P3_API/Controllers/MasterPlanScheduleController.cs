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
	public class MasterPlanScheduleController : Controller
	{
		private readonly string _configuration;
		private string? remarks = "";

		public MasterPlanScheduleController(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterPlanSchedule>> INQ(MasterPlanSchedule usergroup)
		{
			List<MasterPlanSchedule> UserGroups = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_PlanSchedule_Sel", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				conn.Open();
				SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
				{
					UserGroups.Add(new MasterPlanSchedule
					{
						SchId = sdr["SchId"].ToString(),
						PlantId = sdr["PlantId"].ToString(),
						BuildingName = sdr["BuildingName"].ToString(),
						LineId = sdr["LineId"].ToString(),
						LineName = sdr["LineName"].ToString(),
						Cycle = sdr["Cycle"].ToString(),
						TimePulling = sdr["TimePulling"].ToString(),
						TimeDeparture = sdr["TimeDeparture"].ToString()
					});
				}
				conn.Close();
			}
			return UserGroups;
		}

        [HttpPost]
        public async Task<List<RemarksNote>> Upload(IFormFile file, string? UID)
        {
            var list = new List<RemarksNote>();
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    BusinessObject b = new();
                    string remarks = b.UploadXLS(package, UID, _configuration);
                    if ("success" != remarks)
                    {
                        b.WriteLog(remarks, "XLSRemarks");
                    }
                    else
                    {
                        using (SqlConnection conn = new(_configuration))
                        {
                            conn.Open();
                            SqlCommand cmd = new("exec sp_M_PlanSchedule_Upload @EntryUser", conn);
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.Add(new("@EntryUser", UID));
                            cmd.ExecuteScalar();
                            conn.Close();
                        }

                    }
                    list.Add(new RemarksNote { Remarks = remarks });
                    return list;
                }
            }
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterPlanSchedule>> INS(MasterPlanSchedule paramObj)
		{
			using (SqlConnection conn = new(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_PlanSchedule_Ins", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@SchId", paramObj.SchId));
				cmd.Parameters.Add(new("@TimePulling", paramObj.TimePulling));
				cmd.Parameters.Add(new("@TimeDeparture", paramObj.TimeDeparture));
				cmd.Parameters.Add(new("@UserLogin", paramObj.UserLogin));

				conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterPlanSchedule>> DEL(MasterPlanSchedule planSchedule)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_PlanSchedule_Del", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@SchId", planSchedule.SchId));

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
