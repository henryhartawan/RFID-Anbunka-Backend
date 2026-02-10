using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class MasterCalendarController : Controller
	{
		private readonly string _configuration;
		private string? remarks = "";

		public MasterCalendarController(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterCalendar>> INQ(MasterCalendar cal)
		{
			List<MasterCalendar> calendar = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Calendar_Sel", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@LineOrderCode", cal.LineOrderCode));
                cmd.Parameters.Add(new("@MonthYear", cal.MonthYear));

                conn.Open();
				SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
				{
                    calendar.Add(new MasterCalendar
                    {
                        ExCore = sdr["ExCore"].ToString(),
                        LineOrderCode = sdr["LineOrderCode"].ToString(),
                        Line = sdr["Line"].ToString(),
                        CalendarDate = sdr["CalendarDate"].ToString(),
                        DateView = sdr["DateView"].ToString(),
                        Shift = sdr["Shift"].ToString(),
                        CalendarStatus = sdr["CalendarStatus"].ToString(),
                        CalendarStat = sdr["CalendarStat"].ToString(),
                        LastUpdate = sdr["DateUpdate"].ToString(),
						UserUpdate = sdr["UserUpdate"].ToString(),
						Remarks = sdr["Remarks"].ToString()
                    });
				}
				conn.Close();
			}
			return calendar;
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterCalendar>> INS(MasterCalendar calendar)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Calendar_Ins", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@IUType", calendar.IUType));
				cmd.Parameters.Add(new("@ExCore", calendar.ExCore));
                cmd.Parameters.Add(new("@LineOrderCode", calendar.LineOrderCode));
                cmd.Parameters.Add(new("@CalendarDate", calendar.CalendarDate));
                cmd.Parameters.Add(new("@Shift", calendar.Shift));
                cmd.Parameters.Add(new("@Status", calendar.CalendarStatus));
                cmd.Parameters.Add(new("@UserLogin", calendar.UserLogin));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterCalendar>> DEL(MasterCalendar calendar)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Calendar_Del", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@ExCore", calendar.ExCore));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterCalendar>> Cal(MasterCalendar cal)
        {
            List<MasterCalendar> calendar = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_Calendar_Cal", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@LineOrderCode", cal.LineOrderCode));
                cmd.Parameters.Add(new("@MonthYear", cal.MonthYear));

                conn.Open();
                SqlDataReader sdr = cmd.ExecuteReader();

                while (sdr.Read())
                {
                    calendar.Add(new MasterCalendar
                    {
                        title = sdr["title"].ToString(),
                        start = sdr["start"].ToString(),
                        backgroundColor = sdr["backgroundColor"].ToString(),
                        borderColor = sdr["borderColor"].ToString(),
                        textColor = sdr["textColor"].ToString()
                    });
                }
                conn.Close();
            }
            return calendar;
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
                            SqlCommand cmd = new("exec sp_M_Calendar_Upload @EntryUser", conn);
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
