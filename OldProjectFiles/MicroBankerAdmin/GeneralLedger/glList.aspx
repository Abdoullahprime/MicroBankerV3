<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="glList.aspx.vb" Inherits="MicroBankerAdmin.glList" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>


<asp:Content ID="Content1" ContentPlaceHolderID="Header" runat="server">
     <link href="../Content/XStyleSheet.css" rel="Stylesheet" type="text/css" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <div style="border:1px solid white; width:98%; margin-top:10%">
                <asp:Table runat="server" ID="glist" CssClass="GeneralLedgerListTableStyle"
                    gridlines="none">
                    <asp:TableRow>
                        <asp:TableCell runat="server">
                            Search Options:
                        </asp:TableCell>
                        <asp:TableCell>
                            <div style="padding-right:5px; text-align:right">
                           <asp:textbox ID="txtAccountNo" runat="server" Placeholder="Account No/Name" ReadOnly="false" CssClass="tinyText" ></asp:textbox>
                        </div>
                        </asp:TableCell>
                        <asp:TableCell>
                            <div style="margin-left:30px;margin-top:10px; text-align:left; vertical-align:bottom">
                            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn-Save" />
                         <asp:Button ID="btnCancel" runat="server" Text="Cancel" Visible="true" CssClass="btn-Save" />
                       </div>
                        </asp:TableCell>
                        <asp:TableCell>

                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow>

                        <asp:TableCell ColumnSpan="4">
                            </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow>
                        <asp:TableCell>

                        </asp:TableCell>
                        <asp:TableCell>

                        </asp:TableCell>
                        <asp:TableCell>

                        </asp:TableCell>
                        <asp:TableCell>

                        </asp:TableCell>
                    </asp:TableRow>
                </asp:Table>
                            <div style="border:1px solid white; width:98%; margin-top:5px">
                            <asp:GridView ID="GeneralLedgerGridView" runat="server" Width="100%"
                                                                    AutoGenerateColumns="False" 
                                                                    Font-Size="11pt" 
                                ShowHeaderWhenEmpty="true"
                                                                    HeaderStyle-BackColor="WhiteSmoke" AllowPaging="True" 
                                                                                                            OnRowDataBound="GeneralLedgerGridView_RowDataBound"
                                                                    OnRowCommand="GeneralLedgerGridView_RowCommand"
                                                                    PageSize="20" CssClass="GridView table-striped" EmptyDataText="No Records Found" EmptyDataRowStyle-ForeColor="Red" EmptyDataRowStyle-HorizontalAlign="Center">
                                                                    <Columns>
                                                                        <asp:TemplateField HeaderText="No.">
                                                                    <ItemTemplate>
                                                                        <asp:Label ID="SeqLabel" runat="server" Text='<%# Container.DataItemIndex + 1 %>'></asp:Label>
                                                                    </ItemTemplate>

                                                                    <HeaderStyle HorizontalAlign="CENTER" VerticalAlign="Bottom" Width="5%" />
                                                                    <ItemStyle HorizontalAlign="Center" VerticalAlign="Bottom" Width="5%" />
                                                                </asp:TemplateField>

                                                                        <asp:TemplateField  HeaderText="ACCOUNT NO." >
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="AccountNoLabel" runat="server" Text='<%# Eval("GLACCTNO").ToString.Trim()%>'></asp:Label>
                                                                            </ItemTemplate>

                                                                            <ItemStyle Width="20%" HorizontalAlign="left" VerticalAlign="Bottom" />
                                                                            <HeaderStyle HorizontalAlign="Center" VerticalAlign="Bottom"/>
                                                                        </asp:TemplateField>
                                                                        <asp:TemplateField ItemStyle-Width="40%" HeaderText="Account Name">
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="NameLabel" runat="server"
                                                                                    Text='<%# Eval("GLNAME")%>'></asp:Label>
                                                                            </ItemTemplate>
                                                                            <HeaderStyle HorizontalAlign="Center" VerticalAlign="Bottom" />
                                                                            <ItemStyle HorizontalAlign="left" VerticalAlign="Bottom" Width="40%" />
                                                                        </asp:TemplateField>
                                                                        <asp:TemplateField ItemStyle-Width="8%" HeaderText="Currency">
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="CurrencyNameLabel" runat="server" Text='<%# Eval("CCY")%>'></asp:Label>
                                                                            </ItemTemplate>
                                                                            <HeaderStyle HorizontalAlign="Center" VerticalAlign="Bottom" />
                                                                            <ItemStyle HorizontalAlign="Center" VerticalAlign="Bottom" Width="8%" />
                                                                        </asp:TemplateField>
                                                                        <asp:TemplateField ItemStyle-Width="12%" HeaderText="Account Type">
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="accountTypeLabel" runat="server" Text='<%# Eval("ACCTTYPE")%>'></asp:Label>
                                                                            </ItemTemplate>
                                                                            <HeaderStyle HorizontalAlign="Center" VerticalAlign="Bottom" />
                                                                            <ItemStyle HorizontalAlign="Center" VerticalAlign="Bottom" Width="8%" />
                                                                        </asp:TemplateField>
                                                                        <asp:TemplateField ItemStyle-Width="8%" HeaderText="Balance Type">
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="balanceTypeLabel" runat="server" Text='<%# Eval("BALTYPE")%>'></asp:Label>
                                                                            </ItemTemplate>
                                                                            <HeaderStyle HorizontalAlign="Center" VerticalAlign="Bottom" />
                                                                            <ItemStyle HorizontalAlign="Center" VerticalAlign="Bottom" Width="8%" />
                                                                        </asp:TemplateField>
                                                                        <asp:TemplateField ItemStyle-Width="20%" HeaderText="Actual Balance">
                                                                            <ItemTemplate>
                                                                                <div style="text-align:right">
                                                                                <asp:Label ID="CurrentBalanceLabel" runat="server" Text='<%# Eval("ACT_BAL", "{0:###,###,###.00}")%>'></asp:Label>
                                                                           </div>
                                                                                    </ItemTemplate>
                                                                            <HeaderStyle HorizontalAlign="Center" VerticalAlign="Bottom" />
                                                                            <ItemStyle HorizontalAlign="Center" VerticalAlign="Bottom" Width="20%" />
                                                                        </asp:TemplateField>
                                                                        
                                                                               
                                                                             <asp:TemplateField HeaderText="Delete" Visible="false">
                                                                    <ItemTemplate>
                                                                        <asp:Button ID="ImgDelete" runat="server" 
                                                                             ToolTip="Delete GL" 
                                                                            OnClick="ImgDelete_Click" CommandArgument='<%# Eval("GLACCTNO")%>' />
                                                                    </ItemTemplate>
                                                                    
                                                                    <HeaderStyle HorizontalAlign="CENTER" VerticalAlign="Bottom" Width="4%" />
                                                                    <ItemStyle HorizontalAlign="Center" VerticalAlign="Bottom" Width="4%" />
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="VIEW" Visible="false">
                                                                    <ItemTemplate>
                                                                        <asp:Button ID="ImgView" runat="server" CommandName="V"
                                                                             ToolTip="View GL" 
                                                                            OnClick="ImgView_Click" CommandArgument='<%# Eval("GLACCTNO")%>'  />
                                                                    </ItemTemplate>
                                                                    <HeaderStyle HorizontalAlign="CENTER" VerticalAlign="Bottom" Width="5%" />
                                                                    <ItemStyle HorizontalAlign="Center" VerticalAlign="Bottom" Width="5%" />
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="Edit" Visible="false">
                                                                    <ItemTemplate>
                                                                        <asp:Button ID="ImgEdit" runat="server"  CommandName="E"
                                                                             ToolTip="Edit GL" 
                                                                            OnClick="ImgEdit_Click" CommandArgument='<%# Eval("GLACCTNO")%>'  />
                                                                    </ItemTemplate>
                                                                    <HeaderStyle HorizontalAlign="CENTER" VerticalAlign="Bottom" Width="5%" />
                                                                    <ItemStyle HorizontalAlign="Center" VerticalAlign="Bottom" Width="5%" />
                                                                </asp:TemplateField>
                                                                    </Columns>
                                 
                                <EmptyDataRowStyle CssClass="EmptyDataRowStyle"  />
                               
                                                                                                    <EmptyDataTemplate>NO RECORDS FOUND</EmptyDataTemplate>
                                                                    <AlternatingRowStyle BackColor="#C2D69B" />
                                                                    <HeaderStyle BackColor="WhiteSmoke" />
                                                                </asp:GridView>
                                                                <br />
                        
                                </div>
                        
                </div>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
          