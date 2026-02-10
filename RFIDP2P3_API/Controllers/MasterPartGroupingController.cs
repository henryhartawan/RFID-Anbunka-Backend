using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class MasterPartGroupingController : Controller
	{
		private readonly string _configuration;
		private string? remarks = "";

		public MasterPartGroupingController(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterPartGrouping>> INQ()
		{
			List<MasterPartGrouping> pg = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Part_Grouping_Sel", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				conn.Open();
				SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
				{
                    pg.Add(new MasterPartGrouping
                    {
                        PartGroupingID = sdr["PartGroupingID"].ToString(),
                        ExCore = sdr["ExCore"].ToString(),
                        Core = sdr["Core"].ToString(),
                        TypeGroupPart = sdr["TypeGroupPart"].ToString(),
                        GroupingID = sdr["GroupingID"].ToString(),
                        Grouping = sdr["Grouping"].ToString(),
                        BatchOrderBox = sdr["BatchOrderBox"].ToString(),
                        PartGroupingStatus = sdr["PartGroupingStatus"].ToString(),
						LastUpdate = sdr["DateUpdate"].ToString(),
						UserUpdate = sdr["UserUpdate"].ToString(),
						Remarks = sdr["Remarks"].ToString()
                    });
				}
				conn.Close();
			}
			return pg;
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterPartGrouping>> INS(MasterPartGrouping pg)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Part_Grouping_Ins", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@IUType", pg.IUType));
				cmd.Parameters.Add(new("@PartGroupingID", pg.PartGroupingID));
                cmd.Parameters.Add(new("@ExCore", pg.ExCore));
                cmd.Parameters.Add(new("@TypeGroupPart", pg.TypeGroupPart));
                cmd.Parameters.Add(new("@GroupingID", pg.GroupingID));
                cmd.Parameters.Add(new("@BatchOrderBox", pg.BatchOrderBox));
                cmd.Parameters.Add(new("@UserLogin", pg.UserLogin));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterPartGrouping>> DEL(MasterPartGrouping pg)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Part_Grouping_Del", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@PartGroupingID", pg.PartGroupingID));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterPartGrouping>> ACT(MasterPartGrouping pg)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_Part_Grouping_Act", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@PartGroupingID", pg.PartGroupingID));
                cmd.Parameters.Add(new("@UserLogin", pg.UserLogin));

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
                            SqlCommand cmd = new("exec sp_M_Part_Grouping_Upload @EntryUser", conn);
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
    }
}
