using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UMTools.Common
{
	public static class ControlExtensions
	{
		public static void SetButtonPathContextMenu( this Button button, string full_path )
		{
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
				button.ContextMenu = context_menu;
			}
		}

	}
}
