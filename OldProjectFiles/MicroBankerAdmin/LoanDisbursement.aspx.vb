Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Imports System.Data.SqlClient
Public Class LoanDisbursement
    Inherits System.Web.UI.Page
    Public P_AccountNo As String = "", P_CustomerNo As String = "", P_AccountName As String = "", P_AccountStatus As String = "", P_AssetGL_Control As String = "", P_LiabilityGL_Control As String = "", P_Settlements_LiabilityGL_Control As String = "", P_DisburseStatus As String, P_BranchCode As String = "", P_AvailBal As Decimal = 0.00, P_BookBalance As Decimal = 0.00, P_LoanAmount As Decimal = 0.00
    Public P_TxnCode As String = "", P_TxnDesc As String = "", P_TxnType As String = "", P_ChargeAmt As String = "", P_ChqNo As String = "", P_TxnCash As String = "", P_PostMode As String = "", P_ContraGL As String = "", P_TelleID As String = "", P_AttachedTxnCode As String = "", P_RefCode As String = "", P_SettlementsAccountNo As String = ""
    Public P_ProdRefCode As String = ""
    Public P_Approved As Integer
    Const CURRENCY_DISPLAY_FORMAT As String = "##,##0.00"
    Dim refNo As String = ""
    Dim depositAmount As Double = 0
    Dim UserId As String = "Lamin"
    Dim TxnReference As String = ""
    Dim TxnCode As String = ""
    Dim TxnDate As DateTime = Date.Now()
    Dim actionDate As String = ""
    Dim customerSeqNo As String = ""
    Dim customerNo As String = ""
    Dim branchID As String = "001"
    Dim fxRate As String = "1"
    Dim _module As String = ""
    'seqNo is last seqNo
    Dim SeqNo As String = ""
    Dim customerInsertSQl As String = ""
    Dim dailyTransactionInsertSQl As String = ""
    Dim AllTransactionInsertSQl As String = ""
    Dim GLTransactionInsertSQl As String = ""
    Dim _day As String = ""
    Dim _month As String = ""
    Dim _year As String = ""
    '1. Get Populated Table With Its Content
    Dim dtCurrentTable As DataTable = DirectCast(ViewState("CurrentTable"), DataTable)

    Dim ds As DataSet = New DataSet()
    Private P_AccountDetailsHashMap As New Hashtable


    Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If lblTotalDebit.Text <> lblTotalCredits.Text Then
            lblSuccessMsg.Text = "Debits and credists totals must be equal"
            Exit Sub
        End If

        Dim CreditAmt As Decimal = Convert.ToDecimal(txtAmount.Text)
        Dim DebitAmt As Decimal = -+Convert.ToDecimal(txtAmount.Text)


        PostTransactions()
        lblSuccessMsg.Text = "Transaction posted and saved Successfully"

        RetailPostingGridView.DataSource = Nothing
        RetailPostingGridView.DataBind()
        lblDescription.Text = ""
        lblAccountName.Text = ""
        txtAccountNo.Text = ""
        txtAmount.Text = ""
        txtTransactionCode.Text = ""
        txtSettlementAccountNo.Text = ""
        txtSettlementAccountName.Text = ""
        txtCRTransactionCode.Text = ""
        lblTotalDebit.Text = ""
        lblTotalCredits.Text = ""
        TextBox1.Text = ""
        lblCRDescription.Text = ""
        txtAmount.Text = ""
        'txtSettlementAccountName.TemplateCont
    End Sub
    Protected Sub PostTransactions()
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch2 As String = "select count(Journal) as Count from MB_RetailPosting_Temp"
            Dim MyReader2 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch2, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader2.HasRows = True Then
                MyReader2.Read()
                Dim count As Integer = 0
                Dim newCount As Integer = 0
                newCount = MyReader2("Count").ToString
                While count < newCount
                    Dim mySearch4 As String = "select Journal,AccountNo,TxnCode,Amount,TxnType,LiabilityGL_Control from MB_RetailPosting_Temp"
                    Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
                    If MyReader4.HasRows = True Then
                        MyReader4.Read()
                        Dim TransCode As String = ""
                        Dim liabilityGL As String = ""
                        Dim transType As String = ""
                        Dim amounts As Decimal = 0
                        Dim accountBal As Decimal = 0.00
                        P_RefCode = MyReader4("Journal").ToString
                        P_AccountNo = MyReader4("AccountNo").ToString
                        TransCode = MyReader4("TxnCode").ToString
                        amounts = CDbl(MyReader4("Amount").ToString)
                        transType = MyReader4("TxnType").ToString
                        liabilityGL = MyReader4("LiabilityGL_Control").ToString
                        accountBal = GetAccountBal(P_AccountNo)
                        GetAccountDetails(P_AccountNo)
                        GetTransactionCode(TransCode)
                        InsertDailyTransaction(P_AccountNo)
                        InsertDailyTransactionHistory(P_AccountNo)
                        InsertGLTransaction(P_AccountNo)
                        InsertGLTransactionHistory(P_AccountNo)
                        InsertAllDailyTransaction(P_AccountNo)
                        InsertAllDailyTransactionHistory(P_AccountNo)
                        InsertAuditTrail(P_AccountNo)
                        InsertAuditTrailHistory(P_AccountNo)
                        UpdateCustomerBalances(P_AccountNo, amounts, transType.Trim(), accountBal)
                        UpdateCustomerGL(liabilityGL, amounts, transType.Trim())

                        DropTempTable(P_AccountNo)

                        count = count + 1
                    End If
                    MyReader4.Close()
                End While
            End If

            MyReader2.Close()
        Catch ex As Exception
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
            MyReader4.Close()
        Catch ex As Exception
            'txtOrgName.Text = ""
            'txtAdress1.Text = ""
            'Label4.Text = "Error Pulling Account Details"
        End Try
        Return accountBalance
    End Function
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
            MyReader4.Close()
        Catch ex As Exception
            'Label4.Text = "Error Pulling Account Details"
        End Try
    End Sub
    Private Sub InsertDailyTransaction(AccountNo)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "INSERT INTO MB_Acc_DailyTransactions (SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,RCT_No,INV_No,Module,Reference,PayBy,ChequeNo,DR_GL,CR_GL,PostedBy,TxnDate,ActDate,SrNo,TxnRefNo,Journal,ExtSeqNo,BookBalance,AvailBal,GL_Control,CustomerNo,TxnCode)
