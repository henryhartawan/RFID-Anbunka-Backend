using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MasterSupplierController : ControllerBase
    {
        private readonly string _configuration;
        private string? remarks = "";

        public MasterSupplierController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterSupplier>> INQ()
        {
            List<MasterSupplier> Supplier = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Supplier_Sel", conn))
			{
                cmd.CommandType = CommandType.StoredProcedure;
				conn.Open();
				SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
                {
                    Supplier.Add(new MasterSupplier
                    {
                        SupplierCode = sdr["SupplierCode"].ToString(),
                        SupplierName = sdr["SupplierName"].ToString(),
                        SupplierAlias = sdr["SupplierAlias"].ToString(),
                        Address = sdr["Address"].ToString(),
                        City = sdr["City"].ToString(),
                        State = sdr["State"].ToString(),
                        Country = sdr["Country"].ToString(),
                        SupplierStatus = sdr["SupplierStatus"].ToString(),
                        LastUpdate = sdr["DateUpdate"].ToString(),
                        UserUpdate = sdr["UserUpdate"].ToString()
                    });
                }
                conn.Close();
            }
            return Supplier;
        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterSupplier>> DEL(MasterSupplier supplier)
        {
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Supplier_Del", conn))
			{
                cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
				
                cmd.Parameters.Add(new("@SupplierCode", supplier.SupplierCode));

				conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
            if (remarks != "") return BadRequest(remarks);
            else return Ok("success");
		}

        [HttpPost]
        public ActionResult<IEnumerable<MasterSupplier>> INS(MasterSupplier supplier)
        { 
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Supplier_Ins", conn))
			{
                cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@IUType", supplier.IUType));
                cmd.Parameters.Add(new("@SupplierCode", supplier.SupplierCode));
                cmd.Parameters.Add(new("@SupplierName", supplier.SupplierName));
                cmd.Parameters.Add(new("@SupplierAlias", supplier.SupplierAlias));
                cmd.Parameters.Add(new("@Address", supplier.Address));
                cmd.Parameters.Add(new("@City", supplier.City));
                cmd.Parameters.Add(new("@State", supplier.State));
                cmd.Parameters.Add(new("@Country", supplier.Country));
                cmd.Parameters.Add(new("@UserLogin", supplier.UserLogin));

				conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
            if (remarks != "") return BadRequest(remarks);
            else return Ok("success");

        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterSupplier>> ACT(MasterSupplier supplier)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_Supplier_Act", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@SupplierCode", supplier.SupplierCode));
                cmd.Parameters.Add(new("@UserLogin", supplier.UserLogin));

                conn.Open();
                cmd.ExecuteNonQuery();
                remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
                conn.Close();
            }
            if (remarks != "") return BadRequest(remarks);
            else return Ok("success");
        }

        [HttpPost]
        public async Task<List<RemarksNote>> Upload(IFormFile file, string? UID)
        {
            var list = new List<RemarksNote>();
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    BusinessObject b = new();
                    string remarks = b.UploadXLS(package, UID, _configuration);
                    if ("success" != remarks)
                    {
                        b.WriteLog(remarks, "XLSRemarks");
                    }
                    else
                    {
                        object result;
                        using (SqlConnection conn = new(_configuration))
                        {
                            conn.Open();
                            SqlCommand cmd = new("exec sp_M_Supplier_Upload @EntryUser", conn);
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.Add(new("@EntryUser", UID));
                            result = cmd.ExecuteScalar();
                            conn.Close();
                        }
                        remarks = result.ToString();
                    }
                    list.Add(new RemarksNote { Remarks = remarks });
                    return list;
                }
            }
        }
    }
}
