using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;
using ClosedXML.Excel;

namespace RFIDP2P3_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class AddMasterCalendarController : Controller
	{
		private readonly string _configuration;
		private string? remarks = "";

		public AddMasterCalendarController(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}

		[HttpPost]
		public ActionResult<IEnumerable<AddMasterCalendar>> INQ(AddMasterCalendar cal)
		{
			List<AddMasterCalendar> calendar = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Add_Calendar_Sel", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@ProdDate", cal.ProdDate));

                conn.Open();
				SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
				{
                    calendar.Add(new AddMasterCalendar
                    {
                        CalendarId = sdr["CalendarId"].ToString(),
                        LineOrderCode = sdr["LineOrderCode"].ToString(),
                        Line = sdr["Line"].ToString(),
                        CalendarDate = sdr["CalendarDate"] != DBNull.Value ? Convert.ToDateTime(sdr["CalendarDate"]).ToString("yyyy-MM-dd") : "",
                        Shift = sdr["Shift"].ToString(),
                        CalendarStatus = sdr["CalendarStatus"].ToString(),
                        CalendarStat = sdr["CalendarStat"].ToString(),
                        WorkingTime = sdr["WorkingTime"].ToString(),
                        OEE = sdr["OEE"].ToString(),
                        CT = sdr["CT"].ToString(),
                        EarlyOvertime = sdr["EarlyOvertime"].ToString(),
                        EndOvertime = sdr["EndOvertime"].ToString(),
                        MandatoryPdt = sdr["MandatoryPdt"].ToString(),
                        OtherPdt = sdr["OtherPdt"].ToString(),
                        TimePdt = sdr["TimePdt"].ToString(),
                        CreatedBy = sdr["CreatedBy"].ToString(),
                        CreatedDate = sdr["CreatedDate"] != DBNull.Value ? Convert.ToDateTime(sdr["CreatedDate"]).ToString("yyyy-MM-dd HH:mm") : "",
                        UpdatedBy = sdr["UpdatedBy"].ToString(),
                        UpdatedDate = sdr["UpdatedDate"] != DBNull.Value ? Convert.ToDateTime(sdr["UpdatedDate"]).ToString("yyyy-MM-dd HH:mm") : "",
						Remarks = sdr["Remarks"].ToString()
                    });
				}
				conn.Close();
			}
			return calendar;
		}

        [HttpPost]
        public async Task<List<RemarksNote>> Upload([FromForm] IFormFile file, [FromForm] string Periode, [FromQuery] string? UID)
        {
            var list = new List<RemarksNote>();
            string remarks = "";

            try
            {
                if (file == null || file.Length == 0)
                {
                    list.Add(new RemarksNote { Remarks = "File Excel kosong atau tidak ditemukan." });
                    return list;
                }

                DataTable dt = new DataTable();
                dt.Columns.Add("LineOrderCode", typeof(string));
                dt.Columns.Add("CalendarDate", typeof(DateTime));
                dt.Columns.Add("Shift", typeof(string));
                dt.Columns.Add("CalendarStatus", typeof(string));
                dt.Columns.Add("WorkingTime", typeof(int));
                dt.Columns.Add("OEE", typeof(decimal));
                dt.Columns.Add("CT", typeof(decimal));
                dt.Columns.Add("EarlyOvertime", typeof(int));
                dt.Columns.Add("EndOvertime", typeof(int));
                dt.Columns.Add("MandatoryPdt", typeof(int));
                dt.Columns.Add("OtherPdt", typeof(int));
                dt.Columns.Add("TimePdt", typeof(int));
                dt.Columns.Add("Remarks", typeof(string));

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheets.First();
                        var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

                        int rowNumber = 1;
                        
                        foreach (var row in rows)
                        {
                            rowNumber++;
                            
                            if (string.IsNullOrWhiteSpace(row.Cell(1).GetString()))
                                return new List<RemarksNote> { new RemarksNote { Remarks = $"Upload Gagal: Line Order Code di baris {rowNumber} tidak boleh kosong." } };

                            if (row.Cell(2).IsEmpty() || !DateTime.TryParse(row.Cell(2).GetString(), out DateTime calDate))
                                return new List<RemarksNote> { new RemarksNote { Remarks = $"Upload Gagal: Calendar Date di baris {rowNumber} kosong atau format salah." } };

                            if (string.IsNullOrWhiteSpace(row.Cell(3).GetString()))
                                return new List<RemarksNote> { new RemarksNote { Remarks = $"Upload Gagal: Shift di baris {rowNumber} tidak boleh kosong." } };

                            if (row.Cell(4).IsEmpty())
                                return new List<RemarksNote> { new RemarksNote { Remarks = $"Upload Gagal: Calendar Status di baris {rowNumber} tidak boleh kosong." } };

                            if (row.Cell(5).IsEmpty())
                                return new List<RemarksNote> { new RemarksNote { Remarks = $"Upload Gagal: Working Time di baris {rowNumber} tidak boleh kosong." } };

                            if (row.Cell(6).IsEmpty())
                                return new List<RemarksNote> { new RemarksNote { Remarks = $"Upload Gagal: OEE di baris {rowNumber} tidak boleh kosong." } };

                            if (row.Cell(7).IsEmpty())
                                return new List<RemarksNote> { new RemarksNote { Remarks = $"Upload Gagal: CT di baris {rowNumber} tidak boleh kosong." } };

                            DataRow dr = dt.NewRow();
                            dr["LineOrderCode"] = row.Cell(1).GetString();
                            dr["CalendarDate"] = calDate;
                            dr["Shift"] = row.Cell(3).GetString();
                            dr["CalendarStatus"] = row.Cell(4).GetValue<int>();
                            dr["WorkingTime"] = row.Cell(5).GetValue<int>();
                            dr["OEE"] = row.Cell(6).IsEmpty() ? 0m : row.Cell(6).GetValue<decimal>();
                            dr["CT"] = row.Cell(7).IsEmpty() ? 0m : row.Cell(7).GetValue<decimal>();

                            dr["EarlyOvertime"] = row.Cell(8).IsEmpty() ? DBNull.Value : row.Cell(8).GetValue<int>();
                            dr["EndOvertime"] = row.Cell(9).IsEmpty() ? DBNull.Value : row.Cell(9).GetValue<int>();
                            dr["MandatoryPdt"] = row.Cell(10).IsEmpty() ? DBNull.Value : row.Cell(10).GetValue<int>();
                            dr["OtherPdt"] = row.Cell(11).IsEmpty() ? DBNull.Value : row.Cell(11).GetValue<int>();
                            dr["TimePdt"] = row.Cell(12).IsEmpty() ? DBNull.Value : row.Cell(12).GetValue<int>();
                            
                            dr["Remarks"] = row.Cell(13).GetString();

                            dt.Rows.Add(dr);
                        }
                    }
                }

                if (dt.Rows.Count == 0)
                {
                    list.Add(new RemarksNote { Remarks = "Tidak ada data valid di dalam Excel." });
                    return list;
                }

                using (SqlConnection conn = new SqlConnection(_configuration))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_M_Add_Calendar_Upload", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        
                        SqlParameter tvpParam = cmd.Parameters.AddWithValue("@AddCalendarData", dt);
                        tvpParam.SqlDbType = SqlDbType.Structured;
                        tvpParam.TypeName = "dbo.AddCalendarType";
                        
                        cmd.Parameters.AddWithValue("@EntryUser", UID ?? "System");
                        cmd.Parameters.AddWithValue("@Periode", Periode ?? "");

                        await conn.OpenAsync();
                        object result = await cmd.ExecuteScalarAsync();
                        remarks = result != null ? result.ToString() : "Error executing upload";
                    }
                }
            }
            catch (Exception ex)
            {
                remarks = "Exception: " + ex.Message;
            }

            list.Add(new RemarksNote { Remarks = remarks });
            return list;
        }
        
        [HttpGet] 
        public IActionResult DownloadTemplate(string periode)
        {
            try
            {
                if (string.IsNullOrEmpty(periode))
                    return BadRequest("Invalid Period.");

                string[] parts = periode.Split('-');
                int year = int.Parse(parts[0]);
                int month = int.Parse(parts[1]);
                int daysInMonth = DateTime.DaysInMonth(year, month);

                List<string> lineOrders = new List<string>();
                Dictionary<string, int> existingCalendarStatus = new Dictionary<string, int>();
                
                using (SqlConnection conn = new SqlConnection(_configuration))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT LineOrderCode FROM M_Line_Order WHERE LineOrderStatus = 1", conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lineOrders.Add(reader["LineOrderCode"].ToString());
                            }
                        }
                    }
                    
                    string queryCal = @"SELECT LineOrderCode, CalendarDate, Shift, CalendarStatus 
                                        FROM M_Calendar 
                                        WHERE YEAR(CalendarDate) = @Year AND MONTH(CalendarDate) = @Month";
                    
                    using (SqlCommand cmdCal = new SqlCommand(queryCal, conn))
                    {
                        cmdCal.Parameters.AddWithValue("@Year", year);
                        cmdCal.Parameters.AddWithValue("@Month", month);
                        
                        using (SqlDataReader readerCal = cmdCal.ExecuteReader())
                        {
                            while (readerCal.Read())
                            {
                                string loc = readerCal["LineOrderCode"].ToString();
                                DateTime calDate = Convert.ToDateTime(readerCal["CalendarDate"]);
                                string shift = readerCal["Shift"].ToString();
                                int calStatus = readerCal["CalendarStatus"] != DBNull.Value ? Convert.ToInt32(readerCal["CalendarStatus"]) : 0;

                                string key = $"{loc}|{calDate.ToString("yyyy-MM-dd")}|{shift}";
                                existingCalendarStatus[key] = calStatus;
                            }
                        }
                    }
                }
                
                if (lineOrders.Count == 0)
                    return BadRequest("Data Line Order kosong di database. Tidak dapat membuat template.");
                
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Template");
                    
                    string[] headers = { 
                        "Line Order Code", "Calendar Date (yyyy-mm-dd)", "Shift", "Calendar Status", 
                        "Working Time", "OEE", "CT", "Early Overtime", "End Overtime", "Mandatory PDT", "Other PDT", 
                        "PM/AM PDT", "Remarks" 
                    };
                    
                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = headers[i];
                    }
                    
                    var headerRow = worksheet.Range(1, 1, 1, headers.Length);
                    headerRow.Style.Font.Bold = true;
                    headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    int currentRow = 2;
                    string[] shifts = { "Day", "Night" };
                    
                    foreach (var line in lineOrders)
                    {
                        for (int day = 1; day <= daysInMonth; day++)
                        {
                            DateTime currentDate = new DateTime(year, month, day);
                            string dateString = currentDate.ToString("yyyy-MM-dd");
                            
                            int defaultStatus = (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday) ? 0 : 1;

                            foreach (var shift in shifts)
                            {
                                string lookupKey = $"{line}|{dateString}|{shift}";
                                int finalStatus;

                                if (existingCalendarStatus.ContainsKey(lookupKey))
                                    finalStatus = existingCalendarStatus[lookupKey];
                                else
                                    finalStatus = defaultStatus;
                                
                                worksheet.Cell(currentRow, 1).Value = line;
                                worksheet.Cell(currentRow, 2).Value = dateString;
                                worksheet.Cell(currentRow, 3).Value = shift;
                                worksheet.Cell(currentRow, 4).Value = finalStatus;
                                
                                currentRow++;
                            }
                        }
                    }
                    
                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Template_Master_Calendar_{periode}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Error generating template: " + ex.Message);
            }
        }
    }
}
