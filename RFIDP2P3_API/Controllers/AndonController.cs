using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Net.NetworkInformation;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AndonController : Controller
    {
		private readonly string _configuration;

		public AndonController(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}
        [HttpPost]
        public ActionResult<IEnumerable<AndonStock>> AndonStock(AndonStock paramObj)
        {
			List<AndonStock> Stocks = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_AndonBuffer_Sel", conn))
			{
				conn.Open();
				cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@num", paramObj.tabGrid));
                cmd.Parameters.Add(new("@gedung", paramObj.gedung));
                SqlDataReader sdr = cmd.ExecuteReader();
				while (sdr.Read())
				{
					Stocks.Add(new AndonStock
					{ 
                        No = sdr["Num"].ToString(),
                        tabGrid = sdr["tabGrid"].ToString(),
                        PartStockMax = sdr["PartStockMax"].ToString(),
                        PartStockMin2 = sdr["PartStockMin2"].ToString(),
                        PartStockMax2 = sdr["PartStockMax2"].ToString(),
                        PartStockDay = sdr["PartStockDay"].ToString(),
                        PartStockDay2 = sdr["PartStockDay2"].ToString(),
                        StockEnd2 = sdr["StockEnd2"].ToString(),
                        CycleTime2 = sdr["CycleTime2"].ToString(),
                        Efficiency2 = sdr["Efficiency2"].ToString(),
                        PartNumber  = sdr["PartNumber"].ToString(),
                        PartName  = sdr["PartName"].ToString(),
                        PrcBlank  = sdr["PrcBlank"].ToString(),
                        PrcVaccum  = sdr["PrcVaccum"].ToString(),
                        PrcOut  = sdr["PrcOut"].ToString(),
                        StockBlank  = sdr["StockBlank"].ToString(),
                        StockVaccum  = sdr["StockVaccum"].ToString(),
                        StockEnd  = sdr["StockEnd"].ToString(),
                        PrcStatus  = sdr["PrcStatus"].ToString(),
                        StockEstCasting  = sdr["StockEstCasting"].ToString(),
                        StockEstEngine  = sdr["StockEstEngine"].ToString(),
                        ProgressCasting  = sdr["ProgressCasting"].ToString(),
                        ProgressEngine  = sdr["ProgressEngine"].ToString(),
                        MaxkEstCasting  = sdr["MaxkEstCasting"].ToString(),
                        MaxEstEngine  = sdr["MaxEstEngine"].ToString(),
                        StockPalletP3Unit  = sdr["StockPalletP3Unit"].ToString(),
                        StockPalletP3Day  = sdr["StockPalletP3Day"].ToString(),
                        StockPalletP2Day = sdr["StockPalletP2Day"].ToString(),
                        StockPalletP2Unit = sdr["StockPalletP2Unit"].ToString()

                    });
				}
				conn.Close();
			}
			return Stocks;
        }
        [HttpPost]
        public ActionResult<IEnumerable<AndonStock>> AndonCentral(AndonStock paramObj)
        {
			List<AndonStock> Stocks = new();
            //BusinessObject b = new();
            //b.WriteLog(paramObj.tabGrid, "cek");
            using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_AndonCentral_Sel", conn))
			{
				conn.Open();
				cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@num", paramObj.tabGrid));
                SqlDataReader sdr = cmd.ExecuteReader();
				while (sdr.Read())
				{
					Stocks.Add(new AndonStock
					{ 
                        No = sdr["Num"].ToString(),
                        tabGrid  = sdr["tabGrid"].ToString(),
                        PartStockMax = sdr["PartStockMax"].ToString(),
                        PartStockMin2 = sdr["PartStockMin2"].ToString(),
                        PartStockMax2 = sdr["PartStockMax2"].ToString(),
                        PartStockDay = sdr["PartStockDay"].ToString(),
                        PartStockDay2 = sdr["PartStockDay2"].ToString(),
                        StockEnd2 = sdr["StockEnd2"].ToString(),
                        CycleTime2 = sdr["CycleTime2"].ToString(),
                        Efficiency2 = sdr["Efficiency2"].ToString(),
                        PartNumber  = sdr["PartNumber"].ToString(),
                        PartName  = sdr["PartName"].ToString(),
                        PartDesc  = sdr["PartDesc"].ToString(),
                        PrcBlank  = sdr["PrcBlank"].ToString(),
                        PrcVaccum  = sdr["PrcVaccum"].ToString(),
                        PrcOut  = sdr["PrcOut"].ToString(),
                        StockBlank  = sdr["StockBlank"].ToString(),
                        StockVaccum  = sdr["StockVaccum"].ToString(),
                        StockEnd  = sdr["StockEnd"].ToString(),
                        PrcStatus  = sdr["PrcStatus"].ToString(),
                        StockEstCasting  = sdr["StockEstCasting"].ToString(),
                        StockEstEngine  = sdr["StockEstEngine"].ToString(),
                        ProgressCasting  = sdr["ProgressCasting"].ToString(),
                        ProgressEngine  = sdr["ProgressEngine"].ToString(),
                        MaxkEstCasting  = sdr["MaxkEstCasting"].ToString(),
                        MaxEstEngine  = sdr["MaxEstEngine"].ToString(),
                        StockPalletP3Unit  = sdr["StockPalletP3Unit"].ToString(),
                        StockPalletP3Day  = sdr["StockPalletP3Day"].ToString(),
                        StockPalletP2Day = sdr["StockPalletP2Day"].ToString(),
                        StockPalletP2Unit = sdr["StockPalletP2Unit"].ToString()

                    });
				}
				conn.Close();
			}
			return Stocks;
        }
        [HttpPost]
        public ActionResult<IEnumerable<AndonStockEngine>> AndonStockEngine(AndonStockEngine paramObj)
        {
			List<AndonStockEngine> Stocks = new();

            //BusinessObject b = new(); b.WriteLog(paramObj.tabGrid, "grd");
            using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_AndonStockEngine_Sel", conn))
			{
				conn.Open();
				cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@num", paramObj.tabGrid));
                cmd.Parameters.Add(new("@gedung", paramObj.gedung));
                SqlDataReader sdr = cmd.ExecuteReader();
				while (sdr.Read())
				{
					Stocks.Add(new AndonStockEngine
                    {
						Num= sdr["Num"].ToString(),
						PartNumber= sdr["PartNumber"].ToString(),
						PartDesc= sdr["PartDesc"].ToString(),
						PartName= sdr["PartName"].ToString(),
						MIN= sdr["MIN"].ToString(),
						MAX= sdr["MAX"].ToString(), 
                        CycleTime = sdr["CycleTime"].ToString(),
						Efficiency= sdr["Efficiency"].ToString(),
						StartStock= sdr["StartStock"].ToString(),
						EnterStock= sdr["EnterStock"].ToString(),
						ProdSupply= sdr["ProdSupply"].ToString(),
						endStock= sdr["endStock"].ToString(),
						Status= sdr["Status"].ToString(),
                        StockBar = sdr["StockBar"].ToString(),
                        MaxBar = sdr["MaxBar"].ToString(),
                        StockHour = sdr["StockHour"].ToString(),
                        MaxHour = sdr["MaxHour"].ToString(),
                        tabGrid = sdr["TabGrid"].ToString()
					});
				}
				conn.Close();
			}
			return Stocks;
        }
        [HttpPost]
        public ActionResult<IEnumerable<AndonReturnPallet>> AndonReturnPallet(AndonReturnPallet paramObj)
        {
			List<AndonReturnPallet> Stocks = new();

            //BusinessObject b = new(); b.WriteLog(paramObj.tabGrid, "grd");
            using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_AndonMonitoringReturnPallet", conn))
			{
				conn.Open();
				cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@num", paramObj.tabGrid));
                cmd.Parameters.Add(new("@gedung", paramObj.gedung));
                SqlDataReader sdr = cmd.ExecuteReader();
				while (sdr.Read())
				{
					Stocks.Add(new AndonReturnPallet
					{
						Num= sdr["Num"].ToString(),
                        PalletType = sdr["PalletType"].ToString(),
						Qty= sdr["Qty"].ToString(),
						QtyScan= sdr["QtyScan"].ToString(),
						Total= sdr["Total"].ToString(),
						idx= sdr["idx"].ToString(),
						ItemPallet= sdr["ItemPallet"].ToString(),
						typ = sdr["typ"].ToString()

					});
				}
				conn.Close();
			}
			return Stocks;
        }
        [HttpPost]
        public ActionResult<IEnumerable<AndonStockEngineList>> AndonStockEngineList(AndonStockEngineList paramObj)
        {
			List<AndonStockEngineList> Stocks = new();

            //BusinessObject b = new(); b.WriteLog(paramObj.tabGrid, "grd");
            using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_AndonStockEngineList_Sel", conn))
			{
				conn.Open();
				cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@gedung", paramObj.gedung));
                cmd.Parameters.Add(new("@type", paramObj.type));
                cmd.Parameters.Add(new("@num", paramObj.tabGrid));
                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    Stocks.Add(new AndonStockEngineList
                    {
                        PartNumber = sdr["PartNumber"].ToString(),
                        tabGrid = sdr["tabGrid"].ToString(),
                        DNNumber = sdr["DNNumber"].ToString(),
                        PartName = sdr["PartName"].ToString(),
                        Part_Qty = sdr["Part_Qty"].ToString(),
                        NR1 = sdr["NR1"].ToString(),
                        NR2 = sdr["NR2"].ToString(),
                        NR3 = sdr["NR3"].ToString(),
                        NR4 = sdr["NR4"].ToString(),
                        NR5 = sdr["NR5"].ToString(),
                        NR6 = sdr["NR6"].ToString(),
                        NR7 = sdr["NR7"].ToString(),
                        NR8 = sdr["NR8"].ToString(),
                        SZ1 = sdr["SZ1"].ToString(),
                        SZ2 = sdr["SZ2"].ToString(),
                        SZ3 = sdr["SZ3"].ToString(),
                        SZ4 = sdr["SZ4"].ToString(),
                        SZ5 = sdr["SZ5"].ToString(),
                        SZ6 = sdr["SZ6"].ToString()
                    });
                }
				conn.Close();
			}
			return Stocks;
        }
        
        [HttpPost]
        public ActionResult<IEnumerable<AndonStockEngineList>> AndonStockEngineCentral(AndonStockEngineList paramObj)
        {
			List<AndonStockEngineList> Stocks = new();

            //BusinessObject b = new(); b.WriteLog(paramObj.tabGrid, "grd");
            using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_AndonStockEngineCentral_Sel", conn))
			{
				conn.Open();
				cmd.CommandType = CommandType.StoredProcedure;
                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    Stocks.Add(new AndonStockEngineList
                    {
                        tabGrid = sdr["tabGrid"].ToString(),
                        PartNumber = sdr["PartNumber"].ToString(),
                        DNNumber = sdr["DNNumber"].ToString(),
                        PartName = sdr["PartName"].ToString(),
                        Part_Qty = sdr["Part_Qty"].ToString()
                    });
                }
				conn.Close();
			}
			return Stocks;
        }
        
        [HttpPost]
        public ActionResult<IEnumerable<SMRCentral>> SMRCentral(SMRCentral paramObj)
        {
			List<SMRCentral> ReturnValue = new();

            //BusinessObject b = new(); b.WriteLog(paramObj.tabGrid, "grd");
            using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_AndonSMRCentral_Sel", conn))
			{
				conn.Open();
				cmd.CommandType = CommandType.StoredProcedure;
                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    ReturnValue.Add(new SMRCentral
                    {
                        num = sdr["num"].ToString(),
                        Label = sdr["Label"].ToString(),
                        AM = sdr["AM"].ToString(),
                        LB = sdr["LB"].ToString(),
                        color1 = sdr["color1"].ToString(),
                        color2 = sdr["color2"].ToString()
                    });
                }
				conn.Close();
			}
			return ReturnValue;
        }

        [HttpPost]
        public ActionResult<IEnumerable<AndonStockLDKList>> AndonStockLDKList(AndonStockLDKList paramObj)
        {
			List<AndonStockLDKList> Stocks = new();

            //BusinessObject b = new(); b.WriteLog(paramObj.tabGrid, "grd");
            using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("[sp_AndonStockLDKList_Sel]", conn))
			{
				conn.Open();
				cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@gedung", paramObj.gedung));
                //cmd.Parameters.Add(new("@num", paramObj.tabGrid));
                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    Stocks.Add(new AndonStockLDKList
                    {
                        NR1 = sdr["NR1"].ToString(),
                        NR2 = sdr["NR2"].ToString(),
                        NR3 = sdr["NR3"].ToString(),
                        NR4 = sdr["NR4"].ToString(),
                        NR5 = sdr["NR5"].ToString(),
                        NR6 = sdr["NR6"].ToString(),
                        NR7 = sdr["NR7"].ToString(),
                        NR8 = sdr["NR8"].ToString(),
                        SZ1 = sdr["SZ1"].ToString(),
                        SZ2 = sdr["SZ2"].ToString(),
                        SZ3 = sdr["SZ3"].ToString(),
                        SZ4 = sdr["SZ4"].ToString(),
                        SZ5 = sdr["SZ5"].ToString(),
                        SZ6 = sdr["SZ6"].ToString()
                    });
                }
				conn.Close();
			}
			return Stocks;
        }
        [HttpPost]
		public ActionResult<IEnumerable<AndonPallet>> AndonPallet()
		{
			List<AndonPallet> Pallets = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_AndonPallet_Sel", conn))
			{
				conn.Open();
				cmd.CommandType = CommandType.StoredProcedure;

				SqlDataReader sdr = cmd.ExecuteReader();
				while (sdr.Read())
				{
					Pallets.Add(new AndonPallet
					{
						PalletId = sdr["PalletId"].ToString(),
						PalletType = sdr["PalletType"].ToString(),
						PartNumber = sdr["PartNumber"].ToString(),
						PalletStatus = sdr["PalletStatus"].ToString()
					});
				}
				conn.Close();
			}
			return Pallets;
		}
        [HttpPost]
        public ActionResult<IEnumerable<AndonPreDelivery>> AndonPreDelivery(AndonPreDelivery paramObj)
        {
            List<AndonPreDelivery> Selectitems = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_AndonPreDeliveryStatus_Sel", conn))
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@gedung", paramObj.gedung));

                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    Selectitems.Add(new AndonPreDelivery
                    {
                        leftLabel = sdr["leftLabel"].ToString(),
                        rightLabel = sdr["rightLabel"].ToString(),
                        NR1= sdr["NR1"].ToString(),
                        NR2= sdr["NR2"].ToString(),
                        NR3= sdr["NR3"].ToString(),
                        NR4= sdr["NR4"].ToString(),
                        NR5= sdr["NR5"].ToString(),
                        NR6= sdr["NR6"].ToString(),
                        NR7= sdr["NR7"].ToString(),
                        NR8= sdr["NR8"].ToString(),
                        SZKR1= sdr["SZKR1"].ToString(),
                        SZKR2= sdr["SZKR2"].ToString(),
                        SZKR3= sdr["SZKR3"].ToString(),
                        SZKR4= sdr["SZKR4"].ToString(),
                        WAD1= sdr["WAD1"].ToString(),
                        WAD2= sdr["WAD2"].ToString(), 
                    });
                }
                conn.Close();
            }
            return Selectitems;
        }
        [HttpPost]
        public ActionResult<IEnumerable<AndonPreDelivery>> AndonPreDeliveryCentral(AndonPreDelivery paramObj)
        {
            List<AndonPreDelivery> Selectitems = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_AndonPreDeliveryCentral_Sel", conn))
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    Selectitems.Add(new AndonPreDelivery
                    {
                        leftLabel = sdr["leftLabel"].ToString(),
                        rightLabel = sdr["rightLabel"].ToString(),
                        NR1= sdr["NR1"].ToString(),
                        NR2= sdr["NR2"].ToString(),
                        NR3= sdr["NR3"].ToString(),
                        NR4= sdr["NR4"].ToString(),
                        NR5= sdr["NR5"].ToString(),
                        NR6= sdr["NR6"].ToString(),
                        NR7= sdr["NR7"].ToString(),
                        NR8= sdr["NR8"].ToString(),
                        SZKR1= sdr["SZKR1"].ToString(),
                        SZKR2= sdr["SZKR2"].ToString(),
                        SZKR3= sdr["SZKR3"].ToString(),
                        SZKR4= sdr["SZKR4"].ToString(),
                        WAD1= sdr["WAD1"].ToString(),
                        WAD2= sdr["WAD2"].ToString(), 
                    });
                }
                conn.Close();
            }
            return Selectitems;
        }
        
        [HttpPost]
        public ActionResult<IEnumerable<AndonPreDelivery>> AndonDelivery(AndonPreDelivery paramObj)
        {
            List<AndonPreDelivery> Selectitems = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_AndonDeliveryStatus_Sel", conn))
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@gedung", paramObj.gedung));

                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    Selectitems.Add(new AndonPreDelivery
                    {
                        leftLabel = sdr["leftLabel"].ToString(),
                        rightLabel = sdr["rightLabel"].ToString(),
                        NR1= sdr["NR1"].ToString(),
                        NR2= sdr["NR2"].ToString(),
                        NR3= sdr["NR3"].ToString(),
                        NR4= sdr["NR4"].ToString(),
                        NR5= sdr["NR5"].ToString(),
                        NR6= sdr["NR6"].ToString(),
                        NR7= sdr["NR7"].ToString(),
                        NR8= sdr["NR8"].ToString(),
                        SZKR1= sdr["SZKR1"].ToString(),
                        SZKR2= sdr["SZKR2"].ToString(),
                        SZKR3= sdr["SZKR3"].ToString(),
                        SZKR4= sdr["SZKR4"].ToString(),
                        WAD1= sdr["WAD1"].ToString(),
                        WAD2= sdr["WAD2"].ToString(), 
                    });
                }
                conn.Close();
            }
            return Selectitems;
        }
        
        [HttpPost]
        public ActionResult<IEnumerable<AndonPreDelivery>> AndonDeliveryCentral(AndonPreDelivery paramObj)
        {
            List<AndonPreDelivery> Selectitems = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_AndonDeliveryCentral_Sel", conn))
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    Selectitems.Add(new AndonPreDelivery
                    {
                        leftLabel = sdr["leftLabel"].ToString(),
                        rightLabel = sdr["rightLabel"].ToString(),
                        NR1= sdr["NR1"].ToString(),
                        NR2= sdr["NR2"].ToString(),
                        NR3= sdr["NR3"].ToString(),
                        NR4= sdr["NR4"].ToString(),
                        NR5= sdr["NR5"].ToString(),
                        NR6= sdr["NR6"].ToString(),
                        NR7= sdr["NR7"].ToString(),
                        NR8= sdr["NR8"].ToString(),
                        SZKR1= sdr["SZKR1"].ToString(),
                        SZKR2= sdr["SZKR2"].ToString(),
                        SZKR3= sdr["SZKR3"].ToString(),
                        SZKR4= sdr["SZKR4"].ToString(),
                        WAD1= sdr["WAD1"].ToString(),
                        WAD2= sdr["WAD2"].ToString(), 
                    });
                }
                conn.Close();
            }
            return Selectitems;
        }
        
        [HttpPost]
        public ActionResult<IEnumerable<AndonDeliveryDetail>> AndonDeliveryDetail(AndonDeliveryDetail paramObj)
        {
            List<AndonDeliveryDetail> Selectitems = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_AndonDeliveryDetail_Sel", conn))
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@num", paramObj.tabGrid));
                cmd.Parameters.Add(new("@gedung", paramObj.gedung));

                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    Selectitems.Add(new AndonDeliveryDetail
                    {
                        Nos = sdr["Nos"].ToString(),
                        tabGrid = sdr["tabGrid"].ToString(),
                        PartNumber = sdr["PartNumber"].ToString(),
                        PartName = sdr["PartName"].ToString(),
                        DateCycle = sdr["DateCycle"].ToString(),
                        DA1 = sdr["DA1"].ToString(),
                        DA2 = sdr["DA2"].ToString(),
                        DA3 = sdr["DA3"].ToString(),
                        DA4 = sdr["DA4"].ToString(),
                        DA5 = sdr["DA5"].ToString(),
                        DA6 = sdr["DA6"].ToString(),
                        DA7 = sdr["DA7"].ToString(),
                        DA8 = sdr["DA8"].ToString(),
                        DB1 = sdr["DB1"].ToString(),
                        DB2 = sdr["DB2"].ToString(),
                        DB3 = sdr["DB3"].ToString(),
                        DB4 = sdr["DB4"].ToString(),
                        DB5 = sdr["DB5"].ToString(),
                        DB6 = sdr["DB6"].ToString(),
                        DB7 = sdr["DB7"].ToString(),
                        DB8 = sdr["DB8"].ToString()
                    });
                }
                conn.Close();
            }
            return Selectitems;
        }
        [HttpPost]
        public ActionResult<IEnumerable<AndonPairingStockHeader>> AndonPairingStockHeader(AndonPairingStockHeader paramObj)
        {
            List<AndonPairingStockHeader> StockHeaders = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_AndonPairingStockHeader_Sel", conn))
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@gedung", paramObj.gedung));

                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    StockHeaders.Add(new AndonPairingStockHeader
                    {
                        PalletId = sdr["PalletId"].ToString(),
                        PartName = sdr["PartName"].ToString(),
                        PartQty = sdr["PartQty"].ToString(),
                        Arrow = sdr["Arrow"].ToString(),
                        PairingStatus = sdr["PairingStatus"].ToString()
                    });
                }
                conn.Close();
            }
            return StockHeaders;
        }
        [HttpPost]
        public ActionResult<IEnumerable<AndonPairingStockList>> AndonPairingStockList(AndonPairingStockList paramObj)
        {
            List<AndonPairingStockList> StockLists = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_AndonPairingStockList_Sel", conn))
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@gedung", paramObj.gedung));

                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    StockLists.Add(new AndonPairingStockList
                    {
                        PalletId = sdr["PalletId"].ToString(),
                        PartName = sdr["PartName"].ToString(),
                        PartQty = sdr["PartQty"].ToString(),
                        Arrow = sdr["Arrow"].ToString(),
                        PairingStatus = sdr["PairingStatus"].ToString()
                    });
                }
                conn.Close();
            }
            return StockLists;
        }
        [HttpPost]
        public ActionResult<IEnumerable<AndonPairingKanbanHeader>> AndonPairingKanbanHeader(AndonPairingKanbanHeader paramObj)
        {
            List<AndonPairingKanbanHeader> KanbanHeaders = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_AndonPairingKanbanHeader_Sel", conn))
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@gedung", paramObj.gedung));

                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    KanbanHeaders.Add(new AndonPairingKanbanHeader
                    {
                        labels = sdr["labels"].ToString(),
                        KanbanId = sdr["KanbanId"].ToString(),
                        PartName = sdr["PartName"].ToString(),
                        PartQty = sdr["PartQty"].ToString(),
                        PairingStatus = sdr["PairingStatus"].ToString()
                    });
                }
                conn.Close();
            }
            return KanbanHeaders;
        }
        [HttpPost]
        public ActionResult<IEnumerable<AndonPairingKanbanList>> AndonPairingKanbanList(AndonPairingKanbanList paramObj)
        {
            List<AndonPairingKanbanList> KanbanLists = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_AndonPairingKanbanList_Sel", conn))
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@gedung", paramObj.gedung));

                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    KanbanLists.Add(new AndonPairingKanbanList
                    {
                        Nos = sdr["Nos"].ToString(),
                        KanbanId = sdr["KanbanId"].ToString(),
                        Cycle = sdr["Cycle"].ToString(),
                        PartName = sdr["PartName"].ToString(),
                        PartQty = sdr["PartQty"].ToString(),
                        PairingStatus  = sdr["PairingStatus"].ToString()
                    });
                }
                conn.Close();
            }
            return KanbanLists;
        }

        [HttpPost]
        public ActionResult<IEnumerable<AndonPreDeliveryStatus>> AndonPreDeliveryStatus()
        {
            List<AndonPreDeliveryStatus> PreDelStatus = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_AndonPreDeliveryStatus_Sel", conn))
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    PreDelStatus.Add(new AndonPreDeliveryStatus
                    {
                        LineName = sdr["LineName"].ToString(),
                        Cycle = sdr["Cycle"].ToString(),
                        TimePlan = sdr["TimePlan"].ToString(),
                        TimeAct = sdr["TimeAct"].ToString(),
                        QtyPlan = sdr["QtyPlan"].ToString(),
                        QtyAct = sdr["QtyAct"].ToString(),
                        QtyBal = sdr["QtyBal"].ToString(),
                        ProgressStatus = sdr["ProgressStatus"].ToString()
                    });
                }
                conn.Close();
            }
            return PreDelStatus;
        }
        [HttpPost]
        public ActionResult<IEnumerable<AndonBuffer>> AndonBuffer()
        {
            List<AndonBuffer> getResult = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_AndonBuffer_Sel", conn))
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    getResult.Add(new AndonBuffer
                    {

                       ProdDate = sdr["ProdDate"].ToString(),
                        PartStockMax = sdr["PartStockMax"].ToString(),
                       ProdShift = sdr["ProdShift"].ToString(),
                       ProdTime = sdr["ProdTime"].ToString(),
                       PartNumber = sdr["PartNumber"].ToString(),
                       PartDesc = sdr["PartDesc"].ToString(),
                       PartName = sdr["PartName"].ToString(),
                       ProcessBlank = sdr["ProcessBlank"].ToString(),
                       ProcessVaccum = sdr["ProcessVaccum"].ToString(),
                       ProcessOut = sdr["ProcessOut"].ToString(),
                       StockBlank = sdr["StockBlank"].ToString(),
                       StockVaccum = sdr["StockVaccum"].ToString(),
                       StockEnd = sdr["StockEnd"].ToString(),
                       StockStatus = sdr["StockStatus"].ToString(),
                       StockEstCasting = sdr["StockEstCasting"].ToString(),
                       StockEstEngine = sdr["StockEstEngine"].ToString(),
                       StockPalleP3tUnit = sdr["StockPalleP3tUnit"].ToString(),
                       StockPalletP3Day = sdr["StockPalletP3Day"].ToString(),
                       StockPalleP2tUnit = sdr["StockPalleP2tUnit"].ToString(),
                       StockPalleP2tDay = sdr["StockPalleP2tDay"].ToString(),
                        MaxkEstCasting = sdr["MaxkEstCasting"].ToString(),
                       MaxEstEngine = sdr["MaxEstEngine"].ToString()

                    });
                }
                conn.Close();
            }
            return getResult;
        }
        [HttpPost]
        public ActionResult<IEnumerable<AndonPairingStockHeader>> AndonPairingCek(AndonPairingStockHeader paramObj)
        {
            List<AndonPairingStockHeader> StockHeaders = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_AndonPairing_cek", conn))
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@param", paramObj.param));
                cmd.Parameters.Add(new("@gedung", paramObj.gedung));

                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    StockHeaders.Add(new AndonPairingStockHeader
                    {
                        PalletId = sdr["PalletId"].ToString()
                    });
                }
                conn.Close();
            }
            return StockHeaders;
        }

        [HttpPost]
        public ActionResult<IEnumerable<Shift>> Shift(Shift paramObj)
        {
            List<Shift> retShift = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Shift_cek", conn))
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@param", paramObj.Plant));

                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    retShift.Add(new Shift
                    {
                        shift = sdr["shift"].ToString()
                    });
                }
                conn.Close();
            }
            return retShift;
        }
        [HttpPost]
        public ActionResult<IEnumerable<AndonReceivingEngine>> AndonReceivingEngine(AndonReceivingEngine paramObj)
        {
            List<AndonReceivingEngine> retValue = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_AndonReceivingEngine_Sel", conn))
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@gedung", paramObj.gedung));
                cmd.Parameters.Add(new("@type", '1'));
                cmd.Parameters.Add(new("@num", paramObj.tabGrid));

                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    retValue.Add(new AndonReceivingEngine
                    {
                        Nos = sdr["Nos"].ToString(),
                        Cycle = sdr["Cycle"].ToString(),
                        DNNo = sdr["DNNo"].ToString(),
                        PartNumber = sdr["PartNumber"].ToString(),
                        PartName = sdr["PartName"].ToString(),
                        Arrival = sdr["Arrival"].ToString(),
                        orderKanban = sdr["orderKanban"].ToString(),
                        received = sdr["received"].ToString(),
                        balanched = sdr["balanched"].ToString(),
                        TotalReceived = sdr["TotalReceived"].ToString(),
                        status = sdr["status"].ToString(),
                        acheivement = sdr["acheivement"].ToString(),
                        tabGrid = sdr["tabGrid"].ToString()
                    });
                }
                conn.Close();
            }
            return retValue;
        }
        [HttpPost]
        public ActionResult<IEnumerable<AndonReceivingEngineList>> AndonReceivingEngineList(AndonReceivingEngineList paramObj)
        {
            List<AndonReceivingEngineList> retValue = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_AndonReceivingEngineDetail_Sel", conn))
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@gedung", paramObj.gedung));
                cmd.Parameters.Add(new("@type", paramObj.type));
                cmd.Parameters.Add(new("@num", paramObj.tabGrid));

                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    retValue.Add(new AndonReceivingEngineList
                    {
                        Cycle = sdr["Cycle"].ToString(),
                        tabGrid = sdr["tabGrid"].ToString(),
                        DNNo = sdr["DNNo"].ToString(),
                        Num = sdr["Num"].ToString(),
                        PartName = sdr["PartName"].ToString(),
                        PartNumber = sdr["PartNumber"].ToString(),
                        received = sdr["received"].ToString(),
                        zCycle = sdr["zCycle"].ToString(),
                        zDNNo = sdr["zDNNo"].ToString(),
                        zNum = sdr["zNum"].ToString(),
                        zPartName = sdr["zPartName"].ToString(),
                        zPartNumber = sdr["zPartNumber"].ToString(),
                        zReceived = sdr["zReceived"].ToString()
                       
                    });
                }
                conn.Close();
            }
            return retValue;
        }
        
        [HttpPost]
        public ActionResult<IEnumerable<AndonReceivingEnginePairingLeft>> AndonReceivingEnginePairingLeft(AndonReceivingEnginePairingLeft paramObj)
        {
            List<AndonReceivingEnginePairingLeft> retValue = new();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_AndonReceivingEngineDetails_Sel", conn))
            {
                conn.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@gedung", paramObj.gedung));
                cmd.Parameters.Add(new("@type", '1'));

                SqlDataReader sdr = cmd.ExecuteReader();
                while (sdr.Read())
                {
                    retValue.Add(new AndonReceivingEnginePairingLeft
                    {
                        Num = sdr["Num"].ToString(),
                        tabGrid = sdr["tabGrid"].ToString(),
                        Num2 = sdr["Num2"].ToString(),
                        DNNo = sdr["DNNo"].ToString(),
                        Cycle = sdr["Cycle"].ToString(),
                        Category = sdr["Category"].ToString(),
                        Item = sdr["Item"].ToString(),
                        Resumes = sdr["Resumes"].ToString(),
                        Qty  = sdr["Qty"].ToString()
                       
                       
                    });
                }
                conn.Close();
            }
            return retValue;
        }
    }
}
