using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Threading;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Xml;
using System.Web;
using System.Data;
using MPX_WebCallback.Models;
using MPX_WebCallback.Common;
using MySql.Data.MySqlClient;
using NLog;

namespace MPX_WebCallback.Controllers
{
	public class QKController : ApiController
    {
		const string SUCCESS = "SUCCESS";
		const string FAILED = "FAILED";

		const string DB_CONN = "QKDBConnection";

		static Logger logger = LogManager.GetCurrentClassLogger();

		//------------------------------------------------------------------------		
		/*
			POST /api/v1/qk/pcallback HTTP/1.1
			HOST: localhost:62828
			content-type: application/x-www-form-urlencoded
			content-length: 1523

			nt_data=@108@119@174@165@158@82@167@152@171@166@161@162@159@115@90@106@94@103@83@85@154@166@156@161@155@160@166@155@118@90@139@141@118@101@110@90@82@165@165@148@167@151@153@159@160@164@157@118@82@165@160@87@116@118@117@165@162@176@165@163@168@166@169@152@157@157@169@171@147@153@150@113@117@160@157@166@164@151@159@158@110@115@166@158@153@118@109@110@102@172@161@152@119@116@162@168@151@161@164@151@160@147@158@152@119@164@169@164@162@167@169@117@95@163@160@156@158@166@152@160@152@164@157@114@117@167@171@173@143@167@168@156@151@164@144@161@168@113@116@98@160@171@172@152@159@169@149@154@167@151@167@161@117@115@167@166@157@157@168@152@158@167@116@104@99@98@97@101@105@100@108@100@98@102@109@106@101@106@100@101@102@105@106@100@111@103@107@103@117@103@165@171@148@157@168@151@160@161@111@111@169@148@177@146@165@159@165@158@110@105@97@102@105@101@106@99@100@103@109@84@106@109@112@108@99@114@103@108@110@97@161@148@178@146@172@156@158@155@118@117@145@164@160@170@163@172@119@98@101@103@105@112@104@153@163@168@165@166@170@118@110@165@165@148@173@168@171@113@97@114@103@172@164@152@165@170@168@118@117@151@175@171@170@149@172@151@166@154@162@153@163@171@112@170@169@171@117@98@157@171@165@168@153@172@143@167@146@167@150@165@172@112@115@102@165@153@172@171@151@160@149@118@114@103@165@157@170@160@168@162@166@166@144@163@157@172@163@152@152@154@115&sign=@104@155@109@158@105@104@104@106@111@103@112@103@148@152@104@158@149@111@101@151@102@111@156@149@155@112@113@105@154@110@108@106&md5Sign=07dae8d71374651c1f0ee4cc16a37f6e
		*/
		[HttpPost]
		[Route("api/v1/qk/pcallback")]
		[FilterIP( ConfigurationKeyAllowedSingleIPs = "QCB_AllowedSingleIPs", ConfigurationKeyDeniedSingleIPs = "QCB_DeniedSingleIPs" )]
		public async Task<HttpResponseMessage> PayCallback( [FromBody]QKModel.CallbackData value )
		{
			string md5_key = ConfigurationManager.AppSettings.Get( "QKPayMD5Key" );
			string callback_key = ConfigurationManager.AppSettings.Get( "QKPayCallbackKey" );

			QKModel.CallbackReturnData result = await _PayCallbackProcess( value, md5_key, callback_key, 1 );

			HttpResponseMessage response = Request.CreateResponse( HttpStatusCode.OK );
			response.Content = new StringContent( result.result );
			return response;
		}
		[HttpPost]
		[Route( "api/v1/qk/pcallback2" )]
		[FilterIP( ConfigurationKeyAllowedSingleIPs = "QCB_AllowedSingleIPs", ConfigurationKeyDeniedSingleIPs = "QCB_DeniedSingleIPs" )]
		public async Task<HttpResponseMessage> PayCallback2( [FromBody]QKModel.CallbackData value )
		{
			string md5_key = ConfigurationManager.AppSettings.Get( "QKPayMD5Key2" );
			string callback_key = ConfigurationManager.AppSettings.Get( "QKPayCallbackKey2" );

			QKModel.CallbackReturnData result = await _PayCallbackProcess( value, md5_key, callback_key, 2 );

			HttpResponseMessage response = Request.CreateResponse( HttpStatusCode.OK );
			response.Content = new StringContent( result.result );
			return response;
		}

