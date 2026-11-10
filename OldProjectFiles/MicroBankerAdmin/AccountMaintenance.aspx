<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="AccountMaintenance.aspx.vb" Inherits="MicroBankerAdmin.AccountMaintenance" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
       <asp:UpdateProgress ID="UpdateProgress1" runat="server">
        <ProgressTemplate>
            <img src="Img/ajax-loader1.gif" style="width: 35px" /><asp:Label ID="Label3" runat="server" Text="Please Wait..." ForeColor="Red"></asp:Label>
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>


            <table style="width: 728px">
                <tbody>
                    <tr>
                        <td style="width: 186px; height: 25px; text-align: left">
                            <asp:Button ID="CmdApprovalList" OnClick="CmdApprovalList_Click" runat="server" Width="176px" Text="New Organization" ForeColor="White" BackColor="#5D7B9D" CausesValidation="False" BorderStyle="Inset" BorderColor="Silver"></asp:Button>&nbsp;<br />
                            <asp:Label ID="Label1" runat="server" Width="329px" Text="Fields marked with * are mandatory " CssClass="Criteria" ForeColor="Red"></asp:Label><br />
                        </td>
                        <td style="height: 25px"></td>
                        <td style="height: 25px"></td>
                    </tr>
                    <tr>
                        <td colspan="3">
                            <cc1:TabContainer ID="TabContainer1" runat="server" ActiveTabIndex="0">
                                <cc1:TabPanel runat="server" HeaderText="TabPanel1" ID="TabPanel1">
                                    <HeaderTemplate>
                                        Create Organization
                                    </HeaderTemplate>
                                    <ContentTemplate>
                                        <table style="width: 704px">
                                            <tbody>
                                                <tr>
                                                    <td style="width: 136px; height: 26px">Account Type Code*</td>
                                                    <td style="width: 286px; height: 26px">
                                                        <asp:TextBox ID="txtAccountCode" runat="server" OnTextChanged="txtAccountCode_TextChanged" CssClass="tinyText" MaxLength="14"></asp:TextBox>
                                                        <asp:Button ID="cmasearch" OnClick="cmasearch_Click" runat="server" Text="Search" CausesValidation="False"></asp:Button>
                                                    </td>
                                                    <td style="height: 26px">
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" Display="Dynamic" CssClass="Validator" SetFocusOnError="true" ErrorMessage="*" ControlToValidate="txtAccountCode">Please enter Account Type Code</asp:RequiredFieldValidator>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style="width: 136px; height: 26px">Account Type Name*</td>
                                                    <td style="width: 286px; height: 26px">
                                                        <asp:TextBox ID="txtAccountName" runat="server" CssClass="tinyText"></asp:TextBox>
                                                        &nbsp; </td>
                                                    <td style="height: 26px">
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" Width="240px" Display="Dynamic" CssClass="Validator" SetFocusOnError="true" ErrorMessage="Please Enter Account Name" ControlToValidate="txtAccountName"></asp:RequiredFieldValidator>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style="width: 136px; height: 26px">Dormancy Days*</td>
                                                    <td style="width: 286px; height: 26px">
                                                        <asp:TextBox ID="txtDormancyDays" runat="server" CssClass="tinyText" MaxLength="8"></asp:TextBox>
                                                    </td>
                                                    <td style="height: 26px">
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" Width="232px" Display="Dynamic" CssClass="Validator" SetFocusOnError="true" ErrorMessage="Please Enter Dormancy Days" ControlToValidate="txtDormancyDays"></asp:RequiredFieldValidator>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style="height: 23px" colspan="3">&nbsp;<strong style="font-weight: bold; color: black">Control General Ledgers</strong>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style="width: 136px">Asset GL*</td>
                                                    <td style="width: 286px">
                                                        <asp:TextBox ID="txtAssetGL" runat="server" Width="320px" CssClass="tinyText" TextMode="MultiLine"></asp:TextBox>
                                                    </td>
                                                    <td>
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" Width="208px" Display="Dynamic" CssClass="Validator" SetFocusOnError="true" ErrorMessage="Please Enter Asset GL" ControlToValidate="txtAssetGL"></asp:RequiredFieldValidator>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style="width: 136px; height: 22px;">Liability GL*</td>
                                                    <td style="width: 286px; height: 22px;">
                                                        <asp:TextBox ID="txtLiabilityGL" runat="server" Width="320px" OnTextChanged="txtAdress3_TextChanged" CssClass="tinyText"></asp:TextBox>
                                                    </td>
                                                    <td style="height: 22px">
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" Width="174px" Font-Bold="True" ControlToValidate="txtLiabilityGL">Please Enter Liability GL</asp:RequiredFieldValidator>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style="width: 136px">Income GL*</td>
                                                    <td style="width: 286px">
                                                        <asp:TextBox ID="txtIncomeGL" runat="server" CssClass="tinyText"></asp:TextBox>
                                                    </td>
                                                    <td>
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" Width="230px" Display="Dynamic" CssClass="Validator" SetFocusOnError="true" ErrorMessage="*" ControlToValidate="txtIncomeGL">Please Enter Income GL</asp:RequiredFieldValidator>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style="width: 136px">Expense GL*</td>
                                                    <td style="width: 286px">
                                                        <asp:TextBox ID="txtExpenseGL" runat="server" CssClass="tinyText"></asp:TextBox>
                                                    </td>
                                                    <td>
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" Width="188px" Display="Dynamic" CssClass="Validator" SetFocusOnError="true" ErrorMessage="*" ControlToValidate="txtExpenseGL">Please Enter Expense GL</asp:RequiredFieldValidator>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style="width: 136px; height: 26px">Charge GL*</td>
                                                    <td style="width: 286px; height: 26px">
                                                        <asp:TextBox ID="txtChargeGL" runat="server" OnTextChanged="txtEmail_TextChanged" CssClass="tinyText" MaxLength="10"></asp:TextBox>
                                                    </td>
                                                    <td style="height: 26px">
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" Width="189px" Display="Dynamic" CssClass="Validator" SetFocusOnError="true" ErrorMessage="*" ControlToValidate="txtChargeGL">Please Enter Charge GL</asp:RequiredFieldValidator>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style="width: 136px; height: 26px">Fee GL*</td>
                                                    <td style="width: 286px; height: 26px">
                                                        <asp:TextBox ID="txtFeeGL" runat="server" CssClass="tinyText" MaxLength="14"></asp:TextBox>
                                                    </td>
                                                    <td style="height: 26px">
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator9" runat="server" Width="226px" Display="Dynamic" CssClass="Validator" SetFocusOnError="true" ErrorMessage="*" ControlToValidate="txtFeeGL">Please Enter Fee GL</asp:RequiredFieldValidator>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style="height: 23px" colspan="3">&nbsp;<strong style="font-weight: bold; color: black">Account Interest</strong>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style="width: 136px">Int. Rate</td>
                                                    <td style="width: 286px">
                                                        <asp:TextBox ID="txtIntRate" runat="server" CssClass="tinyText"></asp:TextBox></td>
                                                    <td></td>
                                                </tr>

                                                <tr>
                                                    <td style="width: 136px">Int. Frequency</td>
                                                    <td style="width: 286px">
                                                        <asp:TextBox ID="txtIntFreq" runat="server" CssClass="tinyText"></asp:TextBox></td>
                                                    <td></td>
                                                </tr>
                                                <tr>
                                                    <td style="height: 19px" colspan="3">
                                                        <asp:Label ID="Label2" runat="server" ForeColor="Red"></asp:Label>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style="width: 136px">Int. Accrual Frequency</td>
                                                    <td style="width: 286px">
                                                        <asp:TextBox ID="txtIntAccrualFreq" runat="server" CssClass="tinyText"></asp:TextBox></td>
                                                    <td></td>
                                                </tr>
                                                <tr>
                                                    <td style="height: 19px" colspan="3">
                                                        <asp:Label ID="Label5" runat="server" ForeColor="Red"></asp:Label>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style="height: 23px" colspan="3">&nbsp;<strong style="font-weight: bold; color: black">Interest Rules</strong>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style="width: 136px">Min Balance For Interest</td>
                                                    <td style="width: 286px">
                                                        <asp:RadioButton ID="rdbMinInterestBal" runat="server" />
                                                    </td>
                                                    <td></td>
                                                </tr>
                                                <tr>
                                                    <td style="width: 136px">Min. Account Balance</td>
                                                    <td style="width: 286px">
                                                        <asp:RadioButton ID="rdbMinAccountBal" runat="server" />
                                                    </td>
                                                    <td></td>
                                                </tr>
                                                <tr>
                                                    <td style="width: 136px">No of Debits Allowed Per Month</td>
                                                    <td style="width: 286px">
                                                        <asp:RadioButton ID="rdbDebitsNoPerMonth" runat="server" />
                                                    </td>
                                                    <td></td>
                                                </tr>
                                                <tr>
                                                    <td style="height: 19px" colspan="3">
                                                        <asp:Label ID="Label7" runat="server" ForeColor="Red"></asp:Label>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style="width: 136px; height: 24px"></td>
                                                    <td style="width: 286px; height: 24px">
                                                        <asp:Button ID="Button1" OnClick="Button1_Click" runat="server" Text="Save" CssClass="button"></asp:Button>
                                                        &nbsp;
                                                        <asp:Button ID="Button2" OnClick="Button2_Click" runat="server" Text="Clear" CausesValidation="False" CssClass="button"></asp:Button>
                                                        &nbsp;&nbsp;&nbsp;&nbsp;
                                                        <asp:Button ID="cmddelete" OnClick="cmddelete_Click" runat="server" Width="86px" Text="Delete"></asp:Button>
                                                    </td>
                                                    <td style="height: 24px"></td>
                                                </tr>
                                                <tr>
                                                    <td style="width: 136px; height: 18px"></td>
                                                    <td style="width: 286px; height: 18px">&nbsp;</td>
                                                    <td style="height: 18px"></td>
                                                </tr>
                                                <tr>
                                                    <td colspan="3">&nbsp;</td>
                                                </tr>
                                                <tr>
                                                    <td style="width: 136px"></td>
                                                    <td style="width: 286px">
                                                        <cc1:ConfirmButtonExtender ID="ConfirmButtonExtender1" runat="server" Enabled="True" TargetControlID="Button1" ConfirmText="Are you sure you want to save?"></cc1:ConfirmButtonExtender>
                                                        <cc1:ConfirmButtonExtender ID="ConfirmButtonExtender2" runat="server" Enabled="True" TargetControlID="cmddelete" ConfirmText="Are you sure you want to Delete?"></cc1:ConfirmButtonExtender>
                                                    </td>
                                                    <td></td>
                                                </tr>
                                                <tr>
                                                    <td style="width: 136px">&nbsp;</td>
                                                    <td style="width: 286px">
                                                        <asp:HiddenField ID="HiddenField1" runat="server"></asp:HiddenField>
                                                    </td>
                                                    <td></td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </ContentTemplate>
                                </cc1:TabPanel>
                                <cc1:TabPanel runat="server" HeaderText="TabPanel2" ID="TabPanel2">
                                    <HeaderTemplate>
                                        Organizations
                                    </HeaderTemplate>
                                    <ContentTemplate>
                                        
                                        <asp:RadioButtonList ID="RdoSearch" runat="server" Width="690px" OnSelectedIndexChanged="RdoSearch_SelectedIndexChanged" RepeatDirection="Horizontal">
                                            <asp:ListItem Value="Names">Account Name</asp:ListItem>
                                            <asp:ListItem Value="Account">Account Code</asp:ListItem>
                                        </asp:RadioButtonList><br />

                                        &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;
                                        <asp:TextBox ID="txtSearch" runat="server" Width="349px" OnTextChanged="txtSearch_TextChanged" CssClass="tinyText"></asp:TextBox>

                                        &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;<br />&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;
    <br />
                                        &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;&nbsp;
                                        <asp:Button ID="CmdSearch" OnClick="CmdSearch_Click" runat="server" Width="104px" Text="Search" CausesValidation="False" CssClass="button"></asp:Button>
                                        &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;
    <asp:Button ID="CmdViewAll" OnClick="CmdViewAll_Click" runat="server" Width="96px" Text="View All" CausesValidation="False" CssClass="button"></asp:Button>
                                        <br />
                                        <br />

                                        <asp:GridView ID="GridView1" runat="server" Width="696px" ForeColor="#333333" Font-Size="XX-Small" OnSelectedIndexChanged="GridView1_SelectedIndexChanged" EnableTheming="True" GridLines="None" CellPadding="4" AutoGenerateColumns="False" AllowPaging="True">
                                            <AlternatingRowStyle BackColor="White"></AlternatingRowStyle>
                                            <Columns>
                                                <asp:BoundField DataField="AccountCode" HeaderText="ACCOUNT CODE"></asp:BoundField>
                                                <asp:BoundField DataField="AccountName" HeaderText="ACCOUNT NAME"></asp:BoundField>
                                                <asp:BoundField DataField="DormancyDays" HeaderText="DORMANCY DAYS"></asp:BoundField>
                                                <asp:BoundField DataField="AssetGL" HeaderText="ASSET GL"></asp:BoundField>
                                                <asp:BoundField DataField="LiabilityGL" HeaderText="LIABILITY GL"></asp:BoundField>
                                                <asp:BoundField DataField="IncomeGL" HeaderText="INCOME GL"></asp:BoundField>
                                                <asp:BoundField DataField="ExpenseGL" HeaderText="EXPENSE GL"></asp:BoundField>
                                                <asp:BoundField DataField="ChargeGL" HeaderText=" CHARGE GL"></asp:BoundField>
                                                <asp:BoundField DataField="FeeGL" HeaderText="FEE GL"></asp:BoundField>
                                                <asp:BoundField DataField="IntRate" HeaderText="INTEREST RATE"></asp:BoundField>
                                                <asp:TemplateField>
                                                    <ItemTemplate>
                                                        <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandArgument='<%# Eval("AccountCode") %>' CommandName="edit">Edit</asp:LinkButton>
                                                    </ItemTemplate>
                                                </asp:TemplateField>
                                            </Columns>

                                            <EditRowStyle BackColor="#7C6F57"></EditRowStyle>

                                            <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White"></FooterStyle>

                                            <HeaderStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White"></HeaderStyle>

                                            <PagerStyle HorizontalAlign="Center" BackColor="#666666" ForeColor="White"></PagerStyle>

                                            <RowStyle BackColor="#E3EAEB"></RowStyle>

                                            <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333"></SelectedRowStyle>
                                        </asp:GridView>
                                        <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:ConnectionString %>" ProviderName="<%$ ConnectionStrings:ConnectionString.ProviderName %>" SelectCommand="select OrgCode,OrgName,Address1,Address2,Address3,PhoneNo,Website,Email,LocalCurrency,CurrencyCode from Organisation"></asp:SqlDataSource>
                                    </ContentTemplate>
                                </cc1:TabPanel>
                            </cc1:TabContainer></td>
                    </tr>
                    <tr>
                        <td style="width: 186px; height: 16px"></td>
                        <td style="height: 16px"></td>
                        <td style="height: 16px"></td>
                    </tr>
                </tbody>
            </table>



        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
