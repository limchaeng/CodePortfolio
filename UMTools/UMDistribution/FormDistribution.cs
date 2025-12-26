using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Xml;
using System.IO;
using System.Diagnostics;
using UMF.Core;
using UMTools.Common;
using UMF.Core.I18N;

namespace UMTools.Distribution
{
	public partial class FormDistribution : Form
	{
		public const string REG_SUB_KEY = "UMDistribution"; 
		const string REG_KEY_LAST_TAB = "last_tab";

		LogWriter mLog = null;

		ProjectPropertyConfig mProjectProperty = null;
		ProjectGlobaltypePropertyConfig mProjectGlobaltypeProperty = null;
		GlobalPropertyConfig mGlobalProperty = null;

		public FormDistribution()
		{
			InitializeComponent();

			mLog = new LogWriter( rtb_log );

			string last_tab = ToolUtil.GetPrefs<string>( REG_SUB_KEY, REG_KEY_LAST_TAB, "" );
			foreach(TabPage page in tabControl.TabPages)
			{
				if( page.Name == last_tab )
				{
					tabControl.SelectedTab = page;
					break;
				}
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
			this.Text = string.Format( "Distribution(VER:{0}) PROJECT : {1}({2})"
				, Assembly.GetExecutingAssembly().GetName().Version.ToString()
				, project_name, mProjectGlobaltypeProperty.GlobalType
				);
			lbl_projectname.Text = $"{project_name}({mProjectGlobaltypeProperty.GlobalType})";

			lbl_qa_tag_label.Text = string.Format( "TAG:{0}", mProjectGlobaltypeProperty.qa_tag_path );
			tb_qa_tagcreate_ver.Text = "Unknown";
			tb_qa_commitlive_version.Text = "Unknown";

			System.Version last_ver = null;

			string tag_parent_path = Path.Combine( mProjectGlobaltypeProperty.qa_tag_path, ".." );
			if( Directory.Exists( tag_parent_path ) )
			{
				string[] dirs = Directory.GetDirectories( tag_parent_path );
				
				foreach( string dir in dirs )
				{
					System.Version ver;
					string dir_name = Path.GetFileName( dir );
					if( System.Version.TryParse( dir_name, out ver ) )
					{
						if( last_ver == null || ver > last_ver )
							last_ver = ver;
					}
				}
			}

			if( last_ver != null )
				tb_qa_tagcreate_ver.Text = last_ver.ToString();

			last_ver = null;
			if( Directory.Exists( mProjectGlobaltypeProperty.svc_live_path ) )
			{
				string[] dirs = Directory.GetDirectories( mProjectGlobaltypeProperty.svc_live_path );
				foreach( string dir in dirs )
				{
					System.Version ver;
					string dir_name = Path.GetFileName( dir );
					if( System.Version.TryParse( dir_name, out ver ) )
					{
						if( last_ver == null || ver > last_ver )
							last_ver = ver;
					}
				}
			}

			if( last_ver != null )
				tb_qa_commitlive_version.Text = last_ver.ToString();

			UpdateButtonTooltips();
		}

		void UpdateButtonTooltips()
		{
			tooltip.RemoveAll();

			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_devsvnupdate, mProjectGlobaltypeProperty.dev_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_dev_update_dev_patch, mProjectGlobaltypeProperty.svc_dev_patch_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_tbl_svn_commit, mProjectGlobaltypeProperty.TBLPath );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_tbl_localizeall_compare_all, mProjectGlobaltypeProperty.TBLPath );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_tbl_data_compare_all, mProjectGlobaltypeProperty.TBLPath );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_dev_svc_server_update, mProjectGlobaltypeProperty.svc_dev_server_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_dev_svc_server_commit, mProjectGlobaltypeProperty.svc_dev_server_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_tbl_svn_update, mProjectGlobaltypeProperty.TBLPath );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_tbl_bin_copy, mProjectGlobaltypeProperty.TBLPath );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_qa_updatelive, mProjectGlobaltypeProperty.svc_live_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_qa_updaterv, mProjectGlobaltypeProperty.svc_review_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_qa_updateqa2, mProjectGlobaltypeProperty.svc_qa2_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_qa_updateqa, mProjectGlobaltypeProperty.svc_qa_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_qa_update_tag_dev, mProjectGlobaltypeProperty.qa_tag_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_qa_commit_tag_dev, mProjectGlobaltypeProperty.qa_tag_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_qa_merge_tag_dev, mProjectProperty.svn_trunk_dev_url );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_qa_commitqa2_patch, mProjectGlobaltypeProperty.svc_qa2_patch_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_qa_commitqa2, mProjectGlobaltypeProperty.svc_qa2_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_qa_switch_all, mProjectGlobaltypeProperty.tag_svn_url );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_qa_tagcreate, mProjectGlobaltypeProperty.tag_svn_url );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_qa_commitlive_patch, mProjectGlobaltypeProperty.svc_live_patch_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_qa_commitqa_patch, mProjectGlobaltypeProperty.svc_qa_patch_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_qa_commitlive, mProjectGlobaltypeProperty.svc_live_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_qa_commitrv, mProjectGlobaltypeProperty.svc_review_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_qa_commitqa, mProjectGlobaltypeProperty.svc_qa_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_open_jenkins, mProjectProperty.jenkins_url );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_client_work_commit, mProjectProperty.client_work_path, "\\Assets" );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_server_work_commit, mProjectProperty.server_work_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_server_work_update, mProjectProperty.server_work_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_client_work_update, mProjectProperty.client_work_path, "\\Assets" );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_svnlog_tag_get, mProjectGlobaltypeProperty.qa_tag_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_svnlog_get, mProjectGlobaltypeProperty.dev_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_dev_work_commit, mProjectProperty.dev_work_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_dev_work_update, mProjectProperty.dev_work_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_dev_tool_update, mGlobalProperty.tool_work_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_dev_tool_commit, mGlobalProperty.tool_work_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_dev_tool_distribution, mGlobalProperty.tool_dist_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_dev_umfunity_ext_commit, mProjectProperty.client_work_path + "\\Assets\\UMF.Unity" );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_dev_umf_commit, mGlobalProperty.umf_work_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_dev_umf_update, mGlobalProperty.umf_work_path );
			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_dev_umf_distribution, mGlobalProperty.umf_dist_path );

			UMToolsCommon.ButtonPathTooltipOrDisable( tooltip, btn_apple_ios_qa_upload, "" );
		}

		//------------------------------------------------------------------------
		const string SVN_REV_PREFIX = "##rev.";
		const string SVN_REV_SUFFIX = ".##";

		public class SVNRevisionData
		{
			public int revision = 0;
			public string gracefull_text = "";
		}
		public class SVNLogData
		{
			public int last_revision = 0;
			public string gracefull_text = "";
			public string log_xml = "";
			public List<SVNRevisionData> revision_list = new List<SVNRevisionData>();
		}
		SVNLogData GetSVNLogMessage( string path, int start_revision = 0, int end_revision = 0 )
		{
			SVNLogData svn_data = new SVNLogData();

			string args = "";
			if( start_revision > 0 )
				args = string.Format( "log {0} -r{2}:{1} --xml", path, start_revision, end_revision > 0 ? end_revision.ToString() : "HEAD" );
			else
				args = string.Format( "log {0} -l 1 --xml", path );

			svn_data.log_xml = ExecuteProcess( mGlobalProperty.svn_path, args ).TrimEnd();
			if( svn_data.log_xml.EndsWith( "</log>" ) == false )
				svn_data.log_xml += "</log>";

			mLog.LogWrite( svn_data.log_xml );
			StringBuilder sb_log = new StringBuilder();

			svn_data.last_revision = end_revision;

			try
			{
				XmlDocument doc = new XmlDocument();
				using( StringReader sr = new StringReader( svn_data.log_xml ) )
				{
					doc.Load( sr );
				}

				XmlNode log_node = doc.SelectSingleNode( "log" );
				foreach( XmlNode entry_node in log_node.SelectNodes( "logentry" ) )
				{
					int revision = XMLUtil.ParseAttribute<int>( entry_node, "revision", 0 );
					if( revision > svn_data.last_revision )
						svn_data.last_revision = revision;

					XmlNode msg_node = entry_node.SelectSingleNode( "msg" );

					string msg = msg_node.InnerText;
					string rev_text = string.Format( "[r{0}] ", revision );
					string space = "";
					for( int i = 0; i < rev_text.Length; i++ )
						space += " ";
					msg = msg.TrimEnd( '\r', '\n' );
					msg = msg.Replace( "\r\n", "\n" )
						.Replace( "\r", "\n" )
						.Replace( "\n\n", "\n" )
						.Replace( "\n", "\n" + space );

					string grace_msg = string.Format( "{0}{1}", rev_text, msg );
					sb_log.AppendLine( grace_msg );

					SVNRevisionData rev_data = new SVNRevisionData();
					rev_data.revision = revision;
					rev_data.gracefull_text = grace_msg;

					svn_data.revision_list.Add( rev_data );
				}
			}
			catch( System.Exception ex )
			{
				MessageBox.Show( ex.ToString() );
				sb_log.Clear();
			}

			svn_data.gracefull_text = string.Format( "{0}{1}{2}\n\n{3}", SVN_REV_PREFIX, svn_data.last_revision, SVN_REV_SUFFIX, sb_log.ToString() );

			return svn_data;
		}

		//------------------------------------------------------------------------
		string ExecuteProcess(string path, string args)
		{
			Process process = new Process()
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = path,
					Arguments = args,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true,
					StandardOutputEncoding = System.Text.Encoding.UTF8
				}
			};

			process.Start();
			string output = process.StandardOutput.ReadToEnd();
			process.WaitForExit();

			return output;
		}

		//------------------------------------------------------------------------
		void _translate_compare_process( bool is_server, bool is_merge )
		{
			if( is_server )
			{
				MessageBox.Show( "not implement." );
				return;
			}

			string translate_export_path = Path.Combine( mProjectGlobaltypeProperty.TBLPath, TBLInfoBase.EXPORT_PATH, I18NTextConst.DEFAULT_PATH_ROOT, I18NTextExportConst.TRANSLATION_PATH );
			if( is_server )
			{
				translate_export_path = "";
			}

			string[] sub_folders = Directory.GetDirectories( translate_export_path );
			if( sub_folders == null )
				return;

			string find_prefix = I18NTextExportConst.TRANSLATION_IMPORT_PATH_PREFIX;
			if( is_merge )
				find_prefix = I18NTextExportConst.TRANSLATION_MERGED_PATH_PREFIX;

			string last_find_path = "";
			int last_parent_date = 0;
			int last_parent_time = 0;
			int last_date = 0;
			foreach( string sub in sub_folders )
			{
				string dir_name = Path.GetFileName( sub );
				int dir_date;
				int dir_time;

				if( dir_name.Length <= 8 || int.TryParse( dir_name.Substring( 0, 8 ), out dir_date ) == false )
					dir_date = 0;

				if( dir_name.Length <= ( 8 + 1 + 6 ) || int.TryParse( dir_name.Substring( 9, 6 ), out dir_time ) == false )
					dir_time = 0;

				string[] export_paths = Directory.GetDirectories( sub );
				if( export_paths != null )
				{
					foreach( string path in export_paths )
					{
						string f_name = Path.GetFileName( path );
						if( f_name.StartsWith( find_prefix ) )
						{
							string f_name_date = f_name.Replace( find_prefix, "" );
							if( f_name_date.Length > 8 )
								f_name_date = f_name_date.Substring( 0, 8 );	// xml_20240315_xxxx

							int _date;
							if( int.TryParse( f_name_date, out _date ) )
							{
								if( last_date == 0 || _date > last_date )
								{
									last_parent_date = dir_date;
									last_parent_time = dir_time;
									last_date = _date;
									last_find_path = path;
								}
								else if( _date == last_date )
								{
									if( dir_date > last_parent_date || ( dir_date == last_parent_date && dir_time > last_parent_time ) )
									{
										last_parent_date = dir_date;
										last_parent_time = dir_time;
										last_date = _date;
										last_find_path = path;
									}
								}
							}
						}
					}
				}
			}

			if( string.IsNullOrEmpty( last_find_path ) == false )
			{
				string c1_path = "";
				string c2_path = "";

				if( is_server )
				{
					//c1_path = Path.Combine( last_find_path, "ServerText" );
					//c2_path = Path.Combine( mSetting.server_work_path, "_ConfigLocal", mSetting.globaldata.global_type.ToString() );
				}
				else
				{
					c1_path = Path.Combine( last_find_path, I18NTextConst.DEFAULT_PATH_LANGUAGES );
					c2_path = Path.Combine( mProjectProperty.client_work_path, "Assets", "_DATA", mProjectGlobaltypeProperty.GlobalType, TBLInfoBase.DEFAULT_PATH_ROOT, I18NTextConst.DEFAULT_PATH_ROOT, I18NTextConst.DEFAULT_PATH_LANGUAGES );
				}

				string args = string.Format( "/r {0} {1}", c1_path, c2_path );
				ExecuteProcess( mGlobalProperty.mergetool_path, args );
			}
			else
			{
				MessageBox.Show( "비교할 마지막 번역 폴더를 찾을수 없습니다." );
			}
		}


		private void btn_close_Click( object sender, EventArgs e )
		{
			Close();
		}

		private void btn_tbl_svn_update_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:update /path:{0}", mProjectGlobaltypeProperty.TBLPath );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void tabControl_SelectedIndexChanged( object sender, EventArgs e )
		{
			string curr_tab_name = tabControl.SelectedTab.Name;
			ToolUtil.SavePrefs( REG_SUB_KEY, REG_KEY_LAST_TAB, curr_tab_name );
		}

		private void btn_client_work_commit_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:commit /path:{0}\\Assets", mProjectProperty.client_work_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_open_jenkins_Click( object sender, EventArgs e )
		{
			Process.Start( mProjectProperty.jenkins_url );
		}

		private void btn_dev_svc_server_commit_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:commit /path:{0}", mProjectGlobaltypeProperty.svc_dev_server_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_qa_commitqa_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:commit /path:{0}", mProjectGlobaltypeProperty.svc_qa_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_qa_commitrv_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:commit /path:{0}", mProjectGlobaltypeProperty.svc_review_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_qa_commitlive_Click( object sender, EventArgs e )
		{
			string live_version = tb_qa_commitlive_version.Text;
			if( string.IsNullOrEmpty(live_version) )
			{
				MessageBox.Show( "Live Version Text empty!", "WARNING" );
				return;					
			}

			string path = Path.Combine( mProjectGlobaltypeProperty.svc_live_path, live_version );
			if( Directory.Exists(path) == false )
			{
				MessageBox.Show( string.Format( "Live Path [{0}] not found!", path ), "WARNING" );
				return;
			}

			string args = string.Format( "/command:commit /path:{0}", path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_dev_update_dev_patch_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:update /path:{0}", mProjectGlobaltypeProperty.svc_dev_patch_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_qa_commitqa_patch_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:commit /path:{0}", mProjectGlobaltypeProperty.svc_qa_patch_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_qa_commitlive_patch_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:commit /path:{0}", mProjectGlobaltypeProperty.svc_live_patch_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_qa_tagcreate_Click( object sender, EventArgs e )
		{
			if( string.IsNullOrEmpty(tb_qa_tagcreate_ver.Text) )
			{
				MessageBox.Show( "Tag Create Version needed!" );
				return;
			}

			// dev
			string tag_url = string.Format( "{0}/{1}", mProjectGlobaltypeProperty.tag_svn_url, tb_qa_tagcreate_ver.Text );
			string log_message = string.Format( "====={0}=====", tb_qa_tagcreate_ver.Text );
			string args = string.Format( "/command:copy /path:{0} /url:{1} /logmsg:{2}", mProjectProperty.dev_work_path, tag_url, log_message );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_qa_switch_all_Click( object sender, EventArgs e )
		{
			string tag_url = string.Format( "{0}/{1}", mProjectGlobaltypeProperty.tag_svn_url, tb_qa_tagcreate_ver.Text );
			string args = string.Format( "/command:switch /path:{0} /url:{1}", mProjectGlobaltypeProperty.qa_tag_path, tag_url );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );

			string ver_folder_path = Path.Combine( mProjectGlobaltypeProperty.qa_tag_path, "..", tb_qa_tagcreate_ver.Text );
			if( Directory.Exists( ver_folder_path ) == false )
				Directory.CreateDirectory( ver_folder_path );
		}

		private void btn_qa_commitqa2_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:commit /path:{0}", mProjectGlobaltypeProperty.svc_qa2_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_apple_ios_qa_upload_Click( object sender, EventArgs e )
		{
			FormAppleUploader.LoaderData data = new FormAppleUploader.LoaderData();
// 			data.SSH_HOST = mProjectGlobaltypeProperty.apple_ssh_host;
// 			data.SSH_ID = mProjectGlobaltypeProperty.apple_ssh_username;
// 			data.SSH_PW = mProjectGlobaltypeProperty.apple_ssh_password;
// 			data.work_path = mProjectGlobaltypeProperty.apple_ssh_work_path_qa;

			FormAppleUploader loader = new FormAppleUploader( data );
			loader.ShowDialog();
		}

		private void btn_dev_svc_server_update_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:update /path:{0}", mProjectGlobaltypeProperty.svc_dev_server_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_qa_commitqa2_patch_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:commit /path:{0}", mProjectGlobaltypeProperty.svc_qa2_patch_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_server_work_commit_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:commit /path:{0}", mProjectProperty.server_work_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_tbl_bin_copy_Click( object sender, EventArgs e )
		{
			List<string> copy_path_list = new List<string>();
			copy_path_list.Add( TBLInfoBase.EXPORT_PATH_CLIENT );
			copy_path_list.Add( TBLInfoBase.EXPORT_PATH_BINARY );
			copy_path_list.Add( TBLInfoBase.EXPORT_PATH_BINARY_ENCRYPT );
			copy_path_list.Add( TBLInfoBase.EXPORT_PATH_ENCRYPT );

			List<string> output = new List<string>();

			foreach(string copy_path in copy_path_list)
			{
				string bin_path = Path.Combine( mProjectGlobaltypeProperty.TBLPath, TBLInfoBase.EXPORT_PATH, copy_path );
				string bin_target_path = Path.Combine( mProjectProperty.client_work_path, "Assets", "_DATA", mProjectGlobaltypeProperty.GlobalType, TBLInfoBase.DEFAULT_PATH_ROOT, copy_path );
				if( MessageBox.Show( string.Format( "copy {0} to {1}. OK?", bin_path, bin_target_path ), "COPY", MessageBoxButtons.OKCancel ) == DialogResult.OK )
					output.AddRange( ToolUtil.DirectoryCopy( bin_path, bin_target_path, true, false, pgbar_tbl_bin_copy, lbl_tbl_bin_copy_count ) );
			}

			if( output.Count > 0 )
			{
				StringBuilder sb = new StringBuilder();
				output.ForEach( a => sb.AppendLine( a ) );

				FormExportPopup pop = new FormExportPopup();
				pop.SetExportText( sb.ToString() );
				pop.ShowDialog();
			}
		}

		private void btn_tbl_data_compare_all_Click( object sender, EventArgs e )
		{
			string c1_path = Path.Combine( mProjectGlobaltypeProperty.TBLPath, TBLInfoBase.EXPORT_PATH );
			string c2_path = Path.Combine( mProjectProperty.client_work_path, "Assets", "_DATA", mProjectGlobaltypeProperty.GlobalType, TBLInfoBase.DEFAULT_PATH_ROOT );
			string args = string.Format( "/r {0} {1}", c1_path, c2_path );
			ExecuteProcess( mGlobalProperty.mergetool_path, args );
		}

		private void btn_tbl_localizeall_compare_all_Click( object sender, EventArgs e )
		{
			string c1_path = Path.Combine( mProjectGlobaltypeProperty.TBLPath, TBLInfoBase.EXPORT_PATH, I18NTextConst.DEFAULT_PATH_ROOT, I18NTextConst.DEFAULT_PATH_LANGUAGES );
			string c2_path = Path.Combine( mProjectProperty.client_work_path, "Assets", "_DATA", mProjectGlobaltypeProperty.GlobalType, TBLInfoBase.DEFAULT_PATH_ROOT, I18NTextConst.DEFAULT_PATH_ROOT, I18NTextConst.DEFAULT_PATH_LANGUAGES );
			string args = string.Format( "/r {0} {1}", c1_path, c2_path );
			ExecuteProcess( mGlobalProperty.mergetool_path, args );

			// TODO : server text
// 			c1_path = Path.Combine( mProjectGlobaltypeProperty.TBLPath, "Export", "ServerLocalize", "Language" );
// 			c2_path = Path.Combine( mProjectProperty.server_work_path, "_ConfigLocal", mProjectGlobaltypeProperty.GlobalType );
// 			args = string.Format( "/r {0} {1}", c1_path, c2_path );
// 			ExecuteProcess( mGlobalProperty.mergetool_path, args );
		}

		private void btn_client_work_update_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:update /path:{0}\\Assets", mProjectProperty.client_work_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_server_work_update_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:update /path:{0}", mProjectProperty.server_work_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_tbl_svn_commit_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:commit /path:{0}", mProjectGlobaltypeProperty.TBLPath );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_svnlog_get_Click( object sender, EventArgs e )
		{
			int end_rev = -1;
			if( int.TryParse( tb_svnlog_get_revision_end.Text, out end_rev ) == false )
				end_rev = -1;

			int start_rev = 0;
			if( int.TryParse( tb_svnlog_get_revision_start.Text, out start_rev ) == false )
				start_rev = 0;

			SVNLogData svn_data = GetSVNLogMessage( mProjectGlobaltypeProperty.dev_path, start_rev, end_rev );
			SVNLogData svn_tbl_data = GetSVNLogMessage( mProjectGlobaltypeProperty.TBLPath, start_rev, end_rev );

			List<SVNRevisionData> rev_list = new List<SVNRevisionData>();
			rev_list.AddRange( svn_data.revision_list );
			rev_list.AddRange( svn_tbl_data.revision_list );

			rev_list = rev_list.OrderByDescending( a => a.revision ).ToList();
			int last_revision = svn_data.last_revision;
			if( svn_tbl_data.last_revision > last_revision )
				last_revision = svn_tbl_data.last_revision;

			StringBuilder full_msg = new StringBuilder();

			full_msg.AppendLine( string.Format( "{0}{1}{2}\n", SVN_REV_PREFIX, last_revision, SVN_REV_SUFFIX ) );

			foreach( SVNRevisionData r_data in rev_list )
				full_msg.AppendLine( r_data.gracefull_text );

			FormExportPopup pop = new FormExportPopup();
			pop.SetExportText( svn_data.gracefull_text );
			pop.ShowDialog();
		}

		private void btn_svnlog_tag_get_Click( object sender, EventArgs e )
		{
			int end_rev = -1;
			if( int.TryParse( tb_svnlog_get_revision_end.Text, out end_rev ) == false )
				end_rev = -1;

			int start_rev = 0;
			if( int.TryParse( tb_svnlog_get_revision_start.Text, out start_rev ) == false )
				start_rev = 0;

			string qa_tag_path = mProjectGlobaltypeProperty.qa_tag_path;

			SVNLogData svn_data = GetSVNLogMessage( qa_tag_path, start_rev, end_rev );
			FormExportPopup pop = new FormExportPopup();
			pop.SetExportText( svn_data.gracefull_text );
			pop.ShowDialog();
		}

		private void btn_dev_work_commit_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:commit /path:{0}", mProjectProperty.dev_work_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_dev_work_update_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:update /path:{0}", mProjectProperty.dev_work_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_qa_merge_tag_dev_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:merge /fromurl:{0} /path:{1}", mProjectProperty.svn_trunk_dev_url, mProjectGlobaltypeProperty.qa_tag_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_qa_commit_tag_dev_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:commit /path:{0}", mProjectGlobaltypeProperty.qa_tag_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_qa_update_tag_dev_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:update /path:{0}", mProjectGlobaltypeProperty.qa_tag_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_devsvnupdate_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:update /path:{0}\\DEV", mProjectGlobaltypeProperty.dev_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_devsvncommit_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:commit /path:{0}\\DEV", mProjectGlobaltypeProperty.dev_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_dev_tool_commit_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:commit /path:{0}", mGlobalProperty.tool_work_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_dev_tool_update_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:update /path:{0}", mGlobalProperty.tool_work_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_dev_tool_distribution_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:commit /path:{0}", mGlobalProperty.tool_dist_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_qa_updateqa_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:update /path:{0}", mProjectGlobaltypeProperty.svc_qa_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_qa_updateqa2_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:update /path:{0}", mProjectGlobaltypeProperty.svc_qa2_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_qa_updaterv_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:update /path:{0}", mProjectGlobaltypeProperty.svc_review_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_qa_updatelive_Click( object sender, EventArgs e )
		{
			string live_version = tb_qa_commitlive_version.Text;
			if( string.IsNullOrEmpty( live_version ) )
			{
				MessageBox.Show( "Live Version Text empty!", "WARNING" );
				return;
			}

			string path = Path.Combine( mProjectGlobaltypeProperty.svc_live_path, live_version );
			if( Directory.Exists( path ) == false )
			{
				MessageBox.Show( string.Format( "Live Path [{0}] not found!", path ), "WARNING" );
				return;
			}

			string args = string.Format( "/command:update /path:{0}", path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );

		}

		private void btn_qa_update_dev_patch_Click_1( object sender, EventArgs e )
		{

		}

		private void btn_project_select_Click( object sender, EventArgs e )
		{
			using( FormProjectProperty form = new FormProjectProperty() )
			{
				form.ShowDialog();
			}

			AllRefresh();
		}

		private void btn_dev_umf_commit_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:commit /path:{0}", mGlobalProperty.umf_work_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_dev_umf_update_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:update /path:{0}", mGlobalProperty.umf_work_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_dev_umf_distribution_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:commit /path:{0}", mGlobalProperty.umf_dist_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_dev_umfunity_ext_commit_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:commit /path:{0}\\Assets\\UMF.Unity", mProjectProperty.client_work_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_config_compare_all_Click( object sender, EventArgs e )
		{
			string c1_path = mProjectGlobaltypeProperty.ConfigPath;
			string c2_suffix = Path.GetFileName( mProjectGlobaltypeProperty.ConfigPath );
			string c2_path = Path.Combine( mProjectProperty.client_work_path, "Assets", "_DATA", mProjectGlobaltypeProperty.GlobalType, c2_suffix );
			string args = string.Format( "/r {0} {1}", c1_path, c2_path );
			ExecuteProcess( mGlobalProperty.mergetool_path, args );
		}

		private void btn_trunksvnupdate_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:update /path:{0}", mProjectGlobaltypeProperty.dev_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_trunksvncommt_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:commit /path:{0}", mProjectGlobaltypeProperty.dev_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_docsvnupdate_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:update /path:{0}\\DOC", mProjectGlobaltypeProperty.dev_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_docsvncommit_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:commit /path:{0}\\DOC", mProjectGlobaltypeProperty.dev_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_artsvnupdate_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:update /path:{0}\\ART", mProjectGlobaltypeProperty.dev_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_artsvncommit_Click( object sender, EventArgs e )
		{
			string args = string.Format( "/command:commit /path:{0}\\ART", mProjectGlobaltypeProperty.dev_path );
			ExecuteProcess( mGlobalProperty.tsvn_path, args );
		}

		private void btn_i18ntext_t_compare_client_Click( object sender, EventArgs e )
		{
			_translate_compare_process( false, false );
		}

		private void btn_i18ntext_m_compare_client_Click( object sender, EventArgs e )
		{
			_translate_compare_process( false, true );
		}
	}
}

