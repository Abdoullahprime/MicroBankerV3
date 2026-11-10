<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="ChartOfAccounts_.aspx.vb" Inherits="MicroBankerAdmin.ChartOfAccounts_" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="Header" Runat="Server">  
      <link href="../Content/XStyleSheet.css" rel="Stylesheet" type="text/css" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
          <div style="border:1px solid red; width:98%; margin-top:10%">
              <asp:Table ID="TableAddChartOfAccounts" runat="server"  Font-Size="Medium" GridLines="both" CssClass="TableAddChartOfAccountsStyle">
                                                        <asp:TableRow>
                                                            <asp:TableCell ColumnSpan="4" HorizontalAlign="Center">
                                                                <div style="text-align:center; font-size:large">
                        For Error and other messages
                                </div>
                                                            </asp:TableCell>
                                                        </asp:TableRow>
                                                        <asp:TableRow>
                                                            <asp:TableCell  runat="server" ID="frmMessage" Text="message" Font-Bold="true"
                                                                colspan="4" HorizontalAlign="Center">

                                                            </asp:TableCell>
                                                            
                                                        </asp:TableRow>
                                                        <asp:TableRow>
                                                            <asp:TableCell>
                                                                
                                                            </asp:TableCell>
                                                            <asp:TableCell>
                                                                GL HEAD TYPE
                                                               <asp:Label runat="server" ID="Label5" Text="*" ForeColor="red" Font-Bold="true" Font-Size="small" />
                                                            </asp:TableCell>
                                                            <asp:TableCell ColumnSpan="1">
                                                                <asp:DropDownList ID="GL_HeadTypeDropDownList" runat="server" AutoPostBack="true" CssClass="tinyText">
                                                                    <asp:ListItem Value="0" Selected="True">Select Head Type</asp:ListItem>
                                                                    <asp:ListItem Value="1">HEAD GL</asp:ListItem>
                                                                    <asp:ListItem Value="2">SUB HEAD 1</asp:ListItem>
                                                                    <asp:ListItem Value="3">SUB HEAD 2</asp:ListItem>
                                                                </asp:DropDownList>
                                                            
                                                            </asp:TableCell>

                                                            <asp:TableCell runat="server" ID="AcctNoError" ForeColor="Red" HorizontalAlign="Left" >
                                                                
                                                            </asp:TableCell>
                                                        </asp:TableRow>
                                                         <asp:TableRow runat="server" ID="headGlRow" Visible="false">

                                                            <asp:TableCell>
                                                                
                                                            </asp:TableCell>
                                                            <asp:TableCell>
                                                                
                                                              Existing HEAD GL
                                                            </asp:TableCell>
                                                            <asp:TableCell ColumnSpan="1">
                                                                 <asp:DropDownList ID="HeadDropDownList" runat="server" AutoPostBack="true" Enabled="true" CssClass="tinyText">
                                                                </asp:DropDownList>
                                                           
                                                            </asp:TableCell>
                                                            <asp:TableCell>
                   
                                                            </asp:TableCell>

                                                        </asp:TableRow>
                                                        <asp:TableRow>

                                                            <asp:TableCell>
                                                                
                                                            </asp:TableCell>
                                                            <asp:TableCell>
                                                                
                                                               HEAD GL

                                                            </asp:TableCell>
                                                            <asp:TableCell ColumnSpan="1">
                                                                 <asp:DropDownList ID="HeadGLDropDownList" runat="server" AutoPostBack="true" Enabled="false" CssClass="tinyText">
                                                                </asp:DropDownList>
                                                           
                                                            </asp:TableCell>
                                                            <asp:TableCell>
                   
                                                            </asp:TableCell>

                                                        </asp:TableRow>
                                                        <asp:TableRow>

                                                            <asp:TableCell>
                                                                
                                                               
                                                            </asp:TableCell>
                                                            <asp:TableCell>
                                                                 
                                                               ACCOUNT TYPE
                                                                <asp:Label runat="server" ID="Label4" Text="*" ForeColor="red" Font-Bold="true" Font-Size="small" />
                                                            </asp:TableCell>
                                                            <asp:TableCell ColumnSpan="1">
                                                                 <asp:DropDownList ID="AccountTypeDropDownList" runat="server" AutoPostBack="true" Enabled="false" CssClass="tinyText">
                                                                    <asp:ListItem Value="0">Select...</asp:ListItem>
                                                                    <asp:ListItem Value="A">ASSET</asp:ListItem>
                                                                    <asp:ListItem Value="L">LIABILITY</asp:ListItem>
                                                                    <asp:ListItem Value="I">INCOME</asp:ListItem>
                                                                    <asp:ListItem Value="E">EXPENDITURE</asp:ListItem>
                                                                </asp:DropDownList>
                                                                
                                                            
                                                            </asp:TableCell>
                                                            <asp:TableCell>
                   
                                                            </asp:TableCell>

                                                        </asp:TableRow>
                                                         <asp:TableRow>
                                                            <asp:TableCell>

                                                            </asp:TableCell>
                                                            <asp:TableCell>
                                                                
                                                                SUB HEAD 1
                                                            </asp:TableCell>
                                                            <asp:TableCell ColumnSpan="1">
                                                                <asp:DropDownList ID="SubHead1DropDownList" runat="server" AutoPostBack="true" Enabled="false" CssClass="tinyText">
                                                                   <%-- <asp:ListItem Value="0" Selected="True">Select Sub Head 1</asp:ListItem>--%>
                                                                </asp:DropDownList>
                                                            
                                                            </asp:TableCell>
                                                            <asp:TableCell>

                                                            </asp:TableCell>
                                                        </asp:TableRow>
                                                        <asp:TableRow runat="server" ID="existingSubHeadRow" Visible="false">
                                                            <asp:TableCell>

                                                            </asp:TableCell>
                                                            <asp:TableCell>
                                                                
                                                           EXISTING SUB HEAD 1
                                                            </asp:TableCell>
                                                            <asp:TableCell ColumnSpan="1">
                                                                <asp:DropDownList ID="ExistingSubHead1sDropDownList" runat="server" AutoPostBack="true" Enabled="false" CssClass="tinyText">
                                                                   <%-- <asp:ListItem Value="0" Selected="True">Select Sub Head 1</asp:ListItem>--%>
                                                                </asp:DropDownList>
                                                            
                                                            </asp:TableCell>
                                                            <asp:TableCell>

                                                            </asp:TableCell>
                                                        </asp:TableRow>
                                                        <asp:TableRow>
                                                            <asp:TableCell>

                                                            </asp:TableCell>
                                                            <asp:TableCell>
                                                                
                                                                BALANCE TYPE
                                                                <asp:Label runat="server" ID="Label3" Text="*" ForeColor="red" Font-Bold="true" Font-Size="small" />
                                                            </asp:TableCell>
                                                            <asp:TableCell ColumnSpan="1">
                                                                <asp:DropDownList ID="BalanceTypeDropDownList" runat="server" AutoPostBack="false" Enabled="false" CssClass="tinyText">
                                                                    <asp:ListItem Value="0">Select...</asp:ListItem>
                                                                    <asp:ListItem Value="C">CREDIT</asp:ListItem>
                                                                    <asp:ListItem Value="D">DEBIT</asp:ListItem>
                                                                </asp:DropDownList>
                                                            

                                                            </asp:TableCell>
                                                            <asp:TableCell>

                                                            </asp:TableCell>
                                                        </asp:TableRow>
                   <asp:TableRow>
                                                            <asp:TableCell>

                                                            </asp:TableCell>
                                                            <asp:TableCell>
                                                                Allow Posting
                                                                <asp:Label runat="server" ID="Label15" Text="*" ForeColor="red" Font-Bold="true" Font-Size="small" />
                                                            </asp:TableCell>
                                                            <asp:TableCell ColumnSpan="1">
                                                                 <asp:DropDownList ID="AllowPostingDropDownList" runat="server" AutoPostBack="true" Enabled="true" CssClass="tinyText">
                                                                    <asp:ListItem Value="0">Select...</asp:ListItem>
                                                                    <asp:ListItem Value="N">No</asp:ListItem>
                                                                    <asp:ListItem Value="Y" Selected="True">Yes</asp:ListItem>
                                                                </asp:DropDownList>

                                                            </asp:TableCell>
                                                            <asp:TableCell>

                                                            </asp:TableCell>
                                                        </asp:TableRow>
                                                         <asp:TableRow>
                                                            <asp:TableCell>

                                                            </asp:TableCell>
                                                            <asp:TableCell>
                                                               
                                                                ACCOUNT NO
                                                                <asp:Label runat="server" ID="Label1" Text="*" ForeColor="red" Font-Bold="true" Font-Size="small" />
                                                            </asp:TableCell>
                                                            <asp:TableCell ColumnSpan="1">
                                                                <asp:textbox ID="txtAccountNo" runat="server" Placeholder="Account No" ReadOnly="false" CssClass="tinyText" MaxLength="2"></asp:textbox>
                                                                
                                                            

                                                            </asp:TableCell>
                                                            <asp:TableCell>

                                                            </asp:TableCell>
                                                        </asp:TableRow>
                                                         <asp:TableRow>
                                                            <asp:TableCell>

                                                            </asp:TableCell>
                                                            <asp:TableCell>
                                                                 ACCOUNT NAME
                                                                <asp:Label runat="server" ID="Label2" Text="*" ForeColor="red" Font-Bold="true" Font-Size="small" />
                                                            </asp:TableCell>
                                                            <asp:TableCell ColumnSpan="1">
                                                                 <asp:textbox ID="txtAccountName" runat="server" Placeholder="Account Name"
                                                                     CssClass="tinyText"></asp:textbox>
                                                            

                                                            </asp:TableCell>
                                                            <asp:TableCell>

                                                            </asp:TableCell>
                                                        </asp:TableRow>
                                                        
                                                         <asp:TableRow>
                                                            <asp:TableCell>

                                                            </asp:TableCell>
                                                            <asp:TableCell>

                                                            </asp:TableCell>
                                                            <asp:TableCell ColumnSpan="2">
                                                                  <asp:Button ID="btnSave" runat="server" Text="Save" Enabled="TRUE" CssClass="btn-Save" />
                                                                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn-Save"/>
                                                              
                                                                <asp:Button ID="btnReset" runat="server" Text="Reset" Enabled="TRUE" CssClass="btn-Save" />
                                                                
                                                            
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

                                                        <asp:TableRow>
                                                            <asp:TableCell ColumnSpan="1">

                                                            </asp:TableCell>
                                                            <asp:TableCell ColumnSpan="3">
                                                                </asp:TableCell>
                                                        </asp:TableRow>
                                                        <asp:TableRow>

                                                            <asp:TableCell>

                                                            </asp:TableCell>
                                                            <asp:TableCell>

                                                            </asp:TableCell>
                                                            <asp:TableCell>

                                                            </asp:TableCell>
                                                            <asp:TableCell ID="ButtonCell" ColumnSpan="2" HorizontalAlign="right">
                              
                                                            </asp:TableCell>
                                                        </asp:TableRow>
                                                    </asp:Table>

          </div>

             </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
