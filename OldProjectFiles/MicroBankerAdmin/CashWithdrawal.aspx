<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="CashWithdrawal.aspx.vb" Inherits="MicroBankerAdmin.CashWithdrawal" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>


            <table class="tbMain">
                <tbody>
                    <tr>
                        <td>
                            <asp:Button ID="CmdApprovalList" runat="server" Text="New Cash Withdrawal" CausesValidation="False" CssClass="btn-New"></asp:Button>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <cc1:TabContainer ID="TabContainer1" runat="server" ActiveTabIndex="0">
                                <cc1:TabPanel runat="server" HeaderText="TabPanel1" ID="TabPanel1">
                                    <HeaderTemplate>
                                        Withdraw Cash
                                    </HeaderTemplate>
                                    <ContentTemplate>


                                        <table class="tbMain">
                                            <tbody>
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
                                                    <td class="tbCell1">Transaction Code</td>
                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="txtTransactionCode" runat="server" CssClass="ShortText" MaxLength="4"></asp:TextBox>
                                                        <asp:Button ID="btncheckTranCode" runat="server" CssClass="btn-Save" Text="Check" CausesValidation="False"></asp:Button>
                                                    </td>


                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="lblDescription" runat="server" CssClass="tinyText" TextMode="MultiLine" MaxLength="20" Enabled="False"></asp:TextBox>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="tbCell1">Account Number*</td>
                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="txtAccountNo" runat="server" CssClass="tinyText" MaxLength="20" Width="148px" Height="30px"></asp:TextBox>
                                                        <asp:Button ID="btnCheckAccNo" runat="server" CssClass="btn-Save" Text="Check" CausesValidation="False"></asp:Button>
                                                    </td>

                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="lblAccountName" runat="server" CssClass="tinyText" MaxLength="8" Enabled="False"></asp:TextBox>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="tbCell1">Amount</td>
                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="txtAmount" runat="server" CssClass="tinyText"></asp:TextBox>

                                                    </td>
                                                    <td class="tbCell2">
                                                        <asp:Label ID="lblSuccessMsg" ForeColor="Red" runat="server" />

                                                    </td>
                                                </tr>

                                                <tr>
                                                    <td style="height: 19px" colspan="3">
                                                        <asp:Label ID="Label4" runat="server" ForeColor="Red"></asp:Label>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                        <table class="tbMain">
                                            <tbody>
                                                <tr>
                                                    <td class="tbCell1">
                                                        <asp:Button ID="btnCancel" runat="server" Text="Clear" CausesValidation="False" CssClass="btn-Cancel"></asp:Button>
                                                    </td>
                                                    <td class="tbCell1">
                                                        <asp:Button ID="btnPost" runat="server" Width="86px" Text="Post" CssClass="btn-Save"></asp:Button>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                        <table class="tbMain" style="width:100%">
                                            <tbody>
                                                <tr>
                                                    <td class="tbCell1">
                                                        <div style="width: 98%; border: 1px solid blue; margin: 20px 0;overflow: scroll">
                                                            <asp:GridView ID="CashTxnGridView" runat="server" AutoGenerateColumns="False"
                                                                            OnRowCreated="CashTxnGridView_RowCreated" Width="100%">
                                                                            <Columns>
                                                                                <asp:BoundField DataField="RowNumber" HeaderText="S.No" >
                                                                                <HeaderStyle Width="5%" />
                                                                                <ItemStyle HorizontalAlign="Center" Width="5%" />
                                                                                </asp:BoundField>
                                                                                <asp:TemplateField HeaderText="Account No">
                                                                                    <ItemTemplate>
                                                                                        <asp:Label ID="AccountNoLabel" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "Account No") %>'></asp:Label>
                                                                                         <%--<asp:TextBox ID="AccountNo_TextBox" Width="100%" runat="server" ReadOnly="true"></asp:TextBox>--%>
                                                                                    </ItemTemplate>
                                                                                    <HeaderStyle Width="15%" />
                                                                                    <ItemStyle Width="15%" />
                                                                                </asp:TemplateField>
                                                                                <asp:TemplateField HeaderText="ACCOUNT NAME">
                                                                                    <ItemTemplate>
                                                                                        <asp:Label ID="accountNameLabel" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "ACCOUNT NAME") %>'></asp:Label>
                                                                                       <%--<asp:TextBox ID="acctName_TextBox" Width="100%" runat="server" ReadOnly="true"></asp:TextBox>--%>
                                                                                    </ItemTemplate>
                                                                                    <HeaderStyle Width="20%" />
                                                                                    <ItemStyle Width="20%" />
                                                                                </asp:TemplateField>
                                                                                <asp:TemplateField HeaderText="TXN TYPE">
                                                                                    <ItemTemplate>
                                                                                        <asp:Label ID="TxnTypeLabel" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "TXN TYPE") %>'></asp:Label>
                                                                                       <%-- <asp:TextBox ID="TXN_TYPE_TextBox" runat="server" Width="98%" ReadOnly="false" 
                                                                                            AutoPostBack="True"></asp:TextBox>--%>
                                                                                    </ItemTemplate>
                                                                                    <HeaderStyle Width="10%" />
                                                                                    <ItemStyle Width="10%" HorizontalAlign="Center" />
                                                                                </asp:TemplateField>
                                                                                <asp:TemplateField HeaderText="Amount">
                                                                                    <ItemTemplate>
                                                                                        <asp:Label ID="AmountLabel" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "Amount") %>'></asp:Label>
                                                                                        <%--<asp:TextBox ID="Amount_TextBox" runat="server" Width="98%" ReadOnly="true"></asp:TextBox>--%>
                                                                                    </ItemTemplate>
                                                                                    <HeaderStyle Width="10%" />
                                                                                    <ItemStyle Width="10%" HorizontalAlign="Center" />
                                                                                </asp:TemplateField>
                                                                                
                                                                                <asp:TemplateField HeaderText="Desc">
                                                                                    <ItemTemplate>
                                                                                        <asp:Label ID="DescLabel" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "Desc") %>'></asp:Label>
                                                                                        <%--<asp:TextBox ID="DescTextBox" runat="server" Width="99%" ReadOnly="true"></asp:TextBox>--%>
                                                                                    </ItemTemplate>
                                                                                    <HeaderStyle Width="40%" />
                                                                                    <ItemStyle Width="40%" />
                                                                                </asp:TemplateField>
                                                                              
                                                                                
                                                                            </Columns>
                                                                            <HeaderStyle CssClass="header"></HeaderStyle>
                                                                            <RowStyle CssClass="rows"></RowStyle>
                                                                        </asp:GridView>
                                                        </div>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                        <table class="tbMain">
                                            <tbody>
                                                <tr>
                                                    <td class="tbCell1">New Balance</td>
                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="txtNewBalance" runat="server" CssClass="tinyText" MaxLength="8" Enabled="False"></asp:TextBox>
                                                    </td>
                                                    <td class="tbCell1">Reference Number</td>
                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="txtRefNo" runat="server" CssClass="tinyText" MaxLength="8" Enabled="False"></asp:TextBox>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="tbCell1">
                                                        <asp:Button ID="btnSave" runat="server" Text="Save" CausesValidation="False" CssClass="btn-Save"></asp:Button>
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
            </table>



        </ContentTemplate>
    </asp:UpdatePanel>




    <%-- <div style="border: 1px solid white; width: 98%; padding: 3px;">
        <asp:Table runat="server" GridLines="none" Width="75%" OnDataBinding="btncheckTranCode_Click">
            <asp:TableRow>
                <asp:TableCell runat="server" ID="lblDate" ColumnSpan="5" HorizontalAlign="Center">
                <%--'Tuesday 10 August 2021  
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell Width="20%">
                    Transaction Code
                    <asp:Label runat="server" ForeColor="Red">*</asp:Label>
                </asp:TableCell>
                <asp:TableCell Width="10%">
                    <asp:TextBox ID="txtTransactionCode" runat="server" CssClass="ShortText"></asp:TextBox>
                </asp:TableCell>
                <asp:TableCell Width="10%">
                    <asp:Button ID="btncheckTranCode" runat="server" Width="100%" CssClass="btn-Check" Text="Check" />
                </asp:TableCell>
                <asp:TableCell Width="20%" HorizontalAlign="Left">
                    Description
                </asp:TableCell>
                <asp:TableCell runat="server" ID="lblDescription" Width="20%">
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell>
                   Account No 
                </asp:TableCell>
                <asp:TableCell>
                    <asp:TextBox ID="txtAccountNo" runat="server" CssClass="ShortText"></asp:TextBox>
                </asp:TableCell>
                <asp:TableCell>
                    <asp:Button ID="btnCheckAccNo" runat="server" Width="100%" CssClass="btn-Check" Text="Check" />
                </asp:TableCell>
                <asp:TableCell HorizontalAlign="Left">
                    Account Name
                </asp:TableCell>
                <asp:TableCell runat="server" ID="lblAccountName">
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell>
                   Amount
                </asp:TableCell>
                <asp:TableCell ColumnSpan="2">
                    <asp:TextBox ID="txtAmount" runat="server" Width="100%"></asp:TextBox>
                </asp:TableCell>
                <asp:TableCell HorizontalAlign="Right">
                    <asp:Button ID="btnCancel" runat="server" CssClass="btn-Cancel" Text="Clear" />
                </asp:TableCell>
                <asp:TableCell>
                    <asp:Button ID="btnPost" runat="server" CssClass="btn-Save" Text="Post" />
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell ColumnSpan="5">
                    <div style="width: 98%; border: 1px solid blue; margin: 20px 0;">
                        <asp:GridView ID="GridView1" runat="server" EnableTheming="True" CellPadding="4" AutoGenerateColumns="False" AllowPaging="True"
                            BackColor="White" BorderColor="white" BorderStyle="Solid" BorderWidth="3px" CssClass="GridView table-striped" ForeColor="Black" GridLines="Vertical">
                            <AlternatingRowStyle BackColor="White"></AlternatingRowStyle>
                            <EditRowStyle BackColor="#7C6F57"></EditRowStyle>
                            <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White"></FooterStyle>
                            <HeaderStyle BackColor="#0f3b5a" Font-Bold="True" ForeColor="White"></HeaderStyle>
                            <PagerStyle HorizontalAlign="Center" BackColor="#666666" ForeColor="White"></PagerStyle>
                            <RowStyle BackColor="#E3EAEB"></RowStyle>
                            <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333"></SelectedRowStyle>
                        </asp:GridView>
                    </div>
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell runat="server" ID="lblSuccessMsg" ColumnSpan="4" ForeColor="Red">
                    <%--Record Saved Successfully!
                </asp:TableCell>
                <asp:TableCell>
                    <asp:Button ID="btnSave" runat="server" CssClass="btn-Save" Text="Save" />
                </asp:TableCell>
            </asp:TableRow>
        </asp:Table>
  </div>--%>
</asp:Content>
