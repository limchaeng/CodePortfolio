//////////////////////////////////////////////////////////////////////////
//
// AppVerifyModuleServer
// 
// Created by LCY.
//
// Copyright 2022 FN
// All rights reserved
//
//////////////////////////////////////////////////////////////////////////
// Version 1.0
//
//////////////////////////////////////////////////////////////////////////
#if UMSERVER

using UMF.Core;
using System.Collections.Generic;
using System;
using System.Text;
using UMF.Database;
using UMF.Core.Module;
using UMP.Server;
using UMP.Module.AppVerifyModule.DB;
using UMP.Module.AppVerifyModule.Master;

namespace UMP.Module.AppVerifyModule
{
	public class AppVerifyModuleGameServer : ModuleCoreBase, IExecuteAfterFirstReloadData
	{
		public enum eAppVerifyResult
		{
			Success = 0,
			CheckDB,
			Invalid,
			Warning,
		}

		public override string ModuleName => AppVerifyModuleCommon.MODULE_NAME;

		UMPServerApplication mApplication = null;
		DatabaseMain mDatabase = null;
		AppVerifyModuleMasterConnector mMasterModule = null;		
		public AppVerifyModuleMasterConnector MasterModule { get { return mMasterModule; } }

		List<SP_APP_VERIFY_USE> mVerifyUseList = new List<SP_APP_VERIFY_USE>();
		List<SP_APP_VERIFY_DATA> mVerifyDataList = new List<SP_APP_VERIFY_DATA>();

		DateTime mDBLastGetTime = DateTime.MinValue;

		bool mIsProcessing = false;
		DateTime mProcessingTimeout = DateTime.MinValue;

		AutoRefreshTimePerHour mRefreshTime = new AutoRefreshTimePerHour( 25, 55 );
		public AutoRefreshTimePerHour RefreshTime
		{
			get { mRefreshTime.ResetTimeCallback = OnRefreshTimeResetted; return mRefreshTime; }
		}

		//------------------------------------------------------------------------
		public AppVerifyModuleGameServer( UMPServerApplication applicationn, DatabaseMain database )
		{
			AppVerifyModuleConfig.MakeInstance();

			mApplication = applicationn;
			mApplication.AddUpdater( Update );
			mDatabase = database;

			if( mApplication.MasterConnector != null )
			{
				mMasterModule = new AppVerifyModuleMasterConnector( mApplication.MasterConnector );
			}

			DataReloader.Instance.AddExecuteAfterFirstReload( this );
		}

		//------------------------------------------------------------------------
		// IExecuteAfterFirstReloadData
		public bool IsExecutedAfterFirstReload { get; set; } = false;
		public void ExecuteAfterFirstReload()
		{
			DBProcessGet( true, true );
		}
		// IExecuteAfterFirstReloadData

		//------------------------------------------------------------------------
		void OnRefreshTimeResetted( bool is_new )
		{
			if( is_new && mIsProcessing == false && IsExecutedAfterFirstReload )
				DBProcessGet( false );
		}

		//------------------------------------------------------------------------	
		public string ShowInfo()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine( "== AppVerify INFO ==" );
			foreach( SP_APP_VERIFY_USE use in mVerifyUseList )
			{
				sb.AppendLine( string.Format( "# UseType:{0} Warning:{1} Fixed:{2}", use.verify_type, use.all_warning, use.fixed_valid_verify ) );
			}
			sb.AppendLine( string.Format( "-> Cached Verify:{0}", mVerifyDataList.Count ) );
			foreach( SP_APP_VERIFY_DATA data in mVerifyDataList )
			{
				sb.AppendLine( string.Format( "* Kind:{2} Type:{0} Code:{1}", data.verify_type, data.verify_code, data.validation_kind ) );
			}
			sb.AppendLine( string.Format( "-> RefreshTime:{0}", mRefreshTime.ToString() ) );
			sb.AppendLine( string.Format( "-> DB Last Get Time:{0} ", mDBLastGetTime ) );
			Log.WriteImportant( sb.ToString() );

			return sb.ToString();
		}

