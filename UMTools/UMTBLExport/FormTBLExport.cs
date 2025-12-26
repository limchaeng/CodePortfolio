using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Security.Cryptography;
using UMTools.Common;
using UMF.Core;
using UMF.Core.I18N;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace UMTools.TBLExport
{
	public partial class FormTBLExport : Form
	{
		bool mExportProgress = false;

		public enum eLogType
		{
			Normal,
			Error,
			Important,
		}

		public enum eFileStatus
		{
			Normal,
			Busy,
		}
		
		public class FileData
		{
			public string filePath;
			public eFileStatus status;
		}

		public class ExcelCellColorData
		{
			public string m_Language;
			public int m_Row;
			public int m_Col;
			public Color m_Color;
		}
		public class RowCellData
		{
			public DataRow m_Row;
			public int m_CellRowIndex;
		}

		// tabControl's index
		enum eTABType
		{
			None = -1,
			TBLExport = 0,
			I18NTextExport = 1,
		}

		public enum eOptionEnableType
		{
			OPT_I18NText_ExportDuplicateInclude,
		}

		public const string KOREAN_LANGUAGE = "Korean";
		readonly string CHANGEDONLY = "_CHANGEDONLY";
		readonly string DUPLICATED_SUFFIX = "_DUP";
		readonly string FULL_SEP = "_FULLSEP";
		readonly string DIFFONLY = "_DIFFONLY";

		const string REG_SUB_KEY = "UMTBLExport";
		const string REG_KEY_LAST_EXCEL2XML_PATH = "_TExcel2XmlPath_";

		public string LastTranslateExcel2XmlPath
		{
			get
			{
				string path = ToolUtil.GetPrefs<string>( REG_SUB_KEY, REG_KEY_LAST_EXCEL2XML_PATH, "" );
				if( string.IsNullOrEmpty( path ) || Directory.Exists( path ) == false )
					path = Path.Combine( mProjectGlobaltypeProperty.TBLPath, TBLInfoBase.EXPORT_PATH, I18NTextConst.DEFAULT_PATH_ROOT, I18NTextExportConst.TRANSLATION_PATH );

				return path;
			}

			set { ToolUtil.SavePrefs( REG_SUB_KEY, REG_KEY_LAST_EXCEL2XML_PATH, value ); }
		}


		List<FileData> mFilePathList = new List<FileData>();
		public List<FileData> FilePathList { get { return mFilePathList; } }
		LogWriter mLogWriter = null;

		TBLExportTranslate mTranslate = null;
		public TBLExportTranslate ExportTranslate { get { return mTranslate; } }

		ProjectPropertyConfig mProjectProperty = null;
		ProjectGlobaltypePropertyConfig mProjectGlobaltypeProperty = null;
		GlobalPropertyConfig mGlobalProperty = null;

		public ProjectPropertyConfig ProjectProperty { get { return mProjectProperty; } }


		public FormTBLExport()
		{
			InitializeComponent();

			TranslateManager.Instance.Init( this );
			mTranslate = new TBLExportTranslate( this );
			mLogWriter = new LogWriter( rtb_log, "UMTBLExport.log" );
			Log.SetAll( LogWrite_2_Text );

			if( ProjectConfig.Instance.QuickSave.IsValid() == false )
			{
				FormProjectProperty.Open();
			}

			toolTip.SetToolTip( radio_i18n_xml2excel, "TBL Export 된 I18NText 들을 엑셀로 EXPORT." );
			toolTip.SetToolTip( radio_i18n_excel2xml, "번역된 엑셀파일을 XML 파일로 IMPORT." );
			toolTip.SetToolTip( radio_i18n_merge, "번역이 완료된 이전 XML 파일과 현재 XML을 병합하여 엑셀로 EXPORT." );

			foreach( eOptionEnableType opt_type in Enum.GetValues( typeof( eOptionEnableType ) ) )
			{
				switch( opt_type )
				{
					case eOptionEnableType.OPT_I18NText_ExportDuplicateInclude:
						cb_i18ntext_duplicate_include.Checked = ToolUtil.GetPrefs<bool>( REG_SUB_KEY, opt_type.ToString(), false );
						break;
				}
			}

			AllRefresh();
		}

		bool IsCurrentTab( eTABType tab )
		{
			return tabControl.SelectedIndex == (int)tab;
		}

		public bool IsOptionEnabled( eOptionEnableType opt )
		{
			switch( opt )
			{
				case eOptionEnableType.OPT_I18NText_ExportDuplicateInclude:
					return cb_i18ntext_duplicate_include.Enabled && cb_i18ntext_duplicate_include.Checked;
			}

			return false;
		}

		private void btn_Refresh_Click(object sender, EventArgs e)
		{
			AllRefresh();
		}

		private void btn_export_Click(object sender, EventArgs e)
		{
			OnTBLExport(-1, true, false);
		}

		private void btn_close_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		void ButtonsEnable( bool bEnable)
		{
			btn_close.Enabled = bEnable;
			btn_Refresh.Enabled = bEnable;
			btn_export.Enabled = bEnable;
			btn_localizeOnlyExport.Enabled = bEnable;

			radio_i18n_xml2excel.Enabled = bEnable;
			radio_i18n_excel2xml.Enabled = bEnable;
			radio_i18n_merge.Enabled = bEnable;
			btn_i18n_export.Enabled = bEnable;
			btn_i18ntext_summary.Enabled = bEnable;
		}

		//------------------------------------------------------------------------	
		public void LogWrite_2_Text(string str_log)
		{
			LogWrite_2_Text( str_log, null );
		}
		public void LogWrite_2_Text(string fmt, params object[] parms)
		{
			string log = "";
			if( parms == null || parms.Length <= 0 )
				log = fmt;
			else
				log = string.Format( fmt, parms );

			mLogWriter.LogWrite( log );
		}

		//------------------------------------------------------------------------
		public string LogWrite_2_List(eLogType log_type, string fmt, params object[] parms)
		{
			string ret_log = "";
			string log_type_str = "";
			Color log_color = Color.Black;
			switch(log_type)
			{
				case eLogType.Normal:
					log_type_str = "N";
					break;

				case eLogType.Error:
					log_type_str = "E";
					log_color = Color.Red;
					break;

				case eLogType.Important:
					log_type_str = "I";
					log_color = Color.Blue;
					break;
			}

			string log = "";
			if( parms != null && parms.Length > 0 )
			{
				ret_log = string.Format( fmt, parms );
				log = string.Format( "[{0}][{1}] {2}", log_type_str, DateTime.Now, string.Format( fmt, parms ) );
			}
			else
			{
				ret_log = fmt;
				log = string.Format( "[{0}][{1}] {2}", log_type_str, DateTime.Now, fmt );
			}

			mLogWriter.LogWrite( log );

			ListViewItem lv_item = new ListViewItem( log );
			lv_item.ForeColor = log_color;
			lv_item.ToolTipText = log;
			lv_log.Items.Add( lv_item );
			lv_log.EnsureVisible( lv_log.Items.Count - 1 );

			return ret_log;
		}

		//------------------------------------------------------------------------
		void AllRefresh()
		{ 
			progress_export.Value = 0;
			lv_log.Items.Clear();

			mGlobalProperty = ProjectConfig.Instance.GlobalProperty;
			mProjectProperty = ProjectConfig.Instance.CurrentProjectProerty;
			mProjectGlobaltypeProperty = ProjectConfig.Instance.CurrentProjectGlobaltypeProperty;

			if( Directory.Exists( mProjectGlobaltypeProperty.TBLPath ) == false )
			{
				MessageBox.Show( $"테이블 경로를 확인하세요.{mProjectGlobaltypeProperty.TBLPath}", $"{mProjectGlobaltypeProperty.ProjectName}" );
				return;
			}

			ProjectConfig.Data project_data = ProjectConfig.Instance.CurrentProjectData;

			string project_name = $"{mProjectGlobaltypeProperty.ProjectName}";
			if( string.IsNullOrEmpty( project_data.ProjectSubName ) == false )
				project_name += $"-{project_data.ProjectSubName}";

			this.BackColor = ToolUtil.GetColorFromHEX( project_data.FormColor );
			this.Text = string.Format("TBL EXPORT(VER:{0}) PROJECT : {1}({2})"
				, Assembly.GetExecutingAssembly().GetName().Version.ToString()
				, project_name, mProjectGlobaltypeProperty.GlobalType
				);
			lbl_projectname.Text = $"{project_name}({mProjectGlobaltypeProperty.GlobalType})";

			mTranslate.RefreshSupportedLanguage( mProjectGlobaltypeProperty.TBLPath );
			clb_supportLanguage.Items.Clear();
			foreach( TBLExportTranslate.LanguageCultureData lan_data in mTranslate.SupportedLanguages )
			{
				clb_supportLanguage.Items.Add( lan_data.language, true );
			}

			tb_pathtext.Text = mProjectGlobaltypeProperty.TBLPath;

			RefreshFileList();
		}

		//------------------------------------------------------------------------
		void RefreshFileList()
		{
			lv_filelist.Items.Clear();

			if( Directory.Exists( mProjectGlobaltypeProperty.TBLPath ) == false )
			{
				MessageBox.Show( $"테이블 경로를 확인하세요.{mProjectGlobaltypeProperty.TBLPath}", $"{mProjectGlobaltypeProperty.ProjectName}" );
				return;
			}

			mFilePathList.Clear();

			if( IsCurrentTab( eTABType.TBLExport ) )
			{
				string[] filenames = Directory.GetFiles( mProjectGlobaltypeProperty.TBLPath );

				for( int i = 0; i < filenames.Length; i++ )
				{
					string filepath = filenames[i];
					if( Path.GetExtension( filepath ).ToLower() == ".xlsx" )
					{
						if( Path.GetFileName( filepath ).StartsWith( "~$" ) == false )
						{
							FileData fd = new FileData();
							fd.filePath = filepath;
							fd.status = eFileStatus.Normal;

							FileInfo fi = null;
							FileStream fs = null;
							try
							{
								fi = new FileInfo( filepath );
								fs = fi.Open( FileMode.Open );
							}
							catch( System.Exception ex )
							{
								fd.status = eFileStatus.Busy;
							}
							finally
							{
								if( fs != null )
									fs.Close();
							}

							int listIdx = mFilePathList.Count;
							mFilePathList.Add( fd );

							ListViewItem lv_item = new ListViewItem( Path.GetFileName( fd.filePath ) );
							ListViewItem.ListViewSubItem subItem = new ListViewItem.ListViewSubItem();
							subItem.Text = fd.status.ToString();
							if( fd.status == eFileStatus.Busy )
							{
								subItem.BackColor = Color.Red;
								subItem.ForeColor = Color.White;
							}
							lv_item.SubItems.Add( subItem );
							lv_item.SubItems.Add( "" );

							string export_xml_path = Path.Combine( mProjectGlobaltypeProperty.TBLPath, TBLInfoBase.EXPORT_PATH, TBLInfoBase.EXPORT_PATH_XML );
							string prev_exportFileName = Path.Combine( export_xml_path, Path.GetFileNameWithoutExtension( filepath ) + ".xml" );

							lv_item.SubItems.Add( GetTBLPrevVersion( prev_exportFileName ).ToString() );

							lv_item.UseItemStyleForSubItems = false;
							lv_item.ToolTipText = fd.filePath;

							lv_filelist.Items.Add( lv_item );
						}
					}
				}
			}
			else if( IsCurrentTab( eTABType.I18NTextExport ) )
			{
				cb_i18ntext_duplicate_include.Enabled = radio_i18n_xml2excel.Checked;

				if( radio_i18n_excel2xml.Checked )
				{
					string excel_path = LastTranslateExcel2XmlPath;

					folderBrowserDialog.SelectedPath = excel_path;
					folderBrowserDialog.RootFolder = Environment.SpecialFolder.Desktop;
					folderBrowserDialog.ShowDialog();

					excel_path = folderBrowserDialog.SelectedPath;

					if( string.IsNullOrEmpty( excel_path ) || Directory.Exists( excel_path ) == false )
					{
						LastTranslateExcel2XmlPath = "";
						MessageBox.Show( "최종 번역된 엑셀 파일의 위치를 선택하세요." );
						return;
					}

					LastTranslateExcel2XmlPath = excel_path;

					string[] filenames = Directory.GetFiles( excel_path );

					for( int i = 0; i < filenames.Length; i++ )
					{
						string filepath = filenames[i];
						if( Path.GetExtension( filepath ).ToLower() == ".xlsx" )
						{
							// specific file name "_CHANGEDONLY.xlsx";
							if( Path.GetFileName( filepath ).Contains( CHANGEDONLY ) == true )
								continue;

							if( Path.GetFileName( filepath ).Contains( FULL_SEP ) == true )
								continue;

							if( Path.GetFileName( filepath ).Contains( DIFFONLY ) == true )
								continue;

							if( Path.GetFileName( filepath ).StartsWith( "~$" ) == false )
							{
								FileData fd = new FileData();
								fd.filePath = filepath;
								fd.status = eFileStatus.Normal;

								FileInfo fi = null;
								FileStream fs = null;
								try
								{
									fi = new FileInfo( filepath );
									fs = fi.Open( FileMode.Open );
								}
								catch( System.Exception ex )
								{
									fd.status = eFileStatus.Busy;
								}
								finally
								{
									if( fs != null )
										fs.Close();
								}

								int listIdx = mFilePathList.Count;
								mFilePathList.Add( fd );

								ListViewItem lv_item = new ListViewItem( Path.GetFileName( fd.filePath ) );
								ListViewItem.ListViewSubItem subItem = new ListViewItem.ListViewSubItem();
								subItem.Text = fd.status.ToString();
								if( fd.status == eFileStatus.Busy )
								{
									subItem.BackColor = Color.Red;
									subItem.ForeColor = Color.White;
								}
								lv_item.SubItems.Add( subItem );
								lv_item.SubItems.Add( "" );

								string exportFolder = Path.Combine( mProjectGlobaltypeProperty.TBLPath, TBLInfoBase.EXPORT_PATH, TBLInfoBase.EXPORT_PATH_XML );
								string tbl_filename = Path.GetFileNameWithoutExtension( filepath ).Replace( "_Text_" + FormTBLExport.KOREAN_LANGUAGE, "" ) + ".xml";
								string prev_exportFileName = Path.Combine( exportFolder, tbl_filename );

								lv_item.SubItems.Add( GetTBLPrevVersion( prev_exportFileName ).ToString() );

								lv_item.UseItemStyleForSubItems = false;
								lv_item.ToolTipText = fd.filePath;

								lv_filelist.Items.Add( lv_item );
							}
						}
					}
				}
				else
				{
					string i18ntext_def_lan_path = Path.Combine( mProjectGlobaltypeProperty.TBLPath, TBLInfoBase.EXPORT_PATH, I18NTextConst.DEFAULT_PATH_ROOT, I18NTextConst.DEFAULT_PATH_LANGUAGES, KOREAN_LANGUAGE );

					string[] filenames = Directory.GetFiles( i18ntext_def_lan_path );

					for( int i = 0; i < filenames.Length; i++ )
					{
						string filepath = filenames[i];
						if( Path.GetExtension( filepath ).ToLower() == ".xml" )
						{
							if( Path.GetFileName( filepath ).StartsWith( "~$" ) == false )
							{
								FileData fd = new FileData();
								fd.filePath = filepath;
								fd.status = eFileStatus.Normal;

								FileInfo fi = null;
								FileStream fs = null;
								try
								{
									fi = new FileInfo( filepath );
									fs = fi.Open( FileMode.Open );
								}
								catch( System.Exception ex )
								{
									fd.status = eFileStatus.Busy;
								}
								finally
								{
									if( fs != null )
										fs.Close();
								}

								int listIdx = mFilePathList.Count;
								mFilePathList.Add( fd );

								ListViewItem lv_item = new ListViewItem( Path.GetFileName( fd.filePath ) );
								ListViewItem.ListViewSubItem subItem = new ListViewItem.ListViewSubItem();
								subItem.Text = fd.status.ToString();
								if( fd.status == eFileStatus.Busy )
								{
									subItem.BackColor = Color.Red;
									subItem.ForeColor = Color.White;
								}
								lv_item.SubItems.Add( subItem );
								lv_item.SubItems.Add( "" );

								lv_item.SubItems.Add( GetI18NTextCurrentVersion( filepath ).ToString() );

								lv_item.UseItemStyleForSubItems = false;
								lv_item.ToolTipText = fd.filePath;

								lv_filelist.Items.Add( lv_item );
							}
						}
					}
				}
			}
		}

		//------------------------------------------------------------------------
		public bool IsSupportedLanguageChecked(string language)
		{
			foreach(object checked_item in clb_supportLanguage.CheckedItems)
			{
				if( checked_item.ToString() == language )
					return true;
			}

			return false;
		}

		//------------------------------------------------------------------------
		string mLastFileName = "";
		void OnTBLExport(int listidx, bool is_export, bool text_only)
		{
			if( mFilePathList.Count <= 0 )
				return;
			
			ProjectConfig.Data project_data = ProjectConfig.Instance.CurrentProjectData;
			string global_type = ProjectConfig.Instance.QuickSave.LastGlobalType;

			if( MessageBox.Show( $"EXPORT Project:{mProjectGlobaltypeProperty.ProjectName} GlobalType:{global_type} Folder:{mProjectGlobaltypeProperty.TBLPath}\nAre you OK?", "Warning", MessageBoxButtons.OKCancel ) == DialogResult.Cancel )
 				return;

			Cursor.Current = Cursors.WaitCursor;

			ButtonsEnable( false );
			mExportProgress = true;

			string tbl_path = mProjectGlobaltypeProperty.TBLPath;

			try
			{
				if( listidx == -1 )
				{
					for( int i = 0; i < lv_filelist.Items.Count; i++)
					{
						lv_filelist.Items[i].SubItems[2].Text = "";
					}
					lv_filelist.Update();

					progress_export.Maximum = mFilePathList.Count * 100;
					progress_export.Value = 0;
					progress_export.Update();
					for(int i=0; i<mFilePathList.Count; i++)
					{
						mLastFileName = mFilePathList[i].filePath;
						DoExport( mFilePathList[i].filePath, tbl_path, i * 100, i, is_export, text_only );
					}

					progress_export.Value = progress_export.Maximum;
					progress_export.Update();
				}
				else
				{
					lv_filelist.Items[listidx].SubItems[2].Text = "";
					lv_filelist.Update();
					progress_export.Maximum = 100;
					progress_export.Value = 0;
					progress_export.Update();
					mLastFileName = mFilePathList[listidx].filePath;
					DoExport( mFilePathList[listidx].filePath, tbl_path, 0, listidx, is_export, text_only );

					progress_export.Value = progress_export.Maximum;
					progress_export.Update();
				}
			}
			catch (System.Exception ex)
			{
				LogWrite_2_List(eLogType.Error, string.Format("({0}){1}", mLastFileName, ex.ToString()));
				MessageBox.Show(ex.ToString());
			}
			finally
			{
				ButtonsEnable( true );
				mExportProgress = false;
			}

			Cursor.Current = Cursors.Default;
		}

		//------------------------------------------------------------------------
		I18NTextData CheckI18NTextField(string category, DataSet data_set)
		{
			I18NTextData text_data = null;
			int textFieldColumnIdx = -1;
			foreach(DataTable table in data_set.Tables)
			{
				if( table.TableName == "Define" )
				{
					DataRow fieldRow = table.Rows[0];
					for( int col = 0; col < table.Columns.Count; col++ )
					{
						if( fieldRow[col].ToString().Trim() == I18NTextData.I18NText_FIELD )
						{
							textFieldColumnIdx = col;
							break;
						}						
					}

					// set text field info
					if( textFieldColumnIdx != -1 )
					{
						for( int row = 1; row < table.Rows.Count; row++ )
						{
							DataRow dataRow = table.Rows[row];
							string data = dataRow[textFieldColumnIdx].ToString().Trim();
							if( string.IsNullOrEmpty(data) == false )
							{
								if( data == I18NTextData.I18NText_SINGLE_KEY )
								{
									if( text_data == null )
										text_data = new I18NTextData( this, category );
									text_data.SetKeyField( table.Rows[row][0].ToString().Trim() );
								}
								else if( data == I18NTextData.I18NText_TEXT )
								{
									if( text_data == null )
										text_data = new I18NTextData( this, category );
									text_data.AddI18NTextdField( table.Rows[row][0].ToString().Trim() );
								}
								else
								{
									if( data.StartsWith( I18NTextData.I18NText_MULTI_KEY ) == true )
									{
										if( text_data == null )
											text_data = new I18NTextData( this, category );

										int idx = int.Parse( data.Replace( I18NTextData.I18NText_MULTI_KEY, "" ) );
										text_data.SetKeyField( table.Rows[row][0].ToString().Trim() );
									}
								}
							}
						}						
					}
				}
			}

			if( text_data != null )
			{
				LogWrite_2_List( eLogType.Normal, text_data.ToString() );
			}

			return text_data;
		}

		//------------------------------------------------------------------------
		void DoExport(string xlsx_filepath, string tbl_Path, int progressBase, int listIdx, bool is_export, bool text_only)
		{
			ProjectConfig.Data p_config_data = ProjectConfig.Instance.CurrentProjectData;

			List<string> _folders = new List<string>();
			string export_path = Path.Combine( tbl_Path, TBLInfoBase.EXPORT_PATH );
			_folders.Add( export_path );
			string forXMLFolder = Path.Combine( tbl_Path, TBLInfoBase.EXPORT_PATH, TBLInfoBase.EXPORT_PATH_XML );
			_folders.Add( forXMLFolder );
			string forClientFolder = Path.Combine( tbl_Path, TBLInfoBase.EXPORT_PATH, TBLInfoBase.EXPORT_PATH_CLIENT );
			_folders.Add( forClientFolder );
			string forBinaryFolder = Path.Combine( tbl_Path, TBLInfoBase.EXPORT_PATH, TBLInfoBase.EXPORT_PATH_BINARY );
			_folders.Add( forBinaryFolder );
			string forBinaryEncryptFolder = Path.Combine( tbl_Path, TBLInfoBase.EXPORT_PATH, TBLInfoBase.EXPORT_PATH_BINARY_ENCRYPT );
			_folders.Add( forBinaryEncryptFolder );
			string forEncryptFolder = Path.Combine( tbl_Path, TBLInfoBase.EXPORT_PATH, TBLInfoBase.EXPORT_PATH_ENCRYPT );
			_folders.Add( forEncryptFolder );

			foreach(string f in _folders)
			{
				if( Directory.Exists( f ) == false )
					Directory.CreateDirectory( f );
			}

			string data_id = Path.GetFileNameWithoutExtension( xlsx_filepath );
			string export_xml_file = Path.Combine( forXMLFolder, data_id + TBLInfoBase.EXTENSION_XML );
			if( Directory.Exists( Path.GetDirectoryName( export_xml_file ) ) == false )
				Directory.CreateDirectory( Path.GetDirectoryName( export_xml_file ) );

			int version = GetTBLPrevVersion( export_xml_file ) + 1;

			LogWrite_2_List( eLogType.Normal, "====================================================" );
			LogWrite_2_List( eLogType.Normal, "Export Begin: version:{0} path:{1}", version, xlsx_filepath );

			lv_filelist.Items[listIdx].BackColor = Color.Blue;
			lv_filelist.Items[listIdx].ForeColor = Color.White;

			string timeString = DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss" );

			List<I18NTextAddData> i18ntext_add_list = null;
			I18NTextData i18n_text_data = null;
			I18NTextData i18n_info_data = null;
			Dictionary<short, string> fields_dic_for_binary = null;

			TBLToolParameters tool_param = GetTBLToolParameter( data_id );
			bool check_field_length_not_matched = false;

			if( tool_param.ExportIgnore && tool_param.I18NTextOnly == false )
			{
				LogWrite_2_List( eLogType.Normal, "Export Ignore : {0}", xlsx_filepath );
				lv_filelist.Items[listIdx].SubItems[2].Text = "Ignore";
				lv_filelist.Items[listIdx].BackColor = Color.Gray;
				lv_filelist.Items[listIdx].ForeColor = Color.Black;
				return;
			}

			DataSet ds = ExportUtil.LoadXLSX( xlsx_filepath, false );
			XmlDocument xml_doc = null;

			if( tool_param.FullSheetExport )
			{
				xml_doc = new XmlDocument();
				XmlNode rootNode = xml_doc.AppendChild( xml_doc.CreateElement( "DataList" ) );
				XMLUtil.AddAttribute( rootNode, "version", version );
				XMLUtil.AddAttribute( rootNode, "data_id", data_id );
				XMLUtil.AddAttribute( rootNode, "created_time", timeString );

				foreach( DataTable table in ds.Tables )
				{
					if( table.TableName == "Define" || string.IsNullOrEmpty( table.TableName ) )
						continue;

					string sheet_name = table.TableName;
					List<string> field_name_list = new List<string>();

					for( int row = 0; row < table.Rows.Count; row++ )
					{
						DataRow dataRow = table.Rows[row];
						if( row == 0 )
						{
							for( int col = 0; col < table.Columns.Count; col++ )
							{
								string col_name = dataRow[col].ToString().Trim();
								if( string.IsNullOrEmpty( col_name ) == false )
									field_name_list.Add( col_name );
							}
						}
						else
						{
							// check empty row
							int _emptyRow = 0;
							int _emptyCount = 0;
							int _columnCount = 0;

							XmlNode row_node = rootNode.AppendChild( rootNode.OwnerDocument.CreateElement( "Row" ) );
							XMLUtil.AddAttribute( row_node, "_Sheet", sheet_name );
							for( int col = 0; col < table.Columns.Count; col++ )
							{
								_emptyRow = row;

								string fieldValueText = XMLUtil.ConvertXmlTextPreDefined( dataRow[col], false, true );
								if( col < field_name_list.Count )
								{
									string field_name = field_name_list[col];
									if( sheet_name == I18NTextData.I18NText_SHEET && ( field_name == KOREAN_LANGUAGE || mTranslate.SupportedLanguages.Exists( s => s.language == field_name ) ) )
									{
										XmlNode text_node = row_node.AppendChild( row_node.OwnerDocument.CreateElement( "I18NText" ) );
										XMLUtil.AddAttribute( text_node, "Language", field_name );
										text_node.InnerText = fieldValueText;
									}
									else
									{
										XMLUtil.AddAttribute( row_node, field_name, fieldValueText );
									}
								}

								_columnCount++;
								if( string.IsNullOrEmpty( fieldValueText ) )
									_emptyCount++;
							}

							if( _emptyCount >= _columnCount )
							{
								LogWrite_2_List( eLogType.Error, "Detected Empty Row :{0}", ( _emptyRow + 1 ) );
								rootNode.RemoveChild( row_node );
							}
						}
					}
				}
			}
			else
			{
				i18n_text_data = CheckI18NTextField( data_id, ds );
				List<string> fieldNameList = new List<string>();

				i18n_info_data = null;
				List<string> infoFieldNameList = null;

				foreach( DataTable table in ds.Tables )
				{
					if( table.TableName == "Data" )
					{
						DataRow dataRow = table.Rows[0];
						for( int col = 0; col < table.Columns.Count; col++ )
						{
							fieldNameList.Add( dataRow[col].ToString() );
						}
					}
					else if( table.TableName == "Info" )
					{
						if( i18n_text_data != null )
							throw new Exception( string.Format( "I18N Text in Define and Info Sheet can not duplicated!" ) );

						infoFieldNameList = new List<string>();
						i18n_info_data = new I18NTextData( this, data_id );
						i18n_info_data.CategorySort = false;

						DataRow dataRow = table.Rows[0];
						for( int col = 0; col < table.Columns.Count; col++ )
						{
							infoFieldNameList.Add( dataRow[col].ToString().Trim() );

							string field_name = dataRow[col].ToString().Trim();
							if( field_name.StartsWith( "%" ) == false )
							{
								if( field_name == "IDN" )
									i18n_info_data.SetKeyField( field_name );
								else if( string.IsNullOrEmpty( field_name ) == false )
									i18n_info_data.AddI18NTextdField( field_name );
							}
						}

						if( infoFieldNameList.Exists( f => f == "IDN" ) == false )
							throw new Exception( string.Format( "Info sheet : need IDN(key) field" ) );
					}
					else if( table.TableName.StartsWith( I18NTextData.I18NText_ADD_FIELD ) )
					{
						I18NTextAddData add_data = new I18NTextAddData();
						add_data.m_SheetName = table.TableName;
						add_data.m_data = new I18NTextData( this, data_id );
						add_data.m_data.CategorySort = false;
						add_data.m_fieldNameList = new List<string>();

						DataRow dataRow = table.Rows[0];
						for( int col = 0; col < table.Columns.Count; col++ )
						{
							add_data.m_fieldNameList.Add( dataRow[col].ToString() );

							string field_name = dataRow[col].ToString().Trim();
							if( field_name.StartsWith( "%" ) == false )
							{
								if( field_name == "ID" )
									add_data.m_data.SetKeyField( field_name );
								else if( string.IsNullOrEmpty( field_name ) == false )
									add_data.m_data.AddI18NTextdField( field_name );
							}
						}

						if( add_data.m_fieldNameList.Exists( f => f == "ID" ) == false )
							throw new Exception( string.Format( "TextAdd sheet : need ID(key) field" ) );

						if( i18ntext_add_list == null )
							i18ntext_add_list = new List<I18NTextAddData>();
						i18ntext_add_list.Add( add_data );
					}
				}

				if( CheckTBLVerify( data_id, fieldNameList, ref check_field_length_not_matched ) == false )
				{
					lv_filelist.Items[listIdx].SubItems[2].Text = "Field Not Matched!";

					lv_filelist.Items[listIdx].BackColor = Color.Red;
					lv_filelist.Items[listIdx].ForeColor = Color.White;
				}

				if( tool_param != null && i18n_text_data != null )
					i18n_text_data.CategorySort = tool_param.I18NTextCategorySort;

				xml_doc = new XmlDocument();
				XmlNode rootNode = xml_doc.AppendChild( xml_doc.CreateElement( "DataList" ) );
				XMLUtil.AddAttribute( rootNode, "version", version );
				XMLUtil.AddAttribute( rootNode, "data_id", data_id );
				XMLUtil.AddAttribute( rootNode, "created_time", timeString );

				List<int> List_IDN = new List<int>();
				List<string> List_ID = new List<string>();
				fields_dic_for_binary = new Dictionary<short, string>();

				foreach( DataTable table in ds.Tables )
				{
					if( table.TableName == "Info" )
					{
						for( int row = 1; row < table.Rows.Count; row++ )
						{
							DataRow dataRow = table.Rows[row];
							i18n_info_data.MakeI18NTextText( infoFieldNameList, dataRow );
						}
					}
					else if( table.TableName.StartsWith( I18NTextData.I18NText_ADD_FIELD ) )
					{
						if( i18ntext_add_list != null )
						{
							I18NTextAddData add_data = i18ntext_add_list.Find( l => l.m_SheetName == table.TableName );
							if( add_data != null )
							{
								for( int row = 1; row < table.Rows.Count; row++ )
								{
									DataRow dataRow = table.Rows[row];
									add_data.m_data.MakeI18NTextText( add_data.m_fieldNameList, dataRow );
								}
							}
						}
					}
					else if( table.TableName == "Data" )
					{
						int progress = progressBase;
						for( int row = 1; row < table.Rows.Count; row++ )
						{
							DataRow dataRow = table.Rows[row];

							if( table.Columns.Count != fieldNameList.Count )
								throw new Exception( string.Format( "field count not matched({0} != {1})", dataRow.ItemArray.Length, fieldNameList.Count ) );

							if( i18n_text_data != null )
								i18n_text_data.MakeI18NTextText( fieldNameList, dataRow );

							// check empty row
							int _emptyRow = 0;
							int _emptyCount = 0;
							int _columnCount = 0;

							for( int col = 0; col < table.Columns.Count; col++ )
							{
								_emptyRow = row;
								if( fieldNameList[col].StartsWith( "%" ) == false )
								{
									string fieldName = fieldNameList[col];
									if( string.IsNullOrEmpty( fieldName ) )
										continue;

									if( i18n_text_data == null || i18n_text_data.IsI18NTextField( fieldName ) == false )
									{
										_columnCount++;
										if( string.IsNullOrEmpty( XMLUtil.ConvertXmlTextPreDefined( dataRow[col], false, true ) ) )
										{
											_emptyCount++;
										}
									}
								}
							}
							if( _emptyCount >= _columnCount )
							{
								LogWrite_2_List( eLogType.Error, "Detected Empty Row :{0}", ( _emptyRow + 1 ) );	// xlsx begin row 1
								continue;
							}

							XmlNode rowNode = rootNode.AppendChild( rootNode.OwnerDocument.CreateElement( "Row" ) );
							for( int col = 0; col < table.Columns.Count; col++ )
							{
								if( fieldNameList[col].StartsWith( "%" ) == false )
								{
									string fieldName = fieldNameList[col];
									if( string.IsNullOrEmpty( fieldName ) )
										continue;

									if( i18n_text_data == null || i18n_text_data.IsI18NTextField( fieldName ) == false )
									{
										string fieldValueText = XMLUtil.ConvertXmlTextPreDefined( dataRow[col], false, true );
										if( tool_param.CheckSameIds )
										{
											// check idn / id
											if( fieldName.ToUpper() == "IDN" )
											{
												int idn;
												if( Int32.TryParse( fieldValueText, out idn ) == true )
												{
													if( List_IDN.Contains( idn ) )
													{
														throw new Exception( string.Format( "Detected Same IDN Row :{0}", row ) );
													}
													else
													{
														List_IDN.Add( idn );
													}
												}
											}

											if( fieldName.ToUpper() == "ID" )
											{
												if( List_ID.Contains( fieldValueText ) )
												{
													throw new Exception( string.Format( "Detected Same ID Row :{0}", row ) );
												}
												else
												{
													List_ID.Add( fieldValueText );
												}
											}
										}

										short dic_col_key = (short)( col + 1 );
										if( fields_dic_for_binary.ContainsKey( dic_col_key ) == false )
											fields_dic_for_binary.Add( dic_col_key, fieldNameList[col] );

										XMLUtil.AddAttribute( rowNode, fieldNameList[col], fieldValueText );
									}
								}
							}

							progress = (int)( ( (float)row / ( table.Rows.Count - 1 ) ) * 100f );
							progress_export.Value = progressBase + progress;
							progress_export.Update();

							lv_filelist.Items[listIdx].SubItems[2].Text = string.Format( "{0}%", progress );
							lv_filelist.Update();
						}

						progress_export.Value = progressBase + 100;
						progress_export.Update();
						lv_filelist.Items[listIdx].SubItems[2].Text = "100%";
						lv_filelist.Update();
					}
				}
			}

			if( tool_param.ExportIgnore == false )
			{
				if( is_export && text_only == false )
				{
					XMLUtil.SaveXmlDocToFile( export_xml_file, xml_doc );

					if( tool_param.ValidCheckHandler != null )
						tool_param.ValidCheckHandler( export_xml_file );
				}

				if( is_export && text_only == false )
				{
					if( tool_param.UseClient )
					{
						string exportForClient = Path.Combine( forClientFolder, data_id + TBLInfoBase.EXTENSION_XML );
						XMLUtil.SaveXmlDocToFile( exportForClient, xml_doc );
					}

 					string exportForEncrypt = Path.Combine( forEncryptFolder, data_id + TBLInfoBase.EXTENSION_XML_ENCRYPT + ".bytes" );
					XMLUtil.SaveXmlDocToEncryptFile( exportForEncrypt, xml_doc, p_config_data.EncryptKey8 );

					if( fields_dic_for_binary != null )
					{
						string exportForBinary = Path.Combine( forBinaryFolder, data_id + TBLInfoBase.EXTENSION_XML_BINARY + ".bytes" );
						using( FileStream fs = new FileStream( exportForBinary, FileMode.Create ) )
						{
							using( BinaryWriter bw = new BinaryWriter( fs ) )
							{
								XmlBinary.WriteXml( bw, xml_doc, fields_dic_for_binary );
								bw.Close();
								fs.Close();
							}
						}

						string exportForBinaryEncrypt = Path.Combine( forBinaryEncryptFolder, data_id + TBLInfoBase.EXTENSION_XML_BINARY_ENCRYPT + ".bytes" );
						using( FileStream fs = new FileStream( exportForBinaryEncrypt, FileMode.Create ) )
						{
							DESCryptoServiceProvider desProvider = new DESCryptoServiceProvider();
							desProvider.Key = ASCIIEncoding.ASCII.GetBytes( p_config_data.EncryptKey8 );
							desProvider.IV = ASCIIEncoding.ASCII.GetBytes( p_config_data.EncryptKey8 );

							using( CryptoStream cs = new CryptoStream( fs, desProvider.CreateEncryptor(), CryptoStreamMode.Write ) )
							{
								using( BinaryWriter bw = new BinaryWriter( cs ) )
								{
									XmlBinary.WriteXml( bw, xml_doc, fields_dic_for_binary );
									bw.Close();
									cs.Close();
									fs.Close();
								}
							}
						}
					}
				}
			}

			ds.Dispose();

			if( is_export )
			{
				I18NTextData text_data = null;
				if( i18n_info_data != null )
					text_data = i18n_info_data;
				else if( i18n_text_data != null )
					text_data = i18n_text_data;

				if( i18ntext_add_list != null )
				{
					if( text_data == null )
						text_data = new I18NTextData( this, data_id );

					foreach( I18NTextAddData add_data in i18ntext_add_list )
					{
						text_data.I18NTextAdd( add_data );
					}
				}

				if( text_data != null )
				{
					if( tool_param != null )
						text_data.DataSort = tool_param.I18NTextDataSort;

					text_data.Export( export_path, data_id, timeString, version, p_config_data.EncryptKey8, tool_param.IgnoreTranslateCategorys );
				}
			}

			if( is_export && text_only == false )
			{
				lv_filelist.Items[listIdx].SubItems[3].Text = version.ToString();
				LogWrite_2_List( eLogType.Normal, "Export : {0}", export_xml_file );
			}
			else if( text_only )
				LogWrite_2_List( eLogType.Normal, "TextOnly : {0}", export_xml_file );
			else
				LogWrite_2_List( eLogType.Normal, "Verify : {0}", export_xml_file );

			lv_filelist.Items[listIdx].BackColor = Color.White;
			lv_filelist.Items[listIdx].ForeColor = Color.Black;
		}

		//------------------------------------------------------------------------	
		int GetTBLPrevVersion(string prev_xml_filepath)
		{
			int ret_version = 0;
			if( File.Exists( prev_xml_filepath ) )
			{
				XmlDocument doc = new XmlDocument();
				try
				{
					doc.Load( prev_xml_filepath );
					XmlNode rootNode = doc.SelectSingleNode( "DataList" );
					if( rootNode != null )
					{
						ret_version = XMLUtil.ParseAttribute<int>( rootNode, "version", 0 );
					}
				}
				catch (System.Exception ex)
				{
					LogWrite_2_List( eLogType.Error, ex.ToString() );
				}
			}

			return ret_version;
		}

		//------------------------------------------------------------------------	
		int GetI18NTextCurrentVersion( string path )
		{
			int ret_version = -1;
			if( File.Exists( path ) )
			{
				XmlDocument doc = new XmlDocument();
				try
				{
					doc.Load( path );
					XmlNode root_node = doc.SelectSingleNode( "DataList" );
					if( root_node != null )
					{
						ret_version = XMLUtil.ParseAttribute<int>( root_node, "tbl_version", 0 );
					}
				}
				catch( Exception ex )
				{
					LogWrite_2_List( eLogType.Error, ex.ToString() );
				}
			}

			return ret_version;
		}

		//------------------------------------------------------------------------
		void OnTranslateExport_XML2Excel( bool is_merge, int file_list_idx = -1 )
		{
			if( mFilePathList.Count <= 0 )
				return;

			TranslateManager.Instance.Clear();

			string export_path = Path.Combine( mProjectGlobaltypeProperty.TBLPath, TBLInfoBase.EXPORT_PATH, I18NTextConst.DEFAULT_PATH_ROOT, I18NTextExportConst.TRANSLATION_PATH );
			string prev_folder = "";
			string other_prev_folder = "";
			string kor_eng_xmlmerged_prev_check_folder = ""; // 국문/영문 FULLSEP 변경사항 체크

			string i18ntext_def_lan_path = Path.Combine( mProjectGlobaltypeProperty.TBLPath, TBLInfoBase.EXPORT_PATH, I18NTextConst.DEFAULT_PATH_ROOT, I18NTextConst.DEFAULT_PATH_LANGUAGES, KOREAN_LANGUAGE );
			string xml2excel_path = $"{I18NTextConst.DEFAULT_PATH_ROOT}/{I18NTextExportConst.TRANSLATION_PATH}/export_path";
			string import_path = $"{xml2excel_path}/export_path/{I18NTextExportConst.TRANSLATION_IMPORT_PATH_PREFIX}_날짜";
			string merged_path = $"{xml2excel_path}/export_path/{I18NTextExportConst.TRANSLATION_MERGED_PATH_PREFIX}_이전날짜";

			if( is_merge )
			{
				folderBrowserDialog.Description = $"이전 번역의 import 된 폴더({import_path}) 선택";
				folderBrowserDialog.SelectedPath = export_path;
				folderBrowserDialog.ShowDialog();
				prev_folder = folderBrowserDialog.SelectedPath;
				if( prev_folder == export_path )
				{
					LogWrite_2_List( eLogType.Normal, "Select Other Previous Folder!" );
					return;
				}

				//if( cb_localize_merge_with_otherglobal.Checked )
				//{
				//	folderBrowserDialog.Description = "Select Other Translate XML Data Folder!";
				//	folderBrowserDialog.SelectedPath = LastOpenLocal_XML2ExelFolder;
				//	folderBrowserDialog.ShowDialog();
				//	string other_folder = folderBrowserDialog.SelectedPath;
				//	if( string.IsNullOrEmpty( other_folder ) == false && other_folder != LastOpenLocal_XML2ExelFolder )
				//	{
				//		other_prev_folder = other_folder;
				//	}
				//}

				//if( cb_xmlmerge_prev_changed_check.Checked )
				//{
				//	folderBrowserDialog.Description = "비교할 이전 xmlmerged 폴더 위치";
				//	folderBrowserDialog.SelectedPath = LastOpenLocal_XML2ExelFolder;
				//	folderBrowserDialog.ShowDialog();
				//	kor_eng_xmlmerged_prev_check_folder = folderBrowserDialog.SelectedPath;
				//}
			}

			Cursor.Current = Cursors.WaitCursor;

			ButtonsEnable( false );
			mExportProgress = true;

			string MAKE_DATE = System.DateTime.Now.ToString( "yyyyMMdd_HHmmss" );
			string exportFolder = Path.Combine( export_path, MAKE_DATE );
			if( is_merge )
			{
				exportFolder += "_MERGED_";
				exportFolder += Path.GetFileName( prev_folder );
			}
			else
			{
				exportFolder += "_EXPORT";
			}

			if( Directory.Exists( exportFolder ) == false )
			{
				Directory.CreateDirectory( exportFolder );
			}

			if( is_merge )
			{
				// merge info
				try
				{
					string logfilename = Path.Combine( exportFolder, MAKE_DATE + "_MERGE.log" );
					if( File.Exists( logfilename ) )
						File.Delete( logfilename );

					FileStream fs = new FileStream( logfilename, FileMode.CreateNew, FileAccess.Write );
					StreamWriter writer = new StreamWriter( fs, System.Text.Encoding.UTF8 );

					writer.WriteLine( "===== MERGED INFO =====" );
					writer.WriteLine( "SOURCE : " + i18ntext_def_lan_path );
					writer.WriteLine( "MERGED FROM : " + prev_folder );
					writer.WriteLine( "MERGED TO : " + exportFolder );
					writer.WriteLine( "MERGED OTHER : " + other_prev_folder );

					writer.Flush();
					writer.Close();
				}
				catch( System.Exception ex )
				{
					LogWrite_2_List( eLogType.Error, ex.ToString() );
				}
			}

			mTranslate.SetLog( exportFolder, "XLSXEXPORT" );

			if( is_merge )
			{
				LogWrite_2_List( eLogType.Normal, "===== MERGED INFO =====" );
				LogWrite_2_List( eLogType.Normal, "SOURCE : " + i18ntext_def_lan_path );
				LogWrite_2_List( eLogType.Normal, "MERGED FROM : " + prev_folder );
				LogWrite_2_List( eLogType.Normal, "MERGED TO : " + exportFolder );
				LogWrite_2_List( eLogType.Normal, "MERGED OTHER : " + other_prev_folder );
			}
			else
			{
				LogWrite_2_List( eLogType.Normal, "===== EXPORT INFO =====" );
				LogWrite_2_List( eLogType.Normal, "SOURCE : " + i18ntext_def_lan_path );
			}

			try
			{
				if( file_list_idx == -1 )
				{
					for( int i = 0; i < lv_filelist.Items.Count; i++ )
					{
						lv_filelist.Items[i].SubItems[2].Text = "";
					}
					lv_filelist.Update();

					progress_export.Maximum = mFilePathList.Count * 100;
					progress_export.Value = 0;
					progress_export.Update();
					for( int i = 0; i < mFilePathList.Count; i++ )
					{
						mLastFileName = mFilePathList[i].filePath;
						_DoTranslateExport_XML2Excel_Process( mLastFileName, prev_folder, other_prev_folder, exportFolder, i * 100, i, is_merge, kor_eng_xmlmerged_prev_check_folder );
					}
				}
				else
				{
					lv_filelist.Items[file_list_idx].SubItems[2].Text = "";
					lv_filelist.Update();
					progress_export.Maximum = 100;
					progress_export.Value = 0;
					progress_export.Update();
					mLastFileName = mFilePathList[file_list_idx].filePath;
					_DoTranslateExport_XML2Excel_Process( mFilePathList[file_list_idx].filePath, prev_folder, other_prev_folder, exportFolder, 0, file_list_idx, is_merge, kor_eng_xmlmerged_prev_check_folder );
				}
			}
			catch( System.Exception ex )
			{
				LogWrite_2_List( eLogType.Error, string.Format( "({0}){1}", mLastFileName, ex.ToString() ) );
				MessageBox.Show( ex.ToString() );
			}
			finally
			{
				ButtonsEnable( true );
				mExportProgress = false;
			}

			Cursor.Current = Cursors.Default;

			mTranslate.UnSetLog();
		}

		//------------------------------------------------------------------------
		void OnTranslateExport_Excel2XML( int listidx = -1 )
		{
			if( mFilePathList.Count <= 0 )
				return;

			ButtonsEnable( false );
			mExportProgress = true;

			mTranslate.SetLog( LastTranslateExcel2XmlPath, "XMLEXPORT" );

			Cursor.Current = Cursors.WaitCursor;

			try
			{
				if( listidx == -1 )
				{
					for( int i = 0; i < lv_filelist.Items.Count; i++ )
					{
						lv_filelist.Items[i].SubItems[2].Text = "";
					}
					lv_filelist.Update();

					progress_export.Maximum = mFilePathList.Count * 100;
					progress_export.Value = 0;
					progress_export.Update();
					for( int i = 0; i < mFilePathList.Count; i++ )
					{
						mLastFileName = mFilePathList[i].filePath;
						DoTranslateExport_Excel2XML( mFilePathList[i].filePath, i * 100, i );
					}
				}
				else
				{
					lv_filelist.Items[listidx].SubItems[2].Text = "";
					lv_filelist.Update();
					progress_export.Maximum = 100;
					progress_export.Value = 0;
					progress_export.Update();
					mLastFileName = mFilePathList[listidx].filePath;
					DoTranslateExport_Excel2XML( mFilePathList[listidx].filePath, 0, listidx );
				}
			}
			catch( System.Exception ex )
			{
				LogWrite_2_List( eLogType.Error, string.Format( "({0}){1}", mLastFileName, ex.ToString() ) );
				MessageBox.Show( ex.ToString() );
			}
			finally
			{
				ButtonsEnable( true );
				mExportProgress = false;
			}

			mTranslate.UnSetLog();
			Cursor.Current = Cursors.Default;
		}


		//------------------------------------------------------------------------
		void _DoTranslateExport_XML2Excel_Process( string filepath, string prev_folder, string other_prev_folder, string exportPath, int progressBase, int listIdx, bool is_merge, string koreng_mlxmerged_check_folder )
		{
			lv_filelist.Items[listIdx].BackColor = Color.Blue;
			lv_filelist.Items[listIdx].ForeColor = Color.White;

			string[] support_languages = mTranslate.SupportedLanguages.Where( s => IsSupportedLanguageChecked( s.language ) ).Select( l => l.language ).ToArray();
			TranslateData translate_data = TranslateManager.Instance.ExecuteMerge( is_merge, filepath, prev_folder, other_prev_folder, support_languages );
			if( translate_data != null )
			{
				//if( cb_xmlmerge_prev_changed_check.Checked && string.IsNullOrEmpty( koreng_mlxmerged_check_folder ) == false )
				//{
				//	DoXmlmergedPrevChangeCheck( exportPath, filepath, translate_data, koreng_mlxmerged_check_folder );
				//}

				_DoTranslateExport_XML2Excel_FULLSEP( filepath, exportPath, translate_data, listIdx, progressBase, null );
				foreach( TBLExportTranslate.LanguageCultureData language in mTranslate.SupportedLanguages )
				{
					if( IsSupportedLanguageChecked( language.language ) == false )
						continue;

					_DoTranslateExport_XML2Excel_FULLSEP( filepath, exportPath, translate_data, listIdx, progressBase, language.language );
				}

				if( is_merge )
				{
					// korean first = language = null;
					_DoTranslateExport_XML2Excel_ChangedOnly( filepath, exportPath, translate_data, null, true );
					foreach( TBLExportTranslate.LanguageCultureData language in mTranslate.SupportedLanguages )
					{
						if( IsSupportedLanguageChecked( language.language ) == false )
							continue;

						bool exported = false;
						//if( cb_export_prev_korean_diff.Checked && mPrevKoreanDiffPaths.ContainsKey( language.language ) )
						//	exported = DoExportLocalizeMergeXML2ExcelDiffOnly( filepath, exportPath, translate_data, language.language );

						if( exported == false )
							_DoTranslateExport_XML2Excel_ChangedOnly( filepath, exportPath, translate_data, language.language, true );

						// if have equal text
						// deprecated : changed_equal_only (problem missing icon name and equal text's sort..)
						// instead use ExportExcel2XML
						//string exportFileName = Path.Combine( exportPath, string.Format( "{0}.xlsx", Path.GetFileNameWithoutExtension( filepath ) ) );
						//translate_data.DoExportXML( exportFileName, new List<LanguageCultureData>() { language }, true );
					}

					string exportFileName = Path.Combine( exportPath, string.Format( "{0}.xlsx", Path.GetFileNameWithoutExtension( filepath ) ) );
					// xmlmerged export~~
					DoTranslateExport_Excel2XML( exportFileName, 0, 0, true );

					translate_data.CheckDuplicatedText( true );

					// 중복제거된 데이터로 한번더 export
					_DoTranslateExport_XML2Excel_ChangedOnly( filepath, exportPath, translate_data, null, false );
					foreach( TBLExportTranslate.LanguageCultureData language in mTranslate.SupportedLanguages )
					{
						if( IsSupportedLanguageChecked( language.language ) == false )
							continue;

						bool exported = false;
						//if( cb_export_prev_korean_diff.Checked && mPrevKoreanDiffPaths.ContainsKey( language.language ) )
						//	exported = DoExportLocalizeMergeXML2ExcelDiffOnly( filepath, exportPath, translate_data, language.language );

						if( exported == false )
							_DoTranslateExport_XML2Excel_ChangedOnly( filepath, exportPath, translate_data, language.language, false );
					}
				}
			}

			lv_filelist.Items[listIdx].BackColor = Color.White;
			lv_filelist.Items[listIdx].ForeColor = Color.Black;
		}

		//------------------------------------------------------------------------	
		void _DoTranslateExport_XML2Excel_FULLSEP( string filepath, string exportPath, TranslateData translate_data, int listIdx, int progressBase, string _language )
		{
			string exportFileName = Path.Combine( exportPath, string.Format( "{0}.xlsx", Path.GetFileNameWithoutExtension( filepath ) ) );
			if( _language != null )
				exportFileName = Path.Combine( exportPath, string.Format( "{0}{1}_{2}.xlsx", Path.GetFileNameWithoutExtension( filepath ), FULL_SEP, _language ) );

			LogWrite_2_List( eLogType.Normal, "====================================================" );
			LogWrite_2_List( eLogType.Normal, "ExportExcel Begin: {0}->{1}", filepath, exportFileName );

			if( File.Exists( exportFileName ) )
			{
				try
				{
					File.Delete( exportFileName );
				}
				catch( Exception ex )
				{
					LogWrite_2_List( eLogType.Error, ex.ToString() );
					lv_filelist.Items[listIdx].SubItems[2].Text = "Busy";
					lv_filelist.Items[listIdx].BackColor = Color.Red;
					lv_filelist.Items[listIdx].ForeColor = Color.White;
					return;
				}
			}

			TranslateData prev_korean_diff_data = null;
			//if( _language != null && cb_export_prev_korean_diff.Checked && mPrevKoreanDiffPaths.ContainsKey( _language ) )
			//{
			//	// filepath = GameText_Localize-NE_Korean.xml
			//	string prev_korean_diff_file_name = Path.GetFileNameWithoutExtension( filepath ).Replace( GlobalTypeSuffix, "" ).Replace( "_Korean", "" );
			//	foreach( eGlobalType g_type in System.Enum.GetValues( typeof( eGlobalType ) ) )
			//	{
			//		string selected_korean_file = Path.Combine( mPrevKoreanDiffPaths[_language], string.Format( prev_korean_diff_file_name + "-" + g_type.ToString() + "_Korean.xml" ) );
			//		if( File.Exists( selected_korean_file ) )
			//		{
			//			prev_korean_diff_data = new TranslateData( this );
			//			prev_korean_diff_data.LoadFromXml( KOREAN_LANGUAGE, selected_korean_file );
			//			break;
			//		}
			//	}
			//}

			string lan_export_path = "";
			StreamWriter tagissue_writer = null;
			StreamWriter changedonly_writer = null;
			if( _language != null )
			{
				lan_export_path = Path.Combine( exportPath, DateTime.Now.ToString( "yyyyMMdd" ) + "_" + _language );
				if( Directory.Exists( lan_export_path ) == false )
					Directory.CreateDirectory( lan_export_path );
			}

			int progress = progressBase;
			// make excel
			using( ExcelPackage excelPackage = new ExcelPackage() )
			{
				// step 0 : category sheet create

				Dictionary<string, string> category_sheet_dic = MakeExcelCategorySheet( excelPackage, translate_data );

				int count = 0;
				foreach( TranslateData.CategoryData c_data in translate_data.CategoryList )
				{
					if( _language != null && ( c_data.m_StringList.Exists( s => s.m_Language == KOREAN_LANGUAGE && s.HasTranslateFlag( I18NTextConst.eTranslateFlag.IgnoreTranslate ) == false ) == false ) )
						continue;

					bool is_english_include = false;
					if( _language != null && _language != "English" /*&& cb_export_eng_include.Checked*/ )
						is_english_include = true;

					DataTable dt = new DataTable( c_data.m_Category );
					dt.Columns.Add( "id" );
					dt.Columns.Add( KOREAN_LANGUAGE );
					dt.Columns.Add( $"{KOREAN_LANGUAGE}_State" );
					if( _language == null )
					{
						foreach( TBLExportTranslate.LanguageCultureData language in mTranslate.SupportedLanguages )
						{
							dt.Columns.Add( language.language );
							dt.Columns.Add( language.language + "_State" );
							dt.Columns.Add( language.language + "_TagIssue" );
						}
					}
					else
					{
						if( is_english_include )
							dt.Columns.Add( "_ENG_" );

						dt.Columns.Add( _language );
						dt.Columns.Add( _language + "_State" );
						dt.Columns.Add( _language + "_TagIssue" );
					}

					if( prev_korean_diff_data != null )
					{
						//dt.Columns.Add( PREV_KOREAN_DIFF_COLUMN );
					}

					List<ExcelCellColorData> excelColorList = new List<ExcelCellColorData>();
					List<ExcelCellColorData> excelPrevKoreanDiffColorList = new List<ExcelCellColorData>();
					Dictionary<string, RowCellData> rowDIc = new Dictionary<string, RowCellData>();

					int last_col = dt.Columns.Count;
					int cell_row = 1;

					foreach( TranslateData.StringData s_data in c_data.m_StringList )
					{
						TranslateData.StringData korean_data = null;
						TranslateData.StringData english_data = null;
						if( _language != null )
						{
							if( s_data.m_Language != _language && s_data.m_Language != KOREAN_LANGUAGE )
								continue;

							korean_data = c_data.m_StringList.Find( s => s.m_Language == KOREAN_LANGUAGE && s.m_ID == s_data.m_ID );
							if( korean_data != null && korean_data.HasTranslateFlag( I18NTextConst.eTranslateFlag.IgnoreTranslate ) )
								continue;

							if( is_english_include )
								english_data = c_data.m_StringList.Find( a => a.m_Language == "English" && a.m_ID == s_data.m_ID );
						}

						string stateText = s_data.m_State != TranslateData.eState.None ? s_data.m_State.ToString() : "";
						if( _language == null && s_data.HasTranslateFlag( I18NTextConst.eTranslateFlag.IgnoreTranslate ) )
							stateText = TranslateData.eState.IGNORETRANS.ToString();

						string tag_issue_text = "";
						if( s_data.m_Language != KOREAN_LANGUAGE )
						{
							//TranslateData.StringData kor_data = c_data.m_StringList.Find( s => s.m_Language == KOREAN_LANGUAGE && s.m_ID == s_data.m_ID );
							//if( kor_data != null && kor_data.m_State != TranslateData.eState.ADDED && kor_data.m_State != TranslateData.eState.CHANGED
							//	&& s_data.m_State != TranslateData.eState.EMPTY && s_data.HasTranslateFlag( I18NTextConst.eTranslateFlag.IgnoreTranslate ) == false )
							//{
							//	string k_text = kor_data.MakeExcelText();
							//	string s_text = s_data.MakeExcelText();
							//	string fixed_text = "";
							//	string reverse_error_msg = "";
							//	if( ExportUtil.CheckTAG( Path.GetFileNameWithoutExtension( exportFileName ), kor_data.m_TranslateFlags, k_text, s_text, mCheckTagPolicyList, mCheckTagPolicyFlags, ref tag_issue_text, ref reverse_error_msg, ref fixed_text ) == false )
							//	{
							//		StringBuilder sb = new StringBuilder();
							//		sb.AppendLine( string.Format( "- TagIssue:[{0}][{1}][{2}] {3}", c_data.m_Category, s_data.m_ID, s_data.m_Language, tag_issue_text ) );
							//		if( string.IsNullOrEmpty( reverse_error_msg ) == false )
							//		{
							//			sb.AppendLine( "<==== ReverseTagIssue : " + reverse_error_msg );
							//		}
							//		sb.AppendLine( k_text );
							//		sb.AppendLine( "====>" );
							//		sb.AppendLine( s_text );
							//		if( string.IsNullOrEmpty( fixed_text ) == false )
							//		{
							//			sb.AppendLine( "- Fixed Suggestion" );
							//			sb.AppendLine( fixed_text );
							//		}

							//		LogWrite_2_List( eLogType.Error, sb.ToString() );
							//		LogWrite2( "\n-------------------------------------------------------------" );
							//		LogWrite2( sb.ToString() );

							//		if( tagissue_writer == null )
							//		{
							//			FileStream fs = new FileStream( Path.Combine( lan_export_path, "_TAG_ISSUE.log" ), FileMode.Append, FileAccess.Write );
							//			tagissue_writer = new StreamWriter( fs, System.Text.Encoding.UTF8 );

							//			tagissue_writer.WriteLine( "" );
							//			tagissue_writer.WriteLine( "*************************************************************" );
							//			tagissue_writer.WriteLine( Path.GetFileName( exportFileName ) );
							//			tagissue_writer.WriteLine( "*************************************************************" );
							//		}

							//		tagissue_writer.WriteLine( "\n-------------------------------------------------------------" );
							//		tagissue_writer.WriteLine( sb.ToString() );
							//		tagissue_writer.Flush();
							//	}
							//}
						}

						DataRow row;
						RowCellData row_cell_data;
						if( rowDIc.TryGetValue( s_data.m_ID, out row_cell_data ) == false )
						{
							row = dt.NewRow();
							dt.Rows.Add( row );
							cell_row++;

							row_cell_data = new RowCellData();
							row_cell_data.m_Row = row;
							row_cell_data.m_CellRowIndex = cell_row;
							rowDIc.Add( s_data.m_ID, row_cell_data );
						}
						else
						{
							row = row_cell_data.m_Row;
							cell_row = row_cell_data.m_CellRowIndex;
						}

						row["id"] = s_data.m_ID;
						row[s_data.m_Language + "_State"] = stateText;

						if( s_data.m_Language != KOREAN_LANGUAGE )
						{
							row[s_data.m_Language + "_TagIssue"] = tag_issue_text;
						}

						if( is_english_include )
						{
							if( english_data != null )
								row["_ENG_"] = english_data.MakeExcelText();
							else
								row["_ENG_"] = "";
						}

						if( s_data.m_State != TranslateData.eState.None || string.IsNullOrEmpty( tag_issue_text ) == false )
						{
							DataColumn dc = dt.Columns[s_data.m_Language + "_State"];
							int colIdx = dc.Ordinal + 1;

							if( excelColorList.Exists( c => c.m_Row == cell_row && c.m_Language == KOREAN_LANGUAGE ) == false )
							{
								ExcelCellColorData colorData = new ExcelCellColorData();
								colorData.m_Language = s_data.m_Language;
								colorData.m_Row = cell_row;
								colorData.m_Col = colIdx;
								switch( s_data.m_State )
								{
									case TranslateData.eState.ADDED:
									case TranslateData.eState.ADDED_EQUAL:
										colorData.m_Color = Color.Gold;
										break;

									case TranslateData.eState.CHANGED:
									case TranslateData.eState.KCHANGED:
									case TranslateData.eState.ECHANGED:
									case TranslateData.eState.CHANGED_EQUAL:
										colorData.m_Color = Color.LightBlue;

										if( s_data.m_State == TranslateData.eState.ECHANGED || s_data.m_State == TranslateData.eState.KCHANGED )
											colorData.m_Color = Color.LightGreen;

										if( s_data.m_State != TranslateData.eState.CHANGED_EQUAL )
										{
											if( changedonly_writer == null )
											{
												FileStream fs = new FileStream( Path.Combine( lan_export_path, "_CHANGEDONLY.log" ), FileMode.Append, FileAccess.Write );
												changedonly_writer = new StreamWriter( fs, System.Text.Encoding.UTF8 );

												changedonly_writer.WriteLine( "" );
												changedonly_writer.WriteLine( "*************************************************************" );
												changedonly_writer.WriteLine( Path.GetFileName( exportFileName ) );
												changedonly_writer.WriteLine( "*************************************************************" );
											}

											StringBuilder sb = new StringBuilder();
											sb.AppendLine( string.Format( "- Changed:[{0}][{1}][{2}]", c_data.m_Category, s_data.m_ID, s_data.m_Language ) );
											sb.AppendLine( s_data.m_ChangedPrevText );
											sb.AppendLine( "PREV <====> NEW" );
											sb.AppendLine( s_data.MakeXMLText() );

											changedonly_writer.WriteLine( "\n-------------------------------------------------------------" );
											changedonly_writer.WriteLine( sb.ToString() );
											changedonly_writer.Flush();
										}
										break;

									case TranslateData.eState.REMOVED:
										colorData.m_Color = Color.Tomato;
										break;

									case TranslateData.eState.CONFILICT:
										colorData.m_Color = Color.Red;
										break;

									case TranslateData.eState.EMPTY:
									case TranslateData.eState.EMPTY_EQUAL:
										colorData.m_Color = Color.LightGray;
										break;

									default:
										colorData.m_Color = Color.Black;
										break;
								}

								if( string.IsNullOrEmpty( tag_issue_text ) == false )
									colorData.m_Color = Color.Orange;

								excelColorList.Add( colorData );
							}
						}

						row[s_data.m_Language] = s_data.MakeExcelText();

						if( prev_korean_diff_data != null )
						{
							//TranslateData.StringData prev_korean_data = prev_korean_diff_data.FindStringData( c_data.m_Category, s_data.m_ID, KOREAN_LANGUAGE );
							//string prev_korean_text = "";
							//if( prev_korean_data != null )
							//	prev_korean_text = prev_korean_data.MakeExcelText();

							//row[PREV_KOREAN_DIFF_COLUMN] = prev_korean_text;

							//if( korean_data != null && prev_korean_data != null && korean_data.IsTextDifferent( prev_korean_data.m_TextList ) )
							//{
							//	if( excelPrevKoreanDiffColorList.Exists( c => c.m_Row == cell_row && c.m_Language == KOREAN_LANGUAGE ) == false )
							//	{
							//		DataColumn dc = dt.Columns[PREV_KOREAN_DIFF_COLUMN];
							//		int colIdx = dc.Ordinal + 1;

							//		ExcelCellColorData colorData = new ExcelCellColorData();
							//		colorData.m_Language = KOREAN_LANGUAGE;
							//		colorData.m_Row = cell_row;
							//		colorData.m_Col = colIdx;
							//		colorData.m_Color = Color.LightBlue;
							//		excelPrevKoreanDiffColorList.Add( colorData );
							//	}
							//}
						}
					}

					string sheet_name = c_data.m_Category;
					if( category_sheet_dic.ContainsKey( c_data.m_Category ) )
						sheet_name = category_sheet_dic[c_data.m_Category];
					ExcelWorksheet ws = excelPackage.Workbook.Worksheets.Add( sheet_name );
					ws.Cells[1, 1].LoadFromDataTable( dt, true );

					ws.Cells.AutoFitColumns();
					ws.Cells.Style.WrapText = true;
					ws.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
					//					ws.Cells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
					//					ws.Cells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
					//					ws.Cells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
					//					ws.Cells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

					//					ws.Column( 1 ).Style.Fill.PatternType = ExcelFillStyle.Solid;
					//					ws.Column( 1 ).Style.Fill.BackgroundColor.SetColor( ToolUtil.GetColorFromHEX( "#BCDAB6" ) );

					//					ws.Column( 2 ).Style.Fill.PatternType = ExcelFillStyle.Solid;
					//					ws.Column( 2 ).Style.Fill.BackgroundColor.SetColor( ToolUtil.GetColorFromHEX( "#D6BAAA" ) );

					ExcelRow firstRow = ws.Row( 1 );
					firstRow.Style.Fill.PatternType = ExcelFillStyle.Solid;
					firstRow.Style.Fill.BackgroundColor.SetColor( ToolUtil.GetColorFromHEX( "#DDD9C4" ) );
					firstRow.Style.Font.Color.SetColor( Color.Black );

					// cell color
					foreach( ExcelCellColorData data in excelColorList )
					{
						ExcelRange coloredCell = ws.Cells[data.m_Row, 1, data.m_Row, last_col];
						coloredCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
						coloredCell.Style.Fill.BackgroundColor.SetColor( data.m_Color );
						coloredCell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
						coloredCell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
						coloredCell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
						coloredCell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
					}

					// prev korean cell color
					if( excelPrevKoreanDiffColorList.Count > 0 )
					{
						foreach( ExcelCellColorData data in excelPrevKoreanDiffColorList )
						{
							ExcelRange coloredCell = ws.Cells[data.m_Row, data.m_Col, data.m_Row, data.m_Col];
							coloredCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
							coloredCell.Style.Fill.BackgroundColor.SetColor( data.m_Color );
							coloredCell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
							coloredCell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
							coloredCell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
							coloredCell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
						}
					}

					// progress
					count++;
					progress = (int)( ( (float)count / translate_data.CategoryList.Count ) * 100f );
					progress_export.Value = progressBase + progress;
					progress_export.Update();

					lv_filelist.Items[listIdx].SubItems[2].Text = string.Format( "{0}%", progress );
					lv_filelist.Update();
				}

				progress_export.Value = progressBase + 100;
				progress_export.Update();
				lv_filelist.Items[listIdx].SubItems[2].Text = "100%";
				lv_filelist.Update();

				FileInfo saveInfo = new FileInfo( exportFileName );
				excelPackage.SaveAs( saveInfo );

				if( tagissue_writer != null )
				{
					tagissue_writer.Flush();
					tagissue_writer.Close();
					tagissue_writer = null;
				}

				if( changedonly_writer != null )
				{
					changedonly_writer.Flush();
					changedonly_writer.Close();
					changedonly_writer = null;
				}

				// xml copy
				if( _language == null )
					File.Copy( filepath, Path.Combine( exportPath, Path.GetFileName( filepath ) ), true );

				// langauge copy
				if( _language != null )
					DoTranslateExport_CopyLanguage( exportFileName, lan_export_path );
			}
		}

		//------------------------------------------------------------------------	
		void _DoTranslateExport_XML2Excel_ChangedOnly( string filepath, string exportPath, TranslateData translate_data, string language, bool is_duplicated_include )
		{
			string dup_include_name = "";
			if( is_duplicated_include )
				dup_include_name = DUPLICATED_SUFFIX;

			string exportFileName = Path.Combine( exportPath, string.Format( "{0}{1}{2}.xlsx", Path.GetFileNameWithoutExtension( filepath ), CHANGEDONLY, dup_include_name ) );
			string lan_export_path = "";
			if( language != null )
			{
				exportFileName = Path.Combine( exportPath, string.Format( "{0}{1}_{2}{3}.xlsx", Path.GetFileNameWithoutExtension( filepath ), CHANGEDONLY, language, dup_include_name ) );

				lan_export_path = Path.Combine( exportPath, DateTime.Now.ToString( "yyyyMMdd" ) + "_" + language, CHANGEDONLY );
				if( is_duplicated_include )
					lan_export_path = Path.Combine( exportPath, DateTime.Now.ToString( "yyyyMMdd" ) + "_" + language );
			}

			List<TranslateData.StringData> kor_changed_only_equal_text_list = new List<TranslateData.StringData>();
			List<TranslateData.StringData> kor_duplicated_text_list = new List<TranslateData.StringData>();

			using( ExcelPackage excelPackage = new ExcelPackage() )
			{
				Dictionary<string, string> category_sheet_dic = MakeExcelCategorySheet( excelPackage, translate_data );

				foreach( TranslateData.CategoryData c_data in translate_data.CategoryList )
				{
					if( c_data.m_StringList.Exists( t => t.IsChanged ) == false )
						continue;

					if( language != null && ( c_data.m_StringList.Exists( s => s.m_Language == KOREAN_LANGUAGE && s.HasTranslateFlag( I18NTextConst.eTranslateFlag.IgnoreTranslate ) == false ) == false ) )
						continue;



					bool is_english_include = false;
					if( language != null && language != "English" /*&& cb_export_eng_include.Checked*/ )
						is_english_include = true;

					DataTable dt = new DataTable( c_data.m_Category );
					dt.Columns.Add( "id" );
					dt.Columns.Add( KOREAN_LANGUAGE );
					dt.Columns.Add( $"{KOREAN_LANGUAGE}_State" );
					if( language == null )
					{
						foreach( TBLExportTranslate.LanguageCultureData lan in mTranslate.SupportedLanguages )
						{
							dt.Columns.Add( lan.language );
							dt.Columns.Add( lan.language + "_State" );
							dt.Columns.Add( lan.language + "_TagIssue" );
						}
					}
					else
					{
						if( is_english_include )
							dt.Columns.Add( "_ENG_" );

						dt.Columns.Add( language );
						dt.Columns.Add( language + "_State" );
						dt.Columns.Add( language + "_TagIssue" );
					}

					List<ExcelCellColorData> excelColorList = new List<ExcelCellColorData>();
					Dictionary<string, RowCellData> rowDIc = new Dictionary<string, RowCellData>();

					int last_col = dt.Columns.Count;
					int cell_row = 1;
					foreach( TranslateData.StringData s_data in c_data.m_StringList )
					{
						//if( cb_xmlmerge_prev_changed_check.Checked == false )
						{
							if( s_data.m_Language == KOREAN_LANGUAGE && s_data.IsChanged == false )
								continue;
						}

						TranslateData.StringData kor_data = c_data.m_StringList.Find( l => l.m_ID == s_data.m_ID && l.m_Language == KOREAN_LANGUAGE );
						if( kor_data != null && kor_data.IsChanged == false && s_data.IsChanged == false )
							continue;

						if( s_data.m_State == TranslateData.eState.REMOVED )
							continue;

						if( language != null && s_data.m_Language != KOREAN_LANGUAGE && s_data.m_Language != language )
							continue;

						if( language != null && kor_data != null && kor_data.HasTranslateFlag( I18NTextConst.eTranslateFlag.IgnoreTranslate ) )
							continue;

						//if( cb_changedonly_removeempty.Checked )
						//{
						//	if( s_data.m_Language != KOREAN_LANGUAGE && kor_data != null && s_data.m_State == TranslateData.eState.EMPTY && kor_data.m_State == TranslateData.eState.None )
						//		continue;
						//}

						TranslateData.StringData english_data = null;
						if( is_english_include )
							english_data = c_data.m_StringList.Find( a => a.m_Language == "English" && a.m_ID == s_data.m_ID );

						// check already changedonly insert text
						// 						if( kor_data != null )
						// 						{
						// 							if( s_data.m_Language == KOREAN_LANGUAGE )
						// 							{
						// 								if( kor_changed_only_equal_text_list.Exists( k => k.IsTextDifferent( kor_data.m_TextList ) == false ) == true )
						// 									continue;
						// 
						// 								if( kor_changed_only_equal_text_list.Contains( kor_data ) == false )
						// 									kor_changed_only_equal_text_list.Add( kor_data );
						// 							}
						// 							else
						// 							{
						// 								if( kor_changed_only_equal_text_list.Exists( k => k.m_ID == s_data.m_ID ) == false )
						// 									continue;
						// 							}							
						// 						}

						string stateText = s_data.m_State != TranslateData.eState.None ? s_data.m_State.ToString() : "";
						if( language == null && s_data.HasTranslateFlag( I18NTextConst.eTranslateFlag.IgnoreTranslate ) )
							stateText = TranslateData.eState.IGNORETRANS.ToString();

						string tag_issue_text = "";
						if( s_data.m_Language != KOREAN_LANGUAGE )
						{
							//if( kor_data != null && kor_data.m_State != TranslateData.eState.ADDED && kor_data.m_State != TranslateData.eState.CHANGED
							//	&& s_data.m_State != TranslateData.eState.EMPTY && s_data.HasTranslateFlag( I18NTextConst.eTranslateFlag.IgnoreTranslate ) == false )
							//{
							//	string k_text = kor_data.MakeExcelText();
							//	string s_text = s_data.MakeExcelText();
							//	string fixed_text = "";
							//	string reverse_error_msg = "";
							//	if( ExportUtil.CheckTAG( Path.GetFileNameWithoutExtension( exportFileName ), kor_data.m_TranslateFlags, k_text, s_text, mCheckTagPolicyList, mCheckTagPolicyFlags, ref tag_issue_text, ref reverse_error_msg, ref fixed_text ) == false )
							//	{
							//		StringBuilder sb = new StringBuilder();
							//		sb.AppendLine( string.Format( "- TagIssue:[{0}][{1}][{2}] {3}", c_data.m_Category, s_data.m_ID, s_data.m_Language, tag_issue_text ) );
							//		if( string.IsNullOrEmpty( reverse_error_msg ) == false )
							//		{
							//			sb.AppendLine( "<==== ReverseTagIssue : " + reverse_error_msg );
							//		}
							//		sb.AppendLine( k_text );
							//		sb.AppendLine( "====>" );
							//		sb.AppendLine( s_text );
							//		if( string.IsNullOrEmpty( fixed_text ) == false )
							//		{
							//			sb.AppendLine( "- Fixed Suggestion" );
							//			sb.AppendLine( fixed_text );
							//		}
							//		LogWrite_2_List( eLogType.Error, sb.ToString() );
							//	}
							//}
						}

						DataRow row;
						RowCellData row_cell_data;
						if( rowDIc.TryGetValue( s_data.m_ID, out row_cell_data ) == false )
						{
							row = dt.NewRow();
							dt.Rows.Add( row );
							cell_row++;

							row_cell_data = new RowCellData();
							row_cell_data.m_Row = row;
							row_cell_data.m_CellRowIndex = cell_row;
							rowDIc.Add( s_data.m_ID, row_cell_data );
						}
						else
						{
							row = row_cell_data.m_Row;
							cell_row = row_cell_data.m_CellRowIndex;
						}

						row["id"] = s_data.m_ID;
						if( language != null && kor_data != null )
						{
							row[KOREAN_LANGUAGE] = kor_data.MakeExcelText();
						}

						row[s_data.m_Language + "_State"] = stateText;

						if( s_data.m_Language != KOREAN_LANGUAGE )
						{
							row[s_data.m_Language + "_TagIssue"] = tag_issue_text;
						}

						if( is_english_include && english_data != null )
						{
							row["_ENG_"] = english_data.MakeExcelText();
						}

						if( s_data.m_State != TranslateData.eState.None || string.IsNullOrEmpty( tag_issue_text ) == false )
						{
							DataColumn dc = dt.Columns[s_data.m_Language + "_State"];
							int colIdx = dc.Ordinal + 1;

							if( excelColorList.Exists( c => c.m_Row == cell_row && c.m_Language == KOREAN_LANGUAGE ) == false )
							{
								ExcelCellColorData colorData = new ExcelCellColorData();
								colorData.m_Language = s_data.m_Language;
								colorData.m_Row = cell_row;
								colorData.m_Col = colIdx;
								switch( s_data.m_State )
								{
									case TranslateData.eState.ADDED:
									case TranslateData.eState.ADDED_EQUAL:
										colorData.m_Color = Color.Gold;
										break;

									case TranslateData.eState.CHANGED:
									case TranslateData.eState.CHANGED_EQUAL:
										colorData.m_Color = Color.LightBlue;
										break;

									case TranslateData.eState.KCHANGED:
									case TranslateData.eState.ECHANGED:
										colorData.m_Color = Color.LightGreen;
										break;

									case TranslateData.eState.REMOVED:
										colorData.m_Color = Color.Tomato;
										break;

									case TranslateData.eState.CONFILICT:
										colorData.m_Color = Color.Red;
										break;

									case TranslateData.eState.EMPTY:
									case TranslateData.eState.EMPTY_EQUAL:
										colorData.m_Color = Color.LightGray;
										break;

									default:
										colorData.m_Color = Color.Black;
										break;
								}

								if( string.IsNullOrEmpty( tag_issue_text ) == false )
									colorData.m_Color = Color.Orange;

								excelColorList.Add( colorData );
							}
						}

						row[s_data.m_Language] = s_data.MakeExcelText();

						if( s_data.m_State == TranslateData.eState.EMPTY )
						{
							List<TranslateData.StringData> other_s_datas = c_data.m_StringList.FindAll( s => s.m_ID == s_data.m_ID );
							foreach( TranslateData.StringData other_lan in other_s_datas )
							{
								if( other_lan.m_Language != s_data.m_Language )
								{
									if( language == null || other_lan.m_Language == KOREAN_LANGUAGE )
									{
										row[other_lan.m_Language] = other_lan.MakeExcelText();
									}
								}
							}
						}
					}

					if( dt.Rows.Count < 1 )
						continue;

					string sheet_name = c_data.m_Category;
					if( category_sheet_dic.ContainsKey( c_data.m_Category ) )
						sheet_name = category_sheet_dic[c_data.m_Category];
					ExcelWorksheet ws = excelPackage.Workbook.Worksheets.Add( sheet_name );
					ws.Cells[1, 1].LoadFromDataTable( dt, true );

					ws.Cells.AutoFitColumns();
					ws.Cells.Style.WrapText = true;
					ws.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
					// 					ws.Cells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
					// 					ws.Cells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
					// 					ws.Cells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
					// 					ws.Cells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
					// 
					// 					ws.Column( 1 ).Style.Fill.PatternType = ExcelFillStyle.Solid;
					// 					ws.Column( 1 ).Style.Fill.BackgroundColor.SetColor( ToolUtil.GetColorFromHEX( "#BCDAB6" ) );
					// 
					// 					ws.Column( 2 ).Style.Fill.PatternType = ExcelFillStyle.Solid;
					// 					ws.Column( 2 ).Style.Fill.BackgroundColor.SetColor( ToolUtil.GetColorFromHEX( "#D6BAAA" ) );

					ExcelRow firstRow = ws.Row( 1 );
					firstRow.Style.Fill.PatternType = ExcelFillStyle.Solid;
					firstRow.Style.Fill.BackgroundColor.SetColor( ToolUtil.GetColorFromHEX( "#DDD9C4" ) );
					firstRow.Style.Font.Color.SetColor( Color.Black );

					// cell color
					foreach( ExcelCellColorData data in excelColorList )
					{
						ExcelRange coloredCell = ws.Cells[data.m_Row, 1, data.m_Row, last_col];
						coloredCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
						coloredCell.Style.Fill.BackgroundColor.SetColor( data.m_Color );
						coloredCell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
						coloredCell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
						coloredCell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
						coloredCell.Style.Border.Right.Style = ExcelBorderStyle.Thin;

					}
				}

				FileInfo saveInfo = new FileInfo( exportFileName );
				if( excelPackage.Workbook.Worksheets.Count > 1 )
				{
					excelPackage.SaveAs( saveInfo );

					// language copy
					if( language != null )
						DoTranslateExport_CopyLanguage( exportFileName, lan_export_path );
				}
			}
		}
		void DoTranslateExport_CopyLanguage( string export_file, string target_folder )
		{
			if( Directory.Exists( target_folder ) == false )
				Directory.CreateDirectory( target_folder );

			File.Copy( export_file, Path.Combine( target_folder, Path.GetFileName( export_file ) ) );
		}

		void DoTranslateExport_Excel2XML( string filepath, int progressBase, int listIdx, bool from_merge = false )
		{
			if( File.Exists( filepath ) == false )
			{
				LogWrite_2_List( eLogType.Error, "DoExportLocalizeExcel2XML : Export(FromMerge:{0}) file({1}) not found:", from_merge, filepath );
				return;
			}

			LogWrite_2_List( eLogType.Normal, "====================================================" );
			if( from_merge )
			{
				LogWrite_2_List( eLogType.Normal, "Export(FromMerge) Begin:{0}", filepath );
			}
			else
			{
				LogWrite_2_List( eLogType.Normal, "Export Begin:{0}", filepath );

				lv_filelist.Items[listIdx].BackColor = Color.Blue;
				lv_filelist.Items[listIdx].ForeColor = Color.White;
			}

			TranslateData trans_data = new TranslateData( this );

			using( ExcelPackage excelPackage = new ExcelPackage( new FileInfo( filepath ) ) )
			{
				DataSet ds = new DataSet();
				foreach( ExcelWorksheet ws in excelPackage.Workbook.Worksheets )
				{
					DataTable dt = ExcelWorkSheet2DataTable( ws, false );
					ds.Tables.Add( dt );
				}

				trans_data.LoadFromDataset( ds, true, KOREAN_LANGUAGE, false, false, filepath );
			}

			// FULLSEP
			foreach( TBLExportTranslate.LanguageCultureData _language in mTranslate.SupportedLanguages )
			{
				if( IsSupportedLanguageChecked( _language.language ) == false )
					continue;

				string full_path = filepath.Replace( ".xlsx", string.Format( "{0}_{1}.xlsx", FULL_SEP, _language.language ) );
				if( File.Exists( full_path ) )
				{
					using( ExcelPackage excelPackage = new ExcelPackage( new FileInfo( full_path ) ) )
					{
						DataSet ds = new DataSet();

						foreach( ExcelWorksheet ws in excelPackage.Workbook.Worksheets )
						{
							if( trans_data.SheetValid( ws.Name ) == false )
							{
								LogWrite_2_List( eLogType.Error, "invalid sheet name:{0}", ws.Name );
								continue;
							}

							//LogWrite_2_List( eLogType.Important, "FULLSEP:" +  ws.Name );
							DataTable dt = ExcelWorkSheet2DataTable( ws, false );
							ds.Tables.Add( dt );
						}

						trans_data.LoadFromDataset( ds, false, _language.language, true, false, full_path );
					}
				}
			}

			//CHANGEDONLY or DIFFONLY
			foreach( TBLExportTranslate.LanguageCultureData _language in mTranslate.SupportedLanguages )
			{
				if( IsSupportedLanguageChecked( _language.language ) == false )
					continue;

				string changeOnlyPath = filepath.Replace( ".xlsx", string.Format( "{0}_{1}.xlsx", CHANGEDONLY, _language.language ) );
				string diffonlypath = filepath.Replace( ".xlsx", string.Format( "{0}_{1}.xlsx", DIFFONLY, _language.language ) );
				string exist_path = "";

				if( File.Exists( changeOnlyPath ) )
					exist_path = changeOnlyPath;
				else if( File.Exists( diffonlypath ) )
					exist_path = diffonlypath;

				if( string.IsNullOrEmpty( exist_path ) == false )
				{
					using( ExcelPackage excelPackage = new ExcelPackage( new FileInfo( exist_path ) ) )
					{
						DataSet ds = new DataSet();

						foreach( ExcelWorksheet ws in excelPackage.Workbook.Worksheets )
						{
							if( trans_data.SheetValid( ws.Name ) == false )
							{
								LogWrite_2_List( eLogType.Error, "invalid sheet name:{0}", ws.Name );
								continue;
							}

							//LogWrite_2_List( eLogType.Important, "CHANGEDONLY:" + ws.Name );
							DataTable dt = ExcelWorkSheet2DataTable( ws, false );
							ds.Tables.Add( dt );
						}

						trans_data.LoadFromDataset( ds, false, _language.language, false, true, exist_path );
					}
				}
			}

			trans_data.DoExportXML( filepath, mTranslate.SupportedLanguages.FindAll( s => IsSupportedLanguageChecked( s.language ) ), false, from_merge );

			if( from_merge == false )
			{
				lv_filelist.Items[listIdx].BackColor = Color.White;
				lv_filelist.Items[listIdx].ForeColor = Color.Black;

				progress_export.Value = progressBase + 100;
				progress_export.Update();
				lv_filelist.Items[listIdx].SubItems[2].Text = "100%";
				lv_filelist.Update();
			}
		}

		DataTable ExcelWorkSheet2DataTable( ExcelWorksheet ws, bool hasHeader )
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

		//------------------------------------------------------------------------	
		Dictionary<string, string> MakeExcelCategorySheet( ExcelPackage excel, TranslateData translate_data )
		{
			Dictionary<string, string> category_sheet_dic = new Dictionary<string, string>();
			DataTable category_dt = new DataTable( TranslateData.CATEGORY_TABLE_NAME );
			category_dt.Columns.Add( "sheet" );
			category_dt.Columns.Add( "category" );
			foreach( TranslateData.CategoryData c_data in translate_data.CategoryList )
			{
				if( category_sheet_dic.ContainsKey( c_data.m_Category ) == false )
				{
					string sheet_base_name = c_data.m_Category;
					if( sheet_base_name.Length > 28 )
						sheet_base_name = c_data.m_Category.Substring( 0, 28 );
					string sheet_name = sheet_base_name;
					int exist_name_count = 1;
					while( true )
					{
						if( category_sheet_dic.Values.Contains( sheet_name ) == false )
						{
							category_sheet_dic.Add( c_data.m_Category, sheet_name );

							DataRow row = category_dt.NewRow();
							row["sheet"] = sheet_name;
							row["category"] = c_data.m_Category;
							category_dt.Rows.Add( row );
							break;
						}
						sheet_name = string.Format( "{0}_{1:00}", sheet_base_name, exist_name_count );
						exist_name_count++;
					}
				}
			}

			ExcelWorksheet ws = excel.Workbook.Worksheets.Add( TranslateData.CATEGORY_TABLE_NAME );
			ws.Cells[1, 1].LoadFromDataTable( category_dt, true );
			ws.Cells.AutoFitColumns();
			ws.Cells.Style.WrapText = true;
			ws.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
			ExcelRow firstRow = ws.Row( 1 );
			firstRow.Style.Fill.PatternType = ExcelFillStyle.Solid;
			firstRow.Style.Fill.BackgroundColor.SetColor( ToolUtil.GetColorFromHEX( "#DDD9C4" ) );
			firstRow.Style.Font.Color.SetColor( Color.Black );

			return category_sheet_dic;
		}

		//------------------------------------------------------------------------	
		TBLToolParameters GetTBLToolParameter( string datalistID )
		{
			TBLToolParameters tool_parameter = null;
			// 			switch( datalistID )
			// 			{
			// 				case "TBL_AccountLevel": tool_parameter = AccountLevelInfoManager.ToolParam; break;
			// 				case "TBL_Achievement": tool_parameter = AchievementInfoManager.ToolParam; break;
			// 				case "TBL_Attendance": tool_parameter = AttendanceInfoManager.ToolParam; break;
			// 				case "TBL_Card": tool_parameter = CardInfoManager.ToolParam; break;
			// 				case "TBL_CardLevelExp": tool_parameter = CardLevelExpInfoManager.ToolParam; break;
			// 				case "TBL_CardLevelParam": tool_parameter = CardLevelParamInfoManager.ToolParam; break;
			// 				case "TBL_CardPiece": tool_parameter = CardPieceInfoManager.ToolParam; break;
			// 				case "TBL_CardType": tool_parameter = CardTypeInfoManager.ToolParam; break;
			// 				case "TBL_Collection": tool_parameter = CollectionInfoManager.ToolParam; break;
			// 				case "TBL_Dungeon": tool_parameter = DungeonInfoManager.ToolParam; break;
			// 				case "TBL_DungeonDrop": tool_parameter = DungeonDropInfoManager.ToolParam; break;
			// 				case "TBL_DungeonRuneMix": tool_parameter = DungeonRuneMixInfoManager.ToolParam; break;
			// 				case "TBL_Equipment": tool_parameter = EquipmentInfoManager.ToolParam; break;
			// 				case "TBL_EquipmentSet": tool_parameter = EquipmentSetInfoManager.ToolParam; break;
			// 				case "TBL_Gacha": tool_parameter = GachaInfoManager.ToolParam; break;
			// 				case "TBL_GameItem": tool_parameter = GameItemInfoManager.ToolParam; break;
			// 				case "TBL_GiftBox": tool_parameter = GiftBoxInfoManager.ToolParam; break;
			// 				case "TBL_GuildSkill": tool_parameter = GuildSkillInfoManager.ToolParam; break;
			// 				case "TBL_ItemMix": tool_parameter = ItemMixInfoManager.ToolParam; break;
			// 				case "TBL_ItemShop": tool_parameter = ItemShopInfoManager.ToolParam; break;
			// 				case "TBL_Mission": tool_parameter = MissionInfoManager.ToolParam; break;
			// 				case "TBL_Raid": tool_parameter = RaidInfoManager.ToolParam; break;
			// 				case "TBL_RankReward": tool_parameter = RankRewardInfoManager.ToolParam; break;
			// 				case "TBL_RaidAnnounce": tool_parameter = RaidAnnounceInfoManager.ToolParam; break;
			// 				case "TBL_Skill": tool_parameter = SkillInfoManager.ToolParam; break;
			// 				case "TBL_SkillGroup": tool_parameter = SkillGroupInfoManager.ToolParam; break;
			// 				case "TBL_SoulTower2": tool_parameter = SoulTower2InfoManager.ToolParam; break;
			// 				case "TBL_SoulTower": tool_parameter = SoulTowerInfoManager.ToolParam; break;
			// 				//case "TBL_Talent": tool_parameter = ; check_params = VIPInfoManager.ToolCheckParam; break;
			// 				case "TBL_Talk": tool_parameter = TalkInfoManager.ToolParam; break;
			// 				case "TBL_UEvent": tool_parameter = UEventInfoManager.ToolParam; break;
			// 				case "TBL_UNotice": tool_parameter = UNoticeInfoManager.ToolParam; break;
			// 				case "TBL_UseItem": tool_parameter = UseItemInfoManager.ToolParam; break;
			// 				case "TBL_Duel2RoundReward": tool_parameter = Duel2RoundRewardInfoManager.ToolParam; break;
			// 				case "TBL_Loading":
			// 					{
			// 						tool_parameter = new ToolParameters();
			// 						tool_parameter.ExportIgnore = true;
			// 						tool_parameter.LocalizeOnly = true;
			// 					}
			// 					break;
			// 						
			// 			}

			if( tool_parameter == null )
				return new TBLToolParameters();
			return tool_parameter;
		}

		//------------------------------------------------------------------------	
		bool CheckTBLVerify(string datalistID, List<string> field_list, ref bool check_field_length_not_matched)
		{
			TBLToolParameters tool_param = GetTBLToolParameter( datalistID );
			if( tool_param == null )
			{
				check_field_length_not_matched = true;
				LogWrite_2_List( eLogType.Important, "ToolParameter({0}) not created. Call to Developer", datalistID );
				return true;
			}

			List<string> verifyField = tool_param.TBLVerifyColumn;

			if( tool_param.ExportIgnore )
				return true;

			if( verifyField == null )
			{
				check_field_length_not_matched = true;
				LogWrite_2_List( eLogType.Important, "verifyField({0}) not created. Call to Developer", datalistID );
				return true;
			}

			if( field_list.Count( f => string.IsNullOrEmpty(f) == false) != verifyField.Count )
			{
				string debugLog = "";
				foreach( string fieldText in field_list )
				{
					if( string.IsNullOrEmpty(fieldText) == false )
						debugLog += string.Format( "[{0}]", fieldText );
				}

				debugLog += " != ";
				foreach( string verifyText in verifyField )
				{
					debugLog += string.Format( "[{0}]", verifyText );
				}

				check_field_length_not_matched = true;
				LogWrite_2_List( eLogType.Important, "Field length not matched:{0}\n{1}", datalistID, debugLog );
				return false;
			}

			string name_not_matched = "";
			List<string> verify_name_only = verifyField.ToList();
			for(int i=0; i<field_list.Count; i++)
			{
				if( string.IsNullOrEmpty( field_list[i] ) == false && field_list[i] != verify_name_only[i] )
				{
					name_not_matched += string.Format( "\nField Name not matched X:{0} != V:{1}", field_list[i], verify_name_only[i] );
				}
			}

			if( string.IsNullOrEmpty( name_not_matched ) == false )
			{
				name_not_matched = datalistID + name_not_matched;
				LogWrite_2_List( eLogType.Important, name_not_matched );
				return false;
			}

			return true;
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
		}

		private void btn_logclear_Click( object sender, EventArgs e )
		{
			lv_log.Items.Clear();
			rtb_log.Clear();
		}

		//------------------------------------------------------------------------
		string _devFieldExport(string xlsx_file_path)
		{
			string datalistID = Path.GetFileNameWithoutExtension( xlsx_file_path );
			DataSet ds = ExportUtil.LoadXLSX( xlsx_file_path, false );

			string outText = "// " + datalistID;
			outText += "\n#if UMTOOL";
			outText += "\n\tpublic static TBLToolParameters ToolParam = new TBLToolParameters()";
			outText += "\n\t{";
			outText += "\n\t\t// DO NOT CHANGE COLUMN ORDER";
			outText += "\n\t\tTBLVerifyColumn = new List<string>()";
			outText += "\n\t\t{";
			outText += "\n\t\t\t";
			int line = 0;
			foreach( DataTable table in ds.Tables )
			{
				if( table.TableName == "Data" )
				{
					DataRow dataRow = table.Rows[0];
					for( int col = 0; col < table.Columns.Count; col++ )
					{
						string colText = dataRow[col].ToString();
						if( string.IsNullOrEmpty( colText ) == false )
						{
							outText += "\"" + dataRow[col].ToString() + "\" , ";

							line++;
							if( line > 5 && col != table.Columns.Count )
							{
								line = 0;
								outText += "\n\t\t\t";
							}
						}
					}
				}
			}
			outText += "\n\t\t},";
			outText += "\n\t\tUseClient = true,";
			outText += "\n\t\tCheckSameIds = true,";
			outText += "\n\t};";
			outText += "\n#endif";

			ListViewItem lv_item = new ListViewItem( outText );
			lv_log.Items.Add( lv_item );

			ds.Dispose();

			return outText;
		}
		private void btn_devFieldExport_Click( object sender, EventArgs e )
		{
			if( mFilePathList.Count <= 0 )
				return;

			StringBuilder sb = new StringBuilder();

			progress_export.Maximum = mFilePathList.Count * 100;
			progress_export.Value = 0;
			progress_export.Update();
			for( int i = 0; i < mFilePathList.Count; i++ )
			{
				sb.AppendLine( _devFieldExport( mFilePathList[i].filePath ) );
			}

			FormView view = new FormView();
			view.SetText( sb.ToString() );
			view.ShowDialog();
		}

		private void lv_log_KeyUp( object sender, KeyEventArgs e )
		{
			if( sender != lv_log )
				return;

			if( e.Control && e.KeyCode == Keys.C )
			{
				StringBuilder sb = new StringBuilder();
				foreach( ListViewItem lvitem in lv_log.SelectedItems )
					sb.AppendLine( lvitem.Text );

				Clipboard.SetText( sb.ToString() );
			}
		}

		private void btn_dev_Click( object sender, EventArgs e )
		{

		}

		private void btn_checkEncrypt_Click( object sender, EventArgs e )
		{
			ProjectConfig.Data p_data_config = ProjectConfig.Instance.CurrentProjectData;

			if( mProjectGlobaltypeProperty == null )
				return;

			if( Directory.Exists( mProjectGlobaltypeProperty.TBLPath ) == false )
			{
				MessageBox.Show( $"테이블 경로를 확인하세요.{mProjectGlobaltypeProperty.TBLPath}" );
				return;
			}

			openFileDialog.InitialDirectory = mProjectGlobaltypeProperty.TBLPath;
			openFileDialog.FileName = "";
			openFileDialog.DefaultExt = "bytes";
			openFileDialog.Filter = $"bytes파일|*.bytes|all|*.*";
			if( openFileDialog.ShowDialog() != DialogResult.OK )
				return;

			string filepath = openFileDialog.FileName;
			string full_ext = ToolUtil.GetFileExtentionFull( Path.GetFileName( filepath ) );
			string data_id = Path.GetFileName( filepath ).Replace( full_ext, "" );

			string type_ext = full_ext.Replace( ".bytes", "" );
			if( type_ext == TBLInfoBase.EXTENSION_XML_ENCRYPT )
			{
				string xml_str = XMLUtil.LoadXmlFromEncryptFile( filepath, p_data_config.EncryptKey8 );

				FormView view = new FormView();
				view.SetText( xml_str );
				view.ShowDialog();
			}
			else if( type_ext == TBLInfoBase.EXTENSION_XML_BINARY )
			{
				XmlBinary xml_bin = new XmlBinary( null, filepath, false, false, "" );
				string xml_str = xml_bin.DebugInfo();
				FormView view = new FormView();
				view.SetText( xml_str );
				view.ShowDialog();
			}
			else if( type_ext == TBLInfoBase.EXTENSION_XML_BINARY_ENCRYPT )
			{
				XmlBinary xml_bin = new XmlBinary( null, filepath, false, true, p_data_config.EncryptKey8 );
				string xml_str = xml_bin.DebugInfo();
				FormView view = new FormView();
				view.SetText( xml_str );
				view.ShowDialog();
			}
		}

		private void lv_log_MouseDoubleClick( object sender, MouseEventArgs e )
		{
			if( e.Button.Equals( MouseButtons.Right ) || e.Button.Equals( MouseButtons.Middle ) )
				return;

			if( lv_log.SelectedItems.Count == 1 )
			{
				ListViewItem lvitem = lv_log.SelectedItems[0];
				string log_message = lvitem.Text;

				MessageBox.Show( log_message );
			}
		}

		private void lv_filelist_MouseClick( object sender, MouseEventArgs e )
		{
			if( e.Button.Equals( MouseButtons.Right ) == false )
				return;

			ContextMenu cm = null;
			int selected_item_idx = lv_filelist.GetItemAt( e.X, e.Y ).Index;
			if( cm == null )
				cm = new ContextMenu();

			if( IsCurrentTab( eTABType.TBLExport ) )
			{
				MenuItem menu = new MenuItem();
				menu.Text = "TextOnlyExport";
				menu.Click += ( senders, es ) =>
				{
					OnTBLExport( selected_item_idx, true, true );
				};

				cm.MenuItems.Add( menu );

				menu = new MenuItem();
				menu.Text = "FiledToScript";
				menu.Click += ( senders, es ) =>
				{
					if( mFilePathList.Count > 0 && selected_item_idx < mFilePathList.Count )
					{
						string file_path = mFilePathList[selected_item_idx].filePath;
						string text = _devFieldExport( file_path );

						FormView view = new FormView();
						view.SetText( text );
						view.ShowDialog();
					}
				};

				cm.MenuItems.Add( menu );
			}
			else if( IsCurrentTab( eTABType.I18NTextExport ) )
			{

			}

			MenuItem open_menu = new MenuItem();
			open_menu.Text = "Open Folder";
			open_menu.Click += ( senders, es ) =>
			{
				if( mFilePathList.Count > 0 && selected_item_idx < mFilePathList.Count )
				{
					string file_path = mFilePathList[selected_item_idx].filePath;
					string dir = Path.GetDirectoryName( file_path );
					System.Diagnostics.Process.Start( "explorer.exe", dir );
				}
			};

			cm.MenuItems.Add( open_menu );

			if( cm != null )
				cm.Show( lv_filelist, new Point( e.X, e.Y ) );
		}

		private void lv_filelist_MouseDoubleClick( object sender, MouseEventArgs e )
		{
			if( mExportProgress )
				return;

			if( e.Button.Equals( MouseButtons.Right ) || e.Button.Equals( MouseButtons.Middle ) )
				return;

			if( IsCurrentTab( eTABType.TBLExport ) )
			{
				OnTBLExport( lv_filelist.SelectedItems[0].Index, true, false );
			}
			else if( IsCurrentTab( eTABType.I18NTextExport ) )
			{
				OnI18NExportClicked( lv_filelist.SelectedItems[0].Index );
			}
		}

		private void lv_filelist_KeyDown( object sender, KeyEventArgs e )
		{
			if( IsCurrentTab( eTABType.TBLExport ) == false )
				return;

			// 			if( radio_ServerConfigCompare.Checked )
			// 			{
			// 				if( e.KeyCode == Keys.Enter )
			// 				{
			// 					string selected_item = lv_filelist.SelectedItems[0].Text;
			// 					DoCompareTo( selected_item );
			// 				}
			// 			}
		}

		private void btn_localizeOnlyExport_Click( object sender, EventArgs e )
		{
			OnTBLExport( -1, true, true );
		}

		private void btn_project_select_Click( object sender, EventArgs e )
		{
			using( FormProjectProperty form = new FormProjectProperty() )
			{
				form.ShowDialog();
			}

			AllRefresh();
		}

		//------------------------------------------------------------------------		
		public class I18NTextSummaryData
		{
			public class SameText
			{
				public string text = "";
				public int count = 0;
			}

			public int full_data_count = 0;
			public int full_char_length = 0;
			public int full_char_ignore_space_length = 0;
			public int full_word_length = 0;

			public List<SameText> same_data_list = new List<SameText>();

			public void CheckText( string text, bool same_ignore )
			{
				if( text.StartsWith( "$$" ) )
					return;

				SameText same_text = same_data_list.Find( a => a.text == text );
				if( same_text != null )
				{
					same_text.count += 1;
					if( same_ignore )
						return;
				}
				else
				{
					same_text = new SameText();
					same_text.text = text;
					same_text.count = 1;
					same_data_list.Add( same_text );
				}

				full_data_count += 1;
				full_char_length += text.Length;
				full_char_ignore_space_length += text.Replace( " ", "" ).Length;
				full_word_length += text.Split( new char[] { ' '}, StringSplitOptions.RemoveEmptyEntries ).Length;
			}

			public string GetResult( bool detail )
			{
				same_data_list.RemoveAll( a => a.count <= 1 );

				StringBuilder sb = new StringBuilder();
				sb.AppendLine( $"* 전체 데이터 수 : {full_data_count.ToString( "N0" )}" );
				sb.AppendLine( $"* 전체 문자수 : {full_char_length.ToString( "N0" )}" );
				sb.AppendLine( $"* 전체 문자수(공백제외) : {full_char_ignore_space_length.ToString( "N0" )}" );
				sb.AppendLine( $"* 전체 단어수(띄어쓰기기준) : {full_word_length.ToString( "N0" )}" );
				sb.AppendLine( $"* 동일한 텍스트 수 : {same_data_list.Count.ToString( "N0" )}" );

				if( detail )
				{
					foreach( I18NTextSummaryData.SameText same_text in same_data_list )
					{
						sb.AppendLine( $"\t동일한 텍스트({same_text.count.ToString( "N0" )}) : {same_text.text.Replace( "\n", "\\n" )}" );
					}
				}

				return sb.ToString();
			}
		}

		private void btn_i18ntext_summary_Click( object sender, EventArgs e )
		{
			ProjectConfig.Data project_data = ProjectConfig.Instance.CurrentProjectData;

			string tbl_path = mProjectGlobaltypeProperty.TBLPath;
			string export_path = Path.Combine( tbl_path, TBLInfoBase.EXPORT_PATH );

			StringBuilder sb = new StringBuilder();
			sb.AppendLine( "= Text SUMMARY =" );

			List<string> languages = new List<string>();
			languages.Add( KOREAN_LANGUAGE );
			languages.AddRange( mTranslate.SupportedLanguages.Select( a => a.language ) );

			foreach( string language in languages )
			{
				string lan_path = Path.Combine( export_path, I18NTextConst.DEFAULT_PATH_ROOT, I18NTextConst.DEFAULT_PATH_LANGUAGES, language );
				if( Directory.Exists( lan_path ) == false )
					continue;

				string[] text_files = Directory.GetFiles( lan_path, "*.xml" );

				sb.AppendLine( $"-----------------------------------" );
				sb.AppendLine( $"> Language={language} Path={lan_path} Files={text_files.Length}" );

				I18NTextSummaryData summary = new I18NTextSummaryData();
				I18NTextSummaryData summary_sameignore = new I18NTextSummaryData();

				foreach( string t_file in text_files )
				{
					string file_name = Path.GetFileName( t_file );

					try
					{
						XmlDocument doc = new XmlDocument();
						doc.Load( t_file );

						XmlNode root_node = doc.SelectSingleNode( "DataList" );
						int version = XMLUtil.ParseAttribute<int>( root_node, "tbl_version", 0 );

						foreach( XmlNode category_node in root_node.ChildNodes )
						{
							foreach( XmlNode data_node in category_node.ChildNodes )
							{
								string text = data_node.InnerText.Replace( "\\n", "\n" );
								summary.CheckText( text, false );
								summary_sameignore.CheckText( text, true );
							}
						}
					}
					catch( System.Exception ex )
					{
						sb.AppendLine( $"[ERROR] {file_name} : {ex.ToString()}" );
					}
				}

				sb.AppendLine( summary.GetResult( cb_text_summary_detail.Checked ) );
				sb.AppendLine( "> 중복 제거시" );
				sb.AppendLine( summary_sameignore.GetResult( false ) );
			}

			FormView view = new FormView();
			view.SetText( sb.ToString() );
			view.ShowDialog();
		}

		private void btn_i18n_help_Click( object sender, EventArgs e )
		{
			string i18ntext_def_lan_path = Path.Combine( mProjectGlobaltypeProperty.TBLPath, TBLInfoBase.EXPORT_PATH, I18NTextConst.DEFAULT_PATH_ROOT, I18NTextConst.DEFAULT_PATH_LANGUAGES, KOREAN_LANGUAGE );
			string xml2excel_path = $"{I18NTextConst.DEFAULT_PATH_ROOT}/{I18NTextExportConst.TRANSLATION_PATH}/export날짜path";
			string import_path = $"{xml2excel_path}/export_path/{I18NTextExportConst.TRANSLATION_IMPORT_PATH_PREFIX}_날짜/{I18NTextConst.DEFAULT_PATH_LANGUAGES}";
			string merged_path = $"{xml2excel_path}/export_path/{I18NTextExportConst.TRANSLATION_MERGED_PATH_PREFIX}_이전날짜";

			StringBuilder sb = new StringBuilder();
			sb.AppendLine( "번역 도움말" );
			sb.AppendLine( "" );
			sb.AppendLine( $"1. [최초] XML -> Excel:최신 Export된 xml 파일({i18ntext_def_lan_path})을 엑셀 파일로 변환한다. ({xml2excel_path}) 번역전달" );
			sb.AppendLine( "" );
			sb.AppendLine( $"2. [번역후] Excel -> XML:이전에 [병합]해서 나온 폴더({xml2excel_path})에 언어별(ex.CHANGEDONLY_Chinese)로 번역해놓은 CHANGEDONLY(혹은 FULLSEP) 엑셀을 복사 후 XML 로 변환한다." );
			sb.AppendLine( $"   번역된 데이터가 XML로 변환되어 xml 폴더({import_path}) 에 정리된다." );
			sb.AppendLine( "" );
			sb.AppendLine( $"3. [병합] MERGE : 최신 xml 파일({i18ntext_def_lan_path})과 [번역후]에서 나온 번역 xml 파일({import_path})을 병합하여 새로 추가/변경 된것을 추스려 새로운 폴더로 export({merged_path}) 된다." );
			sb.AppendLine( "" );
			sb.AppendLine( $"4. [적용] Client 적용은 [번역후]에서 나온 ({import_path}) 폴더에 있는것을 사용한다." );
			sb.AppendLine( "" );
			sb.AppendLine( "5. [추가번역전달] 새롭게 추가된 번역은 [병합]에서 나온 CHANGEDONLY, FULLSEP 파일을 전달한다." );
			sb.AppendLine( "" );
			sb.AppendLine( "> [병합]에서 나온 폴더는 다음 병합시 꼭 필요하니 저장 및 가지고 있는다." );
			sb.AppendLine( "> 엑셀 파일에서 오류가 나면 다시 저장해서 시도" );

			FormView view = new FormView();
			view.SetText( sb.ToString() );
			view.ShowDialog();
		}

		void OnI18NExportClicked( int file_list_idx = -1 )
		{
			if( radio_i18n_xml2excel.Checked )
			{
				OnTranslateExport_XML2Excel( false, file_list_idx );
			}
			else if( radio_i18n_excel2xml.Checked )
			{
				OnTranslateExport_Excel2XML( file_list_idx );
			}
			else if( radio_i18n_merge.Checked )
			{
				OnTranslateExport_XML2Excel( true, file_list_idx );
			}
			else
			{
				MessageBox.Show( "Export 타입을 선택하세요." );
			}
		}
		private void btn_i18n_export_Click( object sender, EventArgs e )
		{
			OnI18NExportClicked( -1 );
		}

		private void radio_i18n_xml2excel_CheckedChanged( object sender, EventArgs e )
		{
		}

		private void radio_i18n_excel2xml_CheckedChanged( object sender, EventArgs e )
		{
		}

		private void radio_i18n_merge_CheckedChanged( object sender, EventArgs e )
		{
		}

		private void tabControl_SelectedIndexChanged( object sender, EventArgs e )
		{
			RefreshFileList();
		}

		private void cb_i18ntext_duplicate_include_CheckedChanged( object sender, EventArgs e )
		{
			ToolUtil.SavePrefs( REG_SUB_KEY, eOptionEnableType.OPT_I18NText_ExportDuplicateInclude.ToString(), cb_i18ntext_duplicate_include.Checked );
		}

		private void radio_i18n_excel2xml_MouseClick( object sender, MouseEventArgs e )
		{
			if( radio_i18n_excel2xml.Checked )
				RefreshFileList();
		}

		private void radio_i18n_xml2excel_MouseClick( object sender, MouseEventArgs e )
		{
			if( radio_i18n_xml2excel.Checked )
				RefreshFileList();
		}

		private void radio_i18n_merge_MouseClick( object sender, MouseEventArgs e )
		{
			if( radio_i18n_merge.Checked )
				RefreshFileList();
		}
	}
}
