<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="ChartOfAccountsList.aspx.vb" Inherits="MicroBankerAdmin.ChartOfAccountsList" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="Header" runat="server">
  <link href="../Content/XStyleSheet.css" rel="Stylesheet" type="text/css" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
           
<div style="border:1px solid white; width:98%; margin-top:2%">
                <asp:Table runat="server" ID="glist" CssClass="GeneralLedgerListTableStyle"
                    gridlines="none">
                    <asp:TableRow>
                        <asp:TableCell runat="server">
                            Search Criteria:
                        </asp:TableCell>
                        <asp:TableCell HorizontalAlign="right">
                            <div style="text-align:right">
                                <div style="padding-bottom:3px">
                             <asp:DropDownList ID="GL_HeadTypeDropDownList" runat="server" AutoPostBack="true" CssClass="tinyText">
                                                                    <asp:ListItem Value="0" Selected="True">Select Head Type</asp:ListItem>
                                                                    <asp:ListItem Value="1">HEAD GL</asp:ListItem>
                                                                    <asp:ListItem Value="2">SUB HEAD 1</asp:ListItem>
                                                                    <asp:ListItem Value="3">SUB HEAD 2</asp:ListItem>
                                                                </asp:DropDownList>
                                    </div>
                             <asp:textbox ID="txtAccountNo" runat="server" Placeholder="Account No" ReadOnly="true" CssClass="BoxStyle" MaxLength="2"></asp:textbox>
                        </div>
                                </asp:TableCell>
                        <asp:TableCell>
                            <div style="margin-left:30px; text-align:left; vertical-align:bottom">
                            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn-Save" />
                         <asp:Button ID="btnCancel" runat="server" Text="Cancel" Visible="true" CssClass="btn-Save" />
                       </div>
                        </asp:TableCell>
                        <asp:TableCell>

                        </asp:TableCell>
                    </asp:TableRow>
                    <asp:TableRow>

                        <asp:TableCell ColumnSpan="4">
                            <div style="border:1px solid white; width:98%; margin-top:5px">
                              <asp:GridView ID="ChartOfAccountsGridView" runat="server" AutoGenerateColumns="False" Width="95%"
                                                                    OnPageIndexChanging="ChartOfAccountsGridView_PageIndexChanging"
                                                                     AllowPaging="true" PageSize="30">
                                                                    <Columns>
                                                                        <asp:TemplateField HeaderText="No.">
                                                                    <ItemTemplate>
                                                                        <asp:Label ID="SeqLabel" runat="server" Text='<%# Container.DataItemIndex + 1 %>'></asp:Label>
                                                                    </ItemTemplate>

                                                                    <HeaderStyle HorizontalAlign="CENTER" VerticalAlign="Bottom" Width="5%" />
                                                                    <ItemStyle HorizontalAlign="Center" VerticalAlign="Bottom" Width="5%" />
                                                                </asp:TemplateField>

                                                                        <asp:TemplateField HeaderText="ACCOUNT NO">
                                                                            <ItemTemplate>
                                                                                <asp:Label runat="server" Text='<%# Eval("ACC_NO") %>'>
                                                                                </asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:TextBox ID="accountNoTextBox" runat="server" Text='<%# Eval("ACC_NO")%>'></asp:TextBox>
                                                                            </EditItemTemplate>
                                                                            <HeaderStyle HorizontalAlign="CENTER" VerticalAlign="Bottom" Width="10%" />
                                                                    <ItemStyle HorizontalAlign="center" VerticalAlign="Bottom" Width="10%" />
                                                                        </asp:TemplateField>
                                                                        <asp:TemplateField HeaderText="ACCOUNT NAME">
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="accountNameLabel" runat="server" Text='<%# Eval("ACC_NAME")%>'></asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:TextBox ID="accountNameTextBox" runat="server" Text='<%# 
				Eval("ACC_NAME") %>'></asp:TextBox>
                                                                            </EditItemTemplate>
                                                                            <HeaderStyle HorizontalAlign="CENTER" VerticalAlign="Bottom" />
                                                                    <ItemStyle HorizontalAlign="left" VerticalAlign="Bottom" />
                                                                        </asp:TemplateField>
                                                                        <asp:TemplateField HeaderText="ALLOW POSTING">
                                                                            <ItemTemplate>
                                                                                <asp:Label ID="allowPostingLabel" runat="server" Text='<%# Eval("ALLOW_POST")%>'></asp:Label>
                                                                            </ItemTemplate>
                                                                            <EditItemTemplate>
                                                                                <asp:CheckBoxList ID="allowPostingCheckBoxList" runat="server">
                                                                                    <asp:ListItem>Yes</asp:ListItem>
                                                                                </asp:CheckBoxList>
                                                                            </EditItemTemplate>
                                                                            <HeaderStyle HorizontalAlign="CENTER" VerticalAlign="Bottom" Width="10%" />
                                                                    <ItemStyle HorizontalAlign="left" VerticalAlign="Bottom" Width="10%" />
                                                                        </asp:TemplateField>
                                                                        <asp:TemplateField HeaderText="Action">
                                                                            <ItemTemplate>
                                                                                <asp:LinkButton ID="BtnLnkRemoveAccountHead" runat="server" CommandName="Delete" CommandArgument='<%# Eval("ACC_NO")%>' Text="Delete"></asp:LinkButton>
                                                                                <asp:LinkButton ID="btnViewAccountHeadRecord" runat="server" CommandArgument='<%# Eval("ACC_NO")%>' OnClick="btnViewAccountHeadRecord_Click" Text="View"></asp:LinkButton>
                                                                                <asp:LinkButton ID="btnEdit" runat="server" CommandArgument='<%# Eval("ACC_NO")%>' OnClick="btnEdit_Click" Text="Edit"></asp:LinkButton>
                                                                            </ItemTemplate>
                                                                            <HeaderStyle HorizontalAlign="CENTER" VerticalAlign="Bottom" Width="15%" />
                                                                    <ItemStyle HorizontalAlign="Center" VerticalAlign="Bottom" Width="15%" />
                                                                        </asp:TemplateField>
                                                                    </Columns>
                                                                </asp:GridView>

                                
                                </div>
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
                </div>
        </ContentTemplate>
    </asp:UpdatePanel>
    </asp:Content>
