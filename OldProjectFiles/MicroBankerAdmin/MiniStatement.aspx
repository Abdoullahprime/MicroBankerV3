<%@ Page Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="MiniStatement.aspx.vb" Inherits="MicroBankerAdmin.MiniStatement" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>


            <table class="tbMain">
                <tbody>
                    <tr>
                        <td>
                            <asp:Button ID="btnClearForm" runat="server" Text="Mini Statement" CausesValidation="False" CssClass="btn-New"></asp:Button>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <cc1:TabContainer ID="TabContainer1" runat="server" ActiveTabIndex="0">
                                <cc1:TabPanel runat="server" HeaderText="TabPanel1" ID="TabPanel1">
                                    <HeaderTemplate>
                                        Mini Statement
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
                                                    <td class="tbCell1"></td>
                                                    <td class="tbCell2">
                                                        <asp:Label ID="lblSuccessMsg" runat="server" ForeColor="Red"></asp:Label>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="tbCell1">Account Number*</td>
                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="txtAccountNo" runat="server" CssClass="midRangeText" MaxLength="20"></asp:TextBox>
                                                        <asp:Button ID="btnCheckAccNo" runat="server" CssClass="btn-Save" Text="Check" CausesValidation="False"></asp:Button>
                                                    </td>

                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="lblAccountName" runat="server" CssClass="tinyText" MaxLength="8" Enabled="False"></asp:TextBox>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="tbCell1">Available Balance*</td>
                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="AailBalance" runat="server" CssClass="tinyText" MaxLength="20" Enabled="False" Width="148px" Height="30px"></asp:TextBox>

                                                    </td>
                                                    <td class="tbCell1">Book Balance*</td>
                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="BookBal" runat="server" CssClass="tinyText" Enabled="False" MaxLength="20" Width="148px" Height="30px"></asp:TextBox>

                                                    </td>

                                                </tr>
                                            </tbody>
                                        </table>
                                        <table class="tbMain">
                                            <tbody>
                                                <tr>
                                                    <td>
                                                        <div class="gridHolder">
                                                            <asp:GridView ID="GridView1" runat="server" EnableTheming="True" CellPadding="4" AutoGenerateColumns="False" AllowPaging="True"
                                                                BackColor="White" BorderColor="White" BorderStyle="Solid" BorderWidth="3px" CssClass="GridView table-striped" ForeColor="Black" GridLines="Vertical">
                                                                <AlternatingRowStyle BackColor="White"></AlternatingRowStyle>
                                                                <Columns>
                                                                    <asp:TemplateField HeaderText="Date">
                                                                        <ItemTemplate>
                                                                            <asp:Label ID="TxnDate" runat="server" Text='<%# Eval("TxnDate").ToString().Substring(0, 10) %>'></asp:Label>
                                                                        </ItemTemplate>
                                                                    </asp:TemplateField>
                                                                   <%-- <asp:BoundField DataField="TxnDate" HeaderText="Date"></asp:BoundField>--%>
                                                                    <asp:BoundField DataField="Details" HeaderText="Details"></asp:BoundField>
                                                                   <%-- <asp:BoundField DataField="TxnType" HeaderText="Transaction Type"></asp:BoundField>--%>
                                                                     <asp:TemplateField HeaderText="Transaction Type">
                                                                        <ItemTemplate>
                                                                            <asp:Label ID="TxnType_" runat="server" Text='<%# Eval("TxnType_") %>'></asp:Label>
                                                                        </ItemTemplate>
                                                                    </asp:TemplateField>
                                                                    <asp:BoundField DataField="TxnRefNo" HeaderText="Reference N0"></asp:BoundField>
                                                                    <asp:TemplateField HeaderText="Debit">
                                                                        <ItemTemplate>
                                                                            <asp:Label ID="Debit" runat="server" Text='<%# Eval("Debit") %>'></asp:Label>
                                                                        </ItemTemplate>
                                                                    </asp:TemplateField>
                                                                    <asp:TemplateField HeaderText="Credit">
                                                                        <ItemTemplate>
                                                                            <asp:Label ID="Credit" runat="server" Text='<%# Eval("Credit") %>'></asp:Label>
                                                                        </ItemTemplate>
                                                                    </asp:TemplateField>
                                                                    <asp:BoundField DataField="BookBalance" HeaderText="Book Balance"></asp:BoundField>
                                                                    <asp:BoundField DataField="AvailBal" HeaderText="Available Bal"></asp:BoundField>
                                                                </Columns>
                                                                <EditRowStyle BackColor="#7C6F57"></EditRowStyle>
                                                                <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White"></FooterStyle>
                                                                <HeaderStyle BackColor="#0F3B5A" Font-Bold="True" ForeColor="White"></HeaderStyle>
                                                                <PagerStyle HorizontalAlign="Center" BackColor="#666666" ForeColor="White"></PagerStyle>
                                                                <RowStyle BackColor="#E3EAEB"></RowStyle>
                                                                <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333"></SelectedRowStyle>
                                                            </asp:GridView>

                                                        </div>
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
</asp:Content>
