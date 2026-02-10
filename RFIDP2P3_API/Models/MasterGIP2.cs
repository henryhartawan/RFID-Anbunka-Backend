using static IronOcr.OcrResult;

namespace RFIDP2P3_API.Models
{
	public class MasterGIP2
	{
		public string? IUType { get; set; }
		public string? UserLogin { get; set; }
		public string? Ftype { get; set; }

		public string? GIP2Id{ get; set; }
		public string? PlantId{ get; set; }
		public string? PlantName{ get; set; }
		public string? ShopId{ get; set; }
		public string? ShopName{ get; set; }
		public string? DeptId{ get; set; }
		public string? DeptName{ get; set; }
		public string? CostCenter{ get; set; }
		public string? UserUpdate{ get; set; }
		public string? DateUpdate { get; set; }


	}
}
