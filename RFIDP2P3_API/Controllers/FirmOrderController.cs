using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using ExcelDataReader;
using RFIDP2P3_API.Helpers;
using RFIDP2P3_API.Models.Request;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class FirmOrderController : ControllerBase
    {
        private readonly string _configuration;

        public FirmOrderController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<Dictionary<string,object>>> INQ(FirmOrder FirmOrder)
        {
            var dt = new DataTable();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_Inq_T_Calc_Order_Firm", conn))
			{
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@Periode_ID", FirmOrder.UploadDate));
                cmd.Parameters.Add(new("@Status", FirmOrder.Status));
                conn.Open();
                
                using (var da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }

                conn.Close();
            }

            var result = new List<Dictionary<string, object>>();
            DateTime baseDate;
            bool isDateValid = DateTime.TryParseExact(
                FirmOrder.UploadDate, 
                "yyyyMM", 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None, 
                out baseDate
            );

            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    string colName = col.ColumnName;
                    int offset = -1;
                    
                    if (colName.Trim().ToUpper() == "N") 
                        offset = 1; 
                    
                    else if (colName.Trim().ToUpper().StartsWith("N+")) 
                    {
                        if (int.TryParse(colName.Substring(2), out int num))
                        {
                            offset = num + 1; 
                        }
                    }
                    
                    else if (int.TryParse(colName, out int num)) 
                        offset = num; 
                    
                    if (isDateValid && offset >= 0)
                    {
                        DateTime targetDate = baseDate.AddMonths(offset);
                        string monthName = targetDate.ToString("MMM_yyyy", new CultureInfo("id-ID"));
                        dict["_" + monthName] = row[col];
                    }
                    else
                    {
                        dict[colName] = row[col];
                    }
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

            int rowCount = 2; 
            List<string> errorLogs = new List<string>();
            HashSet<string> excelCombinations = new HashSet<string>();
    
            DataTable tempDbTable = new DataTable();
            tempDbTable.Columns.Add("Periode", typeof(string));
            tempDbTable.Columns.Add("Suffix", typeof(string));
            tempDbTable.Columns.Add("Status", typeof(string));
            tempDbTable.Columns.Add("MonthOffsetLabel", typeof(string));
            tempDbTable.Columns.Add("Qty", typeof(int));
            tempDbTable.Columns.Add("CreatedUser", typeof(string));
    
            string currentUser = UID ?? "SystemUpload";
            
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
                        string[] expectedHeaders = { "Periode", "SFX", "Status", "N" };
                
                        foreach (string header in expectedHeaders)
                        {
                            if (!dataTable.Columns.Contains(header))
                                return BadRequest($"Invalid Excel format. Header column '{header}' is missing.");
                        }
                        
                        Dictionary<string, bool> columnHasData = new Dictionary<string, bool>();
                        for (int i = 3; i < dataTable.Columns.Count; i++)
                        {
                            columnHasData[dataTable.Columns[i].ColumnName] = false;
                        }
                        
                        foreach (DataRow row in dataTable.Rows)
                        {
                            string periode = row["Periode"]?.ToString()?.Trim();
                            string sfx = row["SFX"]?.ToString()?.Trim();
                            string status = row["Status"]?.ToString()?.Trim();

                            if (string.IsNullOrEmpty(periode) && string.IsNullOrEmpty(sfx) && string.IsNullOrEmpty(status))
                            {
                                rowCount++;
                                continue;
                            }
                            
                            if (string.IsNullOrEmpty(periode) || string.IsNullOrEmpty(sfx) || string.IsNullOrEmpty(status))
                            {
                                errorLogs.Add($"Row {rowCount}: Periode, SFX, and Status are mandatory fields.");
                                rowCount++;
                                continue;
                            }
                    
                            if (!DateTime.TryParseExact(periode, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out DateTime parsedPeriode))
                            {
                                errorLogs.Add($"Row {rowCount}: Invalid Periode format '{periode}'. Must be in YYYY-MM format.");
                                rowCount++;
                                continue;
                            }
                            
                            string statusLower = status.ToLower();
                            if (statusLower != "firm" && statusLower != "tentative")
                            {
                                errorLogs.Add($"Row {rowCount}: Invalid Status value. It must be strictly 'Firm', 'Tentative'. Found: '{status}'.");
                                rowCount++;
                                continue;
                            }
                            
                            string combinationKey = $"{periode}-{sfx}-{status}";
                            if (!excelCombinations.Add(combinationKey))
                            {
                                errorLogs.Add($"Row {rowCount}: Duplicate combination (Periode, SFX, Status) found in the uploaded file.");
                                rowCount++;
                                continue;
                            }
                            
                            for (int colIndex = 3; colIndex < dataTable.Columns.Count; colIndex++)
                            {
                                string colName = dataTable.Columns[colIndex].ColumnName;
                                string qtyStr = row[colIndex]?.ToString()?.Trim();

                                if (string.IsNullOrEmpty(qtyStr)) qtyStr = "0";

                                if (!int.TryParse(qtyStr, out int qty))
                                {
                                    errorLogs.Add($"Row {rowCount} (Column {colName}): Qty must be a valid integer.");
                                    continue;
                                }
                                
                                if (qty > 0)
                                {
                                    columnHasData[colName] = true;
                                }

                                tempDbTable.Rows.Add(periode, sfx, status, colName, qty, currentUser);
                            }
                            rowCount++;
                        }
                        
                        foreach (var kvp in columnHasData)
                        {
                            if (!kvp.Value)
                            {
                                errorLogs.Add($"Column '{kvp.Key}' is completely empty or contains only zeros. Please provide valid data.");
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
                                combinedErrors += $"<p style='margin-top: 10px; font-size: 12px; color: #777;'><i>...and {errorLogs.Count - 10} more errors.</i></p>";

                            combinedErrors += "</div>";
                            return BadRequest(combinedErrors);
                        }
                        
                        using (SqlConnection conn = new SqlConnection(_configuration))
                        {
                            conn.Open();

                            using (SqlTransaction transaction = conn.BeginTransaction())
                            {
                                try
                                {
                                    using (SqlCommand cmdClear = new SqlCommand("DELETE FROM T_Calc_Order_Firm_Temp WHERE CreatedUser = @UID", conn, transaction))
                                    {
                                        cmdClear.Parameters.AddWithValue("@UID", currentUser);
                                        cmdClear.ExecuteNonQuery();
                                    }

                                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, transaction))
                                    {
                                        bulkCopy.DestinationTableName = "T_Calc_Order_Firm_Temp";
                                        bulkCopy.ColumnMappings.Add("Periode", "Periode");
                                        bulkCopy.ColumnMappings.Add("Suffix", "Suffix");
                                        bulkCopy.ColumnMappings.Add("Status", "Status");
                                        bulkCopy.ColumnMappings.Add("MonthOffsetLabel", "MonthOffsetLabel");
                                        bulkCopy.ColumnMappings.Add("Qty", "Qty");
                                        bulkCopy.ColumnMappings.Add("CreatedUser", "CreatedUser");

                                        bulkCopy.WriteToServer(tempDbTable);
                                    }

                                    using (SqlCommand cmdProcess = new SqlCommand("sp_Upload_T_Calc_Order_Firm", conn, transaction))
                                    {
                                        cmdProcess.CommandType = CommandType.StoredProcedure;
                                        cmdProcess.Parameters.AddWithValue("@EntryUser", currentUser);

                                        object result = cmdProcess.ExecuteScalar();
                                        string spRemarks = result?.ToString() ?? "";

                                        if (!string.IsNullOrEmpty(spRemarks) && spRemarks.ToLower() != "success" && spRemarks != "")
                                        {
                                            if (transaction.Connection != null) 
                                                transaction.Rollback();
                                            
                                            return BadRequest($"<div style='text-align: left; padding: 10px; background: #fdf2f2; border: 1px solid #f2dede; border-radius: 5px; color: #a94442;'>Upload Rejected: {spRemarks}</div>");
                                        }
                                    }

                                    transaction.Commit();
                                }
                                catch (Exception)
                                {
                                    transaction.Rollback();
                                    throw;
                                }
                            }
                        }
                    }
                }
                return Ok("success");
            }
            catch (Exception)
            {
                return BadRequest("<div style='text-align: left; padding: 10px; background: #fdf2f2; border: 1px solid #f2dede; border-radius: 5px; color: #a94442; z-index: 9999;'>" +
                                  "Terjadi kesalahan pada sistem saat memproses file. Silakan coba beberapa saat lagi atau hubungi administrator." +
                                  "</div>");
            }
        }
    }
}
