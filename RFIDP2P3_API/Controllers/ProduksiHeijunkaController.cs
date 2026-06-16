using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProduksiHeijunkaController : ControllerBase
    {
        private readonly string _configuration;
        private string? remarks = "";

        public ProduksiHeijunkaController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<Dictionary<string,object>>> INQ(Produksi pr)
        {
            var dt = new DataTable();

			using (SqlConnection conn = new SqlConnection(_configuration))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("sp_Inq_T_Calc_Produksi_PL_Heijunka", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@Periode_ID", pr.Prod_Date));
                cmd.Parameters.Add(new("@LineOrderCode", pr.Line));
                cmd.Parameters.Add(new("@UniqueNumber", pr.UniqueNumber));

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
    }
}