		//------------------------------------------------------------------------	
		public void DBProcessGet( bool get_all, bool is_sync = false )
		{
			if( mIsProcessing )
				return;

			mIsProcessing = true;
			mProcessingTimeout = DateTime.Now.AddSeconds( 60 );

			mRefreshTime.ResetNextTime( DateTime.Now );

			List<db_tinystr_list> db_new_unknown_list = new List<db_tinystr_list>();
			List<SP_APP_VERIFY_DATA> new_unknown_list = mVerifyDataList.FindAll( v => v.validation_kind == eSP_APP_VERIFY_ValidationKind.Unknown && v.runtime_unknown_added );
			if( new_unknown_list != null && new_unknown_list.Count > 0 )
			{
				foreach( SP_APP_VERIFY_DATA v_data in new_unknown_list )
				{
					db_tinystr_list db_data = new db_tinystr_list();
					db_data.n = (byte)v_data.verify_type;
					db_data.str = v_data.verify_code;

					db_new_unknown_list.Add( db_data );

					// clear unknown add time
					v_data.runtime_unknown_added = false;
				}
			}

			SP_AppVerifyCode_Get _SP_AppVerifyCode_Get = new SP_AppVerifyCode_Get();
			_SP_AppVerifyCode_Get.world_idn = mApplication.GetApplicationConfig.WorldIDN;
			_SP_AppVerifyCode_Get.all_get = get_all;
			_SP_AppVerifyCode_Get.last_get_time = mDBLastGetTime;
			_SP_AppVerifyCode_Get.app_version = mApplication.ServerVersion.ToString();
			_SP_AppVerifyCode_Get.has_unknown = ( db_new_unknown_list.Count > 0 );
			_SP_AppVerifyCode_Get.new_unknown_list = new tv_tinystr_list( db_new_unknown_list );

			if( get_all )
				mVerifyDataList.Clear();

			if( is_sync )
			{
				SP_AppVerifyCode_Get_ACK ackData = DBHandlerExecute.ExecuteSync<SP_AppVerifyCode_Get, SP_AppVerifyCode_Get_ACK>( _SP_AppVerifyCode_Get, mDatabase );
				ProcessCache( ackData );
			}
			else
			{
				DBHandlerExecute.ExecuteDirectCallback<SP_AppVerifyCode_Get, SP_AppVerifyCode_Get_ACK>( _SP_AppVerifyCode_Get, mDatabase, DBProcessCallbackWorld );
			}
		}

		//------------------------------------------------------------------------	
		void DBProcessCallbackWorld( DBHandlerObject data )
		{
			SP_AppVerifyCode_Get_ACK ackData = (SP_AppVerifyCode_Get_ACK)data.readObject;
			ProcessCache( ackData );
		}

		//------------------------------------------------------------------------	
		void ProcessCache( SP_AppVerifyCode_Get_ACK ack_data )
		{
			mIsProcessing = false;

			mDBLastGetTime = ack_data.last_db_time;

			mVerifyUseList.Clear();
			if( ack_data.used_list != null && ack_data.used_list.Count > 0 )
				mVerifyUseList.AddRange( ack_data.used_list );

			if( ack_data.updated_code_list != null )
			{
				foreach( SP_APP_VERIFY_DATA db_data in ack_data.updated_code_list )
				{
					db_data.runtime_unknown_added = false;

					mVerifyDataList.RemoveAll( v => v.idx == db_data.idx || ( v.validation_kind == eSP_APP_VERIFY_ValidationKind.Unknown && v.verify_type == db_data.verify_type && v.verify_code == db_data.verify_code ) );
					mVerifyDataList.Add( db_data );
				}
			}

			if( ack_data.deleted_list != null )
			{
				foreach( SP_APP_VERIFY_DELETED db_data in ack_data.deleted_list )
					mVerifyDataList.RemoveAll( v => v.idx == db_data.idx );
			}

			// notify to login
			if( mMasterModule != null )
			{
				S2M_NotifyAppVerifyData _S2M_NotifyAppVerifyData = new S2M_NotifyAppVerifyData();
				_S2M_NotifyAppVerifyData.verify_data_list = null;
				eAppVerifyRequestTypeFlag notify_flags = AppVerifyModuleConfig.Instance.NotifyToLogin;

				if( notify_flags != eAppVerifyRequestTypeFlag.None )
				{
					foreach( SP_APP_VERIFY_DATA app_data in mVerifyDataList )
					{
						eAppVerifyRequestTypeFlag type_flag = app_data.ToFlag();
						if( ( notify_flags & type_flag ) != 0 )
						{
							if( _S2M_NotifyAppVerifyData.verify_data_list == null )
								_S2M_NotifyAppVerifyData.verify_data_list = new List<P_NotifyAppVerifyData>();

							P_NotifyAppVerifyData exist = _S2M_NotifyAppVerifyData.verify_data_list.Find( p => p.type_flag == (int)type_flag );
							if( exist == null )
							{
								exist = new P_NotifyAppVerifyData();
								exist.type_flag = (int)type_flag;
								exist.verify_list = new List<string>();

								_S2M_NotifyAppVerifyData.verify_data_list.Add( exist );
							}

							exist.verify_list.Add( app_data.verify_code );
						}
					}
				}

				mMasterModule.SendPacket( _S2M_NotifyAppVerifyData );
			}
		}

