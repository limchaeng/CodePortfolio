using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Collections;
using System.Globalization;
using MNS;
using Paging;
using System.Drawing;
using System.Text;

public partial class Pages_Wemix_WemixAirdropSend : System.Web.UI.Page
{
	//--{{ 뒤로가기, 새로고침 시 이벤트 재발생 방지
	private bool _refreshState;
	private bool _isRefresh;
	public bool IsRefresh
	{
		get { return _isRefresh; }
	}
	protected override void LoadViewState( object savedState )
	{
		object[] allStates = (object[])savedState;
		base.LoadViewState( allStates[0] );
		_refreshState = (bool)allStates[1];
		_isRefresh = _refreshState == MNS.XMLUtil.SafeParse<bool>( Session["__ISREFRESH"], false );
	}
	protected override object SaveViewState()
	{
		Session["__ISREFRESH"] = _refreshState;
		object[] allStates = new object[2];
		allStates[0] = base.SaveViewState();
		allStates[1] = !_refreshState;
		return allStates;
	}
	//--}}

	protected void Page_Load( object sender, EventArgs e )
	{
		if( !IsPostBack )
		{
			UpdateInfo();
		}
		gdvTargetList_Show();
		gdvLoadList_Show();
		ButtonEnable( true );
	}

	void UpdateInfo()
	{
		tbxEnvironment.Text = ConfigManager.GetAppSetting( WemixAPI.KEY_ENV );
		tbxContractAddress.Text = "";
		tbxVKSAddress.Text = "";
		tbxFundAddress.Text = "";
		
		tbxRemainTokens.Text = "";
		tbxRemainTokensWei.Text = "";
		tbxRemainTokensWeiHide.Text = "";

		WemixAPI api = new WemixAPI();
		WemixAPI.Res_getPendingAmount getPendingAmount = api.getPendingAmount();
		if( getPendingAmount.IsValid() )
		{
			tbxContractAddress.Text = api.ADDRESS_CONTRACT_FT;
			tbxVKSAddress.Text = api.ADDRESS_VKS;
			tbxFundAddress.Text = api.AIRDROP_FUNDS_SOURCE_ADDRESS;

			tbxRemainTokensWei.Text = decimal.Parse( getPendingAmount.result[0] ).ToString( "N0" );
			tbxRemainTokensWeiHide.Text = decimal.Parse( getPendingAmount.result[0] ).ToString();
			tbxRemainTokens.Text = WemixUTIL.FromWei( getPendingAmount.result[0] ).ToString( "N0" );
		}
		else
		{
			tbxContractAddress.Text = getPendingAmount.GetError();
		}

		WebUtil.DebugLog( this, false, api.UTIL.GetLog );
	}

	private void gdvTargetList_Show()
	{
		try
		{
			string[] splits = tbxTargetInputPerLine.Text.Split( new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries );

			int count = 0;
			DataSet ds = new DataSet();
			ds.Tables.Add( "gdvTargetList" );
			ds.Tables["gdvTargetList"].Columns.Add( "target_address" );
			if( splits != null && splits.Length > 0 )
			{
				for( int i = 0; i < splits.Length; i++ )
				{
					string address = splits[i].Trim();
					if( string.IsNullOrEmpty( address ) == false )
					{
						ds.Tables["gdvTargetList"].Rows.Add( address );
						count += 1;
					}
				}
			}
			else
			{
				ds.Tables["gdvTargetList"].Rows.Add( "" );
			}
			gdvTargetList.DataSource = ds;
			gdvTargetList.DataBind();

			lblTargetaddressinfoCount.Text = string.Format( "대상 주소 확인 ( 지갑수 : {0} )", count );
		}
		catch( Exception ex )
		{
			WebUtil.Alert( this, "db error", Resources.ResourceTool.lblDBError + ex.ToString() );// "Failed to load DB data"); // DB 데이터를 불러오지 못했습니다.
		}
	}

	protected void gdvTargetList_RowDataBound( object sender, GridViewRowEventArgs e )
	{

	}

	protected void gdvTargetList_PageIndexChanging( object sender, GridViewPageEventArgs e )
	{
		gdvTargetList.PageIndex = e.NewPageIndex;
		gdvTargetList_Show();
	}

	decimal GetTotalSendAmount()
	{
		decimal amount = XMLUtil.SafeParse<decimal>( tbxSendAmount.Text, 0 );
		int user_count = XMLUtil.SafeParse<int>( tbxSendUserTotalCount.Text, 0 );

		return amount * user_count;
	}

