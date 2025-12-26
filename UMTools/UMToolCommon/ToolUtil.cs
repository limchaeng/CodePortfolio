using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Reflection;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Security.Cryptography;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;
using System.Drawing;
using System.Runtime.Serialization.Json;
using UMF.Core;

namespace UMTools.Common
{
	//------------------------------------------------------------------------	
	public class WaitCursor : IDisposable
	{
		public WaitCursor()
		{
			Cursor.Current = Cursors.WaitCursor;
		}

		public void Dispose()
		{
			Cursor.Current = Cursors.Default;
		}
	}

	public class ToolUtil
	{
		//------------------------------------------------------------------------

		static Dictionary<string, string> mCommandListArgsCached = null;
		public static T GetCommandLineArgument<T>(string arg_name, T default_value )
		{
			if( mCommandListArgsCached == null )
			{
				mCommandListArgsCached = new Dictionary<string, string>();
				string[] args = System.Environment.GetCommandLineArgs();
				if( args != null )
				{
					foreach(string arg in args)
					{
						string[] splits = arg.Split( '=' );
						if( splits != null )
						{
							string key = "";
							string value = "";
							if( splits.Length > 0 )
								key = splits[0].Replace( "-", "" );

							if( splits.Length > 1 )
								value = splits[1].Trim();

							if( string.IsNullOrEmpty(key) == false )
							{
								mCommandListArgsCached.Add( key, value );									
							}
						}
					}
				}
			}

			string _value;
			if( mCommandListArgsCached.TryGetValue( arg_name, out _value ) )
				return StringUtil.SafeParse<T>( _value, default_value );

			return default_value;
		}

		readonly static string REG_BASE_KEY = "SOFTWARE\\FN_UMTools";
		public static void SavePrefs( string reg_key, string key, object value )
		{
			RegistryKey rk = Registry.CurrentUser.OpenSubKey( $"{REG_BASE_KEY}\\{reg_key}", true );

			if( rk == null )
			{
				rk = Registry.CurrentUser.CreateSubKey( reg_key );
			}

			if( value == null )
				rk.DeleteValue( key, false );
			else
				rk.SetValue( key, value );
		}

		public static T GetPrefs<T>(string reg_key, string key, T default_value)
		{
			string p_value = GetPrefsString( reg_key, key, "" );

			return StringUtil.SafeParse<T>( GetPrefsString( reg_key, key, "" ), default_value );
		}
		public static object GetPrefs( string reg_key, string key )
		{
			RegistryKey rk = Registry.CurrentUser.OpenSubKey( $"{REG_BASE_KEY}\\{reg_key}", true );

			if( rk == null )
			{
				rk = Registry.CurrentUser.CreateSubKey( $"{REG_BASE_KEY}\\{reg_key}" );
			}

			return rk.GetValue( key );
		}

		public static string GetPrefsString(string reg_key, string key, string default_value = "")
		{
			object o_value = GetPrefs( reg_key, key );
			if( o_value == null )
				return default_value;

			return o_value.ToString();
		}

		public static DateTime GetLinkerTime( Assembly assembly, TimeZoneInfo target = null )
		{
			var filePath = assembly.Location;
			const int c_PeHeaderOffset = 60;
			const int c_LinkerTimestampOffset = 8;

			var buffer = new byte[2048];

			using( var stream = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
				stream.Read( buffer, 0, 2048 );

			var offset = BitConverter.ToInt32( buffer, c_PeHeaderOffset );
			var secondsSince1970 = BitConverter.ToInt32( buffer, offset + c_LinkerTimestampOffset );
			var epoch = new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc );

			var linkTimeUtc = epoch.AddSeconds( secondsSince1970 );

			var tz = target ?? TimeZoneInfo.Local;
			var localTime = TimeZoneInfo.ConvertTimeFromUtc( linkTimeUtc, tz );

			return localTime;
		}

		public static String PrintXML( XmlDocument document )
		{
			String Result = "";

			MemoryStream mStream = new MemoryStream();
			XmlTextWriter writer = new XmlTextWriter( mStream, Encoding.Unicode );

			try
			{
				writer.Formatting = Formatting.Indented;

				// Write the XML into a formatting XmlTextWriter
				document.WriteContentTo( writer );
				writer.Flush();
				mStream.Flush();

				// Have to rewind the MemoryStream in order to read
				// its contents.
				mStream.Position = 0;

				// Read MemoryStream contents into a StreamReader.
				StreamReader sReader = new StreamReader( mStream );

				// Extract the text from the StreamReader.
				String FormattedXML = sReader.ReadToEnd();

				Result = FormattedXML;
			}
			catch( XmlException )
			{
			}

			mStream.Close();
			writer.Close();

			return Result;
		}

