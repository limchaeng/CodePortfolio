//////////////////////////////////////////////////////////////////////////
//
// GeoIP
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
using MaxMind.GeoIP2;
using UMF.Core;
using System.Net;
using MaxMind.GeoIP2.Responses;

namespace UMP.Server
{
	//------------------------------------------------------------------------	
	public class GeoIP : Singleton<GeoIP>
	{
		bool mIsLoaded = false;
		DatabaseReader mGeoIP2Reader = null;

		//------------------------------------------------------------------------		
		bool LoadData()
		{
			if( mIsLoaded == true )
				return ( mGeoIP2Reader != null );

			try
			{
				mGeoIP2Reader = new DatabaseReader( "GeoIP/GeoLite2-City.mmdb", MaxMind.Db.FileAccessMode.Memory );
				Log.Write( "## GeoIP Loaded!" );
			}
			catch( System.Exception ex )
			{
				Log.WriteWarning( ex.ToString() );
				mGeoIP2Reader = null;
			}

			return ( mGeoIP2Reader != null );
		}

		//------------------------------------------------------------------------		
		public string FindGeoIPIsoCode( IPAddress ip )
		{
			string tmp = "";
			return FindGeoIPIsoCode( ip, ref tmp );
		}
		public string FindGeoIPIsoCode( IPAddress ip, ref string city_name )
		{
			if( LoadData() )
			{
				try
				{
					CityResponse city;
					if( mGeoIP2Reader.TryCity( ip, out city ) )
					{
						city_name = city.City.Name;
						if( city_name == null )
							city_name = "";

						return city.Country.IsoCode;
					}
				}
				catch( System.Exception ex )
				{
					Log.WriteWarning( ex.ToString() );
				}
			}

			return "";
		}
	}
}
