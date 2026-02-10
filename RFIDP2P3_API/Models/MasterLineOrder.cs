namespace RFIDP2P3_API.Models
{
	public class MasterLineOrder
	{
		public string? IUType { get; set; }
        public string? DockCode { get; set; }
        public string? Dock { get; set; }
        public string? LineOrderCode { get; set; }
        public string? LineOrderName { get; set; }
        public string? BuildingID { get; set; }
        public string? Building { get; set; }
        public string? LineOrderStatus { get; set; }
		public string? UserLogin { get; set; }
		public string? LastUpdate { get; set; }
		public string? UserUpdate { get; set; }
		public string? Remarks { get; set; }
    }
}
