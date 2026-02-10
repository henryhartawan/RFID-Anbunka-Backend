namespace RFIDP2P3_API.Models
{
	public class MasterPartOrder
    {
		public string? IUType { get; set; }
        public string? PartOrderID { get; set; }
        public string? PlantCode { get; set; }
        public string? SupplierCode { get; set; }
        public string? Supplier { get; set; }
        public string? PartNumber { get; set; }
        public string? PartName { get; set; }
        public string? JobNo { get; set; }
        public string? QtyPerBox { get; set; }
        public string? UOM { get; set; }
        public string? PartType { get; set; }
        public string? BoxType { get; set; }
        public string? WeightGross { get; set; }
        public string? AllowanceDeviasi { get; set; }
        public string? QtyBoxPerM3 { get; set; }
        public string? RatioGrade { get; set; }
        public string? PartOrderStatus { get; set; }
		public string? UserLogin { get; set; }
		public string? LastUpdate { get; set; }
		public string? UserUpdate { get; set; }
		public string? Remarks { get; set; }
    }
}