		//------------------------------------------------------------------------	
		DateTime _next_check_time = DateTime.MinValue;
		void Update()
		{
			if( IsExecutedAfterFirstReload == false )
				return;

			// processing timeout check
			if( mIsProcessing && mProcessingTimeout < DateTime.Now )
			{
				mIsProcessing = false;
			}

			// auto getting check
			if( mRefreshTime.NextTime < DateTime.Now )
			{
				DBProcessGet( false );
			}
		}

		//------------------------------------------------------------------------	
		SP_APP_VERIFY_DATA AddUnknownData( eSP_APP_VERIFY_TYPE v_type, string value, DateTime add_time )
		{
			SP_APP_VERIFY_DATA unknown_data = new SP_APP_VERIFY_DATA();
			unknown_data.idx = 0;
			unknown_data.verify_type = v_type;
			unknown_data.verify_code = value;
			unknown_data.validation_kind = eSP_APP_VERIFY_ValidationKind.Unknown;
			unknown_data.runtime_unknown_added = true;

			return unknown_data;
		}

		//------------------------------------------------------------------------
		SP_APP_VERIFY_DATA _CheckVerify( SP_APP_VERIFY_USE v_used, string check_data, DateTime server_time, ref string reason, ref int hack_count, ref int warning_count )
		{
			SP_APP_VERIFY_DATA unknown_data = null;
			SP_APP_VERIFY_DATA a_data = mVerifyDataList.Find( v => v.verify_type == v_used.verify_type && v.IsEqual( check_data ) );
			if( a_data == null )
			{
				if( string.IsNullOrEmpty( check_data ) )
				{
					if( v_used.all_warning )
					{
						reason += string.Format( "[W:{0}:Empty]", v_used.verify_type );
						warning_count++;
					}
					else
					{
						reason += string.Format( "[{0}:Empty]", v_used.verify_type );
						hack_count++;
					}
				}
				else if( v_used.fixed_valid_verify )
				{
					if( v_used.all_warning )
					{
						reason += string.Format( "[W:{0}:{1}]", v_used.verify_type, check_data );
						warning_count++;
					}
					else
					{
						reason += string.Format( "[{0}:{1}]", v_used.verify_type, check_data );
						hack_count++;
					}
				}
				else
				{
					if( hack_count <= 0 )
						unknown_data = AddUnknownData( v_used.verify_type, check_data, server_time );
				}
			}
			else
			{
				switch( v_used.verify_type )
				{
					case eSP_APP_VERIFY_TYPE.iOSJailbreak:
					case eSP_APP_VERIFY_TYPE.iOSLibCheck:
						if( v_used.all_warning )
						{
							reason += string.Format( "[WA:{0}:{1}]", v_used.verify_type, check_data );
							warning_count++;
						}
						break;
				}

				switch( a_data.validation_kind )
				{
					case eSP_APP_VERIFY_ValidationKind.IsInvalid:
						if( v_used.all_warning )
						{
							reason += string.Format( "[W:{0}:{1}]", v_used.verify_type, check_data );
							warning_count++;
						}
						else
						{
							reason += string.Format( "[{0}:{1}]", v_used.verify_type, check_data );
							hack_count++;
						}
						break;

					case eSP_APP_VERIFY_ValidationKind.IsWarning:
						reason += string.Format( "[W:{0}:{1}]", v_used.verify_type, check_data );
						warning_count++;
						break;
				}
			}

			return unknown_data;
		}

