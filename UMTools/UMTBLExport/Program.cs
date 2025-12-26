using System;
using System.Threading;
using System.Windows.Forms;

namespace UMTools.TBLExport
{
	static class Program
	{
		/// <summary>
		/// 해당 응용 프로그램의 주 진입점입니다.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.ThreadException += new System.Threading.ThreadExceptionEventHandler( Application_ThreadException );
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler( CurrentDomain_UnhandledException );

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new FormTBLExport());
		}

		private static void CurrentDomain_UnhandledException( object sender, UnhandledExceptionEventArgs e )
		{
			Exception ex = e.ExceptionObject as Exception;
			MessageBox.Show( $"{ex.ToString()}", "Domain Exception" );
		}

		private static void Application_ThreadException( object sender, ThreadExceptionEventArgs e )
		{
			MessageBox.Show( $"{e.Exception.Message}", "Thread Exception" );
		}
	}
}
