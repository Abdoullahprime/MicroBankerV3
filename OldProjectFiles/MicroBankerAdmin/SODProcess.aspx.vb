Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Imports System.Data.SqlClient
Public Class SODProcess
    Inherits System.Web.UI.Page
    Public P_AccountNo As String = "", P_CustomerNo As String = "", P_AccountName As String = "", P_AssetGL_Control As String = "", P_LiabilityGL_Control As String = "", P_BranchCode As String = "", P_AvailBal As Decimal = 0.00, P_BookBalance As Decimal = 0.00
    Public P_TxnCode As String = "", P_TxnDesc As String = "", P_TxnType As String = "", P_ChargeAmt As String = "", P_ChqNo As String = "", P_TxnCash As String = "", P_PostMode As String = "", P_ContraGL As String = "", P_TelleID As String = "", P_AttachedTxnCode As String = "", P_RefCode As String = ""
    Public P_ProdRefCode As String = "", P_refNo As String = ""
    Const CURRENCY_DISPLAY_FORMAT As String = "##,##0.00"
    Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If lblTotalDebit.Text <> lblTotalCredits.Text Then
            lblSuccessMsg.Text = "Debits and credists totals must be equal"
            Exit Sub
        End If
        PostTransactions()
        'Me.saveGridTxnRecords()
        'lblSuccessMsg.Text = "Transaction posted and saved Successfully"
        RetailPostingGridView.DataSource = Nothing
        RetailPostingGridView.DataBind()
        lblDescription.Text = ""
        lblAccountName.Text = ""
        txtAccountNo.Text = ""
        txtAmount.Text = ""
        txtTransactionCode.Text = ""
        lblTotalCredits.Text = ""
        lblTotalDebit.Text = ""
    End Sub
    Protected Sub PostTransactions()
        Try

            Dim MyDatObject As New DataManagement
            Dim mySearch2 As String = "select count(*) as Count from tbAccount where ProductDefCode = 'LA'"
            Dim MyReader2 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch2, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader2.HasRows = True Then
                MyReader2.Read()
                Dim count As Integer = 0
                Dim newCount As Integer = 0
                newCount = MyReader2("Count").ToString
                While count < newCount
                    Dim mySearch4 As String = "select AccountNo,SettlementsAccountNo,RepaymentAmount,AssetGL_Control,LiabilityGL_Control,ProductDefCode from tbAccount where SOD_Status = 0"
                    Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
                    While MyReader4.HasRows
                        MyReader4.Read()
                        Dim TransCode As String = ""
                        Dim accountGL As String = ""
                        Dim transType As String = ""
                        Dim amounts As Decimal = 0
                        Dim accNo As String = ""
                        Dim accountBal As Decimal = 0.00
                        Dim proDefCode As String = ""
                        proDefCode = "LA"
                        Dim accsNo As String = ""
                        accsNo = mySearch4("AccountNo").ToString.Trim()
                        Dim repaymentAmount As Double = 200
                        If proDefCode = "LA" Then
                            Dim mySearch5 As String = "select AccountNo,AssetGL_Control,ProductDefCode,BookBalance,AvailBal from tbAccount where AccountNo = '222-100052-2222-02'"
                            Dim MyReader5 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch5, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
                            If MyReader5.HasRows = True Then
                                MyReader5.Read()
                                'Dim accNo As String = "222-100052-2222-02"
                                'P_RefCode = MyReader5("Journal").ToString
                                'TransCode = MyReader5("TrxCode").ToString
                                TransCode = "C90"
                                'amounts = CDbl(MyReader5("Amount").ToString)
                                amounts = repaymentAmount
                                'transType = MyReader5("TxnType").ToString
                                transType = "C"
                                accountGL = MyReader5("AssetGL_Control").ToString
                                'P_refNo = MyReader5("Reference").ToString
                                P_refNo = "12345"
                                Dim seqNo As Integer = 23
                                'UtilitiesModule.GetAccountDetails(P_AccountNo)
                                P_BookBalance = MyReader5("BookBalance").ToString
                                InsertGridRecords(accsNo, amounts, transType, accountGL, P_refNo, seqNo)
                                'InsertGridRecords(1, 2, accountGL, TransCode, accountGL, accountGL, transType, 241, 210, 100, 1, 100, transType, transType, transType, transType, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), 2, P_refNo, 0)
                                'InsertGridRecords(i + 1, SeqNo.Trim(), LiabilityGL_Control.Trim(), Me.txtTransactionCode.Text.Trim(), customerNo.Trim(), Me.lblAccountName.Text.Trim(), TxnType, branchID.Trim(), branchID.Trim(), depositAmount, fxRate, depositAmount, Me.txtNarration.Text.Trim(), Modules, TxnReference, UserId.Trim(), TxnDate, actionDate, SeqNo.Trim(), TxnReference, checkedValue)
                                GetAccountDetails(accNo)
                                GetTransactionCode(TransCode)
                                'accountBal = GetAccountBal(accNo)
                                InsertDailyTransaction(accNo)
                                InsertDailyTransactionHistory(accNo)
                                InsertGLTransaction(accNo)
                                InsertGLTransactionHistory(accNo)
                                'confirm this all in all modules
                                InsertAllDailyTransaction(accNo)
                                InsertAllDailyTransactionHistory(accNo)
                                'UpdateCustomerBalances(accNo, amounts, transType, P_BookBalance)
                                UpdateCustomerBalances(accNo, amounts, transType.Trim(), accountBal)
                                UpdateCustomerGL(accountGL, amounts, transType.Trim(), accNo)
                                UpdateCashierGL(TransCode, amounts)
                                UpdateSOD_Status(accNo)
                                count = count + 1
                            End If
                        Else
                        End If
                    End While
                    MyReader4.Close()
                End While
            End If

            MyReader2.Close()
        Catch ex As Exception
            'Label4.Text = "Error Pulling Account Details"
            lblSuccessMsg.Text = ex.ToString
        End Try
    End Sub

    Protected Sub RetailPostingGridView_RowCreated(sender As Object, e As GridViewRowEventArgs)
        If e.Row.RowType = DataControlRowType.DataRow Then
            Dim dt As DataTable = DirectCast(ViewState("CurrentTable"), DataTable)
            Dim lb As LinkButton = DirectCast(e.Row.FindControl("LinkButton1"), LinkButton)
            If lb IsNot Nothing Then
                If dt.Rows.Count > 1 Then
                    If e.Row.RowIndex = dt.Rows.Count - 1 Then
                        lb.Visible = False
                    End If
                Else
                    lb.Visible = False
                End If
            End If
        End If
    End Sub


    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            lblDate.Text = DateAndTime.Today()
            Me.InitializeGridview()
        End If
    End Sub

    Private Sub InitializeGridview()
        Dim dta As DataTable = New DataTable
        Dim dr As DataRow = Nothing
        dta.Columns.Add(New DataColumn("RowNumber", GetType(String)))
        dta.Columns.Add(New DataColumn("Account No", GetType(String)))
        dta.Columns.Add(New DataColumn("Account Name", GetType(String)))
        dta.Columns.Add(New DataColumn("TXN TYPE", GetType(String)))
        dta.Columns.Add(New DataColumn("Amount", GetType(String)))
        dta.Columns.Add(New DataColumn("RATE", GetType(String)))
        dta.Columns.Add(New DataColumn("Desc", GetType(String)))
        dta.Columns.Add(New DataColumn("GL Account", GetType(String)))
        ViewState("CurrentTable") = dta
        RetailPostingGridView.DataSource = dta
        RetailPostingGridView.DataBind()
    End Sub

    Private Sub btncheckTranCode_Click(sender As Object, e As EventArgs) Handles btncheckTranCode.Click
        lblSuccessMsg.Text = ""
        lblDescription.Text = ""
        lblAccountName.Text = ""
        txtAmount.Text = ""
        txtAccountNo.Text = ""
        GetTransactionCode(Me.txtTransactionCode.Text.Trim)
    End Sub

    Protected Sub GetTransactionCode(transCode)
        Try
            If txtTransactionCode.Text = "" Then
                txtTransactionCode.Focus()
                lblSuccessMsg.Text = " Please enter Transaction Code"
                Exit Sub
            End If
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select TxnCode,TxnDesc,TxnType,ChargeAmt,ChqNo,TxnCash,PostMode,ContraGL,TelleID,AttachedTxnCode,RefCode from tbTransactionCode where TxnCode = '" & transCode & "'"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                lblDescription.Text = MyReader4("TxnDesc").ToString
                P_TxnType = MyReader4("TxnType").ToString
                P_TxnCode = MyReader4("TxnCode").ToString
                P_PostMode = MyReader4("PostMode").ToString
                P_ContraGL = MyReader4("ContraGL").ToString
                P_TelleID = MyReader4("TelleID").ToString
                P_RefCode = MyReader4("RefCode").ToString
                If P_TxnCode = "D00" Or P_TxnCode = "C00" Then
                Else
                    lblSuccessMsg.Text = "Invalid transaction code"
                    lblDescription.Text = ""
                    Exit Sub
                End If
            Else
                lblSuccessMsg.Text = "Transaction Code does not exist"
                Exit Sub
            End If
        Catch ex As Exception
            'Label4.Text = "Error Pulling Account Details"
        End Try
    End Sub
    Protected Sub GetProductDefinition(productDefCode)
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select ProductDefCode,ProductName,RefCode from MB_ProductDefinitions where ProductDefCode = '" & productDefCode & "'"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                P_ProdRefCode = MyReader4("RefCode").ToString
            Else
                lblSuccessMsg.Text = "Transaction Code does not exist"
                Exit Sub
            End If
        Catch ex As Exception
            'Label4.Text = "Error Pulling Account Details"
        End Try
    End Sub

    Private Sub btnCheckAccNo_Click(sender As Object, e As EventArgs) Handles btnCheckAccNo.Click
        If lblDescription.Text = "" Then
            lblSuccessMsg.Text = "Please Check Transaction Code"
            Exit Sub
        Else
            GetAccountDetails(Me.txtAccountNo.Text.Trim)
        End If
    End Sub

    Public Sub GetAccountDetails(AccountNo)
        Try
            lblSuccessMsg.Text = ""
            lblAccountName.Text = ""
            ' txtAmount.Text = ""
            If txtAccountNo.Text = "" Then
                txtAccountNo.Focus()
                lblSuccessMsg.Text = " Please enter Account Number"
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
                P_AvailBal = MyReader4("AvailBal").ToString
                P_BookBalance = MyReader4("BookBalance").ToString
                P_DefinationCode = MyReader4("ProductDefCode").ToString().Trim()
            Else
                lblSuccessMsg.Text = "Account Number does not exist"
            End If
        Catch ex As Exception
            'txtOrgName.Text = ""
            'txtAdress1.Text = ""
            '' DropDownList1.Text = ""
            'txtOrgCode.ReadOnly = False
            'Label4.Text = "Error Pulling Account Details"
        End Try
    End Sub
    Public Function GetAccountBal(AccountNo)
        Dim accountBalance As Decimal = 0.00
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select AccountNo,CustomerNo,AccountName,AssetGL_Control,LiabilityGL_Control,BranchCode,AvailBal,BookBalance,ProductDefCode from [dbo].[tbAccount] where AccountNo = '" & AccountNo & "'" ' ='" & Me.HiddenField1.Value & "' "
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()

                accountBalance = MyReader4("BookBalance").ToString
            Else
                lblSuccessMsg.Text = "Account Number does not exist"
            End If
        Catch ex As Exception
            'txtOrgName.Text = ""
            'txtAdress1.Text = ""
            '' DropDownList1.Text = ""
            'txtOrgCode.ReadOnly = False
            'Label4.Text = "Error Pulling Account Details"
        End Try
        Return accountBalance
    End Function
    Protected Sub UpdateCustomerBalances(accountNo, amount, transType, P_BookBalance)
        'GetAccountDetails(P_AccountNo)
        Select Case transType
            Case "C"
                Try
                    Dim newAccBal As String = ""
                    newAccBal = P_BookBalance + Val(amount)
                    Dim MySqlaction1 As String
                    Dim mySqlAction As String = ""
                    Dim MyCurrencyName As String = ""
                    Dim MyDBAction As New DataManagement
                    MySqlaction1 = "update tbAccount set AvailBal ='" & newAccBal & "',BookBalance ='" & newAccBal & "', LastActivityDate = '" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "',LastCRactvtyDate ='" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "',LastCRDate = '" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "' where AccountNo ='" & accountNo & "'"
                    MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
                    'Dim displayBal As String = ""
                    'displayBal = Format(newAccBal, "###,###")
                    'txtNewBalance.Text = newAccBal
                Catch ex As Exception
                    'Label4.Text = "Error Inserting"
                End Try
            Case "D"
                Try
                    Dim newAccBal As String = ""
                    newAccBal = P_BookBalance - Val(amount)
                    Dim MySqlaction1 As String
                    Dim mySqlAction As String = ""
                    Dim MyCurrencyName As String = ""
                    Dim MyDBAction As New DataManagement
                    MySqlaction1 = "update tbAccount set AvailBal ='" & newAccBal & "',BookBalance ='" & newAccBal & "', LastActivityDate = '" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "',LastDRactvtyDate ='" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "',LastDRDate = '" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "' where AccountNo ='" & accountNo & "'"
                    MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
                    'Dim displayBal As String = ""
                    'displayBal = Format(newAccBal, "###,###")
                    'txtNewBalance.Text = newAccBal
                Catch ex As Exception
                    'Label4.Text = "Error Inserting"
                End Try
        End Select
    End Sub
    Protected Sub UpdateCustomerGL(liabilityGL, amounts, transtype, P_AccountNo)

        Select Case transtype
            Case "C"
                Try
                    Dim MyDatObject As New DataManagement
                    Dim mySearch4 As String = "select GLACCTNO,GLNAME,ACT_BAL,LCY_BAL from GL_DETAIL where GLACCTNO = '" & liabilityGL & "'"
                    Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
                    If MyReader4.HasRows = True Then
                        MyReader4.Read()
                        Dim GLBal As String = ""
                        Dim newGLBal As String = ""
                        Dim GLName As String = ""
                        GLBal = MyReader4("ACT_BAL").ToString
                        GLName = MyReader4("GLNAME").ToString
                        'dunga
                        P_LiabilityGL_Control = MyReader4("GLACCTNO").ToString
                        UpdateAllGLNames(P_AccountNo, P_LiabilityGL_Control, P_refNo, GLName)
                        UpdateAllGLNamesHistory(P_AccountNo, P_LiabilityGL_Control, P_refNo, GLName)
                        newGLBal = Val(GLBal) + Val(amounts)
                        Dim MySqlaction1 As String
                        Dim mySqlAction As String = ""
                        Dim MyCurrencyName As String = ""
                        Dim MyDBAction As New DataManagement
                        MySqlaction1 = "update GL_DETAIL set ACT_BAL ='" & newGLBal & "',LCY_BAL ='" & newGLBal & "' where GLACCTNO = '" & liabilityGL & "'"
                        MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
                    End If
                Catch ex As Exception
                    'Label4.Text = "Error Pulling Account Details, Commission A/c must start 01M8 .... E6 "
                End Try
            Case "D"
                Try
                    Dim MyDatObject As New DataManagement
                    Dim mySearch4 As String = "select GLACCTNO,GLNAME,ACT_BAL,LCY_BAL from GL_DETAIL where GLACCTNO = '" & liabilityGL & "'"
                    Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
                    If MyReader4.HasRows = True Then
                        MyReader4.Read()
                        Dim GLBal As String = ""
                        Dim newGLBal As String = ""
                        Dim GLName As String = ""
                        GLBal = MyReader4("ACT_BAL").ToString
                        GLName = MyReader4("GLNAME").ToString
                        P_LiabilityGL_Control = MyReader4("GLACCTNO").ToString
                        'dunga
                        'P_LiabilityGL_Control = MyReader4("GLAccount").ToString
                        UpdateAllGLNames(P_AccountNo, P_LiabilityGL_Control, P_refNo, GLName)
                        UpdateAllGLNamesHistory(P_AccountNo, P_LiabilityGL_Control, P_refNo, GLName)
                        newGLBal = Val(GLBal) - Val(amounts)
                        Dim MySqlaction1 As String
                        Dim mySqlAction As String = ""
                        Dim MyCurrencyName As String = ""
                        Dim MyDBAction As New DataManagement
                        MySqlaction1 = "update GL_DETAIL set ACT_BAL ='" & newGLBal & "',LCY_BAL ='" & newGLBal & "' where GLACCTNO = '" & liabilityGL & "'"
                        MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
                    End If
                Catch ex As Exception
                    'Label4.Text = "Error Pulling Account Details, Commission A/c must start 01M8 .... E6 "
                End Try
        End Select

    End Sub
    Protected Sub UpdateAllGLNames(accountNo, liabilityGL, refNo, glName)
        Dim MySqlaction1 As String
        Dim MyCurrencyName As String = ""
        Dim MyDBAction1 As New DataManagement
        MySqlaction1 = "update MB_GL_DailyTransaction set GLAccNo ='" & liabilityGL & "',GLName ='" & glName & "' where TxnRefNo = '" & refNo & "'"
        MyDBAction1.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
    End Sub
    Protected Sub UpdateAllGLNamesHistory(accountNo, liabilityGL, refNo, glName)
        Dim MySqlaction1 As String
        Dim MyCurrencyName As String = ""
        Dim MyDBAction1 As New DataManagement
        MySqlaction1 = "update MB_GL_DailyTransaction_History set GLAccNo ='" & liabilityGL & "',GLName ='" & glName & "' where TxnRefNo = '" & refNo & "'"
        MyDBAction1.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
    End Sub
    Protected Sub UpdateCashierGL(transcode, amounts)
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select TxnCode,ContraGL from tbTransactionCode where TxnCode = '" & transcode & "' "
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                Dim cashierGL As String = ""
                cashierGL = MyReader4("ContraGL").ToString
                Try
                    Dim mySearch5 As String = "select Amount,GLName from tbCashGL where GLAccount = '" & cashierGL & "' " ' ='" & Me.HiddenField1.Value & "' "
                    Dim MyReader5 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch5, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
                    If MyReader5.HasRows = True Then
                        MyReader5.Read()
                        Dim GLBal As String = ""
                        Dim newGLBal As String = ""
                        GLBal = MyReader5("Amount").ToString
                        'cashierGLName = MyReader5("GLName").ToString
                        newGLBal = GLBal - amounts
                        Dim MySqlaction1 As String
                        Dim mySqlAction As String = ""
                        Dim MyCurrencyName As String = ""
                        Dim MyDBAction As New DataManagement
                        MySqlaction1 = "update tbCashGL set Amount ='" & newGLBal & "' where GLAccount = '" & cashierGL & "'"
                        MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
                    End If
                Catch ex As Exception
                End Try
            End If
        Catch ex As Exception
            'Label4.Text = "Error Pulling Account Details, Commission A/c must start 01M8 .... E6 "
        End Try
    End Sub
    Protected Function GetTransactionCode()
        Try
            lblSuccessMsg.Text = ""
            lblDescription.Text = ""
            lblAccountName.Text = ""
            If txtTransactionCode.Text = "" Then
                txtTransactionCode.Focus()
                lblSuccessMsg.Text = " Please enter Transaction Code"
                Exit Function
            End If
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select TxnCode,TxnDesc,TxnType,ChargeAmt,ChqNo,TxnCash,PostMode,ContraGL,TelleID,AttachedTxnCode,RefCode from tbTransactionCode where TxnCode = '" & txtTransactionCode.Text & "'"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                lblDescription.Text = MyReader4("TxnDesc").ToString
                P_TxnType = MyReader4("TxnType").ToString
                P_TxnCode = MyReader4("TxnCode").ToString
                P_PostMode = MyReader4("PostMode").ToString
                P_ContraGL = MyReader4("ContraGL").ToString
                P_TelleID = MyReader4("TelleID").ToString
                P_RefCode = MyReader4("RefCode").ToString
                P_TxnDesc = MyReader4("TxnDesc").ToString
                If P_TxnType = "D" Then
                    lblSuccessMsg.Text = "Invalid transaction code"
                    lblDescription.Text = ""
                    Exit Function
                End If
                If P_TxnDesc = "CASH DEPOSIT" Then
                Else
                    lblSuccessMsg.Text = "Invalid transaction code"
                    lblDescription.Text = ""
                    Exit Function
                End If
            Else
                lblSuccessMsg.Text = "Transaction Code does not exist"
                Exit Function
            End If
        Catch ex As Exception
            'Label4.Text = "Error Pulling Account Details"
        End Try
    End Function

    Private Sub btnPost_Click(sender As Object, e As EventArgs) Handles btnPost.Click
        GetTransactionCode()
        'Dim test2 As String = GetTransactionRefCode(txtTransactionCode.Text.Trim)
        'Dim tcode As String = test2
        If txtTransactionCode.Text = "" Then
            lblSuccessMsg.Text = "Please enter Transaction Code"
            txtTransactionCode.Focus()
            Exit Sub
        Else
            lblSuccessMsg.Text = ""
        End If
        If txtAccountNo.Text = "" Then
            lblSuccessMsg.Text = "Please enter Account Number"
            txtAccountNo.Focus()
            Exit Sub
        Else
            lblSuccessMsg.Text = ""
        End If
        If txtAmount.Text = "" Then
            lblSuccessMsg.Text = "Please enter Amount"
            txtAmount.Focus()
            Exit Sub
        Else
            lblSuccessMsg.Text = ""
        End If
        Dim codeTxnType As Char = ""
        Dim totalDebits As Double = 0
        Dim totalCredits As Double = 0
        Dim LiabilityGL_Control As String = ""
        Dim customerSeqNo As String = ""
        Dim refNo As String = ""
        Dim Modules As String = ""
        Dim depositAmount As Double = 0
        Dim UserId As String = "Lamin"
        Dim TxnReference As String = ""
        Dim TxnCode As String = ""
        Dim TxnDate As DateTime = Date.Now()
        Dim actionDate As DateTime = Date.Now()
        Dim customerNo As String = ""
        depositAmount = CDbl(Me.txtAmount.Text.Trim())
        Dim branchID As String = "001"
        Dim fxRate As String = "1"
        Dim _module As String = ""
        'seqNo is last seqNo
        Dim SeqNo As String = ""
        Dim customerInsertSQl As String = ""
        Dim dailyTransactionInsertSQl As String = ""
        Dim _day As String = ""
        Dim _month As String = ""
        Dim _year As String = ""
        UtilitiesModule._GetAccountDetails(Me.txtAccountNo.Text.Trim)
        UtilitiesModule.GetNextSeqNo("CUSTOMER_REFNO")
        customerSeqNo = UtilitiesModule.returnValue
        UtilitiesModule.GetNextSeqNo("LAST_SEQNO")
        SeqNo = UtilitiesModule.returnValue
        GetAccountDetails(Me.txtAccountNo.Text.Trim)
        'GetModule(P_DefinationCode)
        'Modules = test

        GetProductDefinition(P_DefinationCode)
        _module = P_ProdRefCode
        Modules = P_DefinationCode

        customerNo = txtAccountNo.Text.Trim()
        TxnCode = P_RefCode
        'TxnCode = Me.txtTransactionCode.Text.Trim()
        _day = (TxnDate.Day).ToString().PadLeft(2, "0")
        _month = (TxnDate.Month).ToString().PadLeft(2, "0")
        _year = (TxnDate.Year).ToString().Substring(2, 2)
        Dim checkedValue As Byte = 0
        'Dim contraGl_Name As String = ""
        Dim LiabilityGL_Control_Name As String = ""
        UtilitiesModule._GetAccountDetails(Me.txtAccountNo.Text.Trim)
        UtilitiesModule.GetTransactionCodeDetails(Me.txtTransactionCode.Text.Trim())
        '' UtilitiesModule.GetTransactionCodeDetails(Me.txtTransactionCode.Text.Trim())
        'customerAccountName = UtilitiesModule.P_AccountName.Trim()
        '_contraGl = UtilitiesModule.Contra_GL
        LiabilityGL_Control = UtilitiesModule.P_LiabilityGL_Control
        codeTxnType = UtilitiesModule.TxnType
        'Dim dr As DataRow = Nothing
        Dim dtCurrentTable As DataTable = DirectCast(ViewState("CurrentTable"), DataTable)
        ' Dim rowCount As Integer = dtCurrentTable.Rows.Count()
        'Dim rowCount As Integer = dtCurrentTable.Rows.Count()
        For i = 1 To 1
            dtCurrentTable.Rows.Add(dtCurrentTable.NewRow())
        Next
        Dim rowCount As Integer = dtCurrentTable.Rows.Count()
        refNo = ""
        'TxnReference = _module & (TxnCode).ToUpper() & SeqNo & _day & _month & _year
        'totalCredits
        totalDebits = IIf(Me.lblTotalDebit.Text = "", 0, CDbl(Me.lblTotalDebit.Text.Trim()))
        totalCredits = IIf(Me.lblTotalCredits.Text = "", 0, CDbl(Me.lblTotalCredits.Text.Trim()))
        If TxnType = "C" Then
            totalCredits = totalCredits + CDbl(Me.txtAmount.Text.Trim())
            Me.lblTotalCredits.Text = Format(Double.Parse(totalCredits), CURRENCY_DISPLAY_FORMAT)

        ElseIf TxnType = "D" Then
            totalDebits = totalDebits + CDbl(Me.txtAmount.Text.Trim())
            Me.lblTotalDebit.Text = Format(Double.Parse(totalDebits), CURRENCY_DISPLAY_FORMAT)
        End If
        With dtCurrentTable
            For i = 0 To .Rows.Count - 1
                If i = rowCount - 1 Then
                    'Me.txtNarration.Enabled = False
                    dtCurrentTable.Rows(i)("RowNumber") = rowCount
                    'Insert Contra GL Here
                    dtCurrentTable.Rows(i)("Account No") = Me.txtAccountNo.Text.Trim()
                    dtCurrentTable.Rows(i)("ACCOUNT NAME") = Me.lblAccountName.Text.Trim()
                    dtCurrentTable.Rows(i)("TXN TYPE") = codeTxnType
                    dtCurrentTable.Rows(i)("Amount") = Format(Double.Parse(Me.txtAmount.Text.Trim()), CURRENCY_DISPLAY_FORMAT)
                    dtCurrentTable.Rows(i)("Desc") = Me.txtNarration.Text.Trim()
                    'TxnReference = _module & (TxnCode).ToUpper() & SeqNo & i + 1 & _day & _month & _year
                    If TxnType = "C" Then
                        checkedValue = 1
                        dtCurrentTable.Rows(i)("GL Account") = LiabilityGL_Control.Trim()
                        'InsertGridRecords(i + 1, SeqNo.Trim(), LiabilityGL_Control.Trim(), Me.txtTransactionCode.Text.Trim(), customerNo.Trim(), Me.lblAccountName.Text.Trim(), TxnType, branchID.Trim(), branchID.Trim(), depositAmount, fxRate, depositAmount, Me.txtNarration.Text.Trim(), Modules, TxnReference, UserId.Trim(), TxnDate, actionDate, SeqNo.Trim(), TxnReference, checkedValue)
                    ElseIf TxnType = "D" Then
                        checkedValue = 1
                        dtCurrentTable.Rows(i)("GL Account") = LiabilityGL_Control.Trim()
                        'InsertGridRecords(i + 1, SeqNo.Trim(), LiabilityGL_Control.Trim(), Me.txtTransactionCode.Text.Trim(), customerNo.Trim(), Me.lblAccountName.Text.Trim(), TxnType, branchID.Trim(), branchID.Trim(), depositAmount, fxRate, depositAmount, Me.txtNarration.Text.Trim(), Modules, TxnReference, UserId.Trim(), TxnDate, actionDate, SeqNo.Trim(), TxnReference, checkedValue)
                    End If

                    RetailPostingGridView.DataSource = dtCurrentTable
                    RetailPostingGridView.DataBind()
                    ViewState("CurrentTable") = dtCurrentTable
                    ' Me.refreshPost()
                    'ElseIf rowCount > 0 Then
                    '    'UtilitiesModule.GetGlAccoutNames(_contraGl.Trim())
                    '    'contraGl_Name = UtilitiesModule.GLName.Trim()
                    '    dtCurrentTable.Rows(rowCount)("RowNumber") = rowCount + 1
                    '    dtCurrentTable.Rows(rowCount)("Account No") = Me.txtAccountNo.Text.Trim()
                    '    dtCurrentTable.Rows(rowCount)("ACCOUNT NAME") = Me.lblAccountName.Text.Trim()
                    '    dtCurrentTable.Rows(rowCount)("TXN TYPE") = codeTxnType
                    '    dtCurrentTable.Rows(rowCount)("Amount") = Format(Double.Parse(Me.txtAmount.Text.Trim()), CURRENCY_DISPLAY_FORMAT)
                    '    dtCurrentTable.Rows(rowCount)("Desc") = Me.txtNarration.Text.Trim()
                    '    If TxnType = "C" Then
                    '        dtCurrentTable.Rows(rowCount)("GL Account") = LiabilityGL_Control.Trim()
                    '        InsertGridRecords(customerSeqNo.Trim(), LiabilityGL_Control.Trim(), Me.txtTransactionCode.Text.Trim(), customerNo.Trim(), TxnType, branchID.Trim(), branchID.Trim(), depositAmount, fxRate, depositAmount, Me.txtNarration.Text.Trim(), _module.Trim(), TxnReference, UserId.Trim(), TxnDate, actionDate, SeqNo.Trim())
                    '    ElseIf TxnType = "D" Then
                    '        dtCurrentTable.Rows(rowCount)("GL Account") = LiabilityGL_Control.Trim()
                    '        InsertGridRecords(customerSeqNo.Trim(), LiabilityGL_Control.Trim(), Me.txtTransactionCode.Text.Trim(), customerNo.Trim(), TxnType, branchID.Trim(), branchID.Trim(), depositAmount, fxRate, depositAmount, Me.txtNarration.Text.Trim(), _module.Trim(), TxnReference, UserId.Trim(), TxnDate, actionDate, SeqNo.Trim())
                    '    End If
                    '    RetailPostingGridView.DataSource = dtCurrentTable
                    '    RetailPostingGridView.DataBind()
                    '    ViewState("CurrentTable") = dtCurrentTable
                    '    ' Me.refreshPost()
                    '    'rowCount = rowCount - 1
                End If
            Next
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select RowNum  from MB_RetailPosting_Temp"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                Dim rowNum As String = MyReader4("RowNum").ToString
                TxnReference = _module & (TxnCode).ToUpper() & SeqNo & rowNum & _day & _month & _year
                Dim MySqlaction1 As String
                Dim mySqlAction As String = ""
                Dim MyCurrencyName As String = ""
                Dim MyDBAction As New DataManagement
                MySqlaction1 = "update MB_RetailPosting_Temp set SeqNo ='" & SeqNo & "', TxnRefNo = '" & TxnReference & "', ExtSeqNo = '" & SeqNo & "'"
                MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
            End If
        End With
        txtTransactionCode.Text = ""
        lblDescription.Text = ""
        txtAccountNo.Text = ""
        lblAccountName.Text = ""
        'drpAccountType.Text = ""
        txtAmount.Text = ""

    End Sub
    Private Function GetModule(ByVal refCode As String) As String
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select ProductDefCode,ProductName,RefCode from MB_ProductDefinitions where ProductDefCode = '" & refCode & "'"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                refCode = MyReader4("RefCode").ToString
            End If
        Catch ex As Exception
            'Label4.Text = "Error Pulling Account Details"
        End Try
        Return refCode

    End Function
    Private Function GetTransactionRefCode(ByVal txnTefCode As String) As String
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select ProductDefCode,ProductName,RefCode from MB_ProductDefinitions where ProductDefCode = '" & txnTefCode & "'"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                txnTefCode = MyReader4("RefCode").ToString
            End If
        Catch ex As Exception
            'Label4.Text = "Error Pulling Account Details"
        End Try
        Return txnTefCode

    End Function
    Function GetProductCode(ByVal productCode As String)

        'Return numeric
    End Function

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.txtTransactionCode.Text = ""
        Me.lblDescription.Text = ""
        Me.txtAccountNo.Text = ""
        Me.lblAccountName.Text = ""
        Me.txtAmount.Text = ""
        Dim dtCurrentTable As DataTable = DirectCast(ViewState("CurrentTable"), DataTable)
        Dim rowCount As Integer = dtCurrentTable.Rows.Count()
        If rowCount = 0 Then
            Me.txtNarration.Text = ""
        End If
    End Sub

    Private Sub btnClearForm_Click(sender As Object, e As EventArgs) Handles btnClearForm.Click
        Me.refreshPage()

    End Sub

    Private Sub refreshPost()
        Me.txtTransactionCode.Text = ""
        Me.lblDescription.Text = ""
        Me.txtAccountNo.Text = ""
        Me.lblAccountName.Text = ""
        Me.txtAmount.Text = ""
    End Sub
    Private Sub InsertGridRecords(accsNo, amounts, transType, accountGL, P_refNo, seqNo)

        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "Insert into MB_RetailPosting_Temp values('','','" & accountGL & "','', " &
                                        "'" & accsNo & "','','" & transType & "', " &
                    "'','', " &
                    " '" & "GMD" & "','" & amounts & "', " &
                    "'','" & amounts & "', " &
                    "'','" & "00" & "',''," &
                    "'','" & P_refNo & "', " &
                    "'" & "" & "','" & "" & "', " &
                    "'" & "" & "','" & "" & "', " &
                    "'','" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "','" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "','','" & P_refNo & "','')"
        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
        'mySqlAction = "Insert into MB_RetailPosting_Temp values('" & rowCount & "','" & customerSeqNo.Trim() & "','" & LiabilityGL_Control & "','" & transCode & "', " &
        '                        "'" & customerNo.Trim() & "','" & accountName.Trim() & "','" & transType & "', " &
        '    "'" & branchID.Trim() & "','" & branchID.Trim() & "', " &
        '    " '" & "GMD" & "','" & depositAmount & "', " &
        '    "'" & fxRate & "','" & depositAmount & "', " &
        '    "'" & Desc & "','" & "00" & "',''," &
        '    "'" & _module & "','" & TxnReference & "', " &
        '    "'" & "" & "','" & "" & "', " &
        '    "'" & "" & "','" & "" & "', " &
        '    "'" & UserId.Trim() & "','" & TxnDate & "','" & actionDate & "','" & checkedValue & "','" & txnRefNo & "','" & seqNo.Trim() & "')"


        'Dim mySqlAction As String = ""
        'Dim MyDBAction As New DataManagement
        'mySqlAction = "Insert into MB_RetailPosting_Temp (AccountNo,Amount,TxnType,LiabilityGL_Control,TxnRefNo,SeqNo)values('" & accsNo & "','" & amounts & "','" & transType & "','" & accountGL & "','" & P_refNo & "','" & seqNo & "' )" ','" & txtBranchName.Text.ToUpper & "','" & txtAdress2.Text.ToUpper & "','" & txtmobile.Text & "') "
        'MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    End Sub
    Private Sub InsertDailyTransaction(AccountNo)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "INSERT INTO MB_Acc_DailyTransactions (SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,RCT_No,INV_No,Module,PayBy,ChequeNo,DR_GL,CR_GL,PostedBy,TxnDate,ActDate,Journal,SrNo,TxnRefNo,ExtSeqNo)
