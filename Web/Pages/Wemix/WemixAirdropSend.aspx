<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="WemixAirdropSend.aspx.cs" Inherits="Pages_Wemix_WemixAirdropSend" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" Runat="Server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="Server">
<script type="text/javascript">
    $(function () {
        $('.popup-modal').magnificPopup({
            type: 'inline',
            preloader: false,
            modal: true
        });

        $(document).on('click', '.popup-modal-dismiss', function (e) {
            e.preventDefault();
            $.magnificPopup.close();
        });
	});

	function openAddressViewPopup() {
		$.magnificPopup.open({
			items: {
				src: '#address_viewer_modal',
			},
			type: 'inline'
		});
    }

    function openAddressInvalidPopup() {
		$.magnificPopup.open({
			items: {
				src: '#address_invalid_modal',
			},
			type: 'inline'
		});
    }

	function openAPILogPopup() {
		$.magnificPopup.open({
			items: {
				src: '#api_log_modal',
			},
			type: 'inline'
		});
    }

    function openLoadSaveDetailPopup() {
		$.magnificPopup.open({
			items: {
				src: '#loadsave_viewer_modal',
			},
			type: 'inline'
		});
    }

	function SendConfirm() {
		message = "설정된 정보로 지급하시겠습니까? \n돌이킬수 없습니다. \n지급수량 및 지급대상자를 꼭 확인하시기 바랍니다.\n수량 혹은 주소가 변경되었을시에는 입력 버튼으로 확인하시기 바랍니다.";

		if (confirm(message)) {
			return true;
		}
		else {
			return false;
		}
	}

	function SaveConfirm() {
		message = "설정된 정보로 저장하시겠습니까? \n. 변경사항은 꼭 입력 버튼을 눌러 화면을 갱신해야 합니다.\n지급 권한이 있는 사용자가 지급할 수 있습니다.";

		if (confirm(message)) {
			return true;
		}
		else {
			return false;
		}
	}

	function LoadSaveDeleteConfirm() {
		message = "저장정보를 삭제 하시겠습니까?";

		if (confirm(message)) {
			return true;
		}
		else {
			return false;
		}
	}
	function LoadSaveInputConfirm() {
		message = "저장정보를 입력 하시겠습니까?";

		if (confirm(message)) {
			return true;
		}
		else {
			return false;
		}
	}


