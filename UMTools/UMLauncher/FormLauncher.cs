using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using UMF.Core;
using UMTools.Common;

namespace UMTools.UMLauncher
{
	public partial class FormLauncher : Form
	{
		const string REG_SUB_KEY = "UMLauncher";
		const string REG_KEY_AUTOCHECKNEWVER = "_AutoCheckNewVer_";
		const string REG_KEY_CLIENTNOTUSEDOCTBL = "_ClientNotUseDocTBL_";

		const string NEW_VER_CHANGE_LOG_PREFIX = "CHANGES";
		const string DEV_SERVER_CONFIG_FILE = "_ConfigType.xml";

		const string DEV_DOWNLOAD_PATH = "_download";

		string DEV_CLIENT_PATH
		{
			get
			{
				return $"{mProjectGlobaltypeProperty.GlobalType}\\DevClient";
			}
		}

		string DEV_SERVER_PATH
		{
			get
			{
				return $"{mProjectGlobaltypeProperty.GlobalType}\\DevServer";
			}
		}

		string DEV_CLIENT_DOC_PATH
		{
			get
			{
				return $"_documents_";
			}
		}
		string DEV_CLIENT_TEMP_PATH
		{
			get
			{
				return $"_temporaryCachePath_";
			}
		}

		LogWriter mLog = null;
		bool mHasClient = false;
		bool mHasServer = false;
		System.DateTime mAutoUpdateCheckBeginTime = System.DateTime.MinValue;
		List<CheckNewVer.VersionData> mNewVersionDataList = new List<CheckNewVer.VersionData>();
		string CURR_DEV_CLIENT_CHANGED_LOG = "";
		string CURR_DEV_SERVER_CHANGED_LOG = "";

		CheckNewVer.VersionData mCurrentVersionData = null;

		ProjectPropertyConfig mProjectProperty = null;
		ProjectGlobaltypePropertyConfig mProjectGlobaltypeProperty = null;
		GlobalPropertyConfig mGlobalProperty = null;

		// process
		enum eProcessType
		{
			None,
			Client,
			Server,
		}

		Dictionary<eProcessType, List<Process>> mProcessDic = new Dictionary<eProcessType, List<Process>>();
		UMToolsCommon.CSafeControlParams _safe_process_launch_btn_invoke = null;

		public class VersionComboItemData
		{
			public string text;
			public bool is_new;

			public VersionComboItemData( string _text )
				: this( _text, false )
			{
			}
			public VersionComboItemData( string _text, bool _new )
			{
				text = _text;
				is_new = _new;
			}

			public override string ToString()
			{
				if( is_new )
					return "[마지막]" + text;

				return text;
			}
		}

		public FormLauncher()
		{
			InitializeComponent();

			mLog = new LogWriter( rtb_log );

			_safe_process_launch_btn_invoke = new UMToolsCommon.CSafeControlParams( _SafeProcessLaunchBtnHandler );			

			foreach( eProcessType p_type in Enum.GetValues( typeof( eProcessType ) ) )
			{
				if( p_type == eProcessType.None )
					continue;

				mProcessDic.Add( p_type, new List<Process>() );
			}

			object auto_check_obj = ToolUtil.GetPrefs( REG_SUB_KEY, REG_KEY_AUTOCHECKNEWVER );
			if( auto_check_obj != null )
				cb_autochecknewver.Checked = ( (int)auto_check_obj == 1 ? true : false );
			else
				cb_autochecknewver.Checked = true;

			object not_use_doctbl = ToolUtil.GetPrefs( REG_SUB_KEY, REG_KEY_CLIENTNOTUSEDOCTBL );
			if( not_use_doctbl != null )
				cb_client_notusedoctbl.Checked = ( (int)not_use_doctbl == 1 ? true : false );
			else
				cb_client_notusedoctbl.Checked = false;

			if( ProjectConfig.Instance.QuickSave.IsValid() == false )
			{
				FormProjectProperty.Open();
			}

			AllRefresh();
		}

		void AllRefresh()
		{
			mGlobalProperty = ProjectConfig.Instance.GlobalProperty;
			mProjectProperty = ProjectConfig.Instance.CurrentProjectProerty;
			mProjectGlobaltypeProperty = ProjectConfig.Instance.CurrentProjectGlobaltypeProperty;
			ProjectConfig.Data project_data = ProjectConfig.Instance.CurrentProjectData;

			string project_name = $"{mProjectGlobaltypeProperty.ProjectName}";
			if( string.IsNullOrEmpty( project_data.ProjectSubName ) == false )
				project_name += $"-{project_data.ProjectSubName}";

			this.BackColor = ToolUtil.GetColorFromHEX( project_data.FormColor );
			this.Text = string.Format( "Launcher(VER:{0}) PROJECT : {1}({2})"
				, Assembly.GetExecutingAssembly().GetName().Version.ToString()
				, project_name, mProjectGlobaltypeProperty.GlobalType
				);
			lbl_projectname.Text = $"{project_name}({mProjectGlobaltypeProperty.GlobalType})";

			cb_autochecknewver.Text = $"자동체크\n({ToolUtil.GetTimeString( mProjectGlobaltypeProperty.launcher_update_check_time_interval )})";
			cb_autochecknewver_CheckedChanged( null, null );

			mHasServer = ( mProjectGlobaltypeProperty.launcher_server_exec_files.Length > 0 );
			mHasClient = ( mProjectGlobaltypeProperty.launcher_client_exec_file.Length > 0 );

			ButtonLocked( false );
			UpdateProcessBtn();
			RefreshDevServerClientVersion();

			_CheckDevNewVersion( true, false );
		}