		public static string DecimalToHex24( int num )
		{
			return num.ToString( "X6" );
		}
		public static string DecimalToHex32( int num )
		{
			return num.ToString( "X8" );
		}

		public static int HexToDecimal( char ch )
		{
			switch( ch )
			{
				case '0': return 0x0;
				case '1': return 0x1;
				case '2': return 0x2;
				case '3': return 0x3;
				case '4': return 0x4;
				case '5': return 0x5;
				case '6': return 0x6;
				case '7': return 0x7;
				case '8': return 0x8;
				case '9': return 0x9;
				case 'a':
				case 'A': return 0xA;
				case 'b':
				case 'B': return 0xB;
				case 'c':
				case 'C': return 0xC;
				case 'd':
				case 'D': return 0xD;
				case 'e':
				case 'E': return 0xE;
				case 'f':
				case 'F': return 0xF;
			}
			return 0xF;
		}

		public static System.Drawing.Color ParseColor24( string text, int offset )
		{
			int r = ( HexToDecimal( text[offset] ) << 4 ) | HexToDecimal( text[offset + 1] );
			int g = ( HexToDecimal( text[offset + 2] ) << 4 ) | HexToDecimal( text[offset + 3] );
			int b = ( HexToDecimal( text[offset + 4] ) << 4 ) | HexToDecimal( text[offset + 5] );
			return System.Drawing.Color.FromArgb( r, g, b );
		}

		public static System.Drawing.Color ParseColor32( string text, int offset )
		{
			int r = ( HexToDecimal( text[offset] ) << 4 ) | HexToDecimal( text[offset + 1] );
			int g = ( HexToDecimal( text[offset + 2] ) << 4 ) | HexToDecimal( text[offset + 3] );
			int b = ( HexToDecimal( text[offset + 4] ) << 4 ) | HexToDecimal( text[offset + 5] );
			int a = ( HexToDecimal( text[offset + 6] ) << 4 ) | HexToDecimal( text[offset + 7] );
			return System.Drawing.Color.FromArgb( a, r, g, b );
		}

		public static Color GetColorFromHEX( string hex )
		{
			return ColorTranslator.FromHtml( hex );
		}

		public static int ColorToInt( System.Drawing.Color c )
		{
			int retVal = 0;
			retVal |= (int)c.R << 24;
			retVal |= (int)c.G << 16;
			retVal |= (int)c.B << 8;
			retVal |= (int)c.A;
			return retVal;
		}

		public static string EncodeColor24( System.Drawing.Color c )
		{
			int i = 0xFFFFFF & ( ColorToInt( c ) >> 8 );
			return DecimalToHex24( i );
		}
		public static string EncodeColor32( System.Drawing.Color c )
		{
			int i = ColorToInt( c );
			return DecimalToHex32( i );
		}

		//------------------------------------------------------------------------	
		public static System.Drawing.Color GetColor(string color_code)
		{
			if( color_code.Length == 8 )
				return ParseColor32( color_code, 0 );
			else if( color_code.Length == 6 )
				return ParseColor24( color_code, 0 );
			else
				return System.Drawing.Color.White;
		}
		public static System.Drawing.Color GetNegativeColor(System.Drawing.Color color)
		{
			return System.Drawing.Color.FromArgb( 255 - color.R, 255 - color.G, 255 - color.B );
		}

		//------------------------------------------------------------------------	
		public static string GetByteSize( int byte_size )
		{
			return GetByteSize( (long)byte_size );
		}
		public static string GetByteSize( long byte_size )
		{
			if( byte_size < 1024 )
				return string.Format( "{0} byte", byte_size );
			else if( byte_size < 1024 * 1024 )
				return string.Format( "{0:F2} KB", ( (float)byte_size / 1024 ) );
			else if( byte_size < 1024 * 1024 * 1024 )
				return string.Format( "{0:F2} MB", ( (float)byte_size / ( 1024 * 1024 ) ) );
			else
				return string.Format( "{0:F2} GB", ( (float)byte_size / ( 1024 * 1024 * 1024 ) ) );
		}
		public static string GetFileSize(string file_path)
		{
			if( File.Exists( file_path ) == false )
				return "0 byte";

			FileInfo fi = new FileInfo( file_path );
			return GetByteSize( fi.Length );
		}

