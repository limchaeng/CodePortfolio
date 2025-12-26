//////////////////////////////////////////////////////////////////////////
//
// GlobalEnv
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

using System;
using UMF.Core;

namespace UMP.Server
{
	public class GlobalEnv
	{
		static EnvironmentProperty mEnvProp = null;
		public static EnvironmentProperty EnvProp
		{
			get
			{
				if( mEnvProp == null )
				{
					mEnvProp = new EnvironmentProperty();
					mEnvProp.LoadPropertyFile( "_env_server_config/env.property" );
				}

				return mEnvProp;
			}
		}
	}
}
