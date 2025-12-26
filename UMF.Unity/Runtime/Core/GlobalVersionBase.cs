//////////////////////////////////////////////////////////////////////////
//
// GlobalVersionBase
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMF.Core;

namespace UMF.Unity
{
	public abstract class GlobalVersionBase<T> : Singleton<T> where T : class, new()
	{
        // INFO : Version Define [1~21].[0~99].[1~99].[1~9] : 21 for version code(android)
        // INFO : XbuildNum : android automatic convert 2digit number
        // 3Digit
        // = Version Define [0~99].[0~99].[1~99]
        // = BuildNum = 1~999
        // => FullVersionText = [0~99].[0~99].[1~99].[1~999]
        protected abstract System.Version __DEV_VERSION__ { get; }
		protected virtual bool Use3DigitVersion { get { return false; } }
		public virtual int BUILD_NUMBER { get { return 0; } }

		//------------------------------------------------------------------------	
		protected System.Version _version = null;
		public System.Version VERSION
		{
			get
			{
				if( _version == null )
					_version = new System.Version( VERSION_STRING );

				return _version;
			}
		}

		//------------------------------------------------------------------------	
		string _VERSION_STRING_CACHE = null;
		public string VERSION_STRING
		{
			get
			{
				if( _VERSION_STRING_CACHE == null )
					_VERSION_STRING_CACHE = __DEV_VERSION__.ToString();

				return _VERSION_STRING_CACHE;
			}
		}

		//------------------------------------------------------------------------
		string _FULL_VERSION_STRING_CACHE = null;
		public string FULL_VERSION_STRING
		{
			get
			{
				if( _FULL_VERSION_STRING_CACHE == null )
				{
					if( Use3DigitVersion )
						_FULL_VERSION_STRING_CACHE = $"{VERSION_STRING}.{BUILD_NUMBER}";
					else
                        _FULL_VERSION_STRING_CACHE = $"{VERSION_STRING} b{BUILD_NUMBER}";
                }

				return _FULL_VERSION_STRING_CACHE;
			}
		}

        // android build number
        // [nn][nn][nn][nn][nn]
        //   1  00  09  04  60 <= 1.0.9.4 build 60
        // [nn][nn][nn][bbb]
        //   1  12  34  678 <= 1.12.34.678
        public int ANDROID_BUILD_NUMBER( int xbuild_num )
		{
			System.Version _version = VERSION;

			int ver_code = 0;

			if( Use3DigitVersion )
			{
                ver_code += _version.Major * 10000000;   // 0 00 00 000 ~ 99 00 00 000
                ver_code += _version.Minor * 100000;     // 0 00 000 ~ 99 00 000
                ver_code += _version.Build * 1000;       // 1 000 ~ 99 000
				ver_code += ( xbuild_num % 1000 );		 // 1 ~ 999
            }
            else
			{
                ver_code += _version.Major * 1000000;   // 1 00 00 00 ~ 99 00 00 00
                ver_code += _version.Minor * 10000;     // 1 00 00 ~ 99 00 00
                ver_code += _version.Build * 100;       // 1 00 ~ 99 00
                ver_code += _version.Revision;          // 0 ~ 99
                ver_code *= 100;
                ver_code += ( xbuild_num % 100 );       // 0 ~ 99
            }

            return ver_code;
		}

        //------------------------------------------------------------------------	
        // Apple CFBundleVersion
        // [nn].[n][n][n].[nn]
        //   1 .    9  4 . 47 <= 1.0.9.4 build 47
        //   1.  2  2  5 . 12 <= 1.2.2.5 build 12
        // [nn].[nn][nn].[bbb]
        //   1.   2   3 .  99 <= 1.2.3 build 99
        public string IOS_BUILD_NUMBER( int xbuild_num )
		{
			if( Use3DigitVersion == false )
				return string.Format( "{0}.{1}{2}{3}.{4}", VERSION.Major, VERSION.Minor, VERSION.Build, VERSION.Revision, xbuild_num );
			else
                return string.Format( "{0}.{1}{2}.{3}", VERSION.Major, VERSION.Minor, VERSION.Build, xbuild_num );
        }

        //------------------------------------------------------------------------
        // [n].[nn][nn][nn].[n]
        //  1 . 00  09  04 . 10 <= 1.0.9.4 b10    = 1.94.10
        //  1 . 00  12  04 . 10 <= 1.0.12.4 b10   = 1.1204.10
        //  1 . 01  09  04 . 10 <= 1.1.9.4 b10    = 1.10904.10
        //  1 . 01  23  45 , 10 <= 1.1.23.45 b10  = 1.12345.10
        // [n].[nn][nn].[bbb]
		//  1 .  2   3 .  99 <= 1.2.3 build 99
        public System.Version VERSION_STRING_3_DIGIT( int xbuild_num )
		{
			if( Use3DigitVersion == false )
			{
				int major = VERSION.Major;
				int minor = ( VERSION.Revision + ( VERSION.Build * 100 ) + ( VERSION.Minor * 10000 ) );
				int build = xbuild_num;

				return new System.Version( major, minor, build, 0 );
			}
			else
			{
                int major = VERSION.Major;
				int minor = ( VERSION.Build * 1000 ) + ( VERSION.Minor * 100000 );
                int build = xbuild_num;

                return new System.Version( major, minor, build, 0 );
            }
        }
	}
}