		//------------------------------------------------------------------------		
		async Task<QKModel.CallbackReturnData> _PayCallbackProcess( QKModel.CallbackData input_data, string md5_key, string callback_key, int call_id)
		{
			QKModel.CallbackReturnData result_data = new QKModel.CallbackReturnData();
			result_data.result = FAILED;

			bool is_error = false;
			string error_reason = "";

			string s_nt_data = "";
			string s_sign = "";
			string s_md5sign = "";

			if( input_data == null )
			{
				error_reason = ":Input IS NULL";
				is_error = true;
			}

			if( is_error == false )
			{
				input_data.CheckNull();
				s_nt_data = input_data.nt_data;
				s_sign = input_data.sign;
				s_md5sign = input_data.md5Sign;

				string md5local = Util.MD5String.GetHashString( input_data.nt_data + input_data.sign + md5_key );
				if( input_data.md5Sign.ToLower() != md5local.ToLower() )
				{
					is_error = true;
					error_reason = ":MD5Sign Invalid:" + input_data.md5Sign + ":::" + md5local + ":::" + md5_key;					
				}

				if( is_error == false )
				{
					QKModel.PayCallbackNTData nt_data = null;
					string nt_data_decode = decode( input_data.nt_data, callback_key );
					try
					{
						XmlDocument doc = new XmlDocument();
						doc.LoadXml( nt_data_decode );
						nt_data = new QKModel.PayCallbackNTData( doc, nt_data_decode );
					}
					catch( System.Exception ex )
					{
						is_error = true;
						error_reason = ":NTData Exception:" + ex.Message;
						logger.Fatal( ex.ToString() );
					}

					if( nt_data == null || nt_data.IsInvalid() )
					{
						is_error = true;
						error_reason = ":NTData Invalid:" + nt_data_decode;
					}

					// DB process
					if( is_error == false )
					{
						try
						{
							DBConn db = new DBConn( ConfigurationManager.ConnectionStrings[DB_CONN].ConnectionString );
							db.AddParameter( "_is_test", MySqlDbType.VarChar, 500, nt_data.is_test, ParameterDirection.Input );
							db.AddParameter( "_channel", MySqlDbType.VarChar, 500, nt_data.channel, ParameterDirection.Input );
							db.AddParameter( "_channel_uid", MySqlDbType.VarChar, 1000, nt_data.channel_uid, ParameterDirection.Input );
							db.AddParameter( "_game_order", MySqlDbType.VarChar, 1000, nt_data.game_order, ParameterDirection.Input );
							db.AddParameter( "_order_no", MySqlDbType.VarChar, 1000, nt_data.order_no, ParameterDirection.Input );
							db.AddParameter( "_pay_time", MySqlDbType.VarChar, 100, nt_data.pay_time, ParameterDirection.Input );
							db.AddParameter( "_amount", MySqlDbType.VarChar, 500, nt_data.amount, ParameterDirection.Input );
							db.AddParameter( "_status", MySqlDbType.VarChar, 1000, nt_data.status, ParameterDirection.Input );
							db.AddParameter( "_extras_params", MySqlDbType.VarChar, 2000, nt_data.extras_params, ParameterDirection.Input );
							db.AddParameter( "_s_nt_data", MySqlDbType.VarChar, 3500, s_nt_data, ParameterDirection.Input );
							db.AddParameter( "_s_sign", MySqlDbType.VarChar, 1000, s_sign, ParameterDirection.Input );
							db.AddParameter( "_s_md5sign", MySqlDbType.VarChar, 2000, s_md5sign, ParameterDirection.Input );
							db.AddParameter( "_s_nt_data_decode_xml", MySqlDbType.VarChar, 2000, nt_data.org_xml, ParameterDirection.Input );
							db.AddParameter( "_call_id", MySqlDbType.Int32, 0, call_id, ParameterDirection.Input );

							List<DBReturn> db_result = await db.ExecuteAsyncSP<DBReturn>( "sp_qkpaid_callback_create" );
							if( db_result[0].is_success == 0 )
							{
								result_data.result = SUCCESS;
							}
							else
							{
								is_error = true;
								error_reason = ":DB Error:" + db_result[0].is_success.ToString();
							}
						}
						catch( System.Exception ex )
						{
							is_error = true;
							error_reason = ":DB Exception:" + ex.Message;
							logger.Fatal( ex.ToString() + string.Format("==[nt]{0} [sign]{1} [md5]{2}", s_nt_data, s_sign, s_md5sign ) );
						}
					}
				}
			}

			if( is_error )
			{
				try
				{
					DBConn db_error = new DBConn( ConfigurationManager.ConnectionStrings[DB_CONN].ConnectionString );
					db_error.AddParameter( "_s_nt_data", MySqlDbType.VarChar, 3500, s_nt_data, ParameterDirection.Input );
					db_error.AddParameter( "_s_sign", MySqlDbType.VarChar, 1000, s_sign, ParameterDirection.Input );
					db_error.AddParameter( "_s_md5sign", MySqlDbType.VarChar, 2000, s_md5sign, ParameterDirection.Input );
					db_error.AddParameter( "_reason", MySqlDbType.VarChar, 500, error_reason, ParameterDirection.Input );
					db_error.AddParameter( "_call_id", MySqlDbType.Int32, 0, call_id, ParameterDirection.Input );

					await db_error.ExecuteAsyncSP<DBReturn>( "sp_qkpaid_invalid_create" );
				}
				catch (System.Exception ex)
				{
					// DO NOTHING
					logger.Fatal( ex.ToString() + string.Format( "==[nt]{0} [sign]{1} [md5]{2} [call]{3}", s_nt_data, s_sign, s_md5sign, call_id ) );
				}
			}

			return result_data;
		}

