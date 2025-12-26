using System.Collections.Generic;
using System.IO;
using System.Data;
using OfficeOpenXml;
using ExcelDataReader;

namespace UMTools.TBLExport
{
	public class ExportUtil
	{
		public static DataSet LoadXLSX(string filepath, bool use_header)
		{
			ExcelReaderConfiguration reader_config = new ExcelReaderConfiguration();

			ExcelDataSetConfiguration ds_config = new ExcelDataSetConfiguration()
			{
				ConfigureDataTable = ( _ ) => new ExcelDataTableConfiguration()
				{
					UseHeaderRow = use_header
				}
			};

			FileStream f_stream = File.Open( filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );			
			IExcelDataReader excel_reader = ExcelReaderFactory.CreateOpenXmlReader( f_stream );
			DataSet ds = excel_reader.AsDataSet( ds_config );
			excel_reader.Close();

			return ds;
		}
		static DataTable ExcelWorkSheet2DataTable( ExcelWorksheet ws, bool hasHeader )
		{
			DataTable dt = new DataTable( ws.Name );
			int totalRows = ws.Dimension.End.Row;
			int startRow = hasHeader ? 2 : 1;
			ExcelRange wsRow;
			DataRow dr;

			int excel_total_cols = ws.Dimension.End.Column;

			int totalCols = 0;
			foreach( var firstRowCell in ws.Cells[1, 1, 1, excel_total_cols] )
			{
				if( string.IsNullOrEmpty( firstRowCell.Text ) )
					continue;

				totalCols++;
				dt.Columns.Add( hasHeader ? firstRowCell.Text : string.Format( "Column {0}", firstRowCell.Start.Column ) );
			}

			for( int rowNum = startRow; rowNum <= totalRows; rowNum++ )
			{
				wsRow = ws.Cells[rowNum, 1, rowNum, totalCols];
				dr = dt.NewRow();
				foreach( var cell in wsRow )
				{
					dr[cell.Start.Column - 1] = cell.Text;
				}

				dt.Rows.Add( dr );
			}

			return dt;
		}
	}
}
