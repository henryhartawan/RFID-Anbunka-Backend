using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SOController : ControllerBase
    {
        private readonly string _configuration;
        private string? remarks = "";

        public SOController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<Dictionary<string,object>>> INQRoute(SO so)
        {
            var dt = new DataTable();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_Inq_SO_Route", conn))
			{
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@ExCore", so.ExCore));
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
        public ActionResult<IEnumerable<Dictionary<string, object>>> INQPart(SO so)
        {
            var dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Inq_SO_Part", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@LineOrderCode", so.Line));
                cmd.Parameters.Add(new("@SupplierCode", so.Supplier));
                cmd.Parameters.Add(new("@JobNo", so.JobNo));
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
        public ActionResult<IEnumerable<Dictionary<string, object>>> INQPartDetail(SO so)
        {
            var dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Inq_SO_Part_Detail", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@ExCore", so.ExCore));
                cmd.Parameters.Add(new("@LineOrderCode", so.Line));
                cmd.Parameters.Add(new("@SupplierCode", so.Supplier));
                cmd.Parameters.Add(new("@ArrivalDate", so.ArrivalDate));
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
        public ActionResult<IEnumerable<Dictionary<string, object>>> INQCycle(SO so)
        {
            var dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Inq_SO_Cycle", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
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
        public ActionResult<IEnumerable<SO>> INS(SO so)
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string idStr = "";

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Submit_T_SO", conn))
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@SO_No", SqlDbType.VarChar, 50));
                cmd.Parameters.Add(new("@LineOrderCode", SqlDbType.VarChar, 50));
                cmd.Parameters.Add(new("@SupplierCode", SqlDbType.VarChar, 50));
                cmd.Parameters.Add(new("@ArrivalDate", SqlDbType.VarChar, 10));
                cmd.Parameters.Add(new("@RouteCode", SqlDbType.VarChar, 50));
                cmd.Parameters.Add(new("@CycleIssue", SqlDbType.VarChar, 50));
                cmd.Parameters.Add(new("@Volumetric", SqlDbType.VarChar));
                cmd.Parameters.Add(new("@TotalPartVolume", SqlDbType.VarChar));
                cmd.Parameters.Add(new("@Remark", SqlDbType.VarChar, 200));
                cmd.Parameters.Add(new("@ExCore", SqlDbType.VarChar, 50));
                cmd.Parameters.Add(new("@DepthID", SqlDbType.VarChar, 50));
                cmd.Parameters.Add(new("@Part_No", SqlDbType.VarChar, 50));
                cmd.Parameters.Add(new("@Part_Name", SqlDbType.VarChar, 100));
                cmd.Parameters.Add(new("@Job_No", SqlDbType.VarChar, 50));
                cmd.Parameters.Add(new("@Qty_Order", SqlDbType.VarChar));
                cmd.Parameters.Add(new("@QtyPCS", SqlDbType.VarChar));
                cmd.Parameters.Add(new("@Volume", SqlDbType.VarChar));
                cmd.Parameters.Add(new("@EntryDate", SqlDbType.VarChar, 20));
                cmd.Parameters.Add(new("@UserLogin", SqlDbType.VarChar, 50));

                foreach (var parts in so.SOParts)
                {
                    cmd.Parameters["@SO_No"].Value = idStr;
                    cmd.Parameters["@LineOrderCode"].Value = so.Line;
                    cmd.Parameters["@SupplierCode"].Value = so.Supplier;
                    cmd.Parameters["@ArrivalDate"].Value = so.ArrivalDate;
                    cmd.Parameters["@RouteCode"].Value = so.Route;
                    cmd.Parameters["@CycleIssue"].Value = so.Cycle;
                    cmd.Parameters["@Volumetric"].Value = so.Volumetric;
                    cmd.Parameters["@TotalPartVolume"].Value = so.TotalPartVolume;
                    cmd.Parameters["@Remark"].Value = so.Remarks;
                    cmd.Parameters["@ExCore"].Value = parts.ExCore;
                    cmd.Parameters["@DepthID"].Value = parts.DepthID;
                    cmd.Parameters["@Part_No"].Value = parts.Part_No;
                    cmd.Parameters["@Part_Name"].Value = parts.Part_Name;
                    cmd.Parameters["@Job_No"].Value = parts.Job_No;
                    cmd.Parameters["@Qty_Order"].Value = parts.Qty_Order;
                    cmd.Parameters["@QtyPCS"].Value = parts.QtyPCS;
                    cmd.Parameters["@Volume"].Value = parts.Volume;
                    cmd.Parameters["@EntryDate"].Value = now;
                    cmd.Parameters["@UserLogin"].Value = so.UserLogin;

                    cmd.ExecuteNonQuery();
                    remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);

                    if (remarks.Substring(0, 7) != "success")
                    {
                        conn.Close();
                        return BadRequest(remarks);
                    }

                    idStr = remarks.Substring(8);
                }

                conn.Close();
            }
            if (remarks.Substring(0, 7) != "success") return BadRequest(remarks.Substring(6));
            else return Ok("success");
        }

        [HttpPost]
        public ActionResult<IEnumerable<Dictionary<string, object>>> INQ()
        {
            var dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Inq_T_SO", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
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
