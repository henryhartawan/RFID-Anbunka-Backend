using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data.SqlClient;
using System.Data;

namespace RFIDP2P3_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class MasterFinishGoodsController : Controller
	{
		private readonly string _configuration;
		private string? remarks = "";

		public MasterFinishGoodsController(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterFinishGoods>> INQ()
		{
			List<MasterFinishGoods> FinishGoods = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Finish_Goods_Sel", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				conn.Open();
				SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
				{
                    FinishGoods.Add(new MasterFinishGoods
                    {
                        UniqueNumber = sdr["UniqueNumber"].ToString(),
                        PartNumber = sdr["PartNumber"].ToString(),
                        PartName = sdr["PartName"].ToString(),
                        QtyPerBox = sdr["QtyPerBox"].ToString(),
                        UOM = sdr["UOM"].ToString(),
                        BoxType = sdr["BoxType"].ToString(),
                        WeightGross = sdr["WeightGross"].ToString(),
                        FinishGoodsStatus = sdr["FinishGoodsStatus"].ToString(),
						LastUpdate = sdr["DateUpdate"].ToString(),
						UserUpdate = sdr["UserUpdate"].ToString(),
						Remarks = sdr["Remarks"].ToString()
                    });
				}
				conn.Close();
			}
			return FinishGoods;
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterFinishGoods>> INS(MasterFinishGoods finishGoods)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Finish_Goods_Ins", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@IUType", finishGoods.IUType));
				cmd.Parameters.Add(new("@UniqueNumber", finishGoods.UniqueNumber));
                cmd.Parameters.Add(new("@PartNumber", finishGoods.PartNumber));
                cmd.Parameters.Add(new("@PartName", finishGoods.PartName));
                cmd.Parameters.Add(new("@QtyPerBox", finishGoods.QtyPerBox));
                cmd.Parameters.Add(new("@UOM", finishGoods.UOM));
                cmd.Parameters.Add(new("@BoxType", finishGoods.BoxType));
                cmd.Parameters.Add(new("@WeightGross", finishGoods.WeightGross));
                cmd.Parameters.Add(new("@UserLogin", finishGoods.UserLogin));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
		}

		[HttpPost]
		public ActionResult<IEnumerable<MasterFinishGoods>> DEL(MasterFinishGoods finishGoods)
		{
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_M_Finish_Goods_Del", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@UniqueNumber", finishGoods.UniqueNumber));

                conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
			if (remarks != "") return BadRequest(remarks);
			else return Ok("success");
        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterFinishGoods>> ACT(MasterFinishGoods finishGoods)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_M_Finish_Goods_Act", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                cmd.Parameters.Add(new("@UniqueNumber", finishGoods.UniqueNumber));
                cmd.Parameters.Add(new("@UserLogin", finishGoods.UserLogin));

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
