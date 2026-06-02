namespace RFIDP2P3_API.Models
{
	public class MasterSuffixToUnique
    {
		public string? IUType { get; set; }
		public string? SuffixId { get; set; }
		public string? SuffixCode { get; set; }
		public string? UniqueCode { get; set; }
		public string? ModelGroup { get; set; }
		public string? LineOrderCode { get; set; }
		public string? OrderFrom { get; set; }
		public string? PartNumber { get; set; }
		public bool IsActive { get; set; }
        
		public string? UserLogin { get; set; }
		
		public string? CreatedBy { get; set; }
		public string? CreatedDate { get; set; }
		public string? UpdatedBy { get; set; }
		public string? UpdatedDate { get; set; }
		
		public string? Remarks { get; set; }
    }
}
