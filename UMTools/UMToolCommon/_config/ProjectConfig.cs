//////////////////////////////////////////////////////////////////////////
//
// ProjectConfig
// 
// Created by LCY.
//
// Copyright 2022 FourNext
// All rights reserved
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
//////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml;
using UMF.Core;
using System.Windows.Forms;
using System;

namespace UMTools.Common
{
	public class ProjectConfig
	{
		private static ProjectConfig _instance = null;
		public static ProjectConfig Instance
		{
			get
			{
				if( _instance == null )
				{
					_instance = new ProjectConfig();
					_instance.Reload();
				}

				return _instance;
			}
		}

		public const string PROJECT_CONFIG_FILE = "PROJECT_CONFIG_READONLY.xml";
		public const string PROJECT_DEFAULT_GLOBAL_TYPE = "NE";

		public class Data
		{
			public string ProjectName { get; private set; } 
			public List<string> GlobalTypeList { get; private set; }
			public string FormColor { get; private set; }
			public string EncryptKey8 { get; private set; }
			public string ProjectSubName { get; private set; }

			public Data(XmlNode node)
			{
				ProjectName = XMLUtil.ParseAttribute<string>( node, "ProjectName", "" );
				GlobalTypeList = XMLUtil.ParseAttributeToList<string>( node, "GlobalTypes", ',' );
				if( GlobalTypeList == null )
					GlobalTypeList = new List<string>();

				if( GlobalTypeList.Count <= 0 )
					GlobalTypeList.Add( PROJECT_DEFAULT_GLOBAL_TYPE );

				FormColor = XMLUtil.ParseAttribute<string>( node, "FormColor", "" );
				EncryptKey8 = XMLUtil.ParseAttribute<string>( node, "EncryptKey8", "" );

				ProjectSubName = XMLUtil.ParseAttribute<string>( node, "ProjectSubName", "" );
			}
		}

		public List<Data> DataList { get; private set; }

		// runtime
		public Data CurrentProjectData { get; private set; } = null;
		public ProjectPropertyConfig CurrentProjectProerty { get; private set; } = null;
		public ProjectGlobaltypePropertyConfig CurrentProjectGlobaltypeProperty { get; private set; } = null;
		public GlobalPropertyConfig GlobalProperty { get; private set; } = null;

		ProjectQuickSaveConfig mQuickSave = null;
		public ProjectQuickSaveConfig QuickSave { get { return mQuickSave; } }
		

		public void Reload()
		{
			DataList = new List<Data>();

			XmlDocument doc = new XmlDocument();
			doc.Load( PROJECT_CONFIG_FILE );

			XmlNode root = doc.SelectSingleNode( "Project" );

			foreach( XmlNode data_node in root.SelectNodes("Data") )
			{
				if( data_node.NodeType == XmlNodeType.Comment )
					continue;

				DataList.Add( new Data( data_node ) );
			}

			mQuickSave = ProjectQuickSaveConfig.Load();

			GlobalProperty = GlobalPropertyConfig.Load();
			CurrentProjectData = FindProject( mQuickSave.LastProjectName );
			CurrentProjectProerty = ProjectPropertyConfig.Load( mQuickSave.LastProjectName );
			CurrentProjectGlobaltypeProperty = ProjectGlobaltypePropertyConfig.Load( mQuickSave.LastProjectName, mQuickSave.LastGlobalType );
		}

		public void ApplyProject( ProjectPropertyConfig p_config, ProjectGlobaltypePropertyConfig pg_config)
		{
			CurrentProjectProerty = p_config;
			CurrentProjectGlobaltypeProperty = pg_config;
			CurrentProjectData = FindProject( p_config.ProjectName );

			mQuickSave.LastProjectName = p_config.ProjectName;
			mQuickSave.LastGlobalType = pg_config.GlobalType;
			ProjectQuickSaveConfig.Save( mQuickSave );
		}

		public Data FindProject(string project_name)
		{
			return DataList.Find( a => a.ProjectName == project_name );
		}
	}

	public class ProjectQuickSaveConfig
	{
		public string LastProjectName { get; set; } = "";
		public string LastGlobalType { get; set; } = "";

		public bool IsValid()
		{
			return ( string.IsNullOrEmpty( LastProjectName ) == false );
		}

