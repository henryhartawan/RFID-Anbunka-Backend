namespace RFIDP2P3_API.Models
{
    public class LDK
    {
        public string? IUType { get; set; }
        public string? LDK_No { get; set; }
        public string? NGTypeID { get; set; }
        public string? NGType { get; set; }
        public string? ShopId { get; set; }
        public string? ShopName { get; set; }
        public string? DeptId { get; set; }
        public string? DeptName { get; set; }
        public string? PartNumber { get; set; }
        public string? PartDescription { get; set; }
        public string? PartName { get; set; }
        public string? Qty { get; set; }
        public string? Waktu_Kejadian { get; set; }
        public string? Kejadian { get; set; }
        public string? Shift { get; set; }
        public string? Reason { get; set; }
        public string? Status_Approval { get; set; }
        public string? RFIDNo { get; set; }
        public string? Approval { get; set; }
        public string? ApprovalAt { get; set; }
        public string? EditButton { get; set; }
        public string? ApproveButton { get; set; }
        public string? DownloadButton { get; set; }
        public string? UserLogin { get; set; }
        public string? Remarks { get; set; }
        public string? Ftype { get; set; }
        public string? Entry_User { get; set; }
        public string? Entry_Date { get; set; }
        public string? Approve1_By_ID { get; set; }
        public string? Approve1_Date { get; set; }
        public string? Approve2_By_ID { get; set; }
        public string? Approve2_Date { get; set; }
        public string? Approve3_By_ID { get; set; }
        public string? Approve3_Date { get; set; }
        public string? Approve4_By_ID { get; set; }
        public string? Approve4_Date { get; set; }
        public List<MasterPart>? Parts { get; set; }
    }
}
