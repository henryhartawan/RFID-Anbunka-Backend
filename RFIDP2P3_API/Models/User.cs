namespace RFIDP2P3_API.Models
{
    public class User
    {
        public string? PIC_ID { get; set; }
        public string? password { get; set; }
        public string? PIC_Name { get; set; }
		public string? UserGroup_Id { get; set; }
		public string? UserGroup_Name { get; set; }
		public string? PlantId { get; set; }
        public string? MFAStatus { get; set; }
        public List<Privilege>? Privileges { get; set; }
    }
}
