<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="CreatBranch.aspx.vb" Inherits="MicroBankerAdmin.CreatBranch" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <table class="tbMain">
                <tbody>
                    <tr>
                        <td style="width: 186px; height: 25px; text-align: left">
                            <asp:Button ID="CmdApprovalList" OnClick="CmdApprovalList_Click" runat="server" Text="New Branch" CausesValidation="False" CssClass="btn-New"></asp:Button>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="3">
                            <cc1:TabContainer ID="TabContainer1" runat="server" ActiveTabIndex="0">
                                <cc1:TabPanel runat="server" HeaderText="TabPanel1" ID="TabPanel1">
                                    <HeaderTemplate>
                                        Create Branch
                                    </HeaderTemplate>
                                    <ContentTemplate>
                                        <table class="tbMain">
                                            <tbody>
                                                <tr>
                                                    <td class="CriteriaCell" colspan="6">
                                                        <asp:Label ID="Label1" runat="server" Text="Fields marked with * are mandatory " CssClass="Criteria" ForeColor="Red"></asp:Label><br />
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                        <div class="tbMain">
                                            <div class="row">
                                                <div class="col-md-6 col-xs-12">
                                                    <table class="tbMain">
                                                        <tbody>
                                                            <tr>
                                                                <td class="tbCell1">Branch Code:<b class="boldStar">*</b></td>
                                                                <td class="tbCell2">
                                                                    <asp:TextBox ID="txtBranchCode" runat="server" OnTextChanged="txtBranchCode_TextChanged" CssClass="tinyText" MaxLength="3"></asp:TextBox>
                                                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" Display="Dynamic" CssClass="Validator" SetFocusOnError="true" ErrorMessage="*" ControlToValidate="txtBranchCode">Please enter Branch Code</asp:RequiredFieldValidator>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td class="tbCell1">Branch Name:<b class="boldStar">*</b></td>
                                                                <td class="tbCell2">
                                                                    <asp:TextBox ID="txtBranchName" runat="server" CssClass="tinyText"></asp:TextBox>
                                                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" Display="Dynamic" CssClass="Validator" SetFocusOnError="true" ErrorMessage="Please Enter Branch Name" ControlToValidate="txtBranchName"></asp:RequiredFieldValidator>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td class="tbCell1">Address 1:<b class="boldStar">*</b></td>
                                                                <td class="tbCell2">
                                                                    <asp:TextBox ID="txtAdress1" runat="server" CssClass="tinyText"></asp:TextBox>
                                                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" Display="Dynamic" CssClass="Validator" SetFocusOnError="true" ErrorMessage="Please Enter Adress 1" ControlToValidate="txtAdress1"></asp:RequiredFieldValidator>
                                                                </td>
                                                            </tr>
                                                        </tbody>
                                                    </table>
                                                </div>
                                                <div class="col-md-6 col-xs-12">
                                                    <table class="tbMain">
                                                        <tbody>
                                                            <tr>
                                                                <td class="tbCell1">Address 2</td>
                                                                <td class="tbCell2">
                                                                    <asp:TextBox ID="txtAdress2" runat="server" CssClass="tinyText" TextMode="MultiLine"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td class="tbCell1">Address 3</td>
                                                                <td class="tbCell2">
                                                                    <asp:TextBox ID="txtAdress3" runat="server" Width="320px" OnTextChanged="txtAdress3_TextChanged" CssClass="tinyText"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td class="tbCell1">Phone Number</td>
                                                                <td class="tbCell2">
                                                                    <asp:TextBox ID="txtPhoneNo" runat="server" CssClass="tinyText"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td style="height: 19px">
                                                                    <asp:Label ID="lblMessage" runat="server" ForeColor="Red"></asp:Label>
                                                                </td>
                                                            </tr>
                                                        </tbody>
                                                    </table>
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
                                                    <td class="tbCell1"></td>
                                                    <td class="tbCell2">
                                                        <asp:UpdateProgress ID="UpdateProgress1" runat="server">
                                                            <ProgressTemplate>
                                                                <img src="Img/ajax-loader1.gif" style="width: 35px" /><asp:Label ID="Label3" runat="server" Text="Please Wait..." ForeColor="Red"></asp:Label>
                                                            </ProgressTemplate>
                                                        </asp:UpdateProgress>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td colspan="3">&nbsp;</td>
                                                </tr>
                                                <tr>
                                                    <td></td>
                                                    <td>
                                                        <cc1:ConfirmButtonExtender ID="ConfirmButtonExtender1" runat="server" Enabled="True" TargetControlID="btnSave" ConfirmText="Are you sure you want to save?"></cc1:ConfirmButtonExtender>
                                                        <cc1:ConfirmButtonExtender ID="ConfirmButtonExtender2" runat="server" Enabled="True" TargetControlID="cmddelete" ConfirmText="Are you sure you want to Delete?"></cc1:ConfirmButtonExtender>
                                                    </td>
                                                    <td></td>
                                                </tr>
                                                <tr>
                                                    <td></td>
                                                    <td>
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
                                        Branches
                                    </HeaderTemplate>
                                    <ContentTemplate>
                                        <div class="searchHolder">
                                            <asp:RadioButtonList ID="RdoSearch" runat="server" Width="690px" OnSelectedIndexChanged="RdoSearch_SelectedIndexChanged" RepeatDirection="Horizontal">
                                                <asp:ListItem Value="Names">Branch Name</asp:ListItem>
                                                <asp:ListItem Value="Account">Branch Code</asp:ListItem>
                                            </asp:RadioButtonList>
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
                                                    <asp:BoundField DataField="BranchCode" HeaderText="BRANCH CODE"></asp:BoundField>
                                                    <asp:BoundField DataField="BranchName" HeaderText="BRANCH NAME"></asp:BoundField>
                                                    <asp:BoundField DataField="Address1" HeaderText="ADDRESS 1"></asp:BoundField>
                                                    <asp:BoundField DataField="Address2" HeaderText="ADDRESS 2"></asp:BoundField>
                                                    <asp:BoundField DataField="Address3" HeaderText="ADDRESS 3"></asp:BoundField>
                                                    <asp:BoundField DataField="PhoneNo" HeaderText="PHONE NUMBER"></asp:BoundField>
                                                    <asp:TemplateField>
                                                        <ItemTemplate>
                                                            <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandArgument='<%# Eval("BranchCode") %>' CommandName="edit">Edit</asp:LinkButton>
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
                                        </div>
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
