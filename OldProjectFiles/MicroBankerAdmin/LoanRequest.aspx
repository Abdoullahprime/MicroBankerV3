<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="LoanRequest.aspx.vb" Inherits="MicroBankerAdmin.LoanRequest" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <asp:UpdateProgress ID="UpdateProgress1" runat="server">
        <ProgressTemplate>
            <img src="Img/ajax-loader1.gif" /><asp:Label ID="Label3" runat="server" Text="Please Wait..."
                ForeColor="Red"></asp:Label>
        </ProgressTemplate>
    </asp:UpdateProgress>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <table class="tbMain">
                <tbody>
                    <tr>
                        <td colspan="3">
                            <asp:Button ID="CmdApprovalList" OnClick="CmdApprovalList_Click" runat="server" Text="Approve Loan Requests" CausesValidation="False" CssClass="btn-New"></asp:Button>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <cc1:TabContainer ID="TabContainer1" runat="server" ActiveTabIndex="0">
                                <cc1:TabPanel runat="server" HeaderText="TabPanel1" ID="TabPanel1">
                                    <HeaderTemplate>Approve Loan Requests&nbsp;</HeaderTemplate>
                                    <ContentTemplate>


                                        <asp:MultiView ID="MultiView1" runat="server" OnActiveViewChanged="MultiView1_ActiveViewChanged">
                                            <asp:View ID="View1" runat="server">
                                                <div class="searchHolder">
                                                    <asp:RadioButtonList ID="RdoSearch" runat="server" OnSelectedIndexChanged="RdoSearch_SelectedIndexChanged"
                                                        RepeatDirection="Horizontal">
                                                        <asp:ListItem>Account No</asp:ListItem>
                                                        <asp:ListItem>Account Name</asp:ListItem>
                                                    </asp:RadioButtonList>
                                                </div>


                                                <div class="searchHolder">
                                                    <div class="row">
                                                        <div class="col-md-6">
                                                            <asp:TextBox ID="txtSearch" runat="server" OnTextChanged="txtSearch_TextChanged" CssClass="tinyText"></asp:TextBox>
                                                        </div>
                                                        <div class="col-md-3">
                                                            <asp:Button ID="btnSearch" OnClick="btnSearch_Click" runat="server" Text="Search" CssClass="btn-Save"></asp:Button>
                                                        </div>
                                                        <div class="col-md-3">
                                                            <asp:Button ID="Button10" OnClick="Button10_Click" runat="server" Text="View All"
                                                                CssClass="btn-Save"></asp:Button>
                                                        </div>
                                                    </div>
                                                </div>

                                                <div class="gridHolder">
                                                    <asp:GridView ID="GridView1" runat="server" EnableTheming="True" CellPadding="4" AutoGenerateColumns="False" AllowPaging="True"
                                                        BackColor="White" BorderColor="White" BorderStyle="Solid" BorderWidth="3px" CssClass="GridView table-striped" ForeColor="Black" GridLines="Vertical">
                                                        <AlternatingRowStyle BackColor="White"></AlternatingRowStyle>
                                                        <Columns>
                                                            <asp:BoundField DataField="AccountNo" HeaderText="ACCOUNT NO"></asp:BoundField>
                                                            <asp:BoundField DataField="AccountName" HeaderText="ACCOUNT NAME"></asp:BoundField>
                                                            <asp:BoundField DataField="Amount" HeaderText="AMOUNT"></asp:BoundField>
                                                            <asp:BoundField DataField="RequestDate" HeaderText="REQUEST DATE"></asp:BoundField>
                                                            <asp:TemplateField>
                                                                <ItemTemplate>
                                                                    <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandName="select" CommandArgument='<%#Eval("AccountNo") %>'>select</asp:LinkButton>
                                                                </ItemTemplate>
                                                            </asp:TemplateField>
                                                        </Columns>
                                                        <EditRowStyle BackColor="#7C6F57"></EditRowStyle>
                                                        <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White"></FooterStyle>
                                                        <HeaderStyle BackColor="#0F3B5A" Font-Bold="True" ForeColor="White"></HeaderStyle>
                                                        <PagerStyle HorizontalAlign="Center" BackColor="#666666" ForeColor="White"></PagerStyle>
                                                        <RowStyle BackColor="#E3EAEB"></RowStyle>
                                                        <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333"></SelectedRowStyle>
                                                    </asp:GridView>
                                                </div>


                                            </asp:View>
                                            <asp:View ID="View2" runat="server" EnableViewState="False">
                                                <br />
                                                <table class="tbMain">
                                                    <tbody>

                                                        <tr>
                                                            <td class="CriteriaCell" colspan="6">
                                                                <asp:Label ID="Label1" runat="server" Text="Fields marked with * are mandatory " CssClass="Criteria" ForeColor="Red"></asp:Label><br />
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="CriteriaCell" colspan="9">
                                                                <asp:Label ID="lblSuccess" runat="server" ForeColor="Red"></asp:Label>
                                                                <asp:Label ID="lblResponse" runat="server" ForeColor="Red"></asp:Label>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td style="height: 23px" colspan="3" class="text-left">&nbsp;<strong style="font-weight: bold; color: black">Loan Request Details</strong>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1 text-left">Account Number:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtAccountNos" runat="server" CssClass="tinyText" MaxLength="10"></asp:TextBox>

                                                                <asp:Label ID="lblAccountNos" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                            </td>
                                                            <td class="tbCell1 text-left">Account Name:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtAccountName" runat="server" CssClass="tinyText" MaxLength="50"></asp:TextBox>

                                                                <asp:Label ID="Label6" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1 text-left">Mobile No:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtMobileNos" runat="server" CssClass="tinyText" MaxLength="10"></asp:TextBox>

                                                                <asp:Label ID="Label12" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                            </td>
                                                            <td class="tbCell1 text-left">Amount Applied:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtAmount" runat="server" CssClass="tinyText" MaxLength="50"></asp:TextBox>
                                                                <asp:ImageButton ID="ImageButton1" runat="server" Style="width: 23px; height: 23px; padding-top: 5px;" ImageUrl="~/Img/Calendar_scheduleHS.png" />

                                                                <asp:Label ID="Label2" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                            </td>
                                                        </tr>
                                                        <tr>


                                                            <td class="tbCell1 text-left">Request Date:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtDoB" runat="server" CssClass="tinyText" MaxLength="50"></asp:TextBox>
                                                                <asp:ImageButton ID="btncalendars" runat="server" Style="width: 23px; height: 23px; padding-top: 5px;" ImageUrl="~/Img/Calendar_scheduleHS.png" />

                                                                <asp:Label ID="Label23" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                            </td>
                                                            <td class="tbCell1 text-left">Expected Date:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtBizRegDate" runat="server" CssClass="tinyText" MaxLength="15"></asp:TextBox>
                                                                <asp:ImageButton ID="btncalendar" runat="server" Style="width: 23px; height: 23px; padding-top: 5px;" ImageUrl="~/Img/Calendar_scheduleHS.png" />

                                                                <asp:Label ID="Label10" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                            </td>
                                                        </tr>
                                                        <tr>

                                                            <td class="tbCell1 text-left">Reason For Application:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtReason" runat="server" CssClass="tinyText" MaxLength="20"></asp:TextBox>

                                                                <asp:Label ID="lblTinNo" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>

                                                            </td>



                                                        </tr>
                                                    </tbody>
                                                </table>

                                                <table class="tbMain">
                                                    <tbody>
                                                        <tr>
                                                            <td class="tbCell1">
                                                                <asp:Button ID="Button2" OnClick="Button6_Click" runat="server" Text="Previous"
                                                                    CssClass="btn-Cancel" Visible="False"></asp:Button>
                                                            </td>
                                                            <td class="tbCell1">
                                                                <asp:Button ID="btnValidate" OnClick="btnValidate_Click" runat="server" Text="Validate" CssClass="btn-Save"></asp:Button>
                                                            </td>
                                                            <td class="tbCell1">
                                                                <asp:Button ID="btnIndDelete" OnClick="btnIndDelete_Click1" runat="server" Text="Delete"
                                                                    CssClass="btn-Cancel" Visible="False"></asp:Button>
                                                            </td>
                                                            <td class="tbCell1">
                                                                <asp:Button ID="Button12" OnClick="Button1_Click" runat="server"
                                                                    Text="View Accounts" CssClass="btn-Save" Visible="False"></asp:Button>
                                                            </td>
                                                        </tr>
                                                    </tbody>
                                                </table>


                                                <hr style="position: relative; height: 12px; background: black; width: 90%; margin: auto;" />
                                                <asp:Panel ID="General" Style="margin-bottom: 20px;" runat="server" Visible="False">

                                                    <table class="tbMain">
                                                        <tbody>
                                                            <tr>
                                                                <td style="height: 23px" colspan="3" class="text-left">&nbsp;<strong style="font-weight: bold; color: black">Core Account Details</strong>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td class="tbCell1 text-left">Branch Code:<b class="boldStar"></b>&nbsp; </td>
                                                                <td class="tbCell2">
                                                                    <asp:TextBox ID="txtBranchCode" runat="server" CssClass="ShortText" MaxLength="3" Enabled="False"></asp:TextBox>
                                                                </td>

                                                                <td class="tbCell1">Branch Name:</td>
                                                                <td class="tbCell2">
                                                                    <asp:TextBox ID="txtBranchName" runat="server" CssClass="tinyText" Enabled="False" MaxLength="20"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td class="tbCell1 text-left">Account Type Code:<b class="boldStar"></b></td>
                                                                <td class="tbCell2">
                                                                    <asp:TextBox ID="txtAccountCode" runat="server" CssClass="ShortText" MaxLength="4" Enabled="False"></asp:TextBox>
                                                                </td>

                                                                <td class="tbCell1 text-left">Product Name:</td>
                                                                <td class="tbCell2">
                                                                    <asp:TextBox ID="txtProductName" runat="server" OnTextChanged="txtEmail_TextChanged" Enabled="False" CssClass="tinyText" MaxLength="14"></asp:TextBox>
                                                                </td>
                                                            </tr>

                                                            <tr>
                                                                <td class="tbCell1  text-left">Account Number:<b class="boldStar"></b></td>
                                                                <td class="tbCell2">
                                                                    <asp:TextBox ID="txtAccountNo" runat="server" CssClass="tinyText" MaxLength="20" ReadOnly="True" Enabled="False"></asp:TextBox>
                                                                </td>
                                                                <td class="tbCell1 text-left">Account Name:<b class="boldStar"></b></td>
                                                                <td class="tbCell2">
                                                                    <asp:TextBox ID="txtAcountName" runat="server" OnTextChanged="txtAdress3_TextChanged" CssClass="tinyText" MaxLength="50" Enabled="False"></asp:TextBox>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td class="tbCell1 text-left">Book Balance:</td>
                                                                <td class="tbCell2">
                                                                    <asp:TextBox ID="txtBookBal" runat="server" CssClass="tinyText" MaxLength="100" TextMode="MultiLine" Enabled="False"></asp:TextBox>
                                                                    <asp:Label ID="lblAdress1" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                                                </td>
                                                                <td class="tbCell1 text-left">Available Balance:</td>
                                                                <td class="tbCell2">
                                                                    <asp:TextBox ID="txtAvailBal" runat="server" CssClass="tinyText" MaxLength="20" Enabled="False"></asp:TextBox>

                                                                    <asp:Label ID="Label21" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>

                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td class="tbCell1 text-left">Blocked Amount:</td>
                                                                <td class="tbCell2">
                                                                    <asp:TextBox ID="txtBlockedAmount" runat="server" CssClass="tinyText" MaxLength="100" TextMode="MultiLine" Enabled="False"></asp:TextBox>
                                                                    <asp:Label ID="Label27" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                                                </td>
                                                        </tbody>
                                                    </table>
                                                </asp:Panel>


                                                <table class="tabMain">
                                                    <tbody>
                                                        <tr>
                                                            <td>
                                                                <cc1:TabContainer ID="TabContainer2" runat="server" ActiveTabIndex="0" CssClass="" Visible="False">


                                                                    <cc1:TabPanel runat="server" HeaderText="TabPanel1" ID="TabPanel2">
                                                                        <HeaderTemplate>
                                                                            
                                                                        </HeaderTemplate>
                                                                        <ContentTemplate>
                                                                            <div class="tbMain">
                                                                                <div class="row">
                                                                                    <div class="col-md-6">
                                                                                        <table class="tbMain">
                                                                                            <tbody>
                                                                                                <tr>
                                                                                                    <td colspan="6">

                                                                                                        <asp:GridView ID="SODGridView" runat="server" EmptyDataText="No Data To Display" CellPadding="4" AutoGenerateColumns="False"
                                                                                                            AllowPaging="True" BackColor="White" BorderColor="White" BorderStyle="Solid" BorderWidth="3px" CssClass="GridView table-striped" ForeColor="Black" GridLines="Vertical">

                                                                                                            <AlternatingRowStyle BackColor="White"></AlternatingRowStyle>
                                                                                                            <Columns>

                                                                                                                <asp:TemplateField HeaderText="Account No">
                                                                                                                    <ItemTemplate>
                                                                                                                        <asp:Label ID="AccountNo" runat="server" Text='<%# Eval("AccountNo") %>'></asp:Label>
                                                                                                                    </ItemTemplate>
                                                                                                                    <ItemStyle Width="150px" />
                                                                                                                </asp:TemplateField>

                                                                                                                <asp:TemplateField HeaderText="Account Name">
                                                                                                                    <ItemTemplate>
                                                                                                                        <asp:Label ID="AccountName" runat="server" Text='<%# Eval("AccountName") %>'></asp:Label>
                                                                                                                    </ItemTemplate>
                                                                                                                    <ItemStyle Width="250px" />
                                                                                                                </asp:TemplateField>

                                                                                                                <asp:TemplateField HeaderText="Branch Code" Visible="True">
                                                                                                                    <ItemTemplate>
                                                                                                                        <asp:Label ID="BranchCode" runat="server" Text='<%# Eval("BranchCode") %>'></asp:Label>
                                                                                                                    </ItemTemplate>
                                                                                                                </asp:TemplateField>


                                                                                                                <asp:TemplateField HeaderText="Link Account" InsertVisible="False">
                                                                                                                    <ItemTemplate>
                                                                                                                        <asp:CheckBox ID="checkSelect" runat="server" Visible="true" />
                                                                                                                    </ItemTemplate>
                                                                                                                </asp:TemplateField>

                                                                                                                <asp:TemplateField>
                                                                                                                    <ItemTemplate>
                                                                                                                        <%--<asp:Label ID="Status" runat="server" Text='' Visible="false"></asp:Label>--%>
                                                                                                                        <asp:Image ID="success" runat="server" ImageUrl="~/Img/tick.png" Visible="false" Style="width: 20px; height: 20px;" />
                                                                                                                        <asp:Image ID="fail" runat="server" ImageUrl="~/Img/cross.jpg" Visible="false" Style="width: 20px; height: 20px;" />
                                                                                                                        <asp:UpdateProgress ID="UpdateProgress1" runat="server">
                                                                                                                            <ProgressTemplate>
                                                                                                                                <asp:Image ID="loader" src="Img/ajax-loader1.gif" runat="server" Style="width: 15px" />
                                                                                                                            </ProgressTemplate>
                                                                                                                        </asp:UpdateProgress>
                                                                                                                    </ItemTemplate>
                                                                                                                    <ItemStyle Width="30px" />
                                                                                                                </asp:TemplateField>
                                                                                                            </Columns>


                                                                                                            <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White"></FooterStyle>

                                                                                                            <HeaderStyle BackColor="#0F3B5A" Font-Bold="True" ForeColor="White"></HeaderStyle>

                                                                                                            <PagerStyle HorizontalAlign="Center" BackColor="#666666" ForeColor="White"></PagerStyle>

                                                                                                            <RowStyle BackColor="#E3EAEB"></RowStyle>
                                                                                                            <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333"></SelectedRowStyle>
                                                                                                        </asp:GridView>
                                                                                                    </td>
                                                                                                </tr>
                                                                                            </tbody>
                                                                                        </table>
                                                                                    </div>
                                                                                </div>
                                                                            </div>
                                                                        </ContentTemplate>
                                                                    </cc1:TabPanel>
                                                                    <cc1:TabPanel runat="server" HeaderText="TabPanel1" ID="TabPanel3" Visible ="false">
                                                                        <HeaderTemplate>
                                                                            
                                                                        </HeaderTemplate>
                                                                        <ContentTemplate>
                                                                            <div class="tbMain">
                                                                                <div class="row">
                                                                                    <div class="col-md-6">
                                                                                        <table class="tbMain">
                                                                                            <tbody>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Title</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:DropDownList ID="drpdPrefix" runat="server" CssClass="tinyText">
                                                                                                            <asp:ListItem></asp:ListItem>
                                                                                                            <asp:ListItem>MR</asp:ListItem>
                                                                                                            <asp:ListItem>MRS</asp:ListItem>
                                                                                                            <asp:ListItem>MISS</asp:ListItem>
                                                                                                            <asp:ListItem>DR</asp:ListItem>
                                                                                                        </asp:DropDownList>


                                                                                                        <asp:Label ID="Label15" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">First Name:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtFirstName" runat="server" CssClass="tinyText" MaxLength="25"></asp:TextBox>

                                                                                                        <asp:Label ID="lblFname" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Middle Name:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtMiddleName" runat="server" CssClass="tinyText" MaxLength="25"></asp:TextBox>

                                                                                                        <asp:Label ID="Label9" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Last Name:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtLastName" runat="server" CssClass="tinyText" MaxLength="25"></asp:TextBox>

                                                                                                        <asp:Label ID="lblLname" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Marital Status</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:DropDownList ID="drpMaritalStatus" runat="server" CssClass="tinyText">
                                                                                                            <asp:ListItem></asp:ListItem>
                                                                                                            <asp:ListItem>SINGLE</asp:ListItem>
                                                                                                            <asp:ListItem>MARRIED</asp:ListItem>
                                                                                                            <asp:ListItem>DIVORCED</asp:ListItem>
                                                                                                            <asp:ListItem>WIDOW</asp:ListItem>
                                                                                                            <asp:ListItem>WIDOWER</asp:ListItem>
                                                                                                        </asp:DropDownList>
                                                                                                        <asp:Label ID="Label5" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Spouse Name:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtSpouseName" runat="server" CssClass="tinyText" MaxLength="50"></asp:TextBox>
                                                                                                        <asp:Label ID="Label43" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Dependants:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtDepemdant" runat="server" CssClass="tinyText" MaxLength="5"></asp:TextBox>
                                                                                                        <asp:Label ID="lblDependants" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Occupation:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtOccupation" runat="server" CssClass="tinyText" MaxLength="50"></asp:TextBox>
                                                                                                        <asp:Label ID="Label72" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Region:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:DropDownList ID="drpRegion" runat="server" CssClass="tinyText">
                                                                                                        </asp:DropDownList>

                                                                                                        <asp:Label ID="Label16" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                            </tbody>
                                                                                        </table>
                                                                                    </div>
                                                                                    <div class="col-md-6">
                                                                                        <table class="tbMain">
                                                                                            <tbody>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">District:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:DropDownList ID="drpDistrict" runat="server" CssClass="tinyText">
                                                                                                        </asp:DropDownList>

                                                                                                        <asp:Label ID="Label17" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Area:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:DropDownList ID="drpArea" runat="server" CssClass="tinyText">
                                                                                                        </asp:DropDownList>

                                                                                                        <asp:Label ID="Label18" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Residential Status:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtResidential" runat="server" CssClass="tinyText" MaxLength="50"></asp:TextBox>

                                                                                                        <asp:Label ID="Label19" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Sex:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtSex" runat="server" CssClass="tinyText" MaxLength="50"></asp:TextBox>

                                                                                                        <asp:Label ID="Label20" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Nationality:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtNationality" runat="server" CssClass="tinyText" MaxLength="50"></asp:TextBox>

                                                                                                        <asp:Label ID="Label22" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>

                                                                                                <tr>
                                                                                                    <td>
                                                                                                        <asp:Calendar ID="DOBDatePicker" runat="server" BackColor="White" CssClass="auto-style1" Visible="False">
                                                                                                            <DayHeaderStyle CssClass="myCalendarDayHeader" ForeColor="White" BackColor="#0F3B5A" />

                                                                                                            <DayStyle CssClass="myCalendarDay" ForeColor="#2D3338" />

                                                                                                            <NextPrevStyle BackColor="#0F3B5A" ForeColor="White"></NextPrevStyle>

                                                                                                            <OtherMonthDayStyle ForeColor="#B0B0B0" />

                                                                                                            <SelectedDayStyle Font-Bold="True" Font-Size="12px" CssClass="myCalendarSelector" />

                                                                                                            <TitleStyle CssClass="myCalendarTitle" />
                                                                                                        </asp:Calendar>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Language:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtLanguage" runat="server" CssClass="tinyText" MaxLength="50"></asp:TextBox>

                                                                                                        <asp:Label ID="Label24" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>


                                                                                            </tbody>
                                                                                        </table>
                                                                                    </div>
                                                                                </div>
                                                                            </div>
                                                                        </ContentTemplate>
                                                                    </cc1:TabPanel>

                                                                    <cc1:TabPanel runat="server" HeaderText="TabPanel2" ID="TabPanel4" Visible ="false" >
                                                                        <HeaderTemplate>
                                                                            
                                                                        </HeaderTemplate>
                                                                        <ContentTemplate>
                                                                            <div class="tbMain">
                                                                                <div class="row">
                                                                                    <div class="col-md-6">
                                                                                        <table class="tbMain">
                                                                                            <tbody>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Business Name:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtBizName" runat="server" CssClass="tinyText" MaxLength="50"></asp:TextBox>

                                                                                                        <asp:Label ID="lblBizName" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Business Tel No:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtBizTel" runat="server" CssClass="tinyText" MaxLength="50"></asp:TextBox>

                                                                                                        <asp:Label ID="Label26" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>


                                                                                                <tr>
                                                                                                    <td>
                                                                                                        <asp:Calendar ID="BizRegDatePicker" runat="server" BackColor="White" CssClass="myCalendar" Visible="false">
                                                                                                            <DayHeaderStyle CssClass="myCalendarDayHeader" ForeColor="White" BackColor="#0F3B5A" />

                                                                                                            <DayStyle CssClass="myCalendarDay" ForeColor="#2D3338" />

                                                                                                            <NextPrevStyle BackColor="#0F3B5A" ForeColor="White"></NextPrevStyle>

                                                                                                            <OtherMonthDayStyle ForeColor="#B0B0B0" />

                                                                                                            <SelectedDayStyle Font-Bold="True" Font-Size="12px" CssClass="myCalendarSelector" />

                                                                                                            <TitleStyle CssClass="myCalendarTitle" />
                                                                                                        </asp:Calendar>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Registration No:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtRegNo" runat="server" CssClass="tinyText" Height="20px" MaxLength="10"></asp:TextBox>

                                                                                                        <asp:Label ID="lblRegNo" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                            </tbody>
                                                                                        </table>
                                                                                    </div>
                                                                                    <div class="col-md-6">
                                                                                        <table class="tbMain">
                                                                                            <tbody>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Nature of business:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtBizNature" runat="server" CssClass="tinyText" MaxLength="50"></asp:TextBox>

                                                                                                        <asp:Label ID="Label25" runat="server" ForeColor="Red" Width="70px"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Director Name:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtDirName" runat="server" CssClass="tinyText" MaxLength="50"></asp:TextBox>

                                                                                                        <asp:Label ID="Label13" runat="server" ForeColor="Red" Width="70px"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">RelationShip Officer:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtRelOfficer" runat="server" CssClass="tinyText" MaxLength="50"></asp:TextBox>
                                                                                                    </td>
                                                                                                </tr>
                                                                                            </tbody>
                                                                                        </table>
                                                                                    </div>
                                                                                </div>
                                                                            </div>
                                                                        </ContentTemplate>
                                                                    </cc1:TabPanel>

                                                                    <cc1:TabPanel runat="server" HeaderText="TabPanel3" ID="TabPanel5" Visible ="false">
                                                                        <HeaderTemplate>
                                                                            
                                                                        </HeaderTemplate>
                                                                        <ContentTemplate>
                                                                            <div class="tbMain">
                                                                                <div class="row">
                                                                                    <div class="col-md-6">
                                                                                        <table class="tbMain">
                                                                                            <tbody>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Group Name:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtgrpName" runat="server" CssClass="tinyText" MaxLength="50"></asp:TextBox>

                                                                                                        <asp:Label ID="lblGroupName" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Number of Members:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtgrpMemberNo" runat="server" CssClass="tinyText" MaxLength="5"></asp:TextBox>

                                                                                                        <asp:Label ID="lblMembers" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Number of Male:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtMaleNo" runat="server" CssClass="tinyText" MaxLength="5"></asp:TextBox>

                                                                                                        <asp:Label ID="lblMaleNo" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Number of Female:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtFemaleNo" runat="server" CssClass="tinyText" MaxLength="5"></asp:TextBox>

                                                                                                        <asp:Label ID="lblFemaleNo" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                            </tbody>
                                                                                        </table>
                                                                                    </div>
                                                                                    <div class="col-md-6">
                                                                                        <table class="tbMain">
                                                                                            <tbody>
                                                                                                <tr>
                                                                                                    <td class="tbCell1  text-left">Currency:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtCurrency" runat="server" CssClass="tinyText" MaxLength="4"></asp:TextBox>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Sector:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:DropDownList ID="drpSector" runat="server" CssClass="tinyText">
                                                                                                        </asp:DropDownList>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Sub Sector:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:DropDownList ID="drpSubSector" runat="server" CssClass="tinyText">
                                                                                                        </asp:DropDownList>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td colspan="3" class="tbCell1  text-left">
                                                                                                        <asp:Label ID="Label11" runat="server" ForeColor="Red"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td colspan="3" class="tbCell1  text-left">
                                                                                                        <asp:Label ID="Label14" runat="server" ForeColor="Red"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                            </tbody>
                                                                                        </table>
                                                                                    </div>
                                                                                </div>
                                                                            </div>
                                                                        </ContentTemplate>
                                                                    </cc1:TabPanel>


                                                                </cc1:TabContainer>

                                                            </td>
                                                        </tr>
                                                    </tbody>
                                                </table>

                                                <table class="tbMain">
                                                    <tbody>
                                                        <tr>
                                                            <td class="tbCell1">
                                                                <asp:Button ID="btnSave" runat="server" CssClass="btn-Save" OnClick="btnSave_Click" Text="Approve" Visible="False" />
                                                            </td>
                                                            <td class="tbCell1">
                                                                <asp:Button ID="btnClear" runat="server" CausesValidation="False" CssClass="btn-Save" OnClick="btnClear_Click" Text="Clear" Visible="False" />
                                                            </td>
                                                            <td class="tbCell1">
                                                                <asp:Button ID="cmddelete" runat="server" CssClass="btn-Cancel" OnClick="cmddelete_Click" Text="Delete" Visible="False" />
                                                            </td>
                                                            <td class="tbCell1  text-left">&nbsp;</td>
                                                            <td class="tbCell2">
                                                                <asp:HiddenField ID="HiddenField2" runat="server" />
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1  text-left"></td>
                                                            <td class="tbCell2">
                                                                <cc1:ConfirmButtonExtender ID="ConfirmButtonExtender1" runat="server" ConfirmText="Are you sure you want to Approve?" Enabled="True" TargetControlID="btnSave">
                                                                </cc1:ConfirmButtonExtender>
                                                                <cc1:ConfirmButtonExtender ID="ConfirmButtonExtender2" runat="server" ConfirmText="Are you sure you want to Delete?" Enabled="True" TargetControlID="cmddelete">
                                                                </cc1:ConfirmButtonExtender>
                                                            </td>
                                                        </tr>
                                                    </tbody>

                                                </table>

                                            </asp:View>
                                        </asp:MultiView>

                                        <asp:Wizard ID="Wizard1" runat="server" Width="714px" BackColor="#E6E2D8" Font-Size="0.8em"
                                            BorderWidth="1px" BorderStyle="Solid" BorderColor="#999999" OnFinishButtonClick="Wizard1_FinishButtonClick"
                                            Visible="False" Font-Names="Verdana" DisplaySideBar="False" ActiveStepIndex="1">
                                            <FinishCompleteButtonStyle Font-Size="Medium" Height="30px" Width="80px"></FinishCompleteButtonStyle>
                                            <FinishPreviousButtonStyle Font-Size="Medium" Height="30px" Width="80px"></FinishPreviousButtonStyle>
                                            <HeaderStyle HorizontalAlign="Center" BackColor="#666666" BorderColor="#E6E2D8" BorderWidth="2px"
                                                BorderStyle="Solid" Font-Bold="True" Font-Size="0.9em" ForeColor="White"></HeaderStyle>
                                            <NavigationButtonStyle BackColor="White" BorderColor="#C5BBAF" BorderWidth="1px"
                                                BorderStyle="Solid" Font-Names="Verdana" Font-Size="0.8em" ForeColor="#1C5E55"></NavigationButtonStyle>
                                            <StartNextButtonStyle Font-Size="Medium" Height="30px" Width="80px"></StartNextButtonStyle>
                                            <StepNextButtonStyle Font-Size="Medium" Height="30px" Width="80px"></StepNextButtonStyle>
                                            <StepPreviousButtonStyle Font-Overline="False" Font-Size="Medium" Height="30px" Width="80px"></StepPreviousButtonStyle>
                                            <SideBarButtonStyle ForeColor="White"></SideBarButtonStyle>
                                            <SideBarStyle VerticalAlign="Top" BackColor="#1C5E55" Font-Size="0.9em"></SideBarStyle>
                                            <StepStyle BackColor="#F7F6F3" BorderColor="#E6E2D8" BorderWidth="2px" BorderStyle="Solid"></StepStyle>
                                            <WizardSteps>
                                                <asp:WizardStep runat="server" Title="BulkPayType">
                                                    <br />
                                                    <br />
                                                    <br />
                                                </asp:WizardStep>
                                                <asp:WizardStep runat="server" Title="Upload File">
                                                    &#160;&#160;<br />
                                                    <br />
                                                    <br />
                                                    <br />
                                                    <br />
                                                    <br />
                                                    <br />
                                                    <table style="width: 712px">
                                                        <tbody>
                                                            <tr>
                                                                <td colspan="3">&nbsp; </td>
                                                            </tr>
                                                            <tr>
                                                                <td></td>
                                                                <td></td>
                                                                <td></td>
                                                            </tr>
                                                            <tr>
                                                                <td></td>
                                                                <td></td>
                                                                <td></td>
                                                            </tr>
                                                        </tbody>
                                                    </table>
                                                    <br />
                                                </asp:WizardStep>
                                                <asp:WizardStep runat="server" Title="Linking ">
                                                    <table style="width: 705px; height: 71px">
                                                        <tr>
                                                            <td colspan="3">
                                                                <asp:Label runat="server" Text="Account Linking" ForeColor="Red" ID="Label4"></asp:Label></td>
                                                        </tr>
                                                        <tr>
                                                            <td>Customer Number: </td>
                                                            <td>
                                                                <asp:TextBox runat="server" ID="TextBox1"></asp:TextBox></td>
                                                            <td></td>
                                                        </tr>
                                                        <tr>
                                                            <td>Mwallet Account Number: </td>
                                                            <td>
                                                                <asp:TextBox runat="server" ID="TextBox2"></asp:TextBox></td>
                                                            <td></td>
                                                        </tr>
                                                        <tr>
                                                            <td></td>
                                                            <td></td>
                                                            <td></td>
                                                        </tr>
                                                        <tr>
                                                            <td></td>
                                                            <td></td>
                                                            <td></td>
                                                        </tr>
                                                        <tr>
                                                            <td></td>
                                                            <td></td>
                                                            <td></td>
                                                        </tr>
                                                        <tr>
                                                            <td></td>
                                                            <td></td>
                                                            <td></td>
                                                        </tr>
                                                    </table>
                                                </asp:WizardStep>
                                            </WizardSteps>
                                        </asp:Wizard>
                                        <br />
                                        <br />
                                        <asp:Wizard ID="Wizard2" runat="server" Width="714px" BackColor="#E6E2D8" Font-Size="0.8em"
                                            BorderWidth="1px" BorderStyle="Solid" BorderColor="#999999" OnFinishButtonClick="Wizard2_FinishButtonClick"
                                            Visible="False" Font-Names="Verdana" DisplaySideBar="False" ActiveStepIndex="2">
                                            <FinishCompleteButtonStyle Font-Size="Medium" Height="30px" Width="80px"></FinishCompleteButtonStyle>
                                            <FinishPreviousButtonStyle Font-Size="Medium" Height="30px" Width="80px"></FinishPreviousButtonStyle>
                                            <HeaderStyle HorizontalAlign="Center" BackColor="#666666" BorderColor="#E6E2D8" BorderWidth="2px"
                                                BorderStyle="Solid" Font-Bold="True" Font-Size="0.9em" ForeColor="White"></HeaderStyle>
                                            <NavigationButtonStyle BackColor="White" BorderColor="#C5BBAF" BorderWidth="1px"
                                                BorderStyle="Solid" Font-Names="Verdana" Font-Size="0.8em" ForeColor="#1C5E55"></NavigationButtonStyle>
                                            <StartNextButtonStyle Font-Size="Medium" Height="30px" Width="80px"></StartNextButtonStyle>
                                            <StepNextButtonStyle Font-Size="Medium" Height="30px" Width="80px"></StepNextButtonStyle>
                                            <StepPreviousButtonStyle Font-Overline="False" Font-Size="Medium" Height="30px" Width="80px"></StepPreviousButtonStyle>
                                            <SideBarButtonStyle ForeColor="White"></SideBarButtonStyle>
                                            <SideBarStyle VerticalAlign="Top" BackColor="#1C5E55" Font-Size="0.9em"></SideBarStyle>
                                            <StepStyle BackColor="#F7F6F3" BorderColor="#E6E2D8" BorderWidth="2px" BorderStyle="Solid"></StepStyle>
                                            <WizardSteps>
                                                <asp:WizardStep runat="server" Title="BulkPayType"></asp:WizardStep>
                                                <asp:WizardStep runat="server" Title="Upload File"></asp:WizardStep>
                                                <asp:WizardStep runat="server" Title="linking"></asp:WizardStep>
                                            </WizardSteps>
                                        </asp:Wizard>

                                    </ContentTemplate>
                                </cc1:TabPanel>
                            </cc1:TabContainer>
                        </td>
                    </tr>
                    <tr>
                        <td></td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr>
                        <td>
                            <asp:HiddenField ID="HiddenField1" runat="server"></asp:HiddenField>
                        </td>
                        <td></td>
                        <td></td>
                    </tr>
                </tbody>
            </table>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
<asp:Content ID="Content2" runat="server" ContentPlaceHolderID="Header">
    <style type="text/css">
        .auto-style1 {
            border-left: 10px solid #0f3b5a;
            border-right: 10px solid #0f3b5a;
            border-bottom: 10px solid #0f3b5a;
            background-color: #f2f2f2;
            width: 160px;
            margin-top: 2px;
            position: absolute;
            left: 0px;
            top: 314px;
            border-top-style: none;
            border-top-color: inherit;
            border-top-width: 0px;
        }
    </style>
</asp:Content>

