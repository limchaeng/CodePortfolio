//////////////////////////////////////////////////////////////////////////
//
// GlobalConfig
// 
// Created by LCY.
//
// Copyright 2025 FN
// All rights reserved
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
//////////////////////////////////////////////////////////////////////////

using System.IO;

namespace UMF.Core
{
	public static class GlobalConfig
	{
		/*
			#####################
			# GLOBAL_CONFIG.property
			#####################

			# 서비스 분기 : NE/CN/KR 등... : 필수
			GlobalType = 

			# 환경에 따른 config 경로 분기 : QA/검수/CBT/라이브등. : 파일이 없으면 기본 경로 사용
			EnvironmentType = 

			# config 루트 경로
			CONFIG_ROOT = _Config

			# 애플리케이션 환경 config 경로
			# ex) C:/Project/CONFIG_ROOT/GlobalType/[EnvironmentType]/ENV_CONFIG_PATH/config.file
			ENV_CONFIG_PATH = _env_server_config			

			# 데이터베이스 config 경로
			# ex) C:/Project/CONFIG_ROOT/GlobalType/[EnvironmentType]/ENV_DB_PATH/config.file
			ENV_DB_PATH = _env_db_config

			# 네트워크 config 경로
			# ex) C:/Project/CONFIG_ROOT/GlobalType/[EnvironmentType]/ENV_NET_PATH/config.file
			ENV_NET_PATH = _env_net_config

			# 테이블/config 데이터 경로
			# ex) C:/Project/CONFIG_ROOT/GlobalType/[EnvironmentType]/DATA_PATH/config.file
			DATA_PATH = _server_config

			# 로컬라이징 텍스트 경로
			# ex) C:/Project/CONFIG_ROOT/GlobalType/[EnvironmentType]/I18N_PATH/config.file
			I18N_PATH = _I18N

		*/

		public static string PROPERTY_FILE = "../GLOBAL.config";
		public static bool IGNORE_LOAD_EXCEPTION = true;

		static EnvironmentProperty mProperty = null;
		public static EnvironmentProperty Property
		{
			get
			{
				if( mProperty == null )
				{
					mProperty = new EnvironmentProperty();
					mProperty.LoadPropertyFile( PROPERTY_FILE, IGNORE_LOAD_EXCEPTION );

					Reset();
				}

				return mProperty;
			}
		}

		public const string EnvironmentType_KEY = "EnvironmentType";
		public const string GlobalType_KEY = "GlobalType";

		public const string CONFIG_ROOT_KEY = "CONFIG_ROOT";
		public const string ENV_CONFIG_PATH_KEY = "ENV_CONFIG_PATH";
		public const string ENV_DB_PATH_KEY = "ENV_DB_PATH";
		public const string ENV_NET_PATH_KEY = "ENV_NET_PATH";
		public const string DATA_PATH_KEY = "DATA_PATH";
		public const string I18N_PATH_KEY = "I18N_PATH";

		static string _GlobalType = "";
		static string _EnvironmentType = "";

		static string CONFIG_ROOT = "";
		static string ENV_CONFIG_PATH = "";
		static string ENV_DB_PATH = "";
		static string ENV_NET_PATH = "";
		static string DATA_PATH = "";
		static string I18N_PATH = "";

		//------------------------------------------------------------------------
		static void Load()
		{
			if( mProperty != null )
				return;

			EnvironmentProperty p = Property;
		}

		//------------------------------------------------------------------------
		public static void Reset()
		{
			_GlobalType = Property.GetEnvironmentValue( GlobalType_KEY );
			_EnvironmentType = Property.GetEnvironmentValue( EnvironmentType_KEY );

			CONFIG_ROOT = Property.GetEnvironmentValue( CONFIG_ROOT_KEY );
			ENV_CONFIG_PATH = Property.GetEnvironmentValue( ENV_CONFIG_PATH_KEY );
			ENV_DB_PATH = Property.GetEnvironmentValue( ENV_DB_PATH_KEY );
			ENV_NET_PATH = Property.GetEnvironmentValue( ENV_NET_PATH_KEY );
			DATA_PATH = Property.GetEnvironmentValue( DATA_PATH_KEY );
			I18N_PATH = Property.GetEnvironmentValue( I18N_PATH_KEY );
		}

		//------------------------------------------------------------------------
		// c:/project/_Config/[GlobalType]/[PATH]/[EnvironmentType]/[file.xml]
		static string GlobalPath( string sub_path, string file )
		{
			string global_path = "";
			if( string.IsNullOrEmpty( GlobalType ) == false )
				global_path = GlobalType;

			string path = Path.Combine( global_path, sub_path );
			if( string.IsNullOrEmpty( file ) == false )
			{
				path = Path.Combine( global_path, sub_path, EnvironmentType, file );
				if( File.Exists( path ) == false )
					path = Path.Combine( global_path, sub_path, file );
			}

			if( string.IsNullOrEmpty( CONFIG_ROOT ) == false )
				path = Path.Combine( CONFIG_ROOT, path );

			return path;
		}
		public static string EnvConfigPath( string env_file )
		{
			Load();
			return GlobalPath( ENV_CONFIG_PATH, env_file );
		}
		public static string EnvDBPath( string env_file )
		{
			Load();
			return GlobalPath( ENV_DB_PATH, env_file );
		}
		public static string EnvNetPath( string env_file )
		{
			Load();
			return GlobalPath( ENV_NET_PATH, env_file );
		}
		public static string DataPath( string _config_file )
		{
			Load();
			return GlobalPath( DATA_PATH, _config_file );
		}
		public static string I18NPath( string _i18n_file )
		{
			Load();
			return GlobalPath( I18N_PATH, _i18n_file );
		}

		//------------------------------------------------------------------------		
		public static string EnvironmentType
		{
			get
			{
				Load();
				return _EnvironmentType;
			}
		}

		//------------------------------------------------------------------------		
		public static string GlobalType
		{
			get
			{
				Load();
				return _GlobalType;
			}
		}
	}
}