SELECT SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,RCT_No,INV_No,Module,Reference,PayBy,ChequeNo,DR_GL,CR_GL,PostedBy,TxnDate,ActDate,RowNum,TxnRefNo,Journal,ExtSeqNo,BookBalance,AvailBal,LiabilityGL_Control,CustomerNo,TxnCode
FROM MB_RetailPosting_Temp
WHERE AccountNo = '" & AccountNo & "'"
        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    End Sub
    Private Sub InsertGLTransaction(AccountNo)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "INSERT INTO [dbo].[MB_GL_DailyTransaction] (SeqNo,GLAccNo,GLName,TxnType,TxnBranch,AccountBranch,AcCCY,Amount,FX_Rate,LCY_Amount,Description,InvoiceNo,Module,Reference,UserName,TxnDate,SrNo,ActDate,TxnRefNo,ExtRefNo,AccountNo,CustomerNo,TxnCode)
SELECT SeqNo,LiabilityGL_Control,LiabilityGL_Control,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,INV_No,Module,Reference,PostedBy,TxnDate,RowNum ,ActDate,TxnRefNo,ExtSeqNo,AccountNo,CustomerNo,TxnCode FROM MB_RetailPosting_Temp
WHERE AccountNo = '" & AccountNo & "'"
        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    End Sub
    Private Sub InsertGLTransactionHistory(AccountNo)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "INSERT INTO [dbo].[MB_GL_DailyTransaction_History] (SeqNo,GLAccNo,GLName,TxnType,TxnBranch,AccountBranch,AcCCY,Amount,FX_Rate,LCY_Amount,Description,InvoiceNo,Module,Reference,UserName,TxnDate,SrNo,ActDate,TxnRefNo,ExtRefNo,AccountNo,CustomerNo,TxnCode)
SELECT SeqNo,LiabilityGL_Control,LiabilityGL_Control,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,INV_No,Module,Reference,PostedBy,TxnDate,RowNum ,ActDate,TxnRefNo,ExtSeqNo,AccountNo,CustomerNo,TxnCode FROM MB_RetailPosting_Temp
WHERE AccountNo = '" & AccountNo & "'"
        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    End Sub
    Private Sub InsertDailyTransactionHistory(AccountNo)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "INSERT INTO MB_Acc_DailyTransactions_History (SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,RCT_No,INV_No,Module,Reference,PayBy,ChequeNo,DR_GL,CR_GL,PostedBy,TxnDate,ActDate,SrNo,TxnRefNo,Journal,ExtSeqNo,BookBalance,AvailBal,GL_Control,CustomerNo,TxnCode)
SELECT SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,RCT_No,INV_No,Module,Reference,PayBy,ChequeNo,DR_GL,CR_GL,PostedBy,TxnDate,ActDate,RowNum,TxnRefNo,Journal,ExtSeqNo,BookBalance,AvailBal,LiabilityGL_Control,CustomerNo,TxnCode
FROM MB_RetailPosting_Temp
WHERE AccountNo = '" & AccountNo & "'"
        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    End Sub
    Private Sub InsertAllDailyTransaction(AccountNo)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "INSERT INTO MB_All_DailyTransactions (SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccountBranch,AcCCY,Amount,FX_Rate,LCY_Amount,Description,InvoiceNo,Module,Reference,UserName,TxnDate,ActDate,SrNo,TxnRefNo,ExtRefNo,Contra_AccNo,CustomerNo,TxnCode)
SELECT SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,INV_No,Module,Reference,PostedBy,TxnDate,ActDate,RowNum,TxnRefNo,ExtSeqNo,LiabilityGL_Control,CustomerNo,TxnCode
FROM MB_RetailPosting_Temp
WHERE AccountNo = '" & AccountNo & "'"
        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    End Sub
    Private Sub InsertAllDailyTransactionHistory(AccountNo)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "INSERT INTO MB_All_DailyTransactions_History (SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccountBranch,AcCCY,Amount,FX_Rate,LCY_Amount,Description,InvoiceNo,Module,Reference,UserName,TxnDate,ActDate,SrNo,TxnRefNo,ExtRefNo,Contra_AccNo,CustomerNo,TxnCode)
SELECT SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,INV_No,Module,Reference,PostedBy,TxnDate,ActDate,RowNum,TxnRefNo,ExtSeqNo,LiabilityGL_Control,CustomerNo,TxnCode
FROM MB_RetailPosting_Temp
WHERE AccountNo = '" & AccountNo & "'"
        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)

    End Sub
    Private Sub InsertAuditTrail(AccountNo)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "INSERT INTO MB_TransactionsAuditTrail (SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccountBranch,AcCCY,Amount,FX_Rate,LCY_Amount,Description,Module,Reference,UserName,TxnDate,ActDate,SrNo,TxnRefNo,ExtRefNo,Contra_AccNo,CustomerNo,TxnCode)
SELECT SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,Module,Reference,PostedBy,TxnDate,ActDate,RowNum,TxnRefNo,ExtSeqNo,LiabilityGL_Control,CustomerNo,TxnCode
FROM MB_RetailPosting_Temp
WHERE AccountNo = '" & AccountNo & "'"
        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    End Sub
    'audit history
    Private Sub InsertAuditTrailHistory(AccountNo)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "INSERT INTO MB_TransactionsAuditTrail_History (SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccountBranch,AcCCY,Amount,FX_Rate,LCY_Amount,Description,Module,Reference,UserName,TxnDate,ActDate,SrNo,TxnRefNo,ExtRefNo,Contra_AccNo,CustomerNo,TxnCode)
