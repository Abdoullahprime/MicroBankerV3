Imports System.Data.SqlClient
Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Public Class CreatBranch
    Inherits System.Web.UI.Page
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        'page load
        If Not Page.IsPostBack Then
            TabContainer1.ActiveTabIndex = 0
            DisplayDetails("All")
            txtBranchCode.ReadOnly = False
        End If
    End Sub
    Public Function LogError(ByVal strErrorMessage As String, ByVal strErrorID As String, ByVal strModuleName As String, ByVal strMethodName As String, ByVal strTimeOccur As Date, ByVal strOperation As String) As Boolean
        Try
            'Error logging
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
        Try
            'Filter data
            Dim mySqlAction As String = ""
            Select Case ShowType
                Case "All"
                    mySqlAction = "select BranchCode,BranchName,Address1,Address2,Address3,PhoneNo from Branches"
                Case "Filter"
                    If RdoSearch.Items(0).Selected = True Then
                        Processfilter = txtSearch.Text.ToUpper
                        Processfilter = "%" & Processfilter & "%"
                        mySqlAction = "select BranchCode,BranchName,Address1,Address2,Address3,PhoneNo from Branches where BranchName LIKE '" & Processfilter & "'"
                    End If
                    If RdoSearch.Items(1).Selected = True Then
                        Processfilter = txtSearch.Text.ToUpper
                        Processfilter = "%" & Processfilter & "%"
                        mySqlAction = "select BranchCode,BranchName,Address1,Address2,Address3,PhoneNo from Branches where BranchCode LIKE '" & Processfilter & "'"
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
                    txtBranchCode.Enabled = False
                    ShowMatchDetails("Select   * From Branches where BranchCode= '" & HiddenField1.Value.ToString & "' ", "ViewDetails")
                    TabContainer1.ActiveTabIndex = 0
                    DisplayDetails("All")
                Case "select"
                    txtBranchCode.Enabled = True
                    ShowMatchDetails("Select  * From Branches where BranchCode= '" & HiddenField1.Value.ToString & "' ", "ViewDetails")
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
        Dim MyReader As SqlDataReader = TryCast(MySmsObject.DBAction(mySearch, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
        Try
            'Load data from gridview 
            Select Case ShowType
                Case "ViewDetails"
                    If MyReader.HasRows Then
                        MyReader.Read()
                        txtBranchName.Text = MyReader("BranchName").ToString
                        txtAdress2.Text = MyReader("Address2").ToString
                        txtAdress1.Text = MyReader("Address1").ToString
                        txtAdress3.Text = MyReader("Address3").ToString
                        txtPhoneNo.Text = MyReader("PhoneNo").ToString
                        txtBranchCode.Text = MyReader("BranchCode").ToString
                    End If
            End Select
        Catch ex As Exception
        Finally
            If Not IsNothing(MyReader) Then
                MyReader.Close()
            End If
        End Try
    End Sub
    Private Function BranchCodeExist(ByVal strBranchcode As String) As Boolean
        'check if branch code exists
        Dim MyDBAction As New DataManagement
        Dim MyTable As Data.DataTable
        Dim MySqlAction As String = "Select BranchCode from Branches where BranchCode = '" & strBranchcode & "'"
        MyTable = TryCast(MyDBAction.DBAction(MySqlAction, DataManagement.DBActionType.SqlDataTable), Data.DataTable)
        If MyTable.Rows.Count > 0 Then
            Return True
        Else
            Return False
        End If
    End Function
    Function CheckNumeric(ByVal a As String)
        'check only numeric values
        Dim numeric As Boolean = False
        Dim ContainsAlpha As Boolean
        Dim ContainsNumeric As Boolean
        Dim ContainsInvalidCharacter As Boolean
        For Each Character As Char In a
            If Char.IsNumber(Character) Then
                numeric = True
                ContainsNumeric = True
            ElseIf Char.IsLetter(Character) Then
                numeric = False
                ContainsAlpha = True
                Exit For
            ElseIf Not Char.IsWhiteSpace(Character) Then
                numeric = False
                ContainsInvalidCharacter = True
                Exit For
            End If
        Next
        Return numeric
    End Function
    Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSave.Click
        Try
            'Save input data
            If Me.HiddenField1.Value = "" Then
                Dim mystring As String = ""
                mystring = txtBranchCode.Text.ToUpper()
                If CheckNumeric(txtBranchCode.Text) = False Then
                    lblMessage.Text = "Enter correct branch code,it must be numeric"
                    Exit Sub
                End If
                If BranchCodeExist(txtBranchCode.Text.Trim) = True Then
                    lblMessage.Text = "The branch code exists,try another code!"
                    Exit Sub
                End If
                Dim mySqlAction As String = ""
                Dim MyDBAction As New DataManagement
                mySqlAction = "Insert into Branches (BranchCode,BranchName,Address1,Address2,Address3,PhoneNo)values('" & txtBranchCode.Text.Trim & "','" & txtBranchName.Text.ToUpper.Trim & "','" & txtAdress1.Text.ToUpper.Trim & "','" & txtAdress2.Text.ToUpper.Trim & "','" & txtAdress3.Text.ToUpper.Trim & "','" & txtPhoneNo.Text.Trim & "')" ','" & txtBranchName.Text.ToUpper & "','" & txtAdress2.Text.ToUpper & "','" & txtmobile.Text & "') "
                MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
                lblMessage.Text = "Branch Details have been saved successfully"
                DisplayDetails("All")
                'AUDIT TRAIL-------
                'Dim audit As New DataManagement
                'audit.AddAdminAuditTrail(Session("UserName").ToString, "Action", "Successful Saving of branch  '" & txtBranchName.Text.ToUpper & "'", _
                'Request.Url.ToString, Session("ip").ToString, Session("sessionid").ToString)
                ''----END AUDIT TRAIL-----
                txtBranchCode.ReadOnly = False
            Else
                'update
                Dim MySqlaction1 As String
                Dim mySqlAction As String = ""
                Dim MyCurrencyName As String = ""
                Dim MyDBAction As New DataManagement
                MySqlaction1 = "update Branches set BranchCode ='" & txtBranchCode.Text & "', BranchName ='" & txtBranchName.Text.ToUpper.Trim & "', Address1 ='" & txtAdress1.Text.ToUpper.Trim & "', Address2 ='" & txtAdress2.Text.ToUpper.Trim & "',  Address3 ='" & txtAdress3.Text.ToUpper.Trim & "',PhoneNo ='" & txtPhoneNo.Text & "'  where BranchCode ='" & txtBranchCode.Text & "'"
                MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
                DisplayDetails("All")
                lblMessage.Text = "Branch details have been updated successfully"
                txtBranchCode.ReadOnly = False
                'DisplayDetails("All")
                'AUDIT TRAIL-------
                'Dim audit As New DataManagement
                'audit.AddAdminAuditTrail(Session("UserName").ToString, "Action", "Successful Saving of branch  '" & txtBranchName.Text.ToUpper & "'", _
                'Request.Url.ToString, Session("ip").ToString, Session("sessionid").ToString)
                ''----END AUDIT TRAIL-----
            End If
        Catch ex As Exception
        End Try
        ClearControls()
    End Sub
    Protected Sub DropDownList1_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        'populate grid
        DisplayDetails("All")
    End Sub
    Protected Sub CmdApprovalList_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles CmdApprovalList.Click
        'Reset input controls
        ClearControls()
        Me.HiddenField1.Value = ""
        txtBranchCode.Enabled = True
        GridView1.EditIndex = -1
        DisplayDetails("All")
        lblMessage.Text = ""
    End Sub
    Protected Sub btnClear_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnClear.Click
        'Clear controls
        ClearControls()
        txtBranchCode.ReadOnly = False
        GridView1.EditIndex = -1
        DisplayDetails("All")
        lblMessage.Text = ""
    End Sub
    Protected Sub txtAdress3_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs)
    End Sub
    Protected Sub GridView1_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
    End Sub
    Protected Sub LinkButton1_Click(ByVal sender As Object, ByVal e As System.EventArgs)
    End Sub
    Protected Sub cmddelete_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmddelete.Click
        'Delete record
        GridView1.EditIndex = -1
        DisplayDetails("All")
        Try
            If CheckNumeric(txtBranchCode.Text) = False Then
                lblMessage.Text = "Enter correct branch code, it must be numeric"
                Exit Sub
            End If
            If BranchCodeExist(txtBranchCode.Text.Trim) = False Then
                lblMessage.Text = "The branch code does not exist!"
                Exit Sub
            End If
            Dim mySqlAction As String = ""
            Dim MyDBAction As New DataManagement
            mySqlAction = "Delete from Branches where BranchCode='" & HiddenField1.Value.ToString & "'"
            MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Delete)
            Me.lblMessage.Text = "Branch Details Have Been Deleted Successfully"
            DisplayDetails("All")
            ClearControls()
            HiddenField1.Value = ""
            ''AUDIT TRAIL-------
            'Dim audit As New DataManagement
            'audit.AddAdminAuditTrail(Session("UserName").ToString, "Action", "Successful Deletion of branch", _
            'Request.Url.ToString, Session("ip").ToString, Session("sessionid").ToString)
            ''----END AUDIT TRAIL-----
        Catch ex As Exception
        End Try
    End Sub
    Protected Sub txtBranchCode_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs)
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
    Protected Sub ClearControls()
        txtBranchName.Text = ""
        txtAdress2.Text = ""
        HiddenField1.Value = ""
        txtAdress3.Text = ""
        txtPhoneNo.Text = ""
        txtBranchCode.Text = ""
        txtAdress1.Text = ""
    End Sub
End Class
