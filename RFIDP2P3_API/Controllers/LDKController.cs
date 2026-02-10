using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data.SqlClient;
using System.Data;
using System.Net.NetworkInformation;
using OfficeOpenXml;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LDKController : Controller
    {
        private readonly string _configuration;
        private string? remarks = "";
        private string? LDKNo = "";

        public LDKController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<LDK>> INQ(LDK paramObj)
        {
            List<LDK> ContainerObj = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_LDK_Sel", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@LDK_No", paramObj.LDK_No));
                cmd.Parameters.Add(new("@Type_NG", paramObj.NGTypeID));
                cmd.Parameters.Add(new("@TglKejadian", paramObj.Waktu_Kejadian));
                cmd.Parameters.Add(new("@UserLogin", paramObj.UserLogin));
                conn.Open();
                SqlDataReader sdr = cmd.ExecuteReader();

                while (sdr.Read())
                {
                    ContainerObj.Add(new LDK
                    {
                        LDK_No = sdr["LDK_No"].ToString(),
                        NGTypeID = sdr["NGTypeID"].ToString(),
                        NGType = sdr["NGType"].ToString(),
                        ShopId = sdr["ShopId"].ToString(),
                        ShopName = sdr["ShopName"].ToString(),
                        DeptId = sdr["DeptId"].ToString(),
                        DeptName = sdr["DeptName"].ToString(),
                        Waktu_Kejadian = sdr["Waktu_Kejadian"].ToString(),
                        Kejadian = sdr["Kejadian"].ToString(),
                        Shift = sdr["Shift"].ToString(),
                        Reason = sdr["Reason"].ToString(),
                        Status_Approval = sdr["Status_Approval"].ToString(),
                        RFIDNo = sdr["RFIDNo"].ToString(),
                        ApprovalAt = sdr["ApprovalAt"].ToString(),
                        EditButton = sdr["EditButton"].ToString(),
                        ApproveButton = sdr["ApproveButton"].ToString(),
                        DownloadButton = sdr["DownloadButton"].ToString(),
                        Entry_User = sdr["Entry_User"].ToString(),
                        Entry_Date = sdr["Entry_Date"].ToString(),
                        Approve1_By_ID = sdr["Approve1_By_ID"].ToString(),
                        Approve1_Date = sdr["Approve1_Date"].ToString(),
                        Approve2_By_ID = sdr["Approve2_By_ID"].ToString(),
                        Approve2_Date = sdr["Approve2_Date"].ToString(),
                        Approve3_By_ID = sdr["Approve3_By_ID"].ToString(),
                        Approve3_Date = sdr["Approve3_Date"].ToString(),
                        Approve4_By_ID = sdr["Approve4_By_ID"].ToString(),
                        Approve4_Date = sdr["Approve4_Date"].ToString()
                    });
                }
                conn.Close();
            }
            return ContainerObj;
        }

        [HttpPost]
        public ActionResult<IEnumerable<LDK>> INS(LDK paramObj)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_LDK_Ins", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                conn.Open();

                cmd.Parameters.Add(new("@NGTypeID", paramObj.NGTypeID));
                cmd.Parameters.Add(new("@ShopId", paramObj.ShopId));
                cmd.Parameters.Add(new("@DeptId", paramObj.DeptId));
                cmd.Parameters.Add(new("@PartNumber", SqlDbType.VarChar, 20));
                cmd.Parameters.Add(new("@Qty", SqlDbType.Int));
                cmd.Parameters.Add(new("@Waktu_Kejadian", paramObj.Waktu_Kejadian));
                cmd.Parameters.Add(new("@Shift", paramObj.Shift));
                cmd.Parameters.Add(new("@Reason", paramObj.Reason));
                cmd.Parameters.Add(new("@UserLogin", paramObj.UserLogin));

                if (paramObj.LDK_No == "")
                {
                    SqlCommand cmd1 = new("exec sp_M_LDK_Sel_LDKNo @ShopId", conn);
                    cmd1.CommandType = CommandType.Text;
                    cmd1.Parameters.Add(new("@ShopId", paramObj.ShopId));
                    cmd1.ExecuteNonQuery();
                    LDKNo = cmd1.ExecuteScalar().ToString();
                }
                else
                {
                    LDKNo = paramObj.LDK_No;

                    SqlCommand cmd2 = new("DELETE M_LDK WHERE LDK_No = @LDKNo", conn);
                    cmd2.CommandType = CommandType.Text;
                    cmd2.Parameters.Add(new("@LDKNo", LDKNo));
                    cmd2.ExecuteNonQuery();
                }

                cmd.Parameters.Add(new("@LDK_No", LDKNo));

                foreach (var part in paramObj.Parts)
                {
                    cmd.Parameters["@PartNumber"].Value = part.PartNumber;
                    cmd.Parameters["@Qty"].Value = part.QtyProdPerDay;

                    cmd.ExecuteNonQuery();
                    remarks = cmd.ExecuteScalar().ToString();

                    if (remarks != "")
                    {
                        conn.Close();
                        return BadRequest(remarks);
                    }
                }
                conn.Close();
            }
            if (remarks != "") return BadRequest(remarks);
            else return Ok("success");
        }

        [HttpPost]
        public ActionResult<IEnumerable<LDK>> DEL(LDK paramObj)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_LDK_Del", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@LDK_No", paramObj.LDK_No));

                conn.Open();
                cmd.ExecuteNonQuery();
                remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
                conn.Close();
            }
            if (remarks != "") return BadRequest(remarks);
            else return Ok("success");
        }

        [HttpPost]
        public ActionResult<IEnumerable<LDK>> INQp(LDK paramObj)
        {
            List<LDK> ContainerObj = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_LDK_Sel_Part", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@LDK_No", paramObj.LDK_No));
                conn.Open();
                SqlDataReader sdr = cmd.ExecuteReader();

                while (sdr.Read())
                {
                    ContainerObj.Add(new LDK
                    {
                        PartNumber = sdr["PartNumber"].ToString(),
                        Qty = sdr["Qty"].ToString()
                    });
                }
                conn.Close();
            }
            return ContainerObj;
        }

        [HttpPost]
        public ActionResult<IEnumerable<LDK>> INQd(LDK paramObj)
        {
            List<LDK> ContainerObj = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_LDK_Sel_Detail", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@LDK_No", paramObj.LDK_No));
                conn.Open();
                SqlDataReader sdr = cmd.ExecuteReader();

                while (sdr.Read())
                {
                    ContainerObj.Add(new LDK
                    {
                        LDK_No = sdr["LDK_No"].ToString(),
                        PartNumber = sdr["PartNumber"].ToString(),
                        PartName = sdr["PartName"].ToString(),
                        PartDescription = sdr["PartDescription"].ToString(),
                        Qty = sdr["Qty"].ToString()
                    });
                }
                conn.Close();
            }
            return ContainerObj;
        }

        [HttpPost]
        public ActionResult<IEnumerable<LDK>> App(LDK paramObj)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_LDK_App", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@LDK_No", paramObj.LDK_No));
                cmd.Parameters.Add(new("@Approval", paramObj.Approval));
                cmd.Parameters.Add(new("@ApprovalAt", paramObj.ApprovalAt));
                cmd.Parameters.Add(new("@UserLogin", paramObj.UserLogin));

                conn.Open();
                cmd.ExecuteNonQuery();
                remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
                conn.Close();
            }
            if (remarks != "") return BadRequest(remarks);
            else return Ok("success");
		}
		[HttpPost]
		public ActionResult<IEnumerable<LDK>> INQn(LDK paramObj)
		{
			List<LDK> ContainerObj = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_LDK_Sel_Notif", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add(new("@UserLogin", paramObj.UserLogin));
				conn.Open();
				SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
				{
					ContainerObj.Add(new LDK
					{
						Qty = sdr["Qty"].ToString()
					});
				}
				conn.Close();
			}
			return ContainerObj;
		}
	}
}
