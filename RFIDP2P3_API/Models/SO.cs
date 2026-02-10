namespace RFIDP2P3_API.Models
{
    public class SO
    {
        public string? ExCore { get; set; }
        public string? JobNo { get; set; }
        public string? Line { get; set; }
        public string? Supplier { get; set; }
        public string? Route { get; set; }
        public string? ArrivalDate { get; set; }
        public string? Cycle { get; set; }
        public string? Volumetric { get; set; }
        public string? TotalPartVolume { get; set; }
        public string? Remarks { get; set; }
        public string? UserLogin { get; set; }
        public List<SOPart>? SOParts { get; set; }
    }
}
