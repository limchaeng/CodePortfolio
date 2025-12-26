//#define UMF_XBUILD
//////////////////////////////////////////////////////////////////////////
//
// UMFProjectBuilder
// 
// Created by LCY.
//
// Copyright 2025 FN
// All rights reserved
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
// - custom args : -CustomArgs:a=1,b=BB,c=CC12,...
//
// PRE DEFINE
// - UMF_XBUILD
// - UNITY_LOG_ENABLE : Debug.Log() enable
//////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using UMF.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace UMF.Unity.EditorUtil
{
	//------------------------------------------------------------------------	
	public class UMFBuildData
	{
		// builder set
		public BuildTarget build_target = BuildTarget.NoTarget;
		public BuildTargetGroup target_group = BuildTargetGroup.Unknown;
		public string bundle_id = "";
		public int xbuild_num = 0;
		public int revision = 0;
		public bool target_check = false;
		public string output_path = "";
		public string environment_filename = "env.property";
		public string build_path = "";
		public string build_target_file_path = "";
		public string buildreport_file_suffix = "";
		public bool is_forced_qa2dev = false;

		// build menu set
		public string set_app_filename = "";
		public string set_gameversion_string = "";		

		public System.Action<UMFBuildData> handler_pre_verify = null;
		public System.Action<UMFBuildData> handler_post_verify = null;
		public System.Action<UMFBuildData> handler_build_report_tool_setting = null;
		public System.Action<UMFBuildData> handler_build_report_tool_generate = null;

		// for build environment
		public System.Action<UMFBuildData> handler_pre_environment_setting = null;		
		public Dictionary<string, string> set_environment_dic = new Dictionary<string, string>();
	}

	//------------------------------------------------------------------------	
	public static class UMFProjectBuilder
	{
		public const string AOS_BUILD_PATH = "AndroidBuild";
		public const string IOS_BUILD_PATH = "iOSBuild";
		public const string IOS_XCODE_PATH = "XCode";
		public const string WIN_BUILD_PATH = "WinBuild";
		public const string WSA_BUILD_PATH = "WSABuild";

		public const string CUSTOM_ARGS_BUNDLEID = "bundleid";
		public const string CUSTOM_ARGS_DEFINE = "define";
		public const string CUSTOM_ARGS_UNDEFINE = "undefine";
		public const string CUSTOM_ARGS_FILENAME_SUFFIX = "filesuffix";
		public const string CUSTOM_ARGS_IL2CPP = "il2cpp";
		public const string CUSTOM_ARGS_BUILDNUM = "buildnum";
		public const string CUSTOM_ARGS_REVISION = "revision";
		public const string CUSTOM_ARGS_OUTPUTPATH = "output";
		public const string CUSTOM_ARGS_BUILDOPTIONS = "buildoptions";  // | separate
		public const string CUSTOM_ARGS_FORCED_DEV = "forceddev";	// QA 빌드인데 강제로 특정 dev 환경이 필요한 경우 사용

		public const string CUSTOM_ARGS_PREVERIFY = "preverify";
		public const string CUSTOM_ARGS_POSTVERIFY = "postverify";
		public const string CUSTOM_ARGS_SVNCONFLICT = "svnconflict";   // 1:check 2:check and delete(default)

		// property
		public static bool THIS_IS_XBUILD = false;

		public static UMFAppBuildInfo AppBuildInfo { get; set; } = null;

		//------------------------------------------------------------------------	
		public static void PerformCommandDefine( List<string> default_defines )
		{
			THIS_IS_XBUILD = true;

			eSVN_ConfilctCheckType svn_conflict_check = StringUtil.SafeParse<eSVN_ConfilctCheckType>( UMFCommandLineReader.GetCustomArgument( CUSTOM_ARGS_SVNCONFLICT ), eSVN_ConfilctCheckType.CheckAndDelete );
			CheckSVNConflictFiles( svn_conflict_check );

			List<string> define_list = new List<string>();
			if( default_defines != null )
				define_list.AddRange( default_defines );

			List<string> custom_defines = StringUtil.SafeParseToList<string>( UMFCommandLineReader.GetCustomArgument( CUSTOM_ARGS_DEFINE ), ';' );
			if( custom_defines != null )
			{
				define_list.AddRange( custom_defines );
			}

			string undefine_log = "";
			List<string> undefine_list = StringUtil.SafeParseToList<string>( UMFCommandLineReader.GetCustomArgument( CUSTOM_ARGS_UNDEFINE ), ';' );
			if( undefine_list != null )
			{
				foreach( string undefine in undefine_list )
				{
					define_list.Remove( undefine );
					undefine_log += $"{undefine};";
				}
			}

			string log = "";
			foreach( string define in define_list )
				log += $"{define};";

			BuildTargetGroup target_group = BuildPipeline.GetBuildTargetGroup( EditorUserBuildSettings.activeBuildTarget );
			Debug.Log( $"SetScriptingDefineSymbols target:{target_group} define:{log} undefine:{undefine_log}" );
#if UNITY_6000_0_OR_NEWER
			PlayerSettings.SetScriptingDefineSymbols( NamedBuildTarget.FromBuildTargetGroup( target_group ), define_list.ToArray() );
#else
			PlayerSettings.SetScriptingDefineSymbolsForGroup( target_group, define_list.ToArray() );
#endif
		}

		//------------------------------------------------------------------------		
		public static void PerformCommandBuild( UMFBuildData build_data, System.Action build_setting_handler )
		{
			THIS_IS_XBUILD = true;

			// check
			if( UMFCommandLineReader.GetCustomArguments().Count <= 0 )
				throw new System.Exception( "BuildPlayer failure: CustomArgs is Empty!" );

			build_data.build_target = EditorUserBuildSettings.activeBuildTarget;
			build_data.target_group = BuildPipeline.GetBuildTargetGroup( build_data.build_target );
			build_data.bundle_id = UMFCommandLineReader.GetCustomArgument( CUSTOM_ARGS_BUNDLEID );
			if( string.IsNullOrEmpty( build_data.bundle_id ) )
				throw new System.Exception( "BuildPlayer failure: Invalid bundle id!" );

			// option
			BuildOptions buildoptions = BuildOptions.None;
			buildoptions |= StringUtil.SafeParse<BuildOptions>( UMFCommandLineReader.GetCustomArgument( CUSTOM_ARGS_BUILDOPTIONS ), BuildOptions.None );
			Debug.Log( $"BuildOptions = {buildoptions}" );

			build_data.xbuild_num = StringUtil.SafeParse<int>( UMFCommandLineReader.GetCustomArgument( CUSTOM_ARGS_BUILDNUM ), 0 );
			build_data.revision = StringUtil.SafeParse<int>( UMFCommandLineReader.GetCustomArgument( CUSTOM_ARGS_REVISION ), 0 );
			build_data.target_check = false;
			build_data.is_forced_qa2dev = StringUtil.SafeParse<bool>( UMFCommandLineReader.GetCustomArgument( CUSTOM_ARGS_FORCED_DEV ), false );

			// output path
			build_data.output_path = StringUtil.SafeParse<string>( UMFCommandLineReader.GetCustomArgument( CUSTOM_ARGS_OUTPUTPATH ), "" );
			if( string.IsNullOrEmpty( build_data.output_path ) )
				build_data.output_path = Path.GetFullPath( ".." );

			// file name
			string arg_file_suffix = UMFCommandLineReader.GetCustomArgument( CUSTOM_ARGS_FILENAME_SUFFIX );
			string file_name_suffix = ( string.IsNullOrEmpty( arg_file_suffix ) == false ? "_" + arg_file_suffix : "" );

			build_setting_handler.Invoke();

			// il2cpp
			bool il2cpp = StringUtil.SafeParse<bool>( UMFCommandLineReader.GetCustomArgument( CUSTOM_ARGS_IL2CPP ), true );

			string build_dir_path = "";
			switch( build_data.build_target )
			{
				case BuildTarget.StandaloneWindows:
				case BuildTarget.StandaloneWindows64:
					{
#if UNITY_6000_0_OR_NEWER
						if( il2cpp )
						{
							PlayerSettings.SetScriptingBackend( NamedBuildTarget.FromBuildTargetGroup( build_data.target_group ), ScriptingImplementation.IL2CPP );
						}
						else
						{
							PlayerSettings.SetScriptingBackend( NamedBuildTarget.FromBuildTargetGroup( build_data.target_group ), ScriptingImplementation.Mono2x );
						}
#else
						if( il2cpp )
						{
							PlayerSettings.SetScriptingBackend( build_data.target_group, ScriptingImplementation.IL2CPP );
						}
						else
						{
							PlayerSettings.SetScriptingBackend( build_data.target_group, ScriptingImplementation.Mono2x );
						}
#endif

						build_data.build_path = $"{build_data.output_path}/{WIN_BUILD_PATH}";
						build_dir_path = $"{build_data.build_path}/{build_data.set_app_filename}";
						CreateDirectory( build_dir_path );
						build_data.build_target_file_path = $"{build_dir_path}/{build_data.set_app_filename}.exe";

						build_data.buildreport_file_suffix = $"BRT_{build_data.set_app_filename}_{PlayerSettings.bundleVersion}{file_name_suffix}_";
					}
					break;

				case BuildTarget.WSAPlayer:
					{
#if UNITY_6000_0_OR_NEWER
						PlayerSettings.SetScriptingBackend( NamedBuildTarget.FromBuildTargetGroup( build_data.target_group ), ScriptingImplementation.IL2CPP );
#else
						PlayerSettings.SetScriptingBackend( build_data.target_group, ScriptingImplementation.IL2CPP );
#endif
						build_data.build_path = $"{build_data.output_path}/{WSA_BUILD_PATH}";
						build_dir_path = $"{build_data.build_path}/{build_data.set_app_filename}";
						CreateDirectory( build_dir_path );
						build_data.build_target_file_path = build_dir_path;

						build_data.buildreport_file_suffix = $"BRT_{build_data.set_app_filename}_{PlayerSettings.bundleVersion}{file_name_suffix}_";
					}
					break;

				case BuildTarget.Android:
				case BuildTarget.iOS:
					break;

				default:
					throw new System.Exception( string.Format( "BuildPlayer failure: Target [{0}] Not support", build_data.build_target ) );
			}

			if( ( buildoptions & BuildOptions.CleanBuildCache) != 0 )
			{
				UMFEditorUtil.DeleteAllFiles( $"{build_dir_path}", true );
			}

			// create environment variable for Jenkins
			CreateEnvironmentVariable( build_data.output_path, build_data );

			build_data.handler_build_report_tool_setting?.Invoke( build_data );

			bool pre_verify = StringUtil.SafeParse<bool>( UMFCommandLineReader.GetCustomArgument( CUSTOM_ARGS_PREVERIFY ), true );
			if( pre_verify )
				build_data.handler_pre_verify?.Invoke( build_data );

			GenericBuild( build_data.build_target_file_path, build_data.build_target, buildoptions );

			build_data.handler_build_report_tool_generate?.Invoke( build_data );

			bool post_verify = StringUtil.SafeParse<bool>( UMFCommandLineReader.GetCustomArgument( CUSTOM_ARGS_POSTVERIFY ), true );
			if( post_verify )
				build_data.handler_post_verify?.Invoke( build_data );
		}

		//------------------------------------------------------------------------		
		static void GenericBuild( string target_file_path, BuildTarget build_target, BuildOptions build_options )
		{
			List<string> editor_scene_list = new List<string>();
			foreach( EditorBuildSettingsScene scene in EditorBuildSettings.scenes )
			{
				if( scene.enabled == false )
					continue;

				editor_scene_list.Add( scene.path );
			}

			string[] scenes = editor_scene_list.ToArray();

			foreach( string s in scenes )
				Debug.Log( string.Format( "## GenericBuild scene:{0} name:{1} target:{2} option:{3}", s, target_file_path, build_target, build_options ) );

			BuildReport build_report = BuildPipeline.BuildPlayer( scenes, target_file_path, build_target, build_options );
 			if( build_report.summary.result != BuildResult.Succeeded )
 				throw new System.Exception( "BuildPlayer failure" + build_report.summary.result.ToString() );
		}

		//////////////////////////////////////////////////////////////////////////
		///
		//------------------------------------------------------------------------
		public enum eSVN_ConfilctCheckType
		{
			None = 0,
			CheckOnly = 1,
			CheckAndDelete = 2,
		}
		public static void CheckSVNConflictFiles( eSVN_ConfilctCheckType svn_conflict_check )
		{
			Debug.LogFormat( "Check SVN Conflict Files : {0}", svn_conflict_check );
			if( svn_conflict_check == eSVN_ConfilctCheckType.None )
				return;

			string path = Application.dataPath;
			string[] file_list = Directory.GetFiles( path, "*.r*", SearchOption.AllDirectories );

			bool has_conflict = false;
			foreach( string file in file_list )
			{
				string ext = Path.GetExtension( file );
				if( ext == ".meta" )
					continue;

				int revision = StringUtil.SafeParse<int>( ext.Replace( ".r", "" ), 0 );
				if( revision == 0 )
					continue;

				Debug.LogFormat( "- Find Conflict File :{0}", file );

				has_conflict = true;
				string org_file = file.Replace( ext, "" );
				if( File.Exists( org_file ) )
				{
					if( svn_conflict_check == eSVN_ConfilctCheckType.CheckAndDelete )
					{
						File.Delete( org_file );
						Debug.LogFormat( "- Delete Org File :{0}", org_file );
					}
				}

				string mine_file = file.Replace( ext, ".mine" );
				if( File.Exists( mine_file ) )
				{
					if( svn_conflict_check == eSVN_ConfilctCheckType.CheckAndDelete )
					{
						File.Delete( mine_file );
						Debug.LogFormat( "- Delete mine File :{0}", mine_file );
					}
				}

				if( svn_conflict_check == eSVN_ConfilctCheckType.CheckAndDelete )
					File.Delete( file );
			}

			if( has_conflict )
				throw new System.Exception( "- SVN Conflict File Detected!" );
		}

		//------------------------------------------------------------------------
		static bool CreateDirectory( string path )
		{
			try
			{
				if( Directory.Exists( path ) )
					return false;

				Directory.CreateDirectory( path );
				return true;
			}
			catch( System.Exception ex )
			{
				throw ex;
			}
		}

		//------------------------------------------------------------------------	
		static void CreateEnvironmentVariable( string output_path, UMFBuildData build_data )
		{
			CreateDirectory( output_path );

			Debug.Log( string.Format( "## CreateEnvironmentVariable path:{0} bundle:{1}", output_path, build_data.bundle_id ) );

			build_data.handler_pre_environment_setting?.Invoke( build_data );

			if( build_data.set_environment_dic.Count <= 0 )
				return;

			StringBuilder envFile = new StringBuilder();
			envFile.AppendLine( "#BUILD ENVIRONMENT VARIABLE" );

			foreach( var kvp in build_data.set_environment_dic )
			{
				envFile.AppendLine( $"{kvp.Key}={kvp.Value}" );
			}

			File.WriteAllText( $"{output_path}/{build_data.environment_filename}", envFile.ToString(), System.Text.Encoding.UTF8 );
		}
	}

#if UMF_XBUILD
	//------------------------------------------------------------------------
	public sealed class UMFPostprocessBuild : IPostprocessBuildWithReport
	{
		public int callbackOrder => 0;

		public void OnPostprocessBuild( BuildReport report )
		{
			BuildSummary summary = report.summary;

			string output_path = summary.outputPath;
			string output_dir_path = Path.GetDirectoryName( output_path );
			string output_filename = Path.GetFileNameWithoutExtension( output_path );

			Debug.Log( $"## UMFPostprocessBuild : output={output_path} dir={output_dir_path} file={output_filename}" );

			DeletePostFiles( report, output_path, output_dir_path, output_filename );
			DoNotShipBackup( report, output_path, output_dir_path, output_filename );
			UWP_FIX( report, output_path, output_dir_path, output_filename );
		}

		//------------------------------------------------------------------------
		void DeletePostFiles( BuildReport report, string output_path, string output_dir_path, string output_filename )
		{
			Debug.Log( $"DeletePostFiles" );
			UMFAppBuildInfo app_build_info = UMFProjectBuilder.AppBuildInfo;
			if( app_build_info == null )
			{
				Debug.Log( $"- AppBuildInfo is null." );
				return;
			}

			if( app_build_info.DeletePostFileList.Count > 0 )
			{
				foreach( string del_path in app_build_info.DeletePostFileList )
				{
					string real_file_path = Path.Combine( output_dir_path, del_path );
					if( File.Exists( real_file_path ) )
					{
						File.Delete( real_file_path );
						Debug.Log( $"- DeletePostFile : {real_file_path}" );
					}
				}
			}
		}

		//------------------------------------------------------------------------
		private bool IncludeAttributeEndsWith( XElement element, string contents )
		{
			var attr = element.Attribute( "Include" );
			if( attr == null ) return false;
			return attr.Value.EndsWith( contents );
		}

		void UWP_FIX( BuildReport report, string output_path, string output_dir_path, string output_filename )
		{
			if( report.summary.platform != BuildTarget.WSAPlayer )
				return;
			
			{
				string add_display_name = "";
				UMFAppBuildInfo app_build_info = UMFProjectBuilder.AppBuildInfo;
				if( app_build_info != null )
					add_display_name = app_build_info.UWP_DisplayNameAdded;

				string appxmanifest_path = Directory.GetFileSystemEntries( output_dir_path, "Package.appxmanifest", SearchOption.AllDirectories ).FirstOrDefault();
				Debug.Log( $"UWP_FIX : DisplayName Change : {appxmanifest_path} - {add_display_name}" );
				if( File.Exists( appxmanifest_path ) && string.IsNullOrEmpty( add_display_name ) == false )
				{
					Debug.Log( $"- found : {appxmanifest_path}" );
					try
					{
						XmlDocument doc = new XmlDocument();
						doc.Load( appxmanifest_path );

						XmlNamespaceManager ns = new XmlNamespaceManager( doc.NameTable );
						ns.AddNamespace( "ns", "http://schemas.microsoft.com/appx/manifest/foundation/windows10" );

						bool is_dirty = false;
						XmlNode root = doc.SelectSingleNode( "//ns:Package", ns );
						if( root != null )
						{
							XmlNode prop_node = root.SelectSingleNode( "//ns:Properties", ns );
							if( prop_node != null )
							{
								XmlNode display_name_node = prop_node.SelectSingleNode( "//ns:DisplayName", ns );
								if( display_name_node != null )
								{
									string old_text = display_name_node.InnerText;
									display_name_node.InnerText += add_display_name;
									is_dirty = true;

									Debug.Log( $"- DisplayName {old_text} -> {display_name_node.InnerText}" );
								}
							}
						}

						if( is_dirty )
							doc.Save( appxmanifest_path );
					}
					catch( System.Exception ex )
					{
						Debug.LogWarning( ex.ToString() );
					}
				}
			}
			
			{
				string vcx_item_file = Directory.GetFileSystemEntries( output_dir_path, "Unity Data.vcxitems", SearchOption.AllDirectories ).FirstOrDefault();
				Debug.Log( $"UWP_Fix : VCITEMS Project fix : {vcx_item_file}" );

				if( string.IsNullOrEmpty( vcx_item_file ) == false )
				{
					Debug.Log( $"- found : {vcx_item_file}" );
					XDocument xdoc = XDocument.Load( vcx_item_file );
					XElement item_group = xdoc.Descendants().FirstOrDefault( node => node.Name.LocalName == "ItemGroup" );
					if( item_group != null )
					{
						List<XElement> delete_items = item_group.Descendants().Where( node => node.Name.LocalName == "None" && ( IncludeAttributeEndsWith( node, "il2cppFileRoot.txt" ) || IncludeAttributeEndsWith( node, "LineNumberMappings.json" ) ) ).ToList();
						if( delete_items != null )
						{
							foreach( XElement item in delete_items )
							{
								item.Remove();
							}

							xdoc.Save( vcx_item_file );
						}
					}
				}
			}
		}

		//------------------------------------------------------------------------
		void DoNotShipBackup( BuildReport report, string output_path, string output_dir_path, string output_filename )
		{
			Debug.Log( $"DoNotShipBackup" );
			UMFAppBuildInfo app_build_info = UMFProjectBuilder.AppBuildInfo;
			if( app_build_info == null )
			{
				Debug.Log( $"- AppBuildInfo is null." );
				return;
			}

			string PROJ_NAME_PARSE = "[#appname]";
			string BACKUP_PATH = "_DoNotShipBackup";

			string backup_path = $"{output_dir_path}/../{BACKUP_PATH}";

			if( Directory.Exists( backup_path ) )
				Directory.Delete( backup_path, true );

			Directory.CreateDirectory( backup_path );

			List<string> move_folders = new List<string>();
			string[] sub_folder_list = Directory.GetDirectories( output_dir_path );

			if( app_build_info.DoNotShipList != null )
			{
				foreach( UMFAppBuildInfo.DoNotShipInfo info in app_build_info.DoNotShipList )
				{
					if( info.IsFolder == false )
					{
						Debug.Log( $"- DoNotShipBackup : file not support currently." );
						continue;
					}
					else
					{
						string check_folder = info.Path.Replace( PROJ_NAME_PARSE, output_filename );
						if( sub_folder_list != null )
						{
							foreach( string sub in sub_folder_list )
							{
								string dir_name = Path.GetFileName( sub );
								if( dir_name == check_folder )
								{
									move_folders.Add( sub.NormalizeSlashPath() );
								}
							}
						}
					}
				}
			}

			if( move_folders.Count > 0 )
			{
				foreach( string move_path in move_folders )
				{
					string path_name = Path.GetFileName( move_path );
					string dest_path = $"{backup_path}/{path_name}";
					Directory.Move( move_path, dest_path );

					Debug.Log( $"- DoNotShipBackup : backup {move_path} -> {dest_path}" );
				}
			}
		}
	}
#endif
}