		public static ProjectQuickSaveConfig Load()
		{
			ProjectQuickSaveConfig config = null;

			string saved_path = Path.Combine( "_config", $"__project_save_config.json" );
			if( File.Exists( saved_path ) )
				config = JsonUtil.DecodeJsonFile<ProjectQuickSaveConfig>( saved_path );

			if( config == null )
			{
				config = new ProjectQuickSaveConfig();
				if( ProjectConfig.Instance.DataList.Count > 0 )
					config.LastProjectName = ProjectConfig.Instance.DataList[0].ProjectName;
				config.LastGlobalType = ProjectConfig.PROJECT_DEFAULT_GLOBAL_TYPE;
			}

			return config;
		}

		public static void Save(ProjectQuickSaveConfig config)
		{
			JsonUtil.EncodeJsonFile( config, Path.Combine( "_config", $"__project_save_config.json" ) );
		}
	}

	//////////////////////////////////////////////////////////////////////////
	///

	[TypeConverter( typeof( PropertySorter ) )]
	public class ProjectPropertyConfig
	{
		public static ProjectPropertyConfig Load( string project_name )
		{
			ProjectPropertyConfig config = null;
			string json_file = Path.Combine( "_config", $"__{project_name}_config.json" );
			if( File.Exists( json_file ) )
			{
				config = JsonUtil.DecodeJsonFile<ProjectPropertyConfig>( json_file );
			}

			if( config == null )
				config = new ProjectPropertyConfig() { ProjectName = project_name };

			return config;
		}
		public static void Save( ProjectPropertyConfig config )
		{
			if( config == null )
				return;

			string json_file = Path.Combine( "_config", $"__{config.ProjectName}_config.json" );
			JsonUtil.EncodeJsonFile( config, json_file );
		}

		[Category( "0.Project" )]
		[ReadOnly( true )]
		public string ProjectName { get; set; } = "";

		[Category( "1.Work Path" )]
		[Description( "개발 작업 로컬 경로" )]
		[Editor( typeof( System.Windows.Forms.Design.FolderNameEditor ), typeof( System.Drawing.Design.UITypeEditor ) )]
		public string dev_work_path { get; set; } = "";

		[Category( "1.Work Path" )]
		[Description( "클라이언트 작업 로컬 경로" )]
		[Editor( typeof( System.Windows.Forms.Design.FolderNameEditor ), typeof( System.Drawing.Design.UITypeEditor ) )]
		public string client_work_path { get; set; } = "";

		[Category( "1.Work Path" )]
		[Description( "서버 작업 로컬 경로" )]
		[Editor( typeof( System.Windows.Forms.Design.FolderNameEditor ), typeof( System.Drawing.Design.UITypeEditor ) )]
		public string server_work_path { get; set; } = "";

		[Category( "2.URL" )]
		[Description( "빌드 Jenkins" )]
		public string jenkins_url { get; set; } = "";

		[Category( "2.URL" )]
		[Description( "프로젝트 SVN trunk URL" )]
		public string trunk_url { get; set; } = "";

		[Category( "2.URL" )]
		[Description( "프로젝트 SVN DEV URL" )]
		public string svn_trunk_dev_url { get; set; } = "";

		[Category( "2.URL" )]
		[Description( "프로젝트 SVN TBL URL" )]
		public string svn_trunk_tbl_url { get; set; } = "";
	}

	//------------------------------------------------------------------------	
	[TypeConverter(typeof(PropertySorter))]
	public class ProjectGlobaltypePropertyConfig
	{
		public static ProjectGlobaltypePropertyConfig Load( string project_name, string global_type )
		{
			ProjectGlobaltypePropertyConfig config = null;
			string json_file = Path.Combine( "_config", $"__{project_name}_{global_type}_config.json" );
			if( File.Exists( json_file ) )
			{
				config = JsonUtil.DecodeJsonFile<ProjectGlobaltypePropertyConfig>( json_file );
			}

			if( config == null )
				config = new ProjectGlobaltypePropertyConfig() { ProjectName = project_name, GlobalType = global_type };

			return config;
		}
		public static void Save( ProjectGlobaltypePropertyConfig config )
		{
			if( config == null )
				return;

			string json_file = Path.Combine( "_config", $"__{config.ProjectName}_{config.GlobalType}_config.json" );
			JsonUtil.EncodeJsonFile( config, json_file );
		}

		[Category( "0.Project" )]
		[ReadOnly( true )]
		public string ProjectName { get; set; } = "";

		[Category( "0.Project" )]
		[ReadOnly( true )]
		public string GlobalType { get; set; } = "";

