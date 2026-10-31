Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Imports System.Data.SqlClient
Public Class CashWithdrawal
    Inherits System.Web.UI.Page
    Public P_AccountNo As String = "", P_CustomerNo As String = "", P_AccountName As String = "", P_AssetGL_Control As String = "", P_LiabilityGL_Control As String = "", P_BranchCode As String = "", P_AvailBal As Decimal = 0.00, P_BookBalance As Decimal = 0.00
    Public P_TxnCode As String = "", P_TxnDesc As String = "", P_TxnType As String = "", P_ChargeAmt As String = "", P_ChqNo As String = "", P_TxnCash As String = "", P_PostMode As String = "", P_ContraGL As String = "", P_TelleID As String = "", P_AttachedTxnCode As String = "", P_RefCode As String = ""
    Const CURRENCY_DISPLAY_FORMAT As String = "##,##0.00"
    Public P_ProdRefCode As String = ""
    Private P_AccountDetailsHashMap As New Hashtable
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
        ViewState("CurrentTable") = dta
        CashTxnGridView.DataSource = dta
        CashTxnGridView.DataBind()
    End Sub
    Protected Sub LinkDelete_Click(sender As Object, e As EventArgs)
        Dim lb As LinkButton = DirectCast(sender, LinkButton)
        Dim gvRow As GridViewRow = DirectCast(lb.NamingContainer, GridViewRow)
        Dim rowID As Integer = gvRow.RowIndex
        If ViewState("CurrentTable") IsNot Nothing Then
            Dim dt As DataTable = DirectCast(ViewState("CurrentTable"), DataTable)
            If dt.Rows.Count > 0 Or dt.Rows.Count = 0 Then
                'If gvRow.RowIndex < dt.Rows.Count - 1 Then
                dt.Rows.Remove(dt.Rows(rowID))
                ResetRowID(dt)
                'End If
            End If
            ViewState("CurrentTable") = dt
            CashTxnGridView.DataSource = dt
            CashTxnGridView.DataBind()
        End If
    End Sub
    Private Sub ResetRowID(dt As DataTable)
        Dim rowNumber As Integer = 1
        If dt.Rows.Count > 0 Then
            For Each row As DataRow In dt.Rows
                row(0) = rowNumber
                rowNumber += 1
            Next
        End If
    End Sub
    Protected Sub CmdApprovalList_Click(sender As Object, e As EventArgs) Handles CmdApprovalList.Click
        txtAmount.Text = ""
        txtNewBalance.Text = ""
        txtRefNo.Text = ""
        CashTxnGridView.DataSource = Nothing
        CashTxnGridView.DataBind()
        lblDescription.Text = ""
        lblAccountName.Text = ""
        txtAccountNo.Text = ""
        txtTransactionCode.Text = ""
        lblSuccessMsg.Text = ""
    End Sub
    Protected Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        txtAmount.Text = ""
        txtNewBalance.Text = ""
        txtRefNo.Text = ""
        CashTxnGridView.DataSource = Nothing
        CashTxnGridView.DataBind()
        lblDescription.Text = ""
        lblAccountName.Text = ""
        txtAccountNo.Text = ""
        txtTransactionCode.Text = ""
        lblSuccessMsg.Text = ""
    End Sub
    Protected Sub CashTxnGridView_RowCreated(sender As Object, e As GridViewRowEventArgs)
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
    Protected Sub btnPost_Click(sender As Object, e As EventArgs) Handles btnPost.Click
        GetAccountDetails()
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
        If txtAmount.Text <= 0.00 Then
            lblSuccessMsg.Text = "Amount should be more than 0.00"
            txtAmount.Focus()
            Exit Sub
        Else
            lblSuccessMsg.Text = ""
        End If
        If txtAmount.Text > P_BookBalance Then
            lblSuccessMsg.Text = "This account have insufficient funds"
            Exit Sub
        Else
            lblSuccessMsg.Text = ""
        End If
        Dim customerAccountName As String = ""
        Dim d As String = ""
        Dim _contraGl As String = ""
        Dim LiabilityGL_Control As String = ""
        Dim contraGl_Name As String = ""
        Dim LiabilityGL_Control_Name As String = ""
        UtilitiesModule._GetAccountDetails(Me.txtAccountNo.Text.Trim)
        UtilitiesModule.GetTransactionCodeDetails(Me.txtTransactionCode.Text.Trim())
        ' UtilitiesModule.GetTransactionCodeDetails(Me.txtTransactionCode.Text.Trim())
        customerAccountName = UtilitiesModule.P_AccountName.Trim()
        _contraGl = UtilitiesModule.Contra_GL
        LiabilityGL_Control = UtilitiesModule.P_LiabilityGL_Control
        Dim dr As DataRow = Nothing
        Dim dtCurrentTable As DataTable = DirectCast(ViewState("CurrentTable"), DataTable)
        ' Dim rowCount As Integer = dtCurrentTable.Rows.Count()
        For i = 1 To 3
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
                    dtCurrentTable.Rows(i)("Desc") = "CASH WITHDRWAL"
                    CashTxnGridView.DataSource = dtCurrentTable
                    CashTxnGridView.DataBind()
                    ViewState("CurrentTable") = dtCurrentTable
                ElseIf i = 1 Then
                    UtilitiesModule.GetGlAccoutNames(_contraGl.Trim())
                    contraGl_Name = UtilitiesModule.GLName.Trim()
                    dtCurrentTable.Rows(i)("RowNumber") = i + 1
                    dtCurrentTable.Rows(i)("Account No") = _contraGl.Trim()
                    dtCurrentTable.Rows(i)("ACCOUNT NAME") = contraGl_Name.Trim()
                    dtCurrentTable.Rows(i)("TXN TYPE") = "C"
                    dtCurrentTable.Rows(i)("Amount") = Format(Double.Parse(Me.txtAmount.Text.Trim()), CURRENCY_DISPLAY_FORMAT)
                    dtCurrentTable.Rows(i)("Desc") = "CASH WITHDRWAL"
                    CashTxnGridView.DataSource = dtCurrentTable
                    CashTxnGridView.DataBind()
                    ViewState("CurrentTable") = dtCurrentTable
                ElseIf i = 2 Then
                    UtilitiesModule.GetGlAccoutNames(LiabilityGL_Control.Trim())
                    LiabilityGL_Control_Name = UtilitiesModule.GLName.Trim()
                    dtCurrentTable.Rows(i)("RowNumber") = i + 1
                    dtCurrentTable.Rows(i)("Account No") = LiabilityGL_Control.Trim()
                    dtCurrentTable.Rows(i)("ACCOUNT NAME") = LiabilityGL_Control_Name.Trim()
                    dtCurrentTable.Rows(i)("TXN TYPE") = "D"
                    dtCurrentTable.Rows(i)("Amount") = Format(Double.Parse(Me.txtAmount.Text.Trim()), CURRENCY_DISPLAY_FORMAT)
                    dtCurrentTable.Rows(i)("Desc") = "CASH WITHDRWAL"
                    CashTxnGridView.DataSource = dtCurrentTable
                    CashTxnGridView.DataBind()
                    ViewState("CurrentTable") = dtCurrentTable
                End If
            Next
        End With
    End Sub
    Private Sub saveGridTxnRecords()
        GetTransactionCode()
        '1. Explain this concept and see if its necessary
        'JP_no1 = GetJournalNo() ' Module JP
        'JP_no = JP_no1 & "/" & CurrentYear
        Dim refNo As String = ""
        Dim depositAmount As Double = 0
        Dim UserId As String = "Lamin"
        Dim TxnReference As String = ""
        Dim txnRefNo As String = ""
        Dim TxnCode As String = ""
        Dim TxnDate As DateTime = Date.Now()
        Dim actionDate As String = ""
        Dim customerSeqNo As String = ""
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
        actionDate = UtilitiesModule.GetTransctionDate()
        UtilitiesModule._GetAccountDetails(Me.txtAccountNo.Text.Trim)
        UtilitiesModule.GetNextSeqNo("CUSTOMER_REFNO")
        customerSeqNo = UtilitiesModule.returnValue
        UtilitiesModule.GetNextSeqNo("LAST_SEQNO")
        GetAccountDetails(Me.txtAccountNo.Text.Trim)
        GetProductDefinition(P_DefinationCode)
        _module = P_ProdRefCode
        '_module = UtilitiesModule.P_DefinationCode.Trim()
        SeqNo = UtilitiesModule.returnValue
        customerNo = txtAccountNo.Text.Trim()
        TxnCode = P_RefCode
        'TxnCode = Me.txtTransactionCode.Text.Trim()
        _day = (TxnDate.Day).ToString().PadLeft(2, "0")
        _month = (TxnDate.Month).ToString().PadLeft(2, "0")
        _year = (TxnDate.Year).ToString().Substring(2, 2)
        '1. Get Populated Table With Its Content
        Dim dtCurrentTable As DataTable = DirectCast(ViewState("CurrentTable"), DataTable)
        '2. Get Row Count: Here Row Count Should Be Three
        Dim rowCount As Integer = dtCurrentTable.Rows.Count()
        '3. Loop Through Grid
        With dtCurrentTable
            For i = 0 To rowCount - 1
                '4. Insert row one into customer table
                customerInsertSQl = ""
                dailyTransactionInsertSQl = ""
                refNo = ""
                'TxnReference = _module & (TxnCode).ToUpper() & SeqNo & rowCount & _day & _month & _year
                'txtRefNo.Text = TxnReference
                Dim drCurrentRow As DataRow = dtCurrentTable.Rows(i)
                If i > 0 Then
                    refNo = SeqNo & drCurrentRow("Account No") & i
                End If
                If i = 0 Then
                    GetAccountDetailMap(customerNo.Trim())
                    P_AvailBal = P_AccountDetailsHashMap("AvailBal")
                    P_LiabilityGL_Control = P_AccountDetailsHashMap("LiabilityGLControl")
                    P_CustomerNo = P_AccountDetailsHashMap("CustomerNo")
                    P_AccountNo = customerNo.Trim()
                    P_BookBalance = P_AvailBal
                    'unique reference number 
                    TxnReference = _module & (TxnCode).ToUpper() & SeqNo & i + 1 & _day & _month & _year
                    'transaction reference number this have to be the same for the same batch
                    txnRefNo = _module & (TxnCode).ToUpper() & SeqNo & _day & _month & _year
                    txtRefNo.Text = txnRefNo
                    '5. Insert row two as a debit entry into The Daily Transaction Table
                    customerInsertSQl = "INSERT INTO [dbo].[MB_Acc_DailyTransactions] (SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,RCT_No,INV_No,Module,Reference,ChequeNo,DR_GL,CR_GL,PostedBy,TxnDate,ActDate,Journal,SrNo,TxnRefNo,ExtSeqNo,BookBalance,AvailBal,GL_Control,CustomerNo,TxnCode) VALUES"
                    customerInsertSQl = customerInsertSQl & "('" & SeqNo.Trim() & "', " &
                                        "'" & customerNo.Trim() & "','" & drCurrentRow("ACCOUNT NAME") & "','" & "D" & "', " &
                    "'" & branchID.Trim() & "','" & branchID.Trim() & "', " &
                    " '" & "GMD" & "','" & depositAmount & "', " &
                    "'" & fxRate & "','" & depositAmount & "', " &
                    "'" & drCurrentRow("Desc") & "','" & "00" & "', " &
                    "'','" & P_DefinationCode & "', " &
                    "'" & TxnReference & "','" & "" & "', " &
                    "'" & "" & "','" & "" & "', " &
                    "'" & UserId.Trim() & "','" & TxnDate & "','" & actionDate & "',1,'" & i + 1 & "','" & txnRefNo & "','" & SeqNo.Trim() & "','" & P_BookBalance & "','" & P_AvailBal & "','" & P_LiabilityGL_Control & "','" & P_CustomerNo & "','" & txtTransactionCode.Text.ToUpper.Trim & "')"
                    Me.InsertTransactions(customerInsertSQl)
                    '6. Insert row two as a debit entry into The Daily Transaction Table History
                    customerInsertSQl = "INSERT INTO [dbo].[MB_Acc_DailyTransactions_History]  (SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,RCT_No,INV_No,Module,Reference,ChequeNo,DR_GL,CR_GL,PostedBy,TxnDate,ActDate,Journal,SrNo,TxnRefNo,ExtSeqNo,BookBalance,AvailBal,GL_Control,CustomerNo,TxnCode) VALUES"
                    customerInsertSQl = customerInsertSQl & "('" & SeqNo.Trim() & "', " &
                                        "'" & customerNo.Trim() & "','" & drCurrentRow("ACCOUNT NAME") & "','" & "D" & "', " &
                    "'" & branchID.Trim() & "','" & branchID.Trim() & "', " &
                    " '" & "GMD" & "','" & depositAmount & "', " &
                    "'" & fxRate & "','" & depositAmount & "', " &
                    "'" & drCurrentRow("Desc") & "','" & "00" & "', " &
                    "'','" & P_DefinationCode & "', " &
                    "'" & TxnReference & "','" & "" & "', " &
                    "'" & "" & "','" & "" & "', " &
                    "'" & UserId.Trim() & "','" & TxnDate & "','" & actionDate & "',1,'" & i + 1 & "','" & txnRefNo & "','" & SeqNo.Trim() & "','" & P_BookBalance & "','" & P_AvailBal & "','" & P_LiabilityGL_Control & "','" & P_CustomerNo & "','" & txtTransactionCode.Text.ToUpper.Trim & "')"
                    Me.InsertTransactions(customerInsertSQl)
                    'insert in all table
                    customerInsertSQl = "INSERT INTO [dbo].[MB_All_DailyTransactions] (SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccountBranch,AcCCY,Amount,FX_Rate,LCY_Amount,Description,InvoiceNo,Module,Reference,SrNo,UserName,TxnDate,ActDate,TxnRefNo,ExtRefNo,Contra_AccNo,CustomerNo,TxnCode) VALUES"
                    customerInsertSQl = customerInsertSQl & "('" & SeqNo.Trim() & "', " &
                                        "'" & customerNo.Trim() & "','" & drCurrentRow("ACCOUNT NAME") & "','" & "D" & "', " &
                    "'" & branchID.Trim() & "','" & branchID.Trim() & "', " &
                    " '" & "GMD" & "','" & depositAmount & "', " &
                    "'" & fxRate & "','" & depositAmount & "', " &
                    "'" & drCurrentRow("Desc") & "','" & "00" & "', " &
                    "'" & P_DefinationCode & "','" & TxnReference & "', " &
                    "'" & i + 1 & "', " &
                    "'" & UserId.Trim() & "','" & TxnDate & "','" & actionDate & "','" & txnRefNo & "','" & SeqNo.Trim() & "','" & P_LiabilityGL_Control & "','" & P_CustomerNo & "','" & txtTransactionCode.Text.ToUpper.Trim & "')"
                    Me.InsertTransactions(customerInsertSQl)
                    'all history
                    customerInsertSQl = "INSERT INTO [dbo].[MB_All_DailyTransactions_History] (SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccountBranch,AcCCY,Amount,FX_Rate,LCY_Amount,Description,InvoiceNo,Module,Reference,SrNo,UserName,TxnDate,ActDate,TxnRefNo,ExtRefNo,Contra_AccNo,CustomerNo,TxnCode) VALUES"
                    customerInsertSQl = customerInsertSQl & "('" & SeqNo.Trim() & "', " &
                                        "'" & customerNo.Trim() & "','" & drCurrentRow("ACCOUNT NAME") & "','" & "D" & "', " &
                    "'" & branchID.Trim() & "','" & branchID.Trim() & "', " &
                    " '" & "GMD" & "','" & depositAmount & "', " &
                    "'" & fxRate & "','" & depositAmount & "', " &
                    "'" & drCurrentRow("Desc") & "','" & "00" & "', " &
                    "'" & P_DefinationCode & "','" & TxnReference & "', " &
                    "'" & i + 1 & "', " &
                    "'" & UserId.Trim() & "','" & TxnDate & "','" & actionDate & "','" & txnRefNo & "','" & SeqNo.Trim() & "','" & P_LiabilityGL_Control & "','" & P_CustomerNo & "','" & txtTransactionCode.Text.ToUpper.Trim & "')"
                    Me.InsertTransactions(customerInsertSQl)
                    'audit
                    customerInsertSQl = "INSERT INTO [dbo].[MB_TransactionsAuditTrail] (SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccountBranch,AcCCY,Amount,FX_Rate,LCY_Amount,Description,InvoiceNo,Module,Reference,SrNo,UserName,TxnDate,ActDate,TxnRefNo,ExtRefNo,Contra_AccNo,CustomerNo,TxnCode) VALUES"
                    customerInsertSQl = customerInsertSQl & "('" & SeqNo.Trim() & "', " &
                                        "'" & customerNo.Trim() & "','" & drCurrentRow("ACCOUNT NAME") & "','" & "D" & "', " &
                    "'" & branchID.Trim() & "','" & branchID.Trim() & "', " &
                    " '" & "GMD" & "','" & depositAmount & "', " &
                    "'" & fxRate & "','" & depositAmount & "', " &
                    "'" & drCurrentRow("Desc") & "','" & "00" & "', " &
                    "'" & P_DefinationCode & "','" & TxnReference & "', " &
                    "'" & i + 1 & "', " &
                    "'" & UserId.Trim() & "','" & TxnDate & "','" & actionDate & "','" & txnRefNo & "','" & SeqNo.Trim() & "','" & P_LiabilityGL_Control & "','" & P_CustomerNo & "','" & txtTransactionCode.Text.ToUpper.Trim & "')"
                    Me.InsertTransactions(customerInsertSQl)

                    'audit history
                    customerInsertSQl = "INSERT INTO [dbo].[MB_TransactionsAuditTrail_History] (SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccountBranch,AcCCY,Amount,FX_Rate,LCY_Amount,Description,InvoiceNo,Module,Reference,SrNo,UserName,TxnDate,ActDate,TxnRefNo,ExtRefNo,Contra_AccNo,CustomerNo,TxnCode) VALUES"
                    customerInsertSQl = customerInsertSQl & "('" & SeqNo.Trim() & "', " &
                                        "'" & customerNo.Trim() & "','" & drCurrentRow("ACCOUNT NAME") & "','" & "D" & "', " &
                    "'" & branchID.Trim() & "','" & branchID.Trim() & "', " &
                    " '" & "GMD" & "','" & depositAmount & "', " &
                    "'" & fxRate & "','" & depositAmount & "', " &
                    "'" & drCurrentRow("Desc") & "','" & "00" & "', " &
                    "'" & P_DefinationCode & "','" & TxnReference & "', " &
                    "'" & i + 1 & "', " &
                    "'" & UserId.Trim() & "','" & TxnDate & "','" & actionDate & "','" & txnRefNo & "','" & SeqNo.Trim() & "','" & P_LiabilityGL_Control & "','" & P_CustomerNo & "','" & txtTransactionCode.Text.ToUpper.Trim & "')"
                    Me.InsertTransactions(customerInsertSQl)
                ElseIf i = 1 Then
                    TxnReference = _module & (TxnCode).ToUpper() & SeqNo & i + 1 & _day & _month & _year
                    '7. Insert row three as a credit entry into The GL Transaction Table
                    dailyTransactionInsertSQl = "INSERT INTO [dbo].[MB_GL_DailyTransaction]([SeqNo]
      ,[GLAccNo]
      ,[GLName]
      ,[TxnType]
      ,[TxnBranch]
      ,[AccountBranch]
      ,[AcCCY]
      ,[Amount]
      ,[FX_Rate]
      ,[LCY_Amount]
      ,[Description]
      ,[InvoiceNo]
      ,[Module]
      ,[Reference]
      ,[UserName]
      ,[TxnDate]
      ,[ActDate]
      ,[SrNo]
      ,[TxnRefNo]
      ,[ExtRefNo]
      ,[AccountNo]
      ,[CustomerNo]
      ,[TxnCode])VALUES"
                    dailyTransactionInsertSQl = dailyTransactionInsertSQl & "('" & SeqNo.Trim() & "', " &
                                                " '" & drCurrentRow("Account No") & "','" & drCurrentRow("ACCOUNT NAME") & "', " &
                    "'" & drCurrentRow("TXN TYPE") & "','" & branchID.Trim() & "', " &
                    "'" & branchID.Trim() & "','" & "GMD" & "', " &
                    "'" & depositAmount & "','" & fxRate & "', " &
                    "'" & depositAmount & "','" & drCurrentRow("Desc") & "', " &
                    "'','" & P_DefinationCode & "','" & TxnReference & "','" & UserId & "', " &
                    "'" & TxnDate & "','" & actionDate & "','" & i + 1 & "','" & txnRefNo & "','','" & P_AccountNo & "','" & P_CustomerNo & "','" & txtTransactionCode.Text.ToUpper.Trim & "')"
                    Me.InsertTransactions(dailyTransactionInsertSQl)
                    '8. Insert row three as a credit entry into The GL Transaction Table History
                    dailyTransactionInsertSQl = "INSERT INTO [dbo].[MB_GL_DailyTransaction_History]([SeqNo]
      ,[GLAccNo]
      ,[GLName]
      ,[TxnType]
      ,[TxnBranch]
      ,[AccountBranch]
      ,[AcCCY]
      ,[Amount]
      ,[FX_Rate]
      ,[LCY_Amount]
      ,[Description]
      ,[InvoiceNo]
      ,[Module]
      ,[Reference]
      ,[UserName]
      ,[TxnDate]
      ,[ActDate]
      ,[SrNo]
      ,[TxnRefNo]
      ,[ExtRefNo]
      ,[AccountNo]
      ,[CustomerNo]
      ,[TxnCode]) VALUES"
                    dailyTransactionInsertSQl = dailyTransactionInsertSQl & "('" & SeqNo.Trim() & "', " &
                                                " '" & drCurrentRow("Account No") & "','" & drCurrentRow("ACCOUNT NAME") & "', " &
                    "'" & drCurrentRow("TXN TYPE") & "','" & branchID.Trim() & "', " &
                    "'" & branchID.Trim() & "','" & "GMD" & "', " &
                    "'" & depositAmount & "','" & fxRate & "', " &
                    "'" & depositAmount & "','" & drCurrentRow("Desc") & "', " &
                    "'','" & P_DefinationCode & "','" & TxnReference & "','" & UserId & "', " &
                    "'" & TxnDate & "','" & actionDate & "','" & i + 1 & "','" & txnRefNo & "','','" & P_AccountNo & "','" & P_CustomerNo & "','" & txtTransactionCode.Text.ToUpper.Trim & "')"
                    Me.InsertTransactions(dailyTransactionInsertSQl)
                    ' add all transactions here
                    dailyTransactionInsertSQl = "INSERT INTO [dbo].[MB_All_DailyTransactions]([SeqNo]
      ,[AccountNo]
      ,[AccountName]
      ,[TxnType]
      ,[TxnBranch]
      ,[AccountBranch]
      ,[AcCCY]
      ,[Amount]
      ,[FX_Rate]
      ,[LCY_Amount]
      ,[Description]
      ,[InvoiceNo]
      ,[Module]
      ,[Reference]
      ,[UserName]
      ,[TxnDate]
      ,[ActDate]
      ,[SrNo]
      ,[TxnRefNo]
      ,[ExtRefNo]
      ,[Contra_AccNo]
      ,[CustomerNo]
      ,[TxnCode]) VALUES"
                    dailyTransactionInsertSQl = dailyTransactionInsertSQl & "('" & SeqNo.Trim() & "', " &
                                                " '" & drCurrentRow("Account No") & "','" & drCurrentRow("ACCOUNT NAME") & "', " &
                    "'" & drCurrentRow("TXN TYPE") & "','" & branchID.Trim() & "', " &
                    "'" & branchID.Trim() & "','" & "GMD" & "', " &
                    "'" & depositAmount & "','" & fxRate & "', " &
                    "'" & depositAmount & "','" & drCurrentRow("Desc") & "', " &
                    "'','" & P_DefinationCode & "','" & TxnReference & "','" & UserId & "', " &
                    "'" & TxnDate & "','" & actionDate & "','" & i + 1 & "','" & txnRefNo & "','','" & P_AccountNo & "','" & P_CustomerNo & "','" & txtTransactionCode.Text.ToUpper.Trim & "')"
                    Me.InsertTransactions(dailyTransactionInsertSQl)
                    'all history
                    dailyTransactionInsertSQl = "INSERT INTO [dbo].[MB_All_DailyTransactions_History]([SeqNo]
      ,[AccountNo]
      ,[AccountName]
      ,[TxnType]
      ,[TxnBranch]
      ,[AccountBranch]
      ,[AcCCY]
      ,[Amount]
      ,[FX_Rate]
      ,[LCY_Amount]
      ,[Description]
      ,[InvoiceNo]
      ,[Module]
      ,[Reference]
      ,[UserName]
      ,[TxnDate]
      ,[ActDate]
      ,[SrNo]
      ,[TxnRefNo]
      ,[ExtRefNo]
      ,[Contra_AccNo]
      ,[CustomerNo]
      ,[TxnCode]) VALUES"
                    dailyTransactionInsertSQl = dailyTransactionInsertSQl & "('" & SeqNo.Trim() & "', " &
                                                " '" & drCurrentRow("Account No") & "','" & drCurrentRow("ACCOUNT NAME") & "', " &
                    "'" & drCurrentRow("TXN TYPE") & "','" & branchID.Trim() & "', " &
                    "'" & branchID.Trim() & "','" & "GMD" & "', " &
                    "'" & depositAmount & "','" & fxRate & "', " &
                    "'" & depositAmount & "','" & drCurrentRow("Desc") & "', " &
                    "'','" & P_DefinationCode & "','" & TxnReference & "','" & UserId & "', " &
                    "'" & TxnDate & "','" & actionDate & "','" & i + 1 & "','" & txnRefNo & "','','" & P_AccountNo & "','" & P_CustomerNo & "','" & txtTransactionCode.Text.ToUpper.Trim & "')"
                    Me.InsertTransactions(dailyTransactionInsertSQl)
                    'audit
                    dailyTransactionInsertSQl = "INSERT INTO [dbo].[MB_TransactionsAuditTrail]([SeqNo]
      ,[AccountNo]
      ,[AccountName]
      ,[TxnType]
      ,[TxnBranch]
      ,[AccountBranch]
      ,[AcCCY]
      ,[Amount]
      ,[FX_Rate]
      ,[LCY_Amount]
      ,[Description]
      ,[InvoiceNo]
      ,[Module]
      ,[Reference]
      ,[UserName]
      ,[TxnDate]
      ,[ActDate]
      ,[SrNo]
      ,[TxnRefNo]
      ,[ExtRefNo]
      ,[Contra_AccNo]
      ,[CustomerNo]
      ,[TxnCode]) VALUES"
                    dailyTransactionInsertSQl = dailyTransactionInsertSQl & "('" & SeqNo.Trim() & "', " &
                                                " '" & drCurrentRow("Account No") & "','" & drCurrentRow("ACCOUNT NAME") & "', " &
                    "'" & drCurrentRow("TXN TYPE") & "','" & branchID.Trim() & "', " &
                    "'" & branchID.Trim() & "','" & "GMD" & "', " &
                    "'" & depositAmount & "','" & fxRate & "', " &
                    "'" & depositAmount & "','" & drCurrentRow("Desc") & "', " &
                    "'','" & P_DefinationCode & "','" & TxnReference & "','" & UserId & "', " &
                    "'" & TxnDate & "','" & actionDate & "','" & i + 1 & "','" & txnRefNo & "','','" & P_AccountNo & "','" & P_CustomerNo & "','" & txtTransactionCode.Text.ToUpper.Trim & "')"
                    Me.InsertTransactions(dailyTransactionInsertSQl)
                    'audit history
                    dailyTransactionInsertSQl = "INSERT INTO [dbo].[MB_TransactionsAuditTrail_History]([SeqNo]
      ,[AccountNo]
      ,[AccountName]
      ,[TxnType]
      ,[TxnBranch]
      ,[AccountBranch]
      ,[AcCCY]
      ,[Amount]
      ,[FX_Rate]
      ,[LCY_Amount]
      ,[Description]
      ,[InvoiceNo]
      ,[Module]
      ,[Reference]
      ,[UserName]
      ,[TxnDate]
      ,[ActDate]
      ,[SrNo]
      ,[TxnRefNo]
      ,[ExtRefNo]
      ,[Contra_AccNo]
      ,[CustomerNo]
      ,[TxnCode]) VALUES"
                    dailyTransactionInsertSQl = dailyTransactionInsertSQl & "('" & SeqNo.Trim() & "', " &
                                                " '" & drCurrentRow("Account No") & "','" & drCurrentRow("ACCOUNT NAME") & "', " &
                    "'" & drCurrentRow("TXN TYPE") & "','" & branchID.Trim() & "', " &
                    "'" & branchID.Trim() & "','" & "GMD" & "', " &
                    "'" & depositAmount & "','" & fxRate & "', " &
                    "'" & depositAmount & "','" & drCurrentRow("Desc") & "', " &
                    "'','" & P_DefinationCode & "','" & TxnReference & "','" & UserId & "', " &
                    "'" & TxnDate & "','" & actionDate & "','" & i + 1 & "','" & txnRefNo & "','','" & P_AccountNo & "','" & P_CustomerNo & "','" & txtTransactionCode.Text.ToUpper.Trim & "')"
                    Me.InsertTransactions(dailyTransactionInsertSQl)

                ElseIf i = 2 Then
                    TxnReference = _module & (TxnCode).ToUpper() & SeqNo & i + 1 & _day & _month & _year
                    '7. Insert row three as a credit entry into The GL Transaction Table
                    dailyTransactionInsertSQl = "INSERT INTO [dbo].[MB_GL_DailyTransaction]([SeqNo]
      ,[GLAccNo]
      ,[GLName]
      ,[TxnType]
      ,[TxnBranch]
      ,[AccountBranch]
      ,[AcCCY]
      ,[Amount]
      ,[FX_Rate]
      ,[LCY_Amount]
      ,[Description]
      ,[InvoiceNo]
      ,[Module]
      ,[Reference]
      ,[UserName]
      ,[TxnDate]
      ,[ActDate]
      ,[SrNo]
      ,[TxnRefNo]
      ,[ExtRefNo]
      ,[AccountNo]
      ,[CustomerNo]
      ,[TxnCode]) VALUES"
                    dailyTransactionInsertSQl = dailyTransactionInsertSQl & "('" & SeqNo.Trim() & "', " &
                                               "'" & drCurrentRow("Account No") & "','" & drCurrentRow("ACCOUNT NAME") & "', " &
                   "'" & drCurrentRow("TXN TYPE") & "','" & branchID.Trim() & "', " &
                    "'" & branchID.Trim() & "','" & "GMD" & "', " &
                    "'" & depositAmount & "','" & fxRate & "', " &
                    "'" & depositAmount & "','" & drCurrentRow("Desc") & "', " &
                    "'','" & P_DefinationCode & "','" & TxnReference & "','" & UserId & "', " &
                    "'" & TxnDate & "','" & actionDate & "','" & i + 1 & "','" & txnRefNo & "','','" & P_AccountNo & "','" & P_CustomerNo & "','" & txtTransactionCode.Text.ToUpper.Trim & "')"
                    Me.InsertTransactions(dailyTransactionInsertSQl)
                    ' '7. Insert row three as a credit entry into The GL Transaction Table History
                    dailyTransactionInsertSQl = "INSERT INTO [dbo].[MB_GL_DailyTransaction_History]([SeqNo]
      ,[GLAccNo]
      ,[GLName]
      ,[TxnType]
      ,[TxnBranch]
      ,[AccountBranch]
      ,[AcCCY]
      ,[Amount]
      ,[FX_Rate]
      ,[LCY_Amount]
      ,[Description]
      ,[InvoiceNo]
      ,[Module]
      ,[Reference]
      ,[UserName]
      ,[TxnDate]
      ,[ActDate]
      ,[SrNo]
      ,[TxnRefNo]
      ,[ExtRefNo]
      ,[AccountNo]
      ,[CustomerNo]
      ,[TxnCode]) VALUES"
                    dailyTransactionInsertSQl = dailyTransactionInsertSQl & "('" & SeqNo.Trim() & "', " &
                                               "'" & drCurrentRow("Account No") & "','" & drCurrentRow("ACCOUNT NAME") & "', " &
                   "'" & drCurrentRow("TXN TYPE") & "','" & branchID.Trim() & "', " &
                    "'" & branchID.Trim() & "','" & "GMD" & "', " &
                    "'" & depositAmount & "','" & fxRate & "', " &
                    "'" & depositAmount & "','" & drCurrentRow("Desc") & "', " &
                    "'','" & P_DefinationCode & "','" & TxnReference & "','" & UserId & "', " &
                    "'" & TxnDate & "','" & actionDate & "','" & i + 1 & "','" & txnRefNo & "','','" & P_AccountNo & "','" & P_CustomerNo & "','" & txtTransactionCode.Text.ToUpper.Trim & "')"
                    Me.InsertTransactions(dailyTransactionInsertSQl)
                    'audit
                    dailyTransactionInsertSQl = "INSERT INTO [dbo].[MB_TransactionsAuditTrail]([SeqNo]
      ,[AccountNo]
      ,[AccountName]
      ,[TxnType]
      ,[TxnBranch]
      ,[AccountBranch]
      ,[AcCCY]
      ,[Amount]
      ,[FX_Rate]
      ,[LCY_Amount]
      ,[Description]
      ,[InvoiceNo]
      ,[Module]
      ,[Reference]
      ,[UserName]
      ,[TxnDate]
      ,[ActDate]
      ,[SrNo]
      ,[TxnRefNo]
      ,[ExtRefNo]
      ,[Contra_AccNo]
      ,[CustomerNo]
      ,[TxnCode]) VALUES"
                    dailyTransactionInsertSQl = dailyTransactionInsertSQl & "('" & SeqNo.Trim() & "', " &
                                               "'" & drCurrentRow("Account No") & "','" & drCurrentRow("ACCOUNT NAME") & "', " &
                   "'" & drCurrentRow("TXN TYPE") & "','" & branchID.Trim() & "', " &
                    "'" & branchID.Trim() & "','" & "GMD" & "', " &
                    "'" & depositAmount & "','" & fxRate & "', " &
                    "'" & depositAmount & "','" & drCurrentRow("Desc") & "', " &
                    "'','" & P_DefinationCode & "','" & TxnReference & "','" & UserId & "', " &
                    "'" & TxnDate & "','" & actionDate & "','" & i + 1 & "','" & txnRefNo & "','','" & P_AccountNo & "','" & P_CustomerNo & "','" & txtTransactionCode.Text.ToUpper.Trim & "')"
                    Me.InsertTransactions(dailyTransactionInsertSQl)
                    'audit history
                    dailyTransactionInsertSQl = "INSERT INTO [dbo].[MB_TransactionsAuditTrail_History]([SeqNo]
      ,[AccountNo]
      ,[AccountName]
      ,[TxnType]
      ,[TxnBranch]
      ,[AccountBranch]
      ,[AcCCY]
      ,[Amount]
      ,[FX_Rate]
      ,[LCY_Amount]
      ,[Description]
      ,[InvoiceNo]
      ,[Module]
      ,[Reference]
      ,[UserName]
      ,[TxnDate]
      ,[ActDate]
      ,[SrNo]
      ,[TxnRefNo]
      ,[ExtRefNo]
      ,[Contra_AccNo]
      ,[CustomerNo]
      ,[TxnCode]) VALUES"
                    dailyTransactionInsertSQl = dailyTransactionInsertSQl & "('" & SeqNo.Trim() & "', " &
                                               "'" & drCurrentRow("Account No") & "','" & drCurrentRow("ACCOUNT NAME") & "', " &
                   "'" & drCurrentRow("TXN TYPE") & "','" & branchID.Trim() & "', " &
                    "'" & branchID.Trim() & "','" & "GMD" & "', " &
                    "'" & depositAmount & "','" & fxRate & "', " &
                    "'" & depositAmount & "','" & drCurrentRow("Desc") & "', " &
                    "'','" & P_DefinationCode & "','" & TxnReference & "','" & UserId & "', " &
                    "'" & TxnDate & "','" & actionDate & "','" & i + 1 & "','" & txnRefNo & "','','" & P_AccountNo & "','" & P_CustomerNo & "','" & txtTransactionCode.Text.ToUpper.Trim & "')"
                    Me.InsertTransactions(dailyTransactionInsertSQl)
                    'add all transactions here
                    ' dailyTransactionInsertSQl = "INSERT INTO [dbo].[MB_All_DailyTransactions] VALUES"
                    ' dailyTransactionInsertSQl = dailyTransactionInsertSQl & "('" & SeqNo.Trim() & "', " &
                    '                            "'" & drCurrentRow("Account No") & "','" & drCurrentRow("ACCOUNT NAME") & "', " &
                    '"'" & drCurrentRow("TXN TYPE") & "','" & branchID.Trim() & "', " &
                    ' "'" & branchID.Trim() & "','" & "GMD" & "', " &
                    ' "'" & depositAmount & "','" & fxRate & "', " &
                    ' "'" & depositAmount & "','" & drCurrentRow("Desc") & "', " &
                    ' "'','" & _module & "','" & TxnReference & "','" & UserId & "', " &
                    ' "'" & TxnDate & "','" & actionDate & "','" & i & "','" & refNo & "','')"
                    ' Me.InsertTransactions(dailyTransactionInsertSQl)
                End If
            Next
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
            MyReader4.Close()
        Catch ex As Exception
            'txtOrgName.Text = ""
            'txtAdress1.Text = ""
            '' DropDownList1.Text = ""
            'txtOrgCode.ReadOnly = False
            'Label4.Text = "Error Pulling Account Details"
        End Try
    End Sub
    Private Sub InsertTransactions(ByVal SqlStr As String)
        'InsertDailyTransactions()
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
    Protected Sub btnCheckAccNo_Click(sender As Object, e As EventArgs) Handles btnCheckAccNo.Click
        If lblDescription.Text = "" Then
            lblSuccessMsg.Text = "Please Check Transaction Code"
            Exit Sub
        Else
            GetAccountDetails()
        End If
    End Sub
    Public Sub GetAccountDetails()
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
            Dim mySearch4 As String = "select AccountNo,CustomerNo,AccountName,AssetGL_Control,LiabilityGL_Control,BranchCode,AvailBal,BookBalance from [dbo].[tbAccount] where AccountNo = '" & txtAccountNo.Text.Trim & "' and ProductDefCode = 'SA' and Approved = 1" ' ='" & Me.HiddenField1.Value & "' "
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
            Else
                lblSuccessMsg.Text = "Account Number does not exist or not a savings a/c or a/c is not approved"
            End If
            MyReader4.Close()
        Catch ex As Exception
            'txtOrgName.Text = ""
            'txtAdress1.Text = ""
            '' DropDownList1.Text = ""
            'txtOrgCode.ReadOnly = False
            'Label4.Text = "Error Pulling Account Details"
        End Try
    End Sub
    Protected Sub btncheckTranCode_Click(sender As Object, e As EventArgs) Handles btncheckTranCode.Click
        GetTransactionCode()
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
                If P_TxnType = "C" Then
                    lblSuccessMsg.Text = "Invalid transaction code"
                    lblDescription.Text = ""
                    Exit Function
                End If
                If P_TxnDesc = "CASH WITHDRAWAL" Then
                Else
                    lblSuccessMsg.Text = "Invalid transaction code"
                    lblDescription.Text = ""
                    Exit Function
                End If
            Else
                lblSuccessMsg.Text = "Transaction Code does not exist"
                Exit Function
            End If
            MyReader4.Close()
        Catch ex As Exception
            'Label4.Text = "Error Pulling Account Details"
        End Try
    End Function
    Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        GetAccountDetails()
        GetTransactionCode()
        UpdateCustomerBalances()
        '18062022 commented to allow the use of trigger
        UpdateCustomerGL()
        UpdateCashierGL()
        Me.saveGridTxnRecords()
        'InsertDailyTransactions()
        ''InsertDailyHistory()
        CashTxnGridView.DataSource = Nothing
        CashTxnGridView.DataBind()
        lblDescription.Text = ""
        lblAccountName.Text = ""
        txtAccountNo.Text = ""
        txtAmount.Text = ""
        txtTransactionCode.Text = ""
    End Sub
    Protected Function UpdateCustomerBalances()
        Try
            Dim newAccBal As String = ""
            newAccBal = P_BookBalance - Val(txtAmount.Text.Trim)
                Dim MySqlaction1 As String
                Dim mySqlAction As String = ""
                Dim MyCurrencyName As String = ""
                Dim MyDBAction As New DataManagement
            MySqlaction1 = "update tbAccount set AvailBal ='" & newAccBal & "',BookBalance ='" & newAccBal & "', LastActivityDate = '" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "',LastDRactvtyDate ='" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "',LastDRDate = '" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "' where AccountNo ='" & txtAccountNo.Text.Trim & "'"
            MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
            txtNewBalance.Text = newAccBal
        Catch ex As Exception
            'Label4.Text = "Error Inserting"
        End Try
    End Function
    Protected Sub UpdateCustomerGL()
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select GLACCTNO,GLNAME,ACT_BAL,LCY_BAL from GL_DETAIL where GLACCTNO = '" & P_LiabilityGL_Control & "'"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                Dim GLBal As String = ""
                Dim newGLBal As String = ""
                GLBal = MyReader4("ACT_BAL").ToString
                P_LiabilityGL_Control = MyReader4("GLACCTNO").ToString
                newGLBal = Val(GLBal) - Val(txtAmount.Text)
                Dim MySqlaction1 As String
                Dim mySqlAction As String = ""
                Dim MyCurrencyName As String = ""
                Dim MyDBAction As New DataManagement
                MySqlaction1 = "update GL_DETAIL set ACT_BAL ='" & newGLBal & "',LCY_BAL ='" & newGLBal & "' where GLACCTNO = '" & P_LiabilityGL_Control & "'"
                MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
            End If
            MyReader4.Close()
        Catch ex As Exception
            'Label4.Text = "Error Pulling Account Details, Commission A/c must start 01M8 .... E6 "
        End Try
    End Sub
    Protected Sub UpdateCashierGL()
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select TxnCode,ContraGL from tbTransactionCode where TxnCode = '" & txtTransactionCode.Text.Trim & "' "
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                Dim cashierGL As String = ""
                cashierGL = MyReader4("ContraGL").ToString
                Try
                    Dim mySearch5 As String = "select GLACCTNO,GLNAME,ACT_BAL,LCY_BAL from GL_DETAIL where GLACCTNO = '" & cashierGL & "' " ' ='" & Me.HiddenField1.Value & "' "
                    Dim MyReader5 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch5, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
                    If MyReader5.HasRows = True Then
                        MyReader5.Read()
                        Dim GLBal As String = ""
                        Dim newGLBal As String = ""
                        GLBal = MyReader5("ACT_BAL").ToString
                        'cashierGLName = MyReader5("GLName").ToString
                        newGLBal = GLBal + Val(txtAmount.Text)
                        Dim MySqlaction1 As String
                        Dim mySqlAction As String = ""
                        Dim MyCurrencyName As String = ""
                        Dim MyDBAction As New DataManagement
                        MySqlaction1 = "update GL_DETAIL set ACT_BAL ='" & newGLBal & "',LCY_BAL ='" & newGLBal & "' where GLACCTNO = '" & cashierGL & "'"
                        MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
                    End If
                    MyReader5.Close()
                Catch ex As Exception
                End Try
            End If
            MyReader4.Close()
        Catch ex As Exception
            'Label4.Text = "Error Pulling Account Details, Commission A/c must start 01M8 .... E6 "
        End Try
    End Sub
    Protected Sub InsertDailyTransactions()
        Try
            Dim mySqlAction As String = ""
            Dim MyDBAction As New DataManagement
            mySqlAction = "Insert into MB_DailyTransactions(AccountNo,CustomerNo,AccountName,AssetGL_Control,LiabilityGL_Control,BranchCode,Description,TxnType,TxnCode,PostMode,ContraGL,TelleID,Reference,Amount,TrxDate,Module)values('" & txtAccountNo.Text.ToUpper.Trim & "','" & P_CustomerNo.ToUpper.Trim & "','" & P_AccountName.ToUpper.Trim & "','" & P_AssetGL_Control.ToUpper.Trim & "','" & P_LiabilityGL_Control.ToUpper.Trim & "','" & P_BranchCode.ToUpper.Trim & "','" & lblDescription.Text.ToUpper.Trim & "','" & P_TxnType.ToUpper.Trim & "','" & P_TxnCode.ToUpper.Trim & "','" & P_PostMode.ToUpper.Trim & "','" & P_ContraGL.ToUpper.Trim & "','" & P_TelleID.ToUpper.Trim & "','123456789','" & txtAmount.Text.ToUpper.Trim & "','" & DateAndTime.Today() & "','" & P_RefCode.ToUpper.Trim & "') "
            MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
            lblSuccessMsg.Text = "Transaction posted and saved Successfully"
        Catch ex As Exception
            'Label4.Text = "Error Inserting"
        End Try
    End Sub
    Protected Sub InsertDailyHistory()
        Try
            Dim mySqlAction As String = ""
            Dim MyDBAction As New DataManagement
            mySqlAction = "Insert into MB_DailyTransactionsHistory(AccountNo,CustomerNo,AccountName,AssetGL_Control,LiabilityGL_Control,BranchCode,Description,TxnType,TxnCode,PostMode,ContraGL,TelleID,Reference,Amount,TrxDate,Module)values('" & txtAccountNo.Text.ToUpper.Trim & "','" & P_CustomerNo.ToUpper.Trim & "','" & P_AccountName.ToUpper.Trim & "','" & P_AssetGL_Control.ToUpper.Trim & "','" & P_LiabilityGL_Control.ToUpper.Trim & "','" & P_BranchCode.ToUpper.Trim & "','" & lblDescription.Text.ToUpper.Trim & "','" & P_TxnType.ToUpper.Trim & "','" & P_TxnCode.ToUpper.Trim & "','" & P_PostMode.ToUpper.Trim & "','" & P_ContraGL.ToUpper.Trim & "','" & P_TelleID.ToUpper.Trim & "','123456789','" & txtAmount.Text.ToUpper.Trim & "','" & DateAndTime.Today() & "','" & P_RefCode.ToUpper.Trim & "') "
            MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
            lblSuccessMsg.Text = "Transaction posted and saved Successfully"
            'txtAmount.Text = ""
        Catch ex As Exception
            'Label4.Text = "Error Inserting"
        End Try
    End Sub
    Protected Sub GenerateRefNumber()
        'GetAccountDetails()
        'GetTransactionCode()
        'Try
        '    Dim mySqlAction As String = ""
        '    Dim MyDBAction As New DataManagement
        '    mySqlAction = "Insert into MB_DailyTransactionsHistory(AccountNo,CustomerNo,AccountName,AssetGL_Control,LiabilityGL_Control,BranchCode,Description,TxnType,TxnCode,PostMode,ContraGL,TelleID,Reference,Amount,TrxDate,Module)values('" & txtAccountNo.Text.ToUpper.Trim & "','" & P_CustomerNo.ToUpper.Trim & "','" & P_AccountName.ToUpper.Trim & "','" & P_AssetGL_Control.ToUpper.Trim & "','" & P_LiabilityGL_Control.ToUpper.Trim & "','" & P_BranchCode.ToUpper.Trim & "','" & lblDescription.Text.ToUpper.Trim & "','" & P_TxnType.ToUpper.Trim & "','" & P_TxnCode.ToUpper.Trim & "','" & P_PostMode.ToUpper.Trim & "','" & P_ContraGL.ToUpper.Trim & "','" & P_TelleID.ToUpper.Trim & "','123456789','" & txtAmount.Text.ToUpper.Trim & "','" & DateAndTime.Today() & "','" & P_RefCode.ToUpper.Trim & "') "
        '    MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
        '    lblSuccessMsg.Text = "Record Saved Successfully"

        '    'txtAmount.Text = ""
        'Catch ex As Exception
        '    'txtOrgName.Text = ""
        '    'txtAdress1.Text = ""
        '    '' DropDownList1.Text = ""
        '    'txtOrgCode.ReadOnly = False
        '    'Label4.Text = "Error Inserting"
        'End Try
    End Sub

    Private Sub CashTxnGridView_RowDataBound(sender As Object, e As GridViewRowEventArgs) Handles CashTxnGridView.RowDataBound

        'Dim _AccountNo_TextBox As TextBox = DirectCast(e.Row.FindControl("AccountNo_TextBox"), TextBox)
        'Dim _acctName_TextBox As TextBox = DirectCast(e.Row.FindControl("acctName_TextBox"), TextBox)
        'Dim TXN_TYPE_TextBox As TextBox = DirectCast(e.Row.FindControl("TXN_TYPE_TextBox"), TextBox)
        'Dim _Amount_TextBox As TextBox = DirectCast(e.Row.FindControl("Amount_TextBox"), TextBox)
        'Dim _DescTextBox As TextBox = DirectCast(e.Row.FindControl("DescTextBox"), TextBox)


        'Dim AccountNoLabel As Label = DirectCast(e.Row.FindControl("AccountNoLabel"), Label)
        'Dim accountNameLabel As Label = DirectCast(e.Row.FindControl("accountNameLabel"), Label)
        'Dim TxnTypeLabel As Label = DirectCast(e.Row.FindControl("TxnTypeLabel"), Label)
        'Dim AmountLabel As Label = DirectCast(e.Row.FindControl("AmountLabel"), Label)
        'Dim DescLabel As Label = DirectCast(e.Row.FindControl("DescLabel"), Label)
        'If AccountNoLabel IsNot Nothing Then
        '    _AccountNo_TextBox.Text = AccountNoLabel.Text
        '    _acctName_TextBox.Text = accountNameLabel.Text
        '    TXN_TYPE_TextBox.Text = TxnTypeLabel.Text
        '    _Amount_TextBox.Text = AmountLabel.Text
        '    _DescTextBox.Text = DescLabel.Text

        '    AmountLabel.Visible = False
        '    AccountNoLabel.Visible = False
        '    accountNameLabel.Visible = False
        '    DescLabel.Visible = False
        '    TxnTypeLabel.Visible = False


        'End If
    End Sub
End Class