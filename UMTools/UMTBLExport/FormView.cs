using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UMTools.TBLExport
{
	public partial class FormView : Form
	{
		public FormView()
		{
			InitializeComponent();
			rtb_textview.Text = mText;
		}

		string mText = "";
		public void SetText(string text)
		{
			mText = text;
			rtb_textview.Text = mText;
		}

		private void btn_close_Click( object sender, EventArgs e )
		{
			Close();
		}

		private void btn_all_select_Click( object sender, EventArgs e )
		{
			rtb_textview.SelectAll();
			rtb_textview.Focus();
		}

		private void cb_wordwrap_CheckedChanged( object sender, EventArgs e )
		{
			if( cb_wordwrap.Checked )
			{
				rtb_textview.WordWrap = true;
			}
			else
			{
				rtb_textview.WordWrap = false;
			}
		}
	}
}
