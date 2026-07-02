using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using ClosedXML.Excel;
using ExcelDataReader;
using RFIDP2P3_API.Helpers;
using RFIDP2P3_API.Models;
using RFIDP2P3_API.Models.Request;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ParameterRecoverySctController : ControllerBase
    {
        private readonly string _configuration;

        public ParameterRecoverySctController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<Dictionary<string, object>>> INQ([FromBody] ParamAdjustmentRequest request)
        {
            var dt = new DataTable();
            string periode = request?.Periode ?? "";

            using (SqlConnection conn = new SqlConnection(_configuration))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("sp_Inq_M_Recovery_Sct", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Periode", periode);
                    
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            var result = new List<Dictionary<string, object>>();
            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    if (col.DataType == typeof(DateTime) && row[col] != DBNull.Value)
                        dict[col.ColumnName] = ((DateTime)row[col]).ToString("yyyy-MM-dd");
                    else
                        dict[col.ColumnName] = row[col];
                }
                result.Add(dict);
            }

            return result;
        }

        [HttpPost]
        public ActionResult<string> Upload([FromForm] IFormFile file, [FromQuery] string periode, [FromQuery] string? UID)
        {
            var validation = FileHelper.ValidateFile(
                file, 
                maxSizeInMb: 5, 
                allowedExtensions: new[] { ".xls", ".xlsx" }
            );

            if (!validation.IsValid)
                return BadRequest(validation.ErrorMessage);
            
            if (string.IsNullOrEmpty(periode) || !DateTime.TryParseExact(periode.Replace("-", ""), "yyyyMM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime baseDate))
                return BadRequest("Invalid period parameter. Expected format: YYYY-MM.");

            int year = baseDate.Year;
            int month = baseDate.Month;
            int daysInMonth = DateTime.DaysInMonth(year, month);

            var uploadDict = new Dictionary<(string LineCode, DateTime TargetDate), UploadAdjustment>();
            List<string> errorLogs = new List<string>();
            
            try
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    stream.Position = 0;

                    using (var reader = ExcelDataReader.ExcelReaderFactory.CreateReader(stream))
                    {
                        var dataSet = reader.AsDataSet(new ExcelDataReader.ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataReader.ExcelDataTableConfiguration() { UseHeaderRow = true }
                        });
                        
                        var dataTable = dataSet.Tables[0];
                        
                        if (dataTable.Columns.Count < 3 || 
                            !dataTable.Columns[0].ColumnName.Equals("Line Code", StringComparison.OrdinalIgnoreCase) || 
                            !dataTable.Columns[1].ColumnName.Equals("Parameter", StringComparison.OrdinalIgnoreCase))
                        {
                            return BadRequest("Invalid Excel format. Expected 'Line Code' and 'Parameter' as the first two columns.");
                        }
                        
                        string firstDateHeader = dataTable.Columns[2].ColumnName; 
                        if (DateTime.TryParse(firstDateHeader, out DateTime parsedHeaderDate))
                        {
                            if (parsedHeaderDate.Year != year || parsedHeaderDate.Month != month)
                            {
                                return BadRequest($"<div style='text-align: left; padding: 10px; background: #fdf2f2; border: 1px solid #f2dede; border-radius: 5px; color: #a94442;'><b>Upload Rejected:</b> Period mismatch! The Excel file contains data for <b>{parsedHeaderDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture)}</b>, but you are attempting to upload for the <b>{baseDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture)}</b> period.</div>");
                            }
                        }

                        int rowCount = 2;
                        foreach (DataRow row in dataTable.Rows)
                        {
                            if (row[0] == DBNull.Value && row[1] == DBNull.Value) continue;

                            string lineCode = row[0]?.ToString()?.Trim() ?? "";
                            string parameter = row[1]?.ToString()?.Trim() ?? "";

                            if (string.IsNullOrEmpty(lineCode) || string.IsNullOrEmpty(parameter))
                            {
                                rowCount++;
                                continue;
                            }

                            for (int i = 2; i < dataTable.Columns.Count; i++)
                            {
                                string colName = dataTable.Columns[i].ColumnName;

                                if (DateTime.TryParse(colName, out DateTime headerDate))
                                {
                                    if (headerDate.Year != year || headerDate.Month != month)
                                        continue;

                                    string cellValue = row[i]?.ToString()?.Trim() ?? "";
                                    if (string.IsNullOrEmpty(cellValue)) continue;

                                    var key = (lineCode.ToUpper(), headerDate);

                                    if (!uploadDict.ContainsKey(key))
                                        uploadDict[key] = new UploadAdjustment { LineCode = key.Item1, TargetDate = headerDate };

                                    if (parameter.Equals("Special Cycle Time", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (decimal.TryParse(cellValue, out decimal ct)) 
                                            uploadDict[key].SpecialCycleTime = ct;
                                        else 
                                            errorLogs.Add($"Row {rowCount} (Date {headerDate:dd-MMM}): Special CT must be a number.");
                                    }
                                    else if (parameter.Equals("Recovery Day", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (decimal.TryParse(cellValue, out decimal recDay)) 
                                            uploadDict[key].RecoveryDay = recDay;
                                        else 
                                            errorLogs.Add($"Row {rowCount} (Date {headerDate:dd-MMM}): Recovery Day must be a number.");
                                    }
                                    else if (parameter.Equals("Recovery Night", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (decimal.TryParse(cellValue, out decimal recNight)) 
                                            uploadDict[key].RecoveryNight = recNight;
                                        else 
                                            errorLogs.Add($"Row {rowCount} (Date {headerDate:dd-MMM}): Recovery Night must be a number.");
                                    }
                                }
                            }
                            rowCount++;
                        }
                    }
                }
                
                if (errorLogs.Count > 0)
                {
                    var topErrors = errorLogs.Take(10).Select(e => $"<li style='margin-bottom: 5px;'>{e}</li>");
                    string combinedErrors = "<div style='text-align: left; max-height: 200px; overflow-y: auto; padding: 10px; background: #fdf2f2; border: 1px solid #f2dede; border-radius: 5px;'><ul style='padding-left: 20px; color: #a94442; font-size: 13px; margin: 0;'>" + string.Join("", topErrors) + "</ul></div>";
                    return BadRequest(combinedErrors);
                }
                
                if (uploadDict.Count == 0) return BadRequest("No data available to process. All inputs are empty.");
                
                var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value?? "SystemUpload";
                
                using (SqlConnection conn = new SqlConnection(_configuration))
                {
                    conn.Open();
                    foreach (var item in uploadDict.Values)
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_Upload_M_Recovery_Sct", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@LineOrderCode", item.LineCode);
                            cmd.Parameters.AddWithValue("@TargetDate", item.TargetDate);
                            cmd.Parameters.AddWithValue("@SpecialCycleTime", item.SpecialCycleTime.HasValue ? (object)item.SpecialCycleTime.Value : DBNull.Value);
                            cmd.Parameters.AddWithValue("@RecoveryDay", item.RecoveryDay);
                            cmd.Parameters.AddWithValue("@RecoveryNight", item.RecoveryNight);
                            cmd.Parameters.AddWithValue("@Remarks", "");
                            cmd.Parameters.AddWithValue("@UserLogin", userId);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                
                return Ok("success");
            }
            catch (Exception e)
            {
                return BadRequest($"System error occurred: {e.Message}");
                // return BadRequest("<div style='text-align: left; padding: 10px; background: #fdf2f2; border: 1px solid #f2dede; border-radius: 5px; color: #a94442; z-index: 9999;'>" +
                //                   "Terjadi kesalahan pada sistem saat memproses file. Silakan coba beberapa saat lagi atau hubungi administrator." +
                //                   "</div>");
            }
        }
        
        [HttpGet] 
        public IActionResult DownloadTemplate(string periode)
        {
            try
            {
                if (string.IsNullOrEmpty(periode)) return BadRequest("Invalid Period.");
                string cleanPeriode = periode.Replace("-", "");
        
                if (!DateTime.TryParseExact(cleanPeriode, "yyyyMM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime baseDate))
                    return BadRequest("Invalid period format. Use YYYY-MM.");
                
                int year = baseDate.Year;
                int month = baseDate.Month;
                int daysInMonth = DateTime.DaysInMonth(year, month);
                
                List<string> lineCodes = new List<string>();
                using (SqlConnection conn = new SqlConnection(_configuration))
                {
                    conn.Open();
                    string sqlLine = "SELECT LineOrderCode FROM M_Line_Order WHERE LineOrderStatus = 1 ORDER BY LineOrderCode";
                    using (SqlCommand cmd = new SqlCommand(sqlLine, conn))
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lineCodes.Add(dr["LineOrderCode"].ToString());
                        }
                    }
                }
                
                if (lineCodes.Count == 0)
                    return BadRequest("No active Line Master data found. The template cannot be downloaded.");
                
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Adjustment_Template");
            
                    worksheet.Cell(1, 1).Value = "Line Code";
                    worksheet.Cell(1, 2).Value = "Parameter";
                    
                    var headerRange = worksheet.Range(1, 1, 1, 2 + daysInMonth);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    
                    for (int d = 1; d <= daysInMonth; d++)
                    {
                        var cellDate = new DateTime(year, month, d);
                        var headerCell = worksheet.Cell(1, 2 + d);
                        headerCell.Value = cellDate;
                        headerCell.Style.DateFormat.Format = "dd-MMM";

                        bool isWeekend = (cellDate.DayOfWeek == DayOfWeek.Saturday || cellDate.DayOfWeek == DayOfWeek.Sunday);
                        if (isWeekend)
                        {
                            headerCell.Style.Fill.BackgroundColor = XLColor.LightCoral;
                            headerCell.Style.Font.FontColor = XLColor.White;
                        }
                    }
                    
                    string[] parameters = { "Special Cycle Time", "Recovery Day", "Recovery Night" };
                    int currentRow = 2;
                    
                    foreach (var line in lineCodes)
                    {
                        foreach (var param in parameters)
                        {
                            worksheet.Cell(currentRow, 1).Value = line;
                            worksheet.Cell(currentRow, 2).Value = param;
                            
                            for (int d = 1; d <= daysInMonth; d++)
                            {
                                var cellDate = new DateTime(year, month, d);
                                var currentCell = worksheet.Cell(currentRow, 2 + d);

                                bool isWeekend = (cellDate.DayOfWeek == DayOfWeek.Saturday || cellDate.DayOfWeek == DayOfWeek.Sunday);
                        
                                if (isWeekend) 
                                {
                                    currentCell.Value = 0;
                                    currentCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E0E0E0"); 
                                } 
                                else 
                                {
                                    currentCell.Value = ""; 
                                    currentCell.Style.Fill.BackgroundColor = XLColor.FromHtml("#FFFDE7");
                                }
                        
                                currentCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                currentCell.Style.Border.OutsideBorderColor = XLColor.LightGray;

                                if (param == "Recovery Night") {
                                    currentCell.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                                    currentCell.Style.Border.BottomBorderColor = XLColor.Charcoal;
                                }
                            }
                            
                            var leftCells = worksheet.Range(currentRow, 1, currentRow, 2);
                            leftCells.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            leftCells.Style.Border.OutsideBorderColor = XLColor.LightGray;
                            if (param == "Recovery Night") {
                                leftCells.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                                leftCells.Style.Border.BottomBorderColor = XLColor.Charcoal;
                            }
                             
                            currentRow++;
                        }
                    }

                    worksheet.Columns().AdjustToContents();
                    worksheet.Protect("Admin-ICS");
                    worksheet.Row(1).Style.Protection.SetLocked(true);
            
                    worksheet.Column(1).Style.Protection.SetLocked(true);
                    worksheet.Column(2).Style.Protection.SetLocked(true);
                    
                    worksheet.Range(2, 3, currentRow - 1, 2 + daysInMonth)
                        .Style.Protection.SetLocked(false);
                    
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Template_PlanAdjustment_{cleanPeriode}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Error generating template");
            }
        }
    }
}
