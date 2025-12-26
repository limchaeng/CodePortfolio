using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using UMF.Core;

namespace UMTools.UMLauncher
{
	public class CheckNewVer : Singleton<CheckNewVer>
	{
		public class VersionData
		{
			public string file_name = "";
			public Version version = new Version( 0, 0, 0, 0 );
			public int revision = 0;
			public int build_num = 0;
			public bool use_xdelta = false;

			//
			public string type_name = "";
			public string download_file_path = "";
			public string install_path = "";
			public string changed_log = "";

			public bool IsAboveVersion( VersionData src )
			{
				if( src == null )
					return true;

				if( version > src.version )
					return true;

				if( version == src.version && revision > src.revision )
					return true;

				if( version == src.version && revision == src.revision && build_num > src.build_num )
					return true;

				return false;
			}

			public string ToVersionString()
			{
				return string.Format( "version={0},revision={1},build={2},file={3}", version, revision, build_num, file_name );
			}

			public Version GetPrevVersion( int step )
			{
				int v1 = version.Major;
				int v2 = version.Minor;
				int v3 = version.Build;
				int v4 = version.Revision;

				for( int i = 0; i < step; i++ )
				{
					if( v4 > 1 )
					{
						v4--;
					}
					else
					{
						v4 = 9;
						if( v3 > 0 )
						{
							v3--;
						}
						else
						{
							v3 = 9;
							if( v2 > 0 )
							{
								v2--;
							}
							else
							{
								v2 = 9;
								if( v1 > 0 )
								{
									v1--;
								}
							}
						}
					}
				}

				return new Version( v1, v2, v3, v4 );
			}
		}

		//------------------------------------------------------------------------			
		// file : [prefix]_[version]_[typename]_r[revision]_b[build num].extention
		public VersionData FileName2VersionData( string src )
		{
			return FileName2VersionData<VersionData>( src );
		}
		public T FileName2VersionData<T>( string src ) where T : VersionData, new()
		{
			T data = new T();
			data.file_name = src;

			string[] s_split = src.Split( '_' );
			if( s_split == null || s_split.Length < 5 )
				return data;

			data.version = StringUtil.SafeParse<Version>( s_split[1].Trim(), new Version( 0, 0, 0, 0 ) );
			data.revision = StringUtil.SafeParse<int>( s_split[3].Trim().Replace( "r", "" ), 0 );
			data.build_num = StringUtil.SafeParse<int>( s_split[4].Trim().Replace( "b", "" ), 0 );
			data.type_name = s_split[2].Trim();

			return data;
		}

		//------------------------------------------------------------------------
		public T VersionURL2VersionData<T>( string src ) where T : VersionData, new()
		{
			T data = new T();

			string[] splits = src.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
			if( splits != null )
			{
				foreach( string sp in splits )
				{
					string[] split2 = sp.Split( '=' );
					if( split2 == null || split2.Length < 2 )
						continue;

					if( split2[0].Trim() == "version" )
						data.version = StringUtil.SafeParse<Version>( split2[1].Trim(), new Version( 0, 0, 0, 0 ) );
					else if( split2[0].Trim() == "revision" )
						data.revision = StringUtil.SafeParse<int>( split2[1].Trim(), 0 );
					else if( split2[0].Trim() == "build" )
						data.build_num = StringUtil.SafeParse<int>( split2[1].Trim(), 0 );
					else if( split2[0].Trim() == "file" )
						data.file_name = split2[1].Trim();
					else if( split2[0].Trim() == "xdelta" )
						data.use_xdelta = true;
				}
			}

			return data;
		}

		//------------------------------------------------------------------------
		public VersionData CheckNewVersion( string check_url, string change_log_url_prefix )
		{
			VersionData v_data = null;
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create( check_url );
			request.Method = "GET";
			request.Timeout = 30 * 1000;

			using( HttpWebResponse resp = (HttpWebResponse)request.GetResponse() )
			{
				HttpStatusCode status = resp.StatusCode;
				if( status != HttpStatusCode.OK )
				{
					throw new Exception( resp.StatusDescription );
				}
				else
				{
					Stream respStream = resp.GetResponseStream();
					using( StreamReader sr = new StreamReader( respStream ) )
					{
						while( true )
						{
							string line = sr.ReadLine();
							if( line == null )
								break;

							v_data = VersionURL2VersionData<VersionData>( line );
						}
					}
				}
			}

			if( v_data != null && string.IsNullOrEmpty( change_log_url_prefix ) == false )
			{
				v_data.changed_log = GetChangedLog( v_data, check_url, change_log_url_prefix );
			}

			return v_data;
		}

		//------------------------------------------------------------------------		
		public string GetChangedLog( VersionData v_data, string ver_url_prefix, string change_log_url_prefix )
		{
			// version list
			string version_list_url = string.Format( "{0}list_{1}.txt", ver_url_prefix.Replace( ".txt", "" ), v_data.version );
			List<VersionData> vdatalist = new List<VersionData>();

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create( version_list_url );
			request.Method = "GET";
			request.Timeout = 30 * 1000;

			using( HttpWebResponse resp = (HttpWebResponse)request.GetResponse() )
			{
				HttpStatusCode status = resp.StatusCode;
				if( status != HttpStatusCode.OK )
				{
					throw new Exception( resp.StatusDescription );
				}
				else
				{
					Stream respStream = resp.GetResponseStream();
					using( StreamReader sr = new StreamReader( respStream ) )
					{
						while( true )
						{
							string line = sr.ReadLine();
							if( line == null )
								break;

							VersionData prev_v_data = VersionURL2VersionData<VersionData>( line );
							vdatalist.Add( prev_v_data );
						}
					}
				}
			}

			vdatalist = vdatalist.OrderByDescending( v => v.revision ).ToList();
			if( vdatalist.Count <= 0 )
				vdatalist.Add( v_data );

			StringBuilder sb_log = new StringBuilder();

			foreach( VersionData ver_data in vdatalist )
			{
				sb_log.AppendLine( string.Format( "==== r{0} ====", ver_data.revision ) );
				string file_ext = Path.GetExtension( ver_data.file_name );
				string _log_url = string.Format( "{0}_{1}.xml", change_log_url_prefix, ver_data.file_name.Replace( file_ext, "" ) );

				try
				{
					XmlDocument doc = new XmlDocument();
					doc.Load( _log_url );

					foreach( XmlNode msg_node in doc.SelectNodes( "freeStyleBuild/changeSet/item/msg" ) )
					{
						sb_log.AppendLine( msg_node.InnerText );
					}
				}
				catch( Exception )
				{
					sb_log.AppendLine( $">> not found change log : {_log_url}" );
				}

				sb_log.AppendLine( "" );
			}

			return sb_log.ToString();
		}
	}
}
