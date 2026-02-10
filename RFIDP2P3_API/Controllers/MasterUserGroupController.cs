using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MasterUserGroupController : ControllerBase
    {
        private readonly string _configuration;
        private string? remarks = "";

        public MasterUserGroupController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterUserGroup>> INQ(MasterUserGroup usergroup)
        {
            List<MasterUserGroup> UserGroups = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_UserGroup_Sel", conn))
			{
                cmd.CommandType = CommandType.StoredProcedure;
				conn.Open();
				SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
                {
                    UserGroups.Add(new MasterUserGroup
                    {
                        UserGroup_Id = sdr["UserGroupId"].ToString(),
                        UserGroup = sdr["UserGroupName"].ToString()
                    });
                }
                conn.Close();
            }
            return UserGroups;
        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterUserGroup>> DEL(MasterUserGroup usergroup)
        {
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_UserGroup_Del", conn))
			{
                cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
				
                cmd.Parameters.Add(new("@UserGroupId", usergroup.UserGroup_Id));

				conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				conn.Close();
			}
            if (remarks != "") return BadRequest(remarks);
            else return Ok("success");
		}

        [HttpPost]
        public ActionResult<IEnumerable<MasterUserGroup>> INS(MasterUserGroup usergroup)
        {
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_UserGroup_Ins", conn))
			{
                cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

				cmd.Parameters.Add(new("@IUType", usergroup.IUType));
				cmd.Parameters.Add(new("@UserGroupID", usergroup.UserGroup_Id));
                cmd.Parameters.Add(new("@UserGroupName", usergroup.UserGroup));
                cmd.Parameters.Add(new("@UserLogin", usergroup.UserLogin));

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
