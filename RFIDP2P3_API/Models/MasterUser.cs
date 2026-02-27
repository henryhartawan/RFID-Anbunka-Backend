using static System.Collections.Specialized.BitVector32;

namespace RFIDP2P3_API.Models
{
    public class MasterUser
    {
		public string? IUType { get; set; }
		public string? PIC_ID { get; set; }
        public string? password { get; set; }
        public string? UserGroup_Id { get; set; }
        public string? PIC_Name { get; set; }
        public string? UserGroup { get; set; }
        public string? UserLogin { get; set; }
		public string? PlantID { get; set; }
		public string? DeptId { get; set; }
		public string? SectionId { get; set; }
		public string? BuildingId { get; set; }
		public string? PlantName { get; set; }
		public string? DeptName { get; set; }
		public string? SectionName { get; set; }
		public string? BuildingName { get; set; }
        public string? SupplierCode { get; set; }
        public string? Supplier { get; set; }
    }
}
