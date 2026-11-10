Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Imports System.Data.SqlClient
Public Class CreateTransactionCode
    Inherits System.Web.UI.Page
    'page load
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim MyAcctDetails As String = ""
        If Not Page.IsPostBack Then
            TabContainer1.ActiveTabIndex = 0
            DisplayDetails("All")
            txtTxnCode.ReadOnly = False
        End If
    End Sub
Public Function LogError(ByVal strErrorMessage As String, ByVal strErrorID As String, ByVal strModuleName As String, ByVal strMethodName As String, ByVal strTimeOccur As Date, ByVal strOperation As String) As Boolean
        'Error logging
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
        'pre load data
        Dim searchStr As String = ""
        Dim SqlAction As String = ""
        Dim Processfilter As String = ""
        Dim ApprovalStatus As String = ""
        Try
            Dim mySqlAction As String = ""
            Select Case ShowType
                Case "All"
                    mySqlAction = "select TxnCode,TxnDesc,TxnType,ChargeAmt,ChqNo,TxnCash,PostMode,ContraGL,TelleID,AttachedTxnCode from tbTransactionCode"
                Case "Filter"
                    If RdoSearch.Items(0).Selected = True Then
                        Processfilter = txtSearch.Text.ToUpper
                        Processfilter = "%" & Processfilter & "%"
                        mySqlAction = "select TxnCode,TxnDesc,TxnType,ChargeAmt,ChqNo,TxnCash,PostMode,ContraGL,TelleID,AttachedTxnCode from tbTransactionCode where TxnCode LIKE '" & Processfilter & "'"
                    End If
                    If RdoSearch.Items(1).Selected = True Then
                        Processfilter = txtSearch.Text.ToUpper
                        Processfilter = "%" & Processfilter & "%"
                        mySqlAction = "select TxnCode,TxnDesc,TxnType,ChargeAmt,ChqNo,TxnCash,PostMode,ContraGL,TelleID,AttachedTxnCode from tbTransactionCode where TxnDesc LIKE '" & Processfilter & "'"
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
                    txtTxnCode.Enabled = False
                    txtTelleID.Enabled = False
                    ShowMatchDetails("Select   * From tbTransactionCode where TxnCode= '" & HiddenField1.Value.ToString & "' ", "ViewDetails")
                    TabContainer1.ActiveTabIndex = 0
                    DisplayDetails("All")
                Case "select"
                    txtTxnCode.Enabled = True
                    txtTelleID.Enabled = True
                    ShowMatchDetails("Select  * From tbTransactionCode where TxnCode= '" & HiddenField1.Value.ToString & "' ", "ViewDetails")
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
            'Load data from gridview 
            Select Case ShowType
                Case "ViewDetails"
                    If MyReader.HasRows Then
                        MyReader.Read()
                        txtTxnDesc.Text = MyReader("TxnDesc").ToString
                        txtChargeAmt.Text = MyReader("ChargeAmt").ToString
                        drpDRCR.Text = MyReader("TxnType").ToString
                        drpChqNoReqrd.Text = MyReader("ChqNo").ToString
                        drpCashTrnx.Text = MyReader("TxnCash").ToString
                        drpPostMode.Text = MyReader("PostMode").ToString
                        txtContraGL.Text = MyReader("ContraGL").ToString
                        txtTxnCode.Text = MyReader("TxnCode").ToString
                        txtTelleID.Text = MyReader("TelleID").ToString
                        txtAttachedTxnCode.Text = MyReader("AttachedTxnCode").ToString
                        txtRefCode.Text = MyReader("RefCode").ToString
                        txtAccType.Text = MyReader("AccountType").ToString
                        Dim checkbox As Boolean = Convert.ToBoolean(MyReader("LoanDisbursement"))
                        If checkbox = True Then
                            chkLoan.Checked = True
                        Else
                            chkLoan.Checked = False
                        End If
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
                mystring = txtTxnCode.Text.ToUpper()
                Dim MySmsObject As New DataManagement
                Dim mySearch As String = "select TxnCode from tbTransactionCode Where TxnCode ='" & txtTxnCode.Text.Trim & "'"
                Dim MyReader As SqlDataReader = TryCast(MySmsObject.DBAction(mySearch, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
                If MyReader.HasRows = True Then
                    Label4.Text = "This Transaction Code Already Exist"
                    Exit Sub
                Else
                End If
                MyReader.Close()
                If txtChargeAmt.Text = "" Then
                    txtChargeAmt.Text = 0
                End If
                Dim mySqlAction As String = ""
                Dim MyDBAction As New DataManagement
                mySqlAction = "Insert into tbTransactionCode(TxnCode,TxnDesc,TxnType,ChargeAmt,ChqNo,TxnCash,PostMode,ContraGL,TelleID,AttachedTxnCode,RefCode,AccountType,LoanDisbursement)values('" & txtTxnCode.Text.ToUpper.Trim & "','" & txtTxnDesc.Text.ToUpper.Trim & "','" & drpDRCR.Text.ToUpper.Trim & "','" & txtChargeAmt.Text.ToUpper.Trim & "','" & drpChqNoReqrd.Text.ToUpper.Trim & "','" & drpCashTrnx.Text.ToUpper.Trim & "','" & drpPostMode.Text.ToUpper.Trim & "','" & txtContraGL.Text.ToUpper.Trim & "', '" & txtTelleID.Text.ToUpper.Trim & "', '" & txtAttachedTxnCode.Text.ToUpper.Trim & "','" & txtRefCode.Text.ToUpper.Trim & "','" & txtAccType.Text.ToUpper.Trim & "','" & chkLoan.Checked.ToString & "')"
                MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
                Label4.Text = "Transaction Code Details have been saved successfully"
                DisplayDetails("All")
                'AUDIT TRAIL-------
                'Dim audit As New DataManagement
                'audit.AddAdminAuditTrail(Session("UserName").ToString, "Action", "Successful Saving of branch  '" & txtOrgName.Text.ToUpper & "'", _
                'Request.Url.ToString, Session("ip").ToString, Session("sessionid").ToString)
                ''----END AUDIT TRAIL-----
                txtTxnCode.ReadOnly = False
            Else
                'update after edit
                Dim MySqlaction1 As String
                Dim mySqlAction As String = ""
                Dim MyCurrencyName As String = ""
                Dim MyDBAction As New DataManagement
                MySqlaction1 = "update tbTransactionCode set TxnCode ='" & txtTxnCode.Text.ToUpper.Trim & "', TxnDesc ='" & txtTxnDesc.Text.ToUpper.Trim & "', TxnType ='" & drpDRCR.Text.ToUpper.Trim & "', ChargeAmt ='" & txtChargeAmt.Text.ToUpper.Trim & "',  ChqNo ='" & drpChqNoReqrd.Text.ToUpper.Trim & "',TxnCash ='" & drpCashTrnx.Text.ToUpper.Trim & "' ,PostMode = '" & drpPostMode.Text.ToUpper.Trim & "', ContraGL = '" & txtContraGL.Text.ToUpper.Trim & "', TelleID = '" & txtTelleID.Text.ToUpper.Trim & "', AttachedTxnCode = '" & txtAttachedTxnCode.Text.ToUpper.Trim & "', RefCode = '" & txtRefCode.Text.ToUpper.Trim & "',AccountType = '" & txtAccType.Text.ToUpper.Trim & "',LoanDisbursement = '" & chkLoan.Checked.ToString & "' where TxnCode ='" & txtTxnCode.Text.Trim & "'"
                MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
                DisplayDetails("All")
                Label4.Text = "Transaction code details have been Updated successfully"
                txtTxnCode.ReadOnly = False
                'DisplayDetails("All")
                'AUDIT TRAIL-------
                'Dim audit As New DataManagement
                'audit.AddAdminAuditTrail(Session("UserName").ToString, "Action", "Successful Saving of branch  '" & txtOrgName.Text.ToUpper & "'", _
                'Request.Url.ToString, Session("ip").ToString, Session("sessionid").ToString)
                ''----END AUDIT TRAIL-----
            End If
        Catch ex As Exception
        End Try
        txtTxnDesc.Text = ""
        txtChargeAmt.Text = ""
        HiddenField1.Value = ""
        txtRefCode.Text = ""
        drpChqNoReqrd.Text = ""
        drpCashTrnx.Text = ""
        drpPostMode.Text = ""
        txtContraGL.Text = ""
        txtTxnCode.Text = ""
        drpDRCR.Text = ""
        txtTelleID.Text = ""
        txtAttachedTxnCode.Text = ""
        txtAccType.Text = ""
        chkLoan.Checked = False
    End Sub

    Protected Sub DropDownList1_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        DisplayDetails("All")
    End Sub
    Protected Sub CmdApprovalList_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        'Reset input controls
        Me.HiddenField1.Value = ""
        txtTxnDesc.Text = ""
        txtChargeAmt.Text = ""
        txtTxnDesc.Text = ""
        txtChargeAmt.Text = ""
        HiddenField1.Value = ""
        drpChqNoReqrd.Text = ""
        drpCashTrnx.Text = ""
        drpPostMode.Text = ""
        txtContraGL.Text = ""
        txtRefCode.Text = ""
        txtTxnCode.Text = ""
        txtTxnCode.Enabled = True
        txtTelleID.Enabled = True
        drpDRCR.Text = ""
        txtTelleID.Text = ""
        txtAttachedTxnCode.Text = ""
        txtAccType.Text = ""
        chkLoan.Checked = False
    End Sub

    Protected Sub btnClear_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnClear.Click
        'Clear controls
        txtTxnDesc.Text = ""
        txtChargeAmt.Text = ""
        HiddenField1.Value = ""
        txtRefCode.Text = ""
        drpChqNoReqrd.Text = ""
        drpCashTrnx.Text = ""
        drpPostMode.Text = ""
        txtContraGL.Text = ""
        txtTxnCode.Text = ""
        drpDRCR.Text = ""
        txtTelleID.Text = ""
        txtAttachedTxnCode.Text = ""
        txtTxnCode.ReadOnly = False
        GridView1.EditIndex = -1
        txtAccType.Text = ""
        chkLoan.Checked = False
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
        'Delete record
        Try
            Dim mySqlAction As String = ""
            Dim MyDBAction As New DataManagement
            mySqlAction = "Delete from tbTransactionCode where TxnCode='" & HiddenField1.Value.ToString & "'"
            MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Delete)
            Me.Label4.Text = "Transaction Code Details Have Been Deleted Successfully"
            DisplayDetails("All")
            txtTxnDesc.Text = ""
            txtChargeAmt.Text = ""
            HiddenField1.Value = ""
            txtRefCode.Text = ""
            drpChqNoReqrd.Text = ""
            drpCashTrnx.Text = ""
            drpPostMode.Text = ""
            txtContraGL.Text = ""
            txtTxnCode.Text = ""
            drpDRCR.Text = ""
            txtTelleID.Text = ""
            txtAttachedTxnCode.Text = ""
            txtAccType.Text = ""
            chkLoan.Checked = False
        Catch ex As Exception
        End Try
    End Sub

    Protected Sub txtOrgCode_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs)

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