		[Category("1.모든 툴에서 사용")]
		[Description("엑셀 테이블 파일들이 있는 폴더 경로(_TBL)")]
		[Editor( typeof( System.Windows.Forms.Design.FolderNameEditor ), typeof( System.Drawing.Design.UITypeEditor ) )]
		public string TBLPath { get; set; } = "";

		[Category( "1.모든 툴에서 사용" )]
		[Description( "XML Config 파일들이 있는 폴더 경로" )]
		[Editor( typeof( System.Windows.Forms.Design.FolderNameEditor ), typeof( System.Drawing.Design.UITypeEditor ) )]
		public string ConfigPath { get; set; } = "";

		[Category( "2.UMDistribution 툴 사용" )]
		[Description( "DEV 로컬 경로" )]
		[Editor( typeof( System.Windows.Forms.Design.FolderNameEditor ), typeof( System.Drawing.Design.UITypeEditor ) )]
		public string dev_path { get; set; } = "";
	
		[Category( "2.UMDistribution 툴 사용" )]
		[Description( "TAG 추가를 위한 TAG SVN URL" )]
		public string tag_svn_url { get; set; } = "";

		[Category( "2.UMDistribution 툴 사용" )]
		[Description( "QA TAG 로컬 경로" )]
		[Editor( typeof( System.Windows.Forms.Design.FolderNameEditor ), typeof( System.Drawing.Design.UITypeEditor ) )]
		public string qa_tag_path { get; set; } = "";

		[Category( "2.UMDistribution 툴 사용" )]
		[Description( "DEV 서버 파일 로컬 경로" )]
		[Editor( typeof( System.Windows.Forms.Design.FolderNameEditor ), typeof( System.Drawing.Design.UITypeEditor ) )]
		public string svc_dev_server_path { get; set; } = "";

		[Category( "2.UMDistribution 툴 사용" )]
		[Description( "DEV 패치 로컬 경로" )]
		[Editor( typeof( System.Windows.Forms.Design.FolderNameEditor ), typeof( System.Drawing.Design.UITypeEditor ) )]
		public string svc_dev_patch_path { get; set; } = "";

		[Category( "2.UMDistribution 툴 사용" )]
		[Description( "QA 서버 로컬 경로" )]
		[Editor( typeof( System.Windows.Forms.Design.FolderNameEditor ), typeof( System.Drawing.Design.UITypeEditor ) )]
		public string svc_qa_path { get; set; } = "";

		[Category( "2.UMDistribution 툴 사용" )]
		[Description( "QA 패치 로컬 경로" )]
		[Editor( typeof( System.Windows.Forms.Design.FolderNameEditor ), typeof( System.Drawing.Design.UITypeEditor ) )]
		public string svc_qa_patch_path { get; set; } = "";

		[Category( "2.UMDistribution 툴 사용" )]
		[Description( "QA2 서버 로컬 경로" )]
		[Editor( typeof( System.Windows.Forms.Design.FolderNameEditor ), typeof( System.Drawing.Design.UITypeEditor ) )]
		public string svc_qa2_path { get; set; } = "";

		[Category( "2.UMDistribution 툴 사용" )]
		[Description( "QA2 패치 로컬 경로" )]
		[Editor( typeof( System.Windows.Forms.Design.FolderNameEditor ), typeof( System.Drawing.Design.UITypeEditor ) )]
		public string svc_qa2_patch_path { get; set; } = "";

		[Category( "2.UMDistribution 툴 사용" )]
		[Description( "검수 서버 로컬 경로" )]
		[Editor( typeof( System.Windows.Forms.Design.FolderNameEditor ), typeof( System.Drawing.Design.UITypeEditor ) )]
		public string svc_review_path { get; set; } = "";

		[Category( "2.UMDistribution 툴 사용" )]
		[Description( "라이브 서버 로컬 경로" )]
		[Editor( typeof( System.Windows.Forms.Design.FolderNameEditor ), typeof( System.Drawing.Design.UITypeEditor ) )]
		public string svc_live_path { get; set; } = "";

		[Category( "2.UMDistribution 툴 사용" )]
		[Description( "라이브 패치 로컬 경로" )]
		[Editor( typeof( System.Windows.Forms.Design.FolderNameEditor ), typeof( System.Drawing.Design.UITypeEditor ) )]
		public string svc_live_patch_path { get; set; } = "";

		[Category( "3.UMExporter 툴 사용" )]
		[Description( "Text Export시 Trim 사용 안함" )]
		[Editor( typeof( System.Windows.Forms.Design.FolderNameEditor ), typeof( System.Drawing.Design.UITypeEditor ) )]
		public bool export_text_trim_ignore { get; set; } = true;

