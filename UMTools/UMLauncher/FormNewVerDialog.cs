using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UMTools.UMLauncher
{
	public partial class FormNewVerDialog : Form
	{
		public FormNewVerDialog()
		{
			InitializeComponent();
			this.DialogResult = DialogResult.Cancel;
		}

		//------------------------------------------------------------------------
		public void SetVersion( CheckNewVer.VersionData v_data, bool download_enable = true )
		{
			if( download_enable )
			{
				tb_newver_dialog_info.Text = string.Format( "새로운 {0} 버전({1})이 있습니다.다운로드 및 설치 하시겠습니까?", v_data.type_name, v_data.file_name );
				btn_newver_dialog_install.Text = "다운로드 및 설치";
			}
			else
			{
				tb_newver_dialog_info.Text = string.Format( " {0} 버전({1}) 변경사항", v_data.type_name, v_data.file_name );
				btn_newver_dialog_install.Text = "확인";
			}

			rtb_newver_dialog_changedlog.Text = v_data.changed_log;

			tb_newver_dialog_info.Select( 0, 0 );
		}


		private void btn_newver_dialog_cancel_Click( object sender, EventArgs e )
		{
			this.DialogResult = DialogResult.Cancel;
			Close();
		}

		private void btn_newver_dialog_install_Click( object sender, EventArgs e )
		{
			this.DialogResult = DialogResult.OK;
			Close();
		}
	}
}
