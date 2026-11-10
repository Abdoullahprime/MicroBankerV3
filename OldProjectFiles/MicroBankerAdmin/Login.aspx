<%@ Page Language="vb" AutoEventWireup="false" MasterPageFile="~/Login.Master" CodeBehind="Login.aspx.vb" Inherits="MicroBankerAdmin.Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="Login">
        <div class="form-text">
            <asp:Label ID="lblMessage" runat="server" ForeColor="Red"></asp:Label>
            <asp:Label runat="server">User Name:</asp:Label>
            <asp:TextBox ID="txtUserName" runat="server" CssClass="tinyText"></asp:TextBox>
        </div>
        <div class="form-text">
            <asp:Label runat="server">Password:</asp:Label>
            <asp:TextBox ID="txtPassword" runat="server" CssClass="tinyText" TextMode="Password"></asp:TextBox>
        </div>
        <div class="auto-style1">
            <asp:Button ID="cmdLogin" Text="LOGIN" OnClick="Post" runat="server" CssClass="btn-Login" />
        </div>
    </div>
</asp:Content>
<asp:Content ID="Content2" runat="server" contentplaceholderid="Header">
    <style type="text/css">
        .auto-style1 {
            margin: 10px auto;
            width: 95%;
            height: 50px;
        }
    </style>
</asp:Content>

