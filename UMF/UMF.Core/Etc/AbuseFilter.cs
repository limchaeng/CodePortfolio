//////////////////////////////////////////////////////////////////////////
//
// AbuseFilter
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

using System.Collections.Generic;

namespace UMF.Core
{
	public class AbuseFilterManager
	{
		[System.Flags]
		public enum eCheckedMatchFlag
		{
			None = 0x0000,
			CheckName = 0x0001,
			NamePartial = 0x0002,
			CheckChat = 0x0010,
			ChatPartial = 0x0020,
		}

		public class WordCheckedInfo
		{
			public eCheckedMatchFlag m_CheckedMatchFlags = eCheckedMatchFlag.None;
			public string m_Text = "";

			public WordCheckedInfo( eCheckedMatchFlag flag, string _text )
			{
				m_CheckedMatchFlags = flag;
				m_Text = _text;
			}

			public bool IsCheck( eCheckedMatchFlag flag )
			{
				return ( ( m_CheckedMatchFlags & flag ) != 0 );
			}
		}

		public class AbuseFilter
		{
			bool is_end;
			Dictionary<char, AbuseFilter> m_Datas = new Dictionary<char, AbuseFilter>();

			public AbuseFilter()
			{
			}

			public void Add( string str, int index )
			{
				if( str.Length == index )
					is_end = true;
				else
				{
					char c = str[index];

					AbuseFilter filter;
					if( m_Datas.TryGetValue( c, out filter ) == false )
					{
						filter = new AbuseFilter();
						m_Datas.Add( c, filter );
					}

					filter.Add( str, index + 1 );
				}
			}

			public int GetFilteredText( string str, int index )
			{
				if( str.Length == index )
				{
					// input string end
					if( is_end == true )
						return 1;
					else
						return 0;
				}

				AbuseFilter filter;
				string tmpStr = str.ToLower();
				if( m_Datas.TryGetValue( tmpStr[index], out filter ) == true )
				{
					if( filter.is_end )
						return 1 + 1;

					int ret_length = filter.GetFilteredText( str, index + 1 );
					if( ret_length == 0 )
						return 0;
					return ret_length + 1;
				}
				else
				{
					if( is_end == true )
						return 1;
					return 0;
				}
			}
		}

		AbuseFilter m_Root = new AbuseFilter();

		public string GetFilteredText( string str, string replace_text, List<WordCheckedInfo> word_check_info_list )
		{
			if( string.IsNullOrEmpty( str ) == true )
				return str;

			string filteredText = "";

			for( int i = 0; i < str.Length; )
			{
				int filter_length = m_Root.GetFilteredText( str, i );
				if( filter_length > 0 )
				{
					string check_text = str.Substring( i, filter_length - 1 );
					WordCheckedInfo check_info = word_check_info_list.Find( w => w.m_Text == check_text.ToLower() );
					if( check_info != null && check_info.IsCheck( eCheckedMatchFlag.CheckChat ) )
					{
						if( check_info.IsCheck( eCheckedMatchFlag.ChatPartial ) )
						{
							for( int x = 0; x < check_text.Length; x++ )
								filteredText += replace_text;

							i += filter_length - 1;
							continue;
						}
						else
						{
							if( ( i == 0 || str[i - 1] == ' ' ) && ( i + filter_length - 1 >= str.Length || str[i + filter_length - 1] == ' ' ) )
							{
								for( int x = 0; x < check_text.Length; x++ )
									filteredText += replace_text;

								if( str.Length == check_text.Length )
									break;
								else
									i += filter_length - 1;

								continue;
							}
						}
					}
				}

				filteredText += str[i++];
			}
			return filteredText;
		}

		public bool IsMatcheName( string str, List<WordCheckedInfo> word_check_info_list )
		{
			if( use_whitespace == true )
				str = str.Replace( " ", "" ).ToLower();

			for( int i = 0; i < str.Length; ++i )
			{
				int len = m_Root.GetFilteredText( str, i );
				if( len > 0 )
				{
					string filtered_text = str.Substring( i, len - 1 );
					WordCheckedInfo check_info = word_check_info_list.Find( w => w.m_Text == filtered_text.ToLower() );
					if( check_info != null && check_info.IsCheck( eCheckedMatchFlag.CheckName ) )
					{
						if( check_info.IsCheck( eCheckedMatchFlag.NamePartial ) )
						{
							return true;
						}
						else
						{
							if( str.Length == filtered_text.Length )
								return true;
							else
								i += len - 1;
						}
					}
				}
			}
			return false;
		}

		public void Add( string str )
		{
			if( use_whitespace == true )
				str = str.Replace( " ", "" ).ToLower();
			m_Root.Add( str, 0 );
		}

		bool use_whitespace;
		public AbuseFilterManager( bool use_whitespace )
		{
			this.use_whitespace = use_whitespace;
		}
	}
}