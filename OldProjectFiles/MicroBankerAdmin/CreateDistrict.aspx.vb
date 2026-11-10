Imports System.Data.SqlClient
Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Public Class CreateDistrict
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim MyAcctDetails As String = ""
        If Not Page.IsPostBack Then
            'PopulateClients()
            'GetId()
            'MultiView1.SetActiveView(View1)
            TabContainer1.ActiveTabIndex = 0
            DisplayDetails("All")
            'DisplayDetails2("All")
            txtDistrictCode.ReadOnly = False
        End If
        ' ShowAccountDetails(Session("AcctDetais"))
        ' ShowSignatoryGrid(Session("SignatoryDetails"))
    End Sub
    Public Function LogError(ByVal strErrorMessage As String, ByVal strErrorID As String, ByVal strModuleName As String, ByVal strMethodName As String, ByVal strTimeOccur As Date, ByVal strOperation As String) As Boolean
        Try
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
            Dim mySqlAction As String = ""
            Select Case ShowType
                Case "All"

                    mySqlAction = "select DistrictCode,DistrictName from District"

                Case "Filter"
                    If RdoSearch.Items(0).Selected = True Then
                        Processfilter = txtSearch.Text.ToUpper
                        Processfilter = "%" & Processfilter & "%"

                        mySqlAction = "select DistrictCode,DistrictName from District where DistrictName LIKE '" & Processfilter & "'"
                    End If

                    If RdoSearch.Items(1).Selected = True Then
                        Processfilter = txtSearch.Text.ToUpper
                        Processfilter = "%" & Processfilter & "%"

                        mySqlAction = "select DistrictCode,DistrictName from District where DistrictCode LIKE '" & Processfilter & "'"
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

            HiddenField1.Value = ""

            Dim MyID As String = e.CommandArgument

            HiddenField1.Value = MyID

            Select Case e.CommandName
                Case "edit"
                    txtDistrictCode.Enabled = False
                    ShowMatchDetails("Select   * From District where DistrictCode= '" & HiddenField1.Value.ToString & "' ", "ViewDetails")
                    TabContainer1.ActiveTabIndex = 0
                    DisplayDetails("All")

                Case "select"
                    txtDistrictCode.Enabled = True
                    ShowMatchDetails("Select  * From District where DistrictCode= '" & HiddenField1.Value.ToString & "' ", "ViewDetails")
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

                    If MyReader.HasRows Then
                        MyReader.Read()

                        txtDistrictName.Text = MyReader("DistrictName").ToString
                        txtDistrictCode.Text = MyReader("DistrictCode").ToString


                        '(commissionAc)values('" & txtEmail.Text.ToUpper & "','" & txtDistrictCode.Text.ToUpper & "')" ','" & txtDistrictName.Text.ToUpper & "','" & txtAdress2.Text.ToUpper & "','" & txtmobile.Text & "') "


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

        Dim pattern As String = "^[a-zA-Z][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$"
        Dim emailAddressMatch As Match = Regex.Match(emailAddress, pattern)
        If emailAddressMatch.Success Then
            EmailAddressCheck = True
        Else
            EmailAddressCheck = False
        End If

    End Function

    Private Function ACCOUNTExist(ByVal COMMISSIONAC As String) As Boolean
        Dim MyDBAction As New DataManagement
        Dim MyTable As Data.DataTable

        Dim MySqlAction As String = "select COMMISSIONAC from BRANCHES where COMMISSIONAC = '" & COMMISSIONAC & "'"

        MyTable = TryCast(MyDBAction.DBAction(MySqlAction, DataManagement.DBActionType.SqlDataTable), Data.DataTable)
        If MyTable.Rows.Count > 0 Then
            Return True
        Else
            Return False
        End If

    End Function
    Private Function BranchcodeExist(ByVal strBranchcode As String) As Boolean
        Dim MyDBAction As New DataManagement
        Dim MyTable As Data.DataTable

        Dim MySqlAction As String = "Select BRANCHCODE from BRANCHES where BRANCHCODE = '" & strBranchcode & "'"

        MyTable = TryCast(MyDBAction.DBAction(MySqlAction, DataManagement.DBActionType.SqlDataTable), Data.DataTable)
        If MyTable.Rows.Count > 0 Then
            Return True
        Else
            Return False
        End If

    End Function
    Private Function BranchNameExist(ByVal strUserName As String) As Boolean
        Dim MyDBAction As New DataManagement
        Dim MyTable As Data.DataTable

        Dim MySqlAction As String = "Select BRANCHNAME from BRANCHES where BRANCHNAME = '" & strUserName & "'"

        MyTable = TryCast(MyDBAction.DBAction(MySqlAction, DataManagement.DBActionType.SqlDataTable), Data.DataTable)
        If MyTable.Rows.Count > 0 Then
            Return True
        Else
            Return False
        End If

    End Function
    Function chalo(ByVal a As String)
        Dim cow As Boolean = False
        Dim ContainsAlpha As Boolean
        Dim ContainsNumeric As Boolean
        Dim ContainsInvalidCharacter As Boolean
        For Each Character As Char In a
            If Char.IsNumber(Character) Then
                cow = True
                ContainsNumeric = True
            ElseIf Char.IsLetter(Character) Then
                cow = False
                ContainsAlpha = True
                Exit For
            ElseIf Not Char.IsWhiteSpace(Character) Then
                cow = False
                ContainsInvalidCharacter = True
                Exit For
            End If
        Next
        'If ContainsInvalidCharacter Then
        '    'Contains a non alpha or numeric character
        'ElseIf Not ContainsAlpha OrElse Not ContainsNumeric Then
        '    'Doesn't have at least one of each.
        'End If

        Return cow
    End Function

    Protected Sub Button1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Button1.Click

        Try
            If Me.HiddenField1.Value = "" Then

                Dim mystring As String = ""
                mystring = txtDistrictCode.Text.ToUpper()
                'txtDistrictCode.Text = mystring
                'If Mid(mystring, 1, 4).Contains("01M8") = True Then
                '    Label4.Text = ""

                'Else
                '    Label4.Text = "This is not the correct format of a commission account"
                '    Exit Sub
                'End If

                'If BranchcodeExist(txtAdress1.Text.Trim) = True Then
                '    Label4.Text = "The branch code Exists,try another one!"
                '    Exit Sub
                'ElseIf chalo(txtAdress1.Text) = False Then
                '    Label4.Text = "Enter Correct Branch Code Details"
                '    txtAdress1.Text = ""
                '    Exit Sub

                'ElseIf txtAdress1.Text.Length < 3 = True Then
                '    Label4.Text = "bank code too short"
                '    Exit Sub
                'Else
                '    Label4.Text = ""
                'End If

                'If chalo(txtAdress1.Text) = False Then
                '    txtAdress1.Text = ""
                '    Label4.Text = "Enter Correct Branch Code Details"
                '    Exit Sub
                'ElseIf txtAdress1.Text.Length < 3 = True Then
                '    Label4.Text = "bank code too short"
                '    Exit Sub
                'Else
                '    Label4.Text = ""
                'End If

                'If BranchNameExist(txtDistrictName.Text) = True Then
                '    Label4.Text = "The branch Exists,try another one!"
                '    Exit Sub
                'ElseIf txtDistrictName.Text.Length < 3 = True Then
                '    Label4.Text = "too short branch name"
                '    Exit Sub
                'Else
                '    Label4.Text = ""
                'End If
                'If txtAdress2.Text.Length < 3 = True Then
                '    Label4.Text = "too short ContactPerson name"
                '    Exit Sub
                'Else
                '    Label4.Text = ""
                'End If

                'If txtDistrictName.Text = "" Or txtAdress2.Text = "" Then
                '    Me.Label4.Text = "Please enter all the details"
                '    Exit Sub
                'End If


                'If EmailAddressCheck(txtAdress3.Text) = True Then
                '    Label4.Text = ""
                '    'Exit Sub
                'Else
                '    Label4.Text = "Enter the Correct Email Address"
                '    'Label15.Text = "Email Address"
                '    Exit Sub
                'End If

                'If txtPhoneNo.Text.Length < 3 = True Then
                '    Label4.Text = "too short Branch Address"
                'Else
                '    Label4.Text = ""
                'End If

                'If chalo(txtEmail.Text) = False Then
                '    Label4.Text = "Enter correct Phone Number, it must be numeric"
                '    Exit Sub
                'ElseIf txtEmail.Text.Length < 5 Or txtEmail.Text.Length > 10 = True Then
                '    Label4.Text = "phone number length incorrect, must be 10 digits for mobile and 5 for office extention"
                '    Exit Sub
                'Else

                '    Label4.Text = ""
                '    'RequiredFieldValidator5.Display="Dynamic" CssClass="Validator" SetFocusOnError="true" ErrorMessage = "format e.g 07********"
                '    'Exit Sub
                'End If

                'If ACCOUNTExist(txtDistrictCode.Text) Then
                '    Label4.Text = "This commission account number Already exists"
                '    Exit Sub
                'End If

                ''If chalo(txtDistrictCode.Text) = False Then
                ''    Label4.Text = "Enter correct commission account"
                ''    Exit Sub
                ''ElseIf txtDistrictCode.Text.Length < 14 Or txtEmail.Text.Length > 14 = True Then
                ''    Label4.Text = "commission account number length must be 14 digits"
                ''    Exit Sub
                ''Else

                ''    Label4.Text = ""
                ''    'RequiredFieldValidator5.Display="Dynamic" CssClass="Validator" SetFocusOnError="true" ErrorMessage = "format e.g 07********"
                ''    'Exit Sub
                ''End If
                'Dim mystring03 As String = ""
                'mystring03 = txtCurrency.Text.ToUpper()


                'If Mid(mystring03, 1, 4).Contains("0124") = True Then
                '    Label4.Text = ""

                'Else
                '    Label4.Text = "This is not the correct format for an Excise duty account, it must start with 0124"
                '    Exit Sub
                'End If


                'If chalo(txtCurrency.Text) = False Then
                '    Label4.Text = "Enter correct Excise duty account, it must be numeric starting 0124"
                '    Exit Sub
                'ElseIf txtCurrency.Text.Length < 14 Or txtEmail.Text.Length > 14 = True Then
                '    Label4.Text = "Excise duty  account number length must be 14 digits"
                '    Exit Sub
                'Else

                '    Label4.Text = ""
                '    'RequiredFieldValidator5.Display="Dynamic" CssClass="Validator" SetFocusOnError="true" ErrorMessage = "format e.g 07********"
                '    'Exit Sub
                'End If


                ' If HiddenField1.Value = "" Then

                Dim mySqlAction As String = ""
                Dim MyDBAction As New DataManagement
                mySqlAction = "Insert into District (DistrictCode,DistrictName)values('" & txtDistrictCode.Text.Trim & "','" & txtDistrictName.Text.ToUpper.Trim & "')" ','" & txtDistrictName.Text.ToUpper & "','" & txtAdress2.Text.ToUpper & "','" & txtmobile.Text & "') "
                MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
                Label4.Text = "District Details have been saved successfully"
                'AutoIncrementId()
                DisplayDetails("All")
                'AUDIT TRAIL-------
                'Dim audit As New DataManagement
                'audit.AddAdminAuditTrail(Session("UserName").ToString, "Action", "Successful Saving of branch  '" & txtDistrictName.Text.ToUpper & "'", _
                'Request.Url.ToString, Session("ip").ToString, Session("sessionid").ToString)
                ''----END AUDIT TRAIL-----
                txtDistrictCode.ReadOnly = False

            Else
                'update
                Dim MySqlaction1 As String
                Dim mySqlAction As String = ""
                Dim MyCurrencyName As String = ""
                Dim MyDBAction As New DataManagement
                MySqlaction1 = "update District set DistrictCode ='" & txtDistrictCode.Text.Trim & "', DistrictName ='" & txtDistrictName.Text.ToUpper.Trim & "'  where DistrictCode ='" & HiddenField1.Value.ToString & "'"
                MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
                DisplayDetails("All")
                Label4.Text = "District details have been Updated successfully"

                txtDistrictCode.ReadOnly = False

                'DisplayDetails("All")
                'AUDIT TRAIL-------
                'Dim audit As New DataManagement
                'audit.AddAdminAuditTrail(Session("UserName").ToString, "Action", "Successful Saving of branch  '" & txtDistrictName.Text.ToUpper & "'", _
                'Request.Url.ToString, Session("ip").ToString, Session("sessionid").ToString)
                ''----END AUDIT TRAIL-----


            End If
        Catch ex As Exception

        End Try

        txtDistrictName.Text = ""
        HiddenField1.Value = ""
        txtDistrictCode.Text = ""
    End Sub

    Protected Sub DropDownList1_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        'populateClientId()
        DisplayDetails("All")
    End Sub

    Protected Sub CmdApprovalList_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles CmdApprovalList.Click
        Me.HiddenField1.Value = ""
        txtDistrictName.Text = ""
        txtDistrictName.Text = ""
        HiddenField1.Value = ""
        txtDistrictCode.Text = ""
        txtDistrictCode.ReadOnly = False
        txtDistrictCode.Enabled = True
        GridView1.EditIndex = -1
        DisplayDetails("All")

    End Sub

    Protected Sub Button2_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Button2.Click
        txtDistrictName.Text = ""
        HiddenField1.Value = ""
        txtDistrictCode.Text = ""

        txtDistrictCode.ReadOnly = False

        GridView1.EditIndex = -1
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
            Dim mySqlAction As String = ""
            Dim MyDBAction As New DataManagement

            mySqlAction = "Delete from District where DistrictCode='" & HiddenField1.Value.ToString & "'"

            MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Delete)

            'mySqlAction = "Update  Region set APPROVED = 3, remarks = '" & txtDistrictCode.Text.ToString.Replace("'", "") & "'  where BRANCHCODE= '" & HiddenField1.Value.ToString & "' "
            'MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Update)

            Me.Label4.Text = "District/Municipality Details Have Been Deleted Successfully, Awaiting Approval"
            DisplayDetails("All")
            txtDistrictName.Text = ""
            HiddenField1.Value = ""
            txtDistrictCode.Text = ""
            'Label4.Text = ""
            'Else
            'ShowGridDetails("ShowAll")
            'Me.Label4.Text = ""
            'End If

            ''AUDIT TRAIL-------
            'Dim audit As New DataManagement
            'audit.AddAdminAuditTrail(Session("UserName").ToString, "Action", "Successful Deletion of branch", _
            'Request.Url.ToString, Session("ip").ToString, Session("sessionid").ToString)
            ''----END AUDIT TRAIL-----

        Catch ex As Exception

        End Try

    End Sub

    Protected Sub txtDistrictCode_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        'txtDistrictName.Text = ""
        'Label4.Text = ""
        'Try

        '    Dim MyService As New checkBfub.BFUBEnquiries
        '    Dim a As String = MyService.AccountBalanceEnq(txtDistrictCode.Text).branchName
        '    Dim b As String = MyService.AccountBalanceEnq(txtDistrictCode.Text).branchSortCode
        '    'Dim c As String = MyService.AccountBalanceEnq(txtsuspenceacc.Text).branchName
        '    txtDistrictName.Text = a
        '    'txtAdress1.Text = b

        '    b = b.Substring(5, 3)
        '    txtAdress1.Text = b
        '    txtDistrictCode.ReadOnly = True

        '    If b = "" Then
        '        Label4.Text = "The Commission A/c does not exist "
        '        txtDistrictCode.ReadOnly = False
        '    End If
        '    'DropDownList1.Text = c

        'Catch ex As Exception
        '    txtDistrictName.Text = ""
        '    txtAdress1.Text = ""
        '    ' DropDownList1.Text = ""
        '    txtDistrictCode.ReadOnly = False
        '    Label4.Text = "Error Pulling Account Details, Commission A/c must start 01M8 .... E6 "
        'End Try
    End Sub

    Protected Sub getAcc()
        txtDistrictName.Text = ""
        Label4.Text = ""
        Try

            'Dim MyService As New checkBfub.BFUBEnquiries
            'Dim a As String = MyService.AccountBalanceEnq(txtDistrictCode.Text).branchName
            'Dim b As String = MyService.AccountBalanceEnq(txtDistrictCode.Text).branchSortCode
            'Dim c As String = MyService.AccountBalanceEnq(txtsuspenceacc.Text).branchName
            'txtDistrictName.Text = a

            ''txtAdress1.Text = b

            'b = b.Substring(5, 3)
            'txtAdress1.Text = b
            'txtDistrictCode.ReadOnly = True

            ''b = "099"
            ''txtAdress1.Text = b
            ''txtDistrictCode.ReadOnly = True

            'If b = "" Then
            '    Label4.Text = "The Commission A/c does not exist "
            '    txtDistrictCode.ReadOnly = False
            'End If
            'DropDownList1.Text = c

        Catch ex As Exception
            'txtDistrictName.Text = ""
            'txtAdress1.Text = ""
            '' DropDownList1.Text = ""
            'txtDistrictCode.ReadOnly = False
            'Label4.Text = "Error Pulling Account Details, Commission A/c must start 01M8 .... E6 "
        End Try
    End Sub

    Protected Sub cmasearch_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        Dim mystring As String = ""
        mystring = txtDistrictCode.Text.ToUpper()
        If Mid(mystring, 1, 4).Contains("01M8") = True Then
            Label4.Text = ""

            getAcc()

            Exit Sub
        Else
            ' Label4.Text = "This is not the correct format of a commission account"
        End If
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