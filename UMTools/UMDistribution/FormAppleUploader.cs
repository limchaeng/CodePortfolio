using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Renci.SshNet;
using Renci.SshNet.Async;
using UMTools.Common;
using System.IO;
using UMF.Core;

namespace UMTools.Distribution
{
	public partial class FormAppleUploader : Form
	{
		public class LoaderData
		{
			public string SSH_HOST = "";
			public string SSH_ID = "";
			public string SSH_PW = "";
			public string work_path = "";
		}

		public class IPAData
		{
			public string full = "";
			public string file_name = "";
			public System.Version version = null;
			public int revision = 0;
			public int build_num = 0;

			public IPAData(string _full)
			{
				this.full = _full;

				try
				{
					int end_idx = _full.LastIndexOf( '/' );
					if( end_idx == -1 )
						end_idx = _full.LastIndexOf( ' ' );

					if( end_idx != -1 )
						file_name = _full.Substring( end_idx + 1, _full.Length - end_idx - 1 );

					string[] s_split = Path.GetFileNameWithoutExtension( file_name ).Split( '_' );
					if( s_split != null && s_split.Length >= 5 )
					{
						version = StringUtil.SafeParse<Version>( s_split[1].Trim(), new Version( 0, 0, 0, 0 ) );
						revision = StringUtil.SafeParse<int>( s_split[3].Trim().Replace( "r", "" ), 0 );
						build_num = StringUtil.SafeParse<int>( s_split[4].Trim().Replace( "b", "" ), 0 );
					}
				}
				catch (System.Exception ex)
				{
					
				}
			}
		}

		const string REG_KEY_LAST_APPLE_ID = "_LastAppleID_";

		LoaderData mData = null;
		SshClient mSSHClient = null;

		LogWriter mLog;

		public FormAppleUploader( LoaderData loader_data )
		{
			mData = loader_data;
			InitializeComponent();

			mLog = new LogWriter( rtb_altools_log );

			tb_work_path.Text = mData.work_path;
			tb_apple_id.Text = ToolUtil.GetPrefs<string>( FormDistribution.REG_SUB_KEY, REG_KEY_LAST_APPLE_ID, "" );
			tb_apple_pw.Text = "";
		}

		bool DoSSHConnect()
		{
			ConnectionInfo c_info = new ConnectionInfo( mData.SSH_HOST, mData.SSH_ID,
				new PasswordAuthenticationMethod( mData.SSH_ID, mData.SSH_PW ) );

			mSSHClient = new SshClient( c_info );
			mSSHClient.Connect();

			if( mSSHClient.IsConnected == false )
			{
				mLog.LogWrite( "SSH({0}) connect failed!", mData.SSH_HOST );
				return false;
			}				
			else
			{
				mLog.LogWrite( "SSH({0}) connected!", mData.SSH_HOST );
				return true;
			}
		}

		void ClearAndClose()
		{
			if( mSSHClient != null )
			{
				if( mSSHClient.IsConnected )
				{
					mSSHClient.Disconnect();
				}

				mSSHClient.Dispose();
			}

			Close();
		}

		bool CheckConnection()
		{
			if( mSSHClient == null || mSSHClient.IsConnected == false )
			{
				mLog.LogWrite( "SSH Client Not Connected!" );
				return false;
			}

			return true;
		}			

		List<string> DoSSDCommand(string command, ref string end_result)
		{
			Cursor.Current = Cursors.WaitCursor;

			bool connectd = CheckConnection();
			if( connectd == false )
				connectd = DoSSHConnect();

			List<string> output_list = new List<string>();
 			using( SshCommand cmd = mSSHClient.CreateCommand( command ) )
 			{
				var result = cmd.BeginExecute();

				using( var reader = new StreamReader( cmd.OutputStream, Encoding.UTF8, true, 1024, true ) )
				{
					while( result.IsCompleted == false || reader.EndOfStream == false )
					{
						string line = reader.ReadLine();
						if( line != null )
						{
							mLog.LogWrite( line );
							output_list.Add( line );
						}
					}
				}

				end_result = cmd.EndExecute( result );
 			}


			Cursor.Current = Cursors.Default;

			mLog.LogWrite( "cmd:{0} result:{1}", command, end_result );

			return output_list;
		}

