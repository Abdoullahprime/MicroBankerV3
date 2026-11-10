<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/Site.Master" CodeBehind="ProcessRoutineSOD.aspx.vb" Inherits="MicroBankerAdmin.ProcessRoutineSOD" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>

<asp:Content ID="Content1" ContentPlaceHolderID="Header" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>


            <table class="tbMain">
                <tbody>
                    <tr>
                        <td>
                            <asp:Button ID="btnClearForm" runat="server" Text="Process Routine" CausesValidation="False" CssClass="btn-New"></asp:Button>
                        </td>
                    </tr>
                    <tr>
                        <td>
                            <cc1:TabContainer ID="TabContainer1" runat="server" ActiveTabIndex="0">
                                <cc1:TabPanel runat="server" HeaderText="TabPanel1" ID="TabPanel1">
                                    <HeaderTemplate>
                                        Start of Day Process Routine
                                    </HeaderTemplate>




                                    <ContentTemplate>
                                        <table class="tbMain">
                                            <tbody>
                                                 <tr style="padding: 5px;">
                                                    <td>
                                                        <asp:Label ID="Label1" runat="server" Font-Bold="True" Text="Start of Day Date:"></asp:Label>

                                                    </td>
                                                    <td>
                                                        <asp:TextBox ID="txtSOD" runat="server" Enabled="False"></asp:TextBox>

                                                    </td>

                                                      <td>
                                                        <asp:Label ID="Label4" runat="server" Text="End of Month Date:" Font-Bold="True"></asp:Label>

                                                    </td>
                                                    <td>
                                                        <asp:TextBox ID="txtEOM" runat="server" Enabled="False"></asp:TextBox>

                                                    </td>
                                                   
                                                </tr>
                                                <tr style="padding: 5px;">
                                                  

                                                     <td>
                                                        <asp:Label ID="Label2" runat="server" Font-Bold="True" Text="End of Day Date:"></asp:Label>

                                                    </td>
                                                    <td>
                                                        <asp:TextBox ID="txtEOD" runat="server" Enabled="False"></asp:TextBox>

                                                    </td>

                                                    <td>
                                                        <asp:Label ID="Label5" runat="server" Text="End of Year Date:" Font-Bold="True"></asp:Label>
                                                    </td>
                                                    <td>
                                                        <asp:TextBox ID="txtEOY" runat="server" Enabled="False"></asp:TextBox>

                                                    </td>
                                                </tr>

                                                <tr>
                                                    <td align="right">
                                                        <asp:Label ID="lblDate" runat="server" Text="Date:" Font-Bold="True" />




                                                    </td>
                                                    <td>
                                                        <asp:TextBox ID="txtDate" runat="server" Enabled="False" />




                                                        <asp:ImageButton ID="btncalendars" runat="server" Style="width: 23px; height: 23px; padding-top: 5px;" ImageUrl="~/Img/Calendar_scheduleHS.png" />

                                                        <asp:RequiredFieldValidator ID="rfvDate" runat="server" ErrorMessage="*"
                                                            ControlToValidate="txtDate" ValidationGroup="Process" />




                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td></td>
                                                    <td>
                                                        <asp:Calendar ID="EODDatePicker" runat="server" BackColor="White" CssClass="myCalendar">
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
                                                    <td colspan="6">
                                                        <hr style="border: 3px solid #0f3b5a;" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                     <td class="CriteriaCell" colspan="9">
                                                        <asp:Label ID="lblSuccessMsg" runat="server"></asp:Label>

                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td colspan="6">

                                                        <asp:GridView ID="SODGridView" runat="server" AllowPaging="True" AutoGenerateColumns="False" BackColor="White" BorderColor="White" BorderStyle="Solid" BorderWidth="3px" CellPadding="4" CssClass="GridView table-striped" EmptyDataText="No Data To Display" ForeColor="Black" GridLines="Vertical">
                                                            <AlternatingRowStyle BackColor="White"></AlternatingRowStyle>
                                                            <Columns>
                                                                <asp:TemplateField HeaderText="ExecutionOrder">
                                                                    <ItemTemplate>
                                                                        <asp:Label ID="ExecutionOrder" runat="server" Text='<%# Eval("ExecutionOrder") %>'></asp:Label>





                                                                    </ItemTemplate>

                                                                    <ItemStyle Width="150px"></ItemStyle>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="Process Name">
                                                                    <ItemTemplate>
                                                                        <asp:Label ID="ProcessName" runat="server" Text='<%# Eval("ProcessName") %>'></asp:Label>





                                                                    </ItemTemplate>

                                                                    <ItemStyle Width="350px"></ItemStyle>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="Process Function" Visible="False">
                                                                    <ItemTemplate>
                                                                        <asp:Label ID="ProcessFunction" runat="server" Text='<%# Eval("ProcessFunction") %>' Visible="false"></asp:Label>





                                                                    </ItemTemplate>

                                                                    <ItemStyle Width="150px"></ItemStyle>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="Select" InsertVisible="False">
                                                                    <ItemTemplate>
                                                                        <asp:CheckBox ID="checkSelect" runat="server" Visible="true" />





                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="Action">
                                                                    <ItemTemplate>
                                                                        <%--<asp:Button ID="btProcess" runat="server" Text="Start of Day" CssClass="btn-Save" Style="margin: auto;" />--%>
                                                                        <asp:LinkButton ID="btnProcess" runat="server" CssClass="btn-Save" Enabled="true" OnClick="btnProcess_Click" Style="padding: 5px; font-size: small; margin: 3px;" Text="Process" />





                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField HeaderText="Status">
                                                                    <ItemTemplate>
                                                                        <%--<asp:Button ID="btProcess" runat="server" Text="Start of Day" CssClass="btn-Save" Style="margin: auto;" />--%>
                                                                        <asp:Label ID="lblStatus" runat="server" Text="Pending..."></asp:Label>





                                                                    </ItemTemplate>
                                                                </asp:TemplateField>
                                                                <asp:TemplateField>
                                                                    <ItemTemplate>
                                                                        <%--<asp:Label ID="Status" runat="server" Text='' Visible="false"></asp:Label>--%>
                                                                        <asp:Image ID="success" runat="server" ImageUrl="~/Img/tick.png" Style="width: 20px; height: 20px;" Visible="false" />
                                                                        <asp:Image ID="fail" runat="server" ImageUrl="~/Img/cross.jpg" Style="width: 20px; height: 20px;" Visible="false" />
                                                                        <asp:UpdateProgress ID="UpdateProgress1" runat="server">
                                                                            <ProgressTemplate>
                                                                                <asp:Image ID="loader" runat="server" src="Img/ajax-loader1.gif" Style="width: 15px" />
                                                                            </ProgressTemplate>
                                                                        </asp:UpdateProgress>





                                                                    </ItemTemplate>

                                                                    <ItemStyle Width="30px"></ItemStyle>
                                                                </asp:TemplateField>
                                                            </Columns>

                                                            <FooterStyle BackColor="#1C5E55" Font-Bold="True" ForeColor="White"></FooterStyle>

                                                            <HeaderStyle BackColor="#0F3B5A" Font-Bold="True" ForeColor="White"></HeaderStyle>

                                                            <PagerStyle BackColor="#666666" ForeColor="White" HorizontalAlign="Center"></PagerStyle>

                                                            <RowStyle BackColor="#E3EAEB"></RowStyle>

                                                            <SelectedRowStyle BackColor="#C5BBAF" Font-Bold="True" ForeColor="#333333"></SelectedRowStyle>
                                                        </asp:GridView>
                                                    </td>
                                                </tr>

                                                <tr>
                                                    <td colspan="6">
                                                        <hr style="border: 3px solid #0f3b5a;" />

                                                    </td>
                                                </tr>

                                                <tr>
                                                    <td colspan="6">
                                                            <asp:Button ID="btChanageDates" runat="server" Text="Change Date" CssClass="btn-Save" Style="margin: auto; width:200px" />
                                                    </td>
                                                </tr>

                                                <tr>
                                                    <td class="tbCell1"></td>
                                                    <td class="tbCell2"></td>
                                                </tr>
                                                <tr>
                                                    <td class="tbCell1"></td>
                                                    <td class="tbCell2">
                                                        <asp:UpdateProgress ID="UpdateProgress1" runat="server">
                                                            <ProgressTemplate>
                                                                <img src="Img/ajax-loader1.gif" style="width: 35px" /><asp:Label ID="Label3" runat="server" ForeColor="Red" Text="Please Wait..."></asp:Label>





                                                            </ProgressTemplate>
                                                        </asp:UpdateProgress>




















                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>





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
