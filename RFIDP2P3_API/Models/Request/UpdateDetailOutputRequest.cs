namespace RFIDP2P3_API.Models.Request;

public class UpdateDetailOutputRequest
{
    public string Periode { get; set; }
    public string LineCode { get; set; }
    public string UniqueCode { get; set; }
    public DateTime TargetDate { get; set; }
    public int FinalPlanQty { get; set; }
    public int RevisionNo { get; set; }
}