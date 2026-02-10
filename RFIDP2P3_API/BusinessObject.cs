using OfficeOpenXml;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;

namespace RFIDP2P3_API
{
	public class BusinessObject
	{
		public void WriteLog(string strText, string strName) {/* Method statements here */
			StreamWriter sw = new StreamWriter("LOGS/"+strName  + ".txt", true);
			sw.WriteLine(strText);
			sw.Flush();
			sw.Close();

		}

		public string UploadXLS(ExcelPackage package, string UID, string configuration)
		{
			ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
			var rowcount = worksheet.Dimension.Rows;
			string? rem = "";
			using (SqlConnection conn = new SqlConnection(configuration))
			using (SqlCommand cmd = new SqlCommand("sp_DataExcel", conn))
			{
				cmd.CommandType = CommandType.StoredProcedure;

				cmd.Parameters.Add(new("@Entry_User", SqlDbType.VarChar, 50));
				cmd.Parameters.Add(new("@A1", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@B1", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@C1", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@D1", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@E1", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@F1", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@G1", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@H1", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@I1", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@J1", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@K1", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@L1", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@M1", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@N1", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@O1", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@P1", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@Q1", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@R1", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@S1", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@T1", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@U1", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@V1", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@W1", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@X1", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@Y1", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@Z1", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@A2", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@B2", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@C2", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@D2", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@E2", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@F2", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@G2", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@H2", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@I2", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@J2", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@K2", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@L2", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@M2", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@N2", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@O2", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@P2", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@Q2", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@R2", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@S2", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@T2", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@U2", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@V2", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@W2", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@X2", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@Y2", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@Z2", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@A3", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@B3", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@C3", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@D3", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@E3", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@F3", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@G3", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@H3", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@I3", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@J3", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@K3", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@L3", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@M3", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@N3", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@O3", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@P3", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@Q3", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@R3", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@S3", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@T3", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@U3", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@V3", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@W3", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@X3", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@Y3", SqlDbType.VarChar, 4000));
				cmd.Parameters.Add(new("@Z3", SqlDbType.VarChar, 4000));
				cmd.Parameters["@Entry_User"].Value = UID;
				conn.Open();

				SqlCommand cmd1 = new("DELETE DataExcel WHERE Entry_User = @Entry_User", conn);
				cmd1.CommandType = CommandType.Text;
				cmd1.Parameters.Add(new("@Entry_User", UID));
				 cmd1.ExecuteScalar();
				//WriteLog(rem, "rowcount");

				for (int row = 1; row <= rowcount; row++)
				{

					cmd.Parameters["@A1"].Value = "" + worksheet.Cells[row, 1].Value?.ToString().Trim();
					cmd.Parameters["@B1"].Value = "" + worksheet.Cells[row, 2].Value?.ToString().Trim();
					cmd.Parameters["@C1"].Value = "" + worksheet.Cells[row, 3].Value?.ToString().Trim();
					cmd.Parameters["@D1"].Value = "" + worksheet.Cells[row, 4].Value?.ToString().Trim();
					cmd.Parameters["@E1"].Value = "" + worksheet.Cells[row, 5].Value?.ToString().Trim();
					cmd.Parameters["@F1"].Value = "" + worksheet.Cells[row, 6].Value?.ToString().Trim();
					cmd.Parameters["@G1"].Value = "" + worksheet.Cells[row, 7].Value?.ToString().Trim();
					cmd.Parameters["@H1"].Value = "" + worksheet.Cells[row, 8].Value?.ToString().Trim();
					cmd.Parameters["@I1"].Value = "" + worksheet.Cells[row, 9]?.Value?.ToString().Trim();
					cmd.Parameters["@J1"].Value = "" + worksheet.Cells[row, 10].Value?.ToString().Trim();
					cmd.Parameters["@K1"].Value = "" + worksheet.Cells[row, 11].Value?.ToString().Trim();
					cmd.Parameters["@L1"].Value = "" + worksheet.Cells[row, 12].Value?.ToString().Trim();
					cmd.Parameters["@M1"].Value = "" + worksheet.Cells[row, 13].Value?.ToString().Trim();
					cmd.Parameters["@N1"].Value = "" + worksheet.Cells[row, 14].Value?.ToString().Trim();
					cmd.Parameters["@O1"].Value = "" + worksheet.Cells[row, 15].Value?.ToString().Trim();
					cmd.Parameters["@P1"].Value = "" + worksheet.Cells[row, 16].Value?.ToString().Trim();
					cmd.Parameters["@Q1"].Value = "" + worksheet.Cells[row, 17].Value?.ToString().Trim();
					cmd.Parameters["@R1"].Value = "" + worksheet.Cells[row, 18].Value?.ToString().Trim();
					cmd.Parameters["@S1"].Value = "" + worksheet.Cells[row, 19].Value?.ToString().Trim();
					cmd.Parameters["@T1"].Value = "" + worksheet.Cells[row, 20].Value?.ToString().Trim();
					cmd.Parameters["@U1"].Value = "" + worksheet.Cells[row, 21].Value?.ToString().Trim();
					cmd.Parameters["@V1"].Value = "" + worksheet.Cells[row, 22].Value?.ToString().Trim();
					cmd.Parameters["@W1"].Value = "" + worksheet.Cells[row, 23].Value?.ToString().Trim();
					cmd.Parameters["@X1"].Value = "" + worksheet.Cells[row, 24].Value?.ToString().Trim();
					cmd.Parameters["@Y1"].Value = "" + worksheet.Cells[row, 25].Value?.ToString().Trim();
					cmd.Parameters["@Z1"].Value = "" + worksheet.Cells[row, 26].Value?.ToString().Trim();
					cmd.Parameters["@A2"].Value = "" + worksheet.Cells[row, 27].Value?.ToString().Trim();
					cmd.Parameters["@B2"].Value = "" + worksheet.Cells[row, 28].Value?.ToString().Trim();
					cmd.Parameters["@C2"].Value = "" + worksheet.Cells[row, 29].Value?.ToString().Trim();
					cmd.Parameters["@D2"].Value = "" + worksheet.Cells[row, 30].Value?.ToString().Trim();
					cmd.Parameters["@E2"].Value = "" + worksheet.Cells[row, 31].Value?.ToString().Trim();
					cmd.Parameters["@F2"].Value = "" + worksheet.Cells[row, 32].Value?.ToString().Trim();
					cmd.Parameters["@G2"].Value = "" + worksheet.Cells[row, 33].Value?.ToString().Trim();
					cmd.Parameters["@H2"].Value = "" + worksheet.Cells[row, 34].Value?.ToString().Trim();
					cmd.Parameters["@I2"].Value = "" + worksheet.Cells[row, 35].Value?.ToString().Trim();
					cmd.Parameters["@J2"].Value = "" + worksheet.Cells[row, 36].Value?.ToString().Trim();
					cmd.Parameters["@K2"].Value = "" + worksheet.Cells[row, 37].Value?.ToString().Trim();
					cmd.Parameters["@L2"].Value = "" + worksheet.Cells[row, 38].Value?.ToString().Trim();
					cmd.Parameters["@M2"].Value = "" + worksheet.Cells[row, 39].Value?.ToString().Trim();
					cmd.Parameters["@N2"].Value = "" + worksheet.Cells[row, 40].Value?.ToString().Trim();
					cmd.Parameters["@O2"].Value = "" + worksheet.Cells[row, 41].Value?.ToString().Trim();
					cmd.Parameters["@P2"].Value = "" + worksheet.Cells[row, 42].Value?.ToString().Trim();
					cmd.Parameters["@Q2"].Value = "" + worksheet.Cells[row, 43].Value?.ToString().Trim();
					cmd.Parameters["@R2"].Value = "" + worksheet.Cells[row, 44].Value?.ToString().Trim();
					cmd.Parameters["@S2"].Value = "" + worksheet.Cells[row, 45].Value?.ToString().Trim();
					cmd.Parameters["@T2"].Value = "" + worksheet.Cells[row, 46].Value?.ToString().Trim();
					cmd.Parameters["@U2"].Value = "" + worksheet.Cells[row, 47].Value?.ToString().Trim();
					cmd.Parameters["@V2"].Value = "" + worksheet.Cells[row, 48].Value?.ToString().Trim();
					cmd.Parameters["@W2"].Value = "" + worksheet.Cells[row, 49].Value?.ToString().Trim();
					cmd.Parameters["@X2"].Value = "" + worksheet.Cells[row, 50].Value?.ToString().Trim();
					cmd.Parameters["@Y2"].Value = "" + worksheet.Cells[row, 51].Value?.ToString().Trim();
					cmd.Parameters["@Z2"].Value = "" + worksheet.Cells[row, 52].Value?.ToString().Trim();
					cmd.Parameters["@A3"].Value = "" + worksheet.Cells[row, 53].Value?.ToString().Trim();
					cmd.Parameters["@B3"].Value = "" + worksheet.Cells[row, 54].Value?.ToString().Trim();
					cmd.Parameters["@C3"].Value = "" + worksheet.Cells[row, 55].Value?.ToString().Trim();
					cmd.Parameters["@D3"].Value = "" + worksheet.Cells[row, 56].Value?.ToString().Trim();
					cmd.Parameters["@E3"].Value = "" + worksheet.Cells[row, 57].Value?.ToString().Trim();
					cmd.Parameters["@F3"].Value = "" + worksheet.Cells[row, 58].Value?.ToString().Trim();
					cmd.Parameters["@G3"].Value = "" + worksheet.Cells[row, 59].Value?.ToString().Trim();
					cmd.Parameters["@H3"].Value = "" + worksheet.Cells[row, 60].Value?.ToString().Trim();
					cmd.Parameters["@I3"].Value = "" + worksheet.Cells[row, 61].Value?.ToString().Trim();
					cmd.Parameters["@J3"].Value = "" + worksheet.Cells[row, 62].Value?.ToString().Trim();
					cmd.Parameters["@K3"].Value = "" + worksheet.Cells[row, 63].Value?.ToString().Trim();
					cmd.Parameters["@L3"].Value = "" + worksheet.Cells[row, 64].Value?.ToString().Trim();
					cmd.Parameters["@M3"].Value = "" + worksheet.Cells[row, 65].Value?.ToString().Trim();
					cmd.Parameters["@N3"].Value = "" + worksheet.Cells[row, 66].Value?.ToString().Trim();
					cmd.Parameters["@O3"].Value = "" + worksheet.Cells[row, 67].Value?.ToString().Trim();
					cmd.Parameters["@P3"].Value = "" + worksheet.Cells[row, 68].Value?.ToString().Trim();
					cmd.Parameters["@Q3"].Value = "" + worksheet.Cells[row, 69].Value?.ToString().Trim();
					cmd.Parameters["@R3"].Value = "" + worksheet.Cells[row, 70].Value?.ToString().Trim();
					cmd.Parameters["@S3"].Value = "" + worksheet.Cells[row, 71].Value?.ToString().Trim();
					cmd.Parameters["@T3"].Value = "" + worksheet.Cells[row, 72].Value?.ToString().Trim();
					cmd.Parameters["@U3"].Value = "" + worksheet.Cells[row, 73].Value?.ToString().Trim();
					cmd.Parameters["@V3"].Value = "" + worksheet.Cells[row, 74].Value?.ToString().Trim();
					cmd.Parameters["@W3"].Value = "" + worksheet.Cells[row, 75].Value?.ToString().Trim();
					cmd.Parameters["@X3"].Value = "" + worksheet.Cells[row, 76].Value?.ToString().Trim();
					cmd.Parameters["@Y3"].Value = "" + worksheet.Cells[row, 77].Value?.ToString().Trim();
					cmd.Parameters["@Z3"].Value = "" + worksheet.Cells[row, 78].Value?.ToString().Trim();
					try
					{
						cmd.ExecuteNonQuery();
					}
					catch (Exception e)
					{
						WriteLog(e.Message, "DBError");
						return "Error Execute Data Base.";
					}

				}
				conn.Close();
			}
			return "success";
		}
	}
}