		//------------------------------------------------------------------------	
		List<SP_APP_VERIFY_DATA> tmp_unknown_datas = new List<SP_APP_VERIFY_DATA>();
		public eAppVerifyResult CheckVerify( CS_AppVerifyData cs_data, eAppVerifyRequestTypeFlag req_flags, ref string reason, ref bool jailbreak )
		{
			if( cs_data == null )
				return eAppVerifyResult.Invalid;

			int hack_count = 0;
			int warning_count = 0;

			DateTime server_time = DateTime.Now;
			tmp_unknown_datas.Clear();

			foreach( SP_APP_VERIFY_USE v_used in mVerifyUseList )
			{
				string[] v4s = null;
				string check_data = "";
				eAppVerifyRequestTypeFlag check_flag = eAppVerifyRequestTypeFlag.None;
				switch( v_used.verify_type )
				{
					case eSP_APP_VERIFY_TYPE.KeySign:
						check_flag = eAppVerifyRequestTypeFlag.KeySign;
						check_data = ( cs_data.v1 == null ? "" : cs_data.v1.Replace( "\n", "\\n" ).ToLower() );
						break;

					case eSP_APP_VERIFY_TYPE.MD5:
						check_flag = eAppVerifyRequestTypeFlag.MD5;
						check_data = ( cs_data.v2 == null ? "" : cs_data.v2.ToLower() );
						break;

					case eSP_APP_VERIFY_TYPE.CRC:
						check_flag = eAppVerifyRequestTypeFlag.CRC;
						check_data = ( cs_data.v3 == null ? "" : cs_data.v3.ToLower() );
						break;

					case eSP_APP_VERIFY_TYPE.HackFile:
						check_flag = eAppVerifyRequestTypeFlag.HackFile;
						if( string.IsNullOrEmpty( cs_data.v4 ) == false )
							v4s = cs_data.v4.ToLower().Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
						break;

					case eSP_APP_VERIFY_TYPE.Installer:
						check_flag = eAppVerifyRequestTypeFlag.Installer;
						check_data = ( cs_data.v5 == null ? "" : cs_data.v5.ToLower() );
						break;

					case eSP_APP_VERIFY_TYPE.iOSJailbreak:
						check_flag = eAppVerifyRequestTypeFlag.IOSJB;
						check_data = cs_data.v1;

						if( string.IsNullOrEmpty( check_data ) || check_data == "iosjb" )   // ignore check
							continue;

						jailbreak = true;
						break;

					case eSP_APP_VERIFY_TYPE.iOSLibCheck:
						check_flag = eAppVerifyRequestTypeFlag.IOSLIB;
						check_data = cs_data.v2;

						if( string.IsNullOrEmpty( check_data ) || check_data == "ioslib" )   // ignore check
							continue;

						jailbreak = true;
						break;
				}

				if( check_flag == eAppVerifyRequestTypeFlag.None || ( ( req_flags & check_flag ) == 0 ) )
					continue;

				if( v_used.verify_type == eSP_APP_VERIFY_TYPE.HackFile )
				{
					if( v4s == null || v4s.Length <= 0 )
					{
						if( v_used.all_warning )
						{
							reason += string.Format( "[W:{0}:Empty]", v_used.verify_type );
							warning_count++;
						}
						else
						{
							reason += string.Format( "[{0}:Empty]", v_used.verify_type );
							hack_count++;
						}
					}
					else
					{
						foreach( string v4 in v4s )
						{
							SP_APP_VERIFY_DATA unknown_data = _CheckVerify( v_used, v4, server_time, ref reason, ref hack_count, ref warning_count );
							if( unknown_data != null )
								tmp_unknown_datas.Add( unknown_data );

						}
					}
				}
				else
				{
					SP_APP_VERIFY_DATA unknown_data = _CheckVerify( v_used, check_data, server_time, ref reason, ref hack_count, ref warning_count );
					if( unknown_data != null )
						tmp_unknown_datas.Add( unknown_data );
				}
			}

			if( tmp_unknown_datas.Count > 0 )
			{
				foreach( SP_APP_VERIFY_DATA unknown_data in tmp_unknown_datas )
				{
					mVerifyDataList.Add( unknown_data );
					if( hack_count <= 0 )
						mApplication.SendServerNotification( string.Format( "[AppVerifyUnknown][{0}:{1}]", unknown_data.verify_type, unknown_data.verify_code ) );
				}
			}
			tmp_unknown_datas.Clear();

			if( hack_count > 0 )
				return eAppVerifyResult.Invalid;

			if( warning_count > 0 )
				return eAppVerifyResult.Warning;

			return eAppVerifyResult.Success;
		}
	}
}

#endif