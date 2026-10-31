<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="CreateDistrict.aspx.vb" Inherits="MicroBankerAdmin.CreateDistrict" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>


            <table class="tbMain">
                <tbody>
                    <tr>
                        <td style="height: 25px; text-align: left">
                            <asp:Button ID="CmdApprovalList" OnClick="CmdApprovalList_Click" runat="server" Text="New District/Municipality" CausesValidation="False" CssClass="btn-New"></asp:Button>&nbsp;<br />
                        </td>
                        <td style="height: 25px"></td>
                        <td style="height: 25px"></td>
                    </tr>
                    <tr>
                        <td colspan="3">
                            <cc1:TabContainer ID="TabContainer1" runat="server" ActiveTabIndex="0">
                                <cc1:TabPanel runat="server" HeaderText="TabPanel1" ID="TabPanel1">
                                    <HeaderTemplate>
                                        Create District/Municipality
                                   
                                    </HeaderTemplate>
                                    <ContentTemplate>
                                        <table class="tbMain">
                                            <tbody>
                                                <tr>
                                                    <td class="CriteriaCell" colspan="9">
                                                        <asp:Label ID="Label1" runat="server" Text="Fields marked with * are mandatory " CssClass="Criteria" ForeColor="Red"></asp:Label><br /><br />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="tbCell1">District Code:<b class="boldStar">*</b></td>
                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="txtDistrictCode" runat="server" OnTextChanged="txtDistrictCode_TextChanged" CssClass="tinyText" MaxLength="3"></asp:TextBox>
                                                        <%--<asp:Button ID="cmasearch" OnClick="cmasearch_Click" runat="server" Text="Search" CausesValidation="False"></asp:Button>--%>

                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" Display="Dynamic" CssClass="Validator" SetFocusOnError="true" ErrorMessage="*" ControlToValidate="txtDistrictCode">Please enter District Code</asp:RequiredFieldValidator>
                                                    </td>
                                                    <td class="tbCell1">District Name:<b class="boldStar">*</b></td>
                                                    <td class="tbCell2">
                                                        <asp:TextBox ID="txtDistrictName" runat="server" CssClass="tinyText"></asp:TextBox>

                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" Width="240px" Display="Dynamic" CssClass="Validator" SetFocusOnError="true" ErrorMessage="Please Enter District Name" ControlToValidate="txtDistrictName"></asp:RequiredFieldValidator>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style="height: 19px" colspan="3">
                                                        <asp:Label ID="Label4" runat="server" ForeColor="Red"></asp:Label>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td colspan="2">
                                                        <asp:Button ID="Button1" OnClick="Button1_Click" runat="server" Text="Save" CssClass="btn-Save"></asp:Button>
                                                    </td>
                                                    <td colspan="3">
                                                        <asp:Button ID="Button2" OnClick="Button2_Click" runat="server" Text="Clear" CausesValidation="False" CssClass="btn-Cancel"></asp:Button>
                                                    </td>
                                                    <td colspan="3">
                                                        <asp:Button ID="cmddelete" OnClick="cmddelete_Click" runat="server" Width="86px" Text="Delete" CssClass="btn-Cancel"></asp:Button>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td >

                                                        <asp:UpdateProgress ID="UpdateProgress1" runat="server">
                                                            <ProgressTemplate>
                                                                <img src="Img/ajax-loader1.gif" style="width: 35px" /><asp:Label ID="Label3" runat="server" Text="Please Wait..." ForeColor="Red"></asp:Label>
                                                            </ProgressTemplate>
                                                        </asp:UpdateProgress>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td>
                                                        <cc1:ConfirmButtonExtender ID="ConfirmButtonExtender1" runat="server" Enabled="True" TargetControlID="Button1" ConfirmText="Are you sure you want to save?"></cc1:ConfirmButtonExtender>
                                                        <cc1:ConfirmButtonExtender ID="ConfirmButtonExtender2" runat="server" Enabled="True" TargetControlID="cmddelete" ConfirmText="Are you sure you want to Delete?"></cc1:ConfirmButtonExtender>
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
                                </cc1:TabPanel>
                                <cc1:TabPanel runat="server" HeaderText="TabPanel2" ID="TabPanel2">
                                    <HeaderTemplate>
                                        Districts/Municipality
                                   
                                    </HeaderTemplate>
                                    <ContentTemplate>
                                        <div class="searchHolder">
                                            <asp:RadioButtonList ID="RdoSearch" runat="server" Width="690px" OnSelectedIndexChanged="RdoSearch_SelectedIndexChanged" RepeatDirection="Horizontal">
                                                <asp:ListItem Value="Names">District Name</asp:ListItem>
                                                <asp:ListItem Value="Account">District Code</asp:ListItem>
                                            </asp:RadioButtonList><br />
                                        </div>

                                        <div class="searchHolder">
                                            <div class="row">
                                                <div class="col-md-6">
                                                    <asp:TextBox ID="txtSearch" runat="server" Width="349px" OnTextChanged="txtSearch_TextChanged" CssClass="tinyText"></asp:TextBox>
                                                </div>
                                                <div class="col-md-3">
                                                    <asp:Button ID="CmdSearch" OnClick="CmdSearch_Click" runat="server" Width="104px" Text="Search" CausesValidation="False" CssClass="btn-Save"></asp:Button>
                                                </div>
                                                <div class="col-md-3">
                                                    <asp:Button ID="CmdViewAll" OnClick="CmdViewAll_Click" runat="server" Width="96px" Text="View All" CausesValidation="False" CssClass="btn-Save"></asp:Button>
                                                </div>
                                            </div>
                                        </div>



                                        <div class="gridHolder">
                                            <asp:GridView ID="GridView1" runat="server" OnSelectedIndexChanged="GridView1_SelectedIndexChanged"
                                                EnableTheming="True" CellPadding="3" AutoGenerateColumns="False" AllowPaging="True"
                                                BackColor="White" BorderColor="white" BorderStyle="Solid" BorderWidth="3px" CssClass="GridView table-striped" ForeColor="Black" GridLines="Vertical">
                                                <AlternatingRowStyle BackColor="White"></AlternatingRowStyle>
                                                <Columns>
                                                    <asp:BoundField DataField="DistrictCode" HeaderText="DISTRICT CODE"></asp:BoundField>
                                                    <asp:BoundField DataField="DistrictName" HeaderText="DISTRICT NAME"></asp:BoundField>
                                                    <asp:TemplateField>
                                                        <ItemTemplate>
                                                            <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="False" CommandArgument='<%# Eval("DistrictCode") %>' CommandName="edit">Edit</asp:LinkButton>
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
                                        <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:ConnectionString %>" ProviderName="<%$ ConnectionStrings:ConnectionString.ProviderName %>" SelectCommand="select OrgCode,OrgName,Address1,Address2,Address3,PhoneNo,Website,Email,LocalCurrency,CurrencyCode from Organisation"></asp:SqlDataSource>
                                    </ContentTemplate>
                                </cc1:TabPanel>
                            </cc1:TabContainer></td>
                    </tr>
                    <tr>
                        <td style="width: 186px; height: 16px"></td>
                        <td style="height: 16px"></td>
                        <td style="height: 16px"></td>
                    </tr>
                </tbody>
            </table>



        </ContentTemplate>
    </asp:UpdatePanel>
</asp:Content>