</script>
<div class="content-wrapper" style="margin-left:10px; margin-top:10px;">
    <div class="float-left">
        <h1>에어드랍 토큰지급</h1>
    </div>
    <div class="float-right">
        <h3>Wemix - WemixAirdropSend</h3>
    </div>
    <div class="underline"></div>
    <br />
    <div class="float-left SearchBox">
        <fieldset runat="server">
            <table border="1" style="border-style:dotted;" >
                <tr>
                    <th >환경</th>
                    <td style="vertical-align:middle" >
                        <asp:TextBox runat="server" ID="tbxEnvironment" Text="" ReadOnly="true" Width="700" BackColor="LightGray" />
                     </td>
                </tr>
                <tr >
                    <th >지갑 주소</th>
                    <td style="vertical-align:middle">
                        <asp:TextBox runat="server" ID="tbxContractAddress" Text="" ReadOnly="true" Width="700" BackColor="LightGray" />
                    </td>
                </tr>
                <tr>
                    <th >VKS 주소</th>
                    <td style="vertical-align:middle">
                        <asp:TextBox runat="server" ID="tbxVKSAddress" Text="" ReadOnly="true" Width="700" BackColor="LightGray" />
                    </td>
                </tr>
                <tr>
                    <th >자금 출처 주소</th>
                    <td style="vertical-align:middle">
                        <asp:TextBox runat="server" ID="tbxFundAddress" Text="" ReadOnly="true" Width="700" BackColor="LightGray" />
                        </td>
                </tr>
                <tr>
                    <th >토큰 잔량</th>
                    <td style="vertical-align:middle">
                        <asp:TextBox runat="server" ID="tbxRemainTokens" Text="" ReadOnly="true" Width="200" BackColor="LightGray" />
                        <br />
                        <asp:TextBox runat="server" ID="tbxRemainTokensWei" Text="" ReadOnly="true" Width="500" BackColor="LightGray" />(/WEI)
                        <asp:TextBox runat="server" ID="tbxRemainTokensWeiHide" Visible="false" />
                    </td>
                </tr>
                <tr style="background-color:#66ccff">
                    <th  >지급 대상자 수</th>
                    <td style="vertical-align:middle" >
                        <asp:TextBox runat="server" ID="tbxSendUserTotalCount" Text="" Width="700" ReadOnly="true" BackColor="LightGray" />
                    </td>
                </tr>
                <tr style="background-color:#66ccff">
                    <th  >총 지급량</th>
                    <td style="vertical-align:middle" >
                        <asp:TextBox runat="server" ID="tbxSendTotalAmount" Text="" ReadOnly="true" Width="200" BackColor="LightGray" />
                        <br />
                        <asp:TextBox runat="server" ID="tbxSendTotalAmountWei" Text="" Width="500" ReadOnly="true" BackColor="LightGray" />(/WEI)                     
                        <asp:TextBox runat="server" ID="tbxSendTotalAmountWeiHide" Visible="false" />
                    </td>
                </tr>
                <tr style="background-color:#66ccff">
                    <th  >토큰 예상 잔량</th>
                    <td style="vertical-align:middle" >
                        <asp:TextBox runat="server" ID="tbxSendRemainAmount" Text="" ReadOnly="true" Width="200" BackColor="LightGray" />
                        <br />
                        <asp:TextBox runat="server" ID="tbxSendRemainAmountWei" Text="" Width="500" ReadOnly="true" BackColor="LightGray" />(/WEI)
                    </td>
                </tr>
                <tr style="background-color:coral">
                    <th  >불러오기 정보</th>
                    <td style="vertical-align:middle">
                        <asp:TextBox runat="server" ID="tbxLoadInfo" Text="" Width="700" ReadOnly="true" BackColor="LightGray" />
                        <asp:TextBox runat="server" ID="tbxLoadInfoIDX" Text="" Visible="false" />
                        <asp:Label runat="server" ID="lblLoadInfoSaveToolUserID" Text="" Visible ="false" />
                        <asp:Label runat="server" ID="lblLoadInfoSaveToolUserIDX" Text="" Visible ="false" />
                        <asp:Label runat="server" ID="lblLoadInfoSaveTime" Text="" Visible ="false" />
                    </td>
                </tr>
                <tr style="background-color:lightgreen">
                    <th  >지급 수량</th>
                    <td style="vertical-align:middle">
                        <asp:TextBox runat="server" ID="tbxSendAmount" Text="0" Width="200" />
                        <br />
                        <asp:TextBox runat="server" ID="tbxSendAmountWei" Text="" Width="500" ReadOnly="true" BackColor="LightGray" />(/WEI)
                        <asp:TextBox runat="server" ID="tbxSendAmountWeiHide" Visible="false" />
                    </td>
                </tr>
                <tr style="background-color:lightgreen" >
                    <th>
                        주소 입력
                    </th>
                    <td style="vertical-align:middle" >
                        <asp:TextBox runat="server" ID="tbxTargetInputPerLine" Text="" Width="700" Height="100" TextMode="MultiLine" Wrap="false" />                        
                    </td>
                </tr>
                <tr style="background-color:lightgreen" >
                    <td colspan="2">
                        <b>* 주소 입력은 줄단위로 최대 500개 가능합니다. 입력클릭시 대상자 수 확인 가능!</b>
                    </td>
                </tr>
                <tr style="background-color:lightgreen" >
                    <th>
                        저장용 설명
                    </th>
                    <td style="vertical-align:middle" >
                        <asp:TextBox runat="server" ID="tbxSaveDescriptionInput" Text="" Width="700" Height="50" TextMode="MultiLine" Wrap="false" />                        
                    </td>
                </tr>
                <tr style="height:50px">
                    <th >지 급</th>
                    <td style="vertical-align:middle">
                        <asp:Button ID="btnInputAddress" runat="server" Text="입력" Width="100" Height="40" BackColor="#66ccff" OnClick="btnInputAddress_Click" />
                        <asp:Button ID="btnClear" runat="server" Text="모두지우기" Width="100" Height="40" OnClick="btnClear_Click" />
                        <div class="float-right">
                            <asp:Button ID="btnDoSave" runat="server" Text="저장하기" Width="100" Height="40" BackColor="#ff9966" OnClick="btnDoSave_Click" OnClientClick="return SaveConfirm()" />
                            <asp:Button ID="btnDoLoad" runat="server" Text="저장목록 갱신" Width="130" Height="40" BackColor="#ff9966" OnClick="btnDoLoad_Click" />
                            <asp:Button ID="btnDoSend" runat="server" Text="지급하기" Width="100" Height="40" BackColor="#ffff00" OnClick="btnDoSend_Click" OnClientClick="return SendConfirm()" />
                        </div>
                    </td>
                </tr>
            </table>        
        </fieldset>
    </div>
    <br /><br />
    <div class="float-left">
        <h3>
            <asp:Label id="lblLoadTitle" runat="server" Text="불러오기"></asp:Label>
        </h3>
        <asp:GridView ID="gdvLoadList" CssClass="table table-bordered" runat="server" OnRowDataBound="gdvLoadList_RowDataBound" AutoGenerateColumns="false" Width="1000"
            OnRowCommand="gdvLoadList_RowCommand" >
            <Columns>
                <asp:TemplateField ShowHeader="true" HeaderText="index" ItemStyle-Width="100" >
                    <ItemTemplate>
                        <asp:Label ID="lblLoadSaveIndex" runat="server" Text='<%# Bind("save_index") %>'></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField ShowHeader="true" HeaderText="저장시간" ItemStyle-Width="100" >
                    <ItemTemplate>
                        <asp:Label ID="lblLoadSaveTime" runat="server" Text='<%# Bind("save_time") %>'></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField ShowHeader="true" HeaderText="저장정보(클릭시 상세정보)" ItemStyle-Width="600" >
                    <ItemTemplate>
                        <asp:LinkButton ID="lbtnLoadSaveDetail" runat="server" Text="" CssClass="btnLink" CommandName="lbtnLoadSaveDetail_Command"  CommandArgument=""></asp:LinkButton>
                        <asp:Label ID="lblLoadSaveToolUserIDX" runat="server" Text='<%# Bind("tool_user_idx") %>' Visible="false" />
                        <asp:Label ID="lblLoadSaveToolUserID" runat="server" Text='<%# Bind("tool_user_id") %>' Visible="false" />
                        <asp:Label ID="lblLoadsaveSendToken" runat="server" Text='<%# Bind("send_token_count") %>' Visible="false" />
                        <asp:Label ID="lblLoadsaveSendAddressCount" runat="server" Text='<%# Bind("send_address_count") %>' Visible="false" />
                        <asp:Label ID="lblLoadSaveAddresses" runat="server" Text='<%# Bind("send_addresses") %>' Visible="false" />
                        <asp:Label ID="lblLoadsaveDescription" runat="server" Text='<%# Bind("description") %>' Visible="false" />
                        <asp:Label ID="lblLoadSaveCreateTime" runat="server" Text='<%# Bind("create_time") %>' Visible="false" />
                        <asp:Label ID="lblLoadSaveUpdateTime" runat="server" Text='<%# Bind("update_time") %>' Visible="false" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField ShowHeader="true" HeaderText="입력하기" ItemStyle-Width="100" >
                    <ItemTemplate>
                        <asp:LinkButton ID="lbtnLoadSaveLoad" runat="server" Text='입력' CssClass="btnLink" CommandName="lbtnLoadSaveLoad_Command"  CommandArgument="" OnClientClick="return LoadSaveInputConfirm();"></asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField ShowHeader="true" HeaderText="삭제" ItemStyle-Width="100">
                    <ItemTemplate>
                        <asp:LinkButton ID="lbtnLoadSaveDelete" runat="server" Text='삭제' CssClass="btnLink" CommandName="lbtnLoadSaveDelete_Command"  CommandArgument="" OnClientClick="return LoadSaveDeleteConfirm();"></asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <RowStyle Height="50" />
        </asp:GridView>
        <h3>
            <asp:Label id="lblTargetaddressinfoCount" runat="server" Text="대상 주소 확인"></asp:Label>
        </h3>
        <asp:GridView ID="gdvTargetList" CssClass="table table-bordered" runat="server" OnRowDataBound="gdvTargetList_RowDataBound" AutoGenerateColumns="false" Width="1000" 
            AllowPaging="true" PageSize="5" OnPageIndexChanging="gdvTargetList_PageIndexChanging" >
            <Columns>
                <asp:TemplateField ShowHeader="true" HeaderText="대상주소" ItemStyle-Width="900" >
                    <ItemTemplate>
                        <asp:Label ID="lblTargetAddress" runat="server" Text='<%# Bind("target_address") %>'></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField ShowHeader="true" HeaderText="정보" >
                    <ItemTemplate>
                        <asp:LinkButton ID="lbtnTargetAddressDetail" runat="server" Text='지갑정보' CssClass="btnLink" OnCommand="lbtnTargetAddressDetail_Command"  CommandArgument='<%# Bind("target_address") %>'></asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <RowStyle Height="50" />
            <PagerSettings Mode="NumericFirstLast"  PageButtonCount="10" Position="Top" />
        </asp:GridView>
    </div>