		void UpdateButtonTooltips()
		{
			tooltip.RemoveAll();
		}

		void ButtonLocked( bool forced_lock )
		{
			bool unlock_server = mHasServer;
			bool unlock_client = mHasClient;
			if( forced_lock )
			{
				unlock_server = false;
				unlock_client = false;
			}

			btn_launch_devclient.Enabled = unlock_client;
			btn_devclient_close.Enabled = unlock_client;
			cb_launch_devclient_vers.Enabled = unlock_client;
			btn_dev_client_battletest.Enabled = unlock_client;
			btn_dev_client_fxtool.Enabled = unlock_client;
			btn_dev_client_changelog.Enabled = unlock_client;
			btn_client_log_open.Enabled = unlock_client;

			btn_launch_devserver.Enabled = unlock_server;
			btn_devserver_close.Enabled = unlock_server;
			btn_dev_server_changelog.Enabled = unlock_server;
			btn_server_log_open.Enabled = unlock_server;
			tb_devserver_configtype.Enabled = unlock_server;

			btn_dev_newvercheck.Enabled = ( forced_lock == false );
		}

		//------------------------------------------------------------------------		
		void _SafeProcessLaunchBtnHandler( UMToolsCommon.CSafeParamData args )
		{
			eProcessType process_type = args.GetParam<eProcessType>( 0, eProcessType.None );
			if( process_type == eProcessType.None )
				return;

			Button p_button = null;
			switch( process_type )
			{
				case eProcessType.Client: p_button = btn_launch_devclient; break;
				case eProcessType.Server: p_button = btn_launch_devserver; break;
			}

			if( p_button == null )
				return;			

			if( p_button.InvokeRequired )
			{
				p_button.Invoke( _safe_process_launch_btn_invoke, args );
			}
			else
			{
				if( args != null )
				{
					string btn_text = args.GetParam<string>( 1, "" );
					bool is_exit = args.GetParam<bool>( 2, true );

					p_button.Text = btn_text;
					if( is_exit == false )
					{
						p_button.BackColor = Color.Red;
						p_button.ForeColor = Color.White;
					}
					else
					{
						p_button.BackColor = Color.FromArgb( 255, 255, 192 );
						p_button.ForeColor = Color.Black;
					}
				}
			}
		}

		//------------------------------------------------------------------------
		void UpdateProcessBtn()
		{
			string client_ver_text = tb_client_currver.Text;

			bool is_prev_version = false;
			CheckNewVer.VersionData curr_v_data = CheckNewVer.Instance.FileName2VersionData( client_ver_text );
			if( cb_launch_devclient_vers.SelectedIndex > 0 )
			{
				VersionComboItemData item = cb_launch_devclient_vers.SelectedItem as VersionComboItemData;
				if( item != null )
				{
					string ver_path = Path.Combine( Directory.GetCurrentDirectory(), DEV_CLIENT_PATH, item.text );
					if( Directory.Exists( ver_path ) )
					{
						client_ver_text = item.text;
						CheckNewVer.VersionData selected_v_data = CheckNewVer.Instance.FileName2VersionData( client_ver_text );
						if( curr_v_data.version != selected_v_data.version )
							is_prev_version = true;
					}
				}
			}

			if( mProcessDic[eProcessType.Client].Count > 0 )
			{
				string btn_text = $"개발용 클라이언트 실행중\n{client_ver_text}\n현재 {mProcessDic[eProcessType.Client].Count}개 실행중..";
				if( is_prev_version )
					btn_text = $"이전버전용 클라이언트 실행중\n{client_ver_text}\n현재 {mProcessDic[eProcessType.Client].Count}개 실행중..";

				_SafeProcessLaunchBtnHandler( new UMToolsCommon.CSafeParamData( eProcessType.Client, btn_text, false ) );
			}
			else
			{
				_SafeProcessLaunchBtnHandler( new UMToolsCommon.CSafeParamData( eProcessType.Client, "개발용 클라이언트 실행", true ) );
			}

			if( mProcessDic[eProcessType.Server].Count <= 0 )
				_SafeProcessLaunchBtnHandler( new UMToolsCommon.CSafeParamData( eProcessType.Server, "로컬서버 실행", true ) );
			else
				_SafeProcessLaunchBtnHandler( new UMToolsCommon.CSafeParamData( eProcessType.Server, $"로컬서버 실행중 : {mProcessDic[eProcessType.Server].Count}\n{tb_server_currver.Text}", false ) );
		}


		//------------------------------------------------------------------------
		void CloseAllProcess( eProcessType process_type, bool forced )
		{
			if( forced == false )
			{
				string msg = "";
				switch( process_type )
				{
					case eProcessType.Client: msg = "모든 클라이언트를 종료하시겠습니까?"; break;
					case eProcessType.Server: msg = "모든 서버를 종료하시겠습니까?"; break;
				}

				if( MessageBox.Show( msg, "Close", MessageBoxButtons.OKCancel ) == DialogResult.Cancel )
					return;
			}

			foreach( Process s_p in mProcessDic[process_type] )
			{
				if( s_p != null && s_p.HasExited == false )
				{
					s_p.Kill();
					s_p.Dispose();
				}
			}
			mProcessDic[process_type].Clear();
		}

