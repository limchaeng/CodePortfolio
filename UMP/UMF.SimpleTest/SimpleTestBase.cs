using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace UMF.SimpleTest
{
	//------------------------------------------------------------------------	
	public abstract class SimpleTestBase
	{
		public SimpleTestBase()
		{
			Console.OutputEncoding = Encoding.UTF8;
			UMF.Core.Log.SetAll( LogTest );
			DoTest();
		}

		public void DoTest()
		{
			try
			{
				Console.WriteLine( "=================================" );
				Console.WriteLine( this.GetType().ToString() );
				Console.WriteLine( "=================================:" );
				TestLogic();
				Console.WriteLine( "=================================:" );
			}
			catch (System.Exception ex)
			{
				Console.WriteLine( ex.ToString() );
			}
		}
		protected abstract void TestLogic();
		public void Sep()
		{
			LogTest( "" );
		}
		public static void LogTest( string text )
		{
			Console.WriteLine( text );
		}
		public static void LogTest( string fmt, params object[] parms )
		{
			Console.WriteLine( string.Format( fmt, parms ) );
		}
		public static void LogTest( CultureInfo culture, string fmt, params object[] parms )
		{
			Console.WriteLine( string.Format( culture, fmt, parms ) );
		}

		public static void LogList<T>( string fmt, List<T> list )
		{
			string data_text = "";
			foreach( T data in list )
			{
				data_text += data.ToString() + ",";
			}

			data_text = data_text.TrimEnd( ',' );
			LogTest( fmt, data_text );
		}

		public void WaitKey()
		{
			LogTest( " -----------------------------" );
			LogTest( "Wait any Key..." );
			Console.ReadKey();
			LogTest( " -----------------------------" );
		}

	}
}
