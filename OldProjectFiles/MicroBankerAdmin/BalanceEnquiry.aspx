<%@ Page Language="vb" AutoEventWireup="false"  MasterPageFile="~/Site.Master" CodeBehind="BalanceEnquiry.aspx.vb" Inherits="MicroBankerAdmin.BalanceEnquiry1" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
     <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>


            <table class="tbMain">
                <tbody>
                    <tr>
                        <td>
                            <asp:Button ID="btnClearForm" runat="server" Text="Balance Enquiry" CausesValidation="False" CssClass="btn-New"></asp:Button>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <cc1:TabContainer ID="TabContainer1" runat="server" ActiveTabIndex="0">
                                <cc1:TabPanel runat="server" HeaderText="TabPanel1" ID="TabPanel1">
                                    <HeaderTemplate>
                                        Balance Enquiry
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
                                                        <asp:TextBox ID="AailBalance" runat="server" CssClass="tinyText" MaxLength="20"  Enabled="False" Width="148px" Height="30px"></asp:TextBox>
                                                       
                                                    </td>
                                                     <td class="tbCell1">Book Balance*</td>
                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="BookBal" runat="server" CssClass="tinyText"  Enabled="False" MaxLength="20" Width="148px" Height="30px"></asp:TextBox>
                                                       
                                                    </td>

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
                                                        <asp:Button ID="btnSave" runat="server" Text="Save" CausesValidation="False" CssClass="btn-Save" Visible="False"></asp:Button>
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