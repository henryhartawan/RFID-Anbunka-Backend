namespace RFIDP2P3_API.Models.Request;

public class CalcCapacityRequest
{
    public string Type { get; set; }
    public string Periode_ID { get; set; }
    public string Suffix { get; set; }
    public List<int> ProdPlanAdvance { get; set; }
    public List<int> Mandatory { get; set; }
    public List<int> OvertimeHot { get; set; }
    public string User_Login { get; set; }
}