		private void btn_altools_close_Click( object sender, EventArgs e )
		{
			ClearAndClose();
		}

		void DoAppleAltoolsExecute(bool is_upload)
		{
			if( lv_altools_ipalist.SelectedItems.Count <= 0 )
			{
				MessageBox.Show( "Select IPA Data!" );
				return;
			}

			string apple_id = tb_apple_id.Text;
			string apple_pw = tb_apple_pw.Text;

			if( string.IsNullOrEmpty( apple_id ) || string.IsNullOrEmpty( apple_pw ) )
			{
				MessageBox.Show( "Apple ID or PW invalid!" );
				return;
			}

			ToolUtil.SavePrefs( FormDistribution.REG_SUB_KEY, REG_KEY_LAST_APPLE_ID, apple_id );

			ListViewItem selected_item = lv_altools_ipalist.SelectedItems[0];
			IPAData ipa_data = selected_item.Tag as IPAData;
			if( ipa_data == null )
			{
				MessageBox.Show( "IPAData invalid!" );
				return;
			}

			string command = "";
			if( is_upload )
				command = string.Format( "xcrun altool --upload-app -f {0}/{1} -u {2} -p {3} --verbose", mData.work_path, ipa_data.file_name, apple_id, apple_pw );
			else
				command = string.Format( "xcrun altool --validate-app -f {0}/{1} -u {2} -p {3} --verbose", mData.work_path, ipa_data.file_name, apple_id, apple_pw );

			string end_result = "";
			DoSSDCommand( command, ref end_result );

			if( string.IsNullOrEmpty( end_result ) == false )
				MessageBox.Show( end_result );
		}
		private void btn_altools_upload_ipa_Click( object sender, EventArgs e )
		{
			if( MessageBox.Show( "", "Apple upload?", MessageBoxButtons.OKCancel ) == DialogResult.OK )
			{
				DoAppleAltoolsExecute( true );
			}
		}

		private void btn_altools_verify_ipa_Click( object sender, EventArgs e )
		{
			DoAppleAltoolsExecute( false );
		}

		private void btn_altools_list_ipa_Click( object sender, EventArgs e )
		{
			string command = string.Format( "ls -lh {0}/*.ipa", mData.work_path );
			string end_result = "";

			List<string> ipa_list = DoSSDCommand( command, ref end_result );

			List<IPAData> data_list = new List<IPAData>();
			
			foreach(string file_info in ipa_list )
			{
				data_list.Add( new IPAData( file_info ) );
			}

			data_list = data_list.OrderByDescending( a => a.version ).ThenByDescending( a => a.revision ).ThenByDescending( a => a.build_num ).ToList();

			lv_altools_ipalist.Items.Clear();
			bool order_color = false;
			for(int i=0; i<data_list.Count; i++ )
			{
				IPAData data = data_list[i];

				ListViewItem item = new ListViewItem();
				item.Text = data.full;
				item.Tag = data;
				if( i == 0 )
					item.BackColor = Color.LightGreen;
				else
					item.BackColor = order_color ? Color.LightGray : Color.White;
				lv_altools_ipalist.Items.Add( item );

				order_color = !order_color;
			}

			lv_altools_ipalist.AutoResizeColumns( ColumnHeaderAutoResizeStyle.HeaderSize );
			lv_altools_ipalist.AutoResizeColumns( ColumnHeaderAutoResizeStyle.ColumnContent );
		}

		private void btn_clear_log_Click( object sender, EventArgs e )
		{
			if( mLog != null )
				mLog.Clear();
		}
	}
}