	List<string> CollectValidAddress(ref List<string> invalid_list )
	{
		List<string> address_list = tbxTargetInputPerLine.Text.Split( new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
		if( address_list == null || address_list.Count <= 0 )
			return null;

		address_list.RemoveAll( a => string.IsNullOrEmpty( a.Trim() ) );

		List<string> valid_list = new List<string>();
		foreach( string address in address_list )
		{
			string t_addr = address.Trim();
			if( WemixUTIL.IsAddressValid( t_addr ) == false )
			{
				if( invalid_list == null )
					invalid_list = new List<string>();

				invalid_list.Add( t_addr );
			}
			else
			{
				valid_list.Add( t_addr );
			}
		}

		return valid_list;
	}

	protected void btnInputAddress_Click( object sender, EventArgs e )
	{
		tbxSendAmountWei.Text = WemixUTIL.ToWei( tbxSendAmount.Text ).ToString("N0");
		tbxSendAmountWeiHide.Text = WemixUTIL.ToWei( tbxSendAmount.Text ).ToString( "" );

		List<string> invalid_list = null;
		List<string> address_list = CollectValidAddress( ref invalid_list );
		if( invalid_list != null )
		{
			SendInfoReset();

			StringBuilder sb = new StringBuilder();
			foreach( string invalid in invalid_list )
			{
				sb.AppendLine( invalid );
			}

			tbxInvalidAddressText.Text = sb.ToString();

			Page.ClientScript.RegisterStartupScript( this.GetType(), "open-modal", "openAddressInvalidPopup();", true );
			return;
		}

		if( address_list == null || address_list.Count <= 0 )
			SendInfoReset();

		if( address_list != null )
			tbxSendUserTotalCount.Text = address_list.Count.ToString();

		decimal total_amount = GetTotalSendAmount();
		decimal remain_tokens = XMLUtil.SafeParse<decimal>( tbxRemainTokens.Text, 0 );

		tbxSendTotalAmount.Text = total_amount.ToString( "N0" );
		tbxSendTotalAmountWei.Text = WemixUTIL.ToWei( tbxSendTotalAmount.Text ).ToString("N0");
		tbxSendTotalAmountWeiHide.Text = WemixUTIL.ToWei( tbxSendTotalAmount.Text ).ToString();
		tbxSendRemainAmount.Text = ( remain_tokens - total_amount ).ToString("N0");
		tbxSendRemainAmountWei.Text = WemixUTIL.ToWei( tbxSendRemainAmount.Text ).ToString("N0");

		if( address_list != null && address_list.Count > 0 )
		{
			gdvTargetList.PageIndex = 0;
			gdvTargetList_Show();
		}
	}

	void SendInfoReset()
	{
		tbxSendUserTotalCount.Text = "";
		tbxSendTotalAmount.Text = "";
		tbxSendTotalAmountWei.Text = "";
		tbxSendTotalAmountWeiHide.Text = "";
		tbxSendRemainAmount.Text = "";
		tbxSendRemainAmountWei.Text = "";
	}

	protected void btnClear_Click( object sender, EventArgs e )
	{
		SendInfoReset();

		tbxSendAmount.Text = "";
		tbxSendAmountWei.Text = "";
		tbxSendAmountWeiHide.Text = "";

		tbxTargetInputPerLine.Text = "";

		gdvTargetList.PageIndex = 0;
		gdvTargetList_Show();

		tbxLoadInfo.Text = "";
		tbxLoadInfoIDX.Text = "";
		tbxSaveDescriptionInput.Text = "";
	}

	protected void lbtnTargetAddressDetail_Command( object sender, CommandEventArgs e )
	{
		if( !_isRefresh )
		{
			string address = e.CommandArgument.ToString();

			tbxAddrviewAddressInfo.Text = "";
			tbxAddrviewResponseText.Text = "";
			if( string.IsNullOrEmpty( address ) == false )
			{
				WemixAPI api = new WemixAPI();
				WemixAPI.Res_contract_callContract_balanceOf response = api.contract_callContract_balanceOf( address );
				if( response.IsValid() )
				{
					tbxAddrviewAddressInfo.Text = string.Format( "balanceOf : {0}", response.result[0] );
				}
				else
				{
					tbxAddrviewAddressInfo.Text = response.GetError();
				}

				tbxAddrviewResponseText.Text = response.original_response_text;

				WebUtil.DebugLog( this, false, api.UTIL.GetLog );
			}

			lblAddrviewAddress.Text = address;
			Page.ClientScript.RegisterStartupScript( this.GetType(), "open-modal", "openAddressViewPopup();", true );
		}
	}

	void ButtonEnable( bool bEnable )
	{
		btnInputAddress.Enabled = bEnable;
		btnClear.Enabled = bEnable;

		if( WemixUTIL.HasAuth( Session, WemixUTIL.eAuthFlags.Setup ) )
		{
			btnDoSave.Enabled = bEnable;
		}
		else
		{
			btnDoSave.Enabled = false;
		}

		if( btnDoSave.Enabled )
			btnDoSave.BackColor = Color.FromArgb( 255, 153, 102 );
		else
			btnDoSave.BackColor = Color.Gray;

		if( WemixUTIL.HasAuth( Session, WemixUTIL.eAuthFlags.Exec ) )
		{
			btnDoSend.Enabled = bEnable;
		}
		else
		{
			btnDoSend.Enabled = false;
		}

		if( btnDoSend.Enabled )
		{
			btnDoSend.BackColor = Color.Red;
			btnDoSend.ForeColor = Color.White;
		}
		else
		{
			btnDoSend.BackColor = Color.Gray;
			btnDoSend.ForeColor = Color.Black;
		}
	}

	protected void btnDoSave_Click( object sender, EventArgs e )
	{
		ButtonEnable( false );
		DoSaveOrExecuted( true );
		ButtonEnable( true );
	}

	protected void btnDoSend_Click( object sender, EventArgs e )
	{
		ButtonEnable( false );
		DoSaveOrExecuted( false );
		ButtonEnable( true );
	}

	protected void DoSaveOrExecuted( bool is_save_only )
	{
		List<string> invalid_list = null;
		List<string> address_list = CollectValidAddress( ref invalid_list );
		if( address_list == null || address_list.Count <= 0  )
		{
			WebUtil.Alert( this, "warning", "지급할 주소가 비어 있습니다." );
			return;
		}
		if( invalid_list != null )
		{
			StringBuilder sb = new StringBuilder();
			foreach( string invalid in invalid_list )
			{
				sb.AppendLine( invalid );
			}

			tbxInvalidAddressText.Text = sb.ToString();

			Page.ClientScript.RegisterStartupScript( this.GetType(), "open-modal", "openAddressInvalidPopup();", true );
			return;
		}

		decimal total_amount = GetTotalSendAmount();
		if( total_amount <= 0 )
		{
			WebUtil.Alert( this, "warning", "지급수량이 0 입니다." );			
			return;
		}

		decimal remain_tokens = XMLUtil.SafeParse<decimal>( tbxRemainTokens.Text, 0 );
		if( remain_tokens - total_amount < 0 )
		{
			WebUtil.Alert( this, "warning", "지급할 토큰이 부족합니다. 토큰 예상 잔량을 확인해주세요." );
			return;
		}

		WemixAPI api = new WemixAPI();

		// step 0 : 지급시점의 토큰 잔량 체크
		WemixAPI.Res_getPendingAmount getPendingAmount = api.getPendingAmount();
		if( getPendingAmount.IsValid() == false )
		{
			APIErrorPopup( api, getPendingAmount );
			return;
		}

		if( decimal.Parse( getPendingAmount.result[0] ).ToString( "N0" ) != tbxRemainTokensWei.Text )
		{
			string error = string.Format( "화면의 토큰 잔량과 현재 잔량이 다릅니다. 확인 다시 시도해주세요.\n화면:{0} API:{1}", tbxRemainTokensWei.Text, getPendingAmount.result[0] );
			APIErrorPopup( api, error );
			return;
		}

		StringBuilder log_sb = new StringBuilder();

		string server_type = Session["server_type_text"].ToString();
		int tool_user_idx = XMLUtil.SafeParse<int>( Session["tool_user_idx"], 0 );
		string tool_user_id = Session["tool_user_id"].ToString();
		string tool_user_ip = GetIP();

		string contract_name = api.CONTRACT_NAME;
		string contract_address = api.ADDRESS_CONTRACT_FT;
		string vks_address = api.ADDRESS_VKS;
		string fund_resource_address = api.AIRDROP_FUNDS_SOURCE_ADDRESS;
		string org_token_count = tbxRemainTokensWeiHide.Text;
		string send_token_count = tbxSendAmountWeiHide.Text;
		int send_address_count = XMLUtil.SafeParse<int>( tbxSendUserTotalCount.Text, 0 );
		string send_addresses = "";
		foreach( string addr in address_list )
		{
			send_addresses += string.Format( "{0},", addr );
		}
		string description = tbxSaveDescriptionInput.Text;

		int save_index = XMLUtil.SafeParse<int>( tbxLoadInfoIDX.Text, 0 );

		log_sb.AppendLine( "== Wemix Basic info ==" );
		log_sb.AppendLine( string.Format( "server_type = {0}", server_type ) );
		log_sb.AppendLine( string.Format( "tool_user_idx = {0}", tool_user_idx ) );
		log_sb.AppendLine( string.Format( "tool_user_id = {0}", tool_user_id ) );
		log_sb.AppendLine( string.Format( "tool_user_ip = {0}", tool_user_ip ) );

		log_sb.AppendLine( string.Format( "environment = {0}", ConfigManager.GetAppSetting( WemixAPI.KEY_ENV ) ) );		
		log_sb.AppendLine( string.Format( "contract_name = {0}", contract_name ) );
		log_sb.AppendLine( string.Format( "contract_address = {0}", contract_address ) );
		log_sb.AppendLine( string.Format( "chain_name = {0}", api.CHAIN_NAME ) );

		log_sb.AppendLine( string.Format( "binder_url = {0}", api.URL_BINDER ) );
		log_sb.AppendLine( string.Format( "binder_jwt = {0}", api.JWT_BINDER ) );

		log_sb.AppendLine( string.Format( "vks_address = {0}", vks_address ) );
		log_sb.AppendLine( string.Format( "vks_url = {0}", api.URL_VKS ) );
		log_sb.AppendLine( string.Format( "vks_jwt = {0}", api.JWT_VKS ) );
		
		log_sb.AppendLine( string.Format( "fund_resource_address = {0}", fund_resource_address ) );
		
		log_sb.AppendLine( string.Format( "org_token_count = {0}", org_token_count ) );
		log_sb.AppendLine( string.Format( "send_token_count = {0}", send_token_count ) );

		log_sb.AppendLine( string.Format( "send_address_count = {0}", send_address_count ) );
		log_sb.AppendLine( string.Format( "send_addresses = {0}", send_addresses ) );
		
		log_sb.AppendLine( string.Format( "description = {0}", description ) );
		log_sb.AppendLine( string.Format( "save_index = {0}", save_index ) );

		if( is_save_only )
		{
			try
			{
				string connect_str = "Log_" + server_type + "_Connection";

				DBConn dbconn = new DBConn( ConfigManager.GetConnectionString( connect_str ) );

				dbconn.command.CommandText = "dbo.spt_ToolWemixAirdropSave_Update";
				dbconn.command.CommandType = CommandType.StoredProcedure;

				dbconn.SetParameter( "@save_index", System.Data.SqlDbType.Int, save_index );
				dbconn.SetParameter( "@save_time", System.Data.SqlDbType.DateTime2, DateTime.Now );

				dbconn.SetParameter( "@tool_user_idx", System.Data.SqlDbType.Int, tool_user_idx );
				dbconn.SetParameter( "@tool_user_id", System.Data.SqlDbType.NVarChar, 100, tool_user_id );
				dbconn.SetParameter( "@tool_user_ip", System.Data.SqlDbType.NVarChar, 100, tool_user_ip );

				//dbconn.SetParameter( "@contract_name", System.Data.SqlDbType.NVarChar, 100, contract_name );
				//dbconn.SetParameter( "@contract_address", System.Data.SqlDbType.NVarChar, 100, contract_address );
				//dbconn.SetParameter( "@vks_address", System.Data.SqlDbType.NVarChar, 100, vks_address );
				//dbconn.SetParameter( "@fund_resource_address", System.Data.SqlDbType.NVarChar, 100, fund_resource_address );
				//dbconn.SetParameter( "@org_token_count", System.Data.SqlDbType.NVarChar, -1, org_token_count );
				dbconn.SetParameter( "@send_token_count", System.Data.SqlDbType.NVarChar, -1, send_token_count );
				dbconn.SetParameter( "@send_address_count", System.Data.SqlDbType.Int, send_address_count );
				dbconn.SetParameter( "@send_addresses", System.Data.SqlDbType.NVarChar, -1, send_addresses );
				dbconn.SetParameter( "@description", System.Data.SqlDbType.NVarChar, -1, description );

				log_sb.AppendLine( "" );
				log_sb.AppendLine( "== DBDump ==" );
				log_sb.AppendLine( DBSqlDump.GetCommandDump( "spt_ToolWemixAirdropSave_Update", dbconn.command ) );

				dbconn.DBOpen();
				dbconn.ExecuteSPNonQuery();
				dbconn.DBClose();

				gdvLoadList_Show();
			}
			catch( System.Exception ex )
			{
				log_sb.AppendLine( "" );
				log_sb.AppendLine( "== Exception ==" );
				log_sb.AppendLine( ex.ToString() );

				WebUtil.DebugLog( this, false, ex.ToString() );
				WebUtil.Alert( this, "Warning", ex.Message );				
			}

			WemixUTIL.WriteLog( "WemixAirdropPreSave", "Wemix Airdrop PreSave data", log_sb.ToString() );
		}
		else
		{
			// 1: AirDrop을 할 ST 컨트랙트 address 정보를 요청합니다
			WemixAPI.Res_contract_contractAddress res_contract_contractAddress = api.contract_contractAddress();
			if( res_contract_contractAddress.IsValid() == false )
			{
				APIErrorPopup( api, res_contract_contractAddress );
				return;
			}

			if( res_contract_contractAddress.result != tbxContractAddress.Text )
			{
				string error = string.Format( "화면의 토큰 주소와 컨트랙트주소가 다릅니다. 확인 다시 시도해주세요.\n화면:{0} API:{1}", tbxContractAddress.Text, res_contract_contractAddress.result );
				APIErrorPopup( api, error );
				return;
			}
			// Util.Log( "2.contract_contractAddress address = {0}", res_contract_contractAddress.result );

			// 2. Binder를 통해 서명되지 않는 트랜잭션 생성(tx_makeUnsignedTx)을 요청하며, unsigned transaction, hash값을 반환 받습니다.
			List<WemixAPI.Req_tx_makeUnsignedTx_Targets> target_address_list = new List<WemixAPI.Req_tx_makeUnsignedTx_Targets>();
			foreach( string t_addr in address_list )
			{
				WemixAPI.Req_tx_makeUnsignedTx_Targets target_address = new WemixAPI.Req_tx_makeUnsignedTx_Targets();
				target_address.address = t_addr;
				target_address.amount = WemixUTIL.ToWei( tbxSendAmount.Text ).ToString();

				target_address_list.Add( target_address );
			}

			int nonce = 0;
			// 확인결과 tx_makeUnsignedTx요청시 1234를 0으로 해주시면 되겠습니다.
			// 1234를 하게되면 1234번째 전송하는 tx라고 생각을 하기 떄문에 앞에 1233까지 tx가 전송되지 않으면 처리가 안됩니다. 따라서 0으로 해주시면 순차적으로 전송되니 참고 부탁드립니다.
			WemixAPI.Res_tx_makeUnsignedTx res_tx_makeUnsignedTx = api.tx_makeUnsignedTx( res_contract_contractAddress.result, target_address_list, nonce );
			if( res_tx_makeUnsignedTx.IsValid() == false )
			{
				log_sb.AppendLine( "" );
				log_sb.AppendLine( "== Wemix API Process ==" );
				log_sb.AppendLine( api.UTIL.GetLog );
				WemixUTIL.WriteLog( "WemixAirdropFailed", "Wemix Airdrop Executed data", log_sb.ToString() );

				APIErrorPopup( api, res_tx_makeUnsignedTx );
				return;
			}
			//Util.Log( "3.tx_makeUnsignedTx hash = {0}", res_tx_makeUnsignedTx.result.hash );

			// 3.Validator-Keystore를 통해 <3>단계에서 생성한 transaction에 대한 Hash에 서명합니다.
			WemixAPI.Res_vks_sign res_vks_sign = api.vks_sign( res_tx_makeUnsignedTx.result.hash );
			if( res_vks_sign.IsValid() == false )
			{
				log_sb.AppendLine( "" );
				log_sb.AppendLine( "== Wemix API Process ==" );
				log_sb.AppendLine( api.UTIL.GetLog );
				WemixUTIL.WriteLog( "WemixAirdropFailed", "Wemix Airdrop Executed data", log_sb.ToString() );

				APIErrorPopup( api, res_vks_sign );
				return;
			}
			//Util.Log( "4.vks_sign result = {0}", res_vks_sign.result );

			// 4. VKS Hash 서명의 유효성을 확인하기 위해서, Game-Server는 서명을 복호화 하여 검증합니다.
			WemixAPI.Res_contract_recoverHash res_contract_recoverHash = api.contract_recoverHash( res_tx_makeUnsignedTx.result.hash, res_vks_sign.result );
			if( res_contract_recoverHash.IsValid() == false )
			{
				log_sb.AppendLine( "" );
				log_sb.AppendLine( "== Wemix API Process ==" );
				log_sb.AppendLine( api.UTIL.GetLog );
				WemixUTIL.WriteLog( "WemixAirdropFailed", "Wemix Airdrop Executed data", log_sb.ToString() );

				APIErrorPopup( api, res_contract_recoverHash );
				return;
			}
			//Util.Log( "5.contract_recoverHash vks address = {0}", res_contract_recoverHash.result );
			if( api.CheckVKSAddress( res_contract_recoverHash.result ) == false )
			{
				log_sb.AppendLine( "" );
				log_sb.AppendLine( "== Wemix API Process ==" );
				log_sb.AppendLine( api.UTIL.GetLog );
				WemixUTIL.WriteLog( "WemixAirdropFailed", "Wemix Airdrop Executed data", log_sb.ToString() );

				string error = string.Format( "4.VKS Address invalid RES:{0} <> API:{1}", res_contract_recoverHash.result, api.ADDRESS_VKS );
				APIErrorPopup( api, error );
				return;
			}

			// 5. VKS 서명값을 전달 받은 Game-Server는 유효성 확인 후 Binder를 통해 트랜잭션 전송(tx_sendSignedTx) 요청을 합니다.
			WemixAPI.Res_tx_sendSignedTx res_tx_sendSignedTx = api.tx_sendSignedTx( res_tx_makeUnsignedTx.result.unsignedTx, res_vks_sign.result );
			if( res_tx_sendSignedTx.IsValid() == false )
			{
				log_sb.AppendLine( "" );
				log_sb.AppendLine( "== Wemix API Process ==" );
				log_sb.AppendLine( api.UTIL.GetLog );
				WemixUTIL.WriteLog( "WemixAirdropFailed", "Wemix Airdrop Executed data", log_sb.ToString() );

				APIErrorPopup( api, res_tx_sendSignedTx );
				return;
			}

			string status_desc = api.GetStatusDesc_tx_sendSignedTx( res_tx_sendSignedTx.result.status );
			// 6. 트랜잭션 전송 요청에 대한 Response를 통해 토큰 전송에 대한 결과를 확인 할 수 있습니다.
			if( res_tx_sendSignedTx.result.logs == null )
			{
				log_sb.AppendLine( "" );
				log_sb.AppendLine( "== Wemix API Process ==" );
				log_sb.AppendLine( api.UTIL.GetLog );
				WemixUTIL.WriteLog( "WemixAirdropFailed", "Wemix Airdrop Executed data", log_sb.ToString() );

				APIErrorPopup( api, status_desc );
				//	Util.Log( "6.tx_sendSignedTx status = {0}", res_tx_sendSignedTx.result.status );
			}
			else
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendLine( status_desc );
				sb.AppendLine( res_tx_sendSignedTx.original_response_text );

				log_sb.AppendLine( "" );
				log_sb.AppendLine( "== Wemix API Process ==" );
				log_sb.AppendLine(  api.UTIL.GetLog );


				// step 7 : 지급후 토큰 잔량 체크
				string after_remain_token = "_error_";
				bool remain_token_from_api = false;
				WemixAPI.Res_getPendingAmount getPendingAmount_after = api.getPendingAmount();
				if( getPendingAmount_after.IsValid() )
				{
					remain_token_from_api = true;
					after_remain_token = getPendingAmount_after.result[0];
				}
				else
				{
					after_remain_token = tbxRemainTokensWeiHide.Text;
				}

				string connect_str = "Log_" + server_type + "_Connection";

				bool has_db_error = false;

				try
				{
					string remain_token_count = after_remain_token;
					bool remain_from_api = remain_token_from_api;

					string request_log_text = api.UTIL.GetLog;
					string response_text = res_tx_sendSignedTx.original_response_text;

					string save_time = lblLoadInfoSaveTime.Text;
					string save_tool_user_idx = lblLoadInfoSaveToolUserIDX.Text;
					string save_tool_user_id = lblLoadInfoSaveToolUserID.Text;

					DBConn dbconn = new DBConn( ConfigManager.GetConnectionString( connect_str ) );

					dbconn.command.CommandText = "dbo.spt_ToolWemixAirdropLog_Insert";
					dbconn.command.CommandType = CommandType.StoredProcedure;

					dbconn.SetParameter( "@send_time", System.Data.SqlDbType.DateTime2, DateTime.Now );

					dbconn.SetParameter( "@tool_user_idx", System.Data.SqlDbType.Int, tool_user_idx );
					dbconn.SetParameter( "@tool_user_id", System.Data.SqlDbType.NVarChar, 100, tool_user_id );
					dbconn.SetParameter( "@tool_user_ip", System.Data.SqlDbType.NVarChar, 100, tool_user_ip );

					dbconn.SetParameter( "@contract_name", System.Data.SqlDbType.NVarChar, 100, contract_name );
					dbconn.SetParameter( "@contract_address", System.Data.SqlDbType.NVarChar, 100, contract_address );
					dbconn.SetParameter( "@vks_address", System.Data.SqlDbType.NVarChar, 100, vks_address );
					dbconn.SetParameter( "@fund_resource_address", System.Data.SqlDbType.NVarChar, 100, fund_resource_address );
					dbconn.SetParameter( "@org_token_count", System.Data.SqlDbType.NVarChar, -1, org_token_count );
					dbconn.SetParameter( "@send_token_count", System.Data.SqlDbType.NVarChar, -1, send_token_count );
					dbconn.SetParameter( "@remain_token_count", System.Data.SqlDbType.NVarChar, -1, remain_token_count );
					dbconn.SetParameter( "@remain_from_api", System.Data.SqlDbType.Bit, remain_from_api );
					dbconn.SetParameter( "@send_address_count", System.Data.SqlDbType.Int, send_address_count );
					dbconn.SetParameter( "@send_addresses", System.Data.SqlDbType.NVarChar, -1, send_addresses );

					dbconn.SetParameter( "@request_log", System.Data.SqlDbType.NVarChar, -1, request_log_text );
					dbconn.SetParameter( "@response_json", System.Data.SqlDbType.NVarChar, -1, response_text );

					dbconn.SetParameter( "@save_idx", SqlDbType.Int, save_index );
					dbconn.SetParameter( "@save_time", SqlDbType.DateTime2, save_time );
					dbconn.SetParameter( "@save_tool_user_idx", SqlDbType.Int, save_tool_user_idx );
					dbconn.SetParameter( "@save_tool_user_id", SqlDbType.NVarChar, 100, save_tool_user_id );
					dbconn.SetParameter( "@description", SqlDbType.NVarChar, -1, description );

					log_sb.AppendLine( "" );
					log_sb.AppendLine( "== DBDump ==" );
					log_sb.AppendLine( DBSqlDump.GetCommandDump( "spt_ToolWemixAirdropLog_Insert", dbconn.command ) );

					dbconn.DBOpen();
					dbconn.ExecuteSPNonQuery();
					dbconn.DBClose();
				}
				catch( System.Exception ex )
				{
					log_sb.AppendLine( "" );
					log_sb.AppendLine( "== Exception ==" );
					log_sb.AppendLine( ex.ToString() );

					sb.AppendLine( ">>>>>> DB Exception" );
					sb.AppendLine( ex.ToString() );
					has_db_error = true;
				}

				if( save_index > 0 )
				{
					try
					{
						DBConn dbconn = new DBConn( ConfigManager.GetConnectionString( connect_str ) );

						dbconn.command.CommandText = "dbo.spt_ToolWemixAirdropSave_Send";
						dbconn.command.CommandType = CommandType.StoredProcedure;
						dbconn.SetParameter( "@save_index", System.Data.SqlDbType.Int, save_index );
						dbconn.SetParameter( "@tool_user_idx", System.Data.SqlDbType.Int, tool_user_idx );
						dbconn.SetParameter( "@tool_user_id", System.Data.SqlDbType.NVarChar, 100, tool_user_id );
						dbconn.SetParameter( "@send_time", System.Data.SqlDbType.DateTime2, DateTime.Now );
						dbconn.DBOpen();
						dbconn.ExecuteSPNonQuery();
						dbconn.DBClose();

						gdvLoadList_Show();
					}
					catch( Exception ex )
					{
						log_sb.AppendLine( "" );
						log_sb.AppendLine( "== Exception ==" );
						log_sb.AppendLine( ex.ToString() );

						sb.AppendLine( ">>>>>> DB Exception" );
						sb.AppendLine( ex.ToString() );
						//WebUtil.Alert( this, "db error", Resources.ResourceTool.lblDBError + ex.ToString() );
						has_db_error = true;
					}
				}

				WemixUTIL.WriteLog( "WemixAirdropExec", "Wemix Airdrop Executed data", log_sb.ToString() );

				UpdateInfo();
				string title = "AirDrop SUCCESS";
				if( has_db_error )
					title += " ( With DB Error )";
				APILogPopup( api, title, sb.ToString(), true );
				// TODO : log
				//Util.Log( "6.tx_sendSignedTx status = {0} logs = {1}", res_tx_sendSignedTx.result.status, res_tx_sendSignedTx.result.logs.Length );
			}
		}
	}

