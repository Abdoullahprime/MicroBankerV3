<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="CHART_OF_ACCOUNTS.aspx.vb" Inherits="MicroBankerAdmin.CHART_OF_ACCOUNTS" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<%@ Register Assembly="Microsoft.ReportViewer.WebForms" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<asp:Content ID="Content1" ContentPlaceHolderID="Header" runat="server">
     <link href="../Content/XStyleSheet.css" rel="Stylesheet" type="text/css" />
<link href = "../../Calender/jquery-ui.css" rel = "stylesheet">
<script src = "../../Calender/jquery-1.10.2.js"></script>
<script src = "../../Calender/jquery-ui.js"></script>


<script>
    <%--var jqOld = jQuery.noConflict();
    jqOld(function() {
        jqOld("#<%=txtstartdate.ClientID%>").datepicker();
    })--%>
</script>

<link rel="stylesheet"  href="../../Content/bootstrap.min.css">

<script src="../../Calender/jquery.min.js"></script>
<script src="../../Calender/bootstrap.min.js"></script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
          <div style="border:1px solid red; width:98%; margin-top:10%">
              <asp:Table ID="tblChart_Of_accounts" runat="server" CssClass="ReportsTableStyle"
                  gridline="both">
                  <asp:TableRow>
                      <asp:TableCell></asp:TableCell>
                      <asp:TableCell></asp:TableCell>
                      <asp:TableCell></asp:TableCell>
                  </asp:TableRow>
                  <asp:TableRow>
                      <asp:TableCell>
                        
                      </asp:TableCell>
                      <asp:TableCell>
                          <diV style="text-align:center; margin: 5px 5px 5px 5px;border:1px solid white;position: relative; height: 40px; width:99%">
                           <div style="border:1px solid green; width:60%; position: absolute; left: 0">
                              <span>
                              <label for="startdate">Report Type:</label>
                              <asp:DropDownList ID="ReportTypeDropDownList" runat="server" AutoPostBack="TRUE" CssClass="tinyText">
                                                                    <asp:ListItem Value="0" Selected="True">Select Report Type...</asp:ListItem>
                                                                    <asp:ListItem Value="1">Detailed</asp:ListItem>
                                                                    <asp:ListItem Value="2">Summary</asp:ListItem>
                              </asp:DropDownList>
                                  </span>
                                  </div>
                          <%--<label for="startdate">Start Date:</label>
                          <asp:TextBox runat="server" ID="txtstartdate"></asp:TextBox>
                         --%>
                          <div style="border:1px solid blue; width:39%; position: absolute; right: 0">
                              <span>
                              <asp:Button ID="btnView" runat="server" Text="View" Enabled="TRUE" CssClass="btn-Save"  />
                                                                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn-Save"/>
                                                              
                          </span>
                              </diV>
                              </diV>
</asp:TableCell>
                       <asp:TableCell></asp:TableCell>
                  </asp:TableRow>
                  <asp:TableRow>
                      <asp:TableCell ColumnSpan="3">
                           
             <div id="container" style="border: 3px solid #C1C1C1; padding: 4px; width: 98%" class="ui-corner-all;">
         <asp:Table runat="server" Width="97%" border="0" CellPadding="0" CellSpacing="0" align="center" Style="margin-right: 0px" >
             
             <asp:TableRow>
                 <asp:TableCell>
                    <%-- <div runat="server" id="divDetail" style="width:100%; margin:2px 2px 2px 2px; height:500px" visible="false">--%>
                         <rsweb:ReportViewer ID="COA_DetailedReportViewer" runat="server"
                             BorderColor="Blue" ToolbarForegroundDisabledColor="Red" 
       BackColor="" ClientIDMode="AutoID" HighlightBackgroundColor="" 
       InternalBorderColor="204, 204, 204" InternalBorderStyle="Solid"
       InternalBorderWidth="1px" LinkActiveColor="" LinkActiveHoverColor="" 
       LinkDisabledColor="Red" PrimaryButtonBackgroundColor="" 
       PrimaryButtonForegroundColor="" PrimaryButtonHoverBackgroundColor=""
       PrimaryButtonHoverForegroundColor="" SecondaryButtonBackgroundColor="" 
       SecondaryButtonForegroundColor="" SecondaryButtonHoverBackgroundColor="" 
       SecondaryButtonHoverForegroundColor="" SplitterBackColor="" ToolbarDividerColor=""
       ToolbarForegroundColor="" ToolbarHoverBackgroundColor="" ToolbarHoverForegroundColor="" 
       ToolBarItemBorderColor="" ToolBarItemBorderStyle="Solid" ToolBarItemBorderWidth="1px" 
       ToolBarItemHoverBackColor="" ToolBarItemPressedBorderColor="51, 102, 153" 
       ToolBarItemPressedBorderStyle="Solid" ToolBarItemPressedBorderWidth="1px" 
       ToolBarItemPressedHoverBackColor="153, 187, 226" Height="800px" style="margin-right: 13px" Width="100%" ShowReportBody="false" Visible="fAlse">
                              <%--<LocalReport ReportPath="Reports Rdlc\GL Reports\General Ledger Reports\ChartOfAccounts_DetailedReport.rdlc">
                                  </LocalReport>--%>
                         </rsweb:ReportViewer>
                       <%--  </div>
                     <div runat="server" id="divSummary" style="width:100%; margin:2px 2px 2px 2px; height:500px" visible="false">--%>
                        <rsweb:ReportViewer ID="COA_SummaryReportViewer" runat="server"
                             BorderColor="Blue" ToolbarForegroundDisabledColor="Red" 
       BackColor="" ClientIDMode="AutoID" HighlightBackgroundColor="" 
       InternalBorderColor="204, 204, 204" InternalBorderStyle="Solid"
       InternalBorderWidth="1px" LinkActiveColor="" LinkActiveHoverColor="" 
       LinkDisabledColor="Red" PrimaryButtonBackgroundColor="" 
       PrimaryButtonForegroundColor="" PrimaryButtonHoverBackgroundColor=""
       PrimaryButtonHoverForegroundColor="" SecondaryButtonBackgroundColor="" 
       SecondaryButtonForegroundColor="" SecondaryButtonHoverBackgroundColor="" 
       SecondaryButtonHoverForegroundColor="" SplitterBackColor="" ToolbarDividerColor=""
       ToolbarForegroundColor="" ToolbarHoverBackgroundColor="" ToolbarHoverForegroundColor="" 
       ToolBarItemBorderColor="" ToolBarItemBorderStyle="Solid" ToolBarItemBorderWidth="1px" 
       ToolBarItemHoverBackColor="" ToolBarItemPressedBorderColor="51, 102, 153" 
       ToolBarItemPressedBorderStyle="Solid" ToolBarItemPressedBorderWidth="1px" 
       ToolBarItemPressedHoverBackColor="153, 187, 226" Height="1000px" style="margin-right: 13px" Width="100%" ShowReportBody="true" Visible="true">
                             <%-- <LocalReport ReportPath="Reports Rdlc\GL Reports\General Ledger Reports\ChartOfAccounts_SummaryReport.rdlc">
                                  </LocalReport>--%>
                         </rsweb:ReportViewer>
                        <%-- </div>--%>
                     </asp:TableCell>
             </asp:TableRow>
         </asp:Table>


     </div>
            
                      </asp:TableCell>
                  </asp:TableRow>
              </asp:Table>
              </div>
            </ContentTemplate>
        </asp:UpdatePanel>
</asp:Content>
