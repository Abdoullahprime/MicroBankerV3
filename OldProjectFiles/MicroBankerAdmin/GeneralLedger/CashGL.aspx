<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="CashGL.aspx.vb" Inherits="MicroBankerAdmin.CashGL" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>



            <table class="tbMain">
                <tbody>
                    <tr>
                        <td>
                            <asp:Button ID="CmdApprovalList" OnClick="CmdApprovalList_Click" runat="server" Text="New Cash GL" CausesValidation="False" CssClass="btn-New"></asp:Button>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <cc1:TabContainer ID="TabContainer1" runat="server" ActiveTabIndex="0">
                                <cc1:TabPanel runat="server" HeaderText="TabPanel1" ID="TabPanel1">
                                    <HeaderTemplate>
                                        Create GL
                                    </HeaderTemplate>
                                    <ContentTemplate>

                                        <table class="tbMain">
                                            <tbody>
                                                 <tr>
                                                    <td class="tbCell1"></td>
                                                    <td class="tbCell2">
                                                        <asp:Label ID="Label1" runat="server" Width="260px" Text="Fields marked with * are mandatory " ForeColor="Red"></asp:Label><br />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="tbCell1">GL Account No*</td>
                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="txtGLAccount" runat="server"  CssClass="tinyText" MaxLength="14"></asp:TextBox>
                                                        <asp:Button ID="cmasearch" OnClick="cmasearch_Click" runat="server" Text="Search" CausesValidation="False" Visible="False"></asp:Button>

                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" Font-Bold="True" ErrorMessage="*" ControlToValidate="txtGLAccount">Please enter GL Account</asp:RequiredFieldValidator>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="tbCell1">GL Name</td>
                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="txtGLName" runat="server" CssClass="tinyText" MaxLength="14"></asp:TextBox>
                                                        <asp:Button ID="Button3" OnClick="cmasearch_Click" runat="server" Text="Search" CausesValidation="False" Visible="False"></asp:Button>

                                                        
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="tbCell1">Amount</td>
                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="txtAmount" runat="server" CssClass="tinyText" MaxLength="6"></asp:TextBox>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style="height: 19px">
                                                        <asp:Label ID="Label4" runat="server" ForeColor="Red"></asp:Label>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>


                                        <table class="tbMain">
                                            <tbody>
                                                <tr>
                                                    <td class="tbCell1">
                                                        <asp:Button ID="btnSave" OnClick="btnSave_Click" runat="server" Text="Save" CssClass="btn-Save"></asp:Button>
                                                    </td>
                                                    <td class="tbCell1">
                                                        <asp:Button ID="btnClear" OnClick="btnClear_Click" runat="server" Text="Clear" CausesValidation="False" CssClass="btn-Cancel"></asp:Button>
                                                    </td>
                                                    <td class="tbCell1">
                                                        <asp:Button ID="cmddelete" OnClick="cmddelete_Click" runat="server" Width="86px" Text="Delete" CssClass="btn-Cancel"></asp:Button>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>


                                        <table class="tbMain">
                                            <tbody>
                                                <tr>
                                                    <td class="tbCell1"></td>
                                                    <td class="tbCell2">
                                                        <cc1:ConfirmButtonExtender ID="ConfirmButtonExtender1" runat="server" Enabled="True" TargetControlID="btnSave" ConfirmText="Are you sure you want to save?"></cc1:ConfirmButtonExtender>
                                                        <cc1:ConfirmButtonExtender ID="ConfirmButtonExtender2" runat="server" Enabled="True" TargetControlID="cmddelete" ConfirmText="Are you sure you want to Delete?"></cc1:ConfirmButtonExtender>
                                                    </td>
                                                    <td></td>
                                                </tr>
                                                <tr>
                                                    <td class="tbCell1">&nbsp;</td>
                                                    <td class="tbCell2">
                                                        <asp:HiddenField ID="HiddenField1" runat="server"></asp:HiddenField>
                                                    </td>
                                                    <td></td>
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
                                <cc1:TabPanel runat="server" HeaderText="TabPanel2" ID="TabPanel2">
                                    <HeaderTemplate>
                                        GL Codes
                                    </HeaderTemplate>
                                    <ContentTemplate>


                                        <div class="searchHolder">
                                            <asp:RadioButtonList ID="RdoSearch" runat="server" Width="690px" OnSelectedIndexChanged="RdoSearch_SelectedIndexChanged" RepeatDirection="Horizontal">
                                                <asp:ListItem Value="Names">GL Account</asp:ListItem>
                                                <asp:ListItem Value="Account">GL Name</asp:ListItem>
                                            </asp:RadioButtonList>
                                        </div>

                                        <div class="searchHolder">
                                            <div class="row">
                                                <div class="col-md-6">
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

                                        <div class="gridHolder">
                                            <asp:GridView ID="GridView1" runat="server" OnSelectedIndexChanged="GridView1_SelectedIndexChanged" EnableTheming="True" CellPadding="4" AutoGenerateColumns="False" AllowPaging="True"
                                                BackColor="White" BorderColor="white" BorderStyle="Solid" BorderWidth="3px" CssClass="GridView table-striped" ForeColor="Black" GridLines="Vertical">
                                                <AlternatingRowStyle BackColor="White"></AlternatingRowStyle>
                                                <Columns>
                                                    <asp:BoundField DataField="GLAccount" HeaderText="GL ACCOUNT"></asp:BoundField>
                                                    <asp:BoundField DataField="GLName" HeaderText="GL NAME"></asp:BoundField>
                                                    <asp:BoundField DataField="Amount" HeaderText="AMOUNT"></asp:BoundField>
                                                    <asp:TemplateField>
                                                        <ItemTemplate>
                                                            <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandArgument='<%# Eval("GLAccount") %>' CommandName="edit">Edit</asp:LinkButton>
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
                            </cc1:TabContainer>

                            </td>
                    </tr>
                </tbody>
            </table>

        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