	private String GetIP()
	{
		string ipString;
		if( string.IsNullOrEmpty( Request.ServerVariables["HTTP_X_FORWARDED_FOR"] ) )
		{
			ipString = Request.ServerVariables["REMOTE_ADDR"];
		}
		else
		{
			ipString = Request.ServerVariables["HTTP_X_FORWARDED_FOR"].Split( ",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries ).FirstOrDefault();
		}
		return ipString;
	}


	void APIErrorPopup( WemixAPI api, WemixAPI.ResBase response )
	{
		APIErrorPopup( api, response.GetError() );
	}
	void APIErrorPopup( WemixAPI api, string error )
	{
		APILogPopup( api, "API ERROR", error );
	}
	void APILogPopup( WemixAPI api, string title, string log, bool is_success = false )
	{
		lblAPILogTitle.Text = title;
		tbxAPILogText.Text = log;
		tbxAPILogLogText.Text = api.UTIL.GetLog;
		Page.ClientScript.RegisterStartupScript( this.GetType(), "open-modal", "openAPILogPopup();", true );
	}

	protected void btnDoLoad_Click( object sender, EventArgs e )
	{
		lblLoadTitle.Visible = true;
		gdvLoadList_Show();
	}

	private void gdvLoadList_Show()
	{
		try
		{
			string server_type = Session["server_type_text"].ToString();
			string connect_str = "Log_" + server_type + "_Connection";

			DBConn dbconn = new DBConn( ConfigManager.GetConnectionString( connect_str ) );

			dbconn.command.CommandText = "dbo.spt_ToolWemixAirdropSave_Select";
			dbconn.command.CommandType = CommandType.StoredProcedure;
			dbconn.DBOpen();
			dbconn.SetOutputParameter( "@TotalCount", System.Data.SqlDbType.Int, 0 );
			DataSet ds = dbconn.ExecuteSPDs( "gdvLoadList" );

			string total_row = dbconn.command.Parameters["@TotalCount"].Value.ToString();

			dbconn.DBClose();

			gdvLoadList.DataSource = ds;
			gdvLoadList.DataBind();
			gdvLoadList.Visible = true;

			lblLoadTitle.Visible = true;
			lblLoadTitle.Text = string.Format( "불러오기 ( 저장된 수 : {0} )", total_row );
		}
		catch( Exception ex )
		{
			WebUtil.Alert( this, "db error", Resources.ResourceTool.lblDBError + ex.ToString() );//"Failed to load DB data"); // DB 데이터를 불러오지 못했습니다.
		}
	}

	protected void gdvLoadList_RowDataBound( object sender, GridViewRowEventArgs e )
	{
		if( !_isRefresh )
		{
			if( e.Row.RowType == DataControlRowType.DataRow )
			{
				int RowIndex = e.Row.RowIndex;

				DataRowView drv = (DataRowView)e.Row.DataItem;

				Label lblLoadSaveToolUserIDX = e.Row.FindControl( "lblLoadSaveToolUserIDX" ) as Label;
				Label lblLoadSaveToolUserID = e.Row.FindControl( "lblLoadSaveToolUserID" ) as Label;
				Label lblLoadsaveDescription = e.Row.FindControl( "lblLoadsaveDescription" ) as Label;
				Label lblLoadsaveSendToken = e.Row.FindControl( "lblLoadsaveSendToken" ) as Label;
				Label lblLoadsaveSendAddressCount = e.Row.FindControl( "lblLoadsaveSendAddressCount" ) as Label;

				LinkButton lbtnLoadSaveDetail = e.Row.FindControl( "lbtnLoadSaveDetail" ) as LinkButton;
				LinkButton lbtnLoadSaveLoad = e.Row.FindControl( "lbtnLoadSaveLoad" ) as LinkButton;
				LinkButton lbtnLoadSaveDelete = e.Row.FindControl( "lbtnLoadSaveDelete" ) as LinkButton;

				lbtnLoadSaveDetail.Text = string.Format( "{0}({1}) / {2} / send:{3} / address:{4}", 
					lblLoadSaveToolUserID.Text,
					lblLoadSaveToolUserIDX.Text,
					lblLoadsaveDescription.Text,
					lblLoadsaveSendToken.Text,
					lblLoadsaveSendAddressCount.Text					
					);

				lbtnLoadSaveDetail.CommandArgument = RowIndex.ToString();
				lbtnLoadSaveLoad.CommandArgument = RowIndex.ToString();
				lbtnLoadSaveDelete.CommandArgument = RowIndex.ToString();
			}
		}

	}

	protected void gdvLoadList_RowCommand( object sender, GridViewCommandEventArgs e )
	{
		if( !_isRefresh )
		{
			int RowIndex = Convert.ToInt32( e.CommandArgument.ToString() );
			if( e.CommandName.Equals( "lbtnLoadSaveDetail_Command" ) )
			{
				Label lblLoadSaveTime = gdvLoadList.Rows[RowIndex].FindControl( "lblLoadSaveTime" ) as Label;
				Label lblLoadSaveToolUserIDX = gdvLoadList.Rows[RowIndex].FindControl( "lblLoadSaveToolUserIDX" ) as Label;
				Label lblLoadSaveToolUserID = gdvLoadList.Rows[RowIndex].FindControl( "lblLoadSaveToolUserID" ) as Label;
				Label lblLoadsaveDescription = gdvLoadList.Rows[RowIndex].FindControl( "lblLoadsaveDescription" ) as Label;
				Label lblLoadsaveSendToken = gdvLoadList.Rows[RowIndex].FindControl( "lblLoadsaveSendToken" ) as Label;
				Label lblLoadsaveSendAddressCount = gdvLoadList.Rows[RowIndex].FindControl( "lblLoadsaveSendAddressCount" ) as Label;
				Label lblLoadSaveAddresses = gdvLoadList.Rows[RowIndex].FindControl( "lblLoadSaveAddresses" ) as Label;
				Label lblLoadSaveCreateTime = gdvLoadList.Rows[RowIndex].FindControl( "lblLoadSaveCreateTime" ) as Label;
				Label lblLoadSaveUpdateTime = gdvLoadList.Rows[RowIndex].FindControl( "lblLoadSaveUpdateTime" ) as Label;

				lblsavedetail_savetime.Text = string.Format( "{0} 생성:{1} 업데이트:{2}", lblLoadSaveTime.Text, lblLoadSaveCreateTime.Text, lblLoadSaveUpdateTime.Text );
				
				tbxsavedetail_description.Text = lblLoadsaveDescription.Text;
				tbxsavedetail_sendtoken.Text = lblLoadsaveSendToken.Text;

				StringBuilder sb = new StringBuilder();
				string[] splits = lblLoadSaveAddresses.Text.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
				int count = 0;
				if( splits != null && splits.Length > 0 )
				{
					for( int i = 0; i < splits.Length; i++ )
					{
						string address = splits[i].Trim();
						if( string.IsNullOrEmpty( address ) == false )
						{
							sb.AppendLine( address );
							count += 1;
						}
					}
				}

				tbxsavedetail_saveuser.Text = string.Format( "{0}({1}) 전송주소:{2}", lblLoadSaveToolUserID.Text, lblLoadSaveToolUserIDX.Text, count );
				tbxsavedetail_addresses.Text = sb.ToString();

				Page.ClientScript.RegisterStartupScript( this.GetType(), "open-modal", "openLoadSaveDetailPopup();", true );
			}
			else if( e.CommandName.Equals( "lbtnLoadSaveLoad_Command" ) )
			{
				Label lblLoadSaveIndex = gdvLoadList.Rows[RowIndex].FindControl( "lblLoadSaveIndex" ) as Label;
				Label lblLoadSaveTime = gdvLoadList.Rows[RowIndex].FindControl( "lblLoadSaveTime" ) as Label;
				Label lblLoadSaveToolUserIDX = gdvLoadList.Rows[RowIndex].FindControl( "lblLoadSaveToolUserIDX" ) as Label;
				Label lblLoadSaveToolUserID = gdvLoadList.Rows[RowIndex].FindControl( "lblLoadSaveToolUserID" ) as Label;
				Label lblLoadsaveDescription = gdvLoadList.Rows[RowIndex].FindControl( "lblLoadsaveDescription" ) as Label;
				Label lblLoadsaveSendToken = gdvLoadList.Rows[RowIndex].FindControl( "lblLoadsaveSendToken" ) as Label;
				Label lblLoadsaveSendAddressCount = gdvLoadList.Rows[RowIndex].FindControl( "lblLoadsaveSendAddressCount" ) as Label;
				Label lblLoadSaveAddresses = gdvLoadList.Rows[RowIndex].FindControl( "lblLoadSaveAddresses" ) as Label;

				tbxLoadInfoIDX.Text = lblLoadSaveIndex.Text;
				tbxLoadInfo.Text = string.Format( "저장IDX : {0} 저장한유저 : {1}", lblLoadSaveIndex.Text, lblLoadSaveToolUserID.Text );
				lblLoadInfoSaveToolUserIDX.Text = lblLoadSaveToolUserIDX.Text;
				lblLoadInfoSaveToolUserID.Text = lblLoadSaveToolUserID.Text;
				lblLoadInfoSaveTime.Text = lblLoadSaveTime.Text;
				tbxSendAmount.Text = WemixUTIL.FromWei( lblLoadsaveSendToken.Text ).ToString();

				StringBuilder sb = new StringBuilder();
				string[] splits = lblLoadSaveAddresses.Text.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
				int count = 0;
				if( splits != null && splits.Length > 0 )
				{
					for( int i = 0; i < splits.Length; i++ )
					{
						string address = splits[i].Trim();
						if( string.IsNullOrEmpty( address ) == false )
						{
							sb.AppendLine( address );
							count += 1;
						}
					}
				}

				tbxTargetInputPerLine.Text = sb.ToString();
				tbxSaveDescriptionInput.Text = lblLoadsaveDescription.Text;
				btnInputAddress_Click( null, null );
			}
			else if( e.CommandName.Equals( "lbtnLoadSaveDelete_Command" ) )
			{
				try
				{
					Label lblLoadSaveIndex = gdvLoadList.Rows[RowIndex].FindControl( "lblLoadSaveIndex" ) as Label;
					int save_index = XMLUtil.SafeParse<int>( lblLoadSaveIndex.Text, 0 );

					string server_type = Session["server_type_text"].ToString();
					string connect_str = "Log_" + server_type + "_Connection";

					DBConn dbconn = new DBConn( ConfigManager.GetConnectionString( connect_str ) );

					dbconn.command.CommandText = "dbo.spt_ToolWemixAirdropSave_Delete";
					dbconn.command.CommandType = CommandType.StoredProcedure;
					dbconn.SetParameter( "@save_index", System.Data.SqlDbType.Int, save_index );
					dbconn.DBOpen();
					dbconn.ExecuteSPNonQuery();
					dbconn.DBClose();

					gdvLoadList_Show();
				}
				catch( Exception ex )
				{
					WebUtil.Alert( this, "db error", Resources.ResourceTool.lblDBError + ex.ToString() );
				}
			}
		}
	}
}