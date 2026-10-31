Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Imports System.Data.SqlClient
Public Class SOD
    Inherits System.Web.UI.Page
    Public P_TxnCode As String = "", P_TxnDesc As String = "", P_TxnType As String = "", P_ChargeAmt As String = "", P_ChqNo As String = "", P_TxnCash As String = "", P_PostMode As String = "", P_ContraGL As String = "", P_TelleID As String = "", P_AttachedTxnCode As String = "", P_RefCode As String = ""
    Public P_ProdRefCode As String = "", P_refNo As String = ""
    Public P_AccountDetailsHashMap As New Hashtable
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Protected Sub btnSod_Click(sender As Object, e As EventArgs) Handles btnSod.Click
        lblDate.Text = Date.Now.ToString("yyyy/MM/dd")
        Dim dateToday As Date = Date.Now.ToString("yyyy/MM/dd")
        If lblDate.Text <> dateToday Then
            lblSuccessMsg.Text = "Start of day process Date is not correct"
        Else
            PostTransactions()
        End If

        lblSuccessMsg.Text = "Start of Day Process Completed Successfully"
    End Sub
    Protected Sub PostTransactions()
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch2 As String = "select count(SOD_Status) as Count from tbAccount where SOD_Status = 0 and ProductDefCode = 'LA'"
            Dim MyReader2 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch2, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader2.HasRows = True Then
                MyReader2.Read()
                Dim count As Integer = 0
                Dim newCount As Integer = 0
                newCount = MyReader2("Count").ToString
                While count < newCount
                    Dim mySearch4 As String = "select AccountNo,AccountName,SettlementsAccountNo,RepaymentAmount,AssetGL_Control,LiabilityGL_Control,ProductDefCode,AvailBal,BookBalance from tbAccount where SOD_Status = 0"
                    Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
                    If MyReader4.HasRows = True Then
                        MyReader4.Read()
                        Dim TransCode As String = ""
                        Dim liabilityGL As String = ""
                        Dim loanAccGL As String = ""
                        Dim settlementAccGL As String = ""
                        Dim loanTransType As String = "C"
                        Dim loanTransDescription As String = "REPAYMENT POSTINGS"
                        Dim settlementTransDescription As String = "REPAYMENT POSTINGS"
                        Dim settlementTransType As String = "D"
                        Dim repaymentAmount As Decimal = 0
                        Dim loanAccountBal As Decimal = 0.00
                        Dim settlementAccountBal As Decimal = 0.00
                        Dim loanAccNo As String = ""
                        Dim settlementAccNo As String = ""
                        Dim refNo As String = "12354"
                        Dim seqNo As String = "777"
                        ' P_RefCode = MyReader4("Journal").ToString
                        ' P_AccountNo = MyReader4("AccountNo").ToString
                        loanAccNo = MyReader4("AccountNo").ToString
                        loanAccGL = MyReader4("AssetGL_Control").ToString
                        repaymentAmount = MyReader4("RepaymentAmount").ToString
                        settlementAccNo = MyReader4("SettlementsAccountNo").ToString
                        loanAccountBal = MyReader4("AvailBal").ToString
                        'settlementAccountBal = MyReader4("SettlementsAccountNo").ToString
                        'insert loan account details
                        InsertGridRecords(loanAccNo, repaymentAmount, loanTransType, loanAccGL, refNo, seqNo, loanTransDescription)
                        PostRecord(loanAccNo)
                        UpdateAccountBalances(loanAccNo, repaymentAmount, loanTransType, loanAccountBal)
                        UpdateCustomerGL(loanAccGL, repaymentAmount, loanTransType, loanAccNo)
                        'get settlement account GL and balances
                        GetAccountDetails(settlementAccNo)
                        settlementAccountBal = P_AccountDetailsHashMap("AvailBal")
                        settlementAccGL = P_AccountDetailsHashMap("LiabilityGLControl")
                        'insert settlement account details
                        InsertGridRecords(settlementAccNo, repaymentAmount, settlementTransType, settlementAccGL, refNo, seqNo, settlementTransDescription)
                        PostRecord(settlementAccNo)
                        UpdateAccountBalances(settlementAccNo, repaymentAmount, settlementTransType, settlementAccountBal)
                        UpdateCustomerGL(settlementAccGL, repaymentAmount, settlementTransType, settlementAccNo)
                        count = count + 1
                    End If
                    MyReader4.Close()
                End While
            End If

            MyReader2.Close()
        Catch ex As Exception
            lblSuccessMsg.Text = ex.ToString
        End Try
    End Sub
    Public Function GetAccountDetails(accountNo) As Hashtable
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
            lblSuccessMsg.Text = ex.ToString
            Exit Function
        End Try
        'Next
        Return P_AccountDetailsHashMap
    End Function
    Protected Sub PostRecord(accNumber)
        Try
            'GetTransactionCode(TransCode)
            'accountBal = GetAccountBal(P_AccountNo)
            InsertDailyTransaction(accNumber)
            InsertDailyTransactionHistory(accNumber)
            InsertGLTransaction(accNumber)
            InsertGLTransactionHistory(accNumber)
            ''confirm this all in all modules
            InsertAllDailyTransaction(accNumber)
            InsertAllDailyTransactionHistory(accNumber)
            'UpdateCustomerBalances(P_AccountNo, amounts, transType, P_BookBalance)

            'UpdateCashierGL(TransCode, amounts)
            UpdateSOD_Status(accNumber)
        Catch ex As Exception
            'Label4.Text = "Error Pulling Account Details"
        End Try
    End Sub
    Protected Sub UpdateAccountBalances(accountNo, amount, transType, P_BookBalance)
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
                        'UpdateAllGLNames(P_AccountNo, P_LiabilityGL_Control, P_refNo, GLName)
                        'UpdateAllGLNamesHistory(P_AccountNo, P_LiabilityGL_Control, P_refNo, GLName)
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
                        Dim GLName As String = ""
                        GLBal = MyReader4("ACT_BAL").ToString
                        GLName = MyReader4("GLNAME").ToString
                        P_LiabilityGL_Control = MyReader4("GLACCTNO").ToString
                        'dunga
                        'P_LiabilityGL_Control = MyReader4("GLAccount").ToString
                        'UpdateAllGLNames(P_AccountNo, P_LiabilityGL_Control, P_refNo, GLName)
                        'UpdateAllGLNamesHistory(P_AccountNo, P_LiabilityGL_Control, P_refNo, GLName)
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
    Protected Sub GetTransactionCode(transCode)
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select TxnCode,TxnDesc,TxnType,ChargeAmt,ChqNo,TxnCash,PostMode,ContraGL,TelleID,AttachedTxnCode,RefCode from tbTransactionCode where TxnCode = '" & transCode & "'"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                'lblDescription.Text = MyReader4("TxnDesc").ToString
                P_TxnType = MyReader4("TxnType").ToString
                P_TxnCode = MyReader4("TxnCode").ToString
                P_PostMode = MyReader4("PostMode").ToString
                P_ContraGL = MyReader4("ContraGL").ToString
                P_TelleID = MyReader4("TelleID").ToString
                P_RefCode = MyReader4("RefCode").ToString
                If P_TxnCode = "D00" Or P_TxnCode = "C00" Then
                Else
                    lblSuccessMsg.Text = "Invalid transaction code"
                    'lblDescription.Text = ""
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
    Private Sub InsertGridRecords(accsNo, amounts, transType, accountGL, P_refNo, seqNo, transDesc)

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
        MyDBAction.DBAction(mySqlAction1, DataManagement.DBActionType.Update)
    End Sub
End Class