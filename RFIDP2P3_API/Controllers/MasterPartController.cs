using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data.SqlClient;
using System.Data;
using static IronOcr.OcrResult;
using System;
using OfficeOpenXml;

namespace RFIDP2P3_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class MasterPartController : Controller
	{
		private readonly string _configuration;
		private string? remarks = "";

		public MasterPartController(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterPart>> INQ()
		{
			List<MasterPart> ContainerObj = new();

			using (SqlConnection conn = new(_configuration))
			{
				conn.Open();
				SqlCommand cmd = new("exec sp_M_Partlist_Sel", conn);
				cmd.CommandType = CommandType.Text;

				SqlDataReader sdr = cmd.ExecuteReader();
				while (sdr.Read())
				{
					ContainerObj.Add(new MasterPart
					{
						PlantID =sdr["PlantID"].ToString(),
						PlantName =sdr["PlantName"].ToString(),
						BuildingID =sdr["BuildingID"].ToString(),
						BuildingName =sdr["BuildingName"].ToString(),
						PartNumber =sdr["PartNumber"].ToString(),
						PartName =sdr["PartName"].ToString(),
						PartDesc =sdr["PartDescription"].ToString(),
						CycleTimeMin =sdr["CycleTimeMin"].ToString(),
						CycleTimeSec =sdr["CycleTimeSec"].ToString(),
						Efficiency =sdr["Efficiency"].ToString(),
						LineID =sdr["LineID"].ToString(),
						LineName =sdr["LineName"].ToString(),
						QtyperPallet =sdr["QtyperPallet"].ToString(),
						QtyProdPerDay =sdr["QtyProdPerDay"].ToString(),
						PartStockMin =sdr["PartStockMin"].ToString(),
						PartStockMax =sdr["PartStockMax"].ToString(),
						PartStockDay =sdr["PartStockDay"].ToString(),
						CycleMax =sdr["CycleMax"].ToString(),
						LastUpdate =sdr["DateUpdate"].ToString(),
						UserUpdate =sdr["UserUpdate"].ToString(),
						Remarks  = sdr["Remarks"].ToString()
					});
				}
				conn.Close();
			}
			return ContainerObj;
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterPart>> INS(MasterPart paramObj)
		{
			using (SqlConnection conn = new(_configuration))
			{
				conn.Open();
				SqlCommand cmd = new("exec sp_M_Partlist_Ins @IUType ,@PartNumber ,@PartName ,@PartDesc ,@CycleTime ,@Efficiency ,@LineID ,@QtyperPallet ,@QtyProdPerDay ,@PartStockMin ,@PartStockMax ,@PartStockDay ,@CycleMax ,@UserLogin ", conn);
				cmd.CommandType = CommandType.Text;
				cmd.Parameters.Add(new("@IUType", paramObj.IUType));
				cmd.Parameters.Add(new("@PartNumber", paramObj.PartNumber));
				cmd.Parameters.Add(new("@PartName", paramObj.PartName));
				cmd.Parameters.Add(new("@PartDesc", paramObj.PartDesc));
				cmd.Parameters.Add(new("@CycleTime", paramObj.CycleTimeSec));
				cmd.Parameters.Add(new("@Efficiency", paramObj.Efficiency));
				cmd.Parameters.Add(new("@LineID", paramObj.LineID));
				cmd.Parameters.Add(new("@QtyperPallet", paramObj.QtyperPallet));
				cmd.Parameters.Add(new("@QtyProdPerDay", paramObj.QtyProdPerDay));
				cmd.Parameters.Add(new("@PartStockMin", paramObj.PartStockMin));
				cmd.Parameters.Add(new("@PartStockMax", paramObj.PartStockMax));
				cmd.Parameters.Add(new("@PartStockDay", paramObj.PartStockDay));
				cmd.Parameters.Add(new("@CycleMax", paramObj.CycleMax));
				cmd.Parameters.Add(new("@UserLogin", paramObj.UserLogin));
				remarks = cmd.ExecuteScalar().ToString();
				conn.Close();

			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterPart>> DEL(MasterPart paramObj)
		{
			using (SqlConnection conn = new(_configuration))
			{
				conn.Open();
				SqlCommand cmd = new("exec sp_M_Partlist_Del @PartNumber, @LineID", conn);
				cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add(new("@PartNumber", paramObj.PartNumber));
                cmd.Parameters.Add(new("@LineID", paramObj.LineID));
                remarks = cmd.ExecuteScalar().ToString();
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
                        using (SqlConnection conn = new(_configuration))
                        {
                            conn.Open();
                            SqlCommand cmd = new("exec sp_M_PartList_Upload @EntryUser", conn);
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.Add(new("@EntryUser", UID));
                            cmd.ExecuteScalar();
                            conn.Close();
                        }

                    }
                    list.Add(new RemarksNote { Remarks = remarks });
                    return list;
                }
            }
        }
    }
}
