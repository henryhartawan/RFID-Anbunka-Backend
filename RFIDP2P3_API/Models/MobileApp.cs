namespace RFIDP2P3_API.Models
{
    public class PalletScan
    {
        public string? QtySuccess { get; set; }
		public string? QtyFailed { get; set; }
	}
    public class BufferStockScan
	{
		public string? PartNo { get; set; }
		public string? PartDesc { get; set; }
		public string? RFIDNo { get; set; }
		public string? PalletType { get; set; }
		public string? Remarks { get; set; }
    }
    public class PreDeliveryScan
	{
		public string? RFIDNo { get; set; }
		public string? PartNo { get; set; }
		public string? PartDesc { get; set; }
		public string? PartQty { get; set; }
		public string? PalletType { get; set; }
		public string? KanbanNo { get; set; }
		public string? CycleNo { get; set; }
		public string? KanbanDate { get; set; }
		public string? Remarks { get; set; }
    }
    public class PostingScan
    {
        public string? RFIDNo { get; set; }
        public string? PartNo { get; set; }
        public string? PartDesc { get; set; }
        public string? PartQty { get; set; }
        public string? PalletType { get; set; }
        public string? KanbanNo { get; set; }
        public string? CycleNo { get; set; }
        public string? KanbanDate { get; set; }
        public string? Remarks { get; set; }
    }
    public class LDKScan
    {
        public string? RFIDNo { get; set; }
        public string? LDKNo { get; set; }
        public string? NGType { get; set; }
        public string? Remarks { get; set; }
    }
    public class AppVer
    {
        public string? App_Ver { get; set; }
        public string? Status { get; set; }
    }
}
