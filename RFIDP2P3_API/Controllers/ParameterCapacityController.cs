using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using ClosedXML.Excel;
using ExcelDataReader;
using RFIDP2P3_API.Helpers;
using RFIDP2P3_API.Models;
using RFIDP2P3_API.Models.Request;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ParameterCapacityController : ControllerBase
    {
        private readonly string _configuration;

        public ParameterCapacityController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<Dictionary<string, object>>> INQ([FromBody] ParamCapacityRequest request)
        {
            var dt = new DataTable();
            string periode = request?.UploadDate ?? "";

            using (SqlConnection conn = new SqlConnection(_configuration))
            {
                conn.Open();
                string sql = @"
                    SELECT 
                        Id,
                        Periode,
                        LineCode,
                        MonthOffsetLabel AS Month,
                        Advance,
                        Mandatory,
                        OvertimeHOT,
                        CreatedUser AS [User],
                        CreatedAt AS UploadDate
                    FROM T_Parameter_Capacity
                    WHERE REPLACE(Periode, '-', '') = REPLACE(@Periode, '-', '')
                    ORDER BY LineCode, 
                             CASE WHEN MonthOffsetLabel = 'N' THEN 0 ELSE CAST(REPLACE(MonthOffsetLabel, 'N+', '') AS INT) END";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
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
                    dict[col.ColumnName] = row[col];
                }
                result.Add(dict);
            }

            return result;
        }

        [HttpPost]
        public ActionResult<string> Upload([FromForm] IFormFile file, [FromQuery] string? UID)
        {
            var validation = FileHelper.ValidateFile(
                file, 
                maxSizeInMb: 5, 
                allowedExtensions: new[] { ".xls", ".xlsx" }
            );

            if (!validation.IsValid)
                return BadRequest(validation.ErrorMessage);

            var validOptionsByShift = new Dictionary<string, HashSet<int>>(StringComparer.OrdinalIgnoreCase);
            var validAssyOptions = new HashSet<int>();
            var validMachiningOptions = new HashSet<int>();

            using (SqlConnection conn = new SqlConnection(_configuration))
            {
                conn.Open();
                string sqlAllowed = "SELECT LineType, Shift, MandatoryValue FROM M_Parameter_Mandatory_Option";
                using (SqlCommand cmd = new SqlCommand(sqlAllowed, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string lType = reader["LineType"].ToString()?.Trim() ?? "";
                        int sft = Convert.ToInt32(reader["Shift"]);
                        int val = Convert.ToInt32(reader["MandatoryValue"]);

                        string key = $"{lType}_{sft}";
                        if (!validOptionsByShift.ContainsKey(key))
                            validOptionsByShift[key] = new HashSet<int>();

                        validOptionsByShift[key].Add(val);

                        if (lType.Equals("Assy", StringComparison.OrdinalIgnoreCase))
                            validAssyOptions.Add(val);
                        else
                            validMachiningOptions.Add(val);
                    }
                }
            }

            List<UploadCapacity> uploadData = new List<UploadCapacity>();
            List<string> errorLogs = new List<string>();
            string extractedPeriode = "";
            int rowCount = 2;
            
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

                        string[] expectedHeaders = { "Periode", "Line Code", "Month", "Shift", "Advance", "Mandatory", "Overtime HOT" };

                        foreach (string header in expectedHeaders)
                        {
                            if (!dataTable.Columns.Contains(header))
                                return BadRequest($"Invalid Excel format. Header column '{header}' is missing.");
                        }
                        
                        foreach (DataRow row in dataTable.Rows)
                        {
                            string periodeStr = row[0]?.ToString()?.Trim();
                            string lineCode = row[1]?.ToString()?.Trim();
                            string bulanProduksi = row[2]?.ToString()?.Trim();
                            string shiftStr = row[3]?.ToString()?.Trim();
                            string advanceStr = row[4]?.ToString()?.Trim();
                            string mandatoryStr = row[5]?.ToString()?.Trim();
                            string overtimeStr = row[6]?.ToString()?.Trim();

                            if (string.IsNullOrEmpty(periodeStr) && string.IsNullOrEmpty(lineCode))
                                continue;

                            if (string.IsNullOrEmpty(extractedPeriode))
                            {
                                extractedPeriode = periodeStr.Replace("-", "");
                                if (DateTime.TryParseExact(extractedPeriode, "yyyyMM",
                                        System.Globalization.CultureInfo.InvariantCulture,
                                        System.Globalization.DateTimeStyles.None, out DateTime parsedPeriode))
                                {
                                    DateTime currentMonthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                                    // if (parsedPeriode <= currentMonthStart)
                                    // {
                                    //     return BadRequest(
                                    //         $"<div style='text-align: left; padding: 10px; background: #fdf2f2; border: 1px solid #f2dede; border-radius: 5px; color: #a94442;'>" +
                                    //         $"<b>Upload Rejected:</b> You can only upload parameters for the next month or future periods. Data for {parsedPeriode.ToString("MMMM yyyy")} or earlier has already been calculated and is locked.</div>");
                                    // }
                                }
                            }

                            bool isAssy = !string.IsNullOrEmpty(lineCode) &&
                                          lineCode.StartsWith("K", StringComparison.OrdinalIgnoreCase);
                            string lineTypeKey = isAssy ? "Assy" : "Machining";

                            int shiftVal = 0;
                            int.TryParse(shiftStr, out shiftVal);

                            int mandatoryVal = 0;
                            if (string.IsNullOrEmpty(mandatoryStr))
                                errorLogs.Add($"Row {rowCount}: Mandatory is missing for Line {lineCode}.");
                            else if (!int.TryParse(mandatoryStr, out mandatoryVal))
                                errorLogs.Add($"Row {rowCount}: Mandatory must be a number.");
                            else
                            {
                                string shiftOptionKey = $"{lineTypeKey}_{shiftVal}";

                                if (validOptionsByShift.TryGetValue(shiftOptionKey, out var allowedValues))
                                {
                                    if (!allowedValues.Contains(mandatoryVal))
                                        errorLogs.Add($"Row {rowCount}: Mandatory value {mandatoryVal} is invalid for {lineCode} ({lineTypeKey} Shift {shiftVal}).");
                                }
                                else
                                {
                                    var fallbackAllowed = isAssy ? validAssyOptions : validMachiningOptions;
                                    if (!fallbackAllowed.Contains(mandatoryVal))
                                        errorLogs.Add($"Row {rowCount}: Mandatory value {mandatoryVal} is invalid for {lineCode} ({lineTypeKey}).");
                                }
                            }

                            int advanceVal = 0;
                            if (!string.IsNullOrEmpty(advanceStr) && !int.TryParse(advanceStr, out advanceVal))
                                errorLogs.Add($"Row {rowCount}: Advance must be a number.");

                            int overtimeVal = 0;
                            if (!string.IsNullOrEmpty(overtimeStr) && !int.TryParse(overtimeStr, out overtimeVal))
                                errorLogs.Add($"Row {rowCount}: Overtime HOT must be a number.");

                            int offsetN = 0;
                            if (!string.IsNullOrEmpty(bulanProduksi))
                            {
                                int startIdx = bulanProduksi.IndexOf('(');
                                int endIdx = bulanProduksi.IndexOf(')');
                                if (startIdx >= 0 && endIdx > startIdx)
                                {
                                    string nVal = bulanProduksi.Substring(startIdx + 1, endIdx - startIdx - 1).ToUpper();

                                    if (nVal == "N") offsetN = 0;
                                    else if (nVal.StartsWith("N+"))
                                    {
                                        int.TryParse(nVal.Replace("N+", ""), out offsetN);
                                    }
                                }
                            }

                            if (errorLogs.Count == 0)
                            {
                                uploadData.Add(new UploadCapacity
                                {
                                    Line = lineCode,
                                    OffsetN = offsetN,
                                    Advance = advanceVal,
                                    Mandatory = mandatoryVal,
                                    OvertimeHot = overtimeVal
                                });
                            }
                    
                            rowCount++;
                        }
                    }
                }
                
                if (errorLogs.Count > 0)
                {
                    var topErrors = errorLogs.Take(10).Select(e => $"<li style='margin-bottom: 5px;'>{e}</li>");
                    string combinedErrors = "<div style='text-align: left; max-height: 200px; overflow-y: auto; padding: 10px; background: #fdf2f2; border: 1px solid #f2dede; border-radius: 5px;'>" +
                                            "<ul style='padding-left: 20px; color: #a94442; font-size: 13px; margin: 0;'>" +
                                            string.Join("", topErrors) +
                                            "</ul>";

                    if (errorLogs.Count > 10)
                        combinedErrors += $"<p style='margin-top: 10px; font-size: 12px; color: #777;'><i>...and {errorLogs.Count - 10} other error(s).</i></p>";
                    combinedErrors += "</div>";
                    return BadRequest(combinedErrors);
                }

                if (uploadData.Count == 0)
                    return BadRequest("No valid data available to process.");
                
                string inputJson = System.Text.Json.JsonSerializer.Serialize(uploadData);
                string currentUser = string.IsNullOrEmpty(UID) ? "SystemUpload" : UID;
                string spRemarks = "";

                using (SqlConnection conn = new SqlConnection(_configuration))
                {
                    conn.Open();
                    using (SqlCommand cmdProcess = new SqlCommand("sp_Upload_Calc_Capacity", conn))
                    {
                        cmdProcess.CommandType = CommandType.StoredProcedure;
                        cmdProcess.Parameters.AddWithValue("@Periode_ID", extractedPeriode);
                        cmdProcess.Parameters.AddWithValue("@InputJson", inputJson);
                        cmdProcess.Parameters.AddWithValue("@User_Login", currentUser);

                        object result = cmdProcess.ExecuteScalar();
                        spRemarks = result?.ToString() ?? "";
                    }
                }

                if (spRemarks.ToLower() != "success")
                    return BadRequest($"<div style='text-align: left; padding: 10px; background: #fdf2f2; border: 1px solid #f2dede; border-radius: 5px; color: #a94442;'>Upload Rejected by System: {spRemarks}</div>");

                return Ok("success");
            }
            catch (Exception e)
            {
                return BadRequest("System error occurred");
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
                if (string.IsNullOrEmpty(periode))
                    return BadRequest("Invalid Period.");

                string cleanPeriode = periode.Replace("-", "");
                if (!DateTime.TryParseExact(cleanPeriode, "yyyyMM", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime baseDate))
                    return BadRequest("Invalid period format. Expected YYYY-MM or YYYYMM.");

                var dt = new DataTable();

                using (SqlConnection conn = new SqlConnection(_configuration))
                {
                    conn.Open();
                    string sql = @"
                        WITH FirmLines AS (
                            SELECT 
                                M.LineOrderCode,
                                F.MonthOffsetLabel,
                                CASE 
                                    WHEN F.MonthOffsetLabel = 'N' THEN 0 
                                    ELSE CAST(REPLACE(F.MonthOffsetLabel, 'N+', '') AS INT) 
                                END AS OffsetUrutan
                            FROM T_Calc_Order_Firm F
                            JOIN M_Suffix_to_Unique M ON F.Suffix = M.SuffixCode
                            WHERE REPLACE(F.Periode, '-', '') = REPLACE(@Periode, '-', '') 
                            GROUP BY M.LineOrderCode, F.MonthOffsetLabel
                        )
                        SELECT 
                            FL.LineOrderCode,
                            FL.MonthOffsetLabel,
                            FL.OffsetUrutan,
                            DATEADD(MONTH, FL.OffsetUrutan + 1, CAST(REPLACE(@Periode, '-', '') + '01' AS DATE)) AS TargetDate,
                            Cal.DominantShift,
                            ISNULL(Cal.TotalDays, 0) AS TotalCalendarDays
                        FROM FirmLines FL
                        OUTER APPLY (
                            SELECT 
                                CASE 
                                    WHEN SUM(CASE WHEN S.ShiftCount >= 2 THEN 1 ELSE 0 END) >= 
                                         SUM(CASE WHEN S.ShiftCount = 1 THEN 1 ELSE 0 END) 
                                    THEN 2 
                                    ELSE 1 
                                END AS DominantShift,
                                COUNT(*) AS TotalDays
                            FROM (
                                SELECT CalendarDate, COUNT(DISTINCT Shift) AS ShiftCount
                                FROM M_Add_Calendar
                                WHERE LineOrderCode = FL.LineOrderCode
                                  AND CalendarDate >= DATEADD(MONTH, FL.OffsetUrutan + 1, CAST(REPLACE(@Periode, '-', '') + '01' AS DATE))
                                  AND CalendarDate <  DATEADD(MONTH, FL.OffsetUrutan + 2, CAST(REPLACE(@Periode, '-', '') + '01' AS DATE))
                                GROUP BY CalendarDate
                            ) S
                        ) Cal
                        ORDER BY FL.LineOrderCode, FL.OffsetUrutan";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Periode", periode);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }

                if (dt.Rows.Count == 0)
                    return BadRequest("No firm order data found for this period. Cannot generate template.");

                var cultureId = new System.Globalization.CultureInfo("id-ID");

                var missingCalendars = new List<string>();
                foreach (DataRow dr in dt.Rows)
                {
                    int totalDays = Convert.ToInt32(dr["TotalCalendarDays"]);
                    if (totalDays == 0)
                    {
                        string line = dr["LineOrderCode"].ToString();
                        string mLabel = dr["MonthOffsetLabel"].ToString();
                        DateTime tDate = Convert.ToDateTime(dr["TargetDate"]);
                        string monthName = tDate.ToString("MMMM yyyy", cultureId);

                        missingCalendars.Add($"Line <b>{line}</b> for period <b>{monthName} ({mLabel})</b>");
                    }
                }

                if (missingCalendars.Count > 0)
                {
                    var distinctErrors = missingCalendars.Distinct().ToList();
                    string errorList = string.Join("", distinctErrors.Select(err => $"<li style='margin-bottom: 4px;'>{err}</li>"));

                    return BadRequest(
                        $"<div style='text-align: left; padding: 10px; background: #fdf2f2; border: 1px solid #f2dede; border-radius: 5px; color: #a94442;'>" +
                        $"<b>Missing Calendar Data:</b> Calendar data is missing for the following line(s) and period(s):" +
                        $"<ul style='padding-left: 20px; margin-top: 8px; margin-bottom: 8px; font-size: 13px;'>" +
                        $"{errorList}" +
                        $"</ul>" +
                        $"Please generate or upload the calendar in the <b>Additional Master Calendar</b> menu first before downloading this template.</div>"
                    );
                }

                var mandatoryOptionsMap = new Dictionary<string, MandatoryOptionConfig>(StringComparer.OrdinalIgnoreCase);
                using (SqlConnection conn = new SqlConnection(_configuration))
                {
                    conn.Open();
                    string sqlOptions = @"
                        SELECT LineType, Shift, MandatoryValue, Description, IsDefault 
                        FROM M_Parameter_Mandatory_Option
                        ORDER BY LineType, Shift, SortOrder";

                    using (SqlCommand cmd = new SqlCommand(sqlOptions, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string lType = reader["LineType"].ToString()?.Trim() ?? "";
                            int sft = Convert.ToInt32(reader["Shift"]);
                            int val = Convert.ToInt32(reader["MandatoryValue"]);
                            string desc = reader["Description"]?.ToString()?.Trim() ?? "";
                            bool isDef = Convert.ToBoolean(reader["IsDefault"]);

                            string key = $"{lType}_{sft}";
                            if (!mandatoryOptionsMap.ContainsKey(key))
                            {
                                mandatoryOptionsMap[key] = new MandatoryOptionConfig
                                {
                                    LineType = lType,
                                    Shift = sft
                                };
                            }

                            mandatoryOptionsMap[key].Items.Add(new MandatoryOptionItem
                            {
                                Value = val,
                                Description = desc,
                                IsDefault = isDef
                            });
                        }
                    }
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Template_Parameter");

                    string[] headers = {
                        "Periode", "Line Code", "Month", "Shift", "Advance", "Mandatory", "Overtime HOT"
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
                    string formattedPeriode = periode.Contains("-") ? periode : periode.Insert(4, "-");

                    foreach (DataRow dr in dt.Rows)
                    {
                        string lineCode = dr["LineOrderCode"].ToString()?.Trim() ?? "";
                        string bulanKeLabel = dr["MonthOffsetLabel"].ToString();
                        int offsetUrutan = Convert.ToInt32(dr["OffsetUrutan"]);

                        DateTime targetMonth = baseDate.AddMonths(offsetUrutan + 1);
                        int shift = Convert.ToInt32(dr["DominantShift"]);

                        string displayBulan = $"{targetMonth.ToString("MMMM yyyy", cultureId)} ({bulanKeLabel})";

                        worksheet.Cell(currentRow, 1).Value = formattedPeriode;
                        worksheet.Cell(currentRow, 2).Value = lineCode;
                        worksheet.Cell(currentRow, 3).Value = displayBulan;
                        worksheet.Cell(currentRow, 4).Value = $"{shift}";

                        worksheet.Cell(currentRow, 5).Value = 0;
                        worksheet.Cell(currentRow, 7).Value = 0;

                        bool isAssy = lineCode.StartsWith("K", StringComparison.OrdinalIgnoreCase);

                        string lineTypeKey = isAssy ? "Assy" : "Machining";
                        string optionKey = $"{lineTypeKey}_{shift}";

                        var mandatoryCell = worksheet.Cell(currentRow, 6);

                        if (mandatoryOptionsMap.TryGetValue(optionKey, out var config) && config.Items.Count > 0)
                        {
                            int defaultValue = config.Items.FirstOrDefault(x => x.IsDefault)?.Value ?? config.Items.First().Value;
                            mandatoryCell.Value = defaultValue;

                            string allowedValues = string.Join(",", config.Items.Select(x => x.Value));
                            string validationList = $"\"{allowedValues}\"";

                            string inputTooltip = string.Join("\n", config.Items.Select(x => $"{x.Value} : {x.Description}"));

                            var validation = mandatoryCell.GetDataValidation();
                            validation.List(validationList, false);

                            validation.ShowInputMessage = true;
                            validation.InputTitle = "Mandatory Option Guide";
                            validation.InputMessage = inputTooltip;

                            validation.ShowErrorMessage = true;
                            validation.ErrorStyle = XLErrorStyle.Stop;
                            validation.ErrorTitle = "Invalid Value";
                            validation.ErrorMessage = $"Mandatory value for {lineTypeKey} ({shift} Shift) must be one of: {allowedValues}.";
                        }
                        else
                        {
                            mandatoryCell.Value = 0;
                        }

                        var inputCells = worksheet.Range(currentRow, 5, currentRow, 7);
                        inputCells.Style.Fill.BackgroundColor = XLColor.LightYellow;
                        inputCells.Style.Protection.SetLocked(false);

                        currentRow++;
                    }

                    worksheet.Protect("Admin-ICS");
                    worksheet.Columns().AdjustToContents();
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Template_ParameterCapacity_{cleanPeriode}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generating template: {ex.Message}");
            }
        }
    }
}
