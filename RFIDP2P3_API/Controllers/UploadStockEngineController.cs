using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UploadStockEngineController : ControllerBase
    {
        private readonly string _configuration;

        public UploadStockEngineController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<Dictionary<string,object>>> INQ(UploadInitStock uis)
        {
            var dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Inq_Upload_Init_Stock_Engine", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@Prod_Date", uis.Prod_Date));
                conn.Open();
                
                using (var da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }

                conn.Close();
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
        public ActionResult<string> Upload([FromForm] IFormFile file, [FromQuery] string UID)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("Excel file size cannot exceed 5MB.");

            string extension = Path.GetExtension(file.FileName).ToLower();
            if (extension != ".xls" && extension != ".xlsx")
                return BadRequest("Invalid file format. Please upload an Excel file (.xls or .xlsx).");

            int rowCount = 2; 
            List<string> errorLogs = new List<string>();
            HashSet<string> excelCombinations = new HashSet<string>();

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

                        string[] expectedHeaders = { "Periode (YYYY-MM)", "Unique", "Qty" };
                        foreach (string header in expectedHeaders)
                        {
                            if (!dataTable.Columns.Contains(header))
                                return BadRequest($"Invalid Excel format. Header column '{header}' is missing.");
                        }

                        using (SqlConnection conn = new SqlConnection(_configuration))
                        {
                            conn.Open();
                            
                            using (SqlTransaction transaction = conn.BeginTransaction())
                            {
                                foreach (DataRow row in dataTable.Rows)
                                {
                                    string periode = row["Periode (YYYY-MM)"]?.ToString()?.Trim();
                                    string uniqueCode = row["Unique"]?.ToString()?.Trim();
                                    string qtyStr = row["Qty"]?.ToString()?.Trim();

                                    if (string.IsNullOrEmpty(periode) && string.IsNullOrEmpty(uniqueCode) && string.IsNullOrEmpty(qtyStr))
                                    {
                                        rowCount++;
                                        continue;
                                    }

                                    if (string.IsNullOrEmpty(periode) || string.IsNullOrEmpty(uniqueCode) || string.IsNullOrEmpty(qtyStr))
                                    {
                                        errorLogs.Add($"Row {rowCount}: Periode, Unique, and Qty are mandatory.");
                                        rowCount++;
                                        continue;
                                    }

                                    if (!DateTime.TryParseExact(periode, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out DateTime parsedPeriode))
                                    {
                                        errorLogs.Add($"Row {rowCount}: Invalid Period format '{periode}'. It must be YYYY-MM.");
                                        rowCount++;
                                        continue;
                                    }

                                    if (!int.TryParse(qtyStr, out int qty))
                                    {
                                        errorLogs.Add($"Row {rowCount}: Qty must be a valid whole number.");
                                        rowCount++;
                                        continue;
                                    }

                                    string combinationKey = $"{periode}-{uniqueCode}";
                                    if (!excelCombinations.Add(combinationKey))
                                    {
                                        errorLogs.Add($"Row {rowCount}: Duplicate combination (Periode & Unique) found within the Excel file.");
                                        rowCount++;
                                        continue;
                                    }

                                    using (SqlCommand cmd = new SqlCommand("sp_Upload_Init_Stock_Engine_Row", conn, transaction))
                                    {
                                        cmd.CommandType = CommandType.StoredProcedure;
                                        cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                                        cmd.Parameters.Add(new SqlParameter("@Periode", periode));
                                        cmd.Parameters.Add(new SqlParameter("@UniqueCode", uniqueCode));
                                        cmd.Parameters.Add(new SqlParameter("@Qty", qty));
                                        cmd.Parameters.Add(new SqlParameter("@UserLogin", UID ?? "SystemUpload"));

                                        cmd.ExecuteNonQuery();
                                        
                                        string spRemarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);

                                        if (!string.IsNullOrEmpty(spRemarks))
                                        {
                                            errorLogs.Add($"Row {rowCount}: {spRemarks}");
                                        }
                                    }
                                    rowCount++;
                                }

                                if (errorLogs.Count > 0)
                                {
                                    transaction.Rollback();
                                    
                                    var topErrors = errorLogs.Take(10).Select(e => $"<li style='margin-bottom: 5px;'>{e}</li>");
                                    string combinedErrors = "<div style='text-align: left; max-height: 200px; overflow-y: auto; padding: 10px; background: #fdf2f2; border: 1px solid #f2dede; border-radius: 5px;'>" + 
                                                            "<ul style='padding-left: 20px; color: #a94442; font-size: 13px; margin: 0;'>" + 
                                                            string.Join("", topErrors) + 
                                                            "</ul>";
                                                            
                                    if (errorLogs.Count > 10)
                                    {
                                        combinedErrors += $"<p style='margin-top: 10px; font-size: 12px; color: #777;'><i>...and {errorLogs.Count - 10} more errors.</i></p>";
                                    }
                    
                                    combinedErrors += "</div>";
                                    return BadRequest(combinedErrors);
                                }
                                else
                                {
                                    transaction.Commit();
                                }
                            }
                        }
                    }
                }

                return Ok("success");
            }
            catch (Exception ex)
            {
                return BadRequest($"<div style='color:red;'>Critical System Error: {ex.Message}</div>");
            }
        }
    }
}