using static IronOcr.OcrResult;

namespace RFIDP2P3_API.Models
{
	public class MasterApprovalLDK
	{
		public string? IUType { get; set; }
		public string? UserLogin { get; set; }
		public string? Ftype { get; set; }
		public string? AppLDKId  { get; set; }
		public string? ShopId  { get; set; }
		public string? ShopName  { get; set; }
		public string? DeptId  { get; set; }
		public string? DeptName  { get; set; }
        public string? Approval1_ID { get; set; }
        public string? Approval1_Name { get; set; }
        public string? Approval2_ID { get; set; }
        public string? Approval2_Name { get; set; }
        public string? Approval3_ID { get; set; }
        public string? Approval3_Name { get; set; }
        public string? Approval4_ID { get; set; }
        public string? Approval4_Name { get; set; }
        public string? UserUpdate  { get; set; }
		public string? DateUpdate { get; set; }
		public string? Remarks { get; set; }


	}
}
