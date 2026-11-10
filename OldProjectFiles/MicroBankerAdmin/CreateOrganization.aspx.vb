Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Imports System.Data.SqlClient
Public Class CreateOrganization
    Inherits System.Web.UI.Page
    'page load
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            TabContainer1.ActiveTabIndex = 0
            DisplayDetails("All")
            txtOrgCode.ReadOnly = False
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
                    mySqlAction = "select OrgCode,OrgName,Address1,Address2,Address3,PhoneNo,Website,Email,LocalCurrency,CurrencyCode from Organisation"
                Case "Filter"
                    If RdoSearch.Items(0).Selected = True Then
                        Processfilter = txtSearch.Text.ToUpper
                        Processfilter = "%" & Processfilter & "%"
                        mySqlAction = "select OrgCode,OrgName,Address1,Address2,Address3,PhoneNo,Website,Email,LocalCurrency,CurrencyCode from Organisation where OrgName LIKE '" & Processfilter & "'"
                    End If
                    If RdoSearch.Items(1).Selected = True Then
                        Processfilter = txtSearch.Text.ToUpper
                        Processfilter = "%" & Processfilter & "%"
                        mySqlAction = "select OrgCode,OrgName,Address1,Address2,Address3,PhoneNo,Website,Email,LocalCurrency,CurrencyCode from Organisation where OrgCode LIKE '" & Processfilter & "'"
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
                    txtOrgCode.Enabled = False
                    txtCurrency.Enabled = False
                    ShowMatchDetails("Select   * From Organisation where OrgCode= '" & HiddenField1.Value.ToString & "' ", "ViewDetails")
                    TabContainer1.ActiveTabIndex = 0
                    DisplayDetails("All")
                Case "select"
                    txtOrgCode.Enabled = True
                    txtCurrency.Enabled = True
                    ShowMatchDetails("Select  * From Organisation where OrgCode= '" & HiddenField1.Value.ToString & "' ", "ViewDetails")
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
            Select Case ShowType
                Case "ViewDetails"
                    'Load data from gridview 
                    If MyReader.HasRows Then
                        MyReader.Read()
                        txtOrgName.Text = MyReader("OrgName").ToString
                        txtAdress2.Text = MyReader("Address2").ToString
                        txtAdress1.Text = MyReader("Address1").ToString
                        txtAdress3.Text = MyReader("Address3").ToString
                        txtPhoneNo.Text = MyReader("PhoneNo").ToString
                        txtWebsite.Text = MyReader("Website").ToString
                        txtEmail.Text = MyReader("Email").ToString
                        txtOrgCode.Text = MyReader("OrgCode").ToString
                        txtCurrency.Text = MyReader("LocalCurrency").ToString
                        txtCurrencyCode.Text = MyReader("CurrencyCode").ToString
                    End If
            End Select
        Catch ex As Exception
        Finally
            If Not IsNothing(MyReader) Then
                MyReader.Close()
            End If
        End Try
    End Sub
    Function EmailAddressCheck(ByVal emailAddress As String) As Boolean
        'check email validity
        Dim pattern As String = "^[a-zA-Z][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$"
        Dim emailAddressMatch As Match = Regex.Match(emailAddress, pattern)
        If emailAddressMatch.Success Then
            EmailAddressCheck = True
        Else
            EmailAddressCheck = False
        End If
    End Function
    Private Function OrgcodeExist(ByVal strBranchcode As String) As Boolean
        'check if org code exists
        Dim MyDBAction As New DataManagement
        Dim MyTable As Data.DataTable
        Dim MySqlAction As String = "Select OrgCode from Organisation where OrgCode = '" & strBranchcode & "'"
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
                If CheckNumeric(txtOrgCode.Text) = False Then
                    lblMessage.Text = "Enter correct organization code, it must be numeric"
                    Exit Sub
                End If
                mystring = txtOrgCode.Text.ToUpper()
                If OrgcodeExist(txtOrgCode.Text.Trim) = True Then
                    lblMessage.Text = "The organization code exists,try another code!"
                    Exit Sub
                End If
                If txtEmail.Text <> "" Then
                    If EmailAddressCheck(txtEmail.Text) = True Then
                        lblMessage.Text = ""
                    Else
                        lblMessage.Text = "Enter the correct email address"
                        Exit Sub
                    End If
                End If
                Dim mySqlAction As String = ""
                Dim MyDBAction As New DataManagement
                mySqlAction = "Insert into Organisation(OrgCode,OrgName,Address1,Address2,Address3,PhoneNo,Website,Email,LocalCurrency,CurrencyCode)values('" & txtOrgCode.Text.Trim & "','" & txtOrgName.Text.ToUpper.Trim & "','" & txtAdress1.Text.ToUpper.Trim & "','" & txtAdress2.Text.ToUpper.Trim & "','" & txtAdress3.Text.ToUpper.Trim & "','" & txtPhoneNo.Text.Trim & "','" & txtWebsite.Text.Trim & "','" & txtEmail.Text.Trim & "', '" & txtCurrency.Text.ToUpper.Trim & "', '" & txtCurrencyCode.Text.ToUpper.Trim & "')" ','" & txtOrgName.Text.ToUpper & "','" & txtAdress2.Text.ToUpper & "','" & txtmobile.Text & "') "
                MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
                lblMessage.Text = "Organization details have been saved successfully"
                DisplayDetails("All")
                'AUDIT TRAIL-------
                'Dim audit As New DataManagement
                'audit.AddAdminAuditTrail(Session("UserName").ToString, "Action", "Successful Saving of branch  '" & txtOrgName.Text.ToUpper & "'", _
                'Request.Url.ToString, Session("ip").ToString, Session("sessionid").ToString)
                ''----END AUDIT TRAIL-----
                txtOrgCode.ReadOnly = False
            Else
                'update
                Dim MySqlaction1 As String
                Dim mySqlAction As String = ""
                Dim MyCurrencyName As String = ""
                Dim MyDBAction As New DataManagement
                MySqlaction1 = "update Organisation set OrgCode ='" & txtOrgCode.Text.Trim & "', OrgName ='" & txtOrgName.Text.ToUpper.Trim & "', Address1 ='" & txtAdress1.Text.ToUpper.Trim & "', Address2 ='" & txtAdress2.Text.ToUpper.Trim & "',  Address3 ='" & txtAdress3.Text.ToUpper.Trim & "',PhoneNo ='" & txtPhoneNo.Text.ToUpper.Trim & "' ,Website = '" & txtWebsite.Text.Trim & "', Email = '" & txtEmail.Text.Trim & "', LocalCurrency = '" & txtCurrency.Text.ToUpper.Trim & "', CurrencyCode = '" & txtCurrencyCode.Text.ToUpper.Trim & "' where OrgCode ='" & txtOrgCode.Text.Trim & "'"
                MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
                DisplayDetails("All")
                lblMessage.Text = "Organisation details have been updated successfully"
                txtOrgCode.ReadOnly = False
                'DisplayDetails("All")
                'AUDIT TRAIL-------
                'Dim audit As New DataManagement
                'audit.AddAdminAuditTrail(Session("UserName").ToString, "Action", "Successful Saving of branch  '" & txtOrgName.Text.ToUpper & "'", _
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
        Me.HiddenField1.Value = ""
        txtOrgCode.Enabled = True
        txtCurrency.Enabled = True
        GridView1.EditIndex = -1
        DisplayDetails("All")
        lblMessage.Text = ""
        ClearControls()
    End Sub
    Protected Sub btnClear_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnClear.Click
        'Clear controls
        ClearControls()
        txtOrgCode.ReadOnly = False
        GridView1.EditIndex = -1
        lblMessage.Text = ""
        DisplayDetails("All")
    End Sub
    Protected Sub txtAdress3_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs)
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
            If CheckNumeric(txtOrgCode.Text) = False Then
                lblMessage.Text = "Enter correct organization code, it must be numeric"
                Exit Sub
            End If
            If OrgcodeExist(txtOrgCode.Text.Trim) = False Then
                lblMessage.Text = "The organization code does not exist!"
                Exit Sub
            End If
            Dim mySqlAction As String = ""
            Dim MyDBAction As New DataManagement
            mySqlAction = "Delete from Organisation where OrgCode='" & HiddenField1.Value.ToString & "'"
            MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Delete)
            Me.lblMessage.Text = "Organization Details Have Been Deleted Successfully"
            DisplayDetails("All")
            ClearControls()
            ''AUDIT TRAIL-------
            'Dim audit As New DataManagement
            'audit.AddAdminAuditTrail(Session("UserName").ToString, "Action", "Successful Deletion of branch", _
            'Request.Url.ToString, Session("ip").ToString, Session("sessionid").ToString)
            ''----END AUDIT TRAIL-----
        Catch ex As Exception
        End Try
    End Sub
    Protected Sub txtOrgCode_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs)
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
        txtOrgName.Text = ""
        txtAdress2.Text = ""
        HiddenField1.Value = ""
        txtAdress3.Text = ""
        txtPhoneNo.Text = ""
        txtWebsite.Text = ""
        txtEmail.Text = ""
        txtOrgCode.Text = ""
        txtAdress1.Text = ""
        txtCurrency.Text = ""
        txtCurrencyCode.Text = ""
    End Sub
End Class