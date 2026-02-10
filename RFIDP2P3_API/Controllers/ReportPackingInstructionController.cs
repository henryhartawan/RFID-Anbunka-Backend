using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using RFIDP2P3_API.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.NetworkInformation;
using SD = System.Drawing;

namespace RFIDP2P3_API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ReportPackingInstructionController : ControllerBase
    {
        private readonly string _configuration;
        private readonly IWebHostEnvironment _env;

        public ReportPackingInstructionController(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration.GetConnectionString("DefaultConnection");
            _env = env;
        }

        [HttpPost]
        public ActionResult<IEnumerable<Dictionary<string, object>>> INQ(ReportPackingInstruction pi)
        {
            var dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Inq_Report_Packing_Instruction", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@Periode_ID", pi.Periode));
                cmd.Parameters.Add(new("@SupplierCode", pi.SupplierCode));
                conn.Open();

                using (var da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }

                conn.Close();
            }

            var result = new List<Dictionary<string, object>>();

            foreach (DataRow row in dt.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                result.Add(dict);
            }

            return result;
        }

        [HttpPost]
        public IActionResult PrintExcel(ReportPackingInstruction pi)
        {
            var dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(_configuration))
            using (SqlCommand cmd = new SqlCommand("sp_Inq_Report_Packing_Instruction", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new("@Periode_ID", pi.Periode));
                cmd.Parameters.Add(new("@SupplierCode", pi.SupplierCode));
                conn.Open();

                using (var da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }

                conn.Close();
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Packing Instruction");
                var range = ws.Cells["A:R"];

                ws.Column(1).Width = 1;
                ws.Column(2).Width = 5.15;
                ws.Column(3).Width = 16.4;
                ws.Column(4).Width = 8.4;
                ws.Column(5).Width = 8.6;
                ws.Column(6).Width = 15.15;
                ws.Column(7).Width = 14.75;
                ws.Column(8).Width = 14.4;
                ws.Column(9).Width = 7.6;
                ws.Column(10).Width = 8.15;
                ws.Column(11).Width = 10.85;
                ws.Column(12).Width = 9.75;
                ws.Column(13).Width = 12.41;
                ws.Column(14).Width = 6.75;
                ws.Column(15).Width = 10.25;
                ws.Column(16).Width = 9.85;
                ws.Column(17).Width = 5.85;
                ws.Column(18).Width = 5.15;

                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFFFFF"));

                var logoPath = Path.Combine(_env.WebRootPath, "img", "logo.png");
                var fileInfo = new FileInfo(logoPath);

                int rr = 1;
                string sup = "";
                string route = "";
                string orderDate = "";
                string orderTime = "";
                string cycleIssue = "";
                string delvDate = "";
                string delvTime = "";
                string cycleArr = "";
                string piNo = "";
                string pino = "";
                string poNo = "";
                string dock = "";
                string dns = "";
                int page = 0;
                int tot = 0;
                int no = 0;
                int skid = 0;
                int box = 0;

                for (int r = 0; r < dt.Rows.Count; r++)
                {
                    if (sup != dt.Rows[r][0].ToString() || route != dt.Rows[r][2].ToString() ||
                        orderDate != dt.Rows[r][3].ToString() ||  orderTime != dt.Rows[r][4].ToString() ||
                        cycleIssue != dt.Rows[r][5].ToString() || delvDate != dt.Rows[r][6].ToString() ||
                        delvTime != dt.Rows[r][7].ToString() || cycleArr != dt.Rows[r][8].ToString() ||
                        piNo != dt.Rows[r][9].ToString() || poNo != dt.Rows[r][10].ToString() ||
                        dock != dt.Rows[r][11].ToString())
                    {
                        sup = dt.Rows[r][0].ToString();
                        route = dt.Rows[r][2].ToString();
                        orderDate = dt.Rows[r][3].ToString();
                        orderTime = dt.Rows[r][4].ToString();
                        cycleIssue = dt.Rows[r][5].ToString();
                        delvDate = dt.Rows[r][6].ToString();
                        delvTime = dt.Rows[r][7].ToString();
                        cycleArr = dt.Rows[r][8].ToString();
                        piNo = dt.Rows[r][9].ToString();
                        poNo = dt.Rows[r][10].ToString();
                        dock = dt.Rows[r][11].ToString();
                        tot++;
                    }
                }

                sup = "";
                route = "";
                orderDate = "";
                orderTime = "";
                cycleIssue = "";
                delvDate = "";
                delvTime = "";
                cycleArr = "";
                piNo = "";
                poNo = "";
                dock = "";
                for (int r = 0; r < dt.Rows.Count; r++)
                {
                    if (sup != dt.Rows[r][0].ToString() || route != dt.Rows[r][2].ToString() || 
                        orderDate != dt.Rows[r][3].ToString()|| orderTime != dt.Rows[r][4].ToString() ||
                        cycleIssue != dt.Rows[r][5].ToString() || delvDate != dt.Rows[r][6].ToString() ||
                        delvTime != dt.Rows[r][7].ToString() || cycleArr != dt.Rows[r][8].ToString() ||
                        piNo != dt.Rows[r][9].ToString() || poNo != dt.Rows[r][10].ToString() ||
                        dock != dt.Rows[r][11].ToString())
                    {
                        if (sup != "" || route != "" || orderDate != "" || orderTime != "" ||
                            cycleIssue != "" || delvDate != "" || delvTime != "" || cycleArr != "" ||
                            piNo != "" || poNo != "" || dock != "") {
                            rr += 2;
                            ws.Cells["B" + rr.ToString() + ":C" + rr.ToString()].Merge = true;
                            range = ws.Cells["B" + rr.ToString() + ":D" + rr.ToString()];
                            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Top.Color.SetColor(ColorTranslator.FromHtml("#000000"));
                            range.Style.Border.Bottom.Color.SetColor(ColorTranslator.FromHtml("#000000"));
                            range.Style.Border.Left.Color.SetColor(ColorTranslator.FromHtml("#000000"));
                            range.Style.Border.Right.Color.SetColor(ColorTranslator.FromHtml("#000000"));

                            ws.Cells["B" + rr.ToString()].Value = "Total Module/Skid";
                            ws.Cells["D" + rr.ToString()].Value = skid;


                            rr++;
                            ws.Cells["B" + rr.ToString() + ":C" + rr.ToString()].Merge = true;
                            range = ws.Cells["B" + rr.ToString() + ":D" + rr.ToString()];
                            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Top.Color.SetColor(ColorTranslator.FromHtml("#000000"));
                            range.Style.Border.Bottom.Color.SetColor(ColorTranslator.FromHtml("#000000"));
                            range.Style.Border.Left.Color.SetColor(ColorTranslator.FromHtml("#000000"));
                            range.Style.Border.Right.Color.SetColor(ColorTranslator.FromHtml("#000000"));

                            ws.Cells["B" + rr.ToString()].Value = "Total Box";
                            ws.Cells["D" + rr.ToString()].Value = box;

                            rr += 5;

                            using (SqlConnection conn = new SqlConnection(_configuration))
                            using (SqlCommand cmd = new SqlCommand("sp_Submit_Report_Packing_Instruction", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add(new("@PI_No", pino));
                                cmd.Parameters.Add(new("@DN_No", dns));
                                cmd.Parameters.Add(new("@Supplier_Code", sup));
                                cmd.Parameters.Add(new("@RouteCode", route));
                                cmd.Parameters.Add(new("@CycleIssue", cycleIssue));
                                cmd.Parameters.Add(new("@DeliveryDate", delvDate));
                                cmd.Parameters.Add(new("@TimeArrival", delvTime));
                                cmd.Parameters.Add(new("@CycleArrival", cycleArr));
                                cmd.Parameters.Add(new("@PO_No", poNo));
                                cmd.Parameters.Add(new("@DockCode", dock));

                                conn.Open();
                                cmd.ExecuteNonQuery();
                                conn.Close();
                            }
                            pino = "";
                        }

                        sup = dt.Rows[r][0].ToString();
                        route = dt.Rows[r][2].ToString();
                        orderDate = dt.Rows[r][3].ToString();
                        orderTime = dt.Rows[r][4].ToString();
                        cycleIssue = dt.Rows[r][5].ToString();
                        delvDate = dt.Rows[r][6].ToString();
                        delvTime = dt.Rows[r][7].ToString();
                        cycleArr = dt.Rows[r][8].ToString();
                        piNo = dt.Rows[r][9].ToString();
                        poNo = dt.Rows[r][10].ToString();
                        dock = dt.Rows[r][11].ToString();
                        page++;
                        no = 0;
                        skid = 0;
                        box = 0;
                        dns = "";

                        if (piNo == "" && pino == "")
                        {
                            using (SqlConnection conn = new SqlConnection(_configuration))
                            using (SqlCommand cmd = new SqlCommand("sp_Inq_PI_No", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                                cmd.Parameters.Add(new("@Periode_ID", pi.Periode));

                                conn.Open();
                                cmd.ExecuteNonQuery();

                                pino = Convert.ToString(cmd.Parameters["@Remarks"].Value);

                                conn.Close();
                            }
                        }
                        else if (piNo != "")
                        {
                            pino = piNo;
                        }

                        var picture = ws.Drawings.AddPicture($"Logo_{r}", fileInfo);
                        picture.SetPosition(rr, 0, 1, 0);
                        picture.SetSize(195, 56);

                        ws.Row(rr).Height = 6.75;

                        rr++;
                        ws.Cells["B" + rr.ToString() + ":D" + (rr + 3).ToString()].Merge = true;

                        ws.Cells["F" + rr.ToString() + ":N" + (rr + 3).ToString()].Merge = true;
                        ws.Cells["F" + rr.ToString()].Value = "PACKING INSTRUCTION";
                        ws.Cells["F" + rr.ToString() + ":N" + (rr + 3).ToString()].Style.Font.Size = 36;
                        ws.Cells["F" + rr.ToString() + ":N" + (rr + 3).ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells["F" + rr.ToString() + ":N" + (rr + 3).ToString()].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                        ws.Cells["O" + rr.ToString() + ":R" + rr.ToString()].Merge = true;
                        ws.Cells["O" + rr.ToString()].Value = "PT. ASTRA DAIHATSU MOTOR";
                        ws.Cells["O" + rr.ToString() + ":R" + rr.ToString()].Style.Font.Bold = true;


                        rr++;
                        ws.Cells["O" + rr.ToString() + ":R" + rr.ToString()].Merge = true;
                        ws.Cells["O" + rr.ToString()].Value = "JL. MALIGI VI-M6, KIIC";


                        rr++;
                        ws.Cells["O" + rr.ToString() + ":R" + rr.ToString()].Merge = true;
                        ws.Cells["O" + rr.ToString()].Value = "TOL JAKARTA - CIKAMPEK KM 47,5";


                        rr++;
                        ws.Cells["O" + rr.ToString() + ":R" + rr.ToString()].Merge = true;
                        ws.Cells["O" + rr.ToString()].Value = "KARAWANG - JAWA BARAT 41361";

                        range = ws.Cells["B" + rr.ToString() + ":R" + rr.ToString()];
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Color.SetColor(ColorTranslator.FromHtml("#000000"));


                        rr += 2;
                        ws.Cells["B" + rr.ToString() + ":C" + rr.ToString()].Merge = true;
                        ws.Cells["B" + rr.ToString()].Value = "SUPPLIER CODE";
                        ws.Cells["B" + rr.ToString() + ":C" + rr.ToString()].Style.Font.Bold = true;

                        ws.Cells["D" + rr.ToString() + ":F" + rr.ToString()].Merge = true;
                        ws.Cells["D" + rr.ToString()].Value = ": " + dt.Rows[r][0];

                        ws.Cells["G" + rr.ToString() + ":J" + rr.ToString()].Merge = true;
                        ws.Cells["G" + rr.ToString()].Value = "ORDER";
                        ws.Cells["G" + rr.ToString()].Style.Font.Bold = true;

                        ws.Cells["K" + rr.ToString() + ":N" + rr.ToString()].Merge = true;
                        ws.Cells["K" + rr.ToString()].Value = "DELIVERY";
                        ws.Cells["K" + rr.ToString()].Style.Font.Bold = true;

                        ws.Cells["O" + rr.ToString()].Value = "P/I NO";
                        ws.Cells["O" + rr.ToString()].Style.Font.Bold = true;
                        ws.Cells["P" + rr.ToString() + ":R" + rr.ToString()].Merge = true;
                        ws.Cells["P" + rr.ToString()].Value = ": " + pino.ToString();


                        rr++;
                        ws.Cells["B" + rr.ToString() + ":C" + rr.ToString()].Merge = true;
                        ws.Cells["B" + rr.ToString()].Value = "SUPPLIER NAME";
                        ws.Cells["B" + rr.ToString() + ":C" + rr.ToString()].Style.Font.Bold = true;

                        ws.Cells["D" + rr.ToString() + ":F" + rr.ToString()].Merge = true;
                        ws.Cells["D" + rr.ToString()].Value = ": " + dt.Rows[r][1];

                        ws.Cells["G" + rr.ToString()].Value = "Date";
                        ws.Cells["H" + rr.ToString() + ":J" + rr.ToString()].Merge = true;
                        ws.Cells["H" + rr.ToString()].Value = ": " + dt.Rows[r][3];

                        ws.Cells["K" + rr.ToString()].Value = "Date";
                        ws.Cells["L" + rr.ToString() + ":N" + rr.ToString()].Merge = true;
                        ws.Cells["L" + rr.ToString()].Value = ": " + dt.Rows[r][6];

                        ws.Cells["O" + rr.ToString()].Value = "PAGE";
                        ws.Cells["P" + rr.ToString() + ":R" + rr.ToString()].Merge = true;
                        ws.Cells["P" + rr.ToString()].Value = ": " + page.ToString() + " / " + tot.ToString();


                        rr++;
                        ws.Cells["B" + rr.ToString() + ":C" + rr.ToString()].Merge = true;
                        ws.Cells["B" + rr.ToString()].Value = "ROUTE";
                        ws.Cells["B" + rr.ToString() + ":C" + rr.ToString()].Style.Font.Bold = true;

                        ws.Cells["D" + rr.ToString() + ":F" + rr.ToString()].Merge = true;
                        ws.Cells["D" + rr.ToString()].Value = ": " + dt.Rows[r][2];

                        ws.Cells["G" + rr.ToString()].Value = "Time";
                        ws.Cells["H" + rr.ToString() + ":J" + rr.ToString()].Merge = true;
                        ws.Cells["H" + rr.ToString()].Value = ": " + dt.Rows[r][4];

                        ws.Cells["K" + rr.ToString()].Value = "Time";
                        ws.Cells["L" + rr.ToString() + ":N" + rr.ToString()].Merge = true;
                        ws.Cells["L" + rr.ToString()].Value = ": " + dt.Rows[r][7];

                        ws.Cells["O" + rr.ToString()].Value = "PO NO";
                        ws.Cells["P" + rr.ToString() + ":R" + rr.ToString()].Merge = true;
                        ws.Cells["P" + rr.ToString()].Value = ": " + dt.Rows[r][10];


                        rr++;
                        ws.Cells["G" + rr.ToString()].Value = "Cycle Issue";
                        ws.Cells["H" + rr.ToString() + ":J" + rr.ToString()].Merge = true;
                        ws.Cells["H" + rr.ToString()].Value = ": " + dt.Rows[r][5];

                        ws.Cells["K" + rr.ToString()].Value = "Cycle";
                        ws.Cells["L" + rr.ToString() + ":N" + rr.ToString()].Merge = true;
                        ws.Cells["L" + rr.ToString()].Value = ": " + dt.Rows[r][8];

                        ws.Cells["O" + rr.ToString()].Value = "Dock";
                        ws.Cells["P" + rr.ToString() + ":R" + rr.ToString()].Merge = true;
                        ws.Cells["P" + rr.ToString()].Value = ": " + dt.Rows[r][11];


                        rr = rr + 2;
                        ws.Row(rr).Height = 27;

                        range = ws.Cells["B" + rr.ToString() + ":R" + rr.ToString()];
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Top.Color.SetColor(ColorTranslator.FromHtml("#000000"));
                        range.Style.Border.Bottom.Color.SetColor(ColorTranslator.FromHtml("#000000"));
                        range.Style.Border.Left.Color.SetColor(ColorTranslator.FromHtml("#000000"));
                        range.Style.Border.Right.Color.SetColor(ColorTranslator.FromHtml("#000000"));
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FFC000"));
                        range.Style.WrapText = true;

                        ws.Cells["B" + rr.ToString() + ":R" + rr.ToString()].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells["B" + rr.ToString() + ":R" + rr.ToString()].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                        ws.Cells["B" + rr.ToString()].Value = "No.";
                        ws.Cells["C" + rr.ToString()].Value = "DN";
                        ws.Cells["D" + rr.ToString()].Value = "Line Code";
                        ws.Cells["E" + rr.ToString()].Value = "Excore";
                        ws.Cells["F" + rr.ToString()].Value = "Part No.";
                        ws.Cells["G" + rr.ToString() + ":H" + rr.ToString()].Merge = true;
                        ws.Cells["G" + rr.ToString()].Value = "Part Name";
                        ws.Cells["I" + rr.ToString()].Value = "Job No";
                        ws.Cells["J" + rr.ToString()].Value = "Qty/Box";
                        ws.Cells["K" + rr.ToString()].Value = "Qty Order (Kanban)";
                        ws.Cells["L" + rr.ToString()].Value = "Qty Order (Pcs)";
                        ws.Cells["M" + rr.ToString()].Value = "Packaging Group";
                        ws.Cells["N" + rr.ToString()].Value = "Lane No.";
                        ws.Cells["O" + rr.ToString()].Value = "Sector";
                        ws.Cells["P" + rr.ToString()].Value = "Rack Number";
                        ws.Cells["Q" + rr.ToString()].Value = "Layer";
                        ws.Cells["R" + rr.ToString()].Value = "Hole";
                    }

                    no++;
                    rr++;
                    range = ws.Cells["B" + rr.ToString() + ":R" + rr.ToString()];
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Top.Color.SetColor(ColorTranslator.FromHtml("#000000"));
                    range.Style.Border.Bottom.Color.SetColor(ColorTranslator.FromHtml("#000000"));
                    range.Style.Border.Left.Color.SetColor(ColorTranslator.FromHtml("#000000"));
                    range.Style.Border.Right.Color.SetColor(ColorTranslator.FromHtml("#000000"));
                    ws.Cells["B" + rr.ToString()].Value = no;
                    ws.Cells["C" + rr.ToString()].Value = dt.Rows[r][12].ToString();
                    ws.Cells["D" + rr.ToString()].Value = dt.Rows[r][13].ToString();
                    ws.Cells["E" + rr.ToString()].Value = dt.Rows[r][14].ToString();
                    ws.Cells["F" + rr.ToString()].Value = dt.Rows[r][15].ToString();
                    ws.Cells["G" + rr.ToString() + ":H" + rr.ToString()].Merge = true;
                    ws.Cells["G" + rr.ToString()].Value = dt.Rows[r][16].ToString();
                    ws.Cells["I" + rr.ToString()].Value = dt.Rows[r][17].ToString();
                    ws.Cells["J" + rr.ToString()].Value = dt.Rows[r][18].ToString();
                    ws.Cells["K" + rr.ToString()].Value = dt.Rows[r][19].ToString();
                    ws.Cells["L" + rr.ToString()].Value = dt.Rows[r][20].ToString();
                    ws.Cells["M" + rr.ToString()].Value = dt.Rows[r][21].ToString();
                    ws.Cells["N" + rr.ToString()].Value = dt.Rows[r][22].ToString();
                    ws.Cells["O" + rr.ToString()].Value = dt.Rows[r][23].ToString();
                    ws.Cells["P" + rr.ToString()].Value = dt.Rows[r][24].ToString();
                    ws.Cells["Q" + rr.ToString()].Value = dt.Rows[r][25].ToString();
                    ws.Cells["R" + rr.ToString()].Value = dt.Rows[r][26].ToString();

                    skid += Convert.ToInt32(dt.Rows[r][27]);
                    box += Convert.ToInt32(dt.Rows[r][19]);
                    dns += ";" + dt.Rows[r][12].ToString();
                }

                rr += 2;
                ws.Cells["B" + rr.ToString() + ":C" + rr.ToString()].Merge = true;
                range = ws.Cells["B" + rr.ToString() + ":D" + rr.ToString()];
                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Top.Color.SetColor(ColorTranslator.FromHtml("#000000"));
                range.Style.Border.Bottom.Color.SetColor(ColorTranslator.FromHtml("#000000"));
                range.Style.Border.Left.Color.SetColor(ColorTranslator.FromHtml("#000000"));
                range.Style.Border.Right.Color.SetColor(ColorTranslator.FromHtml("#000000"));

                ws.Cells["B" + rr.ToString()].Value = "Total Module/Skid";
                ws.Cells["D" + rr.ToString()].Value = skid;


                rr++;
                ws.Cells["B" + rr.ToString() + ":C" + rr.ToString()].Merge = true;
                range = ws.Cells["B" + rr.ToString() + ":D" + rr.ToString()];
                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Top.Color.SetColor(ColorTranslator.FromHtml("#000000"));
                range.Style.Border.Bottom.Color.SetColor(ColorTranslator.FromHtml("#000000"));
                range.Style.Border.Left.Color.SetColor(ColorTranslator.FromHtml("#000000"));
                range.Style.Border.Right.Color.SetColor(ColorTranslator.FromHtml("#000000"));

                ws.Cells["B" + rr.ToString()].Value = "Total Box";
                ws.Cells["D" + rr.ToString()].Value = box;

                using (SqlConnection conn = new SqlConnection(_configuration))
                using (SqlCommand cmd = new SqlCommand("sp_Submit_Report_Packing_Instruction", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new("@PI_No", pino));
                    cmd.Parameters.Add(new("@DN_No", dns));
                    cmd.Parameters.Add(new("@Supplier_Code", sup));
                    cmd.Parameters.Add(new("@RouteCode", route));
                    cmd.Parameters.Add(new("@CycleIssue", cycleIssue));
                    cmd.Parameters.Add(new("@DeliveryDate", delvDate));
                    cmd.Parameters.Add(new("@TimeArrival", delvTime));
                    cmd.Parameters.Add(new("@CycleArrival", cycleArr));
                    cmd.Parameters.Add(new("@PO_No", poNo));
                    cmd.Parameters.Add(new("@DockCode", dock));

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                var excelBytes = package.GetAsByteArray();
                var base64 = Convert.ToBase64String(excelBytes);

                return Ok(new
                {
                    op = "ok",
                    file = "data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64," + base64
                });
            }
        }
    }
}
