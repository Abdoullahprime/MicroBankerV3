Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Imports System.Data.SqlClient
Public Class ProductMaintenance
    Inherits System.Web.UI.Page
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim MyAcctDetails As String = ""
        If Not Page.IsPostBack Then
            GetProduct()
            TabContainer1.ActiveTabIndex = 0
            DisplayDetails("All")
            txtAccountCode.ReadOnly = False
        End If
    End Sub
    Private Sub GetProduct()
        'Load Products
        Try
            Dim MyDataTable As New Data.DataTable
            Dim MyDBAction As New DataManagement
            MyDataTable = TryCast(MyDBAction.DBAction("select ProductDefCode,ProductName from MB_ProductDefinitions", DataManagement.DBActionType.SqlDataTable), Data.DataTable)
            If MyDataTable IsNot Nothing Then
                With drpProduct
                    .Items.Clear()
                    .DataSource = MyDataTable
                    .DataTextField = MyDataTable.Columns("ProductName").ToString.Trim
                    .DataValueField = MyDataTable.Columns("ProductDefCode").ToString.Trim
                    .DataBind()
                    .Items.Add("")
                    .SelectedValue = ""
                End With
                MyDataTable = Nothing
            End If
        Catch ex As Exception
        End Try
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
                    mySqlAction = "select AccountCode,ProductDefCode,AccountPrefix,AccountName,DormancyDays,AssetGL,LiabilityGL,IncomeGL,ExpenseGL,ChargeGL,FeeGL,IntRate from tbSavingsAccountProduct"
                Case "Filter"
                    If RdoSearch.Items(0).Selected = True Then
                        Processfilter = txtSearch.Text.ToUpper
                        Processfilter = "%" & Processfilter & "%"
                        mySqlAction = "select AccountCode,ProductDefCode,AccountPrefix,AccountName,DormancyDays,AssetGL,LiabilityGL,IncomeGL,ExpenseGL,ChargeGL,FeeGL,IntRate from tbSavingsAccountProduct where AccountCode LIKE '" & Processfilter & "'"
                    End If
                    If RdoSearch.Items(1).Selected = True Then
                        Processfilter = txtSearch.Text.ToUpper
                        Processfilter = "%" & Processfilter & "%"
                        mySqlAction = "select AccountCode,ProductDefCode,AccountPrefix,AccountName,DormancyDays,AssetGL,LiabilityGL,IncomeGL,ExpenseGL,ChargeGL,FeeGL,IntRate from tbSavingsAccountProduct where AccountName LIKE '" & Processfilter & "'"
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
                    drpProduct.Enabled = False
                    txtAccountCode.Enabled = False
                    txtPrefix.Enabled = False
                    drpProduct.Enabled = False
                    ShowMatchDetails("Select   * From tbSavingsAccountProduct where AccountCode= '" & HiddenField1.Value.ToString & "' ", "ViewDetails")
                    TabContainer1.ActiveTabIndex = 0
                    DisplayDetails("All")
                Case "select"
                    drpProduct.Enabled = True
                    txtAccountCode.Enabled = True
                    txtPrefix.Enabled = True
                    drpProduct.Enabled = True
                    ShowMatchDetails("Select  * From tbSavingsAccountProduct where AccountCode= '" & HiddenField1.Value.ToString & "' ", "ViewDetails")
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
                        txtAccountCode.Text = MyReader("AccountCode").ToString
                        drpProduct.SelectedValue = MyReader("ProductDefCode").ToString
                        txtPrefix.Text = MyReader("AccountPrefix").ToString
                        txtAccountName.Text = MyReader("AccountName").ToString
                        txtDormancyDays.Text = MyReader("DormancyDays").ToString
                        drpglType.Text = MyReader("GLType").ToString
                        txtAssetGL.Text = MyReader("AssetGL").ToString
                        txtLiabilityGL.Text = MyReader("LiabilityGL").ToString
                        txtIncomeGL.Text = MyReader("IncomeGL").ToString
                        txtExpenseGL.Text = MyReader("ExpenseGL").ToString
                        txtChargeGL.Text = MyReader("ChargeGL").ToString
                        txtFeeGL.Text = MyReader("FeeGL").ToString
                        txtIntPayable.Text = MyReader("IntPayable").ToString
                        txtIntReceivable.Text = MyReader("IntReceivable").ToString
                        txtIntRate.Text = MyReader("IntRate").ToString
                        drpIntAppliedFreq.Text = MyReader("IntFreq").ToString
                        drpIntAccrualFreq.Text = MyReader("IntAccrualFreq").ToString
                        txtMinIntBal.Text = MyReader("MinInterestBal").ToString ',,,,
                        txtMinAccBal.Text = MyReader("MinAccountBal").ToString
                        txtDebitNoPerMnth.Text = MyReader("DebitsNoPerMonth").ToString
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
                mystring = txtAccountCode.Text.ToUpper()
                If ProductCodexist(txtAccountCode.Text.Trim) = True Then
                    lblMessage.Text = "The account type code exists,try another code!"
                    Exit Sub
                End If
                If CheckNumeric(txtPrefix.Text) = False Then
                    lblMessage.Text = "Enter correct product code,it must be numeric"
                    Exit Sub
                End If
                Dim mySqlAction As String = ""
                Dim MyDBAction As New DataManagement
                mySqlAction = "Insert into tbSavingsAccountProduct(AccountCode,AccountPrefix,ProductDefCode,AccountName,DormancyDays,GLType,AssetGL,LiabilityGL,IncomeGL,ExpenseGL,ChargeGL,FeeGL,IntReceivable,IntPayable,IntRate,IntFreq,IntAccrualFreq,MinInterestBal,MinAccountBal,DebitsNoPerMonth)values('" & txtAccountCode.Text.ToUpper.Trim & "','" & txtPrefix.Text.ToUpper.Trim & "','" & drpProduct.SelectedValue.ToUpper.Trim & "','" & txtAccountName.Text.ToUpper.Trim & "','" & txtDormancyDays.Text.ToUpper.Trim & "','" & drpglType.Text.ToUpper.Trim & "','" & txtAssetGL.Text.ToUpper.Trim & "','" & txtLiabilityGL.Text.ToUpper.Trim & "','" & txtIncomeGL.Text.ToUpper.Trim & "','" & txtExpenseGL.Text.ToUpper.Trim & "','" & txtChargeGL.Text.ToUpper.Trim & "', '" & txtFeeGL.Text.ToUpper.Trim & "', '" & txtIntReceivable.Text.ToUpper.Trim & "', '" & txtIntPayable.Text.ToUpper.Trim & "', '" & txtIntRate.Text.ToUpper.Trim & "', '" & drpIntAppliedFreq.Text.ToUpper.Trim & "', '" & drpIntAccrualFreq.Text.ToUpper.Trim & "', '" & txtMinIntBal.Text.ToUpper.Trim & "', '" & txtMinAccBal.Text.ToUpper.Trim & "', '" & txtDebitNoPerMnth.Text.ToUpper.Trim & "')" ','" & txtOrgName.Text.ToUpper & "','" & txtAdress2.Text.ToUpper & "','" & txtmobile.Text & "') "
                MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
                lblMessage.Text = "Product details have been saved successfully"
                DisplayDetails("All")
                'AUDIT TRAIL-------
                'Dim audit As New DataManagement
                'audit.AddAdminAuditTrail(Session("UserName").ToString, "Action", "Successful Saving of branch  '" & txtOrgName.Text.ToUpper & "'", _
                'Request.Url.ToString, Session("ip").ToString, Session("sessionid").ToString)
                ''----END AUDIT TRAIL-----
                txtAccountCode.ReadOnly = False
            Else
                'update
                If CheckNumeric(txtDormancyDays.Text) = False Then
                    lblMessage.Text = "Enter correct dormancy days,it must be numeric"
                    Exit Sub
                End If
                If CheckNumeric(txtDebitNoPerMnth.Text) = False Then
                    lblMessage.Text = "Enter correct no of debits,it must be numeric"
                    Exit Sub
                End If
                Dim MySqlaction1 As String
                Dim mySqlAction As String = ""
                Dim MyCurrencyName As String = ""
                Dim MyDBAction As New DataManagement
                MySqlaction1 = "update tbSavingsAccountProduct set AccountCode ='" & txtAccountCode.Text.ToUpper.Trim & "',AccountPrefix ='" & txtPrefix.Text.ToUpper.Trim & "',ProductDefCode ='" & drpProduct.SelectedValue.ToUpper.Trim & "', AccountName ='" & txtAccountName.Text.ToUpper.Trim & "', DormancyDays ='" & txtDormancyDays.Text.ToUpper.Trim & "',GLType = '" & drpglType.Text.ToUpper.Trim & "', AssetGL ='" & txtAssetGL.Text.ToUpper.Trim & "',  LiabilityGL ='" & txtLiabilityGL.Text.ToUpper.Trim & "',IncomeGL ='" & txtIncomeGL.Text.ToUpper.Trim & "' ,ExpenseGL = '" & txtExpenseGL.Text.ToUpper.Trim & "', ChargeGL = '" & txtChargeGL.Text.ToUpper.Trim & "', FeeGL = '" & txtFeeGL.Text.ToUpper.Trim & "', IntReceivable = '" & txtIntReceivable.Text.ToUpper.Trim & "', IntPayable = '" & txtIntPayable.Text.ToUpper.Trim & "', IntRate = '" & txtIntRate.Text.ToUpper.Trim & "',IntFreq = '" & drpIntAppliedFreq.Text.ToUpper.Trim & "',IntAccrualFreq = '" & drpIntAccrualFreq.Text.ToUpper.Trim & "',MinInterestBal = '" & txtMinIntBal.Text.ToUpper.Trim & "',MinAccountBal = '" & txtMinAccBal.Text.ToUpper.Trim & "',DebitsNoPerMonth = '" & txtDebitNoPerMnth.Text.ToUpper.Trim & "' where AccountCode ='" & txtAccountCode.Text & "'"
                MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
                DisplayDetails("All")
                lblMessage.Text = "Product details have been updated successfully"
                txtAccountCode.ReadOnly = False
                'DisplayDetails("All")
                'AUDIT TRAIL-------
                'Dim audit As New DataManagement
                'audit.AddAdminAuditTrail(Session("UserName").ToString, "Action", "Successful Saving of branch  '" & txtOrgName.Text.ToUpper & "'", _
                'Request.Url.ToString, Session("ip").ToString, Session("sessionid").ToString)
                ''----END AUDIT TRAIL-----
            End If
        Catch ex As Exception
        End Try
        txtAccountCode.Text = ""
        txtAccountName.Text = ""
        txtAssetGL.Text = ""
        HiddenField1.Value = ""
        txtLiabilityGL.Text = ""
        txtIncomeGL.Text = ""
        txtExpenseGL.Text = ""
        txtChargeGL.Text = ""
        txtIntPayable.Text = ""
        txtIntReceivable.Text = ""
        txtDormancyDays.Text = ""
        txtFeeGL.Text = ""
        txtIntRate.Text = ""
        drpIntAccrualFreq.Text = ""
        drpIntAppliedFreq.Text = ""
        txtPrefix.Text = ""
        txtMinIntBal.Text = ""
        txtMinAccBal.Text = ""
        txtDebitNoPerMnth.Text = ""
        txtPrefix.Text = ""
        txtMinIntBal.Text = ""
        txtMinAccBal.Text = ""
        txtDebitNoPerMnth.Text = ""
        drpglType.Text = ""
        drpProduct.Text = ""
    End Sub
    Private Function ProductCodexist(ByVal AccountCode As String) As Boolean
        'check if branch code exists
        Dim MyDBAction As New DataManagement
        Dim MyTable As Data.DataTable
        Dim MySqlAction As String = "select AccountCode from tbSavingsAccountProduct Where AccountCode = '" & AccountCode & "'"
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
    Protected Sub DropDownList1_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        DisplayDetails("All")
    End Sub
    Protected Sub CmdApprovalList_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles CmdApprovalList.Click
        'Reset input controls
        Me.HiddenField1.Value = ""
        drpProduct.Enabled = True
        drpProduct.Text = ""
        txtAccountCode.Enabled = True
        txtPrefix.Enabled = True
        txtAccountName.Text = ""
        txtAssetGL.Text = ""
        txtAccountCode.Text = ""
        txtAccountName.Text = ""
        txtAssetGL.Text = ""
        HiddenField1.Value = ""
        txtLiabilityGL.Text = ""
        txtIncomeGL.Text = ""
        txtExpenseGL.Text = ""
        txtChargeGL.Text = ""
        txtIntReceivable.Text = ""
        txtIntPayable.Text = ""
        txtAccountCode.Text = ""
        txtDormancyDays.Text = 0
        txtFeeGL.Text = ""
        Me.txtIntRate.Text = 0.00
        drpIntAppliedFreq.Text = ""
        drpIntAccrualFreq.Text = ""
        txtAccountCode.ReadOnly = False
        GridView1.EditIndex = -1
        txtPrefix.Text = ""
        txtMinIntBal.Text = 0.00
        txtMinAccBal.Text = 0.00
        txtDebitNoPerMnth.Text = 0
        drpglType.Text = ""
        lblMessage.Text = ""
    End Sub
    Protected Sub btnClear_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnClear.Click
        'Clear controls
        txtAccountCode.Text = ""
        txtAccountName.Text = ""
        txtAssetGL.Text = ""
        HiddenField1.Value = ""
        txtLiabilityGL.Text = ""
        txtIncomeGL.Text = ""
        txtExpenseGL.Text = ""
        txtChargeGL.Text = ""
        txtIntPayable.Text = ""
        txtIntReceivable.Text = ""
        txtAccountCode.Text = ""
        txtDormancyDays.Text = ""
        txtFeeGL.Text = ""
        txtIntRate.Text = ""
        drpIntAppliedFreq.Text = ""
        drpIntAccrualFreq.Text = ""
        'txtAccountCode.ReadOnly = True
        'GridView1.EditIndex = -1
        txtPrefix.Text = ""
        txtMinIntBal.Text = ""
        txtMinAccBal.Text = ""
        txtDebitNoPerMnth.Text = ""
        drpglType.Text = ""
        drpProduct.Enabled = True
        txtAccountCode.Enabled = True
        txtPrefix.Enabled = True
        drpProduct.Enabled = True
        drpProduct.Text = ""
        txtMinIntBal.Text = 0.00
        txtMinAccBal.Text = 0.00
        txtDebitNoPerMnth.Text = 0
        Me.txtIntRate.Text = 0.00
        txtDormancyDays.Text = 0
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
            If ProductCodexist(txtAccountCode.Text.Trim) = False Then
                lblMessage.Text = "The account type code does not exists,try another code!"
                Exit Sub
            End If
            Dim mySqlAction As String = ""
            Dim MyDBAction As New DataManagement
            mySqlAction = "Delete from tbSavingsAccountProduct where AccountCode='" & HiddenField1.Value.ToString & "'"
            MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Delete)
            Me.lblMessage.Text = "Product details have been deleted Successfully."
            DisplayDetails("All")
            txtAccountCode.Text = ""
            txtAccountName.Text = ""
            txtAssetGL.Text = ""
            HiddenField1.Value = ""
            txtLiabilityGL.Text = ""
            txtIncomeGL.Text = ""
            txtExpenseGL.Text = ""
            txtChargeGL.Text = ""
            txtIntPayable.Text = ""
            txtIntReceivable.Text = ""
            txtAccountCode.Text = ""
            txtDormancyDays.Text = ""
            txtFeeGL.Text = ""
            txtIntRate.Text = ""
            drpIntAppliedFreq.Text = ""
            drpIntAccrualFreq.Text = ""
            txtPrefix.Text = ""
            txtMinIntBal.Text = ""
            txtMinAccBal.Text = ""
            txtDebitNoPerMnth.Text = ""
            drpglType.Text = ""
            drpProduct.Text = ""
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
    Protected Sub txtAccountCode_TextChanged(sender As Object, e As EventArgs) Handles txtAccountCode.TextChanged
    End Sub
    Protected Sub txtPrefix_TextChanged(sender As Object, e As EventArgs) Handles txtPrefix.TextChanged
    End Sub
End Class