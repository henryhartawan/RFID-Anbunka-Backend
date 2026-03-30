using System.Data;
using System.Data.SqlClient;
using System.Text.Json;
using ClosedXML.Excel;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;

namespace RFIDP2P3_API.Controllers
{

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MasterCustomerOrderController : Controller
    {
        private readonly string _configuration;

        public MasterCustomerOrderController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterCustomerOrder>> INQ([FromBody] JsonElement body)
        {
            List<MasterCustomerOrder> CustomerOrders = new();

            string periode = "";
            if (body.TryGetProperty("Periode", out JsonElement periodeElement))
            {
                periode = periodeElement.GetString() ?? "";
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration))
                using (SqlCommand cmd = new SqlCommand("sp_M_Customer_Order_Sel", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Periode", periode);

                    conn.Open();
                    SqlDataReader sdr = cmd.ExecuteReader();

                    while (sdr.Read())
                    {
                        CustomerOrders.Add(new MasterCustomerOrder
                        {
                            CustomerOrderID = Convert.ToInt32(sdr["CustomerOrderID"]),
                            Periode = sdr["Periode"].ToString(),
                            Source = sdr["Source"].ToString(),
                            Suffix = sdr["Suffix"].ToString(),
                            DayNumber = Convert.ToInt32(sdr["DayNumber"]),
                            ValueData = Convert.ToDecimal(sdr["ValueData"])
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
        public IActionResult Upload([FromForm] IFormFile file, [FromQuery] string UID)
        {
            if (file == null || file.Length == 0)
                return Ok(new[] { new { Remarks = "File not found or empty." } });

            string ext = Path.GetExtension(file.FileName).ToLower();
            if (ext != ".xls" && ext != ".xlsx")
                return Ok(new[]
                    { new { Remarks = "Invalid file format. Please upload an Excel file (.xls or .xlsx)." } });

            try
            {
                List<string> errorLogs = new List<string>();

                DataTable dtUpload = new DataTable();
                dtUpload.Columns.Add("Periode", typeof(string));
                dtUpload.Columns.Add("Source", typeof(string));
                dtUpload.Columns.Add("Suffix", typeof(string));
                dtUpload.Columns.Add("DayNumber", typeof(int));
                dtUpload.Columns.Add("ValueData", typeof(decimal));

                // Mendefinisikan list validSources
                string[] validSources = { "SAP", "KAP", "DCWA", "TMMIN", "DOMI" };

                using (var stream = file.OpenReadStream())
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true,
                            // Tidak perlu skip row tambahan agar sinkron dengan index
                            // ReadHeaderRow dihilangkan agar by default membaca row 1 sebagai header
                        }
                    });

                    DataTable dtExcel = result.Tables[0];
                    int rowIndex = 1;

                    foreach (DataRow row in dtExcel.Rows)
                    {
                        rowIndex++;

                        // Akses via index agar aman dari perubahan nama header
                        string periode = row[0]?.ToString()?.Trim();
                        string source = row[1]?.ToString()?.Trim()?.ToUpper();
                        string suffix = row[2]?.ToString()?.Trim()?.ToUpper();

                        // Lewati baris yang kosong sepenuhnya
                        if (string.IsNullOrEmpty(periode) && string.IsNullOrEmpty(source) && string.IsNullOrEmpty(suffix))
                            continue;
                            
                        // Jika ada data yang tidak lengkap dalam 3 kolom utama
                        if (string.IsNullOrEmpty(periode) || string.IsNullOrEmpty(source) || string.IsNullOrEmpty(suffix))
                        {
                            errorLogs.Add($"Row {rowIndex}: Periode, Source, and Suffix cannot be empty.");
                            continue;
                        }

                        // VALIDASI SOURCE
                        if (!validSources.Contains(source))
                        {
                            errorLogs.Add($"Row {rowIndex}: Invalid Source '{source}'. Valid sources are: SAP, KAP, DCWA, TMMIN, DOMI.");
                            continue;
                        }

                        // VALIDASI FORMAT PERIODE
                        if (!DateTime.TryParseExact(periode, "yyyy-MM", null, System.Globalization.DateTimeStyles.None,
                                out DateTime parsedPeriode))
                        {
                            errorLogs.Add($"Row {rowIndex}: Invalid Period format '{periode}'. It must be yyyy-MM.");
                            continue;
                        }

                        int daysInMonth = DateTime.DaysInMonth(parsedPeriode.Year, parsedPeriode.Month);

                        // LOOPING HARI (Menggunakan Index Kolom)
                        // Index 0 = Periode, Index 1 = Source, Index 2 = Suffix, Index 3 = Tanggal 1, dst.
                        for (int day = 1; day <= daysInMonth; day++)
                        {
                            int colIndex = day + 2; // Menentukan posisi index kolom berdasarkan tanggal

                            // Cek apakah index kolom masih ada dalam batas file excel yang diunggah
                            if (colIndex < dtExcel.Columns.Count && row[colIndex] != DBNull.Value)
                            {
                                if (decimal.TryParse(row[colIndex].ToString(), out decimal valueData))
                                {
                                    // Membulatkan (0.5 jadi 1, 0.49 jadi 0)
                                    decimal roundedValue = Math.Round(valueData, 0, MidpointRounding.AwayFromZero);

                                    // Hanya simpan jika nilainya lebih dari 0
                                    if (roundedValue > 0)
                                    {
                                        dtUpload.Rows.Add(periode, source, suffix, day, roundedValue);
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

                    return Ok(new[] { new { Remarks = combinedErrors } });
                }

                if (dtUpload.Rows.Count == 0)
                    return Ok(new[]
                        { new { Remarks = "No valid data with value > 0 found in the Excel file." } });

                using (SqlConnection conn = new SqlConnection(_configuration))
                using (SqlCommand cmd = new SqlCommand("sp_M_Customer_Order_Upload", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlParameter tvpParam = cmd.Parameters.AddWithValue("@OrderData", dtUpload);
                    tvpParam.SqlDbType = SqlDbType.Structured;
                    tvpParam.TypeName = "dbo.CustomerOrderType";

                    cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, -1).Direction = ParameterDirection.Output;

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    string spRemarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
                    conn.Close();

                    if (!string.IsNullOrEmpty(spRemarks))
                        return Ok(new[] { new { Remarks = "Database Error: " + spRemarks } });
                }

                return Ok(new[] { new { Remarks = "" } });
            }
            catch (Exception ex)
            {
                return Ok(new[] { new { Remarks = "System Error: " + ex.Message } });
            }
        }
    }
}