		//------------------------------------------------------------------------	
		public static string GetTimeString(int milliseconds)
		{
			if( milliseconds <= 0 )
				return "0 ms";

			float src_ms = (float)milliseconds;
			float sec = src_ms / 1000f;	// seconds
			if( sec < 60f )
				return string.Format( "{0} sec", (int)sec );

			sec = sec / 60f;	// minutes
			if( sec < 60f )
				return string.Format( "{0:N2} min", sec );

			sec = sec / 60f;	// hour
			return string.Format( "{0:N2} hour", sec );
		}

		//------------------------------------------------------------------------	
		public static string MD5File(string file_path)
		{
			string md5 = "";
			if( File.Exists( file_path ) )
			{
				FileStream fs = null;
				try
				{
					fs = File.Open( file_path, FileMode.Open );
					MD5 hash = MD5.Create();
					byte[] md5Hash = hash.ComputeHash( fs );
					md5 = System.Convert.ToBase64String( md5Hash );
				}
				catch( System.Exception ex )
				{
					return ex.ToString();
				}
				finally
				{
					if( fs != null )
						fs.Close();

					fs = null;
				}
			}

			return md5;
		}

		//------------------------------------------------------------------------			
		public static bool MD5FileEqual(string file_path1, string file_path2)
		{
			if( File.Exists( file_path1 ) == false || File.Exists( file_path2 ) == false )
				return false;

			string md5_1 = MD5File( file_path1 );
			string md5_2 = MD5File( file_path2 );

			return md5_1.Equals( md5_2 );
		}

		//------------------------------------------------------------------------	
		public static string MD5HashCheck(string file_path, string checked_md5)
		{
			if( File.Exists( file_path ) )
			{
				FileStream fs = null;
				try
				{
					fs = File.Open( file_path, FileMode.Open );
					MD5 hash = MD5.Create();
					byte[] md5Hash = hash.ComputeHash( fs );
					string s_md5 = System.Convert.ToBase64String( md5Hash );
					if( checked_md5.Equals( s_md5 ) == false )
					{
						return string.Format( "Failed:{0} != {1}", checked_md5, s_md5 );
					}
				}
				catch( System.Exception ex )
				{
					return ex.ToString();
				}
				finally
				{
					if( fs != null )
						fs.Close();

					fs = null;
				}
			}

			return "Hash Check Complete!";
		}

		public static string GetFileExtentionFull( string filename )
		{
			int idx = filename.IndexOf( '.' );
			if( idx < 0 )
				return "";

			return filename.Substring( idx, filename.Length - idx );
		}

		//------------------------------------------------------------------------	
		public static void OpenFolder(string path)
		{
			if( Directory.Exists(path) )
				System.Diagnostics.Process.Start( "explorer.exe", path );
		}
		public static void OpenFolderSelectFile(string path)
		{
			if( File.Exists( path ) )
				System.Diagnostics.Process.Start( "explorer.exe", string.Format( "/select,\"{0}\"", path ) );
		}

		public static string RegExReplace( string input, string pattern, string replacement )
		{
			if( string.IsNullOrEmpty( pattern ) )
				return input;

			return System.Text.RegularExpressions.Regex.Replace( input, pattern, replacement );
		}

		public static bool RegExMatched( string input, string pattern )
		{
			if( string.IsNullOrEmpty( pattern ) )
				return false;

			return System.Text.RegularExpressions.Regex.IsMatch( input, pattern );
		}

		public static bool HasStringsEmpty(params string[] strs)
		{
			foreach (string s in strs)
			{
				if( string.IsNullOrEmpty( s ) )
					return true;
			}

			return false;
		}

		//------------------------------------------------------------------------	
		public static string ZipExtract(string src_zip, string target_path)
		{
			string error = "";
			try
			{
				if( Directory.Exists( target_path ) == false )
					Directory.CreateDirectory( target_path );

				using(FileStream fs = new FileStream(src_zip, FileMode.Open, FileAccess.Read, FileShare.Read) )
				{
					using(ZipInputStream zis = new ZipInputStream(fs))
					{
						ZipEntry zEntry = null;

						while( (zEntry = zis.GetNextEntry()) != null )
						{
							if( zEntry.IsDirectory == false )
							{
								string filename = Path.GetFileName( zEntry.Name );
								string dest_dir = Path.Combine( target_path, Path.GetDirectoryName( zEntry.Name ) );

								if( Directory.Exists( dest_dir ) == false )
									Directory.CreateDirectory( dest_dir );

								string dest_path = Path.Combine( dest_dir, filename );

								using(FileStream writer = new FileStream(dest_path, FileMode.Create, FileAccess.Write, FileShare.Write) )
								{
									byte[] buffer = new byte[2048];
									int len;
									while( (len = zis.Read(buffer, 0, buffer.Length)) > 0 )
									{
										writer.Write(buffer, 0, len);
									}
								}
							}
						}
					}
				}
			}
			catch (System.Exception ex)
			{
				error = ex.ToString();
			}

			return error;
		}

