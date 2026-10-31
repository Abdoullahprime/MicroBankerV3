<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="CreateCustomer.aspx.vb" Inherits="MicroBankerAdmin.CreateCustomer" %>

<%--<%@ Register Assembly="FUA" Namespace="Subgurim.Controles" TagPrefix="cc2" %>--%>
<%--<%@ Register Assembly="FUA" Namespace="Subgurim.Controles" TagPrefix="cc2" %>--%>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <table class="tbMain">
        <tbody>
            <tr>
                <td>
                    <asp:Button ID="CmdApprovalList" OnClick="CmdApprovalList_Click" runat="server" Text="New Customer" CausesValidation="False" CssClass="btn-New"></asp:Button></td>
            </tr>
        </tbody>
    </table>

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <div class="formHolder">
                <div class="tbMain">
                    <div class="row">
                        <div class="col-md-6">
                            <table class="tbMain">
                                <tbody>
                                    <tr>
                                        <td class="tbCell1">Select Customer Type:<b class="boldStar">*</b> </td>
                                        <td class="tbCell2">
                                            <asp:DropDownList ID="drpAccountType" runat="server" CssClass="tinyText">
                                                <asp:ListItem></asp:ListItem>
                                                <asp:ListItem Value="1">INDIVIDUAL ACCOUNT</asp:ListItem>
                                                <asp:ListItem Value="2">BUSINESS ACCOUNT</asp:ListItem>
                                                <asp:ListItem Value="3">GROUP ACCOUNT</asp:ListItem>
                                            </asp:DropDownList>
                                            <asp:Label ID="lblAccountType" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="tbCell1">Customer ID:<b class="boldStar">*</b> </td>
                                        <td class="tbCell2">
                                            <asp:TextBox ID="txtCustID" runat="server" CssClass="ShortText" MaxLength="6"></asp:TextBox>
                                            <asp:Label ID="lblCustIDs" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                            <asp:Button ID="btnCustNo" runat="server" Text="Auto" CssClass="btn-Check" />
                                            <asp:Button ID="btnManual" runat="server" Text="Manual" CssClass="btn-Check" />

                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="tbCell1">Full Name:<b class="boldStar">*</b> </td>
                                        <td class="tbCell2">
                                            <asp:TextBox ID="txtFullName" runat="server" CssClass="tinyText"></asp:TextBox>
                                            <asp:Label ID="lblFullName" runat="server" ForeColor="Red"></asp:Label>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="tbCell1">Address 1:<b class="boldStar">*</b> </td>
                                        <td class="tbCell2">
                                            <asp:TextBox ID="txtAddress1" runat="server" CssClass="tinyText"></asp:TextBox>

                                            <asp:Label ID="lblAdress1" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="tbCell1">Address 2:</td>
                                        <td class="tbCell2">
                                            <asp:TextBox ID="txtAddress2" runat="server" CssClass="tinyText"></asp:TextBox>
                                            <asp:Label ID="Label1" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                        </td>

                                    </tr>
                                    <tr>
                                        <td class="tbCell1">Address 3:</td>
                                        <td class="tbCell2">
                                            <asp:TextBox ID="txtAddress3" runat="server" CssClass="tinyText"></asp:TextBox>
                                            <asp:Label ID="Label67" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                        <div class="col-md-6">
                            <table class="tbMain">
                                <tbody>
                                    <tr>
                                        <td class="tbCell1">Region:<b class="boldStar">*</b> </td>
                                        <td class="tbCell2">
                                            <asp:DropDownList ID="drpdRegion" runat="server" CssClass="tinyText">
                                            </asp:DropDownList>
                                            <asp:Label ID="lblRegion" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="tbCell1">District:<b class="boldStar">*</b> </td>
                                        <td class="tbCell2">
                                            <asp:DropDownList ID="drpdDistrict" runat="server" CssClass="tinyText">
                                            </asp:DropDownList>
                                            <asp:Label ID="lblDistrict" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                        </td>

                                    </tr>
                                    <tr>
                                        <td class="tbCell1">Area:<b class="boldStar">*</b> </td>
                                        <td class="tbCell2">
                                            <asp:DropDownList ID="drpdArea" runat="server" CssClass="tinyText">
                                            </asp:DropDownList>
                                            <asp:Label ID="lblArea" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="tbCell1">Telephone:</td>
                                        <td class="tbCell2">
                                            <asp:TextBox ID="txtTelNo" runat="server" CssClass="tinyText"></asp:TextBox>
                                            <asp:Label ID="Label32" runat="server" ForeColor="Red"></asp:Label>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="tbCell1">Mobile Number:</td>
                                        <td class="tbCell2">
                                            <asp:TextBox ID="txtMobileNo" runat="server" CssClass="tinyText"></asp:TextBox>
                                            <asp:Label ID="Label33" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="tbCell1">Email:</td>
                                        <td class="tbCell2">
                                            <asp:TextBox ID="txtEmail" runat="server" CssClass="tinyText" MaxLength="60"></asp:TextBox>
                                            <asp:Label ID="lblEmail" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="tbCell1">TIN No:<b class="boldStar">*</b> </td>
                                        <td class="tbCell2">
                                            <asp:TextBox ID="txtTillNo" runat="server" CssClass="tinyText"></asp:TextBox>
                                            <asp:Label ID="lblTinNo" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </div>
                    <div class="row" style="padding: 0 10px">
                        <cc1:TabContainer ID="TabContainer1" runat="server" ActiveTabIndex="0">
                            <cc1:TabPanel runat="server" HeaderText="TabPanel1" ID="TabPanel1">
                                <HeaderTemplate>
                                    Individual Customer Registration
                                       
                                </HeaderTemplate>
                                <ContentTemplate>
                                    <div class="tbMain">
                                        <div class="row">
                                            <div class="col-md-6">
                                                <table class="tbMain">
                                                    <tbody>
                                                        <tr>
                                                            <td class="tbCell1">Title:<b class="boldStar">*</b> </td>
                                                            <td class="tbCell2">
                                                                <asp:DropDownList ID="drpdPrefix" runat="server" CssClass="tinyText">
                                                                    <asp:ListItem></asp:ListItem>
                                                                    <asp:ListItem Value="Mr">MR</asp:ListItem>
                                                                    <asp:ListItem Value="Mrs">MRS</asp:ListItem>
                                                                    <asp:ListItem Value="Miss">MISS</asp:ListItem>
                                                                    <asp:ListItem Value="Dr">DR.</asp:ListItem>
                                                                </asp:DropDownList>
                                                                <asp:Label ID="lblPrefix" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                            </td>
                                                        </tr>
                                                        <tr>

                                                            <td class="tbCell1">First Name:<b class="boldStar">*</b> </td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtFirstName" runat="server" CssClass="tinyText"></asp:TextBox>
                                                                <asp:Label ID="lblFname" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1">Middle Name:<b class="boldStar">*</b> </td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtMiddleName" runat="server" CssClass="tinyText" MaxLength="25"></asp:TextBox>
                                                                <asp:Label ID="Label9" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1">Last Name:<b class="boldStar">*</b> </td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtLastName" runat="server" CssClass="tinyText"></asp:TextBox>
                                                                <asp:Label ID="lblLname" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1">National ID:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtIDNo" runat="server" CssClass="tinyText"></asp:TextBox>
                                                                <asp:Label ID="Label2" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                            </td>

                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1">Passport NO:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtPassNo" runat="server" CssClass="tinyText"></asp:TextBox>
                                                                <asp:Label ID="Label5" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1">RelationShip Officer:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtRelOfficer" runat="server" CssClass="tinyText"></asp:TextBox>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1">Nationality:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtNationality" runat="server" CssClass="tinyText"></asp:TextBox>

                                                                <asp:Label ID="Label38" runat="server" ForeColor="Red" Width="70px"></asp:Label>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1">DoB:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtDoB" runat="server" CssClass="tinyText"></asp:TextBox>
                                                                <asp:Image ID="Image1" runat="server" ImageUrl="~/Img/Calendar_scheduleHS.png" />
                                                                <asp:Label ID="Label22" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                            </td>
                                                        </tr>
                                                    </tbody>
                                                </table>
                                            </div>
                                            <div class="col-md-6">
                                                <table class="tbMain">
                                                    <tbody>
                                                        <tr>
                                                            <td class="tbCell1">Resident Status:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtResStatus" runat="server" CssClass="tinyText"></asp:TextBox>
                                                                <asp:Label ID="Label71" runat="server" ForeColor="Red" Width="70px"></asp:Label>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1">Language:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtLang" runat="server" CssClass="tinyText"></asp:TextBox>

                                                                <asp:Label ID="Label40" runat="server" ForeColor="Red"></asp:Label>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1">Gender:<b class="boldStar">*</b> </td>
                                                            <td class="tbCell2">
                                                                <asp:DropDownList ID="drpGender" runat="server" CssClass="tinyText">
                                                                    <asp:ListItem></asp:ListItem>
                                                                    <asp:ListItem>MALE</asp:ListItem>
                                                                    <asp:ListItem>FEMALE</asp:ListItem>
                                                                </asp:DropDownList>
                                                                <asp:Label ID="lblGender" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1">Marital Status:</td>
                                                            <td class="tbCell2">
                                                                <asp:DropDownList ID="drpMaritalStatus" runat="server" CssClass="tinyText">
                                                                    <asp:ListItem></asp:ListItem>
                                                                    <asp:ListItem>SINGLE</asp:ListItem>
                                                                    <asp:ListItem>MARRIED</asp:ListItem>
                                                                    <asp:ListItem>DIVORCED</asp:ListItem>
                                                                    <asp:ListItem>WIDOW</asp:ListItem>
                                                                    <asp:ListItem>WIDOWER</asp:ListItem>
                                                                </asp:DropDownList>
                                                                <asp:Label ID="Label42" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1">Spouse Name:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtSpouseName" runat="server" CssClass="tinyText"></asp:TextBox>

                                                                <asp:Label ID="Label43" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1">Dependants:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtDepemdant" runat="server" CssClass="tinyText">0</asp:TextBox>

                                                                <asp:Label ID="lblDependants" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1">Occupation:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtOccupation" runat="server" CssClass="tinyText"></asp:TextBox>

                                                                <asp:Label ID="Label72" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1">Employer:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtEmployer" runat="server" CssClass="tinyText"></asp:TextBox>

                                                                <asp:Label ID="Label73" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                            </td>
                                                        </tr>
                                                    </tbody>
                                                </table>
                                            </div>
                                        </div>
                                    </div>
                                    <asp:Label ID="lblSuccess" runat="server" ForeColor="Green"></asp:Label>
                                    <div class="searchHolder">
                                        <div class="row">
                                            <div class="col-md-6">
                                                <asp:Button ID="btnSaveIndividual" runat="server" CssClass="btn-Save" OnClick="btnSaveIndividual_Click" Text="Save" />
                                            </div>
                                            <div class="col-md-3">
                                                <asp:Button ID="btnIndClear" runat="server" CssClass="btn-Cancel" OnClick="btnIndClear_Click" Text="Clear" />
                                            </div>
                                        </div>
                                    </div>
                                </ContentTemplate>
                            </cc1:TabPanel>
                            <cc1:TabPanel runat="server" HeaderText="TabPanel2" ID="TabPanel2">
                                <HeaderTemplate>
                                    Business Registration
                                       
                                </HeaderTemplate>
                                <ContentTemplate>
                                    <div class="tbMain">
                                        <div class="row">
                                            <div class="col-md-6">
                                                <table class="tbMain">
                                                    <tbody>
                                                        <tr>
                                                            <td class="tbCell1">Business Name:<b class="boldStar">*</b> </td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtBizName" runat="server" CssClass="tinyText"></asp:TextBox>
                                                                <asp:Label ID="lblBizName" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1">Registration Date:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtBizRegDate" runat="server" CssClass="tinyText"></asp:TextBox>
                                                                <asp:Image ID="Image2" runat="server" ImageUrl="~/Img/Calendar_scheduleHS.png" />
                                                                <asp:Label ID="Label10" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1">Registration No:<b class="boldStar">*</b></td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtRegNo" runat="server" CssClass="tinyText" Height="20px"></asp:TextBox>



                                                                <asp:Label ID="lblRegNo" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                                            </td>
                                                        </tr>
                                                    </tbody>
                                                </table>
                                            </div>
                                            <div class="col-md-6">
                                                <table class="tbMain">
                                                    <tbody>
                                                        <tr>
                                                            <td class="tbCell1">Nature of business:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtBizNature" runat="server" CssClass="tinyText"></asp:TextBox>
                                                                <asp:Label ID="Label25" runat="server" ForeColor="Red" Width="70px"></asp:Label>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1">Director Name:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtDirName" runat="server" CssClass="tinyText"></asp:TextBox>
                                                                <asp:Label ID="Label13" runat="server" Width="70px" ForeColor="Red"></asp:Label>
                                                            </td>
                                                        </tr>
                                                    </tbody>
                                                </table>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="searchHolder">
                                        <div class="row">
                                            <div class="col-md-6">
                                                <asp:Button ID="btnSaveBiz" OnClick="btnSaveBiz_Click" runat="server" Text="Save" CssClass="btn-Save"></asp:Button>
                                            </div>
                                            <div class="col-md-3">
                                                <asp:Button ID="btnBizClear" OnClick="btnBizClear_Click" runat="server" Text="Clear" CssClass="btn-Cancel" Visible="True"></asp:Button>
                                            </div>
                                        </div>
                                    </div>
                                </ContentTemplate>
                            </cc1:TabPanel>
                            <cc1:TabPanel runat="server" HeaderText="TabPanel3" ID="TabPanel3">
                                <HeaderTemplate>
                                    Group Registration
                                </HeaderTemplate>
                                <ContentTemplate>
                                    <div class="tbMain">
                                        <div class="row">
                                            <div class="col-md-6">
                                                <table class="tbMain">
                                                    <tbody>
                                                        <tr>
                                                            <td class="tbCell1">Group Name:<b class="boldStar">*</b> </td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtgrpName" runat="server" CssClass="tinyText"></asp:TextBox>
                                                                <asp:Label ID="lblGroupName" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1">Registration Date:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtgrpRegDate" runat="server" CssClass="tinyText"></asp:TextBox><asp:Image ID="Image3" runat="server" ImageUrl="~/Img/Calendar_scheduleHS.png" /></td>
                                                            <td style="width: 131px; height: 26px;">
                                                                <asp:Label ID="Label26" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                                            </td>

                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1">Registration No:<b class="boldStar">*</b> </td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtgrpRegNo" runat="server" CssClass="tinyText" MaxLength="25" Height="20px"></asp:TextBox>
                                                                <asp:Label ID="lblgrpRegNo" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1">Nature of business:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtgrpBizNature" runat="server" CssClass="tinyText"></asp:TextBox>

                                                                <asp:Label ID="Label59" runat="server" ForeColor="Red" Width="70px"></asp:Label>

                                                            </td>
                                                        </tr>
                                                    </tbody>
                                                </table>
                                            </div>
                                            <div class="col-md-6">
                                                <table class="tbMain">
                                                    <tbody>
                                                        <tr>
                                                            <td class="tbCell1">Director Name:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtgrpDirName" runat="server" CssClass="tinyText"></asp:TextBox>
                                                                <asp:Label ID="Label61" runat="server" Width="84px" ForeColor="Red"></asp:Label>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1">Number of Members:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtgrpMemberNo" runat="server" CssClass="tinyText"></asp:TextBox>
                                                                <asp:Label ID="lblMembers" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1">Number of Male:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtMaleNo" runat="server" CssClass="tinyText" MaxLength="5"></asp:TextBox>
                                                                <asp:Label ID="lblMaleNo" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td class="tbCell1">Number of Female:</td>
                                                            <td class="tbCell2">
                                                                <asp:TextBox ID="txtFemaleNo" runat="server" CssClass="tinyText" MaxLength="5"></asp:TextBox>
                                                                <asp:Label ID="lblFemaleNo" runat="server" Font-Bold="True" ForeColor="Red"></asp:Label>
                                                            </td>
                                                        </tr>
                                                    </tbody>
                                                </table>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="searchHolder">
                                        <div class="row">
                                            <div class="col-md-6">
                                                <asp:Button ID="btnSaveGroup" runat="server" CssClass="btn-Save" OnClick="btnSaveGroup_Click" Text="Save" />
                                            </div>
                                            <div class="col-md-3">
                                                <asp:Button ID="btnGrpClear" runat="server" CssClass="btn-Cancel" OnClick="btnGrpClear_Click" Text="Clear" />
                                            </div>
                                        </div>
                                    </div>
                                </ContentTemplate>
                            </cc1:TabPanel>
                        </cc1:TabContainer>
                        <asp:UpdateProgress ID="UpdateProgress1" runat="server">
                            <ProgressTemplate>
                                <img src="Img/ajax-loader1.gif" /><asp:Label ID="Label3" runat="server" Text="Please Wait..." ForeColor="Red"></asp:Label>
                            </ProgressTemplate>
                        </asp:UpdateProgress>
                    </div>
                </div>
            </div>
            </div>
        </ContentTemplate>
    </asp:UpdatePanel>




</asp:Content>
