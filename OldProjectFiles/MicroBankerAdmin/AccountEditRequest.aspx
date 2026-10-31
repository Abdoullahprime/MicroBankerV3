<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="AccountEditRequest.aspx.vb" Inherits="MicroBankerAdmin.AccountEditRequest" %>

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
                    <table class="tbMain">
                        <tbody>
                            <tr>
                                <td colspan="3">
                                    <asp:Button ID="CmdApprovalList" OnClick="CmdApprovalList_Click" runat="server" Text="Edit Account Request" CausesValidation="False" CssClass="btn-New"></asp:Button>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <cc1:TabContainer ID="TabContainer1" runat="server" ActiveTabIndex="0">
                                        <cc1:TabPanel runat="server" HeaderText="TabPanel1" ID="TabPanel1">
                                            <HeaderTemplate>Request Account Edit&nbsp;</HeaderTemplate>
                                            <ContentTemplate>
                                                <table class="tbMain">
                                                    <tbody>
                                                        <tr>
                                                            <td colspan="3">
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
                                                                                    <asp:BoundField DataField="BranchName" HeaderText="BRANCH NAME"></asp:BoundField>
                                                                                    <asp:BoundField DataField="C_Address1" HeaderText="ADDRESS 1"></asp:BoundField>
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
                                                                                        <asp:Label ID="Label2" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td class="tbCell1 text-left">Branch Code:<b class="boldStar">*</b>&nbsp; </td>
                                                                                    <td class="tbCell2">
                                                                                        <asp:TextBox ID="txtBranchCode" runat="server" OnTextChanged="txtAccountCode_TextChanged" CssClass="ShortText" MaxLength="3" Enabled="False"></asp:TextBox>
                                                                                        <asp:Button ID="btnCheckBranch" OnClick="btnCheckBranch_Click" CssClass="btn-Check" runat="server" Text="Check" CausesValidation="False"></asp:Button>
                                                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" Display="Dynamic" CssClass="Validator" SetFocusOnError="True" ErrorMessage="*" ControlToValidate="txtBranchCode">Enter Branch Code</asp:RequiredFieldValidator>
                                                                                    </td>

                                                                                    <td class="tbCell1">Branch Name:</td>
                                                                                    <td class="tbCell2">
                                                                                        <asp:TextBox ID="txtBranchName" runat="server" CssClass="tinyText" Enabled="False" MaxLength="20"></asp:TextBox>
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td class="tbCell1 text-left">Account Type Code:<b class="boldStar">*</b></td>
                                                                                    <td class="tbCell2">
                                                                                        <asp:TextBox ID="txtAccountCode" runat="server" OnTextChanged="txtAccountCode_TextChanged" CssClass="ShortText" MaxLength="4" Enabled="False"></asp:TextBox>
                                                                                        <asp:Button ID="btnSearchCode" OnClick="btnSearchCode_Click" runat="server" CssClass="btn-Check" Text="Check" CausesValidation="False"></asp:Button>
                                                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" Display="Dynamic" CssClass="Validator" SetFocusOnError="True" ErrorMessage="*" ControlToValidate="txtAccountCode">Enter Account Code</asp:RequiredFieldValidator>
                                                                                    </td>

                                                                                    <td class="tbCell1 text-left">Product Name:</td>
                                                                                    <td class="tbCell2">
                                                                                        <asp:TextBox ID="txtProductName" runat="server" OnTextChanged="txtEmail_TextChanged" Enabled="False" CssClass="tinyText" MaxLength="14"></asp:TextBox>
                                                                                    </td>
                                                                                </tr>
 <asp:TextBox ID="txtProductCode" runat="server" MaxLength="3"  Visible="False" Width="16px"></asp:TextBox>

                                                                                <tr>
                                                                                    <td class="tbCell1 text-left">Customer No:<b class="boldStar">*</b></td>
                                                                                    <td class="tbCell2">
                                                                                        <asp:TextBox ID="txtCustomerNo" runat="server" OnTextChanged="txtAccountCode_TextChanged" CssClass="ShortText" MaxLength="6" Enabled="False"></asp:TextBox>
                                                                                        <asp:Button ID="cmasearch" OnClick="cmasearch_Click" runat="server" CssClass="btn-Check" Text="Check" CausesValidation="False"></asp:Button>
                                                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" Display="Dynamic" CssClass="Validator" SetFocusOnError="True" ErrorMessage="*" ControlToValidate="txtCustomerNo">Enter Customer No</asp:RequiredFieldValidator>
                                                                                    </td>
                                                                                </tr>



                                                                                <tr>
                                                                                    <td class="tbCell1 text-left">Select Account Type:</td>
                                                                                    <td class="tbCell2">
                                                                                        <asp:DropDownList ID="drpAccountType" runat="server" CssClass="tinyText" Enabled="False">
                                                                                            <asp:ListItem></asp:ListItem>
                                                                                            <asp:ListItem>INDIVIDUAL ACCOUNT</asp:ListItem>
                                                                                            <asp:ListItem>BUSINESS ACCOUNT</asp:ListItem>
                                                                                            <asp:ListItem>GROUP ACCOUNT</asp:ListItem>
                                                                                        </asp:DropDownList>
                                                                                        <asp:Label ID="Label5" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                    </td>
                                                                                </tr>

                                                                                <tr>
                                                                                    <td>
                                                                                        <hr />
                                                                                    </td>
                                                                                    <td>
                                                                                        <hr />
                                                                                    </td>
                                                                                    <td>
                                                                                        <hr />
                                                                                    </td>
                                                                                    <td>
                                                                                        <hr />
                                                                                    </td>
                                                                                </tr>

                                                                                <tr>
                                                                                    <td style="height: 23px" colspan="3" class="text-left">&nbsp;<strong style="font-weight: bold; color: black">Accounts Details</strong>
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td class="tbCell1  text-left">Account Number:<b class="boldStar">*</b></td>
                                                                                    <td class="tbCell2">
                                                                                        <asp:TextBox ID="txtAccountNo" runat="server" CssClass="tinyText" MaxLength="20" ReadOnly="True" Enabled="False"></asp:TextBox>
                                                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator9" runat="server" ControlToValidate="txtAccountNo" Display="Dynamic" CssClass="Validator" SetFocusOnError="True" ErrorMessage="*" Font-Bold="True">Enter Account No</asp:RequiredFieldValidator>
                                                                                    </td>

                                                                                    <td class="tbCell1 text-left">Account Name:<b class="boldStar">*</b></td>
                                                                                    <td class="tbCell2">
                                                                                        <asp:TextBox ID="txtAcountName" runat="server" OnTextChanged="txtAdress3_TextChanged" CssClass="tinyText" MaxLength="50" Enabled="False"></asp:TextBox>

                                                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator10" runat="server" ControlToValidate="txtAcountName" Display="Dynamic" CssClass="Validator" SetFocusOnError="True" ErrorMessage="*" Font-Bold="True">Enter Account Name</asp:RequiredFieldValidator>
                                                                                    </td>
                                                                                </tr>

                                                                                <tr>
                                                                                    <td class="tbCell1 text-left">Address 1:</td>
                                                                                    <td class="tbCell2">
                                                                                        <asp:TextBox ID="txtAddress1" runat="server" CssClass="tinyText" MaxLength="100" TextMode="MultiLine" Enabled="False"></asp:TextBox>
                                                                                        <asp:Label ID="lblAdress1" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                                                                    </td>
                                                                                    <td class="tbCell1 text-left">TIN No:</td>
                                                                                    <td class="tbCell2">
                                                                                        <asp:TextBox ID="txtTillNo" runat="server" CssClass="tinyText" MaxLength="20" Enabled="False"></asp:TextBox>

                                                                                        <asp:Label ID="lblTinNo" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>

                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td class="tbCell1 text-left">Address 2:</td>
                                                                                    <td class="tbCell2">
                                                                                        <asp:TextBox ID="txtAddress2" runat="server" CssClass="tinyText" MaxLength="100" TextMode="MultiLine" Enabled="False"></asp:TextBox>
                                                                                        <asp:Label ID="Label7" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                                                                    </td>

                                                                                    <td class="tbCell1text-left">Telephone:</td>
                                                                                    <td class="tbCell2">
                                                                                        <asp:TextBox ID="txtTelNo" runat="server" CssClass="tinyText" MaxLength="14" Enabled="False"></asp:TextBox>

                                                                                        <asp:Label ID="Label32" runat="server" ForeColor="Red"></asp:Label>

                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td class="tbCell1 text-left">Address 3:</td>
                                                                                    <td class="tbCell2">
                                                                                        <asp:TextBox ID="txtAddress3" runat="server" CssClass="tinyText" MaxLength="100" TextMode="MultiLine" Enabled="False"></asp:TextBox>
                                                                                        <asp:Label ID="Label67" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                                                                    </td>
                                                                                    <td class="tbCell1 text-left">Mobile Number:</td>
                                                                                    <td class="tbCell2">
                                                                                        <asp:TextBox ID="txtMobileNo" runat="server" CssClass="tinyText" MaxLength="14" Enabled="False"></asp:TextBox>

                                                                                        <asp:Label ID="Label33" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>

                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td class="tbCell1 text-left">Email:</td>
                                                                                    <td class="tbCell2">
                                                                                        <asp:TextBox ID="txtEmail" runat="server" CssClass="tinyText" MaxLength="60" Enabled="False"></asp:TextBox>

                                                                                        <asp:Label ID="lblEmail" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>

                                                                                    </td>
                                                                                                    <td class="tbCell1 text-left">RelationShip Officer:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtRelOfficer" runat="server" CssClass="tinyText" MaxLength="50" Enabled="false"></asp:TextBox>
                                                                                                    </td>
                                                                                </tr>

                                                                            </tbody>
                                                                        </table>


                                                                        <table class="tabMain">
                                                                            <tbody>
                                                                                <tr>
                                                                                    <td>
                                                                                   <cc1:TabContainer ID="TabContainer2" runat="server" ActiveTabIndex="0" CssClass="">

                                                                    <cc1:TabPanel runat="server" HeaderText="TabPanel1" ID="TabPanel3">
                                                                        <HeaderTemplate>
                                                                            Personal Details
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
                                                                                                        <asp:DropDownList ID="drpdPrefix" runat="server" CssClass="tinyText" Enabled="false">
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
                                                                                                        <asp:TextBox ID="txtFirstName" runat="server" CssClass="tinyText" MaxLength="25" Enabled="false"></asp:TextBox>

                                                                                                        <asp:Label ID="lblFname" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Middle Name:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtMiddleName" runat="server" CssClass="tinyText" MaxLength="25" Enabled="false"></asp:TextBox>

                                                                                                        <asp:Label ID="Label9" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Last Name:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtLastName" runat="server" CssClass="tinyText" MaxLength="25" Enabled="false"></asp:TextBox>

                                                                                                        <asp:Label ID="lblLname" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">National ID:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtIDNo" runat="server" CssClass="tinyText" MaxLength="10" Enabled="false"></asp:TextBox>

                                                                                                        <asp:Label ID="Label8" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Marital Status</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:DropDownList ID="drpMaritalStatus" runat="server" CssClass="tinyText" Enabled="false">
                                                                                                            <asp:ListItem></asp:ListItem>
                                                                                                            <asp:ListItem>SINGLE</asp:ListItem>
                                                                                                            <asp:ListItem>MARRIED</asp:ListItem>
                                                                                                            <asp:ListItem>DIVORCED</asp:ListItem>
                                                                                                            <asp:ListItem>WIDOW</asp:ListItem>
                                                                                                            <asp:ListItem>WIDOWER</asp:ListItem>
                                                                                                        </asp:DropDownList>
                                                                                                        <asp:Label ID="Label6" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Spouse Name:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtSpouseName" runat="server" CssClass="tinyText" MaxLength="50" Enabled="false"></asp:TextBox>
                                                                                                        <asp:Label ID="Label43" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Dependants:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtDepemdant" runat="server" CssClass="tinyText" MaxLength="5" Enabled="false"></asp:TextBox>
                                                                                                        <asp:Label ID="lblDependants" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Occupation:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtOccupation" runat="server" CssClass="tinyText" MaxLength="50" Enabled="false"></asp:TextBox>
                                                                                                        <asp:Label ID="Label72" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Region:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:DropDownList ID="drpRegion" runat="server" CssClass="tinyText" Enabled="false">
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
                                                                                                        <asp:DropDownList ID="drpDistrict" runat="server" CssClass="tinyText" Enabled="false">
                                                                                                        </asp:DropDownList>

                                                                                                        <asp:Label ID="Label17" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Area:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:DropDownList ID="drpArea" runat="server" CssClass="tinyText" Enabled="false">
                                                                                                        </asp:DropDownList>

                                                                                                        <asp:Label ID="Label18" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Residential Status:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtResidential" runat="server" CssClass="tinyText" MaxLength="50" Enabled="false"></asp:TextBox>

                                                                                                        <asp:Label ID="Label19" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Sex:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtSex" runat="server" CssClass="tinyText" MaxLength="50" Enabled="false"></asp:TextBox>

                                                                                                        <asp:Label ID="Label20" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Passport:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtPassport" runat="server" CssClass="tinyText" MaxLength="50" Enabled="false"></asp:TextBox>

                                                                                                        <asp:Label ID="Label21" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Nationality:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtNationality" runat="server" CssClass="tinyText" MaxLength="50" Enabled="false"></asp:TextBox>

                                                                                                        <asp:Label ID="Label22" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Date of Birth:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtDoB" runat="server" CssClass="tinyText" MaxLength="50" Enabled="false"></asp:TextBox>
                                                                                                        
                                                                                                        <asp:Label ID="Label23" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>

                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Language:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtLanguage" runat="server" CssClass="tinyText" MaxLength="50" Enabled="false"></asp:TextBox>

                                                                                                        <asp:Label ID="Label24" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>

                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Employer:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtEmployer" runat="server" CssClass="tinyText" MaxLength="50" Enabled="false"></asp:TextBox>
                                                                                                        <asp:Label ID="Label73" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                            </tbody>
                                                                                        </table>
                                                                                    </div>
                                                                                </div>
                                                                            </div>
                                                                        </ContentTemplate>
                                                                    </cc1:TabPanel>

                                                                    <cc1:TabPanel runat="server" HeaderText="TabPanel2" ID="TabPanel4">
                                                                        <HeaderTemplate>
                                                                            Business Details
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
                                                                                                        <asp:TextBox ID="txtBizName" runat="server" CssClass="tinyText" MaxLength="50" Enabled="false"></asp:TextBox>

                                                                                                        <asp:Label ID="lblBizName" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Business Tel No:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtBizTel" runat="server" CssClass="tinyText" MaxLength="50" Enabled="false"></asp:TextBox>

                                                                                                        <asp:Label ID="Label26" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>

                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Registration Date:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtBizRegDate" runat="server" CssClass="tinyText" MaxLength="15" Enabled="false"></asp:TextBox>
                                                                         
                                                                                                        <asp:Label ID="Label10" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>

                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Registration No:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtRegNo" runat="server" CssClass="tinyText" Height="20px" MaxLength="10" Enabled="false"></asp:TextBox>

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
                                                                                                        <asp:TextBox ID="txtBizNature" runat="server" CssClass="tinyText" MaxLength="50" Enabled="false"></asp:TextBox>

                                                                                                        <asp:Label ID="Label25" runat="server" ForeColor="Red" Width="70px"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Director Name:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtDirName" runat="server" CssClass="tinyText" MaxLength="50" Enabled="false"></asp:TextBox>

                                                                                                        <asp:Label ID="Label13" runat="server" ForeColor="Red" Width="70px"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                               
                                                                                            </tbody>
                                                                                        </table>
                                                                                    </div>
                                                                                </div>
                                                                            </div>
                                                                        </ContentTemplate>
                                                                    </cc1:TabPanel>

                                                                    <cc1:TabPanel runat="server" HeaderText="TabPanel3" ID="TabPanel5">
                                                                        <HeaderTemplate>
                                                                            Group Details
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
                                                                                                        <asp:TextBox ID="txtgrpName" runat="server" CssClass="tinyText" MaxLength="50" Enabled="false"></asp:TextBox>

                                                                                                        <asp:Label ID="lblGroupName" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Number of Members:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtgrpMemberNo" runat="server" CssClass="tinyText" MaxLength="5" Enabled="false"></asp:TextBox>

                                                                                                        <asp:Label ID="lblMembers" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Number of Male:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtMaleNo" runat="server" CssClass="tinyText" MaxLength="5" Enabled="false"></asp:TextBox>

                                                                                                        <asp:Label ID="lblMaleNo" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Number of Female:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtFemaleNo" runat="server" CssClass="tinyText" MaxLength="5" Enabled="false"></asp:TextBox>

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
                                                                                                        <asp:TextBox ID="txtCurrency" runat="server" CssClass="tinyText" MaxLength="4" Enabled="False"></asp:TextBox>
                                                                                                    </td>
                                                                                                </tr>


                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Sector:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:DropDownList ID="drpSector" runat="server" CssClass="tinyText" Enabled="false">
                                                                                                        </asp:DropDownList>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Sub Sector:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:DropDownList ID="drpSubSector" runat="server" CssClass="tinyText" Enabled="false">
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
                                    </ContentTemplate>
                                </cc1:TabPanel>


                            </cc1:TabContainer>

                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td class="tbCell1 text-left">Reason for Edit:</td>
                                                                                    <td class="tbCell2">
                                                                                        <asp:TextBox ID="txtEditReason" runat="server" CssClass="tinyText" TextMode="MultiLine"></asp:TextBox>
                                                                                    </td>
                                                                                    <td class="tbCell1 text-left">
                                                                                        <asp:Label ID="Label12" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                    </td>
                                                                                </tr>
                                                                                <tr>
                                                                                    <td class="tbCell1 text-left">Submit:</td>
                                                                                    <td class="tbCell2">
                                                                                        <asp:RadioButton ID="radioApprove" runat="server" />
                                                                                    </td>
                                                                                    <td class="tbCell1 text-left">
                                                                                        <asp:Label ID="Label29" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                    </td>
                                                                                </tr>
                                                                            </tbody>
                                                                        </table>






                                                                        <table class="tbMain">
                                                                            <tbody>
                                                                                <tr>
                                                                                    <td class="tbCell1">
                                                                                        <asp:Button ID="Button2" OnClick="Button6_Click" runat="server" Text="Previous" CssClass="btn-Save" Visible="False"></asp:Button>

                                                                                    </td>
                                                                                    <td class="tbCell1">
                                                                                        <asp:Button ID="btnSave" OnClick="btnSave_Click" runat="server" Text="Save" CssClass="btn-Save"></asp:Button>

                                                                                    </td>
                                                                                    <td class="tbCell1">
                                                                                        <asp:Button ID="btnIndDelete" OnClick="btnIndDelete_Click1" runat="server" Text="Delete" CssClass="btn-Save" Visible="False"></asp:Button>

                                                                                    </td>
                                                                                    <td class="tbCell1">
                                                                                        <asp:Button ID="Button12" OnClick="Button1_Click" runat="server" Text="View Accounts" CssClass="btn-Save" Visible="False"></asp:Button>

                                                                                    </td>
                                                                                </tr>
                                                                            </tbody>
                                                                        </table>
                                                                        <br />
                                                                        <br />
                                                                        <br />
                                                                    </asp:View>
                                                                </asp:MultiView>


                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td></td>
                                                            <td></td>
                                                            <td style="width: 240px">&nbsp; </td>
                                                        </tr>
                                                        <tr>
                                                            <td colspan="3"></td>
                                                        </tr>
                                                        <tr>
                                                            <td colspan="3">
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
                                                            </td>
                                                        </tr>
                                                    </tbody>
                                                </table>
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