SELECT SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,RCT_No,INV_No,Module,PayBy,ChequeNo,DR_GL,CR_GL,PostedBy,TxnDate,ActDate,Journal,RowNum,TxnRefNo,ExtSeqNo
FROM MB_RetailPosting_Temp
WHERE AccountNo = '" & AccountNo & "'"
        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    End Sub
    Private Sub InsertGLTransaction(AccountNo)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "INSERT INTO [dbo].[MB_GL_DailyTransaction] (SeqNo,GLAccNo,GLName,TxnType,TxnBranch,AccountBranch,AcCCY,Amount,FX_Rate,LCY_Amount,Description,Module,UserName,TxnDate,SrNo,ActDate,TxnRefNo,ExtRefNo)
SELECT SeqNo,LiabilityGL_Control,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,Module,PostedBy,TxnDate,RowNum ,ActDate,TxnRefNo,ExtSeqNo FROM MB_RetailPosting_Temp
WHERE AccountNo = '" & AccountNo & "'"
        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    End Sub
    Private Sub InsertGLTransactionHistory(AccountNo)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "INSERT INTO [dbo].[MB_GL_DailyTransaction_History] (SeqNo,GLAccNo,GLName,TxnType,TxnBranch,AccountBranch,AcCCY,Amount,FX_Rate,LCY_Amount,Description,Module,UserName,TxnDate,SrNo,ActDate,TxnRefNo,ExtRefNo)
