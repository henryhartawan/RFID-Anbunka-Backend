using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data;
using System.Data.SqlClient;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MasterPrivilegeController : ControllerBase
    {
        private readonly string _configuration;
        private string? remarks = "";

        public MasterPrivilegeController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<Privilege>> INQ(MasterPrivilege privilege)
        {
            List<Privilege> privileges = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_UserPreviledge_Sel", conn))
			{
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@UserGroupId", privilege.UserGroup_Id));

                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    privileges.Add(new Privilege
                    {
						MenuGroup_Id = sdr["MenuGroupID"].ToString(),
						MenuGroup_Name = sdr["MenuGroupName"].ToString(),
						Menu_Id = sdr["MenuId"].ToString(),
                        Menu_Name = sdr["MenuName"].ToString(),
                        MenuDescription = sdr["MenuDescription"].ToString(),
                        checkedbox_read = sdr["checkedbox_read"].ToString(),
                        checkedbox_add = sdr["checkedbox_add"].ToString(),
                        checkedbox_edit = sdr["checkedbox_edit"].ToString(),
                        checkedbox_del = sdr["checkedbox_del"].ToString()
                    });
                }
                conn.Close();
            }
            return privileges;
        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterPrivilege>> INS(MasterPrivilege privileges)
        {
			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_UserPreviledge_Del", conn))
			{
                
                cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
				
                cmd.Parameters.Add(new("@UserGroupId", privileges.UserGroup_Id));

				conn.Open();
				cmd.ExecuteNonQuery();
				remarks = Convert.ToString(cmd.Parameters["@Remarks"].Value);
				
                if (remarks == "")
                {
                    using (SqlCommand cmd1 = new SqlCommand("sp_UserPreviledge_Ins", conn))
                    {
                        cmd1.CommandType = CommandType.StoredProcedure;
                        cmd1.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

						cmd1.Parameters.Add(new("@UserGroupId", SqlDbType.VarChar, 3));
						cmd1.Parameters.Add(new("@MenuId", SqlDbType.VarChar, 3));
						cmd1.Parameters.Add(new("@Read", SqlDbType.Bit));
						cmd1.Parameters.Add(new("@Add", SqlDbType.Bit));
						cmd1.Parameters.Add(new("@Edit", SqlDbType.Bit));
						cmd1.Parameters.Add(new("@Delete", SqlDbType.Bit));
						cmd1.Parameters.Add(new("@UserLogin", SqlDbType.VarChar, 50));

						foreach (var privilege in privileges.Privileges)
						{
       //                     privilege.checkedbox_read = privilege.checkedbox_read == ""  ? "0" : privilege.checkedbox_read == null? "0": privilege.checkedbox_read; 
       //                     privilege.checkedbox_add = privilege.checkedbox_add =="" ? "0" : privilege.checkedbox_add == null ? "0" : privilege.checkedbox_add;
							//privilege.checkedbox_edit = privilege.checkedbox_edit =="" ? "0" : privilege.checkedbox_edit == null ? "0" : privilege.checkedbox_edit;
							//privilege.checkedbox_del = privilege.checkedbox_del =="" ? "0" : privilege.checkedbox_del == null ? "0" : privilege.checkedbox_del;


							cmd1.Parameters["@UserGroupId"].Value = privileges.UserGroup_Id;
							cmd1.Parameters["@MenuId"].Value = privilege.Menu_Id;
							cmd1.Parameters["@Read"].Value = Convert.ToBoolean(Convert.ToInt32(privilege.checkedbox_read));
							cmd1.Parameters["@Add"].Value = Convert.ToBoolean(Convert.ToInt32(privilege.checkedbox_add));
							cmd1.Parameters["@Edit"].Value = Convert.ToBoolean(Convert.ToInt32(privilege.checkedbox_edit));
							cmd1.Parameters["@Delete"].Value = Convert.ToBoolean(Convert.ToInt32(privilege.checkedbox_del));
							cmd1.Parameters["@UserLogin"].Value = Convert.ToBoolean(Convert.ToInt32(privileges.UserLogin));

							cmd1.ExecuteNonQuery();
							remarks = Convert.ToString(cmd1.Parameters["@Remarks"].Value);

							if (remarks != "")
							{
								conn.Close();
								return BadRequest(remarks);
							}
						}
					}
                    conn.Close();
                    return Ok(remarks);
                }
                else
                {
                    conn.Close();
                    return BadRequest(remarks);
                }
            }
        }
    }
}
