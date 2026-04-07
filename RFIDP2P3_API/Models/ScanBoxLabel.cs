namespace RFIDP2P3_API.Models
{
    public class ScanBoxLabel
    {
        public string? SKID_ID { get; set; }
        public string? KanbanNo { get; set; }
        public string? UserLogin { get; set; }
        public List<Kanban>? Kanban { get; set; }
    }
}