SELECT SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,Module,Reference,PostedBy,TxnDate,ActDate,RowNum,TxnRefNo,ExtSeqNo,LiabilityGL_Control,CustomerNo,TxnCode
FROM MB_RetailPosting_Temp
WHERE AccountNo = '" & AccountNo & "'"
        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    End Sub

    Private Sub SaveTxnRecords(i, customerInsertSQl)

        Dim drCurrentRow As DataRow = dtCurrentTable.Rows(i)


        If i > 0 Then
            refNo = SeqNo & drCurrentRow("Account No") & i
        End If




    End Sub



    Private Sub saveGridTxnRecords()
    End Sub





    Private Sub SaveTransactions(ByVal SqlStr As String)
        'InsertDailyTransactions()
        Try
            Dim mySqlAction As String = ""
            Dim MyDBAction As New DataManagement
            mySqlAction = SqlStr.Trim()
            MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
            lblSuccessMsg.Text = "Transaction posted and saved Successfully"
            'txtAmount.Text = ""
        Catch ex As Exception
            Throw New Exception("Error InsertTransactions: " & SqlStr & "", ex)
        End Try
    End Sub


    Private Sub InsertTransactions(ByVal SqlStr As String)

        Try
            Dim mySqlAction As String = ""
            Dim MyDBAction As New DataManagement
            mySqlAction = SqlStr.Trim()
            'mySqlAction = "Insert into MB_DailyTransactions(AccountNo,CustomerNo,AccountName,AssetGL_Control,LiabilityGL_Control,BranchCode,Description,TxnType,TxnCode,PostMode,ContraGL,TelleID,Reference,Amount,TrxDate,Module)values('" & txtAccountNo.Text.ToUpper.Trim & "','" & P_CustomerNo.ToUpper.Trim & "','" & P_AccountName.ToUpper.Trim & "','" & P_AssetGL_Control.ToUpper.Trim & "','" & P_LiabilityGL_Control.ToUpper.Trim & "','" & P_BranchCode.ToUpper.Trim & "','" & lblDescription.Text.ToUpper.Trim & "','" & P_TxnType.ToUpper.Trim & "','" & P_TxnCode.ToUpper.Trim & "','" & P_PostMode.ToUpper.Trim & "','" & P_ContraGL.ToUpper.Trim & "','" & P_TelleID.ToUpper.Trim & "','123456789','" & txtAmount.Text.ToUpper.Trim & "','" & DateAndTime.Today() & "','" & P_RefCode.ToUpper.Trim & "') "
            MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
            lblSuccessMsg.Text = "Transaction posted and saved Successfully"
            'txtAmount.Text = ""
        Catch ex As Exception
            Throw New Exception("Error InsertTransactions: " & SqlStr & "", ex)
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
        GetTransactionCode(Me.txtTransactionCode.Text.Trim, "DEBIT")
    End Sub
    Private Sub btncheckTranCodeCR_Click(sender As Object, e As EventArgs) Handles btncheckTranCodeCR.Click

        GetTransactionCode(Me.txtCRTransactionCode.Text.Trim, "CREDIT")
    End Sub
    Private Sub btnCheckAccNo_Click(sender As Object, e As EventArgs) Handles btnCheckAccNo.Click
        GetAccountDetails(Me.txtAccountNo.Text.Trim)
    End Sub

    Protected Sub GetTransactionCode(transCode, CheckValue)
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
                P_TxnType = MyReader4("TxnType").ToString
                P_TxnCode = MyReader4("TxnCode").ToString
                P_PostMode = MyReader4("PostMode").ToString
                P_ContraGL = MyReader4("ContraGL").ToString
                P_TelleID = MyReader4("TelleID").ToString
                P_RefCode = MyReader4("RefCode").ToString


                If CheckValue = "DEBIT" Then
                    If txtTransactionCode.Text <> "D88" Then
                        lblSuccessMsg.Text = "Please enter a Loan Disbursement Debit Transaction Code"
                        txtTransactionCode.Text = ""
                        lblDescription.Text = ""

                    ElseIf txtTransactionCode.Text = "D88" Then
                        lblDescription.Text = MyReader4("TxnDesc").ToString
                    End If
                End If

                If CheckValue = "CREDIT" Then
                    If txtCRTransactionCode.Text <> "C88" Then
                        lblSuccessMsg.Text = "Please enter a Loan Disbursement Credit Transaction Code"
                        txtTransactionCode.Text = ""
                        lblCRDescription.Text = ""
                    ElseIf txtCRTransactionCode.Text = "C88" Then
                        lblCRDescription.Text = MyReader4("TxnDesc").ToString
                    End If
                End If


            Else
                lblSuccessMsg.Text = "Transaction Code does not exist"
                Exit Sub
            End If
            MyReader4.Close()
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
            MyReader4.Close()
        Catch ex As Exception
            'Label4.Text = "Error Pulling Account Details"
        End Try
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
            Dim mySearch4 As String = "select M.AccountNo,M.CustomerNo,M.AccountName,M.AssetGL_Control,M.LiabilityGL_Control,S.LiabilityGL_Control AS Settlements_LiabilityGL_Control,M.BranchCode,M.AvailBal,M.BookBalance,M.ProductDefCode,M.LoanAmount,M.RepaymentAmount,M.SettlementsAccountNo,M.SettlementsAccountName,M.Approved,M.AccountStatus,M.DisburseStatus from [dbo].[tbAccount] M inner join [dbo].[tbAccount] S on M.SettlementsAccountNo = S.AccountNo   where M.AccountNo = '" & AccountNo & "'" ' ='" & Me.HiddenField1.Value & "' "
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                P_DefinationCode = MyReader4("ProductDefCode").ToString().Trim()
                P_Approved = Convert.ToInt32(MyReader4("Approved"))
                P_AccountStatus = MyReader4("AccountStatus").ToString
                P_DisburseStatus = MyReader4("DisburseStatus").ToString
                If P_DisburseStatus = False Then
                    If P_DefinationCode = "LA" Then
                        'waitara commented
                        If P_Approved = 1 Then
                            If P_AccountStatus = "Open" Then
                                lblAccountName.Text = MyReader4("AccountName").ToString
                                'P_AccountNo = MyReader4("TxnCode").ToString
                                P_CustomerNo = MyReader4("CustomerNo").ToString
                                P_AccountName = MyReader4("AccountName").ToString
                                P_AssetGL_Control = MyReader4("AssetGL_Control").ToString
                                P_LiabilityGL_Control = MyReader4("LiabilityGL_Control").ToString
                                P_Settlements_LiabilityGL_Control = MyReader4("Settlements_LiabilityGL_Control").ToString
                                P_BranchCode = MyReader4("BranchCode").ToString
                                txtAmount.Text = MyReader4("LoanAmount").ToString
                                P_SettlementsAccountNo = MyReader4("SettlementsAccountNo").ToString
                                txtSettlementAccountNo.Text = MyReader4("SettlementsAccountNo").ToString
                                txtSettlementAccountName.Text = MyReader4("SettlementsAccountName").ToString
                                P_LoanAmount = Convert.ToDecimal(MyReader4("LoanAmount"))
                                P_AvailBal = Convert.ToDecimal(MyReader4("AvailBal"))
                                TextBox1.Text = Convert.ToDecimal(MyReader4("RepaymentAmount")).ToString
                                P_BookBalance = Convert.ToDecimal(MyReader4("BookBalance"))
                                ' P_BookBalance = MyReader4("SettlementsAccountName").ToString
                                'waitara commented
                            Else
                                txtAccountNo.Text = ""
                                lblAccountName.Text = ""
                                refreshForm()
                                lblSuccessMsg.Text = "Loan Account Has Already been Disbursed"
                            End If
                        Else
                            txtAccountNo.Text = ""
                            lblAccountName.Text = ""
                            refreshForm()
                            lblSuccessMsg.Text = "Account Should be Approved First"
                        End If
                    Else
                        txtAccountNo.Text = ""
                        lblAccountName.Text = ""
                        refreshForm()
                        lblSuccessMsg.Text = "Account Should be a Loan Account"
                    End If
                Else
                    lblSuccessMsg.Text = "Loan Account has been Disbursed.."
                End If
            Else
                lblSuccessMsg.Text = "Account Number does not exist"
            End If
            MyReader4.Close()
        Catch ex As Exception
            'lblSuccessMsg.Text = ex.ToString
            'txtAdress1.Text = ""
            '' DropDownList1.Text = ""
            'txtOrgCode.ReadOnly = False
            'Label4.Text = "Error Pulling Account Details"
        End Try
    End Sub

    Protected Sub UpdateCustomerBalances(accountNo, amount, transType, accountBal)
        Select Case transType
            Case "C"
                Try
                    Dim newAccBal As String = ""
                    newAccBal = accountBal + Val(amount)
                    Dim MySqlaction1 As String
                    Dim mySqlAction As String = ""
                    Dim MyCurrencyName As String = ""
                    Dim MyDBAction As New DataManagement
                    MySqlaction1 = "update tbAccount set AvailBal ='" & newAccBal & "',BookBalance ='" & newAccBal & "', LastCRDate = '" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "',LastCRactvtyDate = '" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "',LastActivityDate = '" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "' where AccountNo ='" & accountNo & "'"
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
                    newAccBal = accountBal - Val(amount)
                    Dim MySqlaction1 As String
                    Dim mySqlAction As String = ""
                    Dim MyCurrencyName As String = ""
                    Dim MyDBAction As New DataManagement
                    MySqlaction1 = "update tbAccount set AvailBal ='" & newAccBal & "',BookBalance ='" & newAccBal & "',LastDRDate ='" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "', LastDRactvtyDate = '" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "', LastActivityDate= '" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "' where AccountNo ='" & accountNo & "'"
                    MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
                    'Dim displayBal As String = ""
                    'displayBal = Format(newAccBal, "###,###")
                    'txtNewBalance.Text = newAccBal
                Catch ex As Exception
                    'Label4.Text = "Error Inserting"
                End Try

        End Select
    End Sub



    Protected Sub UpdateAccountStatus(accountNo, AccountStatus)
        Try
            Dim newAccBal As String = ""
            newAccBal = P_BookBalance + Val(Amount)
            Dim MySqlaction1 As String
            Dim mySqlAction As String = ""
            Dim MyCurrencyName As String = ""
            Dim MyDBAction As New DataManagement
            'MySqlaction1 = "update tbAccount set AccountStatus ='" & AccountStatus & "' where AccountNo ='" & accountNo & "'"
            MySqlaction1 = "update tbAccount set AccountStatus ='" & AccountStatus & "' , DisburseStatus = 1 where AccountNo ='" & accountNo & "'"
            MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
            'Dim displayBal As String = ""
            'displayBal = Format(newAccBal, "###,###")
            'txtNewBalance.Text = newAccBal
        Catch ex As Exception
            'Label4.Text = "Error Inserting"
        End Try
    End Sub
    Protected Sub UpdateCustomerGL(liabilityGL, amounts, transtype)

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
                        GLBal = MyReader4("ACT_BAL").ToString
                        'P_LiabilityGL_Control = MyReader4("GLAccount").ToString
                        newGLBal = Val(GLBal) + Val(amounts)
                        Dim MySqlaction1 As String
                        Dim mySqlAction As String = ""
                        Dim MyCurrencyName As String = ""
                        Dim MyDBAction As New DataManagement
                        MySqlaction1 = "update GL_DETAIL set ACT_BAL ='" & newGLBal & "',LCY_BAL ='" & newGLBal & "' where GLACCTNO = '" & liabilityGL & "'"
                        MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
                    End If
                    MyReader4.Close()
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
                        GLBal = MyReader4("ACT_BAL").ToString
                        'P_LiabilityGL_Control = MyReader4("GLAccount").ToString
                        newGLBal = Val(GLBal) - Val(amounts)
                        Dim MySqlaction1 As String
                        Dim mySqlAction As String = ""
                        Dim MyCurrencyName As String = ""
                        Dim MyDBAction As New DataManagement
                        MySqlaction1 = "update GL_DETAIL set ACT_BAL ='" & newGLBal & "',LCY_BAL ='" & newGLBal & "' where GLACCTNO = '" & liabilityGL & "'"
                        MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
                    End If
                    MyReader4.Close()
                Catch ex As Exception
                    'Label4.Text = "Error Pulling Account Details, Commission A/c must start 01M8 .... E6 "
                End Try
        End Select

    End Sub
    Protected Sub btnPost_Click(sender As Object, e As EventArgs) Handles btnPost.Click

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
        Dim customerAccountName As String = ""
        Dim d As String = ""
        Dim _contraGl As String = ""
        Dim LiabilityGL_Control As String = ""
        Dim AssetGL_Control As String = ""
        Dim contraGl_Name As String = ""
        Dim LiabilityGL_Control_Name As String = ""
        Dim AssetGL_Control_Name As String = ""
        Dim txnRefNo As String = ""


        UtilitiesModule._GetAccountDetails(Me.txtAccountNo.Text.Trim)
        UtilitiesModule.GetTransactionCodeDetails(Me.txtTransactionCode.Text.Trim())
        UtilitiesModule._GetAccountDetails(Me.txtAccountNo.Text.Trim)
        UtilitiesModule.GetNextSeqNo("CUSTOMER_REFNO")
        customerSeqNo = UtilitiesModule.returnValue
        UtilitiesModule.GetNextSeqNo("LAST_SEQNO")
        SeqNo = UtilitiesModule.returnValue
        GetAccountDetails(Me.txtAccountNo.Text.Trim)
        GetProductDefinition(P_DefinationCode)
        _module = P_ProdRefCode

        customerNo = txtAccountNo.Text.Trim()
        TxnCode = RefCode
        'TxnCode = Me.txtTransactionCode.Text.Trim()
        _day = (TxnDate.Day).ToString().PadLeft(2, "0")
        _month = (TxnDate.Month).ToString().PadLeft(2, "0")
        _year = (TxnDate.Year).ToString().Substring(2, 2)
        GetAccountDetails(Me.txtAccountNo.Text.Trim)
        ' UtilitiesModule.GetTransactionCodeDetails(Me.txtTransactionCode.Text.Trim())
        customerAccountName = UtilitiesModule.P_AccountName.Trim()
        _contraGl = UtilitiesModule.Contra_GL
        LiabilityGL_Control = UtilitiesModule.P_LiabilityGL_Control
        Dim dr As DataRow = Nothing
        Dim dtCurrentTable As DataTable = DirectCast(ViewState("CurrentTable"), DataTable)
        ' Dim rowCount As Integer = dtCurrentTable.Rows.Count()
        TxnReference = _module & (TxnCode).ToUpper() & SeqNo & _day & _month & _year
        actionDate = UtilitiesModule.GetTransctionDate()

        For i = 1 To 2
            dtCurrentTable.Rows.Add(dtCurrentTable.NewRow())
        Next
        Dim rowCount As Integer = dtCurrentTable.Rows.Count()
        With dtCurrentTable
            For i = 0 To rowCount
                If i = 0 Then
                    dtCurrentTable.Rows(i)("RowNumber") = 1
                    'Insert Contra GL Here
                    dtCurrentTable.Rows(i)("Account No") = txtAccountNo.Text.Trim
                    dtCurrentTable.Rows(i)("ACCOUNT NAME") = customerAccountName.Trim()
                    dtCurrentTable.Rows(i)("TXN TYPE") = "D"
                    dtCurrentTable.Rows(i)("Amount") = Format(Double.Parse(Me.txtAmount.Text.Trim()), CURRENCY_DISPLAY_FORMAT)
                    dtCurrentTable.Rows(i)("Desc") = "LOAN DISBURSEMENT"
                    dtCurrentTable.Rows(i)("GL Account") = P_AssetGL_Control
                    RetailPostingGridView.DataSource = dtCurrentTable
                    RetailPostingGridView.DataBind()
                    ViewState("CurrentTable") = dtCurrentTable

                    'P_AvailBal = GetAccountBal(txtAccountNo.Text.Trim()) - Format(Double.Parse(Me.txtAmount.Text.Trim()), CURRENCY_DISPLAY_FORMAT)
                    'GetAccountDetailMap(txtAccountNo.Text.Trim)
                    'P_AvailBal = P_AccountDetailsHashMap("AvailBal")
                    'P_BookBalance = P_AvailBal
                    'dump in the temp table here 

                    'get the account balance
                    Dim avBal As Decimal = 0.00
                    Dim bkBal As Decimal = 0.00
                    GetAccountDetailMap(Me.txtAccountNo.Text.Trim())
                    avBal = P_AccountDetailsHashMap("AvailBal")
                    bkBal = P_AccountDetailsHashMap("BookBalance")
                    P_CustomerNo = P_AccountDetailsHashMap("CustomerNo")
                    P_AccountDetailsHashMap.Clear()
                    P_AvailBal = avBal - Format(Double.Parse(Me.txtAmount.Text.Trim()), CURRENCY_DISPLAY_FORMAT)
                    P_BookBalance = bkBal - Format(Double.Parse(Me.txtAmount.Text.Trim()), CURRENCY_DISPLAY_FORMAT)

                    'P_AvailBal = GetAccountBal(Me.txtAccountNo.Text.Trim()) + Format(Double.Parse(Me.txtAmount.Text.Trim()), CURRENCY_DISPLAY_FORMAT)
                    'P_BookBalance = P_AvailBal
                    TxnType = "D"
                    TxnReference = _module & (TxnCode).ToUpper() & SeqNo & i + 1 & _day & _month & _year
                    txnRefNo = _module & (TxnCode).ToUpper() & SeqNo & _day & _month & _year
                    InsertGridRecords(i + 1, SeqNo.Trim(), P_AssetGL_Control, Me.txtTransactionCode.Text.ToUpper.Trim(), Me.txtAccountNo.Text.Trim(), Me.lblAccountName.Text.Trim(), TxnType, branchID.Trim(), branchID.Trim(), Me.txtAmount.Text.Trim(), fxRate, depositAmount, dtCurrentTable.Rows(i)("Desc"), P_DefinationCode, TxnReference, UserId.Trim(), TxnDate, actionDate, SeqNo, P_BookBalance, P_AvailBal, txnRefNo, P_CustomerNo)
                ElseIf i = 1 Then
                    UtilitiesModule.GetGlAccoutNames(txtSettlementAccountNo.Text.Trim)
                    'contraGl_Name = UtilitiesModule.GLName.Trim()
                    dtCurrentTable.Rows(i)("RowNumber") = i + 1
                    dtCurrentTable.Rows(i)("Account No") = txtSettlementAccountNo.Text.Trim
                    dtCurrentTable.Rows(i)("ACCOUNT NAME") = customerAccountName.Trim()
                    dtCurrentTable.Rows(i)("TXN TYPE") = "C"
                    dtCurrentTable.Rows(i)("Amount") = Format(Double.Parse(Me.txtAmount.Text.Trim()), CURRENCY_DISPLAY_FORMAT)
                    dtCurrentTable.Rows(i)("Desc") = "LOAN DISBURSEMENT"
                    dtCurrentTable.Rows(i)("GL Account") = P_Settlements_LiabilityGL_Control
                    RetailPostingGridView.DataSource = dtCurrentTable
                    RetailPostingGridView.DataBind()
                    ViewState("CurrentTable") = dtCurrentTable
                    'GetAccountDetailMap(txtSettlementAccountNo.Text.Trim())
                    'P_AvailBal = P_AccountDetailsHashMap("AvailBal")
                    'P_AvailBal = GetAccountBal(txtSettlementAccountNo.Text.Trim()) + Format(Double.Parse(Me.txtAmount.Text.Trim()), CURRENCY_DISPLAY_FORMAT)
                    'P_BookBalance = P_AvailBal
                    'dump in the temp table here 
                    Dim avBal As Decimal = 0.00
                    Dim bkBal As Decimal = 0.00
                    GetAccountDetailMap(Me.txtSettlementAccountNo.Text.Trim())
                    avBal = P_AccountDetailsHashMap("AvailBal")
                    bkBal = P_AccountDetailsHashMap("BookBalance")
                    P_CustomerNo = P_AccountDetailsHashMap("CustomerNo")
                    P_AccountDetailsHashMap.Clear()
                    P_AvailBal = avBal + Format(Double.Parse(Me.txtAmount.Text.Trim()), CURRENCY_DISPLAY_FORMAT)
                    P_BookBalance = bkBal + Format(Double.Parse(Me.txtAmount.Text.Trim()), CURRENCY_DISPLAY_FORMAT)
                    TxnType = "C"
                    TxnReference = _module & (TxnCode).ToUpper() & SeqNo & i + 1 & _day & _month & _year
                    txnRefNo = _module & (TxnCode).ToUpper() & SeqNo & _day & _month & _year
                    InsertGridRecords(i + 1, SeqNo.Trim(), P_Settlements_LiabilityGL_Control, Me.txtCRTransactionCode.Text.ToUpper.Trim(), Me.txtSettlementAccountNo.Text.Trim(), Me.lblAccountName.Text.Trim(), TxnType, branchID.Trim(), branchID.Trim(), Me.txtAmount.Text.Trim(), fxRate, depositAmount, dtCurrentTable.Rows(i)("Desc"), P_DefinationCode, TxnReference, UserId.Trim(), TxnDate, actionDate, SeqNo, P_BookBalance, P_AvailBal, txnRefNo, P_CustomerNo)
                End If
            Next
            lblTotalCredits.Text = txtAmount.Text
            lblTotalDebit.Text = txtAmount.Text
        End With
    End Sub
    Private Function GetAccountDetailMap(accountNo) As Hashtable
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select AccountNo,CustomerNo,AccountName,AssetGL_Control,LiabilityGL_Control,BranchCode,AvailBal,BookBalance,ProductDefCode from [dbo].[tbAccount] where AccountNo = '" & accountNo.Trim & "'"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                P_AccountDetailsHashMap.Add("CustomerNo", MyReader4("CustomerNo").ToString)
                P_AccountDetailsHashMap.Add("AccountName", MyReader4("AccountName").ToString)
                P_AccountDetailsHashMap.Add("AssetGLControl", MyReader4("AssetGL_Control").ToString)
                P_AccountDetailsHashMap.Add("LiabilityGLControl", MyReader4("LiabilityGL_Control").ToString)
                P_AccountDetailsHashMap.Add("BranchCode", MyReader4("BranchCode").ToString)
                P_AccountDetailsHashMap.Add("AvailBal", MyReader4("AvailBal").ToString)
                P_AccountDetailsHashMap.Add("BookBalance", MyReader4("BookBalance").ToString)
                P_AccountDetailsHashMap.Add("DefinationCode", MyReader4("ProductDefCode").ToString())
            End If
            MyReader4.Close()
        Catch ex As Exception
            'Throw New Exception("Error Loading Account Details", ex)
            'lblSuccessMsg.Text = ex.ToString
            'Exit Function
        End Try
        'Next
        Return P_AccountDetailsHashMap
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
            '  Me.txtNarration.Text = ""
        End If
    End Sub
    Private Sub btnClearForm_Click(sender As Object, e As EventArgs) Handles btnClearForm.Click
        Me.refreshPage()
    End Sub

    Private Sub refreshForm()
        Me.txtTransactionCode.Text = ""
        Me.txtCRTransactionCode.Text = ""
        Me.lblDescription.Text = ""
        Me.lblCRDescription.Text = ""
        Me.txtAccountNo.Text = ""
        Me.txtSettlementAccountNo.Text = ""
        Me.lblAccountName.Text = ""
        Me.txtSettlementAccountName.Text = ""
        Me.txtAmount.Text = ""
        lblTotalCredits.Text = "0"
        lblTotalDebit.Text = "0"

        RetailPostingGridView.DataSource = Nothing
        RetailPostingGridView.DataBind()
    End Sub
    Private Sub InsertGridRecords(rowCount, customerSeqNo, LiabilityGL_Control, transCode, customerNo, accountName, transType, branchID, branchIDs, depositAmount, fxRate, depositAmounts, Desc, _module, TxnReference, UserId, TxnDate, actionDate, seqno, bookBal, availBal, txnRefNo, custNo)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "Insert into MB_RetailPosting_Temp([RowNum]
      ,[SeqNo],[LiabilityGL_Control],[TxnCode],[AccountNo],[AccountName],[TxnType],[TxnBranch],[AccBanch],[Acc_CCY],[Amount],[FX_Rate],[LCY_Amount],[Details],[RCT_No],[INV_No],[Module],[Reference],[PayBy],[ChequeNo]
      ,[DR_GL],[CR_GL],[PostedBy],[TxnDate],[ActDate],[Journal],[TxnRefNo],[ExtSeqNo],[BookBalance],[AvailBal],[CustomerNo]) values('" & rowCount & "','" & customerSeqNo.Trim() & "','" & LiabilityGL_Control & "','" & transCode & "', " &
                                        "'" & customerNo.Trim() & "','" & accountName.Trim() & "','" & transType & "', " &
                    "'" & branchID.Trim() & "','" & branchID.Trim() & "', " &
                    " '" & "GMD" & "','" & depositAmount & "', " &
                    "'" & fxRate & "','" & depositAmount & "', " &
                    "'" & Desc & "','" & "00" & "', " &
                    "'" & "" & "','" & _module & "','" & TxnReference & "', " &
                    "'" & "" & "','" & "" & "', " &
                    "'" & "" & "','" & "" & "', " &
                    "'" & UserId.Trim() & "','" & TxnDate & "','" & actionDate & "',1,'" & txnRefNo & "','" & seqno & "','" & bookBal & "','" & availBal & "','" & custNo & "') "
        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    End Sub
    Private Sub InsertDailyTransaction(AccountNo, _amount, TxnType)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        Dim Desc As String = ""

        customerInsertSQl = "INSERT INTO [dbo].[MB_Acc_DailyTransactions] VALUES"

        UtilitiesModule._GetAccountDetails(AccountNo)
        Desc = lblDescription.Text.Trim.ToUpper


        'mySqlAction = customerInsertSQl & "('" & customerSeqNo.Trim() & "', '" & customerNo.Trim() & "','" & UtilitiesModule.P_AccountName & "','" & TxnType & "', " &
        '        "'" & branchID.Trim() & "','" & branchID.Trim() & "', '" & "GMD" & "','" & _amount & "', '" & fxRate & "','" & _amount & "', '" & lblCRDescription.Text & "',
        '        '" & "00" & "', " & "'" & "" & "','" & TxnReference & "', " & "'" & "" & "','" & "" & "', " & "'" & "" & "','" & "" & "', " & "'" & UserId.Trim() & "',
        '        '" & TxnDate & "','" & actionDate & "',1,'" & SeqNo.Trim() & "')"


        'mySqlAction = customerInsertSQl & "('" & customerSeqNo.Trim() & "', 
        '          '" & AccountNo & "','" & UtilitiesModule.P_AccountName & "','" & "D" & "', 
        '          '" & branchID.Trim() & "','" & branchID.Trim() & "', 
        '          '" & "GMD" & "','" & depositAmount & "', 
        '          '" & fxRate & "','" & depositAmount & "', 
        '          '" & Desc & "','" & "00" & "','" & _module & "','" & TxnReference & "', 
        '          '" & Pay & "','" & CheqNo & "', 
        '          '" & DRGL & "','" & CRGL & "', 
        '          '" & UserId.Trim() & "','" & TxnDate & "','" & actionDate & "',
        '          '" & Journal & "' 1,'" & SeqNo.Trim() & "' , '" & ExtSeqNo & "')"



        mySqlAction = customerInsertSQl & "('" & customerSeqNo.Trim() & "', 
                  '" & AccountNo & "','" & UtilitiesModule.P_AccountName & "','" & "D" & "', 
                  '" & branchID.Trim() & "','" & branchID.Trim() & "', 
                  '" & "GMD" & "','" & depositAmount & "', 
                  '" & fxRate & "','" & depositAmount & "', 
                  '" & Desc & "','" & "00" & "','" & _module & "','" & TxnReference & "', 
                  '" & "" & "','" & "" & "', 
                  '" & "" & "','" & "" & "','" & "" & "', 
                  '" & UserId.Trim() & "','" & TxnDate & "','" & actionDate & "',
                  '" & "" & "', 1,'" & TxnReference.Trim() & "' , '" & "" & "')"

        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    End Sub
    Private Sub InsertDailyTransactionHistory(AccountNo, _amount, TxnType)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        Dim Desc As String = ""
        customerInsertSQl = "INSERT INTO [dbo].[MB_Acc_DailyTransactions_History] VALUES"
        UtilitiesModule._GetAccountDetails(AccountNo)
        Desc = lblDescription.Text.Trim.ToUpper

        mySqlAction = customerInsertSQl & "('" & customerSeqNo.Trim() & "', 
                  '" & AccountNo & "','" & UtilitiesModule.P_AccountName & "','" & "D" & "', 
                  '" & branchID.Trim() & "','" & branchID.Trim() & "', 
                  '" & "GMD" & "','" & depositAmount & "', 
                  '" & fxRate & "','" & depositAmount & "', 
                  '" & Desc & "','" & "00" & "','" & _module & "','" & TxnReference & "', 
                  '" & "" & "','" & "" & "', 
                  '" & "" & "','" & "" & "','" & "" & "', 
                  '" & UserId.Trim() & "','" & TxnDate & "','" & actionDate & "',
                  '" & "" & "' ,1,'" & TxnReference.Trim() & "' , '" & "" & "')"

        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    End Sub
    Private Sub InsertGLTransaction(AccountNo, _amount, TxnType, i)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        Dim Desc As String = ""
        GLTransactionInsertSQl = "INSERT INTO [dbo].[MB_GL_DailyTransaction] VALUES"
        UtilitiesModule._GetAccountDetails(AccountNo)
        Desc = lblDescription.Text.Trim.ToUpper

        mySqlAction = GLTransactionInsertSQl & "('" & SeqNo.Trim() & "', " &
                                                " '" & AccountNo & "','" & UtilitiesModule.P_AccountName & "', " &
                    "'" & TxnType & "','" & branchID.Trim() & "', " &
                    "'" & branchID.Trim() & "','" & "GMD" & "', " &
                    "'" & depositAmount & "','" & fxRate & "', " &
                    "'" & depositAmount & "','" & Desc & "', " &
                    "'','" & _module & "','" & TxnReference & "','" & UserId & "', " &
                    "'" & TxnDate & "','" & actionDate & "','" & i & "','" & TxnReference & "','')"

        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    End Sub
    Private Sub InsertGLTransactionHistory(AccountNo, _amount, TxnType, i)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        Dim Desc As String = ""
        GLTransactionInsertSQl = "INSERT INTO [dbo].[MB_GL_DailyTransaction_History] VALUES"
        UtilitiesModule._GetAccountDetails(AccountNo)
        Desc = lblDescription.Text.Trim.ToUpper

        mySqlAction = GLTransactionInsertSQl & "('" & SeqNo.Trim() & "', " &
                                                " '" & AccountNo & "','" & UtilitiesModule.P_AccountName & "', " &
                    "'" & TxnType & "','" & branchID.Trim() & "', " &
                    "'" & branchID.Trim() & "','" & "GMD" & "', " &
                    "'" & depositAmount & "','" & fxRate & "', " &
                    "'" & depositAmount & "','" & Desc & "', " &
                    "'','" & _module & "','" & TxnReference & "','" & UserId & "', " &
                    "'" & TxnDate & "','" & actionDate & "','" & i & "','" & TxnReference & "','')"

        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    End Sub

    Private Sub InsertAllDailyTransaction(AccountNo, _amount, TxnType, i)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        Dim Desc As String = ""
        AllTransactionInsertSQl = "INSERT INTO [dbo].[MB_All_DailyTransactions] VALUES"
        UtilitiesModule._GetAccountDetails(AccountNo)
        Desc = lblDescription.Text.Trim.ToUpper

        mySqlAction = AllTransactionInsertSQl & "('" & SeqNo.Trim() & "', " &
                                                " '" & AccountNo & "','" & UtilitiesModule.P_AccountName & "', " &
                    "'" & TxnType & "','" & branchID.Trim() & "', " &
                    "'" & branchID.Trim() & "','" & "GMD" & "', " &
                    "'" & depositAmount & "','" & fxRate & "', " &
                    "'" & depositAmount & "','" & Desc & "', " &
                    "'','" & _module & "','" & TxnReference & "','" & UserId & "', " &
                    "'" & TxnDate & "','" & actionDate & "','" & i & "','" & TxnReference & "','')"

        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    End Sub

    Private Sub InsertAllDailyTransactionHistory(AccountNo, _amount, TxnType, i)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        Dim Desc As String = ""
        AllTransactionInsertSQl = "INSERT INTO [dbo].[MB_All_DailyTransactions_History] VALUES"
        UtilitiesModule._GetAccountDetails(AccountNo)
        Desc = lblDescription.Text.Trim.ToUpper

        'mySqlAction = customerInsertSQl & "('" & customerSeqNo.Trim() & "', '" & customerNo.Trim() & "','" & UtilitiesModule.P_AccountName & "','" & TxnType & "', 
        '                                    '" & branchID.Trim() & "','" & branchID.Trim() & "', '" & "GMD" & "','" & _amount & "', '" & fxRate & "','" & _amount & "', 
        '                                    '" & lblCRDescription.Text & "', '" & "00" & "', " & "'" & "" & "','" & TxnReference & "','" & UserId.Trim() & "',
        '                                    '" & TxnDate & "','" & actionDate & "',1,'" & SeqNo.Trim() & "')"

        mySqlAction = AllTransactionInsertSQl & "('" & SeqNo.Trim() & "', " &
                                                " '" & AccountNo & "','" & UtilitiesModule.P_AccountName & "', " &
                    "'" & TxnType & "','" & branchID.Trim() & "', " &
                    "'" & branchID.Trim() & "','" & "GMD" & "', " &
                    "'" & depositAmount & "','" & fxRate & "', " &
                    "'" & depositAmount & "','" & Desc & "', " &
                    "'','" & _module & "','" & TxnReference & "','" & UserId & "', " &
                    "'" & TxnDate & "','" & actionDate & "','" & i & "','" & TxnReference & "','')"

        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    End Sub





    Private Sub DropTempTable(accountno)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "delete from MB_RetailPosting_Temp where AccountNo = '" & accountno & "' "
        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Delete)
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
        ' Me.txtNarration.Text = ""
        ' Me.txtNarration.Enabled = True
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