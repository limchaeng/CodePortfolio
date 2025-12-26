//////////////////////////////////////////////////////////////////////////
//
// TextReplaceUtil
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UMF.Core
{
	public static class TextReplaceUtil
	{
		public static string PREFIX = "[#";
		public static string TAG_END = "]";

		public class TEXT_REPLACE_CONTAINER
		{
			string REPLACE_KEY = "";
			System.Func<string, string, string, string> _DoReplace;

			public TEXT_REPLACE_CONTAINER( string _replace_key, System.Func<string, string, string, string> _func )
			{
				REPLACE_KEY = _replace_key;
				_DoReplace = _func;
			}

			public string DoReplace( string input )
			{
				string replaced_text = input;
				for( int i = 0; i < input.Length; i++ )
				{
					string replace_key = REPLACE_KEY;
					string replace_suffix = "";

					if( CheckReplaceSuffix( replaced_text, ref replace_key, ref replace_suffix ) )
					{
						if( replaced_text.Contains( replace_key ) )
						{
							replaced_text = _DoReplace( input, replace_key, replace_suffix );
						}
						else
						{
							break;
						}
					}
					else
					{
						break;
					}
				}

				return replaced_text;
			}
		}

		//------------------------------------------------------------------------	
		static bool CheckReplaceSuffix( string replace_text, ref string replaceKey, ref string replaceSuffix )
		{
			int offset = -1;
			int endoffset = -1;
			int midoffset = 0;

			offset = replace_text.IndexOf( replaceKey, 0 );
			if( offset != -1 )
			{
				endoffset = replace_text.IndexOf( "]", offset );
				if( endoffset != -1 )
				{
					midoffset = offset + replaceKey.Length;
					replaceSuffix = replace_text.Substring( midoffset, endoffset - midoffset );

					replaceKey = replace_text.Substring( offset, endoffset - offset + 1 );

					return true;
				}
			}

			return false;
		}

		static List<TEXT_REPLACE_CONTAINER> mTextReplaceContainerList = new List<TEXT_REPLACE_CONTAINER>();
		public static void AddContainer( TEXT_REPLACE_CONTAINER container )
		{
			mTextReplaceContainerList.Add( container );
		}

		//------------------------------------------------------------------------		
		public static string GetTextReplace( string src_text )
		{
			if( src_text.Contains( PREFIX ) == false || mTextReplaceContainerList.Count == 0 )
				return src_text;

			string replaced_text = src_text;
			foreach( TEXT_REPLACE_CONTAINER tp in mTextReplaceContainerList )
			{
				replaced_text = tp.DoReplace( replaced_text );
			}

			return replaced_text;
		}
	}
}
