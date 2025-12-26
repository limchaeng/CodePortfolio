using System;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using UMF.Core;
using System.Threading.Tasks;

namespace UMTools.Common
{
	public class I18NTextExportConst
	{
		public const string TRANSLATION_PATH = "_Translation";
		public const string TRANSLATION_IMPORT_PATH_PREFIX = "xml_";
		public const string TRANSLATION_IMPORT_EQUAL_PREFIX = "xmlequal_";
		public const string TRANSLATION_MERGED_PATH_PREFIX = "xmlmerged_";
	}

	public class UMToolsCommon
	{
		public class CSafeParamData
		{
			public object[] obj_params = null;
			
			public CSafeParamData(params object[] parms)
			{
				obj_params = parms;
			}

			public T GetParam<T>(int idx, T def_value)
			{
				if( obj_params == null )
					return def_value;

				if( idx < obj_params.Length )
					return StringUtil.SafeParse<T>( obj_params[idx].ToString(), def_value );

				return def_value;
			}
		}
		public delegate void CSafeProgressSetValue( Int32 value );
		public delegate void CSafeProgressSetMaxValue( Int32 value );
		public delegate void CSafeTextBoxSetText( string value );
		public delegate void CSafeControlParams( CSafeParamData value );

		public class FinderEventProperty
		{
			public object _type;
			public object _sub_type;
			public int _target_idx;

			public FinderEventProperty( object _t ) : this( _t, null, 0 ) { }
			public FinderEventProperty(object _t, object _s_t) : this(_t, _s_t, 0) { }
			public FinderEventProperty(object _t, object _s_t, int _t_idx)
			{
				this._type = _t;
				this._sub_type = _s_t;
				this._target_idx = _t_idx;
			}
		}

		public class FinderEventArgsBase : EventArgs
		{
			public FinderEventProperty property;
			public int idn;
			public string name;
		}

		public delegate void FormSendDataHandler( object obj, EventArgs a );

		public class FinderContextMenu
		{
			public string menu_name;
			public FormSendDataHandler callback;
			public FinderEventProperty property;

			public FinderContextMenu(string _name, FormSendDataHandler _callback, FinderEventProperty _property)
			{
				menu_name = _name;
				callback = _callback;
				property = _property;
			}

			public void Execute(object sender)
			{
				if( callback != null )
				{
					//callback( sender, evnet );
				}
			}
		}

		public static void ButtonPathTooltipOrDisable( ToolTip tooltip, Button btn, string path, string suffix = "" )
		{
			if( string.IsNullOrEmpty( path ) )
			{
				btn.Enabled = false;
				btn.ContextMenu = null;
			}
			else
			{
				string full_path = path + suffix;
				btn.Enabled = true;
				tooltip.SetToolTip( btn, full_path );

				string dir = "";
				if( File.Exists( full_path ) )
					dir = Path.GetDirectoryName( full_path );
				else
					dir = full_path;

				if( Directory.Exists( dir ) )
				{
					ContextMenu context_menu = new ContextMenu();
					MenuItem context_menu_item = new MenuItem( "Open " + dir, ( object sender, EventArgs e ) =>
					{
						System.Diagnostics.Process.Start( "explorer.exe", dir );
					} );
					context_menu.MenuItems.Add( context_menu_item );
					btn.ContextMenu = context_menu;
				}
			}
		}
	}

	public class LogWriter
	{
		[DllImport( "user32.dll", CharSet = CharSet.Auto )]
		private static extern int SendMessage( IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam );
		private const int WM_VSCROLL = 0x115;
		private const int SB_BOTTOM = 7;

		RichTextBox mRTB;
		StreamWriter mFileLogWriter = null;

		public LogWriter( RichTextBox rich_tb, string file_log_path = null)
		{
			mRTB = rich_tb;

			if( string.IsNullOrEmpty(file_log_path) == false )
			{
				try
				{
					if( File.Exists( file_log_path ) )
						File.Delete( file_log_path );

					FileStream fs = new FileStream( file_log_path, FileMode.CreateNew, FileAccess.Write );
					mFileLogWriter = new StreamWriter( fs, System.Text.Encoding.UTF8 );
				}
				catch (System.Exception ex)
				{
					mFileLogWriter = null;
					MessageBox.Show( "FileLog invalid:" + ex.ToString() );
				}
			}
		}
		~LogWriter()
		{
			Closed();
		}

		public void LogWrite( string fmt, params object[] parms )
		{
			if( mRTB == null )
				return;

			string log = fmt;
			if( parms != null && parms.Length > 0 )
				log = string.Format( fmt, parms );

			string timestring = DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss" );
			mRTB.AppendText( log );
			mRTB.AppendText( string.Format( "\r\n----- [{0}] -----\r\n", timestring ) );

			if( mFileLogWriter != null )
			{
				mFileLogWriter.WriteLine( log );
				mFileLogWriter.Flush();
			}

			SendMessage( mRTB.Handle, WM_VSCROLL, (IntPtr)SB_BOTTOM, IntPtr.Zero );
		}

		public void Clear()
		{
			mRTB.Clear();
		}

		public void Closed()
		{
			try
			{
				if( mFileLogWriter != null )
				{
					mFileLogWriter.Flush();
					mFileLogWriter.Close();
					mFileLogWriter = null;
				}
			}
			catch( System.Exception ex )
			{

			}
		}
	}

	public class SimpleTooltip
	{
		public string tooltip;

		public SimpleTooltip( string tooltip )
		{
			this.tooltip = tooltip;
		}

		public static SimpleTooltip Create(string tooltip)
		{
			return new SimpleTooltip( tooltip );
		}
	}
}
