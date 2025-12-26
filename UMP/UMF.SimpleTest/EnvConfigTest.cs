using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMF.Net;
using System.Reflection;
using UMF.Core;

namespace UMF.SimpleTest
{
	public class EnvConfigTest : SimpleTestBase
	{
		public class TestConfig : EnvConfig
		{
			public eCoreLogType enum_value = eCoreLogType.Detail;
			public Uri url_value = null;
			public string string_value = "";
			public DateTime datetime = DateTime.MinValue;
			public int int_value = 0;
			public float float_value = 0f;
		}

		public class ChildConfig : TestConfig
		{
			public string child_string = "child_string";
		}

		TestConfig mConfig;

		protected override void TestLogic()
		{
			UMF.Core.Log._Log = LogTest;

			StringBuilder sb = new StringBuilder();
			sb.AppendLine( "enum_value = Detail" );
			sb.AppendLine( "url_value = " );
			sb.AppendLine( "string_value = " );
			sb.AppendLine( "datetime = 2022-03-17 00:00:00" );
			sb.AppendLine( "int_value = 99" );
			sb.AppendLine( "float_value = 11.22 " );
			//sb.AppendLine( "child_string = AAA" );

			mConfig = new ChildConfig();
			mConfig.ConfigLoad( "test", sb.ToString() );

			LogTest( "======" );
			LogTest( mConfig.ToString() );
		}
	}
}
