<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="MixedPosting.aspx.vb" Inherits="MicroBankerAdmin.MixedPosting" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
     <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>


            <table class="tbMain">
                <tbody>
                    <tr>
                        <td>
                            <asp:Button ID="btnClearForm" runat="server" Text="New Mixed Posting" CausesValidation="False" CssClass="btn-New"></asp:Button>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <cc1:TabContainer ID="TabContainer1" runat="server" ActiveTabIndex="0">
                                <cc1:TabPanel runat="server" HeaderText="TabPanel1" ID="TabPanel1">
                                    <HeaderTemplate>
                                        Mixed Posting
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
                                                    <td class="tbCell1">Post To</td>
                                                    <td class="tbCell2">
                                                        <asp:DropDownList ID="drpAccountType" runat="server" CssClass="tinyText">
                                                            <asp:ListItem></asp:ListItem>
                                                            <asp:ListItem>CUSTOMER ACCOUNT</asp:ListItem>
                                                            <asp:ListItem>GL ACCOUNT</asp:ListItem>
                                                        </asp:DropDownList>
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
                                                </tr>
                                                <tr>
                                                    <td class="tbCell1">Narration</td>
                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="txtNarration" runat="server" CssClass="tinyText"></asp:TextBox>

                                                    </td>
                                                        <td class="tbCell2">
                                                            <asp:Label ID="lblSuccessMsg" ForeColor="Red" runat="server" />

                                                        </td>
                                                </tr>
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
                                                <tr>
                                                    <td class="tbCell1">Total Debits:</td>
                                                    <td class="tbCell2">
                                                        <asp:LABEL ID="lblTotalDebit" runat="server" CssClass="tinyText" ReadOnly="True" Text="0.0"></asp:LABEL>

                                                    </td>
                                                     <td class="tbCell1">Total Credits:</td>
                                                    <td class="tbCell2">
                                                        <asp:LABEL ID="lblTotalCredits" runat="server" CssClass="tinyText" ReadOnly="True" Text="0.0"></asp:LABEL>

                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                        <table class="tbMain" style="width:100%">
                                            <tbody>
                                                <tr>
                                                    <td class="tbCell1">
                                                        <div style="width: 99%; border: 1px solid blue; margin: 20px 0;overflow: scroll">
                                                            <asp:GridView ID="RetailPostingGridView" runat="server" AutoGenerateColumns="False"
                                                                            OnRowCreated="RetailPostingGridView_RowCreated" Width="100%">
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
                                                                                    <HeaderStyle Width="15%" />
                                                                                    <ItemStyle Width="15%" />
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
                                                                                    <HeaderStyle Width="30%" />
                                                                                    <ItemStyle Width="30%" />
                                                                                </asp:TemplateField>
                                                                              <asp:TemplateField HeaderText="GL Account">
                                                                                    <ItemTemplate>
                                                                                        <asp:Label ID="GLAccountLabel" runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "GL Account") %>'></asp:Label>
                                                                                        <%--<asp:TextBox ID="DescTextBox" runat="server" Width="99%" ReadOnly="true"></asp:TextBox>--%>
                                                                                    </ItemTemplate>
                                                                                    <HeaderStyle Width="10%" />
                                                                                    <ItemStyle Width="10%" />
                                                                                </asp:TemplateField>
                                                                                <asp:TemplateField>
                                                                                    <ItemTemplate>
                                                                                        <asp:LinkButton ID="LinkDelete" runat="server" OnClick="LinkDelete_Click" Enabled="true">Remove</asp:LinkButton>
                                                                                    </ItemTemplate>
                                                                                    <HeaderStyle Width="10%" />
                                                                                    <ItemStyle Width="10%" />
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
</asp:Content>
