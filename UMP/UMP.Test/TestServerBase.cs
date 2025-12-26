using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMF.Core;
using UMP.Server;

namespace UMP.Test
{
	public abstract class TestBase
	{
		public void Start()
		{
			try
			{
				PreStart();
				TestStart();
			}
			catch (System.Exception ex)
			{
				Console.WriteLine( ex.ToString() );
			}
			
			Console.WriteLine( "_______ Press any key finished!_____" );
			Console.ReadKey();
		}
		public abstract void TestStart();


		protected void LoadGlobalConfig(string env_path)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine( "CONFIG_ROOT = " );
			sb.AppendLine( $"{GlobalConfig.ENV_CONFIG_PATH_KEY} = {env_path}" );
			sb.AppendLine( $"{GlobalConfig.ENV_DB_PATH_KEY} = {env_path}" );
			sb.AppendLine( $"{GlobalConfig.ENV_NET_PATH_KEY} = {env_path}" );
			sb.AppendLine( $"{GlobalConfig.I18N_PATH_KEY} = " );
			sb.AppendLine( $"{GlobalConfig.EnvironmentType_KEY} = " );
			sb.AppendLine( $"{GlobalConfig.GlobalType_KEY} = " );

			GlobalConfig.Property.LoadProperty( "", sb.ToString() );
			GlobalConfig.Reset();
		}

		protected virtual void PreStart()
		{
			AppConfig.MakeInstance();
			AppConfig.Instance.STATIC_PATH = "_server_config/template_AppConfig.xml";
			LocalizationConfig.MakeInstance();
			LocalizationConfig.Instance.STATIC_PATH = "_server_config/template_LocalizationConfig.xml";
		}
	}
}
