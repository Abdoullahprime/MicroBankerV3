<%@ Page Language="vb" AutoEventWireup="false"  MasterPageFile="~/Site.Master" CodeBehind="UnBlockAmount.aspx.vb" Inherits="MicroBankerAdmin.UnBlockAmount" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
     <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>


            <table class="tbMain">
                <tbody runat="server" id="UnblockTbl" visible="false">
                    <tr>
                        <td>
                            <asp:Button ID="btnClearForm" runat="server" Text="UnBlock Amount" CausesValidation="False" CssClass="btn-New"></asp:Button>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <cc1:TabContainer ID="TabContainer1" runat="server" ActiveTabIndex="0">
                                <cc1:TabPanel runat="server" HeaderText="TabPanel1" ID="TabPanel1">
                                    <HeaderTemplate>
                                        UnBlock Amount
                                    </HeaderTemplate>
                                    <ContentTemplate>
                                        <table class="tbMain" >
                                            <tbody >
                                                <tr>
                                                    <td class="tbCell1"></td>
                                                    <td class="tbCell2">
                                                        <asp:Label ID="Label1" runat="server" Text="Fields marked with * are mandatory " CssClass="Criteria" ForeColor="Red"></asp:Label><br />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td colspan="5" align="center">
                                                        <asp:Label ID="lblDate" runat="server" Font-Bold="True"></asp:Label>

                                                    </td>
                                                </tr>
                                                <tr>
                                                     <td class="tbCell1"></td>
                                                    <td class="tbCell2">
                                                         <asp:Label ID="lblSuccessMsg" runat="server" ForeColor="Red"></asp:Label>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="tbCell1">Account Number*</td>
                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="txtAccountNo" runat="server" CssClass="midRangeText" MaxLength="20" Enabled="False"></asp:TextBox>
                                                    </td>

                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="lblAccountName" runat="server" CssClass="tinyText" MaxLength="8" Enabled="False"></asp:TextBox>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="tbCell1">Available Balance*</td>
                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="AailBalance" runat="server" CssClass="tinyText" MaxLength="20"  Enabled="False" Width="148px" Height="30px"></asp:TextBox>
                                                       
                                                    </td>
                                                     <td class="tbCell1">Book Balance*</td>
                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="BookBal" runat="server" CssClass="tinyText"  Enabled="False" MaxLength="20" Width="148px" Height="30px"></asp:TextBox>
                                                       
                                                    </td>

                                                </tr>
                                                <tr>
                                                    <td class="tbCell1">Amount To UnBlock*</td>
                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="txtBlockAmount" runat="server" CssClass="tinyText" MaxLength="20" Enabled="False" Width="148px" Height="30px"></asp:TextBox>

                                                    </td>
                                                    <td class="tbCell1">Reason To UnBlock*</td>
                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="txtBlockReason" runat="server" CssClass="tinyText"  MaxLength="20" Width="148px" Height="30px"></asp:TextBox>

                                                    </td>

                                                </tr>
                                                <tr>
                                                    <td>&nbsp;</td>
                                                    <td>
                                                        <asp:HiddenField ID="HiddenField1" runat="server"></asp:HiddenField>
                                                    </td>
                                                    <td></td>
                                                </tr>
                                            </tbody>
                                        </table>  
                                        <table class="tbMain">
                                            <tbody>
                                                <tr>
                                                    <td class="tbCell1">&nbsp;</td>
                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="txtNewBalance" runat="server" CssClass="tinyText" MaxLength="8" Enabled="False" Visible="False"></asp:TextBox>
                                                    </td>
                                                    <td class="tbCell1">&nbsp;</td>
                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="txtRefNo" runat="server" CssClass="tinyText" MaxLength="8" Enabled="False" Visible="False"></asp:TextBox>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="tbCell1">
                                                        <asp:Button ID="btnBlock" runat="server" Text="UnBlock" CausesValidation="False" CssClass="btn-Save"></asp:Button>
                                                    </td>
                                                </tr>

                                            </tbody>
                                        </table>
                                        <table class="tbMain">
                                            <tbody>
                                                <tr>
                                                    <td class="tbCell1"></td>
                                                    <td class="tbCell2">
                                                        <asp:UpdateProgress ID="UpdateProgress1" runat="server">
                                                            <ProgressTemplate>
                                                                <img src="Img/ajax-loader1.gif" style="width: 35px" /><asp:Label ID="Label3" runat="server" Text="Please Wait..." ForeColor="Red"></asp:Label>
                                                            </ProgressTemplate>
                                                        </asp:UpdateProgress>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </ContentTemplate>
                                </cc1:TabPanel>
                            </cc1:TabContainer>
                        </td>
                    </tr>
                </tbody>

                  <div class="searchHolder" runat="server" id="RadioButtControls">
                                            <asp:RadioButtonList ID="RdoSearch" runat="server" Width="450px" OnSelectedIndexChanged="RdoSearch_SelectedIndexChanged" RepeatDirection="Horizontal">
                                                <asp:ListItem Value="Names">Account Name</asp:ListItem>
                                                <asp:ListItem Value="Account">Account No.</asp:ListItem>
                                            </asp:RadioButtonList>
                                            <caption>
                                                <br />
                                            </caption>
                                        </div>

             <div class="searchHolder" runat="server" id="searchRow">
                                            <div class="row" >
                                                <div class="col-md-6 col-xs-12">
                                                    <asp:TextBox ID="txtSearch" runat="server" Width="349px" OnTextChanged="txtSearch_TextChanged" CssClass="tinyText"></asp:TextBox>
                                                </div>
                                                <div class="col-md-3">
                                                    <asp:Button ID="CmdSearch" OnClick="CmdSearch_Click" runat="server" Width="104px" Text="Search" CausesValidation="False" CssClass="btn-Save"></asp:Button>
                                                </div>
                                                <div class="col-md-3">
                                                    <asp:Button ID="CmdViewAll" OnClick="CmdViewAll_Click" runat="server" Width="96px" Text="View All" CausesValidation="False" CssClass="btn-Save"></asp:Button>
                                                </div>
                                            </div>
                                        </div>

            <div class="centerScroll">
                                            <div class="gridHolder" runat="server" >
                                                <asp:GridView ID="GridView1" runat="server" OnSelectedIndexChanged="GridView1_SelectedIndexChanged"
                                                    EnableTheming="True" CellPadding="3" AutoGenerateColumns="False" AllowPaging="True"
                                                    BackColor="White" BorderColor="white" BorderStyle="Solid" BorderWidth="3px" CssClass="GridView table-striped" ForeColor="Black" GridLines="Vertical">
                                                    <AlternatingRowStyle BackColor="White"></AlternatingRowStyle>
                                                    <Columns>
                                                        <asp:BoundField DataField="id" HeaderText="ID"></asp:BoundField>
                                                        <asp:BoundField DataField="AccountNo" HeaderText="Account No."></asp:BoundField>
                                                        <asp:BoundField DataField="AccountName" HeaderText="Account Name"></asp:BoundField>
                                                        <asp:BoundField DataField="BlockAmount" HeaderText="Block Amount"></asp:BoundField>
                                                        <asp:BoundField DataField="ReasonToBlock" HeaderText="Block Reason"></asp:BoundField>
                                                        <asp:BoundField DataField="BlockDate" HeaderText="Block Date"></asp:BoundField>
                                                        <%--<asp:BoundField DataField="Website" HeaderText="WEBSITE"></asp:BoundField>
                                                        <asp:BoundField DataField="Email" HeaderText=" EMAIL"></asp:BoundField>
                                                        <asp:BoundField DataField="LocalCurrency" HeaderText="LOCAL CURRENCY"></asp:BoundField>
                                                        <asp:BoundField DataField="CurrencyCode" HeaderText="CURRENCY CODE"></asp:BoundField>--%>
                                                        <asp:TemplateField>
                                                            <ItemTemplate>
                                                                <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandArgument='<%# Eval("id") %>' CommandName="edit">Select</asp:LinkButton>
                                                            </ItemTemplate>
                                                        </asp:TemplateField>
                                                    </Columns>
                                                    <EditRowStyle BackColor="#7C6F57"></EditRowStyle>
                                                    <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White"></FooterStyle>
                                                    <HeaderStyle BackColor="#0f3b5a" Font-Bold="True" ForeColor="White"></HeaderStyle>
                                                    <PagerStyle HorizontalAlign="Center" BackColor="#666666" ForeColor="White"></PagerStyle>
                                                    <RowStyle BackColor="#E3EAEB"></RowStyle>
                                                    <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333"></SelectedRowStyle>
                                                </asp:GridView>
                                            </div>
                                        </div>
            </table>

           
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>