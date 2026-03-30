using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data.SqlClient;
using System.Data;
using ExcelDataReader;

namespace RFIDP2P3_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class MasterSuffixToUniqueController : Controller
	{
		private readonly string _configuration;
		private string? remarks = "";

		public MasterSuffixToUniqueController(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterSuffixToUnique>> INQ()
		{
			List<MasterSuffixToUnique> dataList = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Suffix_to_Unique_Sel", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				conn.Open();
				SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
				{
					dataList.Add(new MasterSuffixToUnique
					{
						SuffixId = sdr["SuffixId"].ToString(),
						SuffixCode = sdr["SuffixCode"].ToString(),
						UniqueCode = sdr["UniqueCode"].ToString(),
						ModelGroup = sdr["ModelGroup"].ToString(),
						LineOrderCode = sdr["LineOrderCode"].ToString(),
						CreatedBy = sdr["CreatedBy"].ToString(),
						CreatedDate = sdr["CreatedDate"].ToString(),
						UpdatedBy = sdr["UpdatedBy"].ToString(),
						UpdatedDate = sdr["UpdatedDate"].ToString()
                    });
				}
				conn.Close();
			}
			return dataList;
		}

		[HttpPost]
		public ActionResult<string> INS(MasterSuffixToUnique model)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Suffix_to_Unique_Ins", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new SqlParameter("@IUType", model.IUType));
				cmd.Parameters.Add(new SqlParameter("@SuffixId", model.SuffixId ?? (object)DBNull.Value));
				cmd.Parameters.Add(new SqlParameter("@SuffixCode", model.SuffixCode));
				cmd.Parameters.Add(new SqlParameter("@UniqueCode", model.UniqueCode));
				cmd.Parameters.Add(new SqlParameter("@ModelGroup", model.ModelGroup ?? (object)DBNull.Value));
				cmd.Parameters.Add(new SqlParameter("@LineOrderCode", model.LineOrderCode));
				cmd.Parameters.Add(new SqlParameter("@UserLogin", model.UserLogin));

				conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (!string.IsNullOrEmpty(remarks)) return BadRequest(remarks);
			else return Ok("success");
		}

		[HttpPost]
		public ActionResult<string> DEL(MasterSuffixToUnique model)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Suffix_to_Unique_Del", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new SqlParameter("@SuffixId", model.SuffixId));

				conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (!string.IsNullOrEmpty(remarks)) return BadRequest(remarks);
			else return Ok("success");
        }
		
		[HttpPost]
		public ActionResult<string> UploadExcel([FromForm] IFormFile file, [FromForm] string UserLogin)
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

		                string[] expectedHeaders = { "Line", "Model Group", "Unique", "Suffix" };
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
		                            string lineOrderCode = row["Line"]?.ToString()?.Trim();
		                            string modelGroup = row["Model Group"]?.ToString()?.Trim();
		                            string uniqueCode = row["Unique"]?.ToString()?.Trim();
		                            string suffixCode = row["Suffix"]?.ToString()?.Trim();

		                            if (string.IsNullOrEmpty(lineOrderCode) && string.IsNullOrEmpty(uniqueCode) && 
		                                string.IsNullOrEmpty(suffixCode) && string.IsNullOrEmpty(modelGroup))
		                            {
		                                rowCount++;
		                                continue;
		                            }

		                            if (string.IsNullOrEmpty(lineOrderCode) || string.IsNullOrEmpty(uniqueCode) || string.IsNullOrEmpty(suffixCode))
		                            {
		                                errorLogs.Add($"Row {rowCount}: Line, Unique, and Suffix are mandatory fields.");
		                                rowCount++;
		                                continue;
		                            }

		                            string combinationKey = $"{lineOrderCode}-{uniqueCode}-{suffixCode}";
		                            if (!excelCombinations.Add(combinationKey))
		                            {
		                                errorLogs.Add($"Row {rowCount}: Duplicate combination found within the Excel file.");
		                                rowCount++;
		                                continue;
		                            }

		                            using (SqlCommand cmd = new SqlCommand("sp_M_Suffix_to_Unique_Upload", conn, transaction))
		                            {
		                                cmd.CommandType = CommandType.StoredProcedure;
		                                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

		                                cmd.Parameters.Add(new SqlParameter("@SuffixCode", suffixCode));
		                                cmd.Parameters.Add(new SqlParameter("@UniqueCode", uniqueCode));
		                                cmd.Parameters.Add(new SqlParameter("@ModelGroup", string.IsNullOrEmpty(modelGroup) ? (object)DBNull.Value : modelGroup));
		                                cmd.Parameters.Add(new SqlParameter("@LineOrderCode", lineOrderCode));
		                                cmd.Parameters.Add(new SqlParameter("@UserLogin", UserLogin ?? "SystemUpload"));
		                                
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
		        return BadRequest($"Critical System Error: {ex.Message}");
		    }
		}
		
    }
}
