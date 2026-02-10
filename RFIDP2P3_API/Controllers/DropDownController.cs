using Microsoft.AspNetCore.Mvc;
using RFIDP2P3_API.Models;
using System.Data.SqlClient;
using System.Data;

namespace RFIDP2P3_API.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class DropDownController : Controller
	{
		private readonly string _configuration;

		public DropDownController(IConfiguration configuration)
		{
			_configuration = configuration.GetConnectionString("DefaultConnection");
		}
		[HttpPost]
		public ActionResult<IEnumerable<DropDown>> INQ(DropDown paramObj)
		{
			List<DropDown> ContainerObj = new();

			using (SqlConnection conn = new SqlConnection(_configuration))
			using (SqlCommand cmd = new SqlCommand("sp_Dropdown_Sel", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;

				cmd.Parameters.Add(new("@Ftype", paramObj.Ftype));
				cmd.Parameters.Add(new("@PlantId", paramObj.Plant));
				cmd.Parameters.Add(new("@BuildingId", paramObj.BuildingId));
				cmd.Parameters.Add(new("@ShopId", paramObj.ShopId));
				cmd.Parameters.Add(new("@DeptId", paramObj.DeptID));

				conn.Open();
				SqlDataReader sdr = cmd.ExecuteReader();

				while (sdr.Read())
				{
					switch (paramObj.Ftype)
					{
						case "Plant":
							ContainerObj.Add(new DropDown
							{
								IdValue = sdr["PlantID"].ToString(),
								ViewValue = sdr["PlantName"].ToString(),
								Plant = sdr["PlantID"].ToString()
							});break;
						case "Gedung":
							ContainerObj.Add(new DropDown
							{
								IdValue = sdr["BuildingId"].ToString(),
								ViewValue = sdr["BuildingName"].ToString(),
								Plant = sdr["PlantID"].ToString()
							});break;
						case "Shop":
							ContainerObj.Add(new DropDown
							{
								IdValue = sdr["ShopId"].ToString(),
								ViewValue = sdr["ShopName"].ToString(),
								Plant = sdr["PlantID"].ToString()
							}); break;
						case "Dept":
							ContainerObj.Add(new DropDown
							{
								IdValue = sdr["DeptID"].ToString(),
								ViewValue = sdr["DeptName"].ToString(),
								Plant = sdr["PlantID"].ToString(),
								ShopId = sdr["ShopId"].ToString()
							}); break;
						case "PalletType":
							ContainerObj.Add(new DropDown
							{
								IdValue = sdr["PalletTypeID"].ToString(),
								ViewValue = sdr["PalletTypeName"].ToString()

							}); break;
						case "Part":
							ContainerObj.Add(new DropDown
							{
								IdValue = sdr["PartNumber"].ToString(),
								ViewValue = sdr["PartName"].ToString(),
								Plant = sdr["PlantID"].ToString(),
								BuildingId = sdr["BuildingId"].ToString()
							}); break;
						case "Line":
							ContainerObj.Add(new DropDown
							{
                                IdValue = sdr["LineID"].ToString(),
								ViewValue = sdr["LineName"].ToString(),
                                Plant = sdr["PlantID"].ToString(),
                                BuildingId = sdr["BuildingId"].ToString()
                            }); break;
						case "Section":
							ContainerObj.Add(new DropDown
							{
                                IdValue = sdr["SectionID"].ToString(),
								ViewValue = sdr["SectionName"].ToString(),
								Plant = sdr["PlantID"].ToString(),
								DeptID = sdr["DeptID"].ToString()
							}); break;
						case "Shift":
							ContainerObj.Add(new DropDown
							{
                                IdValue = sdr["Shift"].ToString(),
								ViewValue = sdr["Shift"].ToString()

							}); break;
						case "GRPro":
							ContainerObj.Add(new DropDown
							{
								IdValue = sdr["GRProType"].ToString(),
								ViewValue = sdr["GRProType"].ToString()

							}); break;
						case "AppLDKShop":
							ContainerObj.Add(new DropDown
							{
								ShopId = sdr["ShopId"].ToString(),
								ViewValue = sdr["ShopName"].ToString()

							}); break;
						case "AppLDKDept":
							ContainerObj.Add(new DropDown
							{
								IdValue = sdr["DeptId"].ToString(),
								ViewValue = sdr["DeptName"].ToString(),
								ShopId = sdr["ShopId"].ToString()

							}); break;
						case "AppLDKUser":
							ContainerObj.Add(new DropDown
							{
								IdValue = sdr["UserID"].ToString(),
								ViewValue = sdr["UserName"].ToString()

							}); break;
						case "NGType":
							ContainerObj.Add(new DropDown
							{
								IdValue = sdr["NGTypeID"].ToString(),
								ViewValue = sdr["NGType"].ToString()

							}); break;
						case "PartLDK":
							ContainerObj.Add(new DropDown
							{
								IdValue = sdr["PartNumber"].ToString(),
								ViewValue = sdr["PartName"].ToString()

							}); break;
                        case "PlantCode":
                            ContainerObj.Add(new DropDown
                            {
                                IdValue = sdr["PlantCode"].ToString(),
                                ViewValue = sdr["PlantCode"].ToString(),
                                Plant = sdr["PlantCode"].ToString(),
                            }); break;
                        case "Dock":
                            ContainerObj.Add(new DropDown
                            {
                                Dock = sdr["DockCode"].ToString(),
                                IdValue = sdr["DockCode"].ToString(),
                                ViewValue = sdr["Dock"].ToString()
                            }); break;
                        case "DockRoute":
                            ContainerObj.Add(new DropDown
                            {
                                Route = sdr["RouteCode"].ToString(),
                                IdValue = sdr["DockCode"].ToString(),
                                ViewValue = sdr["Dock"].ToString()
                            }); break;
                        case "LineOrder":
                            ContainerObj.Add(new DropDown
                            {
                                IdValue = sdr["LineOrderCode"].ToString(),
                                ViewValue = sdr["LineOrder"].ToString()
                            }); break;
                        case "Supplier":
                            ContainerObj.Add(new DropDown
                            {
                                IdValue = sdr["SupplierCode"].ToString(),
                                ViewValue = sdr["Supplier"].ToString()
                            }); break;
                        case "Route":
                            ContainerObj.Add(new DropDown
                            {
                                Route = sdr["RouteCode"].ToString(),
                                IdValue = sdr["RouteCode"].ToString(),
                                ViewValue = sdr["RouteCode"].ToString()
                            }); break;
                        case "BoxType":
                            ContainerObj.Add(new DropDown
                            {
                                IdValue = sdr["BoxType"].ToString(),
                                ViewValue = sdr["BoxType"].ToString()
                            }); break;
                        case "Depth":
                            ContainerObj.Add(new DropDown
                            {
								Dock = sdr["DockCode"].ToString(),
                                IdValue = sdr["DepthID"].ToString(),
                                ViewValue = sdr["Depth"].ToString()
                            }); break;
                        case "PartOrder":
                            ContainerObj.Add(new DropDown
                            {
                                Dock = sdr["DockCode"].ToString(),
                                IdValue = sdr["PartOrderID"].ToString(),
                                ViewValue = sdr["Part"].ToString()
                            }); break;
                        case "ExCore":
                            ContainerObj.Add(new DropDown
                            {
                                ExCore = sdr["ExCore"].ToString(),
                                IdValue = sdr["ExCore"].ToString(),
                                ViewValue = sdr["Core"].ToString()
                            }); break;
                        case "CycleIssueLP":
                            ContainerObj.Add(new DropDown
                            {
                                ExCore = sdr["ExCore"].ToString(),
                                IdValue = sdr["CycleIssueID"].ToString(),
                                ViewValue = sdr["CycleIssueLP"].ToString()
                            }); break;
                        case "CycleIssuePart":
                            ContainerObj.Add(new DropDown
                            {
                                IdValue = sdr["CycleIssueID"].ToString(),
                                ViewValue = sdr["CycleIssue"].ToString()
                            }); break;
                        case "FinishGoods":
                            ContainerObj.Add(new DropDown
                            {
                                IdValue = sdr["UniqueNumber"].ToString(),
                                ViewValue = sdr["FinishGoods"].ToString()
                            }); break;
                        case "UniqueLine":
                            ContainerObj.Add(new DropDown
                            {
                                ExCore = sdr["ExCore"].ToString(),
                                IdValue = sdr["UniqueLineID"].ToString(),
                                ViewValue = sdr["UniqueLine"].ToString()
                            }); break;
                        case "Grouping":
                            ContainerObj.Add(new DropDown
                            {
                                IdValue = sdr["GroupingID"].ToString(),
                                ViewValue = sdr["Grouping"].ToString()
                            }); break;
                        case "RouteExCore":
                            ContainerObj.Add(new DropDown
                            {
								ExCore = sdr["ExCore"].ToString(),
                                IdValue = sdr["RouteCode"].ToString(),
                                ViewValue = sdr["RouteCode"].ToString()
                            }); break;
                        case "Building":
                            ContainerObj.Add(new DropDown
                            {
                                Dock = sdr["DockCode"].ToString(),
                                IdValue = sdr["BuildingID"].ToString(),
                                ViewValue = sdr["Building"].ToString()
                            }); break;
                    }
				}
				conn.Close();
			}
			return ContainerObj;
		}
	}
}