		//------------------------------------------------------------------------
		void _CheckDevNewVersion( bool silent, bool alarm_only )
		{
			if( string.IsNullOrEmpty( mProjectGlobaltypeProperty.launcher_download_base_url ) )
			{
				cb_autochecknewver_CheckedChanged( null, null );
				return;
			}

			ButtonLocked( true );

			lbl_progress_newvertext.Text = "";
			Cursor.Current = Cursors.WaitCursor;

			mNewVersionDataList.Clear();

			if( mHasServer )
			{
				string server_check_url = mProjectGlobaltypeProperty.launcher_download_base_url + "/" + mProjectGlobaltypeProperty.launcher_url_server_ver_file;
				string server_change_log_url = mProjectGlobaltypeProperty.launcher_download_base_url + "/" + NEW_VER_CHANGE_LOG_PREFIX;

				CheckNewVer.VersionData _new_server_data = CheckNewVer.Instance.CheckNewVersion( server_check_url, server_change_log_url );
				CheckNewVer.VersionData curr_server_version = CheckNewVer.Instance.FileName2VersionData<CheckNewVer.VersionData>( tb_server_currver.Text );

				if( _new_server_data != null )
					CURR_DEV_SERVER_CHANGED_LOG = _new_server_data.changed_log;

				if( _new_server_data == null || _new_server_data.IsAboveVersion( curr_server_version ) == false )
				{
					_new_server_data = null;
					if( silent == false )
						MessageBox.Show( "현재 서버는 최신 버전입니다!", "Check" );
				}
				else
				{
					_new_server_data.type_name = "서버";
					_new_server_data.install_path = Path.Combine( DEV_SERVER_PATH, Path.GetFileNameWithoutExtension( _new_server_data.file_name ) );
					_new_server_data.download_file_path = Path.Combine( DEV_SERVER_PATH, DEV_DOWNLOAD_PATH, _new_server_data.file_name );
					mNewVersionDataList.Add( _new_server_data );
				}
			}

			string client_check_url = mProjectGlobaltypeProperty.launcher_download_base_url + "/" + mProjectGlobaltypeProperty.launcher_url_client_ver_file;
			string client_change_log_url = mProjectGlobaltypeProperty.launcher_download_base_url + "/" + NEW_VER_CHANGE_LOG_PREFIX;

			CheckNewVer.VersionData _new_client_data = CheckNewVer.Instance.CheckNewVersion( client_check_url, client_change_log_url );
			CheckNewVer.VersionData curr_client_version = CheckNewVer.Instance.FileName2VersionData<CheckNewVer.VersionData>( tb_client_currver.Text );

			if( _new_client_data != null )
				CURR_DEV_CLIENT_CHANGED_LOG = _new_client_data.changed_log;

			if( _new_client_data == null || _new_client_data.IsAboveVersion( curr_client_version ) == false )
			{
				_new_client_data = null;
				if( silent == false )
					MessageBox.Show( "현재 클라이언트는 최신 버전입니다!", "Check" );
			}
			else
			{
				_new_client_data.type_name = "클라이언트";
				_new_client_data.install_path = Path.Combine( DEV_CLIENT_PATH, Path.GetFileNameWithoutExtension( _new_client_data.file_name ) );
				_new_client_data.download_file_path = Path.Combine( DEV_CLIENT_PATH, DEV_DOWNLOAD_PATH, _new_client_data.file_name );
				mNewVersionDataList.Add( _new_client_data );
			}

			cb_autochecknewver_CheckedChanged( null, null );
			if( alarm_only )
			{
				if( mNewVersionDataList.Count > 0 )
				{
					timer_onesec.Stop();
					timer_Alarm.Interval = 1000;
					timer_Alarm.Start();
				}

				ButtonLocked( false );
			}
			else
			{
				__InstallNewVersion();

				if( mCurrentVersionData == null && mNewVersionDataList.Count <= 0 )
				{
					ButtonLocked( false );
					Cursor.Current = Cursors.Default;
				}
			}
		}

		//------------------------------------------------------------------------
		UMToolsCommon.CSafeProgressSetValue safe_progress_value_invoke;
		UMToolsCommon.CSafeProgressSetMaxValue safe_progress_maxvalue_invoke;
		bool _is_maxvalue_setted = false;
		WebClient mCurrentWebClient = null;

