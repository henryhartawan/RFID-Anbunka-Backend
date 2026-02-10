namespace RFIDP2P3_API.Models
{
	public class InterfaceSAP
	{
		public string? PartNumber { get; set; }
		public string? Sloc { get; set; }
		public string? Qty { get; set; }
	}
	public class InterfaceAnbunka
	{
		public string? IDNo { get; set; }
		public string? DNNo { get; set; }
		public string? DNDate { get; set; }
		public string? DNPrefix { get; set; }
		public string? PartNumber { get; set; }
		public string? KanbanId { get; set; }
		public string? Qty { get; set; }
		public string? Message { get; set; }
		public string? ReceiveNo { get; set; }
	}
	public class InterfaceAnbunkaOut
	{
		public string? IDNo { get; set; }
		public string? DNDate { get; set; }
		public string? DNPrefix { get; set; }
		public string? PartNumber { get; set; }
		public string? Qty { get; set; }
	}
	public class InterfaceRFIDReader
	{
		public string? PalletId { get; set; }
		public string? RFIDNo { get; set; }
		public string? PartNumber { get; set; }
	}
}
