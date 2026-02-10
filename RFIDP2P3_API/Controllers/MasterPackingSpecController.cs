using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class MasterPackingSpecController : Controller
	{
		private readonly string _configuration;
		private string? remarks = "";

		public MasterPackingSpecController(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterPackingSpec>> INQ()
		{
			List<MasterPackingSpec> PackingSpec = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Packing_Spec_Sel", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				conn.Open();
				SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
				{
                    PackingSpec.Add(new MasterPackingSpec
                    {
						BoxType = sdr["BoxType"].ToString(),
						BoxWidth = sdr["BoxWidth"].ToString(),
                        BoxLength = sdr["BoxLength"].ToString(),
                        BoxHeight = sdr["BoxHeight"].ToString(),
                        BoxVolume = sdr["BoxVolume"].ToString(),
                        BoxWeight = sdr["BoxWeight"].ToString(),
                        QtyLayer = sdr["QtyLayer"].ToString(),
                        Stacking = sdr["Stacking"].ToString(),
                        SkidWidth = sdr["SkidWidth"].ToString(),
                        SkidLength = sdr["SkidLength"].ToString(),
                        SkidHeight = sdr["SkidHeight"].ToString(),
                        SkidVolume = sdr["SkidVolume"].ToString(),
                        PackingStatus = sdr["PackingStatus"].ToString(),
                        LastUpdate = sdr["DateUpdate"].ToString(),
						UserUpdate = sdr["UserUpdate"].ToString(),
						Remarks = sdr["Remarks"].ToString()
                    });
				}
				conn.Close();
			}
			return PackingSpec;
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterPackingSpec>> INS(MasterPackingSpec packingSpec)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Packing_Spec_Ins", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@IUType", packingSpec.IUType));
                cmd.Parameters.Add(new("@BoxType", packingSpec.BoxType));
				cmd.Parameters.Add(new("@BoxWidth", packingSpec.BoxWidth));
				cmd.Parameters.Add(new("@BoxLength", packingSpec.BoxLength));
                cmd.Parameters.Add(new("@BoxHeight", packingSpec.BoxHeight));
                cmd.Parameters.Add(new("@BoxVolume", packingSpec.BoxVolume));
                cmd.Parameters.Add(new("@BoxWeight", packingSpec.BoxWeight));
                cmd.Parameters.Add(new("@QtyLayer", packingSpec.QtyLayer));
                cmd.Parameters.Add(new("@Stacking", packingSpec.Stacking));
                cmd.Parameters.Add(new("@SkidWidth", packingSpec.SkidWidth));
                cmd.Parameters.Add(new("@SkidLength", packingSpec.SkidLength));
                cmd.Parameters.Add(new("@SkidHeight", packingSpec.SkidHeight));
                cmd.Parameters.Add(new("@SkidVolume", packingSpec.SkidVolume));
                cmd.Parameters.Add(new("@UserLogin", packingSpec.UserLogin));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterPackingSpec>> DEL(MasterPackingSpec packingSpec)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Packing_Spec_Del", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@BoxType", packingSpec.BoxType));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterPackingSpec>> ACT(MasterPackingSpec packingSpec)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_Packing_Spec_Act", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@BoxType", packingSpec.BoxType));
                cmd.Parameters.Add(new("@UserLogin", packingSpec.UserLogin));

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
                            SqlCommand cmd = new("exec sp_M_Packing_Spec_Upload @EntryUser", conn);
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
