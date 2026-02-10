using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class MasterCycleIssueLPController : Controller
	{
		private readonly string _configuration;
		private string? remarks = "";

		public MasterCycleIssueLPController(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterCycleIssueLP>> INQ()
		{
			List<MasterCycleIssueLP> cil = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Cycle_Issue_LP_Sel", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				conn.Open();
				SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
				{
                    cil.Add(new MasterCycleIssueLP
                    {
                        CycleIssueID = sdr["CycleIssueID"].ToString(),
                        RouteCode = sdr["RouteCode"].ToString(),
                        DockCode = sdr["DockCode"].ToString(),
                        Dock = sdr["Dock"].ToString(),
                        Operation = sdr["Operation"].ToString(),
                        CycleArrival = sdr["CycleArrival"].ToString(),
                        TimeArrival = sdr["TimeArrival"].ToString(),
                        ImplementDate = sdr["ImplementDate"].ToString(),
                        LastUpdate = sdr["DateUpdate"].ToString(),
						UserUpdate = sdr["UserUpdate"].ToString(),
                        CycleIssueStatus = sdr["CycleIssueStatus"].ToString(),
                        Remarks = sdr["Remarks"].ToString()
                    });
				}
				conn.Close();
			}
			return cil;
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterCycleIssueLP>> INS(MasterCycleIssueLP cil)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Cycle_Issue_LP_Ins", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@IUType", cil.IUType));
				cmd.Parameters.Add(new("@CycleIssueID", cil.CycleIssueID));
                cmd.Parameters.Add(new("@RouteCode", cil.RouteCode));
                cmd.Parameters.Add(new("@DockCode", cil.DockCode));
                cmd.Parameters.Add(new("@Operation", cil.Operation));
                cmd.Parameters.Add(new("@CycleArrival", cil.CycleArrival));
                cmd.Parameters.Add(new("@TimeArrival", cil.TimeArrival));
                cmd.Parameters.Add(new("@ImplementDate", cil.ImplementDate));
                cmd.Parameters.Add(new("@UserLogin", cil.UserLogin));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterCycleIssueLP>> DEL(MasterCycleIssueLP cil)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Cycle_Issue_LP_Del", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@CycleIssueID", cil.CycleIssueID));
                cmd.Parameters.Add(new("@DockCode", cil.DockCode));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterCycleIssueLP>> ACT(MasterCycleIssueLP cil)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_Cycle_Issue_LP_Act", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@CycleIssueID", cil.CycleIssueID));
                cmd.Parameters.Add(new("@DockCode", cil.DockCode));
                cmd.Parameters.Add(new("@UserLogin", cil.UserLogin));

                conn.Open();
                cmd.ExecuteNonQuery();
                remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
                conn.Close();
            }
            if (remarks != "") return BadRequest(remarks);
            else return Ok("success");
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
                        object result;
                        using (SqlConnection conn = new(_configuration))
                        {
                            conn.Open();
                            SqlCommand cmd = new("exec sp_M_Cycle_Issue_LP_Upload @EntryUser", conn);
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.Add(new("@EntryUser", UID));
                            result = cmd.ExecuteScalar();
                            conn.Close();
                        }
                        remarks = result.ToString();
                    }
                    list.Add(new RemarksNote { Remarks = remarks });
                    return list;
                }
            }
        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterCycleIssueLP>> CheckTime(MasterCycleIssueLP cil)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_Cycle_Issue_LP_Check", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@DockCode", cil.DockCode));
                cmd.Parameters.Add(new("@RouteCode", cil.RouteCode));
                cmd.Parameters.Add(new("@Operation", cil.Operation));
                cmd.Parameters.Add(new("@CycleArrival", cil.CycleArrival));
                cmd.Parameters.Add(new("@TimeArrival", cil.TimeArrival));

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
