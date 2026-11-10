<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="GeneralLedger.aspx.vb" Inherits="MicroBankerAdmin.GeneralLedger" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="Header" runat="Server">
    <link href="../Content/XStyleSheet.css" rel="Stylesheet" type="text/css" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
     <div style="border:1px solid white; width:98%; margin-top:0%">
            <asp:Table ID="GeneralLedgerTable" runat="server" CssClass="GeneralLedgerTableStyle" GridLines="none">
                
                <asp:TableRow>
                    <asp:TableCell runat="server" Font-Size="Medium" ColumnSpan="3" HorizontalAlign="Center">
                        <div style="text-align:center"> 
                        <asp:Label runat="server" ID="Label9" Text="*" ForeColor="red" Font-Bold="true" Font-Size="small" />
                        Create General Ledger</div> </asp:TableCell>
                  
                    
                </asp:TableRow>
                <asp:TableRow>
                    <asp:TableCell ID="MessageCell" runat="server" ColumnSpan="3" HorizontalAlign="Center" Font-Size="Medium">
                        <asp:HiddenField ID="hdnEditID" runat="server" />
                    </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow>
                    <asp:TableCell ColumnSpan="3" HorizontalAlign="center" runat="server"   >
                            <div style="text-align:center; font-size:large">
                       <asp:Label runat="server" ID="_msgSuccessLabel" Font-Bold="true" Font-Size="Large" ForeColor="green" />
                                </div>
                    </asp:TableCell>
                </asp:TableRow>
                
                <asp:TableRow>
                    <asp:TableCell>
                          Select Head
                        <asp:Label runat="server" ID="Label15" Text="*" ForeColor="red" Font-Bold="true" Font-Size="small" />
                    </asp:TableCell>
                    <asp:TableCell ColumnSpan="1">

                        <asp:DropDownList runat="server" ID="HeadDropDownList" AutoPostBack="true" CssClass="tinyText">
                       
                            </asp:DropDownList>

                    </asp:TableCell>
                    <asp:TableCell>
                        <div style="text-align:center; font-size:large">
                       <asp:Label runat="server" ID="_msgLabel" Text="Invalid GL Account No" Font-Bold="true" Font-Size="Large" ForeColor="red" />
                                </div>
                    </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow runat="server" ID="currentGLRow" Visible="false">
                    <asp:TableCell>
                          <asp:Label runat="server" Text="Current GLs Under This Head"></asp:Label>
                    </asp:TableCell>
                    <asp:TableCell ColumnSpan="1">

                        <asp:DropDownList runat="server" ID="CurrentGLDropDownList" AutoPostBack="true" CssClass="tinyText">
                        </asp:DropDownList>

                    </asp:TableCell>
                    <asp:TableCell>
                        
                    </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow>
                    <asp:TableCell>
                        Account No
                         <asp:Label runat="server" ID="Label1" Text="*" ForeColor="red" Font-Bold="true" Font-Size="small" />
                         
                    </asp:TableCell>
                    <asp:TableCell>
                        <div style="border:1px solid white; width:100%; position: relative; top: -14px">
                            <div style="border:1px solid white; width:60%; position: absolute; left: 0">
                                <asp:textbox id="txtAccountNoPart1" runat="server"  Width="99%">

                                                                </asp:textbox>
                            </div>
                            <div style="border:1px solid white; width:39%; position: absolute; right: 0">
                                <asp:textbox id="txtAccountNoPart2" runat="server"  Width="99%">

                                                                </asp:textbox>
                            </div>
                                

                            </span>
                        </div>
                        
                    </asp:TableCell>
                    
                    <asp:TableCell runat="server" ID="lblAcctNoError" ForeColor="red">
                         Error: This Account No Exists
                    </asp:TableCell>
                </asp:TableRow>
                
                <asp:TableRow>
                    <asp:TableCell>
                         Account Currency
                         <asp:Label runat="server" ID="Label2" Text="*" ForeColor="red" Font-Bold="true" Font-Size="small" />
                         
                    </asp:TableCell>
                    <asp:TableCell ColumnSpan="1">
                        <asp:DropDownList runat="server" ID="currencyDropDownList" CssClass="tinyText" 
                            AutoPostBack="true" Enabled="false">
                        <asp:ListItem Value="0" Selected="false">Select Currency</asp:ListItem>
                                                                    <asp:ListItem Value="GMD" Selected="True">GMD</asp:ListItem>
                                                                    <asp:ListItem Value="USD"></asp:ListItem>
                                                                    
                        </asp:DropDownList>
                    </asp:TableCell>
                    <asp:TableCell>

                    </asp:TableCell>
                </asp:TableRow>

                <asp:TableRow>
                    <asp:TableCell>
                         Account Type
                         <asp:Label runat="server" ID="Label3" Text="*" ForeColor="red" Font-Bold="true" Font-Size="small" />
                         
                    </asp:TableCell>
                    <asp:TableCell ColumnSpan="1">
                        <asp:DropDownList runat="server" ID="TypeDropDownList" AutoPostBack="true" CssClass="tinyText">
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
                         Account Balance Type
                         <asp:Label runat="server" ID="Label4" Text="*" ForeColor="red" Font-Bold="true" Font-Size="small" />
                         
                    </asp:TableCell>
                    <asp:TableCell ColumnSpan="1">
                        <asp:DropDownList runat="server" ID="BalanceTypeDropDownList" CssClass="tinyText" AutoPostBack="true">
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
                         Account Status
                         <asp:Label runat="server" ID="Label5" Text="*" ForeColor="red" Font-Bold="true" Font-Size="small" />
                         
                    </asp:TableCell>
                    <asp:TableCell ColumnSpan="1">
                        <asp:DropDownList runat="server" ID="StatusDropDownList" AutoPostBack="true" CssClass="tinyText">
                            <asp:ListItem Value="0">Select...</asp:ListItem>
                            <asp:ListItem Value="O" Selected="True">OPEN</asp:ListItem>
                            <asp:ListItem Value="C">CLOSED</asp:ListItem>
                        </asp:DropDownList>
                    </asp:TableCell>


                    <asp:TableCell>

                    </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow>
                    <asp:TableCell>
                        Is Bank Account
                         <asp:Label runat="server" ID="Label6" Text="*" ForeColor="red" Font-Bold="true" Font-Size="small" />
                         
                    </asp:TableCell>
                    <asp:TableCell ColumnSpan="1">
                        <asp:DropDownList runat="server" ID="IsBankAccountDropdownList" CssClass="tinyText" AutoPostBack="true">
                            <asp:ListItem Value="Y">YES</asp:ListItem>
                            <asp:ListItem Value="N" Selected="True">NO</asp:ListItem>
                        </asp:DropDownList>
                    </asp:TableCell>
                    <asp:TableCell>

                    </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow>
                    <asp:TableCell>
                         Is Control GL
                         <asp:Label runat="server" ID="Label7" Text="*" ForeColor="red" Font-Bold="true" Font-Size="small" />
                         
                        
                    </asp:TableCell>
                    <asp:TableCell ColumnSpan="1">
                        <asp:DropDownList runat="server" ID="IsControlDropDownList" CssClass="tinyText" AutoPostBack="true">
                            <asp:ListItem Value="Y">YES</asp:ListItem>
                            <asp:ListItem Value="N" Selected="True">NO</asp:ListItem>
                        </asp:DropDownList>
                    </asp:TableCell>
                    <asp:TableCell>

                    </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow>
                    <asp:TableCell>
                         <asp:Label runat="server" Text="Account Name"></asp:Label>
                         <asp:Label runat="server" ID="Label8" Text="*" ForeColor="red" Font-Bold="true" Font-Size="small" />
                         
                    </asp:TableCell>
                    <asp:TableCell ColumnSpan="1">

                        <asp:textbox id="txtAccountName" runat="server" autopostback="true" CssClass="tinyText"></asp:textbox>

                    </asp:TableCell>
                    <asp:TableCell>
                        <div style="text-align:center; font-size:large">
                       <asp:Label runat="server" ID="AccountNameMsgLabel" Text="Account Name Is Missing" Font-Bold="true" Font-Size="Large" ForeColor="red" />
                                </div>
                    </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow>
                    <asp:TableCell>

                    </asp:TableCell>
                    <asp:TableCell ColumnSpan="2">
                        <div style="text-align:right">
                        <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn-Save" />
                         <asp:Button ID="btnCancel" runat="server" Text="Back" Visible="true" CssClass="btn-Save" />
                        <asp:Button ID="btnReset" runat="server" Text="Reset" CssClass="btn-Save"/>
                  </div>
                                             </asp:TableCell>
                </asp:TableRow>
              <%--  <asp:TableRow>
                    <asp:TableCell>

                    </asp:TableCell>
                    <asp:TableCell>

                    </asp:TableCell>
                    <asp:TableCell>
                        
                    </asp:TableCell>
                    
                </asp:TableRow>--%>
            </asp:Table>
                </div>
</asp:Content>
