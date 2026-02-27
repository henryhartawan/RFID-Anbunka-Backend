using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;
using static System.Collections.Specialized.BitVector32;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MasterUserController : ControllerBase
    {
        private readonly string _configuration;
        private string? remarks = "";

        public MasterUserController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterUser>> INQ(MasterUser user)
        {
            List<MasterUser> users = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_UserSetup_Sel", conn))
			{
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@UserLogin", user.UserLogin));

				conn.Open();

				SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    users.Add(new MasterUser
                    {
                        PIC_ID = sdr["UserID"].ToString(),
                        PIC_Name = sdr["UserName"].ToString(),
                        UserGroup_Id = sdr["UserGroupId"].ToString(),
                        UserGroup = sdr["UserGroupName"].ToString(),
						PlantID = sdr["PlantId"].ToString(),
						DeptId = sdr["DeptId"].ToString(),
						SectionId = sdr["SectionID"].ToString(),
						BuildingId = sdr["BuildingId"].ToString(),
						PlantName = sdr["PlantName"].ToString(),
                        DeptName = sdr["DeptName"].ToString(),
                        SectionName = sdr["SectionName"].ToString(),
						BuildingName = sdr["BuildingName"].ToString(),
                        SupplierCode = sdr["SupplierCode"].ToString(),
                        Supplier = sdr["Supplier"].ToString(),
                        Email = sdr["Email"].ToString(),
                        RequireMfa = sdr["RequireMfa"] != DBNull.Value ? Convert.ToBoolean(sdr["RequireMfa"]) : (bool?)null
                    });
                }
                conn.Close();
            }
            return users;
        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterUser>> DEL(MasterUser user)
        {
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_UserSetup_Del", conn))
			{
                cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@UserId", user.PIC_ID));
                cmd.Parameters.Add(new("@UserLogin", user.UserLogin));

				conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
            if (remarks != "") return BadRequest(remarks);
            else return Ok("success");
		}

        [HttpPost]
        public ActionResult<IEnumerable<MasterUser>> INS(MasterUser user)
        {
			string passwordHash = "";
            if(user.password != "")
            {
				passwordHash = BCrypt.Net.BCrypt.HashPassword(user.password);
			}

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_UserSetup_Ins", conn))
			{
                cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@IUType", user.IUType));
				cmd.Parameters.Add(new("@UserId", user.PIC_ID));
                cmd.Parameters.Add(new("@UserName", user.PIC_Name));
                cmd.Parameters.Add(new("@Password", passwordHash));
                cmd.Parameters.Add(new("@Email", user.Email));
                cmd.Parameters.Add(new("@UserGroupId", user.UserGroup_Id));
				cmd.Parameters.Add(new("@PlantID", user.PlantID));
				cmd.Parameters.Add(new("@DeptId", user.DeptId));
				cmd.Parameters.Add(new("@SectionId", user.SectionId));
				cmd.Parameters.Add(new("@BuildingId", user.BuildingId));
                cmd.Parameters.Add(new("@SupplierCode", user.SupplierCode));
                cmd.Parameters.Add(new("@RequireMfa", user.RequireMfa ?? false));
                cmd.Parameters.Add(new("@UserLogin", user.UserLogin));

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
