<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="ProductMaintenance.aspx.vb" Inherits="MicroBankerAdmin.ProductMaintenance" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <table class="tbMain">
                <tbody>
                    <tr>
                        <td style="text-align: left">
                            <asp:Button ID="CmdApprovalList" OnClick="CmdApprovalList_Click" runat="server" Text="New Product" CausesValidation="False" CssClass="btn-New"></asp:Button>&nbsp;<br />
                        </td>
                    </tr>
                    <tr>
                        <td colspan="3">
                            <cc1:TabContainer ID="TabContainer1" runat="server" ActiveTabIndex="0">
                                <cc1:TabPanel runat="server" HeaderText="TabPanel1" ID="TabPanel1">
                                    <HeaderTemplate>
                                        Creat Product
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
                                            </tbody>
                                        </table>

                                        <div class="tbMain">
                                            <div class="row">

                                                <div class="col-md-6 col-xs-12">
                                                    <fieldset>
                                                        <legend>
                                                            <asp:Label ID="Label11" runat="server" />General Details</legend>
                                                        <table class="tbMain">
                                                            <tbody>
                                                                <tr>
                                                                    <td class="tbCell1">Select Product:<b class="boldStar">*</b> </td>
                                                                    <td class="tbCell2">
                                                                        <asp:DropDownList ID="drpProduct" runat="server" CssClass="tinyText">
                                                                        </asp:DropDownList>

                                                                        <asp:Label ID="lblArea" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator20" runat="server" Display="Dynamic" CssClass="Validator" SetFocusOnError="True" ErrorMessage="*" ControlToValidate="drpProduct">Please enter select product</asp:RequiredFieldValidator>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td class="tbCell1">Account Type Code:<b class="boldStar">*</b> </td>
                                                                    <td class="tbCell2">
                                                                        <asp:TextBox ID="txtAccountCode" runat="server" OnTextChanged="txtAccountCode_TextChanged" CssClass="tinyText" MaxLength="4"></asp:TextBox>
                                                                        <asp:Button ID="cmasearch" OnClick="cmasearch_Click" runat="server" Text="Search" CausesValidation="False" Visible="False"></asp:Button>

                                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" Display="Dynamic" CssClass="Validator" SetFocusOnError="True" ErrorMessage="*" ControlToValidate="txtAccountCode">Please enter account type code</asp:RequiredFieldValidator>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td class="tbCell1">Account Type Name:<b class="boldStar">*</b> </td>
                                                                    <td class="tbCell2">
                                                                        <asp:TextBox ID="txtAccountName" runat="server" CssClass="tinyText"></asp:TextBox>

                                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" Width="240px" Display="Dynamic" CssClass="Validator" SetFocusOnError="True" ErrorMessage="Please enter account name" ControlToValidate="txtAccountName"></asp:RequiredFieldValidator>
                                                                    </td>
                                                                </tr>

                                                                <tr>
                                                                    <td class="tbCell1">Product Code:<b class="boldStar">*</b> </td>
                                                                    <td class="tbCell2">
                                                                        <asp:TextBox ID="txtPrefix" runat="server" OnTextChanged="txtPrefix_TextChanged" CssClass="tinyText" MaxLength="4"></asp:TextBox>

                                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator10" runat="server" Display="Dynamic" CssClass="Validator" SetFocusOnError="True" ErrorMessage="*" ControlToValidate="txtPrefix">Please enter account type code</asp:RequiredFieldValidator>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td class="tbCell1">Dormancy Days</td>
                                                                    <td class="tbCell2">
                                                                        <asp:TextBox ID="txtDormancyDays" runat="server" CssClass="tinyText" MaxLength="8">0</asp:TextBox>

                                                                    </td>
                                                                </tr>

                                                            </tbody>
                                                        </table>
                                                    </fieldset>

                                                    <fieldset>
                                                        <legend>
                                                            <asp:Label ID="Label8" runat="server" />Control General Ledgers</legend>

                                                        <table class="tbMain">
                                                            <tbody>
                                                                <tr>
                                                                    <td class="tbCell1">GL Type</td>
                                                                    <td class="tbCell2">
                                                                        <asp:DropDownList ID="drpglType" runat="server" CssClass="tinyText">
                                                                            <asp:ListItem></asp:ListItem>
                                                                            <asp:ListItem Value="A">ASSET</asp:ListItem>
                                                                            <asp:ListItem Value="L">LIABILITY</asp:ListItem>
                                                                        </asp:DropDownList>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td class="tbCell1">Asset GL:<b class="boldStar"></b> </td>
                                                                    <td class="tbCell2">
                                                                        <asp:TextBox ID="txtAssetGL" runat="server" CssClass="tinyText" MaxLength="14"></asp:TextBox>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td class="tbCell1">Liability GL:<b class="boldStar"></b> </td>
                                                                    <td class="tbCell2">
                                                                        <asp:TextBox ID="txtLiabilityGL" runat="server" OnTextChanged="txtAdress3_TextChanged" CssClass="tinyText" MaxLength="14"></asp:TextBox>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td class="tbCell1">Income/Interest Received</td>
                                                                    <td class="tbCell2">
                                                                        <asp:TextBox ID="txtIncomeGL" runat="server" CssClass="tinyText" MaxLength="14"></asp:TextBox>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td class="tbCell1">Expense/Interest Paid</td>
                                                                    <td class="tbCell2">
                                                                        <asp:TextBox ID="txtExpenseGL" runat="server" CssClass="tinyText" MaxLength="14"></asp:TextBox>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td class="tbCell1">Charge GL</td>
                                                                    <td class="tbCell2">
                                                                        <asp:TextBox ID="txtChargeGL" runat="server" OnTextChanged="txtEmail_TextChanged" CssClass="tinyText" MaxLength="14"></asp:TextBox>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td class="tbCell1">Fee GL</td>
                                                                    <td class="tbCell2">
                                                                        <asp:TextBox ID="txtFeeGL" runat="server" CssClass="tinyText" MaxLength="14"></asp:TextBox>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td class="tbCell1">Interest Receivable</td>
                                                                    <td class="tbCell2">
                                                                        <asp:TextBox ID="txtIntReceivable" runat="server" CssClass="tinyText" MaxLength="14"></asp:TextBox>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td class="tbCell1">Interest Payable</td>
                                                                    <td class="tbCell2">
                                                                        <asp:TextBox ID="txtIntPayable" runat="server" CssClass="tinyText" MaxLength="14"></asp:TextBox>
                                                                    </td>
                                                                </tr>

                                                            </tbody>
                                                        </table>
                                                    </fieldset>
                                                </div>
                                                <div class="col-md-6 col-xs-12">
                                                    <fieldset>
                                                        <legend>
                                                            <asp:Label ID="Label4" runat="server" />Account Interest</legend>
                                                        <table class="tbMain">
                                                            <tbody>
                                                                <tr>
                                                                    <td class="tbCell1">Int. Rate</td>
                                                                    <td class="tbCell2">
                                                                        <asp:TextBox ID="txtIntRate" runat="server" CssClass="tinyText" MaxLength="6">0.00</asp:TextBox></td>
                                                                </tr>

                                                                <tr>
                                                                    <td class="tbCell1">Int. Applied Frequency</td>
                                                                    <td class="tbCell2">
                                                                        <asp:DropDownList ID="drpIntAppliedFreq" runat="server" CssClass="tinyText">
                                                                            <asp:ListItem></asp:ListItem>
                                                                            <asp:ListItem Value="M">MONTHLY</asp:ListItem>
                                                                            <asp:ListItem Value="S">SEMI ANNUALLY</asp:ListItem>
                                                                            <asp:ListItem Value="Q">QUARTERLY</asp:ListItem>
                                                                            <asp:ListItem Value="Y">YEARLY</asp:ListItem>
                                                                            <asp:ListItem Value="R">REPAYMENT DATE</asp:ListItem>
                                                                        </asp:DropDownList></td> 
                                                                </tr>
                                                                <tr>
                                                                    <td class="tbCell1">Int. Accrual Frequency</td>
                                                                    <td class="tbCell2">
                                                                        <asp:DropDownList ID="drpIntAccrualFreq" runat="server" CssClass="tinyText">
                                                                            <asp:ListItem></asp:ListItem>
                                                                            <asp:ListItem Value="M">MONTHLY</asp:ListItem>
                                                                            <asp:ListItem Value="S">SEMI ANNUALLY</asp:ListItem>
                                                                            <asp:ListItem Value="Q">QUARTERLY</asp:ListItem>
                                                                            <asp:ListItem Value="Y">YEARLY</asp:ListItem>
                                                                            <asp:ListItem Value="R">REPAYMENT DATE</asp:ListItem>
                                                                        </asp:DropDownList></td> 
                                                                </tr>
                                                                <tr>
                                                                    <td class="tbCell1">
                                                                        <asp:Label ID="Label5" runat="server" ForeColor="Red"></asp:Label>
                                                                    </td>
                                                                </tr>
                                                            </tbody>
                                                        </table>
                                                    </fieldset>
                                                </div>
                                                <div class="col-md-6 col-xs-12" style="top: 50px;">
                                                    <fieldset>
                                                        <legend>
                                                            <asp:Label ID="Label6" runat="server" />Interest Rules</legend>

                                                        <table class="tbMain">
                                                            <tbody>
                                                                <tr>
                                                                    <td class="tbCell1">Min Balance For Interest</td>
                                                                    <td class="tbCell2">
                                                                        <asp:TextBox ID="txtMinIntBal" runat="server" CssClass="tinyText" MaxLength="6">0.00</asp:TextBox></td>
                                                                </tr>
                                                                <tr>
                                                                    <td class="tbCell1">Min. Account Balance</td>
                                                                    <td class="tbCell2">
                                                                        <asp:TextBox ID="txtMinAccBal" runat="server" CssClass="tinyText" MaxLength="6">0.00</asp:TextBox></td>
                                                                </tr>
                                                                <tr>
                                                                    <td class="tbCell1">No of Debits Allowed Per Month</td>
                                                                    <td class="tbCell2">
                                                                        <asp:TextBox ID="txtDebitNoPerMnth" runat="server" CssClass="tinyText" MaxLength="6">0</asp:TextBox></td>
                                                                </tr>
                                                                <tr>
                                                                    <td class="tbCell1">
                                                                        <asp:Label ID="lblMessage" runat="server" ForeColor="Red"></asp:Label>
                                                                    </td>
                                                                </tr>
                                                            </tbody>
                                                        </table>
                                                    </fieldset>
                                                </div>
                                            </div>
                                        </div>

                                        <table class="tbMain">
                                            <tbody>
                                                <tr>
                                                    <td colspan="3">
                                                        <asp:Button ID="btnSave" OnClick="btnSave_Click" runat="server" Text="Save" CssClass="btn-Save"></asp:Button>
                                                    </td>
                                                    <td colspan="3">
                                                        <asp:Button ID="btnClear" OnClick="btnClear_Click" runat="server" Text="Clear" CausesValidation="False" CssClass="btn-Cancel"></asp:Button>
                                                    </td>
                                                    <td colspan="3">
                                                        <asp:Button ID="cmddelete" OnClick="cmddelete_Click" runat="server" Text="Delete" CssClass="btn-Cancel"></asp:Button>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <asp:UpdateProgress ID="UpdateProgress1" runat="server">
                                                            <ProgressTemplate>
                                                                <img src="Img/ajax-loader1.gif" style="width: 35px" /><asp:Label ID="Label3" runat="server" Text="Please Wait..." ForeColor="Red"></asp:Label>
                                                            </ProgressTemplate>
                                                        </asp:UpdateProgress>
                                                    </td>
                                                </tr>

                                                <tr>
                                                    <td>
                                                        <asp:Label ID="Label2" runat="server" ForeColor="Red"></asp:Label>
                                                    </td>
                                                </tr>

                                                <tr>
                                                    <td colspan="3">&nbsp;</td>
                                                </tr>
                                                <tr>
                                                    <td class="tbCell1"></td>
                                                    <td class="tbCell2">
                                                        <cc1:ConfirmButtonExtender ID="ConfirmButtonExtender1" runat="server" Enabled="True" TargetControlID="btnSave" ConfirmText="Are you sure you want to save?"></cc1:ConfirmButtonExtender>
                                                        <cc1:ConfirmButtonExtender ID="ConfirmButtonExtender2" runat="server" Enabled="True" TargetControlID="cmddelete" ConfirmText="Are you sure you want to Delete?"></cc1:ConfirmButtonExtender>
                                                    </td>

                                                </tr>
                                                <tr>
                                                    <td class="tbCell1">&nbsp;</td>
                                                    <td class="tbCell2">
                                                        <asp:HiddenField ID="HiddenField1" runat="server"></asp:HiddenField>
                                                    </td>

                                                </tr>
                                            </tbody>
                                        </table>
                                    </ContentTemplate>
                                </cc1:TabPanel>
                                <cc1:TabPanel runat="server" HeaderText="TabPanel2" ID="TabPanel2">
                                    <HeaderTemplate>
                                        View Products
                                    </HeaderTemplate>
                                    <ContentTemplate>


                                        <div class="searchHolder">
                                            <asp:RadioButtonList ID="RdoSearch" runat="server" Width="690px" OnSelectedIndexChanged="RdoSearch_SelectedIndexChanged" RepeatDirection="Horizontal">
                                                <asp:ListItem Value="Names">Account Name</asp:ListItem>
                                                <asp:ListItem Value="Account">Account Code</asp:ListItem>
                                            </asp:RadioButtonList><br />
                                        </div>


                                        <div class="searchHolder">
                                            <div class="row">
                                                <div class="col-md-6">
                                                    <asp:TextBox ID="txtSearch" runat="server" OnTextChanged="txtSearch_TextChanged" CssClass="tinyText"></asp:TextBox>
                                                </div>
                                                <div class="col-md-3">
                                                    <asp:Button ID="CmdSearch" OnClick="CmdSearch_Click" runat="server" Text="Search" CausesValidation="False" CssClass="btn-Save"></asp:Button>
                                                </div>
                                                <div class="col-md-3">
                                                    <asp:Button ID="CmdViewAll" OnClick="CmdViewAll_Click" runat="server" Text="View All" CausesValidation="False" CssClass="btn-Save"></asp:Button>

                                                </div>
                                            </div>
                                        </div>

                                        <div class="gridHolder">
                                            <asp:GridView ID="GridView1" runat="server" OnSelectedIndexChanged="GridView1_SelectedIndexChanged" EnableTheming="True" CellPadding="4" AutoGenerateColumns="False" AllowPaging="True"
                                                BackColor="White" BorderColor="white" BorderStyle="Solid" BorderWidth="3px" CssClass="GridView table-striped" ForeColor="Black" GridLines="Vertical">
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

                                                <HeaderStyle BackColor="#0f3b5a" Font-Bold="True" ForeColor="White"></HeaderStyle>

                                                <PagerStyle HorizontalAlign="Center" BackColor="#666666" ForeColor="White"></PagerStyle>

                                                <RowStyle BackColor="#E3EAEB"></RowStyle>

                                                <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333"></SelectedRowStyle>
                                            </asp:GridView>
                                            <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:ConnectionString %>" ProviderName="<%$ ConnectionStrings:ConnectionString.ProviderName %>" SelectCommand="select OrgCode,OrgName,Address1,Address2,Address3,PhoneNo,Website,Email,LocalCurrency,CurrencyCode from Organisation"></asp:SqlDataSource>
                                    </ContentTemplate>
                                </cc1:TabPanel>
                            </cc1:TabContainer></td>
                    </tr>
                </tbody>
            </table>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
