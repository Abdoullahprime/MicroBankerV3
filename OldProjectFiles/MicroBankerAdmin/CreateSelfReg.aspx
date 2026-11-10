<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="CreateSelfReg.aspx.vb" Inherits="MicroBankerAdmin.CreateSelfReg" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>


            <table  class="tbMain">
                <tbody>
                    <tr>
                        <td >
                            <asp:Button ID="CmdApprovalList" OnClick="CmdApprovalList_Click" runat="server" Text="New Account" CausesValidation="False" CssClass="btn-New"></asp:Button></td>
                    </tr>
                    <tr>
                        <td colspan="3">
                            <cc1:TabContainer ID="TabContainer1" runat="server" ActiveTabIndex="0">
                                <cc1:TabPanel runat="server" HeaderText="TabPanel1" ID="TabPanel1">
                                    <HeaderTemplate>
                                        Create Account
                                    </HeaderTemplate>
                                    <ContentTemplate>

                                        <table class="tbMain">
                                            <tbody>

                                                <tr>
                                                    <td class="CriteriaCell" colspan="6">
                                                        <asp:Label ID="Label4" runat="server" Text="Fields marked with * are mandatory " CssClass="Criteria" ForeColor="Red"></asp:Label><br />
                                                    </td>
                                                </tr>
                                                <tr>
                                                                 <td class="CriteriaCell" colspan="9">
                                                                    <asp:Label ID="lblSuccess" runat="server" ForeColor="Red"></asp:Label>
                                                                </td>
                                                            </tr>
                                                <tr>
                                                    <td class="tbCell1  text-left" colspan="3">
                                                        <asp:Label ID="lblCheckdetails" runat="server" ForeColor="Red"></asp:Label>
                                                    </td>
                                                    </tr>
                                                    <tr>
                                                        <td>Branch Code:<b class="boldStar">*</b>&nbsp; </td>
                                                        <td class="tbCell2">
                                                            <asp:TextBox ID="txtBranchCode" runat="server" CssClass="ShortText" MaxLength="3" OnTextChanged="txtAccountCode_TextChanged"></asp:TextBox>
                                                            <asp:Button ID="btnCheckBranch" runat="server" CausesValidation="False" CssClass="btn-Check" OnClick="btnCheckBranch_Click" Text="Check" />
                                                        </td>
                                                        <td class="tbCell1">Branch Name:</td>
                                                        <td class="tbCell2">
                                                            <asp:TextBox ID="txtBranchName" runat="server" CssClass="tinyText"  MaxLength="20"></asp:TextBox>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="tbCell1 text-left">Account Type Code:<b class="boldStar">*</b></td>
                                                        <td class="tbCell2">
                                                            <asp:TextBox ID="txtAccountCode" runat="server" CssClass="ShortText" MaxLength="4" OnTextChanged="txtAccountCode_TextChanged"></asp:TextBox>
                                                            <asp:Button ID="btnSearchCode" runat="server" CausesValidation="False" CssClass="btn-Check" OnClick="btnSearchCode_Click" Text="Check" />
                                                        </td>
                                                        <td class="tbCell1 text-left">Product Name:</td>
                                                        <td class="tbCell2">
                                                            <asp:TextBox ID="txtProductCode" runat="server" MaxLength="3" OnTextChanged="txtEmail_TextChanged" Visible="False" Width="16px"></asp:TextBox>
                                                            <asp:TextBox ID="txtProductName" runat="server" CssClass="tinyText"  MaxLength="20"></asp:TextBox>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="tbCell1 text-left">Customer No:<b class="boldStar">*</b></td>
                                                        <td class="tbCell2">
                                                            <asp:TextBox ID="txtCustomerNo" runat="server" CssClass="ShortText" MaxLength="6" OnTextChanged="txtAccountCode_TextChanged"></asp:TextBox>
                                                            <asp:Button ID="cmasearchCust" runat="server" CausesValidation="False" CssClass="btn-Check" OnClick="cmasearchCust_Click" Text="Check" />
                                                        </td>

                                                        <td class="tbCell1 text-left">Select Account Type:</td>
                                                        <td class="tbCell2">
                                                            <asp:DropDownList ID="drpAccountType" runat="server" CssClass="tinyText">
                                                                <asp:ListItem></asp:ListItem>
                                                                <asp:ListItem>INDIVIDUAL ACCOUNT</asp:ListItem>
                                                                <asp:ListItem>BUSINESS ACCOUNT</asp:ListItem>
                                                                <asp:ListItem>GROUP ACCOUNT</asp:ListItem>
                                                            </asp:DropDownList>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="text-left" colspan="3" style="height: 23px">&nbsp;<strong style="font-weight: bold; color: black">Accounts Details</strong> </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="tbCell1  text-left">Account Number:<b class="boldStar">*</b></td>
                                                        <td class="tbCell2">
                                                            <asp:TextBox ID="txtAccountNo" runat="server" CssClass="tinyText" MaxLength="20" ReadOnly="True"></asp:TextBox>
                                                        </td>
                                                        <td class="tbCell1 text-left">Account Name:<b class="boldStar">*</b></td>
                                                        <td class="tbCell2">
                                                            <asp:TextBox ID="txtAcountName" runat="server" CssClass="tinyText" MaxLength="50" OnTextChanged="txtAdress3_TextChanged"></asp:TextBox>
                                                        </td>
                                                    </tr>
                                        <tr>
                                                            <td class="tbCell1 text-left">Address 1:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtAddress1" runat="server" CssClass="tinyText" MaxLength="100" TextMode="MultiLine" ></asp:TextBox>
                                                                <asp:Label ID="lblAdress1" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                                            </td>
                                                            <td class="tbCell1 text-left">TIN No:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtTillNo" runat="server" CssClass="tinyText" MaxLength="20" ></asp:TextBox>

                                                                <asp:Label ID="lblTinNo" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>

                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1 text-left">Address 2:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtAddress2" runat="server" CssClass="tinyText" MaxLength="100" TextMode="MultiLine" ></asp:TextBox>
                                                                <asp:Label ID="Label7" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                                            </td>

                                                            <td class="tbCell1text-left">Telephone:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtTelNo" runat="server" CssClass="tinyText" MaxLength="14" ></asp:TextBox>

                                                                <asp:Label ID="Label32" runat="server" ForeColor="Red"></asp:Label>

                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1 text-left">Address 3:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtAddress3" runat="server" CssClass="tinyText" MaxLength="100" TextMode="MultiLine" ></asp:TextBox>
                                                                <asp:Label ID="Label67" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                                            </td>
                                                            <td class="tbCell1 text-left">Mobile Number:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtMobileNo" runat="server" CssClass="tinyText" MaxLength="14" ></asp:TextBox>

                                                                <asp:Label ID="Label33" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>

                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1 text-left">Email:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtEmail" runat="server" CssClass="tinyText" MaxLength="60" ></asp:TextBox>

                                                                <asp:Label ID="lblEmail" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>

                                                            </td>
                                                               <td class="tbCell1 text-left">Employer:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtEmployer" runat="server" CssClass="tinyText" MaxLength="50" ></asp:TextBox>
                                                                                                        <asp:Label ID="Label73" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

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
                                                                                                        <asp:DropDownList ID="drpdPrefix" runat="server" CssClass="tinyText" >
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
                                                                                                        <asp:TextBox ID="txtFirstName" runat="server" CssClass="tinyText" MaxLength="25" ></asp:TextBox>

                                                                                                        <asp:Label ID="lblFname" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Middle Name:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtMiddleName" runat="server" CssClass="tinyText" MaxLength="25" ></asp:TextBox>

                                                                                                        <asp:Label ID="Label9" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Last Name:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtLastName" runat="server" CssClass="tinyText" MaxLength="25" ></asp:TextBox>

                                                                                                        <asp:Label ID="lblLname" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">National ID:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtIDNo" runat="server" CssClass="tinyText" MaxLength="10" ></asp:TextBox>

                                                                                                        <asp:Label ID="Label8" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Marital Status</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:DropDownList ID="drpMaritalStatus" runat="server" CssClass="tinyText" >
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
                                                                                                        <asp:TextBox ID="txtSpouseName" runat="server" CssClass="tinyText" MaxLength="50" ></asp:TextBox>
                                                                                                        <asp:Label ID="Label43" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Dependants:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtDepemdant" runat="server" CssClass="tinyText" MaxLength="5" ></asp:TextBox>
                                                                                                        <asp:Label ID="lblDependants" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Occupation:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtOccupation" runat="server" CssClass="tinyText" MaxLength="50" ></asp:TextBox>
                                                                                                        <asp:Label ID="Label72" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Region:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtRegion" runat="server" CssClass="tinyText" MaxLength="50" ></asp:TextBox>

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
                                                                                                        <asp:TextBox ID="txtDistrict" runat="server" CssClass="tinyText" MaxLength="50" ></asp:TextBox>

                                                                                                        <asp:Label ID="Label17" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Area:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtArea" runat="server" CssClass="tinyText" MaxLength="50" ></asp:TextBox>

                                                                                                        <asp:Label ID="Label18" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Residential Status:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtResidential" runat="server" CssClass="tinyText" MaxLength="50" ></asp:TextBox>

                                                                                                        <asp:Label ID="Label19" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Sex:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtSex" runat="server" CssClass="tinyText" MaxLength="50" ></asp:TextBox>

                                                                                                        <asp:Label ID="Label20" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Passport:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtPassport" runat="server" CssClass="tinyText" MaxLength="50" ></asp:TextBox>

                                                                                                        <asp:Label ID="Label21" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Nationality:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtNationality" runat="server" CssClass="tinyText" MaxLength="50" ></asp:TextBox>

                                                                                                        <asp:Label ID="Label22" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Date of Birth:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtDoB" runat="server" CssClass="tinyText" MaxLength="50"></asp:TextBox>
                                                                                                        <asp:ImageButton ID="btncalendars" runat="server" Style="width: 23px; height: 23px; padding-top: 5px;" ImageUrl="~/Img/Calendar_scheduleHS.png" />

                                                                                                        <asp:Label ID="Label23" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>

                                                                                                <tr>
                                                                                                    <td>
                                                                                                        <asp:Calendar ID="DOBDatePicker" runat="server" BackColor="White" CssClass="myCalendar" Visible="false">
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
                                                                                                        <asp:TextBox ID="txtLanguage" runat="server" CssClass="tinyText" MaxLength="50" ></asp:TextBox>

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
                                                                                                        <asp:TextBox ID="txtBizName" runat="server" CssClass="tinyText" MaxLength="50" ></asp:TextBox>

                                                                                                        <asp:Label ID="lblBizName" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Business Tel No:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtBizTel" runat="server" CssClass="tinyText" MaxLength="50" ></asp:TextBox>

                                                                                                        <asp:Label ID="Label26" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>

                                                                                                    </td>
                                                                                                </tr>

                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Registration Date:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtBizRegDate" runat="server" CssClass="tinyText" MaxLength="15"></asp:TextBox>
                                                                                                        <asp:ImageButton ID="btncalendar" runat="server" Style="width: 23px; height: 23px; padding-top: 5px;" ImageUrl="~/Img/Calendar_scheduleHS.png" />

                                                                                                        <asp:Label ID="Label10" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
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
                                                                                                        <asp:TextBox ID="txtRegNo" runat="server" CssClass="tinyText" Height="20px" MaxLength="10" ></asp:TextBox>

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
                                                                                                        <asp:TextBox ID="txtBizNature" runat="server" CssClass="tinyText" MaxLength="50" ></asp:TextBox>

                                                                                                        <asp:Label ID="Label25" runat="server" ForeColor="Red" Width="70px"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Director Name:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtDirName" runat="server" CssClass="tinyText" MaxLength="50" ></asp:TextBox>

                                                                                                        <asp:Label ID="Label13" runat="server" ForeColor="Red" Width="70px"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">RelationShip Officer:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtRelOfficer" runat="server" CssClass="tinyText" MaxLength="50" ></asp:TextBox>
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
                                                                                                        <asp:TextBox ID="txtgrpName" runat="server" CssClass="tinyText" MaxLength="50" ></asp:TextBox>

                                                                                                        <asp:Label ID="lblGroupName" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Number of Members:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtgrpMemberNo" runat="server" CssClass="tinyText" MaxLength="5" ></asp:TextBox>

                                                                                                        <asp:Label ID="lblMembers" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Number of Male:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtMaleNo" runat="server" CssClass="tinyText" MaxLength="5" ></asp:TextBox>

                                                                                                        <asp:Label ID="lblMaleNo" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                                                                    </td>
                                                                                                </tr>
                                                                                                <tr>
                                                                                                    <td class="tbCell1 text-left">Number of Female:</td>
                                                                                                    <td class="tbCell2">
                                                                                                        <asp:TextBox ID="txtFemaleNo" runat="server" CssClass="tinyText" MaxLength="5" ></asp:TextBox>

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
                                                                                                        <asp:TextBox ID="txtCurrency" runat="server" CssClass="tinyText" MaxLength="4" ></asp:TextBox>
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
                                                        <asp:Button ID="btnSave" runat="server" CssClass="btn-Save" OnClick="btnSave_Click" Text="Save" />
                                                    </td>
                                                    <td class="tbCell1">
                                                        <asp:Button ID="btnClear" runat="server" CausesValidation="False" CssClass="btn-Save" OnClick="btnClear_Click" Text="Clear" />
                                                    </td>
                                                    <td class="tbCell1">
                                                        <asp:Button ID="cmddelete" runat="server" CssClass="btn-Cancel" OnClick="cmddelete_Click" Text="Delete"  />
                                                    </td>
                                                    <td class="tbCell1  text-left">&nbsp;</td>
                                                    <td class="tbCell2">
                                                        <asp:HiddenField ID="HiddenField1" runat="server" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="tbCell1  text-left"></td>
                                                    <td class="tbCell2">
                                                        <cc1:ConfirmButtonExtender ID="ConfirmButtonExtender1" runat="server" ConfirmText="Are you sure you want to save?" Enabled="True" TargetControlID="btnSave">
                                                        </cc1:ConfirmButtonExtender>
                                                        <cc1:ConfirmButtonExtender ID="ConfirmButtonExtender2" runat="server" ConfirmText="Are you sure you want to Delete?" Enabled="True" TargetControlID="cmddelete">
                                                        </cc1:ConfirmButtonExtender>
                                                    </td>
                                                </tr>
                                            </tbody>

                                        </table>



                                        <table class="tbMain">
                                            <tbody>
                                                <tr>
                                                    <td class="tbCell1"></td>
                                                    <td class="tbCell2">
                                                        <asp:UpdateProgress ID="UpdateProgress1" runat="server" Visible="False">
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
                                <cc1:TabPanel runat="server" HeaderText="TabPanel2" ID="TabPanel2">
                                    <HeaderTemplate>
                                        Edit Account
                                    </HeaderTemplate>
                                    <ContentTemplate>
                                        <div class="searchHolder">
                                            <asp:RadioButtonList ID="RdoSearch" runat="server" OnSelectedIndexChanged="RdoSearch_SelectedIndexChanged" RepeatDirection="Horizontal">
                                                <asp:ListItem Value="Account">Account No</asp:ListItem>
                                                <asp:ListItem Value="Names">Account Name</asp:ListItem>
                                            </asp:RadioButtonList>
                                        </div>

                                        <div class="searchHolder">
                                            <div class="row">
                                                <div class="col-md-6">
                                                    <asp:TextBox ID="txtSearch" runat="server"  OnTextChanged="txtSearch_TextChanged" CssClass="tinyText"></asp:TextBox>
                                                </div>
                                                <div class="col-md-3">
                                                    <asp:Button ID="CmdSearch" OnClick="CmdSearch_Click" runat="server"  Text="Search" CausesValidation="False" CssClass="btn-Save"></asp:Button>
                                                </div>
                                                <div class="col-md-3">
                                                    <asp:Button ID="CmdViewAll" OnClick="CmdViewAll_Click" runat="server"  Text="View All" CausesValidation="False" CssClass="btn-Save"></asp:Button>
                                                </div>
                                            </div>
                                        </div>


                                        <div class="gridHolder">
                                            <asp:GridView ID="GridView1" runat="server" OnSelectedIndexChanged="GridView1_SelectedIndexChanged" EnableTheming="True" CellPadding="4" AutoGenerateColumns="False" AllowPaging="True"
                                                BackColor="White" BorderColor="white" BorderStyle="Solid" BorderWidth="3px" CssClass="GridView table-striped" ForeColor="Black" GridLines="Vertical">
                                                <AlternatingRowStyle BackColor="White"></AlternatingRowStyle>
                                                <Columns>
                                                    <asp:BoundField DataField="AccountNo" HeaderText="ACCOUNT NO"></asp:BoundField>
                                                    <asp:BoundField DataField="AccountName" HeaderText="ACCOUNT NAME"></asp:BoundField>
                                                    <asp:BoundField DataField="BranchName" HeaderText="BRANCH NAME"></asp:BoundField>
                                                    <asp:BoundField DataField="C_Address1" HeaderText="ADDRESS 1"></asp:BoundField>
                                                    <asp:TemplateField>
                                                        <ItemTemplate>
                                                            <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandArgument='<%# Eval("AccountNo") %>' CommandName="edit">Edit</asp:LinkButton>
                                                        </ItemTemplate>
                                                    </asp:TemplateField>
                                                </Columns>

                                                <EditRowStyle BackColor="#7C6F57"></EditRowStyle>

                                                <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White"></FooterStyle>

                                                <HeaderStyle BackColor="#0f3b5a" Font-Bold="True" ForeColor="White"></HeaderStyle>

                                                <PagerStyle HorizontalAlign="Center" BackColor="#666666" ForeColor="White"></PagerStyle>

                                                <RowStyle BackColor="#E3EAEB"></RowStyle>

                                                <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333"></SelectedRowStyle>
                                            </asp:GridView>
                                            <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:ConnectionString %>" ProviderName="<%$ ConnectionStrings:ConnectionString.ProviderName %>" SelectCommand="select OrgCode,OrgName,Address1,Address2,Address3,PhoneNo,Website,Email,LocalCurrency,CurrencyCode from Organisation"></asp:SqlDataSource>
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
