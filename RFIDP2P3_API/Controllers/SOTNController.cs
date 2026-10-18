using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SOTNController : ControllerBase
    {
        private readonly string _configuration;
        private string? remarks = "";

        public SOTNController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<Dictionary<string,object>>> INQRoute(SOTN so)
        {
            var dt = new DataTable();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_Inq_SO_TN_Route", conn))
			{
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@LineOrderCode", so.Line));
                cmd.Parameters.Add(new("@SupplierCode", so.Supplier));
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
        public ActionResult<IEnumerable<Dictionary<string, object>>> INQCycle(SOTN so)
        {
            var dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Inq_SO_TN_Cycle", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@LineOrderCode", so.Line));
                cmd.Parameters.Add(new("@RouteCode", so.Route));
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
        public ActionResult<IEnumerable<SOTN>> INS(SOTN so)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Submit_T_SO_TN", conn))
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@LineOrderCode", so.Line));
                cmd.Parameters.Add(new("@SupplierCode", so.Supplier));
                cmd.Parameters.Add(new("@DeliveryDate", so.DeliveryDate));
                cmd.Parameters.Add(new("@RouteCode", so.Route));
                cmd.Parameters.Add(new("@CycleIssueLP", so.CycleIssueLP));
                cmd.Parameters.Add(new("@Remark", so.Remarks));
                cmd.Parameters.Add(new("@DN_No", so.DN_No));
                cmd.Parameters.Add(new("@UserLogin", so.UserLogin));
                
                cmd.ExecuteNonQuery();
                remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
                conn.Close();
            }
            if (remarks != "success") return BadRequest(remarks);
            else return Ok("success");
        }

        [HttpPost]
        public ActionResult<IEnumerable<Dictionary<string, object>>> INQ(SOTN so)
        {
            var dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Inq_T_SO_TN", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@Periode", so.Periode));
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
        public ActionResult<IEnumerable<Dictionary<string, object>>> INQDN(SOTN so)
        {
            var dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Inq_SO_TN_DN", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@LineOrderCode", so.Line));
                cmd.Parameters.Add(new("@DeliveryDate", so.DeliveryDate));
                cmd.Parameters.Add(new("@RouteCode", so.Route));
                cmd.Parameters.Add(new("@CycleIssueLP", so.CycleIssueLP));
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
    }
}