</div>
<div class="viewer-popup-block mfp-hide" id="address_viewer_modal" style="height:700px; width:700px;">
    <div style="text-align:center; display: inline; margin-bottom:20px;">
        <hgroup>
            <h1 style="margin-bottom:5px; padding:0px 0px 0px 0px; font-size:20px;">
                주소 상세 정보
            </h1>
        </hgroup>
    </div>
    <div style="margin-left: -5px; border-bottom: 1px solid;"></div>
    <div class="viewerPopup">
        <table style="width:650px; margin-left: -10px;">
            <tr>
                <th>주소</th>
                <td>
                    <asp:Label ID="lblAddrviewAddress" runat="server" Text="" Width="98%"></asp:Label>
                </td>
            </tr>            
            <tr>
                <th>상세정보</th>                   
                <td>
                    <asp:TextBox runat="server" ID="tbxAddrviewAddressInfo" Text="" Width="98%" Height="250px" ReadOnly="true" TextMode="MultiLine"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th>응답RAW</th>                   
                <td>
                    <asp:TextBox runat="server" ID="tbxAddrviewResponseText" Text="" Width="98%" Height="250px" ReadOnly="true" TextMode="MultiLine"></asp:TextBox>
                </td>
            </tr>
        </table>
        <div style="float:right; margin-top:15px;">
            <asp:Button runat="server" ID="btnAddrviewClose" Width="60px" Height="30px" Text="Close" BorderStyle="Solid" BorderColor="Black" CssClass="popup-modal-dismiss"/>
        </div>
    </div>
