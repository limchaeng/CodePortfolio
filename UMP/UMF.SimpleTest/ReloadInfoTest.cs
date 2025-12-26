using System;
using System.Collections.Generic;
using UMF.Core;
using UMF.Core.I18N;

namespace UMF.SimpleTest
{
	public class ReloadInfoTest : SimpleTestBase
	{
		public class TestInfo : TBLInfoBase
		{
			public override void Load( XmlSelector node )
			{
				
			}

			public override void LoadAppend( XmlSelector node )
			{
				
			}
		}

		public class TestInfoManager : TBLInfoManager<TestInfo, TestInfoManager>
		{
			public int reload_count = 0;

			public override string DATA_ID => "TEST_DATA";

			public override bool CheckSameIDN => true;

			public override bool CheckSameID => true;

			public override string ReloadData()
			{
				reload_count++;
				return base.ReloadData();
			}

			protected override void ParsingRow( XmlSelector rowNode )
			{
				Console.WriteLine( "Parsing Row" );
			}
		}

		protected override void TestLogic()
		{
			/*/
			TestInfoManager.Instance.RegistClient( false, false, false );
			try
			{
				DataListManager.Instance.Load( "TEST_DATA", "" );
			}
			catch (System.Exception ex)
			{
				Log( ex.ToString() );
			}
			
			Log( "curr : {0} - {1} - {2}", TestInfoManager.Instance.reload_count, TestInfoManager.Instance.DATA_ID, TestInfoManager.Instance.GetHashCode() );


			/**/

			/**/
			LogTest( "Regist" );
			TestInfoManager.MakeInstance();
			//TestInfoManager.Instance.STATIC_PATH = "_config/TestTBL.xml";

			I18NTextMultiLanguage.MakeInstance();
			I18NTextMultiLanguage.SUPPORT_LANGUAGES.Add( "English" );
			I18NTextMultiLanguage.SUPPORT_LANGUAGES.Add( "Chinese" );
			I18NTextMultiLanguage.SUPPORT_LANGUAGES.Add( "Japanese" );

			TestInfoManager prev_instance = TestInfoManager.Instance;
			LogTest( "prev : {0} - {1}", prev_instance.reload_count, prev_instance.DATA_ID );
			prev_instance.reload_count += 10;

			foreach( DataReloader.ReloadInfo info in DataReloader.Instance.ReloadInfoList )
				LogTest( "{0}", info.data_id );

			WaitKey();

			List<string> reload_list = new List<string>()
			{
				"TEST_DATA",
				"SERVER_STRING",
			};

			string response = DataReloader.Instance.ReloadData( reload_list );
			LogTest( "response : " + response );

			TestInfoManager curr_instance = TestInfoManager.Instance;
			LogTest( "prev : {0} - {1} - {2}", prev_instance.reload_count, prev_instance.DATA_ID, prev_instance.GetHashCode() );
			LogTest( "curr : {0} - {1} - {2}", curr_instance.reload_count, curr_instance.DATA_ID, curr_instance.GetHashCode() );

			WaitKey();

			LogTest( "curr : {0} - {1} - {2}", TestInfoManager.Instance.reload_count, TestInfoManager.Instance.DATA_ID, TestInfoManager.Instance.GetHashCode() );

			WaitKey();

			/**/
		}
	}
}
