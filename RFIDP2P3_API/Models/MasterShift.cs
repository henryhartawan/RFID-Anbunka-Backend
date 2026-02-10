namespace RFIDP2P3_API.Models
{
	public class MasterShift
	{
		public string? IUType { get; set; }
		public string? ShiftID { get; set; }
		public string? ShiftName { get; set; }
		public string? PlantID { get; set; }
		public string? DayShiftStart { get; set; }
		public string? DayShiftEnd { get; set; }
		public string? NightShiftStart { get; set; }
		public string? NightShiftEnd { get; set; }

		public string? UserLogin { get; set; }
		public string? LastUpdate { get; set; }
		public string? UserUpdate { get; set; }
		public string? Remarks { get; set; }
		public string? Ftype { get; set; }
	}
}