</div>
<div class="viewer-popup-block mfp-hide" id="address_invalid_modal" style="height:500px; width:700px;">
    <div style="text-align:center; display: inline; margin-bottom:20px;">
        <hgroup>
            <h1 style="margin-bottom:5px; padding:0px 0px 0px 0px; font-size:20px;">
                잘못된 주소 발견
            </h1>
        </hgroup>
    </div>
    <div style="margin-left: -5px; border-bottom: 1px solid;"></div>
    <div class="viewerPopup">
        <table style="width:650px; margin-left: -10px;">
            <tr>
                <th>잘못된 주소들</th>                   
                <td>
                    <asp:TextBox runat="server" ID="tbxInvalidAddressText" Text="" Width="98%" Height="250px" ReadOnly="true" TextMode="MultiLine"></asp:TextBox>
                </td>
            </tr>
        </table>
        <div style="float:right; margin-top:15px;">
            <asp:Button runat="server" ID="Button1" Width="60px" Height="30px" Text="Close" BorderStyle="Solid" BorderColor="Black" CssClass="popup-modal-dismiss"/>
        </div>
    </div>
</div>
<div class="viewer-popup-block mfp-hide" id="api_log_modal" style="height:700px; width:700px;">
    <div style="text-align:center; display: inline; margin-bottom:20px;">
        <hgroup>
            <h1 style="margin-bottom:5px; padding:0px 0px 0px 0px; font-size:20px;">
                <asp:Label runat="server" ID="lblAPILogTitle" Text="API 로그" />
            </h1>
        </hgroup>
    </div>
    <div style="margin-left: -5px; border-bottom: 1px solid;"></div>
    <div class="viewerPopup">
        <table style="width:650px; margin-left: -10px;">
            <tr>
                <th>내용</th>                   
                <td>
                    <asp:TextBox runat="server" ID="tbxAPILogText" Text="" Width="98%" Height="250px" ReadOnly="true" TextMode="MultiLine"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th>로그</th>                   
                <td>
                    <asp:TextBox runat="server" ID="tbxAPILogLogText" Text="" Width="98%" Height="250px" ReadOnly="true" TextMode="MultiLine"></asp:TextBox>
                </td>
            </tr>
        </table>
        <div style="float:right; margin-top:15px;">
            <asp:Button runat="server" ID="btnApiLogClosed" Width="60px" Height="30px" Text="Close" BorderStyle="Solid" BorderColor="Black" CssClass="popup-modal-dismiss" />
        </div>
    </div>
