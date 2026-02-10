using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data.SqlClient;
using System.Data;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LoginController : Controller
    {
        private readonly string _configuration;
        private string? remarks = "";

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public ActionResult<IEnumerable<MasterUser>> Index(MasterUser Login)
        {
            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_UserLogin_Sel", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@UserId", Login.PIC_ID));

                conn.Open();
                SqlDataReader sdr = cmd.ExecuteReader();

                string? PIC_ID = "";
                string? PIC_Name = "";
                string? Pwd = "";
				string? UserGroup_Id = "";
				string? UserGroup_Name = "";
				string? PlantId = "";

                List<User> userLogin = new();

                while (sdr.Read())
                {
                    Pwd = sdr["Passwords"].ToString();
                    PIC_ID = sdr["UserID"].ToString();
                    PIC_Name = sdr["UserName"].ToString();
					UserGroup_Id = sdr["UserGroupID"].ToString();
					UserGroup_Name = sdr["UserGroupName"].ToString();
					PlantId = sdr["PlantId"].ToString();
                }

                if (!sdr.HasRows)
                {
                    sdr.Close();
                    conn.Close();
                    return BadRequest("User not found/not active");
                }
                else if (!BCrypt.Net.BCrypt.Verify(Login.password, Pwd))
                {
                    sdr.Close();
                    conn.Close();
                    return BadRequest("Incorrect login/password");
                }
                else
                {
                    sdr.Close();

                    List<Privilege> privileges = new();
                    using (SqlCommand cmd1 = new SqlCommand("sp_UserAccess_Sel", conn))
                    {
                        cmd1.CommandType = CommandType.StoredProcedure;
                        cmd1.Parameters.Add(new("@UserId", Login.PIC_ID));
                        SqlDataReader sdr1 = cmd1.ExecuteReader();
                        if (sdr1.FieldCount > 0)
                        {
                            while (sdr1.Read())
                            {
                                privileges.Add(new Privilege
                                {
                                    Menu_Id = sdr1["MenuName"].ToString(),
                                    checkedbox_read = sdr1["AllowAccess"].ToString(),
                                    checkedbox_add = sdr1["AllowSubmit"].ToString(),
                                    checkedbox_edit = sdr1["AllowUpdate"].ToString(),
                                    checkedbox_del = sdr1["AllowDelete"].ToString()
                                });
                            }
                            userLogin.Add(new User
                            {
                                PIC_ID = PIC_ID,
                                PIC_Name = PIC_Name,
								UserGroup_Id = UserGroup_Id,
								UserGroup_Name = UserGroup_Name,
								PlantId = PlantId,
                                Privileges = privileges
                            });
                        }
                        sdr1.Close();
                        conn.Close();
                    }
                   
                    //List<User> userLogin = new();
                    //using (SqlCommand cmd2 = new SqlCommand("sp_UserLogin_Sel", conn))
                    //{
                    //    cmd2.Parameters.Add(new("@PIC_ID", Userlogin));
                    //    sdr = cmd2.ExecuteReader();
                    //    while (sdr.Read())
                    //    {
                    //        userLogin.Add(new User
                    //        {
                    //            PIC_ID = sdr["UserID"].ToString(),
                    //            PIC_Name = sdr["PIC_Name"].ToString(),
                    //            UserGroup_Id = sdr["UserGroupID"].ToString(),
                    //            Privileges = privileges
                    //        });
                    //    }
                    //    conn.Close();
                    //}

                    return Ok(userLogin);
                }
            }
        }

        //[HttpPost]
        //public ActionResult Logout(MasterUser logout)
        //{
        //    using (SqlConnection conn = new(_configuration))
        //    {
        //        conn.Open();
        //        SqlCommand cmd = new("EXEC sp_Submit_T_User_Logout @PIC_ID", conn);
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        cmd.Parameters.Add(new("@PIC_ID", logout.PIC_ID));
        //        remarks = cmd.ExecuteScalar().ToString();
        //        conn.Close();
        //    }
        //    if (remarks != "success") return BadRequest(remarks);
        //    else return Ok(remarks);
        //}
    }
}
