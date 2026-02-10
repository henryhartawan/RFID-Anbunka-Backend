namespace RFIDP2P3_API.Models
{
    public class ReportLog
	{
		public string? ID { get; set; }
		public string? ScanType { get; set; }
		public string? ProcessArea { get; set; }
		public string? ProcessDate { get; set; }
		public string? PalletType { get; set; }
		public string? RFIDNo { get; set; }
		public string? LDK_No { get; set; }
		public string? PartNumber { get; set; }
		public string? PartDescription { get; set; }
		public string? Qty { get; set; }
		public string? KanbanNo { get; set; }
		public string? RFIDStatus { get; set; }
		public string? RFIDErrMsg { get; set; }
		public string? SAPStatus { get; set; }
		public string? SAPDocNo { get; set; }
		public string? SAPErrMsg { get; set; }
		public string? AnbunkaStatus { get; set; }
		public string? AnbunkaDocNo { get; set; }
		public string? AnbunkaErrMsg { get; set; }
		public string? SAPButton { get; set; }
		public string? AnbunkaButton { get; set; }
		public string? Type { get; set; }
		public string? UserLogin { get; set; }
		public string? BuildingName { get; set; }
		public string? Cycle { get; set; }
		public string? From { get; set; }
        public string? To { get; set; }
        public string? Remarks { get; set; }
        public string? HeaderTxt { get; set; }
    }
}
