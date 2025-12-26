//////////////////////////////////////////////////////////////////////////
//
// AppVerifyModuleLoginServer
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

using UMF.Core.Module;
using UMP.Module.AppVerifyModule.Master;
using UMP.Server;

namespace UMP.Module.AppVerifyModule
{
	public class AppVerifyModuleLoginServer : ModuleCoreBase
	{
		public override string ModuleName => AppVerifyModuleCommon.MODULE_NAME;

		AppVerifyModuleMasterConnector mMasterModule = null;
		public AppVerifyModuleMasterConnector MasterModule { get { return mMasterModule; } }

		public AppVerifyModuleLoginServer( UMPServerApplication applicationn )
		{
			AppVerifyModuleConfig.MakeInstance();

			if( applicationn.MasterConnector != null )
				mMasterModule = new AppVerifyModuleMasterConnector( applicationn.MasterConnector );			
		}
	}
}

#endif