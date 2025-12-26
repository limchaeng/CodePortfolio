//////////////////////////////////////////////////////////////////////////
//
// CSConfigData
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
using System.Collections.Generic;
using UMF.Net;

namespace UMP.CSCommon
{
	//------------------------------------------------------------------------
	public class CS_URLConfig
	{
		public enum eURLType : byte
		{
			None,
			TermsOfService,
			PrivacyPolicy,
			GameGuide,
			Community,
			Support,
		}

		public eURLType url_type;
		public string url;
	}

	//------------------------------------------------------------------------
	public class CS_AgreementData
	{
		public enum eTypeFlag : byte
		{
			None			= 0x00,
			TermsOfService	= 0x01,
			PrivacyPolicy	= 0x02,
			RemotePush		= 0x04,
			NightRemotePush = 0x08,
		}

		public eTypeFlag agreement_type;
		public string text;
		public string link_url;
		public bool is_option;
	}

	//------------------------------------------------------------------------
	public class CS_AppConfig
	{
		[System.Flags]
		public enum eFlags : int
		{
			None				= 0x0000,
		}

		public eFlags flags;
		public int client_frame_rate;
		[PacketValue( Type = PacketValueType.SerializeNullable )]
		public List<CS_URLConfig> url_list;
		[PacketValue( Type = PacketValueType.SerializeNullable )]
		public List<CS_AgreementData> agreement_list;

		//------------------------------------------------------------------------		
		public bool HasFlag(eFlags flag)
		{
			return ( ( flags & flag ) != 0 );
		}

		public CS_URLConfig GetURL(CS_URLConfig.eURLType url_type)
		{
			if( url_list != null )
				return url_list.Find( a => a.url_type == url_type );

			return null;
		}
	}

	//------------------------------------------------------------------------
	public class CS_LocalizationSupportData
	{
		public string language;
		public string alias;
		public string icon_name;
		public string font_asset_name;
		public string language_name;
		public string culture_code; // ko-kr
	}

	//------------------------------------------------------------------------
	public class CS_LocalizationConfig
	{
		public List<CS_LocalizationSupportData> support_list;
	}
}
