using System.Data;
using System.Data.SqlClient;
using System.Text.Json;
using ClosedXML.Excel;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Helpers;
using RFIDP2P3_API.Models;

namespace RFIDP2P3_API.Controllers
{

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CustomerOrderController : Controller
    {
        private readonly string _configuration;
        private readonly IConfiguration _config;

        public CustomerOrderController(IConfiguration configuration)
        {
            _config = configuration;
            _configuration = configuration.GetConnectionString("DefaultConnection");
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

        [HttpPost]
        public ActionResult<IEnumerable<CustomerOrder>> INQ([FromBody] JsonElement body)
        {
            List<CustomerOrder> CustomerOrders = new();

            string periode = "";
            if (body.TryGetProperty("Periode", out JsonElement periodeElement))
            {
                periode = periodeElement.GetString() ?? "";
            }
            
            int revisionNo = -1;
            if (body.TryGetProperty("RevisionNo", out JsonElement revElement))
            {
                revisionNo = revElement.GetInt32();
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration))
                using (SqlCommand cmd = new SqlCommand("sp_M_Customer_Order_Sel", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Periode", periode);
                    cmd.Parameters.AddWithValue("@RevisionNo", revisionNo);

                    conn.Open();
                    SqlDataReader sdr = cmd.ExecuteReader();

                    while (sdr.Read())
                    {
                        CustomerOrders.Add(new CustomerOrder
                        {
                            CustomerOrderID = Convert.ToInt32(sdr["CustomerOrderID"]),
                            Periode = sdr["Periode"].ToString(),
                            Source = sdr["Source"].ToString(),
                            Suffix = sdr["Suffix"].ToString(),
                            DayNumber = Convert.ToInt32(sdr["DayNumber"]),
                            ValueData = Convert.ToDecimal(sdr["ValueData"]),
                            RevisionNo = Convert.ToInt32(sdr["RevisionNo"])
                        });
                    }

                    conn.Close();
                }

                return CustomerOrders;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPost]
        public IActionResult GetRevisions([FromBody] JsonElement body)
        {
            string periode = "";
            if (body.TryGetProperty("Periode", out JsonElement periodeElement))
            {
                periode = periodeElement.GetString() ?? "";
            }

            List<int> revisions = new List<int>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration))
                using (SqlCommand cmd = new SqlCommand("sp_M_Customer_Order_GetRevisions", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Periode", periode);

                    conn.Open();
                    using (SqlDataReader sdr = cmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            revisions.Add(Convert.ToInt32(sdr["RevisionNo"]));
                        }
                    }
                    conn.Close();
                }
                return Ok(revisions);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult Upload([FromForm] IFormFile file, [FromQuery] string UID)
        {
            var validation = FileHelper.ValidateFile(
                file, 
                maxSizeInMb: 5, 
                allowedExtensions: new[] { ".xls", ".xlsx" }
            );

            if (!validation.IsValid)
                return BadRequest(validation.ErrorMessage);

            try
            {
                bool isTestingMode = _config.GetValue<bool>("AppSettings:IsTestingMode");
                List<string> errorLogs = new List<string>();

                DataTable dtUpload = new DataTable();
                dtUpload.Columns.Add("Periode", typeof(string));
                dtUpload.Columns.Add("Source", typeof(string));
                dtUpload.Columns.Add("Suffix", typeof(string));
                dtUpload.Columns.Add("DayNumber", typeof(int));
                dtUpload.Columns.Add("ValueData", typeof(decimal));

                string[] validSources = { "SAP", "KAP", "DCWA", "TMMIN", "DDMI" };

                bool hasPastPeriod = false; 
                string currentPeriodeStr = DateTime.Now.ToString("yyyy-MM");
                
                using (var stream = file.OpenReadStream())
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true,
                        }
                    });

                    DataTable dtExcel = result.Tables[0];
                    int rowIndex = 1;

