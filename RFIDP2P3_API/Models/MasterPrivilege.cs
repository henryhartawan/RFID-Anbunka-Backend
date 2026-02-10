namespace RFIDP2P3_API.Models
{
    public class MasterPrivilege
    {
        public string? UserGroup_Id { get; set; }
		public string? UserLogin { get; set; }
		public List<Privilege>? Privileges { get; set; }
    }
}
