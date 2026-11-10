<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="DeleteCustomerDetails.aspx.vb" Inherits="MicroBankerAdmin.DeleteCustomerDetails" %>

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
                            <asp:Button ID="CmdApprovalList" OnClick="CmdApprovalList_Click" runat="server" Text="Delete Customer" CausesValidation="False" CssClass="btn-New"></asp:Button></td>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <cc1:TabContainer ID="TabContainer1" runat="server" ActiveTabIndex="0">
                                <cc1:TabPanel runat="server" HeaderText="TabPanel1" ID="TabPanel1">
                                    <HeaderTemplate>
                                        Delete Customer Details&nbsp;
                                    </HeaderTemplate>
                                    <ContentTemplate>
                                        <table class="tbMain">
                                            <tbody>
                                                <tr>
                                                    <td colspan="3">
                                                        <asp:MultiView ID="MultiView1" runat="server" OnActiveViewChanged="MultiView1_ActiveViewChanged">
                                                            <asp:View ID="View1" runat="server">

                                                                <div class="searchHolder">
                                                                    <asp:RadioButtonList ID="RdoSearch" runat="server" Width="440px" OnSelectedIndexChanged="RdoSearch_SelectedIndexChanged"
                                                                        RepeatDirection="Horizontal">
                                                                        <asp:ListItem>Customer No</asp:ListItem>
                                                                        <asp:ListItem>Customer Name</asp:ListItem>
                                                                    </asp:RadioButtonList>
                                                                </div>

                                                                <div class="searchHolder">
                                                                    <div class="row">
                                                                        <div class="col-md-6">
                                                                            <asp:TextBox ID="txtSearch" runat="server" OnTextChanged="txtSearch_TextChanged"
                                                                                CssClass="tinyText" ></asp:TextBox>
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
                                                                        BackColor="White" BorderColor="white" BorderStyle="Solid" BorderWidth="3px" CssClass="GridView table-striped" ForeColor="Black" GridLines="Vertical">
                                                                        <AlternatingRowStyle BackColor="White"></AlternatingRowStyle>
                                                                        <Columns>
                                                                            <asp:BoundField DataField="C_CustomerNo" HeaderText="Customer No"></asp:BoundField>
                                                                            <asp:BoundField DataField="C_CustomerType" HeaderText="Customer Type"></asp:BoundField>
                                                                            <asp:BoundField DataField="C_Telephone" HeaderText="Tel No"></asp:BoundField>
                                                                            <asp:BoundField DataField="C_FullName" HeaderText="Full Name"></asp:BoundField>
                                                                            <asp:BoundField DataField="C_MobileNo" HeaderText="MobileNo" SortExpression="CREATOR"></asp:BoundField>
                                                                            <asp:TemplateField>
                                                                                <ItemTemplate>
                                                                                    <asp:LinkButton ID="LinkButton1" runat="server" CommandName="select" CommandArgument='<%#Eval("C_CustomerNo") %>'>select</asp:LinkButton>
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
                                                                </div>

                                                            </asp:View>
                                                            <asp:View ID="View2" runat="server" EnableViewState="False">
                                                                <br />
                                                                <table class="tbMain">
                                                                    <tbody>
                                                                        <tr>
                                                                            <td style="height: 19px">
                                                                                <asp:Label ID="Label1" runat="server" Text="Customer Details" ForeColor="Red"></asp:Label>
                                                                            </td>
                                                                        </tr>

                                                                    </tbody>
                                                                </table>
                                                                <div class="tbMain">
                                                                    <div class="row">
                                                                        <div class="col-md-6">
                                                                            <table class="tbMain">
                                                                                <tbody>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Customer No:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtCustNo" runat="server" CssClass="tinyText" Enabled="false" ></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Account Type:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:DropDownList ID="drpAccountType" runat="server" CssClass="tinyText" Enabled="False">
                                                                                                <asp:ListItem></asp:ListItem>
                                                                                                <asp:ListItem>INDIVIDUAL ACCOUNT</asp:ListItem>
                                                                                                <asp:ListItem>BUSINESS ACCOUNT</asp:ListItem>
                                                                                                <asp:ListItem>GROUP ACCOUNT</asp:ListItem>
                                                                                            </asp:DropDownList>

                                                                                        </td>
                                                                                        <td></td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Full Name:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtFullName" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Address1:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtAddress1" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Address2:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtAddress2" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Address3:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtAddress3" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                </tbody>
                                                                            </table>
                                                                        </div>
                                                                        <div class="col-md-6">
                                                                            <table class="tbMain">
                                                                                <tbody>
                                                                    <tr>
                                                                        <td class="tbCell1">Region
                                                                        </td>
                                                                        <td class="tbCell2">
                                                                            <asp:DropDownList ID="drpRegion" runat="server" CssClass="tinyText" Enabled="false">
                                                                            </asp:DropDownList>

                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td class="tbCell1">District:
                                                                        </td>
                                                                        <td class="tbCell2">
                                                                            <asp:DropDownList ID="drpDistrict" runat="server" CssClass="tinyText" Enabled="false">
                                                                            </asp:DropDownList>
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td class="tbCell1">Area:
                                                                        </td>
                                                                        <td class="tbCell2">
                                                                            <asp:DropDownList ID="drpArea" runat="server" CssClass="tinyText" Enabled="false">
                                                                            </asp:DropDownList>
                                                                        </td>
                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Telephone:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtTel" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Mobile Number:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtMobileNo" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Email:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtEmail" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">TINNo:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtTINNo" runat="server" CssClass="tinyText"
                                                                                                MaxLength="10" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                </tbody>
                                                                            </table>
                                                                        </div>
                                                                    </div>

                                                                    <table class="tbMain">
                                                                        <tbody>

                                                                            <tr>
                                                                                <td class="tbCell1"><strong style="font-weight: bold; color: black">Other Details</strong>
                                                                                </td>
                                                                            </tr>
                                                                        </tbody>
                                                                    </table>
                                                                    <div class="row">
                                                                        <div class="col-md-6">
                                                                            <table class="tbMain">
                                                                                <tbody>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Title:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtPrefix" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">First Name:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtFirstName" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Middle Name:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtMiddleName" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Last Name:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtLastName" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Sex
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtSex" runat="server" CssClass="tinyText" MaxLength="100" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">DoB
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtDoB" runat="server" CssClass="tinyText" MaxLength="100" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">NationalID:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtNationalID" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">PassportNO:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtPassportNO" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                </tbody>
                                                                            </table>
                                                                        </div>
                                                                        <div class="col-md-6">
                                                                            <table class="tbMain">
                                                                                <tbody>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Resident Status
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtResidentStatus" runat="server" CssClass="tinyText" MaxLength="100" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Language
                                                                                        </td>
                                                                                        <td class="tbCell2">

                                                                                            <asp:TextBox ID="txtLanguage" runat="server" CssClass="tinyText" MaxLength="100" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Nationality :
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtNationality" runat="server" CssClass="tinyText" MaxLength="100" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">MaritalStatus
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtMaritalStatus" runat="server" CssClass="tinyText" MaxLength="100" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">SpouseName
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtSpouseName" runat="server" CssClass="tinyText" MaxLength="100" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Dependants
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtDependants" runat="server" CssClass="tinyText" MaxLength="100" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">EmployerName
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtEmployerName" runat="server" CssClass="tinyText" MaxLength="100" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Occupation
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtOccupation" runat="server" CssClass="tinyText" MaxLength="100" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">RelationShipOfficer:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtRelationShipOfficer" runat="server" CssClass="tinyText" MaxLength="100" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td>
                                                                                            <asp:Label ID="lblResponse" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                                                                        </td>
                                                                                    </tr>
                                                                                </tbody>
                                                                            </table>
                                                                        </div>
                                                                    </div>
                                                                </div>
                                                                <table class="tbMain">
                                                                    <tbody>
                                                                        <tr>
                                                                            <td class="tbCell1">
                                                                                <asp:Button ID="Button2" OnClick="Button6_Click" runat="server" Text="Previous"
                                                                                    CssClass="btn-Save" Visible="False"></asp:Button>
                                                                            </td>
                                                                            <td class="tbCell1">
                                                                                <asp:Button ID="btnSave" OnClick="btnSave_Click" runat="server" Text="Save" CssClass="btn-Save" Visible="False"></asp:Button>
                                                                            </td>
                                                                            <td class="tbCell1">
                                                                                <asp:Button ID="btnIndDelete" OnClick="btnIndDelete_Click1" runat="server" Text="Delete"
                                                                                    CssClass="btn-Save"></asp:Button>
                                                                            </td>
                                                                            <td class="tbCell1">
                                                                                <asp:Button ID="Button12" OnClick="Button1_Click" runat="server"
                                                                                    Text="View Accounts" CssClass="btn-Save" Visible="False"></asp:Button>
                                                                            </td>
                                                                        </tr>
                                                                    </tbody>
                                                                </table>
                                                            </asp:View>
                                                            <asp:View ID="View3" runat="server">
                                                                <table class="tbMain">
                                                                    <tbody>
                                                                        <tr>
                                                                            <td style="height: 19px">
                                                                                <asp:Label ID="Label2" runat="server" Text="Customer Details" ForeColor="Red"></asp:Label>
                                                                            </td>
                                                                        </tr>
                                                                    </tbody>
                                                                </table>
                                                                <div class="tbMain">
                                                                    <div class="row">
                                                                        <div class="col-md-6">
                                                                            <table class="tbMain">
                                                                                <tbody>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Customer No:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtBizCuztNo" runat="server" CssClass="tinyText" Enabled="false" ></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Account Type:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtBizAcTpype" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Full Name:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtBizFullName" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Address1:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtBizAddress1" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Address2:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtBizAdress2" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Address3:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtBizAddress3" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                </tbody>
                                                                            </table>
                                                                        </div>

                                                                        <div class="col-md-6">
                                                                            <table class="tbMain">
                                                                                <tbody>

                                                                                    <tr>
                                                                                        <td class="tbCell1">Region
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:DropDownList ID="drpBizRegion" runat="server" CssClass="tinyText " Enabled="false">
                                                                                            </asp:DropDownList>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">District:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:DropDownList ID="drpBizDistrict" runat="server" CssClass="tinyText" Enabled="false">
                                                                                            </asp:DropDownList>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Area:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:DropDownList ID="drpBizArea" runat="server" CssClass="tinyText" Enabled="false">
                                                                                            </asp:DropDownList>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Telephone:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtBizTel" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Mobile Number:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtBizMobile" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Email:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtBizEmail" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">TINNo:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtBizTinNo" runat="server" CssClass="tinyText"
                                                                                                MaxLength="10" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                </tbody>
                                                                            </table>
                                                                        </div>
                                                                    </div>

                                                                    <table class="tbMain">
                                                                        <tbody>
                                                                            <tr>
                                                                                <td class="tbCell1"><strong style="font-weight: bold; color: black">Other Details</strong>
                                                                                </td>
                                                                            </tr>
                                                                        </tbody>
                                                                    </table>

                                                                    <div class="row">
                                                                        <div class="col-md-6">
                                                                            <table class="tbMain">
                                                                                <tbody>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Business Name:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtBizName" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Registration Date:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtBizRegDate" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Registration Number:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtBizRegNo" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                </tbody>
                                                                            </table>
                                                                        </div>
                                                                        <div class="col-md-6">
                                                                            <table class="tbMain">
                                                                                <tbody>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Nature of Business:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtBizNature" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Director Name:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtBizDirectorName" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td>
                                                                                            <asp:Label ID="lblBizResponse" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                                                                        </td>
                                                                                    </tr>
                                                                                </tbody>
                                                                            </table>
                                                                        </div>
                                                                    </div>
                                                                </div>
                                                                <table class="tbMain">
                                                                    <tbody>
                                                                        <tr>
                                                                            <td class="tbCell1">
                                                                                <asp:Button ID="Button3" OnClick="Button6_Click" runat="server" Text="Previous"
                                                                                    CssClass="btn-Save" Visible="False"></asp:Button>
                                                                            </td>
                                                                            <td class="tbCell1">
                                                                                <asp:Button ID="btnEditBiz" OnClick="btnEditBiz_Click" runat="server" Text="Save" CssClass="btn-Save" Visible="False"></asp:Button>
                                                                            </td>
                                                                            <td class="tbCell1">
                                                                                <asp:Button ID="btnDeleteBiz" OnClick="btnDeleteBiz_Click1" runat="server" Text="Delete"
                                                                                    CssClass="btn-Cancel"></asp:Button>
                                                                            </td>
                                                                            <td class="tbCell1">
                                                                                <asp:Button ID="Button9" OnClick="Button1_Click" runat="server"
                                                                                    Text="View Accounts" CssClass="btn-Save" Visible="False"></asp:Button>
                                                                            </td>
                                                                        </tr>
                                                                    </tbody>
                                                                </table>
                                                            </asp:View>
                                                            <asp:View ID="View4" runat="server">
                                                                <br />
                                                                <table class="tbMain">
                                                                    <tbody>
                                                                        <tr>
                                                                            <td style="height: 19px">
                                                                                <asp:Label ID="Label6" runat="server" Text="Customer Details" ForeColor="Red"></asp:Label>
                                                                            </td>
                                                                        </tr>
                                                                    </tbody>
                                                                </table>
                                                                <div class="tbMain">
                                                                    <div class="row">
                                                                        <div class="col-md-6">
                                                                            <table class="tbMain">
                                                                                <tbody>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Customer No:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtGrpCustNo" runat="server" CssClass="tinyText" Enabled="false" ></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Account Type:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtGrpAcType" runat="server" CssClass="tinyText" Enabled="false" ></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Full Name:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtGrpFullName" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Address1:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtGrpAddress1" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Address2:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtGrpAddress2" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Address3:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtGrpAddress3" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                </tbody>
                                                                            </table>
                                                                        </div>
                                                                        <div class="col-md-6">
                                                                            <table class="tbMain">
                                                                                <tbody>
                                                                    <tr>
                                                                        <td class="tbCell1">Region
                                                                        </td>
                                                                        <td class="tbCell2">
                                                                            <asp:DropDownList ID="drpGrpRegion" runat="server" CssClass="tinyText" Enabled="false">
                                                                            </asp:DropDownList>
                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td class="tbCell1">District:
                                                                        </td>
                                                                        <td class="tbCell2">
                                                                            <asp:DropDownList ID="drpGrpDistrict" runat="server" CssClass="tinyText" Enabled="false">
                                                                            </asp:DropDownList>

                                                                        </td>
                                                                    </tr>
                                                                    <tr>
                                                                        <td class="tbCell1">Area:
                                                                        </td>
                                                                        <td class="tbCell2">
                                                                            <asp:DropDownList ID="DrpGrpArea" runat="server" CssClass="tinyText" Enabled="false">
                                                                            </asp:DropDownList>
                                                                        </td>
                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Telephone:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtGrpTel" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Mobile Number:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtGrpMobileNo" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Email:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtGrpEmail" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">TINNo:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtGrpTinNo" runat="server" CssClass="tinyText"
                                                                                                MaxLength="10" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                </tbody>
                                                                            </table>
                                                                        </div>
                                                                    </div>

                                                                    <table class="tbMain">
                                                                        <tbody>
                                                                            <tr>
                                                                                <td class="tbCell1" colspan="4"><strong style="font-weight: bold; color: black">Other Details</strong>
                                                                                </td>
                                                                            </tr>
                                                                        </tbody>
                                                                    </table>
                                                                    <div class="row">
                                                                        <div class="col-md-6">
                                                                            <table class="tbMain">
                                                                                <tbody>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Group Name:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtGrpName" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Registration Date:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtGrpregDate" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Registration Number:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtGrpRegNo" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Nature of Business:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtGrpBizNature" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                </tbody>
                                                                            </table>
                                                                        </div>
                                                                        <div class="col-md-6">
                                                                            <table class="tbMain">
                                                                                <tbody>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Director Name:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtGrpDirectorName" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Number of Members:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtGrpMemberNo" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Number of Males:
                                                                                        </td>
                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtGrpMaleNo" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>
                                                                                    <tr>
                                                                                        <td class="tbCell1">Number of Females:
                                                                                        </td>

                                                                                        <td class="tbCell2">
                                                                                            <asp:TextBox ID="txtGrpFemaleNo" runat="server" CssClass="tinyText" Enabled="false"></asp:TextBox>
                                                                                        </td>
                                                                                    </tr>

                                                                                    <tr>
                                                                                        <td>
                                                                                            <asp:Label ID="lblGrpResponse" runat="server" ForeColor="Red" Font-Bold="True"></asp:Label>
                                                                                        </td>
                                                                                    </tr>
                                                                                </tbody>
                                                                            </table>
                                                                        </div>
                                                                    </div>
                                                                </div>
                                                                <table class="tbMain">
                                                                    <tbody>
                                                                        <tr>
                                                                            <td class="tbCell1">
                                                                                <asp:Button ID="Button1" OnClick="Button6_Click" runat="server" Text="Previous"
                                                                                    CssClass="btn-Cancel" Visible="False"></asp:Button>
                                                                            </td>
                                                                            <td class="tbCell1">
                                                                                <asp:Button ID="btnEditGroup" OnClick="btnEditGroup_Click" runat="server" Text="Save" CssClass="btn-Save" Visible="False"></asp:Button>
                                                                            </td>
                                                                            <td class="tbCell1">
                                                                                <asp:Button ID="btnDeleteGroup" OnClick="btnDeleteGroup_Click1" runat="server" Text="Delete"
                                                                                    CssClass="btn-Cancel"></asp:Button>
                                                                            </td>
                                                                            <td class="tbCell1">
                                                                                <asp:Button ID="Button6" OnClick="Button1_Click" runat="server"
                                                                                    Text="View Accounts" CssClass="btn-Save" Visible="False"></asp:Button>

                                                                            </td>
                                                                        </tr>
                                                                    </tbody>
                                                                </table>
                                                            </asp:View>
                                                        </asp:MultiView>
                                                        <cc1:ConfirmButtonExtender ID="ConfirmButtonExtender1" runat="server" TargetControlID="btnEditGroup"
                                                            Enabled="False" ConfirmText="Are you sure you want to Approve?">
                                                        </cc1:ConfirmButtonExtender>
                                                        <cc1:ConfirmButtonExtender ID="ConfirmButtonExtender2" runat="server" TargetControlID="btnEditGroup"
                                                            Enabled="True" ConfirmText="Are you sure you want to Approve?">
                                                        </cc1:ConfirmButtonExtender>

                                                        <asp:Wizard ID="Wizard1" runat="server" Width="714px" BackColor="#E6E2D8" Font-Size="0.8em"
                                                            BorderWidth="1px" BorderStyle="Solid" BorderColor="#999999" OnFinishButtonClick="Wizard1_FinishButtonClick"
                                                            Visible="False" Font-Names="Verdana" DisplaySideBar="False" ActiveStepIndex="1">
                                                            <StepStyle BackColor="#F7F6F3" BorderColor="#E6E2D8" BorderWidth="2px" BorderStyle="Solid"></StepStyle>
                                                            <StartNextButtonStyle Font-Size="Medium" Height="30px" Width="80px"></StartNextButtonStyle>
                                                            <FinishCompleteButtonStyle Font-Size="Medium" Height="30px" Width="80px"></FinishCompleteButtonStyle>
                                                            <StepNextButtonStyle Font-Size="Medium" Height="30px" Width="80px"></StepNextButtonStyle>
                                                            <FinishPreviousButtonStyle Font-Size="Medium" Height="30px" Width="80px"></FinishPreviousButtonStyle>
                                                            <WizardSteps>
                                                                <asp:WizardStep runat="server" Title="BulkPayType">
                                                                    <br />
                                                                    <br />
                                                                    <br />
                                                                </asp:WizardStep>
                                                                <asp:WizardStep runat="server" Title="Upload File">
                                                                    &nbsp;&nbsp;<br />
                                                                    <br />
                                                                    <br />
                                                                    <br />
                                                                    <br />
                                                                    <br />
                                                                    <br />
                                                                    <table style="width: 712px">
                                                                        <tbody>
                                                                            <tr>
                                                                                <td colspan="3">&nbsp;
                                                                                </td>
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
                                                                                <asp:Label runat="server" Text="Account Linking" ForeColor="Red" ID="Label4"></asp:Label>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td>Customer Number:
                                                                            </td>
                                                                            <td>
                                                                                <asp:TextBox runat="server" ID="TextBox1" Enabled="False"></asp:TextBox>
                                                                            </td>
                                                                            <td></td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td>Mwallet Account Number:
                                                                            </td>
                                                                            <td>
                                                                                <asp:TextBox runat="server" ID="TextBox2" Enabled="False" ></asp:TextBox>
                                                                            </td>
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
                                                            <SideBarButtonStyle ForeColor="White"></SideBarButtonStyle>
                                                            <NavigationButtonStyle BackColor="White" BorderColor="#C5BBAF" BorderWidth="1px"
                                                                BorderStyle="Solid" Font-Names="Verdana" Font-Size="0.8em" ForeColor="#1C5E55"></NavigationButtonStyle>
                                                            <SideBarStyle VerticalAlign="Top" BackColor="#1C5E55" Font-Size="0.9em"></SideBarStyle>
                                                            <HeaderStyle HorizontalAlign="Center" BackColor="#666666" BorderColor="#E6E2D8" BorderWidth="2px"
                                                                BorderStyle="Solid" Font-Bold="True" Font-Size="0.9em" ForeColor="White"></HeaderStyle>
                                                            <StepPreviousButtonStyle Font-Overline="False" Font-Size="Medium" Height="30px" Width="80px"></StepPreviousButtonStyle>
                                                        </asp:Wizard>

                                                        <asp:Wizard ID="Wizard2" runat="server" Width="714px" BackColor="#E6E2D8" Font-Size="0.8em"
                                                            BorderWidth="1px" BorderStyle="Solid" BorderColor="#999999" OnFinishButtonClick="Wizard2_FinishButtonClick"
                                                            Visible="False" Font-Names="Verdana" DisplaySideBar="False" ActiveStepIndex="2">
                                                            <FinishCompleteButtonStyle Font-Size="Medium" Height="30px" Width="80px"></FinishCompleteButtonStyle>
                                                            <FinishPreviousButtonStyle Font-Size="Medium" Height="30px" Width="80px"></FinishPreviousButtonStyle>
                                                            <StartNextButtonStyle Font-Size="Medium" Height="30px" Width="80px"></StartNextButtonStyle>
                                                            <HeaderStyle HorizontalAlign="Center" BackColor="#666666" BorderColor="#E6E2D8" BorderWidth="2px"
                                                                BorderStyle="Solid" Font-Bold="True" Font-Size="0.9em" ForeColor="White"></HeaderStyle>
                                                            <NavigationButtonStyle BackColor="White" BorderColor="#C5BBAF" BorderWidth="1px"
                                                                BorderStyle="Solid" Font-Names="Verdana" Font-Size="0.8em" ForeColor="#1C5E55"></NavigationButtonStyle>
                                                            <StepNextButtonStyle Font-Size="Medium" Height="30px" Width="80px"></StepNextButtonStyle>
                                                            <StepPreviousButtonStyle Font-Overline="False" Font-Size="Medium" Height="30px" Width="80px"></StepPreviousButtonStyle>
                                                            <SideBarButtonStyle ForeColor="White"></SideBarButtonStyle>
                                                            <SideBarStyle VerticalAlign="Top" BackColor="#1C5E55" Font-Size="0.9em"></SideBarStyle>
                                                            <StepStyle BackColor="#F7F6F3" BorderColor="#E6E2D8" BorderWidth="2px" BorderStyle="Solid"></StepStyle>
                                                            <WizardSteps>
                                                                <asp:WizardStep runat="server" Title="BulkPayType">
                                                                </asp:WizardStep>
                                                                <asp:WizardStep runat="server" Title="Upload File">
                                                                </asp:WizardStep>
                                                                <asp:WizardStep runat="server" Title="linking">
                                                                </asp:WizardStep>
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
                        <td>
                            <asp:HiddenField ID="HiddenField1" runat="server"></asp:HiddenField>
                        </td>
                    </tr>
                </tbody>
            </table>
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
