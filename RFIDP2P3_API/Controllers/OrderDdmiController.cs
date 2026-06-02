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
    public class OrderDdmiController : Controller
    {
        private readonly string _connectionString;

        public OrderDdmiController(IConfiguration configuration)
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
                    using (SqlCommand cmd = new SqlCommand("sp_Inq_T_Daily_Order_DDMI", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Periode",
                            string.IsNullOrEmpty(periode) ? DateTime.Now.ToString("yyyy-MM") : periode);

                        conn.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                var dict = new Dictionary<string, object>();

                                for (int i = 0; i < sdr.FieldCount; i++)
                                {
                                    string colName = sdr.GetName(i);
                                    object val = sdr.GetValue(i);

                                    if (val == DBNull.Value)
                                        dict[colName] = null;
                                    else if (val is DateTime dt)
                                        dict[colName] = dt.ToString("yyyy-MM-dd");
                                    else
                                        dict[colName] = val;
                                }

                                orders.Add(dict);
                            }
                        }
                    }
                }

                return Ok(new { Data = orders, Total = orders.Count });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult Upload([FromForm] IFormFile file, [FromQuery] string UID)
        {
            var validation = FileHelper.ValidateFile(file, maxSizeInMb: 10, allowedExtensions: new[] { ".xls", ".xlsx", ".csv" });
            if (!validation.IsValid) return BadRequest(validation.ErrorMessage);

            try
            {
                DataTable dtUpload = new DataTable();
                dtUpload.Columns.Add("DnNo", typeof(string));
                dtUpload.Columns.Add("SupplierCode", typeof(string));
                dtUpload.Columns.Add("SupplierName", typeof(string));
                dtUpload.Columns.Add("Location", typeof(string));
                dtUpload.Columns.Add("OrderDate", typeof(DateTime));
                dtUpload.Columns.Add("OrderTime", typeof(TimeSpan));
                dtUpload.Columns.Add("DeliveryDate", typeof(DateTime));
                dtUpload.Columns.Add("DeliveryTime", typeof(TimeSpan));
                dtUpload.Columns.Add("Distribution", typeof(string));
                dtUpload.Columns.Add("DockCode", typeof(string));
                dtUpload.Columns.Add("CycleIssue", typeof(int));
                dtUpload.Columns.Add("Rev", typeof(string));
                dtUpload.Columns.Add("Page", typeof(int));
                dtUpload.Columns.Add("Remark", typeof(string));
                dtUpload.Columns.Add("SupplierApprovedBy", typeof(string));
                dtUpload.Columns.Add("SupplierPreparedBy", typeof(string));
                dtUpload.Columns.Add("TransporterDeliveryBy", typeof(string));
                dtUpload.Columns.Add("TransporterReceiverBy", typeof(string));
                dtUpload.Columns.Add("DdmiReceiverBy", typeof(string));
                dtUpload.Columns.Add("DdmiOrderBy", typeof(string));
                
                dtUpload.Columns.Add("ItemNo", typeof(int));
                dtUpload.Columns.Add("BackNo", typeof(string));
                dtUpload.Columns.Add("PartNo", typeof(string));
                dtUpload.Columns.Add("PartName", typeof(string));
                dtUpload.Columns.Add("QtyPerBox", typeof(int));
                dtUpload.Columns.Add("TotalKanban", typeof(int));
                dtUpload.Columns.Add("TotalQty", typeof(int));
                dtUpload.Columns.Add("ActualKanban", typeof(int));
                dtUpload.Columns.Add("LackKanban", typeof(int));

                using (var stream = file.OpenReadStream())
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration() {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = false }
                    });
                    DataTable dtExcel = result.Tables[0];
                    
                    bool isInsideItemTable = false;
                    var bufferDetails = new List<object[]>();
                    
                    string hDnNo = "", hSupplierCode = "", hSupplierName = "", hLocation = "", 
                           hDistribution = "", hDockCode = "", hRev = "", hRemark = "";
                    int hCycle = 0, hPage = 0;
                    DateTime? hOrderDate = null, hDeliveryDate = null;
                    TimeSpan? hOrderTime = null, hDeliveryTime = null;
                    string fSupAppv = "", fSupPrep = "", fTransDeliv = "", fTransRecv = "", fDdmiRecv = "", fDdmiOrd = "";

                    int idxNo = -1, idxBackNo = -1, idxPartNo = -1, idxPartName = -1;
                    int idxQtyBox = -1, idxTotalKbn = -1, idxTotalQty = -1, idxAct = -1, idxLack = -1;

                    string FindValueRight(DataRow rRow, int startCol)
                    {
                        for (int i = startCol + 1; i <= startCol + 8 && i < rRow.ItemArray.Length; i++)
                        {
                            string val = rRow[i]?.ToString()?.Trim() ?? "";
                            if (!string.IsNullOrWhiteSpace(val) && val != ":" && val != "-" && !val.ToUpper().StartsWith("PAGE"))
                                return val;
                        }
                        return "";
                    }

                    string FindSignature(int rowIdx, int colIdx)
                    {
                        for (int i = 1; i <= 3; i++)
                        {
                            if (rowIdx + i < dtExcel.Rows.Count)
                            {
                                string val = dtExcel.Rows[rowIdx + i][colIdx]?.ToString()?.Trim() ?? "";
                                if (!string.IsNullOrWhiteSpace(val) && val != "(" && val != ")" && val != ")(" && !val.ToUpper().StartsWith("NOTE"))
                                    return val;
                            }
                        }
                        return "";
                    }

                    void FlushDataKeTabel()
                    {
                        if (bufferDetails.Count == 0) return;
                        
                        foreach (var d in bufferDetails)
                        {
                            dtUpload.Rows.Add(
                                hDnNo, hSupplierCode, hSupplierName, hLocation,
                                hOrderDate.HasValue ? (object)hOrderDate.Value : DBNull.Value,
                                hOrderTime.HasValue ? (object)hOrderTime.Value : DBNull.Value,
                                hDeliveryDate.HasValue ? (object)hDeliveryDate.Value : DBNull.Value,
                                hDeliveryTime.HasValue ? (object)hDeliveryTime.Value : DBNull.Value,
                                hDistribution, hDockCode, hCycle, hRev, hPage == 0 ? DBNull.Value : hPage, hRemark,
                                fSupAppv, fSupPrep, fTransDeliv, fTransRecv, fDdmiRecv, fDdmiOrd,
                                d[0], d[1], d[2], d[3], d[4], d[5], d[6], d[7], d[8]
                            );
                        }
                        
                        bufferDetails.Clear();
                        idxNo = -1; idxBackNo = -1; idxPartNo = -1; idxPartName = -1;
                        idxQtyBox = -1; idxTotalKbn = -1; idxTotalQty = -1; idxAct = -1; idxLack = -1;
                        fSupAppv = ""; fSupPrep = ""; fTransDeliv = ""; fTransRecv = ""; fDdmiRecv = ""; fDdmiOrd = "";
                    }

                    for (int r = 0; r < dtExcel.Rows.Count; r++)
                    {
                        var row = dtExcel.Rows[r];
                        string colA = row[0]?.ToString()?.Trim().ToUpper() ?? "";

                        if (isInsideItemTable && (colA.StartsWith("SUPPLIER") || colA.Contains("TOTAL") || 
                                                  colA.Contains("REMARK") || colA.Contains("NOTE")))
                            isInsideItemTable = false;

                        if (!isInsideItemTable)
                        {
                            for (int c = 0; c < row.ItemArray.Length; c++)
                            {
                                string cellVal = (row[c]?.ToString()?.Trim() ?? "").ToUpper();
                                if (string.IsNullOrEmpty(cellVal)) continue;

                                string valClean = cellVal.Replace(" ", "");

                                if (valClean.StartsWith("DNNO") || valClean.StartsWith("SUPPLIERCODE"))
                                    FlushDataKeTabel();

                                if (valClean.StartsWith("DNNO")) hDnNo = FindValueRight(row, c);
                                else if (valClean.StartsWith("SUPPLIERCODE")) hSupplierCode = FindValueRight(row, c);
                                else if (valClean.StartsWith("SUPPLIERNAME")) hSupplierName = FindValueRight(row, c);
                                else if (valClean.StartsWith("LOCATION")) hLocation = FindValueRight(row, c);
                                else if (valClean.StartsWith("DISTRIBUTION")) hDistribution = FindValueRight(row, c);
                                else if (valClean.StartsWith("DOCKCODE")) hDockCode = FindValueRight(row, c);
                                else if (valClean.StartsWith("CYCLEISSUE")) int.TryParse(FindValueRight(row, c), out hCycle);
                                else if (valClean.StartsWith("REV")) hRev = FindValueRight(row, c);
                                else if (valClean.StartsWith("PAGE")) int.TryParse(FindValueRight(row, c), out hPage);
                                else if (valClean.StartsWith("REMARK")) hRemark = FindValueRight(row, c);
                                
                                else if (valClean == "DATE" || valClean.StartsWith("DATE:"))
                                {
                                    string dateStr = FindValueRight(row, c);
                                    if (DateTime.TryParse(dateStr, out DateTime dt))
                                    {
                                        if (c < 10) hOrderDate = dt.Date;
                                        else hDeliveryDate = dt.Date;
                                    }
                                }
                                
                                else if (valClean.StartsWith("TIME/SEQ") || valClean == "TIME" || valClean.StartsWith("TIME:"))
                                {
                                    string timeStr = FindValueRight(row, c);
            
                                    if (DateTime.TryParse(timeStr, out DateTime dtTime))
                                    {
                                        if (c < 10) hOrderTime = dtTime.TimeOfDay;
                                        else hDeliveryTime = dtTime.TimeOfDay;
                                    }
                                    
                                    else if (TimeSpan.TryParse(timeStr, out TimeSpan ts))
                                    {
                                        if (c < 10) hOrderTime = ts;
                                        else hDeliveryTime = ts;
                                    }
                                }

                                else if (valClean == "APPROVED" || valClean.StartsWith("APPROVEDBY")) fSupAppv = FindSignature(r, c);
                                else if (valClean == "PREPARED" || valClean.StartsWith("PREPAREDBY")) fSupPrep = FindSignature(r, c);
                                else if (valClean == "RECEIVER" && c < 22) fTransRecv = FindSignature(r, c);
                                else if (valClean == "RECEIVER" && c >= 22) fDdmiRecv = FindSignature(r, c);

                                else if (cellVal == "NO" && idxNo == -1) idxNo = c;
                                else if (cellVal == "BACK NO" && idxBackNo == -1) idxBackNo = c;
                                else if ((cellVal == "PART NO" || cellVal == "P" || cellVal.StartsWith("PART N")) && idxPartNo == -1) idxPartNo = c;
                                else if (cellVal == "PART NAME" && idxPartName == -1) idxPartName = c;
                                else if (cellVal.Contains("QTY") && cellVal.Contains("BOX") && idxQtyBox == -1) idxQtyBox = c;
                                else if (cellVal.Contains("TOTAL") && cellVal.Contains("KANBAN") && idxTotalKbn == -1) idxTotalKbn = c;
                                else if (cellVal.Contains("TOTAL QTY") && idxTotalQty == -1) idxTotalQty = c;
                                else if (cellVal == "ACT" && idxAct == -1) idxAct = c;
                                else if (cellVal == "LACK" && idxLack == -1) idxLack = c;
                            }

                            if (idxNo != -1 && idxPartNo != -1) isInsideItemTable = true;
                        }
                        
                        else if (isInsideItemTable)
                        {
                            string SafeGetString(int idx) => (idx != -1 && idx < row.ItemArray.Length) ? row[idx]?.ToString()?.Trim() ?? "" : "";
                            int SafeGetInt(int idx) {
                                if (idx != -1 && idx < row.ItemArray.Length && int.TryParse(row[idx]?.ToString()?.Trim(), out int val)) return val;
                                return 0;
                            }

                            string strItemNo = SafeGetString(idxNo);
                            string partNo = SafeGetString(idxPartNo);

                            if (!string.IsNullOrEmpty(strItemNo) || !string.IsNullOrEmpty(partNo))
                            {
                                if (int.TryParse(strItemNo, out int itemNo) && !string.IsNullOrEmpty(partNo))
                                {
                                    bufferDetails.Add(new object[] {
                                        itemNo, SafeGetString(idxBackNo), partNo, SafeGetString(idxPartName), 
                                        SafeGetInt(idxQtyBox), SafeGetInt(idxTotalKbn), SafeGetInt(idxTotalQty),
                                        SafeGetInt(idxAct), SafeGetInt(idxLack)
                                    });
                                }
                            }
                        }
                    }

                    FlushDataKeTabel();
                }

                if (dtUpload.Rows.Count == 0) 
                    return Ok(new { Remarks = "There is no valid detailed data in the file." });

                string firstDnNo = dtUpload.Rows[0]["DnNo"]?.ToString() ?? "";
                if (string.IsNullOrEmpty(firstDnNo))
                    return Ok(new { Remarks = "Format Excel tidak valid: DN NO tidak ditemukan." });

                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand("sp_Upload_T_Daily_Order_DDMI", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UploadDate", DateTime.Now.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@PIC_ID", string.IsNullOrEmpty(UID) ? "SYSTEM" : UID);
                    
                    SqlParameter tvpParam = cmd.Parameters.AddWithValue("@OrderData", dtUpload);
                    tvpParam.SqlDbType = SqlDbType.Structured; 
                    tvpParam.TypeName = "dbo.T_Daily_Order_DDMI_Type";
                    
                    cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, -1).Direction = ParameterDirection.Output;

                    conn.Open(); 
                    cmd.ExecuteNonQuery();
                    string spRemarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
                    
                    if (!string.IsNullOrEmpty(spRemarks)) 
                        return Ok(new { Remarks = spRemarks } );
                }
                
                return Ok(new { Remarks = "" });
            }
            catch (Exception e)
            { 
                return Ok(new { Remarks = "Internal Server Error " + e.Message}); 
            }
        }

        [HttpPost]
        public IActionResult Delete([FromBody] JsonElement payload)
        {
            try
            {
                if (!payload.TryGetProperty("UploadDate", out JsonElement dateElement))
                    return BadRequest("UploadDate is required.");

                string uploadDate = dateElement.GetString();

                using (SqlConnection conn = new SqlConnection(_connectionString))
                using (SqlCommand cmd = new SqlCommand("sp_Delete_T_Daily_Order_DDMI", conn))
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