SELECT SeqNo,LiabilityGL_Control,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,Module,PostedBy,TxnDate,RowNum ,ActDate,TxnRefNo,ExtSeqNo FROM MB_RetailPosting_Temp
WHERE AccountNo = '" & AccountNo & "'"
        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    End Sub
    Private Sub InsertDailyTransactionHistory(AccountNo)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "INSERT INTO MB_Acc_DailyTransactions_History (SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,RCT_No,INV_No,Module,PayBy,ChequeNo,DR_GL,CR_GL,PostedBy,TxnDate,ActDate,Journal,SrNo,TxnRefNo,ExtSeqNo)
SELECT SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,RCT_No,INV_No,Module,PayBy,ChequeNo,DR_GL,CR_GL,PostedBy,TxnDate,ActDate,Journal,RowNum,TxnRefNo,ExtSeqNo
FROM MB_RetailPosting_Temp
WHERE AccountNo = '" & AccountNo & "'"
        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    End Sub
    Private Sub InsertAllDailyTransaction(AccountNo)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "INSERT INTO MB_All_DailyTransactions (SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccountBranch,AcCCY,Amount,FX_Rate,LCY_Amount,Description,Module,UserName,TxnDate,ActDate,SrNo,TxnRefNo,ExtRefNo)
SELECT SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,Module,PostedBy,TxnDate,ActDate,RowNum,TxnRefNo,ExtSeqNo
FROM MB_RetailPosting_Temp
WHERE AccountNo = '" & AccountNo & "'"
        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    End Sub
    Private Sub InsertAllDailyTransactionHistory(AccountNo)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "INSERT INTO MB_All_DailyTransactions_History (SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccountBranch,AcCCY,Amount,FX_Rate,LCY_Amount,Description,Module,UserName,TxnDate,ActDate,SrNo,TxnRefNo,ExtRefNo)
