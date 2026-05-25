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
                        string[] expectedHeaders = { "Periode", "Line Code", "Month", "Advance", "Mandatory", "Overtime HOT" };
                
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
                            string advanceStr = row[3]?.ToString()?.Trim();
                            string mandatoryStr = row[4]?.ToString()?.Trim();
                            string overtimeStr = row[5]?.ToString()?.Trim();

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

                            int mandatoryVal = 0;
                            if (string.IsNullOrEmpty(mandatoryStr))
                            {
                                errorLogs.Add($"Baris {rowCount}: Mandatory belum diisi untuk Line {lineCode}.");
                            }
                            else if (!int.TryParse(mandatoryStr, out mandatoryVal))
                            {
                                errorLogs.Add($"Baris {rowCount}: Mandatory harus berupa angka.");
                            }
                            else
                            {
                                bool isKLine = lineCode != null && lineCode.StartsWith("K", StringComparison.OrdinalIgnoreCase);
                        
                                if (isKLine && mandatoryVal != 34 && mandatoryVal != 82)
                                    errorLogs.Add($"Baris {rowCount}: Nilai Mandatory untuk {lineCode} (K-Line) harus 34 atau 82.");
                                else if (!isKLine && mandatoryVal != 100 && mandatoryVal != 52)
                                    errorLogs.Add($"Baris {rowCount}: Nilai Mandatory untuk {lineCode} (Machining) harus 100 atau 52.");
                            }
                            
                            int advanceVal = 0;
                            if (!string.IsNullOrEmpty(advanceStr) && !int.TryParse(advanceStr, out advanceVal))
                                errorLogs.Add($"Baris {rowCount}: Advance harus berupa angka.");

                            int overtimeVal = 0;
                            if (!string.IsNullOrEmpty(overtimeStr) && !int.TryParse(overtimeStr, out overtimeVal))
                                errorLogs.Add($"Baris {rowCount}: Overtime HOT harus berupa angka.");

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
                        combinedErrors += $"<p style='margin-top: 10px; font-size: 12px; color: #777;'><i>...dan {errorLogs.Count - 10} error lainnya.</i></p>";

                    combinedErrors += "</div>";
                    return BadRequest(combinedErrors);
                }

                if (uploadData.Count == 0)
                    return BadRequest("Tidak ada data valid yang bisa diproses.");
                
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
                    return BadRequest($"<div style='text-align: left; padding: 10px; background: #fdf2f2; border: 1px solid #f2dede; border-radius: 5px; color: #a94442;'>Upload Ditolak Sistem: {spRemarks}</div>");

                return Ok("success");
            }
            catch (Exception e)
            {
                return BadRequest("Terjadi kesalahan sistem: " + e.Message);
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

                var dt = new DataTable();

                using (SqlConnection conn = new SqlConnection(_configuration))
                {
                    conn.Open();
                    string sql = @"
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
                        ORDER BY M.LineOrderCode, OffsetUrutan";

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
                    return BadRequest("Data tidak ditemukan untuk periode ini. Tidak dapat men-generate template.");

                string cleanPeriode = periode.Replace("-", "");
                if (!DateTime.TryParseExact(cleanPeriode, "yyyyMM", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime baseDate))
                {
                    return BadRequest("Format periode tidak valid.");
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Template_Parameter");
                    
                    string[] headers = { 
                        "Periode", "Line Code", "Month", "Advance", "Mandatory", "Overtime HOT" 
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

                    var cultureId = new System.Globalization.CultureInfo("id-ID");

                    foreach (DataRow dr in dt.Rows)
                    {
                        string lineCode = dr["LineOrderCode"].ToString();
                        string bulanKeLabel = dr["MonthOffsetLabel"].ToString();
                        int offsetUrutan = Convert.ToInt32(dr["OffsetUrutan"]);

                        DateTime targetMonth = baseDate.AddMonths(offsetUrutan + 1);
                        
                        string displayBulan = $"{targetMonth.ToString("MMMM yyyy", cultureId)} ({bulanKeLabel})";

                        worksheet.Cell(currentRow, 1).Value = formattedPeriode;
                        worksheet.Cell(currentRow, 2).Value = lineCode;
                        worksheet.Cell(currentRow, 3).Value = displayBulan; 
                        
                        worksheet.Cell(currentRow, 4).Value = 0;
                        worksheet.Cell(currentRow, 5).Value = 0;
                        worksheet.Cell(currentRow, 6).Value = 0;
                        
                        worksheet.Range(currentRow, 4, currentRow, 6).Style.Fill.BackgroundColor = XLColor.LightYellow;
                        
                        currentRow++;
                    }

                    worksheet.Protect("Admin123"); 
                    worksheet.Column(1).Style.Protection.SetLocked(true);
                    worksheet.Column(2).Style.Protection.SetLocked(true);
                    worksheet.Column(3).Style.Protection.SetLocked(true);
                    
                    worksheet.Column(4).Style.Protection.SetLocked(false);
                    worksheet.Column(5).Style.Protection.SetLocked(false);
                    worksheet.Column(6).Style.Protection.SetLocked(false);
                    
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
                return BadRequest("Error generating template: " + ex.Message);
            }
        }
    }
}