		//------------------------------------------------------------------------		
		[HttpPost]
		[Route( "api/v1/qkgame/pcallback" )]
		[FilterIP( ConfigurationKeyAllowedSingleIPs = "QCB_AllowedSingleIPs", ConfigurationKeyDeniedSingleIPs = "QCB_DeniedSingleIPs" )]
		public async Task<HttpResponseMessage> QuickGamePayCallback( [FromBody]QKModel.CallbackData value )
		{
			string md5_key = ConfigurationManager.AppSettings.Get( "QKGamePayMD5Key" );
			string callback_key = ConfigurationManager.AppSettings.Get( "QKGamePayCallbackKey" );

			// qkgame call id begin 100
			QKModel.CallbackReturnData result = await _QuickGamePayCallbackProcess( value, md5_key, callback_key, 1 );

			HttpResponseMessage response = Request.CreateResponse( HttpStatusCode.OK );
			response.Content = new StringContent( result.result );
			return response;
		}

		//------------------------------------------------------------------------		
		async Task<QKModel.CallbackReturnData> _QuickGamePayCallbackProcess( QKModel.CallbackData input_data, string md5_key, string callback_key, int call_id )
		{
			QKModel.CallbackReturnData result_data = new QKModel.CallbackReturnData();
			result_data.result = FAILED;

			bool is_error = false;
			string error_reason = "";

			string s_nt_data = "";
			string s_sign = "";
			string s_md5sign = "";

			if( input_data == null )
			{
				error_reason = ":Input IS NULL";
				is_error = true;
			}

			if( is_error == false )
			{
				input_data.CheckNull();
				s_nt_data = input_data.nt_data;
				s_sign = input_data.sign;
				s_md5sign = input_data.md5Sign;

				string md5local = Util.MD5String.GetHashString( input_data.nt_data + input_data.sign + md5_key );
				if( input_data.md5Sign.ToLower() != md5local.ToLower() )
				{
					is_error = true;
					error_reason = ":MD5Sign Invalid:" + input_data.md5Sign + ":::" + md5local + ":::" + md5_key;
				}

				if( is_error == false )
				{
					QKModel.PayCallbackNTDataQuickGame nt_data = null;
					string nt_data_decode = decode( input_data.nt_data, callback_key );
					try
					{
						XmlDocument doc = new XmlDocument();
						doc.LoadXml( nt_data_decode );
						nt_data = new QKModel.PayCallbackNTDataQuickGame( doc, nt_data_decode );
					}
					catch( System.Exception ex )
					{
						is_error = true;
						error_reason = ":NTData Exception:" + ex.Message;
						logger.Fatal( ex.ToString() );
					}

					if( nt_data == null || nt_data.IsInvalid() )
					{
						is_error = true;
						error_reason = ":NTData Invalid:" + nt_data_decode;
					}

					// DB process
					if( is_error == false )
					{
						try
						{
							DBConn db = new DBConn( ConfigurationManager.ConnectionStrings[DB_CONN].ConnectionString );
							db.AddParameter( "_uid", MySqlDbType.VarChar, 1000, nt_data.uid, ParameterDirection.Input );
							db.AddParameter( "_login_name", MySqlDbType.VarChar, 1000, nt_data.login_name, ParameterDirection.Input );
							db.AddParameter( "_out_order_no", MySqlDbType.VarChar, 1000, nt_data.out_order_no, ParameterDirection.Input );
							db.AddParameter( "_order_no", MySqlDbType.VarChar, 1000, nt_data.order_no, ParameterDirection.Input );
							db.AddParameter( "_pay_time", MySqlDbType.VarChar, 100, nt_data.pay_time, ParameterDirection.Input );
							db.AddParameter( "_amount", MySqlDbType.VarChar, 500, nt_data.amount, ParameterDirection.Input );
							db.AddParameter( "_status", MySqlDbType.VarChar, 1000, nt_data.status, ParameterDirection.Input );
							db.AddParameter( "_extras_params", MySqlDbType.VarChar, 2000, nt_data.extras_params, ParameterDirection.Input );
							db.AddParameter( "_s_nt_data", MySqlDbType.VarChar, 3500, s_nt_data, ParameterDirection.Input );
							db.AddParameter( "_s_sign", MySqlDbType.VarChar, 1000, s_sign, ParameterDirection.Input );
							db.AddParameter( "_s_md5sign", MySqlDbType.VarChar, 2000, s_md5sign, ParameterDirection.Input );
							db.AddParameter( "_s_nt_data_decode_xml", MySqlDbType.VarChar, 2000, nt_data.org_xml, ParameterDirection.Input );
							db.AddParameter( "_call_id", MySqlDbType.Int32, 0, call_id, ParameterDirection.Input );

							List<DBReturn> db_result = await db.ExecuteAsyncSP<DBReturn>( "sp_qkgamepaid_callback_create" );
							if( db_result[0].is_success == 0 )
							{
								result_data.result = SUCCESS;
							}
							else
							{
								is_error = true;
								error_reason = ":DB Error:" + db_result[0].is_success.ToString();
							}
						}
						catch( System.Exception ex )
						{
							is_error = true;
							error_reason = ":DB Exception:" + ex.Message;
							logger.Fatal( ex.ToString() + string.Format( "==[nt]{0} [sign]{1} [md5]{2}", s_nt_data, s_sign, s_md5sign ) );
						}
					}
				}
			}

			if( is_error )
			{
				try
				{
					DBConn db_error = new DBConn( ConfigurationManager.ConnectionStrings[DB_CONN].ConnectionString );
					db_error.AddParameter( "_s_nt_data", MySqlDbType.VarChar, 3500, s_nt_data, ParameterDirection.Input );
					db_error.AddParameter( "_s_sign", MySqlDbType.VarChar, 1000, s_sign, ParameterDirection.Input );
					db_error.AddParameter( "_s_md5sign", MySqlDbType.VarChar, 2000, s_md5sign, ParameterDirection.Input );
					db_error.AddParameter( "_reason", MySqlDbType.VarChar, 500, error_reason, ParameterDirection.Input );
					db_error.AddParameter( "_call_id", MySqlDbType.Int32, 0, call_id, ParameterDirection.Input );

					await db_error.ExecuteAsyncSP<DBReturn>( "sp_qkgamepaid_invalid_create" );
				}
				catch( System.Exception ex )
				{
					// DO NOTHING
					logger.Fatal( ex.ToString() + string.Format( "==[nt]{0} [sign]{1} [md5]{2} [call]{3}", s_nt_data, s_sign, s_md5sign, call_id ) );
				}
			}

			return result_data;
		}

