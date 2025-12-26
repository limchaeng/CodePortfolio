using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UMTools.Distribution
{
	public partial class FormExportPopup : Form
	{
		public class ExportOptionData
		{
			public string export_select_title;
			public string export_text;
		}

		List<ExportOptionData> mOptionList = null;

		public FormExportPopup()
		{
			InitializeComponent();

			tb_export_texts.Text = "";
		}

		
		public void SetExportText( string text )
		{
			ExportOptionData def_data = new ExportOptionData();
			def_data.export_select_title = "Default";
			def_data.export_text = text;

			List<ExportOptionData> list = new List<ExportOptionData>();
			list.Add( def_data );

			SetExporTextList( list );
		}
		public void SetExporTextList(List<ExportOptionData> option_data_list)
		{
			cb_export_options.Items.Clear();

			if( option_data_list == null || option_data_list.Count < 1 )
				return;

			mOptionList = option_data_list;
			foreach( ExportOptionData data in option_data_list )
			{
				cb_export_options.Items.Add( data.export_select_title );
			}

			cb_export_options.SelectedIndex = 0;
		}

		private void btn_close_Click( object sender, EventArgs e )
		{
			mOptionList = null;
			Close();
		}

		private void cb_export_options_SelectedIndexChanged( object sender, EventArgs e )
		{
			if( mOptionList == null )
				return;

			int idx = cb_export_options.SelectedIndex;
			if( idx < 0 || idx >= mOptionList.Count )
				return;

			ExportOptionData data = mOptionList[idx];
			tb_export_texts.Text = data.export_text;
		}

		private void btn_all_select_Click( object sender, EventArgs e )
		{
			tb_export_texts.SelectAll();
			tb_export_texts.Focus();
		}

		private void FormExportPopup_KeyDown( object sender, KeyEventArgs e )
		{
			if( e.Control && e.KeyCode == Keys.Escape )
				Close();
		}
	}
}
