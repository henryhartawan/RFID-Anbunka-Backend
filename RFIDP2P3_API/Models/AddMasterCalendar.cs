namespace RFIDP2P3_API.Models
{
	public class AddMasterCalendar
	{
		public string? IUType { get; set; }
        public string? CalendarId { get; set; }
        public string? LineOrderCode { get; set; }
        public string? Line { get; set; }
        public string? CalendarDate { get; set; }
        public string? Shift { get; set; }
        public string? CalendarStatus { get; set; }
        public string? CalendarStat { get; set; }
        public string? WorkingTime { get; set; }
        public string? OEE { get; set; }
        public string? CT { get; set; }
        public string? EarlyOvertime { get; set; }
        public string? EndOvertime { get; set; }
        public string? MandatoryPdt { get; set; }
        public string? OtherPdt { get; set; }
        public string? TimePdt { get; set; }
        
        public string? UserLogin { get; set; }
		public string? CreatedBy { get; set; }
		public string? CreatedDate { get; set; }
		public string? UpdatedBy { get; set; }
		public string? UpdatedDate { get; set; }
		public string? Remarks { get; set; }
		
        public string? title { get; set; }
        public string? start { get; set; }
        public string? backgroundColor { get; set; }
        public string? borderColor { get; set; }
        public string? textColor { get; set; }
        
        public string? ProdDate { get; set; }
        
    }
}
