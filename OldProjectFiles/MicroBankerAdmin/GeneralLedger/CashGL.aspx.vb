Imports System.Data.SqlClient
Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Public Class CashGL
    Inherits System.Web.UI.Page
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        'page load
        Dim MyAcctDetails As String = ""
        If Not Page.IsPostBack Then
            TabContainer1.ActiveTabIndex = 0
            DisplayDetails("All")
            txtGLAccount.ReadOnly = False
        End If
    End Sub
    Public Function LogError(ByVal strErrorMessage As String, ByVal strErrorID As String, ByVal strModuleName As String, ByVal strMethodName As String, ByVal strTimeOccur As Date, ByVal strOperation As String) As Boolean
        Try
            'error logging
            Dim strErrorLog As System.IO.StreamWriter = Nothing
            Dim fileName As String = ""
            Dim DateT = Format(DateTime.Now(), "dd-MMM-yyyy")
            fileName = DateT & ".log"
            Dim path As String = "C:\Logs"
            If path.Substring(path.LastIndexOf("\") + 1).Contains("\") <> False Then
                fileName = path & fileName
            Else
                fileName = path & "\" & fileName
            End If
            If System.IO.File.Exists(fileName) = False Then
                System.IO.File.Create(fileName).Close()
            End If
            strErrorLog = New System.IO.StreamWriter(fileName, True)
            strErrorLog.WriteLine("----------------------------------------------------------------------------------------------------")
            strErrorLog.WriteLine(strErrorMessage & " : " & strErrorID & " : " & strModuleName & " : " & strMethodName & " : " & strTimeOccur & " : " & strOperation)
            strErrorLog.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~")
            strErrorLog.WriteLine()
            strErrorLog.Dispose()
        Catch ex As Exception
        End Try
    End Function

    Private Sub DisplayDetails(ByVal ShowType As String)
        Dim searchStr As String = ""
        Dim SqlAction As String = ""
        Dim Processfilter As String = ""
        Dim ApprovalStatus As String = ""
        'pre load data
        Try
            Dim mySqlAction As String = ""
            Select Case ShowType
                Case "All"
                    mySqlAction = "select GLAccount,GLName,Amount from tbCashGL"
                Case "Filter"
                    If RdoSearch.Items(0).Selected = True Then
                        Processfilter = txtSearch.Text.ToUpper
                        Processfilter = "%" & Processfilter & "%"
                        mySqlAction = "select GLAccount,GLName,Amount from tbCashGL where GLAccount LIKE '" & Processfilter & "'"
                    End If
                    If RdoSearch.Items(1).Selected = True Then
                        Processfilter = txtSearch.Text.ToUpper
                        Processfilter = "%" & Processfilter & "%"
                        mySqlAction = "select GLAccount,GLName,Amount from tbCashGL where GLName LIKE '" & Processfilter & "'"
                    End If
            End Select
            Dim MyDBAction As New DataManagement
            Dim MyDatable As New Data.DataTable
            MyDatable = TryCast(MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.DataTable), Data.DataTable)
            Session("searchtable") = MyDatable
            GridView1.DataSource = MyDatable
            GridView1.DataBind()
        Catch ex As Exception
        End Try
    End Sub
    Protected Sub GridView1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles GridView1.Load

    End Sub
    Protected Sub GridView1_PageIndexChanging(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewPageEventArgs) Handles GridView1.PageIndexChanging
        GridView1.DataSource = Session("searchtable")
        GridView1.PageIndex = e.NewPageIndex
        GridView1.DataBind()
    End Sub

    Protected Sub GridView1_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles GridView1.PreRender

    End Sub
    Protected Sub GridView1_RowCommand(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewCommandEventArgs) Handles GridView1.RowCommand
        Try
            'Gridview data selection
            HiddenField1.Value = ""
            Dim MyID As String = e.CommandArgument
            HiddenField1.Value = MyID
            Select Case e.CommandName
                Case "edit"
                    txtGLAccount.Enabled = False
                    ShowMatchDetails("Select   * From tbCashGL where GLAccount= '" & HiddenField1.Value.ToString & "' ", "ViewDetails")
                    TabContainer1.ActiveTabIndex = 0
                    DisplayDetails("All")
                Case "select"
                    txtGLAccount.Enabled = True
                    ShowMatchDetails("Select  * From tbCashGL where GLAccount= '" & HiddenField1.Value.ToString & "' ", "ViewDetails")
                    TabContainer1.ActiveTabIndex = 0
                    DisplayDetails("All")
            End Select
        Catch ex As Exception
            Dim ErrMsg As String = ex.Message
        End Try
    End Sub
    Protected Sub GridView1_RowDeleting(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewDeleteEventArgs) Handles GridView1.RowDeleting

    End Sub
    Protected Sub GridView1_RowEditing(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewEditEventArgs) Handles GridView1.RowEditing

    End Sub

    Private Sub ShowMatchDetails(ByVal mySearch As String, ByVal ShowType As String)
        Dim MySmsObject As New DataManagement
        Dim MyTransProcessed As Boolean = False
        Dim MyReader As SqlDataReader = TryCast(MySmsObject.DBAction(mySearch, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
        Try
            ''Load data from gridview 
            Select Case ShowType
                Case "ViewDetails"
                    If MyReader.HasRows Then
                        MyReader.Read()
                        txtAmount.Text = MyReader("Amount").ToString
                        txtGLAccount.Text = MyReader("GLAccount").ToString
                        txtGLName.Text = MyReader("GLName").ToString
                    End If
            End Select
        Catch ex As Exception
        Finally
            If Not IsNothing(MyReader) Then
                MyReader.Close()
            End If
        End Try
    End Sub
    Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSave.Click
        Try
            'Save input data
            If Me.HiddenField1.Value = "" Then
                Dim mystring As String = ""
                mystring = txtGLAccount.Text.ToUpper()
                Dim MySmsObject As New DataManagement
                Dim mySearch As String = "select GLAccount from tbCashGL Where GLAccount ='" & txtGLAccount.Text.Trim & "'"
                Dim MyReader As SqlDataReader = TryCast(MySmsObject.DBAction(mySearch, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
                If MyReader.HasRows = True Then
                    Label4.Text = "This GL Account Already Exist"
                    Exit Sub
                End If
                MyReader.Close()
                If txtAmount.Text = "" Then
                    txtAmount.Text = 0
                End If
                Dim mySqlAction As String = ""
                Dim MyDBAction As New DataManagement
                mySqlAction = "Insert into tbCashGL (GLAccount,GLName,Amount)values('" & txtGLAccount.Text.Trim & "','" & txtGLName.Text.Trim & "','" & txtAmount.Text.ToUpper.Trim & "')" ','" & txtDistrictName.Text.ToUpper & "','" & txtAdress2.Text.ToUpper & "','" & txtmobile.Text & "') "
                MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
                Label4.Text = "GL Details have been saved successfully"
                DisplayDetails("All")
                'AUDIT TRAIL-------
                'Dim audit As New DataManagement
                'audit.AddAdminAuditTrail(Session("UserName").ToString, "Action", "Successful Saving of branch  '" & txtDistrictName.Text.ToUpper & "'", _
                'Request.Url.ToString, Session("ip").ToString, Session("sessionid").ToString)
                ''----END AUDIT TRAIL-----
                txtGLAccount.ReadOnly = False
            Else
                'update
                Dim MySqlaction1 As String
                Dim mySqlAction As String = ""
                Dim MyCurrencyName As String = ""
                Dim MyDBAction As New DataManagement
                MySqlaction1 = "update tbCashGL set GLAccount ='" & txtGLAccount.Text.Trim & "',GLName = '" & txtGLName.Text.Trim & "', Amount ='" & txtAmount.Text.ToUpper.Trim & "'  where GLAccount ='" & HiddenField1.Value.ToString & "'"
                MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
                DisplayDetails("All")
                Label4.Text = "GL details have been Updated successfully"
                txtGLAccount.ReadOnly = False
                'DisplayDetails("All")
                'AUDIT TRAIL-------
                'Dim audit As New DataManagement
                'audit.AddAdminAuditTrail(Session("UserName").ToString, "Action", "Successful Saving of branch  '" & txtDistrictName.Text.ToUpper & "'", _
                'Request.Url.ToString, Session("ip").ToString, Session("sessionid").ToString)
                ''----END AUDIT TRAIL-----
            End If
        Catch ex As Exception
        End Try
        txtAmount.Text = ""
        HiddenField1.Value = ""
        txtGLAccount.Text = ""
        txtGLName.Text = ""
    End Sub

    Protected Sub DropDownList1_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        DisplayDetails("All")
    End Sub

    Protected Sub CmdApprovalList_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        'Reset input controls
        Me.HiddenField1.Value = ""
        txtAmount.Text = ""
        txtGLName.Text = ""
        txtGLAccount.Text = ""
        txtGLAccount.Enabled = True
    End Sub

    Protected Sub btnClear_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnClear.Click
        'Clear controls
        txtAmount.Text = ""
        HiddenField1.Value = ""
        txtGLAccount.Text = ""
        txtGLName.Text = ""
        txtGLAccount.ReadOnly = False
        GridView1.EditIndex = -1
        DisplayDetails("All")
    End Sub
    Protected Sub GridView1_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)

    End Sub

    Protected Sub LinkButton1_Click(ByVal sender As Object, ByVal e As System.EventArgs)

    End Sub

    Protected Sub cmddelete_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmddelete.Click
        GridView1.EditIndex = -1
        DisplayDetails("All")
        Try
            'Delete record
            Dim mySqlAction As String = ""
            Dim MyDBAction As New DataManagement
            mySqlAction = "Delete from tbCashGL where GLAccount='" & HiddenField1.Value.ToString & "'"
            MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Delete)
            Me.Label4.Text = "GL Details Have Been Deleted Successfully"
            DisplayDetails("All")
            txtAmount.Text = ""
            HiddenField1.Value = ""
            txtGLAccount.Text = ""
            txtGLName.Text = ""
            ''AUDIT TRAIL-------
            'Dim audit As New DataManagement
            'audit.AddAdminAuditTrail(Session("UserName").ToString, "Action", "Successful Deletion of branch", _
            'Request.Url.ToString, Session("ip").ToString, Session("sessionid").ToString)
            ''----END AUDIT TRAIL-----
        Catch ex As Exception
        End Try
    End Sub
    Protected Sub cmasearch_Click(ByVal sender As Object, ByVal e As System.EventArgs)
    End Sub
    Protected Sub Button3_Click(ByVal sender As Object, ByVal e As System.EventArgs)
    End Sub
    Protected Sub txtEmail_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs)

    End Sub

    Protected Sub RdoSearch_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)

    End Sub

    Protected Sub CmdSearch_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        DisplayDetails("Filter")
    End Sub

    Protected Sub CmdViewAll_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        DisplayDetails("All")
    End Sub

    Protected Sub txtSearch_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs)

    End Sub

End Class