		void __InstallNewVersion()
		{
			mCurrentVersionData = null;
			_is_maxvalue_setted = false;
			if( mNewVersionDataList.Count <= 0 )
			{
				ButtonLocked( false );
				Cursor.Current = Cursors.Default;
				return;
			}

			mCurrentVersionData = mNewVersionDataList[0];
			mNewVersionDataList.RemoveAt( 0 );

			FormNewVerDialog dialog = new FormNewVerDialog();
			dialog.SetVersion( mCurrentVersionData );
			dialog.StartPosition = FormStartPosition.CenterScreen;
			if( dialog.ShowDialog() == DialogResult.OK )
			{
				string down_url = mProjectGlobaltypeProperty.launcher_download_base_url + "/" + mCurrentVersionData.file_name;

				if( safe_progress_value_invoke == null )
					safe_progress_value_invoke = new UMToolsCommon.CSafeProgressSetValue( OnProgressNewVerDownload );

				if( safe_progress_maxvalue_invoke == null )
					safe_progress_maxvalue_invoke = new UMToolsCommon.CSafeProgressSetMaxValue( OnProgressMaxValueNewVerDownload );

				lbl_progress_newvertext.Text = "다운로드중..";

				if( Directory.Exists( Path.GetDirectoryName( mCurrentVersionData.download_file_path ) ) == false )
					Directory.CreateDirectory( Path.GetDirectoryName( mCurrentVersionData.download_file_path ) );

				mCurrentWebClient = new WebClient();
				mCurrentWebClient.DownloadFileCompleted += new AsyncCompletedEventHandler( OnFileDownloadCompleted );
				mCurrentWebClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler( OnFileDownloadProgressChanged );
				mCurrentWebClient.DownloadFileAsync( new Uri( down_url ), mCurrentVersionData.download_file_path );
			}
			else
			{
				__InstallNewVersion();
			}
		}

		void OnProgressNewVerDownload( Int32 value )
		{
			if( progress_newverdownload.InvokeRequired )
			{
				progress_newverdownload.Invoke( safe_progress_value_invoke, value );
			}
			else
			{
				progress_newverdownload.Value = value;
			}
		}

		void OnProgressMaxValueNewVerDownload( Int32 value )
		{
			if( progress_newverdownload.InvokeRequired )
			{
				progress_newverdownload.Invoke( safe_progress_maxvalue_invoke, value );
			}
			else
			{
				progress_newverdownload.Maximum = value;
			}
		}

		void OnFileDownloadProgressChanged( object sender, DownloadProgressChangedEventArgs e )
		{
			if( _is_maxvalue_setted == false )
			{
				OnProgressMaxValueNewVerDownload( (int)e.TotalBytesToReceive );
				_is_maxvalue_setted = true;
			}

			OnProgressNewVerDownload( (int)e.BytesReceived );
		}

		void OnFileDownloadCompleted( object sender, AsyncCompletedEventArgs e )
		{
			mCurrentWebClient = null;
			if( e.Error != null )
			{
				if( e.Cancelled == false )
				{
					MessageBox.Show( "다운로드실패:" + e.Error.ToString(), "Download" );
				}
			}
			else if( File.Exists( mCurrentVersionData.download_file_path ) == false )
			{
				MessageBox.Show( $"다운로드실패:다운로드 받은 파일이 없습니다.{mCurrentVersionData.download_file_path}", "Download" );
			}
			else
			{
				lbl_progress_newvertext.Text = "설치중..";
				lbl_progress_newvertext.Invalidate();

				string error = ToolUtil.ZipExtract( mCurrentVersionData.download_file_path, mCurrentVersionData.install_path );
				if( string.IsNullOrEmpty( error ) == false )
				{
					MessageBox.Show( error, "Download" );
				}
				else
				{
					MessageBox.Show( "다운로드 및 설치 성공!", "Download" );
				}
			}

			lbl_progress_newvertext.Text = "";
			progress_newverdownload.Value = 0;

			RefreshDevServerClientVersion();
			__InstallNewVersion();
		}

		//------------------------------------------------------------------------	
		void RefreshDevServerClientVersion()
		{
			tb_server_currver.Text = "";
			// check server
			if( mHasServer )
			{
				string s_path = Path.Combine( Directory.GetCurrentDirectory(), DEV_SERVER_PATH );
				if( Directory.Exists( s_path ) == false )
					Directory.CreateDirectory( s_path );

				string _config_file = Path.Combine( s_path, DEV_SERVER_CONFIG_FILE );
				if( File.Exists( _config_file ) == false )
					UpdateDevServerConfigType();

				// check server
				CheckNewVer.VersionData last_server_v_data = null;
				string[] server_folders = Directory.GetDirectories( s_path );
				if( server_folders != null )
				{
					foreach( string server in server_folders )
					{
						CheckNewVer.VersionData v_data = CheckNewVer.Instance.FileName2VersionData( Path.GetFileName( server ) );
						if( v_data.IsAboveVersion( last_server_v_data ) )
							last_server_v_data = v_data;
					}
				}

				if( last_server_v_data != null )
					tb_server_currver.Text = last_server_v_data.file_name;
			}

			// check client
			string c_path = Path.Combine( Directory.GetCurrentDirectory(), DEV_CLIENT_PATH );
			if( Directory.Exists( c_path ) == false )
				Directory.CreateDirectory( c_path );

			List<CheckNewVer.VersionData> client_versions = new List<CheckNewVer.VersionData>();
			CheckNewVer.VersionData last_client_v_data = null;
			string[] client_folders = Directory.GetDirectories( c_path );
			if( client_folders != null )
			{
				foreach( string client in client_folders )
				{
					string f_name = Path.GetFileName( client );
					if( f_name == DEV_CLIENT_DOC_PATH || f_name == DEV_CLIENT_TEMP_PATH || f_name == DEV_DOWNLOAD_PATH )
						continue;

					CheckNewVer.VersionData v_data = CheckNewVer.Instance.FileName2VersionData( Path.GetFileName( client ) );
					if( v_data.IsAboveVersion( last_client_v_data ) )
						last_client_v_data = v_data;

					client_versions.Add( v_data );
				}
			}

			if( last_client_v_data != null )
				tb_client_currver.Text = last_client_v_data.file_name;
			else
				tb_client_currver.Text = "";

			// versions launch
			client_versions = client_versions.OrderByDescending( c => c.version ).ThenByDescending( c => c.revision ).ThenByDescending( c => c.build_num ).ToList();
			cb_launch_devclient_vers.Items.Clear();
			cb_launch_devclient_vers.Items.Add( new VersionComboItemData( "최신업데이트" ) );
			cb_launch_devclient_vers.Items.Add( new VersionComboItemData( string.Format( "--------- [{0}]", last_client_v_data != null ? last_client_v_data.version.ToString() : "" ) ) );

			Version tmp_last_ver = null;
			foreach( CheckNewVer.VersionData vdata in client_versions )
			{
				bool is_newest = false;
				if( tmp_last_ver != null && tmp_last_ver != vdata.version )
				{
					cb_launch_devclient_vers.Items.Add( new VersionComboItemData( string.Format( "--------- [{0}]", vdata.version ) ) );
					is_newest = true;
				}

				if( vdata != last_client_v_data )
				{
					cb_launch_devclient_vers.Items.Add( new VersionComboItemData( vdata.file_name, is_newest ) );
				}

				tmp_last_ver = vdata.version;
			}

			cb_launch_devclient_vers.SelectedIndex = 0;
		}