SELECT SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,Module,PostedBy,TxnDate,ActDate,RowNum,TxnRefNo,ExtSeqNo
FROM MB_RetailPosting_Temp
WHERE AccountNo = '" & AccountNo & "'"
        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    End Sub
    Private Sub UpdateSOD_Status(accountno)
        Dim mySqlAction1 As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction1 = "update tbAccount set SOD_Status =1 where AccountNo = '" & accountno & "'"
        MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
    End Sub

    Private Sub refreshPage()
        Me.txtTransactionCode.Text = ""
        Me.lblDescription.Text = ""
        Me.txtAccountNo.Text = ""
        Me.lblAccountName.Text = ""
        Me.txtAmount.Text = ""
        Me.lblTotalCredits.Text = "0.0"
        Me.lblTotalDebit.Text = "0.0"
        Dim dtCurrentTable As DataTable = DirectCast(ViewState("CurrentTable"), DataTable)
        RetailPostingGridView.DataSource = Nothing
        RetailPostingGridView.DataBind()
        ' Me.InitializeGridview()
        Me.txtNarration.Text = ""
        ' Me.txtNarration.Enabled = True
    End Sub

    Protected Sub LinkDelete_Click(sender As Object, e As EventArgs)
        Dim lb As LinkButton = DirectCast(sender, LinkButton)
        Dim gvRow As GridViewRow = DirectCast(lb.NamingContainer, GridViewRow)
        Dim rowID As Integer = gvRow.RowIndex
        If ViewState("CurrentTable") IsNot Nothing Then
            Dim dt As DataTable = DirectCast(ViewState("CurrentTable"), DataTable)
            If dt.Rows.Count > 0 Or dt.Rows.Count = 0 Then
                'If gvRow.RowIndex < dt.Rows.Count - 1 Then
                Dim dbRow As String = ""
                'dt.Rows(rowID).ItemArray(dt)
                dbRow = rowID + 1
                DeleteRecord(dbRow)
                dt.Rows.Remove(dt.Rows(rowID))
                ResetRowID(dt)
                'End If
                ' Me.recalculateGridDebitsCredits()
            End If
            ViewState("CurrentTable") = dt
            RetailPostingGridView.DataSource = dt
            RetailPostingGridView.DataBind()
        End If
    End Sub
    Private Sub DeleteRecord(accountno)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "delete from MB_RetailPosting_Temp where RowNum = '" & accountno & "' "
        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Delete)
    End Sub

    Private Sub ResetRowID(dt As DataTable)
        Dim type As Char = ""
        Dim amount As Double = 0
        Dim totalDebits As Double = 0
        Dim totalCredits As Double = 0

        Dim rowNumber As Integer = 1
        If dt.Rows.Count > 0 Then
            For Each row As DataRow In dt.Rows
                row(0) = rowNumber
                type = row(3).ToString()
                amount = CDbl(row(4).ToString())
                If type = "D" Then
                    totalDebits += amount
                ElseIf type = "C" Then
                    totalCredits += amount
                End If
                rowNumber += 1
            Next
        End If
        Me.lblTotalCredits.Text = Format(Double.Parse(totalCredits), CURRENCY_DISPLAY_FORMAT)
        Me.lblTotalDebit.Text = Format(Double.Parse(totalDebits), CURRENCY_DISPLAY_FORMAT)
    End Sub
    Public Sub GetTransactionCodeDetails()
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select TxnCode,TxnDesc,TxnType,ChargeAmt,ChqNo,TxnCash,PostMode,ContraGL,TelleID,AttachedTxnCode,RefCode from tbTransactionCode where TxnCode = '" & TxnCode & "'"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                TxnType = MyReader4("TxnType").ToString
                TxnCode = MyReader4("TxnCode").ToString
                PostMode = MyReader4("PostMode").ToString
                Contra_GL = MyReader4("ContraGL").ToString
                TelleID = MyReader4("TelleID").ToString
                RefCode = MyReader4("RefCode").ToString
            End If
            MyReader4.Close()
        Catch ex As Exception
            Throw New Exception("Error Loading Transaction Details", ex)
        End Try
    End Sub
End Class