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
    public class TargetStockParamController : ControllerBase
    {
        private readonly string _configuration;

        public TargetStockParamController(IConfiguration configuration)
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
                using (SqlCommand cmd = new SqlCommand("sp_Inq_Target_Stock_Param", conn))
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
                    if (row[col] == DBNull.Value)
                        dict[col.ColumnName] = null;
                    else if (col.DataType == typeof(DateTime))
                        dict[col.ColumnName] = ((DateTime)row[col]).ToString("yyyy-MM-ddTHH:mm:ss");
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
                return BadRequest("Invalid period parameter.");

            string dbPeriode = periode;
            List<string> errorLogs = new List<string>();
            
            var uploadPayload = new List<TargetStockUploadItem>();
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
                            { ConfigureDataTable = (_) => new ExcelDataReader.ExcelDataTableConfiguration() { UseHeaderRow = true } });
                        
                        var dataTable = dataSet.Tables[0];
                        if (dataTable.Columns.Count < 3 || !dataTable.Columns[0].ColumnName.Equals("Parameter Type", StringComparison.OrdinalIgnoreCase))
                            return BadRequest("Invalid Excel format. Please download the correct template.");

                        int rowCount = 2;
                        foreach (DataRow row in dataTable.Rows)
                        {
                            string type = row[0]?.ToString()?.Trim() ?? "";
                            string identifier = row[1]?.ToString()?.Trim() ?? "";
                            string valueStr = row[2]?.ToString()?.Trim() ?? "";

                            if (string.IsNullOrEmpty(type)) continue;

                            if (!int.TryParse(valueStr, out int parsedValue))
                            {
                                errorLogs.Add($"Row {rowCount}: Value for '{identifier}' must be a valid whole number.");
                                rowCount++;
                                continue;
                            }

                            uploadPayload.Add(new TargetStockUploadItem {
                                ParameterType = type,
                                Identifier = identifier,
                                TargetValue = parsedValue
                            });
                            
                            rowCount++;
                        }
                    }
                }
                
                if (errorLogs.Count > 0)
                {
                    var topErrors = errorLogs.Take(10).Select(e => $"<li>{e}</li>");
                    return BadRequest($"<div style='text-align:left; padding:10px; background:#fdf2f2; border:1px solid #f2dede; border-radius:5px;'><ul style='color:#a94442; margin:0;'>{string.Join("", topErrors)}</ul></div>");
                }
                
                if(uploadPayload.Count == 0) return BadRequest("No data found.");
                
                var userId = User.FindFirst("PIC_ID")?.Value ?? "SystemUpload";
                string jsonData = System.Text.Json.JsonSerializer.Serialize(uploadPayload);
                
                using (SqlConnection conn = new SqlConnection(_configuration))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("sp_Upload_Target_Stock_Param", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Periode", dbPeriode);
                        cmd.Parameters.AddWithValue("@JsonData", jsonData);
                        cmd.Parameters.AddWithValue("@UserLogin", userId);

                        cmd.ExecuteNonQuery();
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
                
                List<string> orderFromList = new List<string>();
                using (SqlConnection conn = new SqlConnection(_configuration))
                {
                    conn.Open();
                    string sqlOrderFrom = "SELECT DISTINCT OrderFrom FROM M_Suffix_to_Unique su " +
                                          "INNER JOIN M_Customer_Order co ON su.SuffixCode = co.Suffix " +
                                          "WHERE OrderFrom IS NOT NULL " +
                                            "AND OrderFrom <> '' " +
                                            "AND su.OrderFrom <> 'DCWA' " +
                                            "AND co.Periode = @DbPeriode " +
                                          "ORDER BY OrderFrom";
                    
                    using (SqlCommand cmd = new SqlCommand(sqlOrderFrom, conn))
                    {
                        string dbPeriode = cleanPeriode.Substring(0, 4) + "-" + cleanPeriode.Substring(4, 2);
                        cmd.Parameters.AddWithValue("@DbPeriode", dbPeriode);

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read()) 
                            {
                                orderFromList.Add(dr["OrderFrom"].ToString());
                            }
                        }
                    }
                }
                
                if (orderFromList.Count == 0) { orderFromList.Add("SAP"); orderFromList.Add("KAP"); }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Target_Stock_Param");
            
                    worksheet.Cell(1, 1).Value = "Parameter Type";
                    worksheet.Cell(1, 2).Value = "Identifier (Order From / Parameter)";
                    worksheet.Cell(1, 3).Value = "Value (Unit/Days)";
                    worksheet.Cell(1, 4).Value = "Notes";
                    
                    var headerRange = worksheet.Range(1, 1, 1, 4);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.BabyBlue;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    
                    int currentRow = 2;
                    
                    // [DINAMIS]: Generate baris Excel sesuai jumlah Order From
                    foreach(var orderFrom in orderFromList)
                    {
                        worksheet.Cell(currentRow, 1).Value = "K-Line";
                        worksheet.Cell(currentRow, 2).Value = orderFrom;
                        worksheet.Cell(currentRow, 3).Value = 0; 
                        worksheet.Cell(currentRow, 4).Value = $"Input Cycle Delivery for {orderFrom}";
                        currentRow++;
                    }

                    // Baris Machining (Global)
                    worksheet.Cell(currentRow, 1).Value = "Machining";
                    worksheet.Cell(currentRow, 2).Value = "Standard Day";
                    worksheet.Cell(currentRow, 3).Value = 0; 
                    worksheet.Cell(currentRow, 4).Value = "Input Standard Target Day for Machining";
                    
                    int lastRow = currentRow;

                    worksheet.Range(2, 3, lastRow, 3).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFFDE7");
                    worksheet.Range(2, 1, lastRow, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(2, 1, lastRow, 4).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    worksheet.Columns().AdjustToContents();

                    worksheet.Protect("Admin-ICS");
                    worksheet.Column(1).Style.Protection.SetLocked(true);
                    worksheet.Column(2).Style.Protection.SetLocked(true);
                    worksheet.Column(4).Style.Protection.SetLocked(true);
                    worksheet.Range(2, 3, lastRow, 3).Style.Protection.SetLocked(false); 
                    
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Template_TargetStockParam_{cleanPeriode}.xlsx");
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
