<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="BulkUploadPosting.aspx.vb" Inherits="MicroBankerAdmin.BulkUploadPosting" %>

<asp:Content ID="Content1" ContentPlaceHolderID="Header" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
  <div class="formHolder">
                                        <table class="tbMain">
                                            <tbody>
                                                <tr>
                                                    <td class="CriteriaCell">
                                                        <asp:Label ID="Label1" runat="server" Text="Fields marked with * are mandatory " CssClass="Criteria" ForeColor="Red"></asp:Label><br />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="CriteriaCell">
                                                        <asp:Label ID="lblMessage" runat="server" ForeColor="Red"></asp:Label>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                        <table class="tbMain">
                                            <tbody>

                                                <tr>
                                                      <td class="tbCell1">Transaction Code*</td>
                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="txtTransactionCode" runat="server" CssClass="ShortText" MaxLength="4"></asp:TextBox>
                                                        <asp:Button ID="btncheckTranCode" runat="server" CssClass="btn-Save" Text="Check" CausesValidation="False"></asp:Button>
                                                    </td>
                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="lblDescription" runat="server" CssClass="tinyText" TextMode="MultiLine" MaxLength="20" Enabled="False"></asp:TextBox>
                                                    </td>
                                                </tr>
                                                <tr>
                                                      <td class="tbCell1">Narration*</td>

                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="txtNarration" runat="server" CssClass="tinyText"></asp:TextBox>
                                                    </td>
                                                    <td class="tbCell2">
                                                        <asp:FileUpload ID="FileUpload1" runat="server"   />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="tbCell1">
                                                        <asp:Button Text="Upload" OnClick="Upload" runat="server" CssClass="btn-Save"    />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td >
                                                        <hr />
                                                        </td>
                                                        </tr>
                                                <tr>
                                                    <td colspan="9">
                                                        <asp:GridView runat="server" ID="GridView1"  EnableTheming="True" CellPadding="4" BackColor="White" BorderColor="White" BorderStyle="Solid" BorderWidth="3px" CssClass="GridView table-striped" ForeColor="Black" GridLines="Vertical">
                                                                <AlternatingRowStyle BackColor="White"></AlternatingRowStyle>
                                                               <EditRowStyle BackColor="#7C6F57"></EditRowStyle>
                                                                <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White"></FooterStyle>
                                                                <HeaderStyle BackColor="#0F3B5A" Font-Bold="True" ForeColor="White"></HeaderStyle>
                                                                <PagerStyle HorizontalAlign="Center" BackColor="#666666" ForeColor="White"></PagerStyle>
                                                                <RowStyle BackColor="#E3EAEB"></RowStyle>
                                                                <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333"></SelectedRowStyle>
                                                            </asp:GridView>
                                                    </td>
                                                </tr>

                                                <tr>
                                                    <td>
                                                        <asp:Button Text="Post" OnClick="Post" runat="server" CssClass="btn-Save" />
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
                                            </tbody>
                                        </table>
                                            </div>
</asp:Content>