		//------------------------------------------------------------------------
		void UpdateDevServerConfigType()
		{
			string config_name = tb_devserver_configtype.Text;

			string s_path = Path.Combine( Directory.GetCurrentDirectory(), DEV_SERVER_PATH );
			if( Directory.Exists( s_path ) == false )
				Directory.CreateDirectory( s_path );

			string _config_file = Path.Combine( s_path, DEV_SERVER_CONFIG_FILE );
			if( File.Exists( _config_file ) )
			{
				XmlDocument doc = new XmlDocument();
				doc.Load( _config_file );
				XmlNode root_node = doc.SelectSingleNode( "ConfigType" );
				string config_type = XMLUtil.ParseAttribute<string>( root_node, "type", "" );
				string global = XMLUtil.ParseAttribute<string>( root_node, "global", "" );

				if( config_type == config_name && global == mProjectGlobaltypeProperty.GlobalType )
					return;
			}

			XmlDocument new_doc = new XmlDocument();
			XmlNode root = new_doc.AppendChild( new_doc.CreateElement( "ConfigType" ) );
			XMLUtil.AddAttribute( root, "type", config_name );
			XMLUtil.AddAttribute( root, "global", mProjectGlobaltypeProperty.GlobalType );

			XMLUtil.SaveXmlDocToFile( _config_file, new_doc );
		}

		//------------------------------------------------------------------------
		void OnClientProcessExited( object sender, EventArgs e )
		{
			mProcessDic[eProcessType.Client].RemoveAll( p => p == (Process)sender );
			UpdateProcessBtn();
		}

		void OnServerProcessExited( object sender, EventArgs e )
		{
			mProcessDic[eProcessType.Server].RemoveAll( p => p == (Process)sender );
			UpdateProcessBtn();
		}

		//------------------------------------------------------------------------
		void DoLaunchClient( string args )
		{
			string client_ver_text = tb_client_currver.Text;

			bool is_prev_version = false;
			CheckNewVer.VersionData curr_v_data = CheckNewVer.Instance.FileName2VersionData( client_ver_text );
			if( cb_launch_devclient_vers.SelectedIndex > 0 )
			{
				VersionComboItemData item = cb_launch_devclient_vers.SelectedItem as VersionComboItemData;
				if( item != null )
				{
					string ver_path = Path.Combine( Directory.GetCurrentDirectory(), DEV_CLIENT_PATH, item.text );
					if( Directory.Exists( ver_path ) )
					{
						client_ver_text = item.text;
						CheckNewVer.VersionData selected_v_data = CheckNewVer.Instance.FileName2VersionData( client_ver_text );
						if( curr_v_data.version != selected_v_data.version )
							is_prev_version = true;
					}
				}
			}

			string client_working_path = Path.Combine( Directory.GetCurrentDirectory(), DEV_CLIENT_PATH, client_ver_text );

			List<Process> client_process_list = mProcessDic[eProcessType.Client];
			if( client_process_list.Count > 0 )
			{
				if( MessageBox.Show( "이미 실행중입니다.하나 더 실행할까요?\n복사본이 없으면 자동으로 복사합니다.", "Launch", MessageBoxButtons.OKCancel ) == DialogResult.Cancel )
					return;
			}

			// copy when spare
			string process_suffix = "";
			if( client_process_list.Count > 0 )
			{
				int client_copy_idx = client_process_list.Count + 1;
				string copy_client_working_path = Path.Combine( Directory.GetCurrentDirectory(), DEV_CLIENT_PATH, client_ver_text + "_copy_" + client_copy_idx.ToString() );
				if( Directory.Exists( copy_client_working_path ) == false )
				{
					try
					{
						ToolUtil.DirectoryCopy( client_working_path, copy_client_working_path, true, true );
					}
					catch( System.Exception ex )
					{
						MessageBox.Show( ex.ToString() );
						return;
					}
				}

				process_suffix = "_copy_" + client_copy_idx.ToString();
				client_working_path = copy_client_working_path;
			}

			string client_exe_path = Path.Combine( client_working_path, mProjectGlobaltypeProperty.launcher_client_exec_file );
			if( File.Exists( client_exe_path ) == false )
			{
				MessageBox.Show( string.Format( "해당 실행 파일이 없습니다.\n{0}", client_exe_path ), "Warning" );
				return;
			}

			string _doc_path = Path.Combine( Directory.GetCurrentDirectory(), DEV_CLIENT_PATH, DEV_CLIENT_DOC_PATH );
			if( Directory.Exists( _doc_path ) == false )
				Directory.CreateDirectory( _doc_path );

			string _temp_path = Path.Combine( Directory.GetCurrentDirectory(), DEV_CLIENT_PATH, DEV_CLIENT_TEMP_PATH );
			if( Directory.Exists( _temp_path ) == false )
				Directory.CreateDirectory( _temp_path );

			Process client_process = new Process();
			client_process.StartInfo.FileName = client_exe_path;
			client_process.StartInfo.WorkingDirectory = client_working_path;
			if( is_prev_version == false )
			{
				string tbl_path = mProjectGlobaltypeProperty.TBLPath;
				if( cb_client_notusedoctbl.Checked )
					tbl_path = "";
				client_process.StartInfo.Arguments = $"-tblpath={tbl_path} -docpath={_doc_path} -temppath={_temp_path}";
			}

			if( string.IsNullOrEmpty( args ) == false )
				client_process.StartInfo.Arguments += args;

			client_process.EnableRaisingEvents = true;
			client_process.Exited += OnClientProcessExited;

			mProcessDic[eProcessType.Client].Add( client_process );
			client_process.Start();

			UpdateProcessBtn();
		}