		// ui control tool
		public static TreeNode AddTreeNode( TreeView tree_view, TreeNode parent, object subject, object tag = null )
		{
			TreeNode t_node = null;
			if( parent == null )
				t_node = tree_view.Nodes.Add( subject.ToString() );
			else
				t_node = parent.Nodes.Add( subject.ToString() );

			if( t_node != null )
				t_node.Tag = tag;

			return t_node;
		}

		public static void GroupBoxEnable(bool bEnable, GroupBox g_box, bool all_control)
		{
			g_box.Enabled = bEnable;
			if( all_control )
			{
				foreach (Control ctrl in g_box.Controls)
				{
					ctrl.Enabled = bEnable;
				}
			}
		}

		public static void ComboBoxSelectText(ComboBox cbox, object _text)
		{
			if( cbox.Items.Count <= 0 )
				return;

			for(int i=0; i<cbox.Items.Count; i++)
			{
				if( cbox.Items[i].ToString() == _text.ToString() )
				{
					cbox.SelectedIndex = i;
					return;
				}
			}

			cbox.SelectedIndex = 0;
		}


		public static int DirectoryCopyCount( string src, string dest, bool include_sub_dir )
		{
			int f_count = 0;

			DirectoryInfo dir = new DirectoryInfo( src );
			if( dir.Exists == false )
				return 0;

			try
			{
				DirectoryInfo[] dirs = dir.GetDirectories();
				FileInfo[] files = dir.GetFiles();
				f_count = files.Length;

				if( include_sub_dir )
				{
					foreach( DirectoryInfo sub_dir in dirs )
					{
						string temp_path = Path.Combine( dest, sub_dir.Name );
						int sub_count = DirectoryCopyCount( sub_dir.FullName, temp_path, include_sub_dir );

						f_count += sub_count;
					}
				}
			}
			catch( System.Exception ex )
			{
				f_count = 0;
			}

			return f_count;
		}


		public static List<string> DirectoryCopy(string src, string dest, bool overwrite, bool include_sub_dir, ProgressBar progress_bar=null, Label progress_label=null)
		{
			if( progress_bar != null )
			{
				int file_count = DirectoryCopyCount( src, dest, include_sub_dir );
				progress_bar.Maximum = file_count;
				progress_bar.Value = 0;
				progress_bar.Update();
			}
			if( progress_label != null )
				progress_label.Text = "0";

			List<string> output = new List<string>();

			DirectoryInfo dir = new DirectoryInfo( src );
			if( dir.Exists == false )
				return output;

			try
			{
				DirectoryInfo[] dirs = dir.GetDirectories();
				if( Directory.Exists( dest ) == false )
					Directory.CreateDirectory( dest );

				FileInfo[] files = dir.GetFiles();
				foreach(FileInfo file in files)
				{
					string temppath = Path.Combine( dest, file.Name );
					file.CopyTo( temppath, overwrite );
					output.Add( string.Format( "{0} => To => {1}", file.FullName, temppath ) );

					if( progress_bar != null )
					{
						progress_bar.Value += 1;
						progress_bar.Update();

						if( progress_label != null )
						{
							progress_label.Text = progress_bar.Value.ToString();
							progress_label.Update();
						}
					}
				}

				if( include_sub_dir )
				{
					foreach( DirectoryInfo sub_dir in dirs )
					{
						output.Add( string.Format( "::: SUB_DIR ::: {0}", sub_dir.FullName ) );

						string temp_path = Path.Combine( dest, sub_dir.Name );
						List<string> _output = DirectoryCopy( sub_dir.FullName, temp_path, overwrite, include_sub_dir, progress_bar );
						output.AddRange( _output );
					}
				}
			}
			catch( System.Exception ex )
			{
				output.Add( ex.ToString() );
			}

			return output;
		}
		
