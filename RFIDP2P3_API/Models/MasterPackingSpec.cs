namespace RFIDP2P3_API.Models
{
	public class MasterPackingSpec
	{
		public string? IUType { get; set; }
		public string? BoxType { get; set; }
		public string? BoxWidth { get; set; }
        public string? BoxLength { get; set; }
        public string? BoxHeight { get; set; }
        public string? BoxVolume { get; set; }
        public string? BoxWeight { get; set; }
        public string? QtyLayer { get; set; }
        public string? Stacking { get; set; }
        public string? SkidWidth { get; set; }
        public string? SkidLength { get; set; }
        public string? SkidHeight { get; set; }
        public string? SkidVolume { get; set; }
        public string? PackingStatus { get; set; }
        public string? UserLogin { get; set; }
		public string? LastUpdate { get; set; }
		public string? UserUpdate { get; set; }
		public string? Remarks { get; set; }
    }
}
