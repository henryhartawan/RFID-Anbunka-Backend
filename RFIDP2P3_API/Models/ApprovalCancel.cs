namespace RFIDP2P3_API.Models
{
    public class ApprovalCancel
    {
        public string? ID { get; set; }
        public string? ScanType { get; set; }
        public string? SAP_Doc_No { get; set; }
        public string? LDK_No { get; set; }
        public string? RFIDNo { get; set; }
        public string? PalletTypeName { get; set; }
        public string? PartNumber { get; set; }
        public string? PartDescription { get; set; }
        public string? KanbanNumber { get; set; }
        public string? ProcessArea { get; set; }
        public string? ProcessDate { get; set; }
        public string? UserProposed { get; set; }
        public string? DateProposed { get; set; }
        public string? Reason { get; set; }
        public string? CancelStatus { get; set; }
        public string? UserApprove { get; set; }
        public string? CancelDate { get; set; }
        public string? SAP_Cancel_Doc_No { get; set; }
        public string? From { get; set; }
        public string? To { get; set; }
		public string? Approval { get; set; }
		public string? UserLogin { get; set; }
		public string? BuildingName { get; set; }
		public string? PlantId { get; set; }
		public string? Remarks { get; set; }
    }
}