		public static void DirectoryDelete(string dir, bool is_sub=false)
		{
			DirectoryInfo dir_info = new DirectoryInfo( dir );
			foreach( FileInfo file in dir_info.GetFiles() )
				file.Delete();

			foreach( DirectoryInfo subDirectory in dir_info.GetDirectories() )
				subDirectory.Delete( true );

			dir_info.Delete( true );
		}

		//------------------------------------------------------------------------	
		public static bool IsHex( char ch )
		{
			return ( ch >= '0' && ch <= '9' ) || ( ch >= 'a' && ch <= 'f' ) || ( ch >= 'A' && ch <= 'F' );
		}

		//------------------------------------------------------------------------		
		public static decimal RoundUp( decimal input, int places )
		{
			decimal multiplier = (decimal)Math.Pow( 10, Convert.ToDouble( places ) );
 			return Math.Ceiling( input * multiplier ) / multiplier;
		}
		public static decimal RoundDown( decimal number, int places )
		{
			decimal multiplier = (decimal)Math.Pow( 10, Convert.ToDouble( places ) );
			return Math.Floor( number * multiplier ) / multiplier;
		}

		//------------------------------------------------------------------------
		public static bool FileCompareEqual( string file1, string file2 )
		{
			if( string.IsNullOrEmpty( file1 ) || string.IsNullOrEmpty( file2 ) )
				return false;

			if( File.Exists( file1 ) == false || File.Exists( file2 ) == false )
				return false;

			int file1byte;
			int file2byte;
			FileStream fs1;
			FileStream fs2;

			fs1 = new FileStream( file1, FileMode.Open, FileAccess.Read );
			fs2 = new FileStream( file2, FileMode.Open, FileAccess.Read );

			if( fs1.Length != fs2.Length )
			{
				fs1.Close();
				fs2.Close();

				return false;
			}
			do
			{
				file1byte = fs1.ReadByte();
				file2byte = fs2.ReadByte();
			}
			while( ( file1byte == file2byte ) && ( file1byte != -1 ) );

			fs1.Close();
			fs2.Close();

			return ( ( file1byte - file2byte ) == 0 );
		}

		//------------------------------------------------------------------------
		public static bool TxtFileLineCompareEqual(string file1, string file2, int ignore_line=0)
		{
			if( string.IsNullOrEmpty( file1 ) || string.IsNullOrEmpty( file2 ) )
				return false;

			if( File.Exists( file1 ) == false || File.Exists( file2 ) == false )
				return false;

			int line_count = 0;
			string line1 = "";
			string line2 = "";
			StreamReader sr1 = new StreamReader( file1 );
			StreamReader sr2 = new StreamReader( file2 );

			bool is_equal = true;
			while( true )
			{
				line_count++;
				line1 = sr1.ReadLine();
				line2 = sr2.ReadLine();

				if( line1 == null || line2 == null )
					break;

				if( line_count > ignore_line )
				{
					if( line1 != line2 )
					{
						is_equal = false;
						break;
					}
				}
			}

			sr1.Close();
			sr2.Close();

			return is_equal;
		}

		public static void ConvertPictureBoxImage(PictureBox pbox)
		{
			if( pbox == null || pbox.Image == null )
				return;

			Bitmap bmp = pbox.Image as Bitmap;
			if( bmp == null )
				return;

			pbox.SizeMode = PictureBoxSizeMode.Normal;
			bool source_is_wider = (float)bmp.Width / bmp.Height > (float)pbox.Width / pbox.Height;

			Bitmap resized = new Bitmap( pbox.Width, pbox.Height );
			Graphics g = Graphics.FromImage( resized );
			Rectangle dest_rect = new Rectangle( 0, 0, pbox.Width, pbox.Height );
			Rectangle src_rect;

			if( source_is_wider )
			{
				float size_ratio = (float)pbox.Height / bmp.Height;
				int sample_width = (int)( pbox.Width / size_ratio );
				src_rect = new Rectangle( ( bmp.Width - sample_width ) / 2, 0, sample_width, bmp.Height );
			}
			else
			{
				float size_ratio = (float)pbox.Width / bmp.Width;
				int sample_height = (int)( pbox.Height / size_ratio );
				src_rect = new Rectangle( 0, ( bmp.Height - sample_height ) / 2, bmp.Width, sample_height );
			}

			g.DrawImage( bmp, dest_rect, src_rect, GraphicsUnit.Pixel );
			g.Dispose();

			pbox.Image = resized;
		}
	}
}
