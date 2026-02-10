using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data.SqlClient;
using System.Data;
using System;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MobileAppController : Controller
	{
        private readonly string _configuration;
        private string? remarks = "";

        public MobileAppController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public ActionResult<IEnumerable<PalletScan>> RFIDScan(string? ScanType, string? RFIDNo, string? CancelNotes, string? User)
        {
            List<PalletScan> ContainerObj = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_T_RFID_Sel", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new("@ScanType", ScanType));
				cmd.Parameters.Add(new("@RFIDNos", RFIDNo));
				cmd.Parameters.Add(new("@CancelNotes", CancelNotes));
				cmd.Parameters.Add(new("@UserLogin", User));

				conn.Open();
                SqlDataReader sdr = cmd.ExecuteReader();

                while (sdr.Read())
                {
                    ContainerObj.Add(new PalletScan
                    {
						QtySuccess = sdr["QtySuccess"].ToString(),
						QtyFailed = sdr["QtyFailed"].ToString()
					});
                }
                conn.Close();
            }
            return ContainerObj;
        }

        [HttpGet]
        public ActionResult<IEnumerable<BufferStockScan>> PartScan(string? RFIDNo, string? QRType, string? PartNumber, string? Qty)
        {
            List<BufferStockScan> ContainerObj = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_T_BufferStock_Sel", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add(new("@RFIDNo", RFIDNo));
				cmd.Parameters.Add(new("@QRType", QRType));
                cmd.Parameters.Add(new("@PartNumber", PartNumber));
                cmd.Parameters.Add(new("@Qty", Qty));

                conn.Open();
                SqlDataReader sdr = cmd.ExecuteReader();

                while (sdr.Read())
                {
                    ContainerObj.Add(new BufferStockScan
					{
						RFIDNo = sdr["RFIDNo"].ToString(),
						PartNo = sdr["PartNo"].ToString(),
						PartDesc = sdr["PartDesc"].ToString(),
						PalletType = sdr["PalletType"].ToString(),
						Remarks = sdr["Remarks"].ToString()
                    });
                }
                conn.Close();
            }
            return ContainerObj;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PreDeliveryScan>> KanbanScan(string? RFIDNo, string? KanbanNo)
        {
            List<PreDeliveryScan> ContainerObj = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_T_PreDelivery_Sel", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add(new("@RFIDNo", RFIDNo));
				cmd.Parameters.Add(new("@KanbanNo", KanbanNo));

				conn.Open();
                SqlDataReader sdr = cmd.ExecuteReader();

                while (sdr.Read())
                {
                    ContainerObj.Add(new PreDeliveryScan
					{
						RFIDNo = sdr["RFIDNo"].ToString(),
						PartNo = sdr["PartNo"].ToString(),
						PartDesc = sdr["PartDesc"].ToString(),
						PartQty = sdr["PartQty"].ToString(),
						PalletType = sdr["PalletType"].ToString(),
						KanbanNo = sdr["KanbanNo"].ToString(),
						CycleNo = sdr["CycleNo"].ToString(),
						KanbanDate = sdr["KanbanDate"].ToString(),
                        Remarks = sdr["Remarks"].ToString()
					});
                }
                conn.Close();
            }
            return ContainerObj;
        }

		[HttpGet]
		public ActionResult<IEnumerable<PostingScan>> PostingScan(string? RFIDNo)
		{
			List<PostingScan> ContainerObj = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_T_Posting_Sel", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;

				cmd.Parameters.Add(new("@RFIDNo", RFIDNo));

				conn.Open();
				SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
				{
					ContainerObj.Add(new PostingScan
					{
						RFIDNo = sdr["RFIDNo"].ToString(),
						PartNo = sdr["PartNo"].ToString(),
						PartDesc = sdr["PartDesc"].ToString(),
						PartQty = sdr["PartQty"].ToString(),
						PalletType = sdr["PalletType"].ToString(),
						KanbanNo = sdr["KanbanNo"].ToString(),
						CycleNo = sdr["CycleNo"].ToString(),
						KanbanDate = sdr["KanbanDate"].ToString(),
						Remarks = sdr["Remarks"].ToString()
					});
				}
				conn.Close();
			}
			return ContainerObj;
		}

        [HttpGet]
        public ActionResult<IEnumerable<LDKScan>> LDKScan(string? RFIDNo, string? LDKNos)
        {
            List<LDKScan> ContainerObj = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_T_LDK_Sel", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(new("@RFIDNo", RFIDNo));
                cmd.Parameters.Add(new("@LDKNos", LDKNos));

                conn.Open();
                SqlDataReader sdr = cmd.ExecuteReader();

                while (sdr.Read())
                {
                    ContainerObj.Add(new LDKScan
                    {
                        RFIDNo = sdr["RFIDNo"].ToString(),
                        LDKNo = sdr["LDKNo"].ToString(),
                        NGType = sdr["NGType"].ToString(),
                        Remarks = sdr["Remarks"].ToString()
                    });
                }
                conn.Close();
            }
            return ContainerObj;
        }

		[HttpGet]
        public ActionResult<IEnumerable<RemarksNote>> BufferStockSubmit(string? ScanType, string? RFIDNo, string? QRType, string? PartNo, string? Qty, string? CancelNotes, string? User)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_T_BufferStock_Ins", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@ScanType", ScanType));
                cmd.Parameters.Add(new("@RFIDNo", RFIDNo));
				cmd.Parameters.Add(new("@QRType", QRType));
				cmd.Parameters.Add(new("@PartNumber", PartNo));
				cmd.Parameters.Add(new("@Qty", Qty));
				cmd.Parameters.Add(new("@CancelNotes", CancelNotes));
				cmd.Parameters.Add(new("@UserLogin", User));

				conn.Open();
                cmd.ExecuteNonQuery();
                remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
                conn.Close();
            }
            if (remarks == "") remarks = "success";

            List<RemarksNote> Rems = new();
            Rems.Add(new RemarksNote
            {
                Remarks = remarks,
            });

            return Rems;
        }

        [HttpGet]
        public ActionResult<IEnumerable<RemarksNote>> PreDeliverySubmit(string? ScanType, string? RFIDNo, string? KanbanNo, string? CancelNotes, string? User)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_T_PreDelivery_Ins", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@ScanType", ScanType));
                cmd.Parameters.Add(new("@RFIDNo", RFIDNo));
                cmd.Parameters.Add(new("@KanbanNo", KanbanNo));
                cmd.Parameters.Add(new("@CancelNotes", CancelNotes));
				cmd.Parameters.Add(new("@UserLogin", User));

				conn.Open();
                cmd.ExecuteNonQuery();
                remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
                conn.Close();
            }
            if (remarks == "") remarks = "success";

            List<RemarksNote> Rems = new();
            Rems.Add(new RemarksNote
            {
                Remarks = remarks,
            });

            return Rems;
        }

		[HttpGet]
		public ActionResult<IEnumerable<RemarksNote>> PostingSubmit(string? ScanType, string? RFIDNo, string? CancelNotes, string? User)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_T_Posting_Ins", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@ScanType", ScanType));
				cmd.Parameters.Add(new("@RFIDNo", RFIDNo));
				cmd.Parameters.Add(new("@CancelNotes", CancelNotes));
				cmd.Parameters.Add(new("@UserLogin", User));

				conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks == "") remarks = "success";

			List<RemarksNote> Rems = new();
			Rems.Add(new RemarksNote
			{
				Remarks = remarks,
			});

			return Rems;
		}

        [HttpGet]
        public ActionResult<IEnumerable<RemarksNote>> LDKSubmit(string? ScanType, string? RFIDNo, string? LDKNos, string? CancelNotes, string? User)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_T_LDK_Ins", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@ScanType", ScanType));
                cmd.Parameters.Add(new("@RFIDNo", RFIDNo));
                cmd.Parameters.Add(new("@LDKNo", LDKNos));
                cmd.Parameters.Add(new("@CancelNotes", CancelNotes));
				cmd.Parameters.Add(new("@UserLogin", User));

				conn.Open();
                cmd.ExecuteNonQuery();
                remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
                conn.Close();
            }
            if (remarks == "") remarks = "success";

            List<RemarksNote> Rems = new();
            Rems.Add(new RemarksNote
            {
                Remarks = remarks,
            });

            return Rems;
        }
        [HttpGet]
        public ActionResult<IEnumerable<AppVer>> AppVer()
        {
            List<AppVer> ContainerObj = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM M_AppVer", conn))
            {
                cmd.CommandType = CommandType.Text;

                conn.Open();
                SqlDataReader sdr = cmd.ExecuteReader();

                while (sdr.Read())
                {
                    ContainerObj.Add(new AppVer
                    {
                        App_Ver = sdr["App_Ver"].ToString(),
                        Status = sdr["Status"].ToString()
                    });
                }
                conn.Close();
            }
            return ContainerObj;
        }
    }
}
