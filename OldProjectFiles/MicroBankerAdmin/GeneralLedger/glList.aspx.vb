Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Imports System.Data.SqlClient
Public Class glList
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            Me.LoadAccounts("All")

        End If
    End Sub


    Private Sub LoadAccounts(ByVal accountNo As String)
        Dim searchStr As String = ""
        Dim SqlAction As String = ""
        Dim Processfilter As String = ""
        Dim ApprovalStatus As String = ""
        Try
            Dim mySqlAction As String = ""
            Select Case accountNo
                Case "All"
                    mySqlAction = "SELECT * FROM GL_DETAIL WHERE DELETED='N'"
                Case "Filter"
                    'If RdoSearch.Items(0).Selected = True Then
                    '    Processfilter = txtSearch.Text.ToUpper
                    '    Processfilter = "%" & Processfilter & "%"
                    '    mySqlAction = "select CustomerNo,AccountType,BranchName,AccountNo,AccountName,Currency,Sector,SubSector,IntRate,DebitNoStatus,BranchCode,AccountPrefix,C_Address1 from tbAccount where AccountName LIKE '" & Processfilter & "'"
                    'End If
                    'If RdoSearch.Items(1).Selected = True Then
                    '    Processfilter = txtSearch.Text.ToUpper
                    '    Processfilter = "%" & Processfilter & "%"
                    '    mySqlAction = "select CustomerNo,AccountType,BranchName,AccountNo,AccountName,Currency,Sector,SubSector,IntRate,DebitNoStatus,BranchCode,AccountPrefix,C_Address1 from tbAccount where AccountNo LIKE '" & Processfilter & "'"
                    'End If
            End Select
            Dim MyDBAction As New DataManagement
            Dim MyDatable As New Data.DataTable
            MyDatable = TryCast(MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.DataTable), Data.DataTable)
            Session("searchtable") = MyDatable
            If MyDatable.Rows.Count > 0 Then
                GeneralLedgerGridView.DataSource = MyDatable
                GeneralLedgerGridView.DataBind()
            Else
                Dim dta As DataTable = New DataTable
                GeneralLedgerGridView.DataSource = dta
                GeneralLedgerGridView.DataBind()
            End If
        Catch ex As Exception

        End Try
    End Sub


    Protected Sub GeneralLedgerGridView_RowCommand(sender As Object, e As GridViewCommandEventArgs)

    End Sub

    Protected Sub GeneralLedgerGridView_RowDataBound(sender As Object, e As GridViewRowEventArgs)
        If e.Row.RowType = DataControlRowType.DataRow Then
            Dim _balanceTypeLabel As Label = DirectCast(e.Row.FindControl("balanceTypeLabel"), Label)
            Dim _accountTypeLabel As Label = DirectCast(e.Row.FindControl("accountTypeLabel"), Label)

            If _balanceTypeLabel.Text <> "" Then
                If _balanceTypeLabel.Text.Trim() = "C" Then
                    _balanceTypeLabel.Text = "Credit"
                ElseIf _balanceTypeLabel.Text.Trim() = "D" Then
                    _balanceTypeLabel.Text = "Debit"
                End If
            End If
            If _accountTypeLabel.Text <> "" Then
                If _accountTypeLabel.Text.Trim() = "A" Then
                    _accountTypeLabel.Text = "Asset"
                ElseIf _accountTypeLabel.Text.Trim() = "L" Then
                    _accountTypeLabel.Text = "Liablity"
                ElseIf _accountTypeLabel.Text.Trim() = "I" Then
                    _accountTypeLabel.Text = "Income"
                ElseIf _accountTypeLabel.Text.Trim() = "E" Then
                    _accountTypeLabel.Text = "Expense"
                End If
            End If
        End If
    End Sub

    Private Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
        '"Filter"
    End Sub

    Protected Sub ImgView_Click(sender As Object, e As ImageClickEventArgs)

    End Sub

    Protected Sub ImgEdit_Click(sender As Object, e As ImageClickEventArgs)

    End Sub

    Protected Sub ImgDelete_Click(sender As Object, e As ImageClickEventArgs)

    End Sub
End Class