namespace RFIDP2P3_API.Models
{
    public class Privilege
    {
        public string? MenuGroup_Id { get; set; }
		public string? MenuGroup_Name { get; set; }
		public string? Menu_Id { get; set; }
        public string? Menu_Name { get; set; }
        public string? MenuDescription { get; set; }
        public string? checkedbox_read { get; set; }
        public string? checkedbox_add { get; set; }
        public string? checkedbox_edit { get; set; }
        public string? checkedbox_del { get; set; }
    }
}