		//------------------------------------------------------------------------

		private void btn_close_Click( object sender, EventArgs e )
		{
			Close();
		}

		private void btn_project_select_Click( object sender, EventArgs e )
		{
			DialogResult result = FormProjectProperty.Open();
			if( result == DialogResult.OK )
			{
				AllRefresh();
			}
		}

		private void btn_launch_devclient_Click( object sender, EventArgs e )
		{
			DoLaunchClient( "" );
		}

		private void btn_devclient_close_Click( object sender, EventArgs e )
		{
			CloseAllProcess( eProcessType.Client, false );
			_SafeProcessLaunchBtnHandler( new UMToolsCommon.CSafeParamData( eProcessType.Client, "개발용 클라이언트 실행", true ) );
		}

		private void btn_launch_devserver_Click( object sender, EventArgs e )
		{
			if( mProcessDic[eProcessType.Server].Exists( p => p != null && p.HasExited == false ) )
			{
				if( MessageBox.Show( "서버가 실행중입니다. 모든 서버 종료후 다시 실행할까요?", "Launch", MessageBoxButtons.OKCancel ) == DialogResult.Cancel )
				{
					return;
				}
			}

			foreach( Process server in mProcessDic[eProcessType.Server] )
			{
				server.Kill();
				server.Dispose();
			}

			mProcessDic[eProcessType.Server].Clear();

			UpdateDevServerConfigType();

			List<string> server_exe_list = StringUtil.SafeParseToList<string>( mProjectGlobaltypeProperty.launcher_server_exec_files, ',' );
			if( server_exe_list != null && server_exe_list.Count > 0 )
			{
				string server_exe_path = Path.Combine( Directory.GetCurrentDirectory(), DEV_SERVER_PATH, tb_server_currver.Text );
				foreach( string server_exe in server_exe_list )
				{
					Process server = new Process();
					server.StartInfo.FileName = Path.Combine( server_exe_path, server_exe );
					server.StartInfo.WorkingDirectory = server_exe_path;
					server.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
					server.StartInfo.UseShellExecute = true;
					server.StartInfo.ErrorDialog = false;
					if( server_exe == "MasterServer.exe" )
					{
						server.StartInfo.Arguments = string.Format( "Dev -devlocalpc -tblpath={0}\\", mProjectGlobaltypeProperty.TBLPath );
					}
					else
					{
						server.StartInfo.Arguments = "Dev -devlocalpc";
					}
					server.EnableRaisingEvents = true;
					server.Exited += OnServerProcessExited;

					server.Start();

					mProcessDic[eProcessType.Server].Add( server );
				}

				_SafeProcessLaunchBtnHandler( new UMToolsCommon.CSafeParamData( eProcessType.Server, $"로컬서버 실행중 : {mProcessDic[eProcessType.Server].Count}\n{tb_server_currver.Text}", false ) );
			}
		}

		private void btn_devserver_close_Click( object sender, EventArgs e )
		{
			CloseAllProcess( eProcessType.Server, false );
			_SafeProcessLaunchBtnHandler( new UMToolsCommon.CSafeParamData( eProcessType.Server, "로컬서버 실행", true ) );

		}

		private void FormLauncher_FormClosed( object sender, FormClosedEventArgs e )
		{
			if( mLog != null )
				mLog.Closed();

			foreach( var kvp in mProcessDic )
			{
				CloseAllProcess( kvp.Key, true );
			}

			mProcessDic.Clear();
		}

		private void cb_autochecknewver_CheckedChanged( object sender, EventArgs e )
		{
			if( cb_autochecknewver.Checked )
			{
				mAutoUpdateCheckBeginTime = System.DateTime.Now;
				timer_onesec.Start();
			}
			else
			{
				timer_onesec.Stop();
			}

			ToolUtil.SavePrefs( REG_SUB_KEY, REG_KEY_AUTOCHECKNEWVER, ( cb_autochecknewver.Checked ? 1 : 0 ) );
		}