</div>
<div class="viewer-popup-block mfp-hide" id="loadsave_viewer_modal" style="height:700px; width:700px;">
    <div style="text-align:center; display: inline; margin-bottom:20px;">
        <hgroup>
            <h1 style="margin-bottom:5px; padding:0px 0px 0px 0px; font-size:20px;">
                불러오기 상세정보
            </h1>
        </hgroup>
    </div>
    <div style="margin-left: -5px; border-bottom: 1px solid;"></div>
    <div class="viewerPopup">
        <table style="width:650px; margin-left: -10px;">
            <tr>
                <th>저장시간</th>
                <td>
                    <asp:Label ID="lblsavedetail_savetime" runat="server" Text="" Width="98%"></asp:Label>
                </td>
            </tr>            
            <tr>
                <th>저장유저</th>                   
                <td>
                    <asp:TextBox runat="server" ID="tbxsavedetail_saveuser" Text="" Width="98%" ReadOnly="true" TextMode="MultiLine"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th>설명</th>                   
                <td>
                    <asp:TextBox runat="server" ID="tbxsavedetail_description" Text="" Width="98%" Height="100px" ReadOnly="true" TextMode="MultiLine"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th>전송토큰량</th>                   
                <td>
                    <asp:TextBox runat="server" ID="tbxsavedetail_sendtoken" Text="" Width="98%" ReadOnly="true" TextMode="SingleLine"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <th>전송주소들</th>                   
                <td>
                    <asp:TextBox runat="server" ID="tbxsavedetail_addresses" Text="" Width="98%" Height="250px" ReadOnly="true" TextMode="MultiLine"></asp:TextBox>
                </td>
            </tr>
        </table>
        <div style="float:right; margin-top:15px;">
            <asp:Button runat="server" ID="Button2" Width="60px" Height="30px" Text="Close" BorderStyle="Solid" BorderColor="Black" CssClass="popup-modal-dismiss"/>
        </div>
    </div>
</div>
</asp:Content>


