namespace RFIDP2P3_API.Models;

public class MandatoryOptionConfig
{
    public string LineType { get; set; } = "";
    public int Shift { get; set; }

    public List<MandatoryOptionItem> Items { get; set; } = new List<MandatoryOptionItem>();
}