		private void timer_onesec_Tick( object sender, EventArgs e )
		{
			if( cb_autochecknewver.Checked )
			{
				TimeSpan ts = DateTime.Now - mAutoUpdateCheckBeginTime;
				int remain_time = mProjectGlobaltypeProperty.launcher_update_check_time_interval - (int)ts.TotalMilliseconds;
				cb_autochecknewver.Text = $"자동체크\n({ToolUtil.GetTimeString( remain_time )})";

				if( remain_time <= 0 )
					_CheckDevNewVersion( true, true );
			}
		}

		private void timer_Alarm_Tick( object sender, EventArgs e )
		{
			if( btn_dev_newvercheck.BackColor == Color.Red )
				btn_dev_newvercheck.BackColor = DefaultBackColor;
			else
				btn_dev_newvercheck.BackColor = Color.Red;
		}

		private void btn_dev_client_battletest_Click( object sender, EventArgs e )
		{
			DoLaunchClient( "-battletest " );
		}

		private void btn_dev_client_fxtool_Click( object sender, EventArgs e )
		{
			DoLaunchClient( "-fxtool " );
		}

		private void btn_dev_client_changelog_Click( object sender, EventArgs e )
		{
			CheckNewVer.VersionData curr_client_version = CheckNewVer.Instance.FileName2VersionData<CheckNewVer.VersionData>( tb_client_currver.Text );
			curr_client_version.type_name = "클라이언트";
			curr_client_version.changed_log = CURR_DEV_CLIENT_CHANGED_LOG;

			FormNewVerDialog dialog = new FormNewVerDialog();
			dialog.SetVersion( curr_client_version, false );
			dialog.StartPosition = FormStartPosition.CenterScreen;
			dialog.ShowDialog();
		}

		private void btn_client_log_open_Click( object sender, EventArgs e )
		{
			CheckNewVer.VersionData curr_client_version = CheckNewVer.Instance.FileName2VersionData<CheckNewVer.VersionData>( tb_client_currver.Text );
			string profile_path = System.Environment.GetEnvironmentVariable( "USERPROFILE" );
			string path = Path.Combine( profile_path, "AppData", "LocalLow", mProjectGlobaltypeProperty.launcher_client_company_name );

			if( Directory.Exists( path ) == false )
			{
				MessageBox.Show( $"Not found log path : {path}" );
				return;
			}

			string[] sub_paths = Directory.GetDirectories( path );
			foreach( string sub in sub_paths )
			{
				if( sub.Contains( curr_client_version.version.ToString() ) && sub.Contains( curr_client_version.revision.ToString() ) && sub.Contains( curr_client_version.build_num.ToString() ) )
				{
					string log_file = Path.Combine( sub, "Player.log" );
					if( File.Exists( log_file ) )
						path = log_file;
					else
						path = sub;
					break;
				}
			}

			Process.Start( "explorer.exe", Environment.ExpandEnvironmentVariables( path ) );
		}

		private void btn_dev_server_changelog_Click( object sender, EventArgs e )
		{
			CheckNewVer.VersionData curr_server_version = CheckNewVer.Instance.FileName2VersionData<CheckNewVer.VersionData>( tb_server_currver.Text );
			curr_server_version.type_name = "서버";
			curr_server_version.changed_log = CURR_DEV_SERVER_CHANGED_LOG;

			FormNewVerDialog dialog = new FormNewVerDialog();
			dialog.SetVersion( curr_server_version, false );
			dialog.StartPosition = FormStartPosition.CenterScreen;
			dialog.ShowDialog();
		}

		private void btn_server_log_open_Click( object sender, EventArgs e )
		{
			string path = Path.Combine( Directory.GetCurrentDirectory(), DEV_SERVER_PATH, tb_server_currver.Text );
			Process.Start( "explorer.exe", path );
		}

		private void btn_dev_newvercheck_Click( object sender, EventArgs e )
		{
			btn_dev_newvercheck.BackColor = DefaultBackColor;
			timer_Alarm.Stop();

			_CheckDevNewVersion( false, false );
		}

