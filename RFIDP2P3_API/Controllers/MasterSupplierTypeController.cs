using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MasterSupplierTypeController : ControllerBase
    {
        private readonly string _configuration;
        private string? remarks = "";

        public MasterSupplierTypeController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterSupplierType>> INQ()
        {
            List<MasterSupplierType> SupplierType = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Supplier_Type_Sel", conn))
			{
                cmd.CommandType = CommandType.StoredProcedure;
				conn.Open();
				SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
                {
                    SupplierType.Add(new MasterSupplierType
                    {
                        SupplierType = sdr["SupplierType"].ToString(),
                        SupplierTypeDescription = sdr["SupplierTypeDescription"].ToString(),
                        SupplierTypeStatus = sdr["SupplierTypeStatus"].ToString(),
                        DateUpdate = sdr["DateUpdate"].ToString(),
                        UserUpdate = sdr["UserUpdate"].ToString()
                    });
                }
                conn.Close();
            }
            return SupplierType;
        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterSupplierType>> DEL(MasterSupplierType suppliertype)
        {
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Supplier_Type_Del", conn))
			{
                cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
				
                cmd.Parameters.Add(new("@SupplierType", suppliertype.SupplierType));

				conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
            if (remarks != "") return BadRequest(remarks);
            else return Ok("success");
		}

        [HttpPost]
        public ActionResult<IEnumerable<MasterSupplierType>> INS(MasterSupplierType suppliertype)
        {
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Supplier_Type_Ins", conn))
			{
                cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@IUType", suppliertype.IUType));
				cmd.Parameters.Add(new("@SupplierType", suppliertype.SupplierType));
                cmd.Parameters.Add(new("@SupplierTypeDescription", suppliertype.SupplierTypeDescription));
                cmd.Parameters.Add(new("@UserLogin", suppliertype.UserLogin));

				conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
            if (remarks != "") return BadRequest(remarks);
            else return Ok("success");

        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterSupplierType>> ACT(MasterSupplierType suppliertype)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_Supplier_Type_Act", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@SupplierType", suppliertype.SupplierType));
                cmd.Parameters.Add(new("@UserLogin", suppliertype.UserLogin));

                conn.Open();
                cmd.ExecuteNonQuery();
                remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
                conn.Close();
            }
            if (remarks != "") return BadRequest(remarks);
            else return Ok("success");
        }
    }
}
