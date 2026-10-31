Imports System.Data.SqlClient
Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin

Public Class MiniStatement
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Private Sub btnCheckAccNo_Click(sender As Object, e As EventArgs) Handles btnCheckAccNo.Click
        'If lblAccountName.Text = "" Then
        '    lblSuccessMsg.Text = "Please Check Transaction Code"
        '    Exit Sub
        'Else
        DisplayDetails()
        GetAccountDetails(Me.txtAccountNo.Text.Trim)
        'End If
    End Sub

    Public Sub GetAccountDetails(AccountNo)
        Try
            lblSuccessMsg.Text = ""
            lblAccountName.Text = ""
            ' txtAmount.Text = ""
            If txtAccountNo.Text = "" Then
                txtAccountNo.Focus()
                'lblSuccessMsg.Text = " Please enter Account Number"
                Exit Sub
            End If
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select AccountNo,CustomerNo,AccountName,AssetGL_Control,LiabilityGL_Control,BranchCode,AvailBal,BookBalance,ProductDefCode from [dbo].[tbAccount] where AccountNo = '" & AccountNo & "'" ' ='" & Me.HiddenField1.Value & "' "
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                lblAccountName.Text = MyReader4("AccountName").ToString
                'P_AccountNo = MyReader4("TxnCode").ToString
                P_CustomerNo = MyReader4("CustomerNo").ToString
                P_AccountName = MyReader4("AccountName").ToString
                P_AssetGL_Control = MyReader4("AssetGL_Control").ToString
                P_LiabilityGL_Control = MyReader4("LiabilityGL_Control").ToString
                P_BranchCode = MyReader4("BranchCode").ToString
                AailBalance.Text = MyReader4("AvailBal").ToString
                BookBal.Text = MyReader4("BookBalance").ToString
                P_DefinationCode = MyReader4("ProductDefCode").ToString().Trim()
            Else
                lblSuccessMsg.Text = "Account Number does not exist"
            End If
            MyReader4.Close()
        Catch ex As Exception
            'txtOrgName.Text = ""
            'txtAdress1.Text = ""
            '' DropDownList1.Text = ""
            'txtOrgCode.ReadOnly = False
            'Label4.Text = "Error Pulling Account Details"
        End Try
    End Sub

    Private Sub DisplayDetails()
        Try
            Dim mySqlAction As String = ""
            Dim SqlAction As String = ""
            Dim searchStr As String = ""
            Dim MyDBAction As New DataManagement
            Dim MyDatable As New Data.DataTable

            searchStr = txtAccountNo.Text
            SqlAction = "select Top 5 AccountNo,AccountName,TxnType,Amount,FX_Rate,Details,INV_No,TxnRefNo,ChequeNo,TxnDate,TxnRefNo,Bookbalance,AvailBal from MB_Acc_DailyTransactions  where AccountNo ='" & txtAccountNo.Text & "'  order by SeqNo desc"
            MyDatable = TryCast(MyDBAction.DBAction(SqlAction, DataManagement.DBActionType.DataTable), Data.DataTable)
            Session("searchtable") = MyDatable
            MyDatable.Columns.Add("Debit", GetType(String))
            MyDatable.Columns.Add("Credit", GetType(String))
            MyDatable.Columns.Add("TxnType_", GetType(String))
            Dim i As Integer = 0
            For Each row As DataRow In MyDatable.Rows
                If MyDatable.Rows(i)("TxnType").ToString.Trim() = "C" Then
                    row("Debit") = "0.0"
                    row("Credit") = MyDatable.Rows(i)("Amount").ToString
                    row("TxnType_") = "Credit"
                ElseIf MyDatable.Rows(i)("TxnType").ToString.Trim() = "D" Then
                    row("Debit") = MyDatable.Rows(i)("Amount").ToString
                    row("Credit") = "0.0"
                    row("TxnType_") = "Debit"
                End If
                i = i + 1
            Next

            GridView1.DataSource = MyDatable
            GridView1.DataBind()

        Catch ex As Exception
        End Try
    End Sub

    Protected Sub GridView1_PageIndexChanging(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewPageEventArgs) Handles GridView1.PageIndexChanging
        GridView1.DataSource = Session("searchtable")
        GridView1.PageIndex = e.NewPageIndex
        GridView1.DataBind()
    End Sub

End Class