		//------------------------------------------------------------------------		
		string decode( string src, string key )
		{
			if( src == null || src.Length == 0 )
			{
				return src;
			}

			string pattern = "\\d+";
			MatchCollection results = Regex.Matches( src, pattern );

			ArrayList list = new ArrayList();
			for( int i = 0; i < results.Count; i++ )
			{
				try
				{
					String group = results[i].ToString();
					list.Add( (Object)group );
				}
				catch( Exception e )
				{
					logger.Fatal( e.ToString() );
					return src;
				}
			}

			if( list.Count > 0 )
			{
				try
				{
					byte[] data = new byte[list.Count];
					byte[] keys = System.Text.Encoding.Default.GetBytes( key );

					for( int i = 0; i < data.Length; i++ )
					{
						data[i] = (byte)( Convert.ToInt32( list[i] ) - ( 0xff & Convert.ToInt32( keys[i % keys.Length] ) ) );
					}
					return System.Text.Encoding.Default.GetString( data );
				}
				catch( Exception e )
				{
					logger.Fatal( e.ToString() );
					return src;
				}
			}
			else
			{
				return src;
			}
		}

		//------------------------------------------------------------------------		
		/*
		POST /api/v1/qk/pv HTTP/1.1
		HOST: localhost:62828
		content-type: application/json
		content-length: 78

		channel=test&channel_uid=test&game_order=test&order_no=test&extras_params=test
		*/