                    foreach (DataRow row in dtExcel.Rows)
                    {
                        rowIndex++;

                        string periodeRaw = row[0]?.ToString()?.Trim()?.ToUpper() ?? "";
                        string source = row[1]?.ToString()?.Trim()?.ToUpper();
                        string suffix = row[2]?.ToString()?.Trim()?.ToUpper();
                        
                        if (string.IsNullOrEmpty(periodeRaw) && string.IsNullOrEmpty(source) && string.IsNullOrEmpty(suffix))
                            continue;
                            
                        if (string.IsNullOrEmpty(periodeRaw) || string.IsNullOrEmpty(source) || string.IsNullOrEmpty(suffix))
                        {
                            errorLogs.Add($"Row {rowIndex}: Periode, Source, and Suffix cannot be empty.");
                            continue;
                        }

                        if (!validSources.Contains(source))
                        {
                            errorLogs.Add($"Row {rowIndex}: Invalid Source '{source}'. Valid sources are: SAP, KAP, DCWA, TMMIN, DDMI.");
                            continue;
                        }

                        string cleanPeriode = periodeRaw.Replace("-REV", "").Trim();

                        if (!DateTime.TryParseExact(cleanPeriode, "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out DateTime parsedPeriode))
                        {
                            errorLogs.Add($"Row {rowIndex}: Invalid Period format '{periodeRaw}'. It must be yyyy-MM or yyyy-MM-REV.");
                            continue;
                        }
                        
                        if (cleanPeriode.CompareTo(currentPeriodeStr) < 0)
                            hasPastPeriod = true;

                        int daysInMonth = DateTime.DaysInMonth(parsedPeriode.Year, parsedPeriode.Month);

                        for (int day = 1; day <= daysInMonth; day++)
                        {
                            int colIndex = day + 2;

                            if (colIndex < dtExcel.Columns.Count && row[colIndex] != DBNull.Value)
                            {
                                if (decimal.TryParse(row[colIndex].ToString(), out decimal valueData))
                                {
                                    if (valueData > 0)
                                    {
                                        dtUpload.Rows.Add(periodeRaw, source, suffix, day, valueData);
                                    }
                                }
                            }
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
                    {
                        combinedErrors += $"<p style='margin-top: 10px; font-size: 12px; color: #777;'><i>...and {errorLogs.Count - 10} more errors.</i></p>";
                    }
    
                    combinedErrors += "</div>";

                    return Ok(new[] { new
                    {
                        Remarks = combinedErrors, 
                        IsPastPeriod = false
                    } });
                }

                if (dtUpload.Rows.Count == 0)
                    return Ok(new[] { new
                    {
                        Remarks = "No valid data with value > 0 found in the Excel file.",
                        IsPastPeriod = false
                    } });

                using (SqlConnection conn = new SqlConnection(_configuration))
                using (SqlCommand cmd = new SqlCommand("sp_M_Customer_Order_Upload", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlParameter tvpParam = cmd.Parameters.AddWithValue("@OrderData", dtUpload);
                    tvpParam.SqlDbType = SqlDbType.Structured;
                    tvpParam.TypeName = "dbo.CustomerOrderType";
                    
                    cmd.Parameters.AddWithValue("@IsTestingMode", isTestingMode);
                    cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, -1).Direction = ParameterDirection.Output;

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    string spRemarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
                    conn.Close();

                    if (!string.IsNullOrEmpty(spRemarks))
                        return Ok(new[] { new
                        {
                            Remarks = "Database Error: " + spRemarks, 
                            IsPastPeriod = false
                        } });
                }

                return Ok(new[] { new
                {
                    Remarks = "", 
                    IsPastPeriod = hasPastPeriod
                } });
            }
            catch (Exception ex)
            {
                return Ok(new[] { new
                {
                    Remarks = "System Error: " + ex.Message, 
                    IsPastPeriod = false
                } });
            }
        }
        
        [HttpPost]
        public IActionResult Delete([FromBody] JsonElement body)
        {
            string periode = "";
            if (body.TryGetProperty("Periode", out JsonElement periodeElement))
                periode = periodeElement.GetString() ?? "";

            if (string.IsNullOrEmpty(periode))
                return BadRequest("Period cannot be empty.");

            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration))
                using (SqlCommand cmd = new SqlCommand("sp_M_Customer_Order_Delete", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Periode", periode);
                    cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, -1).Direction = ParameterDirection.Output;

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    string spRemarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
                    conn.Close();

                    if (!string.IsNullOrEmpty(spRemarks))
                        return Ok(new { Remarks = spRemarks });
                }

                return Ok(new { Remarks = "" });
            }
            catch (Exception ex)
            {
                return BadRequest("System Error: " + ex.Message);
            }
        }
    }
}