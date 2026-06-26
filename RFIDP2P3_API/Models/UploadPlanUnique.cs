namespace RFIDP2P3_API.Models;

public class UploadPlanUnique
{
    public string UniqueCode { get; set; } = "";
    public DateTime TargetDate { get; set; }
    public int ValueData { get; set; }
}