		[HttpPost]
		[Route("api/v1/qk/pv")]
		[FilterIP( ConfigurationKeyAllowedSingleIPs = "QPV_AllowedSingleIPs", ConfigurationKeyDeniedSingleIPs = "QPV_DeniedSingleIPs" )]
		public async Task<IHttpActionResult> PayVerify([FromBody] QKModel.PayVerifyInputData input_data)
		{
			QKModel.PayVerifyResultData result_data = new QKModel.PayVerifyResultData();
			result_data.error = 0;
			result_data.error_msg = "";

			bool restore_test = false;
			string restore_test_value = ConfigurationManager.AppSettings.Get( "RestoreTest" );
			if( string.IsNullOrEmpty( restore_test_value ) == false && restore_test_value == "true" )
				restore_test = true;

			if( input_data == null )
			{
				result_data.error = -2;
				result_data.error_msg = "input is null";
			}
			else if( restore_test )
			{
				result_data.error = -3;
				result_data.error_msg = "restore test";
			}
			else
			{
				try
				{
					DBConn db = new DBConn( ConfigurationManager.ConnectionStrings[DB_CONN].ConnectionString );
					db.AddParameter( "_channel", MySqlDbType.VarChar, 500, input_data.channel, ParameterDirection.Input );
					db.AddParameter( "_channel_uid", MySqlDbType.VarChar, 1000, input_data.channel_uid, ParameterDirection.Input );
					db.AddParameter( "_game_order", MySqlDbType.VarChar, 1000, input_data.game_order, ParameterDirection.Input );
					db.AddParameter( "_order_no", MySqlDbType.VarChar, 1000, input_data.order_no, ParameterDirection.Input );
					db.AddParameter( "_extras_params", MySqlDbType.VarChar, 2000, input_data.extras_params, ParameterDirection.Input );
					db.AddParameter( "_call_id", MySqlDbType.Int32, 0, input_data.call_id, ParameterDirection.Input );

					List<QKModel.DBSelectData> db_result = await db.ExecuteAsyncSP<QKModel.DBSelectData>( "sp_qkpaid_callback_select" );
					if( db_result == null || db_result.Count <= 0 )
					{
						result_data.error = 1;
						result_data.error_msg = "Not Found";
					}
					else
					{
						result_data.Set( db_result[0] );
					}
				}
				catch( System.Exception ex )
				{
					result_data.error = -1;
					if( ConfigurationManager.AppSettings.Get( "ErrDetail" ) == "true" )
						result_data.error_msg = ex.ToString();
					else
						result_data.error_msg = ex.Message;
					logger.Fatal( ex.ToString() + "[in]" + input_data.game_order );
				}
			}

			return Ok( result_data );
		}

