namespace RFIDP2P3_API.Models;

public class UploadAdjustment
{
    public DateTime TargetDate { get; set; }
    public string LineCode { get; set; } = string.Empty;
    public decimal? SpecialCycleTime { get; set; }
    public decimal RecoveryDay { get; set; }
    public decimal RecoveryNight { get; set; }
    public string? Remarks { get; set; }
}