		private void btn_dev_clear_prevversion_Click( object sender, EventArgs e )
		{
			CheckNewVer.VersionData curr_v_data = CheckNewVer.Instance.FileName2VersionData( tb_client_currver.Text );
			Version delete_max_ver = curr_v_data.GetPrevVersion( mProjectGlobaltypeProperty.launcher_delete_prev_version_distance );
			if( MessageBox.Show( string.Format( "Version {0} 이하의 모든 클라이언트버전이 삭제됩니다.삭제하시겠습니까?\n(서버는 최신버전 제외하고 모두 삭제)", delete_max_ver ), "Warning", MessageBoxButtons.OKCancel ) == DialogResult.Cancel )
				return;

			List<string> deleted_path_list = new List<string>();
			List<string> deleted_file_list = new List<string>();

			// check client			
			string c_path = Path.Combine( Directory.GetCurrentDirectory(), DEV_CLIENT_PATH );
			if( Directory.Exists( c_path ) )
			{
				string[] dirs = Directory.GetDirectories( c_path );
				foreach( string dir in dirs )
				{
					string short_dir = Path.GetFileName( dir );
					if( short_dir.Contains( tb_client_currver.Text ) )
						continue;

					if( short_dir == DEV_CLIENT_DOC_PATH || short_dir == DEV_CLIENT_TEMP_PATH )
						continue;

					if( short_dir == DEV_DOWNLOAD_PATH )
					{
						string[] down_files = Directory.GetFiles( dir );
						foreach( string down_path in down_files )
						{
							string short_file = Path.GetFileNameWithoutExtension( down_path );
							if( short_file == tb_client_currver.Text )
								continue;

							CheckNewVer.VersionData v_data = CheckNewVer.Instance.FileName2VersionData( short_file );
							if( v_data.version == curr_v_data.version )
								continue;

							if( v_data.version <= delete_max_ver )
								deleted_file_list.Add( down_path );
						}
					}
					else
					{
						CheckNewVer.VersionData v_data = CheckNewVer.Instance.FileName2VersionData( short_dir );
						if( v_data.version == curr_v_data.version )
							continue;

						if( v_data.version <= delete_max_ver )
							deleted_path_list.Add( dir );
					}
				}
			}

			// check server
			string s_path = Path.Combine( Directory.GetCurrentDirectory(), DEV_SERVER_PATH );
			if( Directory.Exists( s_path ) )
			{
				string[] dirs = Directory.GetDirectories( s_path );
				foreach( string dir in dirs )
				{
					string short_dir = Path.GetFileName( dir );
					if( short_dir == tb_server_currver.Text )
						continue;

					if( short_dir == DEV_DOWNLOAD_PATH )
					{
						string[] down_files = Directory.GetFiles( dir );
						foreach( string down_path in down_files )
						{
							string short_file = Path.GetFileNameWithoutExtension( down_path );
							if( short_file == tb_server_currver.Text )
								continue;

							deleted_file_list.Add( down_path );
						}

						continue;
					}

					deleted_path_list.Add( dir );
				}
			}

			int all_count = deleted_path_list.Count + deleted_file_list.Count;
			if( all_count > 0 )
			{
				if( MessageBox.Show( string.Format( "{0}개의 이전 버전이있습니다.모두 삭제하시겠습니까?", all_count ), "Warning", MessageBoxButtons.OKCancel ) == DialogResult.OK )
				{
					DeleteRun( deleted_path_list, deleted_file_list );
				}
			}
			else
			{
				MessageBox.Show( "삭제할 버전이 없습니다.", "Delete" );
			}
		}
		async void DeleteRun( List<string> path_list, List<string> file_list )
		{
			ButtonLocked( true );

			await DeleteTask( path_list, file_list );

			ButtonLocked( false );

			MessageBox.Show( "모두 삭제되었습니다." );
		}
		async Task DeleteTask( List<string> path_list, List<string> file_list )
		{
			try
			{
				int count = path_list.Count + file_list.Count;
				string btn_text = btn_dev_clear_prevversion.Text;

				btn_dev_clear_prevversion.Text = string.Format( "{0} ({1})", btn_text, count );
				btn_dev_clear_prevversion.Update();
				foreach( string d in path_list )
				{
					Directory.Delete( d, true );
					count--;
					mLog.LogWrite( ">> DELETED : " + d );
					btn_dev_clear_prevversion.Text = string.Format( "{0} ({1})", btn_text, count );
					btn_dev_clear_prevversion.Update();

					await Task.Delay( 1 );
				}

				foreach( string f in file_list )
				{
					File.Delete( f );
					count--;
					mLog.LogWrite( ">> DELETED : " + f );
					btn_dev_clear_prevversion.Text = string.Format( "{0} ({1})", btn_text, count );
					btn_dev_clear_prevversion.Update();

					await Task.Delay( 1 );
				}

				btn_dev_clear_prevversion.Text = btn_text;
			}
			catch( System.Exception ex )
			{
				mLog.LogWrite( ex.ToString() );
			}
		}

		private void btn_download_cancel_Click( object sender, EventArgs e )
		{
			if( mCurrentWebClient == null )
				return;

			if( MessageBox.Show( "취소하시겠습니까?", "Download", MessageBoxButtons.OKCancel ) == DialogResult.Cancel )
				return;

			if( mCurrentWebClient != null )
				mCurrentWebClient.CancelAsync();

			mCurrentWebClient = null;
		}

		private void tb_client_currver_TextChanged( object sender, EventArgs e )
		{
			CheckNewVer.VersionData curr_client_version = CheckNewVer.Instance.FileName2VersionData<CheckNewVer.VersionData>( tb_client_currver.Text );
			string profile_path = System.Environment.GetEnvironmentVariable( "USERPROFILE" );
			string path = Path.Combine( profile_path, "AppData", "LocalLow", mProjectGlobaltypeProperty.launcher_client_company_name );

			btn_client_log_open.ContextMenu = null;
			if( Directory.Exists( path ) )
			{
				string[] sub_paths = Directory.GetDirectories( path );
				foreach( string sub in sub_paths )
				{
					if( sub.Contains( curr_client_version.version.ToString() ) && sub.Contains( curr_client_version.revision.ToString() ) && sub.Contains( curr_client_version.build_num.ToString() ) )
					{
						path = sub;
						break;
					}
				}

				btn_client_log_open.SetButtonPathContextMenu( path );
			}
		}
	}
}
