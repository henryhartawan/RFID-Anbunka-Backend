namespace RFIDP2P3_API.Models.Request;

public class UpdateCapacityCellRequest
{
    public string Periode { get; set; }
    public string LineOrderCode { get; set; }
    public string BulanKe { get; set; }
    public string FieldName { get; set; }
    public decimal NewValue { get; set; }
}