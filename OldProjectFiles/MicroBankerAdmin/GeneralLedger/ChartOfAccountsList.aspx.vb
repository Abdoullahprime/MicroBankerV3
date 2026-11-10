Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Imports System.Data.SqlClient
Public Class ChartOfAccountsList
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            Me.LoadAccounts("All")
            ' Me.LoadGL_Head()
        End If
    End Sub

    Private Sub LoadAccounts(ByVal acctNo As String, Optional ByVal headType As Object = Nothing)
        Dim searchStr As String = ""
        Dim SqlAction As String = ""
        Dim Processfilter As String = ""
        Dim ApprovalStatus As String = ""
        Try
            Dim mySqlAction As String = ""
            Select Case acctNo
                Case "All"
                    mySqlAction = "select * from ChartOfAccounts where Deleted='N' order by head,sub1,acc_no"
                Case "Filter"
                    'If RdoSearch.Items(0).Selected = True Then
                    '    Processfilter = txtSearch.Text.ToUpper
                    '    Processfilter = "%" & Processfilter & "%"
                    '    mySqlAction = "select * from ChartOfAccounts where HEAD_TYPE="&Request.form("TYPE")&" and Deleted='N' order by head,sub1,acc_no""
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
            ChartOfAccountsGridView.DataSource = MyDatable
            ChartOfAccountsGridView.DataBind()
        Catch ex As Exception

        End Try
    End Sub

    Protected Sub btnViewAccountHeadRecord_Click(sender As Object, e As EventArgs)

    End Sub

    Protected Sub btnEdit_Click(sender As Object, e As EventArgs)

    End Sub

    Protected Sub ChartOfAccountsGridView_PageIndexChanging(sender As Object, e As GridViewPageEventArgs)

    End Sub
    'Private Sub Search()
    '    Dim headType As String = Me.GL_HeadTypeDropDownList.SelectedValue
    '    Dim acctNo_Name As String = txtSearchText.Text.ToLower.Trim()
    '    If headType = "0" Then
    '        ' Me.searchErrorLabel.Text = "Select Head Type To Proceed."
    '        Me.GL_HeadTypeDropDownList.Focus()
    '    Else
    '        ViewState("SearchHeadType") = headType
    '        ViewState("SearchAcctNo_Name") = acctNo_Name
    '        ' Me.searchErrorLabel.Text = ""
    '        Me.LoadChartOfAccountsGridView(acctNo_Name, headType)
    '        ' Me.COA_EditViewDeletePanel.Visible = True
    '        ' Me.txtSearchText.Text = ""
    '        '     Me.SearchHeadTypeDropDownList.SelectedValue = "0"
    '    End If
    'End Sub

    'Sub LoadChartOfAccountsGridView(queryStr As String, headType As String)
    '    Dim gridRecords = Maintenance.GetCOA_GridRecords(queryStr, headType)
    '    If gridRecords IsNot Nothing Then
    '        Me.ChartOfAccountsGridView.DataSource = gridRecords
    '        Me.ChartOfAccountsGridView.DataBind()
    '        ChartOfAccountsGridView.Visible = True
    '        '  Me.BtnRefresh.Enabled = True
    '        ' Me.BtnRefresh.CssClass = "ActiveButtonStyle"
    '    Else
    '        Dim dta As DataTable = New DataTable
    '        Me.ChartOfAccountsGridView.DataSource = dta
    '        Me.ChartOfAccountsGridView.DataBind()
    '        ChartOfAccountsGridView.Visible = True

    '    End If
    'End Sub
End Class