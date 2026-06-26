namespace RFIDP2P3_API.Models;

public class CustomerOrder
{
    public int CustomerOrderID { get; set; }
    public string? Periode { get; set; }
    public string? Source { get; set; }
    public string? Suffix { get; set; }
    public int DayNumber { get; set; }
    public decimal ValueData { get; set; }
    public int RevisionNo { get; set; }
    public string? Remarks { get; set; }
}