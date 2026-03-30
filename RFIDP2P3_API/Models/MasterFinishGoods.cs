namespace RFIDP2P3_API.Models
{
	public class MasterFinishGoods
    {
		public string? IUType { get; set; }
        public string? UniqueNumber { get; set; }
        public string? PartNumber { get; set; }
        public string? PartName { get; set; }
        public string? EngineType { get; set; }
        public string? EngineBase { get; set; }
        public string? Transmission { get; set; }
        public string? Destination { get; set; }
        public string? Country { get; set; }
        public string? EngineGrouping { get; set; }
        
        public string? QtyPerBox { get; set; }
        public string? UOM { get; set; }
        public string? BoxType { get; set; }
        public string? WeightGross { get; set; }
        public string? FinishGoodsStatus { get; set; }
		public string? UserLogin { get; set; }
		public string? LastUpdate { get; set; }
		public string? UserUpdate { get; set; }
		public string? Remarks { get; set; }
    }
}
