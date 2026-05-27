using System.Data;
using System.Data.SqlClient;
using System.Text.Json;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Helpers;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class OrderTmminController : Controller
    {
        private readonly string _connectionString;

        public OrderTmminController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }
        
        [HttpPost]
        public IActionResult INQ([FromBody] JsonElement body)
        {
            string periode = body.TryGetProperty("Periode", out JsonElement pEl) ? pEl.GetString() ?? "" : "";
            List<Dictionary<string, object>> orders = new();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_Inq_T_Daily_Order_TMMIN", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Periode", string.IsNullOrEmpty(periode) ? DateTime.Now.ToString("yyyy-MM") : periode);
                        
                        conn.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                var dict = new Dictionary<string, object>();
                                dict["orderID"] = Convert.ToInt32(sdr["OrderID"]);
                                dict["uploadDate"] = Convert.ToDateTime(sdr["UploadDate"]).ToString("yyyy-MM-dd");
                                
                                string rawJson = sdr["RawDataJSON"].ToString();
                                if (!string.IsNullOrEmpty(rawJson))
                                {
                                    var rawDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(rawJson);
                                    if (rawDict != null)
                                    {
                                        foreach (var kvp in rawDict)
                                        {
                                            dict[kvp.Key] = kvp.Value.ValueKind == JsonValueKind.String ? kvp.Value.GetString() : kvp.Value.ToString();
                                        }
                                    }
                                }
                                orders.Add(dict);
                            }
                        }
                    }
                }
                return Ok(orders);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpPost]
        public IActionResult Upload([FromForm] IFormFile file, [FromQuery] string UID)
        {
            var validation = FileHelper.ValidateFile(
                file, 
                maxSizeInMb: 10, 
                allowedExtensions: new[]
                {
                    ".xls", ".xlsx", ".csv" 
                });
            
            if (!validation.IsValid) 
                return BadRequest(validation.ErrorMessage);

            try
            {
                DataTable dtUpload = new DataTable();
                dtUpload.Columns.Add("OrderNo", typeof(string));
                dtUpload.Columns.Add("PartNo", typeof(string));
                dtUpload.Columns.Add("PcsPerKbn", typeof(int));
                dtUpload.Columns.Add("TotalPcs", typeof(int));
                dtUpload.Columns.Add("Kanban", typeof(int));
                dtUpload.Columns.Add("OrderDate", typeof(DateTime));
                dtUpload.Columns.Add("Cycle", typeof(string));
                dtUpload.Columns.Add("RawDataJSON", typeof(string));

                using (var stream = file.OpenReadStream())
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true
                        }
                    });
                    
                    DataTable dtExcel = result.Tables[0];
                    
                    foreach (DataRow row in dtExcel.Rows)
                    {
                        var rawDataDict = new Dictionary<string, string>();
                        
                        for (int i = 0; i <= 99; i++)
                        {
                            string key = (i == 0) ? "col0" : (i == 1) ? "col1" : $"_{i - 1}";
                            string val = (i < row.ItemArray.Length) ? row[i]?.ToString()?.Trim() ?? "" : "";
                            
                            if (i < row.ItemArray.Length && row[i] != null && row[i] != DBNull.Value)
                            {
                                if (row[i] is DateTime dtVal)
                                    val = dtVal.ToString("yyyy-MM-dd HH:mm:ss");
                                else
                                    val = row[i].ToString().Trim();
                            }
                            
                            rawDataDict.Add(key, val);
                        }
                        
                        string orderNo = rawDataDict["col0"];
                        if (string.IsNullOrEmpty(orderNo)) 
                            continue;

                        string partNo = rawDataDict["_9"];
                        int.TryParse(rawDataDict["_14"], out int pcsPerKbn);
                        int.TryParse(rawDataDict["_15"], out int totalPcs);
                        int.TryParse(rawDataDict["_16"], out int kanban);
                        
                        DateTime? orderDate = null;
                        if (DateTime.TryParse(rawDataDict["_23"], out DateTime parsedDate)) 
                            orderDate = parsedDate;
                        
                        string cycle = rawDataDict["_26"];
                        string rawDataJson = JsonSerializer.Serialize(rawDataDict);

                        dtUpload.Rows.Add(orderNo, partNo, pcsPerKbn, totalPcs, kanban, 
                            orderDate.HasValue ? orderDate.Value : DBNull.Value, 
                            cycle, rawDataJson);
                    }
                }

                if (dtUpload.Rows.Count == 0) return Ok(new[]
                {
                    new { Remarks = "Tidak ada data valid yang ditemukan." }
                });

                string todaySystemDate = DateTime.Now.ToString("yyyy-MM-dd");
                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand("sp_Upload_T_Daily_Order_TMMIN", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UploadDate", todaySystemDate);
                    cmd.Parameters.AddWithValue("@PIC_ID", string.IsNullOrEmpty(UID) ? "SYSTEM" : UID);
                    
                    SqlParameter tvpParam = cmd.Parameters.AddWithValue("@OrderData", dtUpload);
                    tvpParam.SqlDbType = SqlDbType.Structured; 
                    tvpParam.TypeName = "dbo.DailyOrderTMMIN_Type";
                    cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, -1).Direction = ParameterDirection.Output;

                    conn.Open(); cmd.ExecuteNonQuery();
                    string spRemarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
                    
                    if (!string.IsNullOrEmpty(spRemarks)) 
                        return Ok(new[]
                        {
                            new { Remarks = spRemarks }
                        });
                }
                return Ok(new[]
                {
                    new { Remarks = "" }
                });
            }
            catch (Exception e){ 
                return Ok(new[] {
                    new { Remarks = "Internal Server Error " + e}
                }); 
            }
        }

        [HttpPost]
        public IActionResult Delete([FromBody] JsonElement payload)
        {
            try
            {
                if (!payload.TryGetProperty("UploadDate", out JsonElement dateElement))
                {
                    return BadRequest("UploadDate is required.");
                }

                string uploadDate = dateElement.GetString();

                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand("sp_Delete_T_Daily_Order_TMMIN", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UploadDate", uploadDate);
            
                    SqlParameter remarksParam = new SqlParameter("@Remarks", SqlDbType.VarChar, -1)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(remarksParam);

                    conn.Open(); 
                    cmd.ExecuteNonQuery();

                    string remarks = remarksParam.Value?.ToString();

                    if (!string.IsNullOrEmpty(remarks))
                    {
                        if (remarks.StartsWith("System Error"))
                            return StatusCode(500, "A system error occurred while deleting data.");

                        return BadRequest(remarks); 
                    }
                }
        
                return Ok("success");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "A system error occurred. Please try again later.");
            }
        }
    }
}