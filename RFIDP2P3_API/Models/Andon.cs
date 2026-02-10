using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace RFIDP2P3_API.Models
{
    [DataContract]
    public class AndonStockLDKList
    {
        public string? gedung { get; set; }
        public string? NR1 { get; set; }
        public string? NR2 { get; set; }
        public string? NR3 { get; set; }
        public string? NR4 { get; set; }
        public string? NR5 { get; set; }
        public string? NR6 { get; set; }
        public string? NR7 { get; set; }
        public string? NR8 { get; set; }
        public string? SZ1 { get; set; }
        public string? SZ2 { get; set; }
        public string? SZ3 { get; set; }
        public string? SZ4 { get; set; }
        public string? SZ5 { get; set; }
        public string? SZ6 { get; set; }

    }   
       public class AndonStockEngineList
    {
        public string? gedung { get; set; }
        public string? tabGrid { get; set; }
        public string? type { get; set; }
        public string? PartNumber { get; set; }
        public string? DNNumber { get; set; }
        public string? PartName { get; set; }
        public string? Part_Qty { get; set; }
        public string? NR1 { get; set; }
        public string? NR2 { get; set; }
        public string? NR3 { get; set; }
        public string? NR4 { get; set; }
        public string? NR5 { get; set; }
        public string? NR6 { get; set; }
        public string? NR7 { get; set; }
        public string? NR8 { get; set; }
        public string? SZ1 { get; set; }
        public string? SZ2 { get; set; }
        public string? SZ3 { get; set; }
        public string? SZ4 { get; set; }
        public string? SZ5 { get; set; }
        public string? SZ6 { get; set; }

    }   
    
    public class AndonStockEngine
    {
        public string? gedung { get; set; }
        public string? Num { get; set; }
        public string? CycleTime { get; set; }
        public string? Efficiency { get; set; }
        public string? PartNumber  { get; set; }
        public string? PartDesc  { get; set; }
        public string? PartName  { get; set; }
        public string? StockBar { get; set; }
        public string? MaxBar { get; set; }
        public string? MaxHour { get; set; }
        public string? MIN  { get; set; }
        public string? MAX  { get; set; }
        public string? StartStock  { get; set; }
        public string? EnterStock  { get; set; }
        public string? ProdSupply  { get; set; }
        public string? endStock  { get; set; }
        public string? Status  { get; set; }
        public string? StockHour { get; set; }
        public string? tabGrid { get; set; }

    }
    public class AndonStock
    {

        public string? ProgressCasting { get; set; }
        public string? ProgressEngine { get; set; }
        public string? PartStockDay { get; set; }
        public string? MaxkEstCasting { get; set; }
        public string? MaxEstEngine { get; set; }
        public string? PartStockMax { get; set; }
        public string? PartStockMin2 { get; set; }
        public string? PartStockMax2 { get; set; }
        public string? PartStockDay2 { get; set; }
        public string? StockEnd2 { get; set; }
        public string? CycleTime2 { get; set; }
        public string? Efficiency2 { get; set; }



        public string? gedung { get; set; }

        [DataMember(Name = "leftLabel")]
        public string? leftLabel { get; set; }

        [DataMember(Name = "tabGrid")]
        public string? tabGrid { get; set; }

        [DataMember(Name = "No")]
        public string? No { get; set; }

        [DataMember(Name = "PartNumber")]
        public string? PartNumber { get; set; }

        [DataMember(Name = "PartDesc")]
        public string? PartDesc { get; set; }

        [DataMember(Name = "PartName")]
        public string? PartName { get; set; }

        [DataMember(Name = "PrcBlank")]
        public string? PrcBlank { get; set; }

        [DataMember(Name = "PrcVaccum")]
        public string? PrcVaccum { get; set; }

        [DataMember(Name = "PrcOut")]
        public string? PrcOut { get; set; }

        [DataMember(Name = "StockBlank")]
        public string? StockBlank { get; set; }

        [DataMember(Name = "StockVaccum")]
        public string? StockVaccum { get; set; }

        [DataMember(Name = "StockEnd")]
        public string? StockEnd { get; set; }

		[DataMember(Name = "PrcStatus")]
		public string? PrcStatus { get; set; }

		[DataMember(Name = "StockEstCasting")]
        public string? StockEstCasting { get; set; }

        [DataMember(Name = "StockEstEngine")]
        public string? StockEstEngine { get; set; }

        [DataMember(Name = "StockPalletP3Unit")]
        public string? StockPalletP3Unit { get; set; }

        [DataMember(Name = "StockPalletP3Day")]
        public string? StockPalletP3Day { get; set; }

        [DataMember(Name = "StockPalletP2Unit")]
        public string? StockPalletP2Unit { get; set; }

        [DataMember(Name = "StockPalletP2Day")]
        public string? StockPalletP2Day { get; set; }
    }
	public class AndonPallet
    {
        public string? gedung { get; set; }
        public string? tabGrid { get; set; }
        public string? PalletId { get; set; }
		public string? PalletType { get; set; }
		public string? PartNumber { get; set; }
		public string? PalletStatus { get; set; }
	}
	public class AndonReturnPallet
    {
        public string? gedung { get; set; }
        public string? tabGrid { get; set; }
		public string? Num { get; set; }
		public string? PalletType { get; set; } 
        public string? Qty { get; set; }
        public string? QtyScan { get; set; } 
        public string? Total { get; set; } 
        public string? idx { get; set; } 
        public string? ItemPallet { get; set; }
		public string? typ { get; set; }

	}
	public class AndonDeliveryDetail
    {
        public string? gedung { get; set; }
        public string? tabGrid { get; set; }
        public string? Nos { get; set; }
        public string? PartNumber { get; set; }
        public string? PartName { get; set; }
        public string? DateCycle { get; set; }
        public string? DA1 { get; set; }
        public string? DA2 { get; set; }
        public string? DA3 { get; set; }
        public string? DA4 { get; set; }
        public string? DA5 { get; set; }
        public string? DA6 { get; set; }
        public string? DA7 { get; set; }
        public string? DA8 { get; set; }
        public string? DB1 { get; set; }
        public string? DB2 { get; set; }
        public string? DB3 { get; set; }
        public string? DB4 { get; set; }
        public string? DB5 { get; set; }
        public string? DB6 { get; set; }
        public string? DB7 { get; set; }
        public string? DB8 { get; set; }
    }
	public class AndonPreDelivery
    {
        public string? gedung { get; set; }
        public string? tabGrid { get; set; }
        public string? leftLabel { get; set; }
        public string? rightLabel { get; set; }
        public string? NR1{ get; set; }
        public string? NR2{ get; set; }
		public string? NR3{ get; set; }
		public string? NR4{ get; set; }
		public string? NR5{ get; set; }
        public string? NR6{ get; set; }
        public string? NR7{ get; set; }
        public string? NR8{ get; set; }
        public string? SZKR1{ get; set; }
        public string? SZKR2{ get; set; }
        public string? SZKR3{ get; set; }
        public string? SZKR4{ get; set; }
        public string? WAD1{ get; set; }
        public string? WAD2{ get; set; }
    }
	public class AndonDeliveryStatus
    {
        public string? gedung { get; set; }
        public string? tabGrid { get; set; }
        public string? LineName { get; set; }
		public string? Cycle { get; set; }
		public string? TimePlan { get; set; }
		public string? TimeAct { get; set; }
        public string? QtyPlan { get; set; }
        public string? QtyAct { get; set; }
        public string? QtyBal { get; set; }
        public string? ProgressStatus { get; set; }
    }
    public class AndonPreDeliveryStatus
    {
        public string? gedung { get; set; }
        public string? tabGrid { get; set; }
        public string? LineName { get; set; }
        public string? Cycle { get; set; }
        public string? TimePlan { get; set; }
        public string? TimeAct { get; set; }
        public string? QtyPlan { get; set; }
        public string? QtyAct { get; set; }
        public string? QtyBal { get; set; }
        public string? ProgressStatus { get; set; }
    }
    public class AndonDeliveryMinus
    {
        public string? gedung { get; set; }
        public string? tabGrid { get; set; }
        public string? SeqNo { get; set; }
		public string? PartNumber { get; set; }
		public string? PalletStatus { get; set; }
        public string? PartQty { get; set; }
    }
    public class AndonPairingStockHeader
    {
        public string? gedung { get; set; }
        public string? tabGrid { get; set; }
        public string? param { get; set; }
        public string? PalletId { get; set; }
        public string? PartName { get; set; }
        public string? PartQty { get; set; }
        public string? Arrow { get; set; }
        public string? PairingStatus { get; set; }
    }
    public class AndonPairingStockList
    {
        public string? gedung { get; set; }
        public string? tabGrid { get; set; }
        public string? PalletId { get; set; }
        public string? PartName { get; set; }
        public string? PartQty { get; set; }
        public string? Arrow { get; set; }
        public string? PairingStatus { get; set; }
    }
    public class AndonPairingKanbanHeader
    {
        public string? gedung { get; set; }
        public string? tabGrid { get; set; }
        public string? labels { get; set; }
        public string? KanbanId { get; set; }
        public string? Cycle { get; set; }
        public string? PartName { get; set; }
        public string? PartQty { get; set; }
        public string? PairingStatus { get; set; }
    }
    public class Shift
    {
        public string? Plant { get; set; }
        public string? jam { get; set; }
        public string? shift { get; set; }
    }
    public class SMRCentral
    {
        public string? color1 { get; set; }
        public string? color2 { get; set; }
        public string? num { get; set; }
        public string? Label { get; set; }
        public string? AM { get; set; }
        public string? LB { get; set; }
    }
    public class AndonPairingKanbanList
    {
        public string? gedung { get; set; }
        public string? tabGrid { get; set; }
        public string? Nos { get; set; }
        public string? KanbanId { get; set; }
        public string? Cycle { get; set; }
        public string? PartName { get; set; }
        public string? PartQty { get; set; }
        public string? PairingStatus { get; set; }
    }
    public class AndonReceivingEngine
    {
        public string? gedung { get; set; }
        public string? tabGrid { get; set; }
        public string? Nos { get; set; }
        public string? Cycle  { get; set; }
        public string? DNNo  { get; set; }
        public string? PartNumber  { get; set; }
        public string? PartName  { get; set; }
        public string? Arrival  { get; set; }
        public string? orderKanban  { get; set; }
        public string? received  { get; set; }
        public string? balanched  { get; set; }
        public string? TotalReceived  { get; set; }
        public string? status  { get; set; }
        public string? acheivement { get; set; }

    }
    public class AndonReceivingEngineList
    {
        public string? gedung { get; set; }
        public string? tabGrid { get; set; }
        public string? type { get; set; }
        public string? Cycle { get; set; }
        public string? DNNo { get; set; }
        public string? Num { get; set; }
        public string? PartName { get; set; }
        public string? PartNumber { get; set; }
        public string? received { get; set; }
        public string? zCycle { get; set; }
        public string? zDNNo { get; set; }
        public string? zNum { get; set; }
        public string? zPartName { get; set; }
        public string? zPartNumber { get; set; }
        public string? zReceived { get; set; }
    }
    
    public class AndonReceivingEnginePairingLeft
    {
        public string? gedung { get; set; }
        public string? tabGrid { get; set; }
        public string? Num{ get; set; }
        public string? Num2{ get; set; }
        public string? Resumes { get; set; }
        public string? DNNo{ get; set; }
        public string? Cycle{ get; set; }
        public string? Category{ get; set; }
        public string? Item{ get; set; }
        public string? Qty { get; set; }

    }
    public class AndonBuffer
    {
        public string? gedung { get; set; }
        public string? tabGrid { get; set; }
        public string? PartStockMax { get; set; }
        public string? PartDescription { get; set; }
        public string? MaxkEstCasting { get; set; }
        public string? MaxEstEngine { get; set; }
        public string? ProdDate { get; set; }
        public string? ProdShift { get; set; }
        public string? ProdTime { get; set; }
        public string? PartNumber { get; set; }
        public string? PartDesc { get; set; }
        public string? PartName { get; set; }
        public string? ProcessBlank { get; set; }
        public string? ProcessVaccum { get; set; }
        public string? ProcessOut { get; set; }
        public string? StockBlank { get; set; }
        public string? StockVaccum { get; set; }
        public string? StockEnd { get; set; }
        public string? StockStatus { get; set; }
        public string? StockEstCasting { get; set; }
        public string? StockEstEngine { get; set; }
        public string? StockPalleP3tUnit { get; set; }
        public string? StockPalletP3Day { get; set; }
        public string? StockPalleP2tUnit { get; set; }
        public string? StockPalleP2tDay { get; set; }
    }
}