		[Category( "4.UMLauncher 툴 사용" )]
		[Description( "클라이언트 실행 파일명(확장자포함)" )]
		public string launcher_client_exec_file { get; set; } = "";

		[Category( "4.UMLauncher 툴 사용" )]
		[Description( "서버 실행 파일명(확장자포함, 콤마로여러개)" )]
		public string launcher_server_exec_files { get; set; } = "";

		[Category( "4.UMLauncher 툴 사용" )]
		[Description( "다운로드 URL" )]
		public string launcher_download_base_url { get; set; } = "";

		[Category( "4.UMLauncher 툴 사용" )]
		[Description( "클라이언트 버전 파일" )]
		public string launcher_url_client_ver_file { get; set; } = "";

		[Category( "4.UMLauncher 툴 사용" )]
		[Description( "서버 버전 파일" )]
		public string launcher_url_server_ver_file { get; set; } = "";

		[Category( "4.UMLauncher 툴 사용" )]
		[Description( "클라이언트 Company Name(로그경로를위함)" )]
		public string launcher_client_company_name { get; set; } = "";

		[Category( "4.UMLauncher 툴 사용" )]
		[Description( "이전 버전 삭제 차이" )]
		public int launcher_delete_prev_version_distance { get; set; } = 2;

		[Category( "4.UMLauncher 툴 사용" )]
		[Description( "업데이트체크 주기(ms)" )]
		public int launcher_update_check_time_interval { get; set; } = 600000;

	}

	// global config
	[TypeConverter(typeof(PropertySorter))]
	public class GlobalPropertyConfig
	{
		public static GlobalPropertyConfig Load()
		{
			GlobalPropertyConfig config = null;
			string json_file = Path.Combine( "_config", $"__global_config.json" );
			if( File.Exists( json_file ) )
			{
				config = JsonUtil.DecodeJsonFile<GlobalPropertyConfig>( json_file );
			}

			if( config == null )
				config = new GlobalPropertyConfig();

			return config;
		}
		public static void Save( GlobalPropertyConfig config )
		{
			if( config == null )
				return;

			string json_file = Path.Combine( "_config", $"__global_config.json" );
			JsonUtil.EncodeJsonFile( config, json_file );
		}

		[Category( "0.Application" )]
		[Description( "svn.exe 경로" )]
		[Editor( typeof( System.Windows.Forms.Design.FileNameEditor ), typeof( System.Drawing.Design.UITypeEditor ) )]
		public string svn_path { get; set; } = "C:\\Program Files\\TortoiseSVN\\bin\\svn.exe";
		[Category( "0.Application" )]
		[Description( "TortoiseProc.exe 경로" )]
		[Editor( typeof( System.Windows.Forms.Design.FileNameEditor ), typeof( System.Drawing.Design.UITypeEditor ) )]
		public string tsvn_path { get; set; } = "C:\\Program Files\\TortoiseSVN\\bin\\TortoiseProc.exe";
		[Category( "0.Application" )]
		[Description( "merge tool exe 경로" )]
		[Editor( typeof( System.Windows.Forms.Design.FileNameEditor ), typeof( System.Drawing.Design.UITypeEditor ) )]
		public string mergetool_path { get; set; } = "C:\\Program Files\\WinMerge\\WinMergeU.exe";

		[Category( "1.UMTools" )]
		[Description( "UM 통합 툴 작업 로컬 경로" )]
		[Editor( typeof( System.Windows.Forms.Design.FolderNameEditor ), typeof( System.Drawing.Design.UITypeEditor ) )]
		public string tool_work_path { get; set; } = "";

		[Category( "1.UMTools" )]
		[Description( "UM 통합 툴 배포 로컬 경로" )]
		[Editor( typeof( System.Windows.Forms.Design.FolderNameEditor ), typeof( System.Drawing.Design.UITypeEditor ) )]
		public string tool_dist_path { get; set; } = "";

		[Category( "2.UMF" )]
		[Description( "UMF 작업 로컬 경로" )]
		[Editor( typeof( System.Windows.Forms.Design.FolderNameEditor ), typeof( System.Drawing.Design.UITypeEditor ) )]
		public string umf_work_path { get; set; } = "";

		[Category( "2.UMF" )]
		[Description( "UMF 배포 로컬 경로" )]
		[Editor( typeof( System.Windows.Forms.Design.FolderNameEditor ), typeof( System.Drawing.Design.UITypeEditor ) )]
		public string umf_dist_path { get; set; } = "";

	}
}