		[HttpPost]
		[Route( "api/v1/qkgame/pv" )]
		[FilterIP( ConfigurationKeyAllowedSingleIPs = "QPV_AllowedSingleIPs", ConfigurationKeyDeniedSingleIPs = "QPV_DeniedSingleIPs" )]
		public async Task<IHttpActionResult> QuickGamePayVerify( [FromBody] QKModel.QuickGamePayVerifyInputData input_data )
		{
			QKModel.QuickGamePayVerifyResultData result_data = new QKModel.QuickGamePayVerifyResultData();
			result_data.error = 0;
			result_data.error_msg = "";

			bool restore_test = false;
			string restore_test_value = ConfigurationManager.AppSettings.Get( "RestoreTest" );
			if( string.IsNullOrEmpty( restore_test_value ) == false && restore_test_value == "true" )
				restore_test = true;

			if( input_data == null )
			{
				result_data.error = -2;
				result_data.error_msg = "input is null";
			}
			else if( restore_test )
			{
				result_data.error = -3;
				result_data.error_msg = "restore test";
			}
			else
			{
				try
				{
					DBConn db = new DBConn( ConfigurationManager.ConnectionStrings[DB_CONN].ConnectionString );
					db.AddParameter( "_uid", MySqlDbType.VarChar, 1000, input_data.uid, ParameterDirection.Input );
					db.AddParameter( "_out_order_no", MySqlDbType.VarChar, 1000, input_data.out_order_no, ParameterDirection.Input );
					db.AddParameter( "_order_no", MySqlDbType.VarChar, 1000, input_data.order_no, ParameterDirection.Input );
					db.AddParameter( "_extras_params", MySqlDbType.VarChar, 2000, input_data.extras_params, ParameterDirection.Input );
					db.AddParameter( "_call_id", MySqlDbType.Int32, 0, input_data.call_id, ParameterDirection.Input );

					List<QKModel.QuickGameDBSelectData> db_result = await db.ExecuteAsyncSP<QKModel.QuickGameDBSelectData>( "sp_qkgamepaid_callback_select" );
					if( db_result == null || db_result.Count <= 0 )
					{
						result_data.error = 1;
						result_data.error_msg = "Not Found";
					}
					else
					{
						result_data.Set( db_result[0] );
					}
				}
				catch( System.Exception ex )
				{
					result_data.error = -1;
					if( ConfigurationManager.AppSettings.Get( "ErrDetail" ) == "true" )
						result_data.error_msg = ex.ToString();
					else
						result_data.error_msg = ex.Message;
					logger.Fatal( ex.ToString() + "[in]" + input_data.out_order_no );
				}
			}

			return Ok( result_data );
		}
	}
}
