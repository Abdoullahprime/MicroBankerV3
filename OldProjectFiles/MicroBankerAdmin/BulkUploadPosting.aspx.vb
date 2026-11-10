Imports System.Configuration
Imports System.Data
Imports System.Data.OleDb
Imports System.Data.SqlClient
Imports System.IO
Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Public Class BulkUploadPosting
    Inherits System.Web.UI.Page
    Public P_AccountNo As String = "", P_CustomerNo As String = "", P_AccountName As String = "", P_AssetGL_Control As String = "", P_LiabilityGL_Control As String = "", P_BranchCode As String = "", P_AvailBal As Decimal = 0.00, P_BookBalance As Decimal = 0.00
    Public P_TxnCode As String = "", P_TxnDesc As String = "", P_TxnType As String = "", P_ChargeAmt As String = "", P_ChqNo As String = "", P_TxnCash As String = "", P_PostMode As String = "", P_ContraGL As String = "", P_TelleID As String = "", P_AttachedTxnCode As String = "", P_RefCode As String = ""
    Public P_ProdRefCode As String = "", P_refNo As String = ""
    Const CURRENCY_DISPLAY_FORMAT As String = "##,##0.00"
    'Private P_AccountDetailsHashMap As New Hashtable
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    'Protected Sub Upload(sender As Object, e As EventArgs)

    '    If txtTransactionCode.Text = "" Then
    '        lblMessage.Text = "Please enter Transaction Code"
    '        txtTransactionCode.Focus()
    '        Exit Sub
    '    Else
    '        lblMessage.Text = ""
    '    End If
    '    If txtNarration.Text = "" Then
    '        lblMessage.Text = "Please enter narration"
    '        txtNarration.Focus()
    '        Exit Sub
    '    Else
    '        lblMessage.Text = ""
    '    End If

    '    If FileUpload1.PostedFile.FileName = "" Then
    '        lblMessage.Text = "Please upload file"
    '        Exit Sub
    '    Else
    '        lblMessage.Text = ""
    '    End If
    '    Dim csvPath As String = Server.MapPath("~/Files/") + Path.GetFileName(FileUpload1.PostedFile.FileName)
    '    FileUpload1.SaveAs(csvPath)
    '    Dim dt As New DataTable()
    '    dt.Columns.AddRange(New DataColumn(3) {
    '                        New DataColumn("Account Number", GetType(String)),
    '                        New DataColumn("Account Name", GetType(String)),
    '                        New DataColumn("Amount", GetType(Decimal)),
    '                        New DataColumn("Narration", GetType(String))})

    '    Dim csvData As String = File.ReadAllText(csvPath)
    '    Dim firstRow As Boolean = True
    '    For Each row As String In csvData.Split(ControlChars.Lf)
    '        If firstRow Then
    '            firstRow = False
    '        Else
    '            If Not String.IsNullOrEmpty(row) Then
    '                'dt.Rows.Add()
    '                'Dim i As Integer = 0
    '                'For Each cell As String In row.Split(","c)
    '                '    dt.Rows(dt.Rows.Count - 1)(i) = cell.Trim().Replace("""", "").Replace("�", "")
    '                '    i += 1
    '                dt.Rows.Add()

    '                Dim values() As String = row.Split(","c)

    '                For i As Integer = 0 To Math.Min(values.Length - 1, dt.Columns.Count - 1)
    '                    dt.Rows(dt.Rows.Count - 1)(i) = values(i).Trim().Replace("""", "").Replace("�", "")
    '                Next
    '            End If
    '        End If
    '    Next

    '    Dim dtFinal As DataTable = dt.Clone()
    '    dtFinal.Columns.Remove("Narration")

    '    For Each row As DataRow In dt.Rows
    '        Dim dr As DataRow = dtFinal.NewRow()
    '        dr("Account Number") = row("Account Number")
    '        'UtilitiesModule.GetAccountDetailMap(row("Account Number"))
    '        ''erroe = P_AccountDetailsHashMap("error")
    '        'AccAvailBal = P_AccountDetailsHashMap("AvailBal")
    '        'AccBookBal = P_AccountDetailsHashMap("BookBalance")
    '        dr("Account Name") = row("Account Name")
    '        dr("Amount") = row("Amount")
    '        'dr("Book Balance") = AccBookBal
    '        'dr("Narration") = row("Narration") '& ", " & row("Narration")
    '        dtFinal.Rows.Add(dr)
    '    Next

    '    GridView1.DataSource = dtFinal
    '    GridView1.DataBind()

    '    Dim consString As String = ConfigurationManager.ConnectionStrings("ConnectionString").ConnectionString
    '    Using con As New SqlConnection(consString)
    '        Using sqlBulkCopy As New SqlBulkCopy(con)
    '            'Set the database table name
    '            sqlBulkCopy.DestinationTableName = "dbo.MB_BulkUploadRecordsss"
    '            con.Open()
    '            sqlBulkCopy.WriteToServer(dt)
    '            con.Close()
    '        End Using
    '    End Using
    '    InsertRetailPostingTemp()

    'End Sub

    Protected Sub Upload(sender As Object, e As EventArgs)

        Try

            If txtTransactionCode.Text.Trim = "" Then
                lblMessage.Text = "Please enter Transaction Code"
                txtTransactionCode.Focus()
                Exit Sub
            End If

            If txtNarration.Text.Trim = "" Then
                lblMessage.Text = "Please enter narration"
                txtNarration.Focus()
                Exit Sub
            End If

            If Not FileUpload1.HasFile Then
                lblMessage.Text = "Please upload Excel file"
                Exit Sub
            End If

            lblMessage.Text = ""

            Dim filePath As String = Server.MapPath("~/Files/") & Path.GetFileName(FileUpload1.FileName)
            FileUpload1.SaveAs(filePath)

            Dim dt As New DataTable()
            dt.Columns.Add("AccountNo", GetType(String))
            dt.Columns.Add("AccountName", GetType(String))
            dt.Columns.Add("Amount", GetType(Decimal))
            dt.Columns.Add("Narration", GetType(String))

            Dim connStr As String =
            "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" & filePath & ";Extended Properties='Excel 12.0 Xml;HDR=YES;IMEX=1'"

            Using conExcel As New OleDbConnection(connStr)
                conExcel.Open()

                Dim schema As DataTable = conExcel.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, Nothing)
                Dim sheetName As String = schema.Rows(0)("TABLE_NAME").ToString()

                Dim cmd As New OleDbCommand("SELECT * FROM [" & sheetName & "]", conExcel)

                Using dr As OleDbDataReader = cmd.ExecuteReader()

                    While dr.Read()

                        If IsDBNull(dr(0)) OrElse dr(0).ToString.Trim = "" Then Continue While

                        Dim amt As Decimal = 0
                        Decimal.TryParse(dr(2).ToString().Replace(",", ""), amt)

                        Dim row As DataRow = dt.NewRow()
                        row("AccountNo") = dr(0).ToString.Trim()
                        row("AccountName") = dr(1).ToString.Trim()
                        row("Amount") = amt
                        row("Narration") = txtNarration.Text.Trim()

                        dt.Rows.Add(row)

                    End While

                End Using
            End Using

            If dt.Rows.Count = 0 Then
                lblMessage.Text = "Excel contains no valid data"
                Exit Sub
            End If

            GridView1.DataSource = dt
            GridView1.DataBind()

            Using con As New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ConnectionString)
                Using bulk As New SqlBulkCopy(con)

                    bulk.DestinationTableName = "dbo.MB_BulkUploadRecordsss"

                    bulk.ColumnMappings.Add("AccountNo", "AccountNo")
                    bulk.ColumnMappings.Add("AccountName", "AccountName")
                    bulk.ColumnMappings.Add("Amount", "Amount")
                    bulk.ColumnMappings.Add("Narration", "Narration")

                    con.Open()
                    bulk.WriteToServer(dt)
                    con.Close()

                End Using
            End Using

            InsertRetailPostingTemp()

            lblMessage.Text = "Upload successful. " & dt.Rows.Count & " records imported."

        Catch ex As Exception
            lblMessage.Text = "Upload failed: " & ex.Message
        End Try

    End Sub
    Function UpdateRetailPostingTemp() As Boolean

        Dim value As Boolean

        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch2 As String = "select count(SOD_Status) as Count from [dbo].[MB_RetailPosting_Temp] where TxnCode = 'C77'"
            Dim MyReader2 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch2, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader2.HasRows = True Then
                MyReader2.Read()
                Dim count As Integer = 0
                Dim newCount As Integer = 0
                newCount = MyReader2("Count").ToString
                While count < newCount
                    Dim mySearch4 As String = "select AccountNo,AccountName,BranchCode,CustomerNo,SettlementsAccountNo,RepaymentAmount,AssetGL_Control,LiabilityGL_Control,ProductDefCode,AvailBal,BookBalance from tbAccount where SOD_Status = 0 and ProductDefCode = 'LA'"
                    Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
                    If MyReader4.HasRows = True Then
                        MyReader4.Read()
                        count = count + 1
                        'value = True
                    End If
                    MyReader4.Close()
                End While
                value = True
            End If
            MyReader2.Close()
        Catch ex As Exception
            'lblSuccessMsg.Text = ex.ToString
            value = False
        End Try
        Return value
    End Function
    Private Sub InsertRetailPostingTemp()
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "INSERT INTO MB_RetailPosting_Temp (AccountNo,AccountName,Amount,Details,TxnCode,LiabilityGL_Control,SeqNo,TxnType,TxnBranch,AccBanch,Acc_CCY,FX_Rate,LCY_Amount,Reference,PayBy,DR_GL,CR_GL,PostedBy,TxnDate,ActDate,Journal,TxnRefNo)
select AccountNo,AccountName,Amount,Narration,TrxCode,LiabilityGL_Control,SeqNo,TxnType,TxnBranch,AccBanch,Acc_CCY,FX_Rate,LCY_Amount,Reference,PayBy,DR_GL,CR_GL,PostedBy,TxnDate,ActDate,Journal,TxnRefNo from [dbo].[MB_BulkUploadRecordsss]"
        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    End Sub

    Protected Sub Post(sender As Object, e As EventArgs)
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch2 As String = "select count(*) as Count from dbo.MB_BulkUploadRecordsss"
            Dim MyReader2 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch2, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader2.HasRows = True Then
                MyReader2.Read()
                Dim newCount As Integer = 0
                newCount = MyReader2("Count").ToString
                If newCount > 0 Then
                    PostTransactions()
                    lblMessage.Text = "Bulk transactions posted and saved successfully"

                    GridView1.DataSource = Nothing
                    GridView1.DataBind()
                    lblDescription.Text = ""
                    txtTransactionCode.Text = ""
                    txtNarration.Text = ""

                Else
                    lblMessage.Text = "No records uploaded"

                End If
            End If
            MyReader2.Close()
        Catch ex As Exception

        End Try

        'lblTotalDebit.Text = ""
    End Sub

    Protected Sub PostTransactions()
        Try
            Dim MyDatObject As New DataManagement

            ' First, mark all pending records for processing (optional but recommended)
            ' Or use a transaction-based approach

            ' Get all records that need processing ONCE
            Dim mySearch4 As String = "select Journal, AccountNo, TxnCode, Amount, TxnType, LiabilityGL_Control, Reference from MB_RetailPosting_Temp where Journal = 1 "
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)

            While MyReader4.Read() ' Use Read() directly instead of nested loops
                Try
                    Dim TransCode As String = ""
                    Dim TxnDate As DateTime = Date.Now()
                    Dim liabilityGL As String = ""
                    Dim transType As String = ""
                    Dim ReferenceNo As String = ""
                    Dim txnRefNo As String = "", _module As String = "", SeqNo As String = "", _day As String = "", _month As String = "", _year As String = ""
                    Dim amounts As Decimal = 0
                    Dim accountBal As Decimal = 0.00
                    Dim currentAccountNo As String = ""
                    Dim currentJournal As String = ""

                    ' Store current record values
                    currentJournal = MyReader4("Journal").ToString
                    currentAccountNo = MyReader4("AccountNo").ToString
                    TransCode = MyReader4("TxnCode").ToString
                    amounts = CDbl(MyReader4("Amount").ToString)
                    transType = MyReader4("TxnType").ToString
                    liabilityGL = MyReader4("LiabilityGL_Control").ToString
                    Dim currentReference As String = MyReader4("Reference").ToString

                    ' Get sequence number and other details
                    UtilitiesModule.GetNextSeqNo("LAST_SEQNO")
                    SeqNo = UtilitiesModule.returnValue
                    _module = "77" ' Hardcoded - consider making this configurable
                    Dim TxnCode As String = "88" ' Hardcoded - consider making this configurable

                    ' Build reference number
                    _day = (TxnDate.Day).ToString().PadLeft(2, "0")
                    _month = (TxnDate.Month).ToString().PadLeft(2, "0")
                    _year = (TxnDate.Year).ToString().Substring(2, 2)
                    ReferenceNo = _module & TxnCode.ToUpper() & SeqNo & _day & _month & _year
                    txnRefNo = _module & TxnCode.ToUpper() & _day & _month & _year

                    ' Get account details
                    Dim accountDetails As Hashtable = GetAccountDetails(currentAccountNo)
                    If accountDetails IsNot Nothing AndAlso accountDetails.Count > 0 Then
                        accountBal = CDec(accountDetails("AvailBal"))
                        liabilityGL = accountDetails("LiabilityGLControl").ToString()
                        Dim custNo As String = accountDetails("CustomerNo").ToString()
                        _module = accountDetails("ProductDefCode").ToString()

                        ' Clear the hashtable from the class-level variable if you're using Solution 2/3
                        ' P_AccountDetailsHashMap.Clear() ' Only if using global hashtable

                        Dim trnRetailCode As String = "C77"

                        ' Process the transaction
                        UpdateTempCustomerBalances(currentAccountNo, amounts, accountBal, txtNarration.Text.ToUpper().Trim(), txnRefNo, ReferenceNo, _module, liabilityGL, custNo, trnRetailCode)
                        InsertDailyTransaction(currentAccountNo)
                        InsertDailyTransactionHistory(currentAccountNo)
                        InsertGLTransaction(currentAccountNo)
                        InsertGLTransactionHistory(currentAccountNo)
                        InsertAllDailyTransaction(currentAccountNo)
                        InsertAllDailyTransactionHistory(currentAccountNo)
                        InsertAuditTrail(currentAccountNo)
                        InsertAuditTrailHistory(currentAccountNo)

                        UpdateCustomerBalances(currentAccountNo, amounts, transType.Trim(), accountBal)
                        UpdateCustomerGL(liabilityGL, amounts, transType.Trim(), currentAccountNo)
                        UpdateCashierGL(TransCode, amounts)

                        ' Mark this record as processed
                        MarkRecordAsProcessed(currentJournal, currentAccountNo, currentReference)
                    Else
                        ' Log error: Account details not found
                    End If

                Catch ex As Exception
                    ' Log the specific error for this record
                    ' Consider whether to continue with next record or stop
                    ' lblError.Text = "Error processing transaction: " & ex.Message
                End Try
            End While

            MyReader4.Close()

            ' Clean up processed records
            DropProcessedRecords()

        Catch ex As Exception
            ' Label4.Text = "Error Pulling Account Details"
            ' Log the error properly
        End Try
    End Sub

    ' Add this helper method to mark records as processed
    Private Sub MarkRecordAsProcessed(journalId As String, accountNo As String, reference As String)
        Try
            Dim MyDatObject As New DataManagement
            Dim updateSql As String = "UPDATE MB_RetailPosting_Temp SET Journal = 2 WHERE Journal = @Journal AND AccountNo = @AccountNo AND Reference = @Reference"
            ' Use parameterized query here!
            ' MyDatObject.DBAction(updateSql, parameters, ...)
        Catch ex As Exception
            ' Log error
        End Try
    End Sub

    ' Try one of these common enum values instead:
    Private Sub DropProcessedRecords()
        Try
            Dim MyDatObject As New DataManagement
            Dim deleteSql As String = "DELETE FROM MB_RetailPosting_Temp WHERE Journal = 2"

            ' Try these common enum values one by one:

            ' Option 1: Most common
            ' MyDatObject.DBAction(deleteSql, DataManagement.DBActionType.NonQuery)

            ' Option 2: If you have SqlCommand type
            ' MyDatObject.DBAction(deleteSql, DataManagement.DBActionType.SqlCommand)

            ' Option 3: Execute the query directly
            ' MyDatObject.DBAction(deleteSql, DataManagement.DBActionType.Execute)

            ' Option 4: Command
            ' MyDatObject.DBAction(deleteSql, DataManagement.DBActionType.Command)

        Catch ex As Exception
            ' Log error
        End Try
    End Sub

    Private Sub InsertDailyTransaction(AccountNo)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "INSERT INTO MB_Acc_DailyTransactions (SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,RCT_No,INV_No,Module,Reference,PayBy,ChequeNo,DR_GL,CR_GL,PostedBy,TxnDate,ActDate,Journal,SrNo,TxnRefNo,ExtSeqNo,BookBalance,AvailBal,GL_Control,CustomerNo,TxnCode)
SELECT SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,RCT_No,INV_No,Module,Reference,PayBy,ChequeNo,DR_GL,CR_GL,PostedBy,TxnDate,ActDate,Journal,RowNum,TxnRefNo,ExtSeqNo,BookBalance,AvailBal,LiabilityGL_Control,CustomerNo,TxnCode
FROM MB_RetailPosting_Temp
WHERE AccountNo = '" & AccountNo & "'"
        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    End Sub
    Private Sub InsertGLTransaction(AccountNo)
        'hapa iko shida na GL ac05112021
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "INSERT INTO [dbo].[MB_GL_DailyTransaction] (SeqNo,GLAccNo,GLName,TxnType,TxnBranch,AccountBranch,AcCCY,Amount,FX_Rate,LCY_Amount,Description,Module,Reference,UserName,TxnDate,SrNo,ActDate,TxnRefNo,ExtRefNo,AccountNo,CustomerNo,TxnCode)
SELECT SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,Module,Reference,PostedBy,TxnDate,RowNum ,ActDate,TxnRefNo,ExtSeqNo,AccountNo,CustomerNo,TxnCode FROM MB_RetailPosting_Temp
WHERE AccountNo = '" & AccountNo & "'"
        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    End Sub
    Private Sub InsertGLTransactionHistory(AccountNo)
        'hapa iko shida na GL ac 05112021
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "INSERT INTO [dbo].[MB_GL_DailyTransaction_History] (SeqNo,GLAccNo,GLName,TxnType,TxnBranch,AccountBranch,AcCCY,Amount,FX_Rate,LCY_Amount,Description,Module,Reference,UserName,TxnDate,SrNo,ActDate,TxnRefNo,ExtRefNo,AccountNo,CustomerNo,TxnCode)
SELECT SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,Module,Reference,PostedBy,TxnDate,RowNum ,ActDate,TxnRefNo,ExtSeqNo,AccountNo,CustomerNo,TxnCode FROM MB_RetailPosting_Temp
WHERE AccountNo = '" & AccountNo & "'"
        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    End Sub
    Private Sub InsertDailyTransactionHistory(AccountNo)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "INSERT INTO MB_Acc_DailyTransactions_History (SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,RCT_No,INV_No,Module,Reference,PayBy,ChequeNo,DR_GL,CR_GL,PostedBy,TxnDate,ActDate,Journal,SrNo,TxnRefNo,ExtSeqNo,BookBalance,AvailBal,GL_Control,CustomerNo,TxnCode)
SELECT SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,RCT_No,INV_No,Module,Reference,PayBy,ChequeNo,DR_GL,CR_GL,PostedBy,TxnDate,ActDate,Journal,RowNum,TxnRefNo,ExtSeqNo,BookBalance,AvailBal,LiabilityGL_Control,CustomerNo,TxnCode
FROM MB_RetailPosting_Temp
WHERE AccountNo = '" & AccountNo & "'"
        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    End Sub
    Private Sub InsertAllDailyTransaction(AccountNo)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "INSERT INTO MB_All_DailyTransactions
(SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccountBranch,AcCCY,Amount,FX_Rate,LCY_Amount,Description,InvoiceNo,Module,Reference,UserName,TxnDate,ActDate,SrNo,TxnRefNo,ExtRefNo,Contra_AccNo,CustomerNo,TxnCode)
SELECT SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,INV_No,Module,Reference,PostedBy,TxnDate,ActDate,RowNum,TxnRefNo,ExtSeqNo,CR_GL,CustomerNo,TxnCode
FROM MB_RetailPosting_Temp
WHERE AccountNo = '" & AccountNo & "'"
        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    End Sub
    Private Sub InsertAllDailyTransactionHistory(AccountNo)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "INSERT INTO MB_All_DailyTransactions_History
(SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccountBranch,AcCCY,Amount,FX_Rate,LCY_Amount,Description,InvoiceNo,Module,Reference,UserName,TxnDate,ActDate,SrNo,TxnRefNo,ExtRefNo,Contra_AccNo,CustomerNo,TxnCode)
SELECT SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,INV_No,Module,Reference,PostedBy,TxnDate,ActDate,RowNum,TxnRefNo,ExtSeqNo,CR_GL,CustomerNo,TxnCode
FROM MB_RetailPosting_Temp
WHERE AccountNo = '" & AccountNo & "'"
    End Sub
    Private Sub InsertAuditTrail(AccountNo)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "INSERT INTO MB_TransactionsAuditTrail
(SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccountBranch,AcCCY,Amount,FX_Rate,LCY_Amount,Description,InvoiceNo,Module,Reference,UserName,TxnDate,ActDate,SrNo,TxnRefNo,ExtRefNo,Contra_AccNo,CustomerNo,TxnCode)
SELECT SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,INV_No,Module,Reference,PostedBy,TxnDate,ActDate,RowNum,TxnRefNo,ExtSeqNo,CR_GL,CustomerNo,TxnCode
FROM MB_RetailPosting_Temp
WHERE AccountNo = '" & AccountNo & "'"
    End Sub
    'audit history
    Private Sub InsertAuditTrailHistory(AccountNo)
        Dim mySqlAction As String = ""
        Dim MyDBAction = "INSERT INTO MB_TransactionsAuditTrail_History
(SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccountBranch,AcCCY,Amount,FX_Rate,LCY_Amount,Description,InvoiceNo,Module,Reference,UserName,TxnDate,ActDate,SrNo,TxnRefNo,ExtRefNo,Contra_AccNo,CustomerNo,TxnCode)
SELECT SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,INV_No,Module,Reference,PostedBy,TxnDate,ActDate,RowNum,TxnRefNo,ExtSeqNo,CR_GL,CustomerNo,TxnCode
FROM MB_RetailPosting_Temp
WHERE AccountNo = '" & AccountNo & "'"
    End Sub
    Private Sub DropTempTable(accountno)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "delete from MB_RetailPosting_Temp where AccountNo = '" & accountno & "' "
        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Delete)
    End Sub
    Private Sub DropBilkRecords()
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "delete from MB_BulkUploadRecordsss"
        MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Delete)
    End Sub
    'Public Sub GetAccountDetails(AccountNo)
    '    Try
    '        'lblSuccessMsg.Text = ""
    '        'lblAccountName.Text = ""
    '        '' txtAmount.Text = ""
    '        'If txtAccountNo.Text = "" Then
    '        '    txtAccountNo.Focus()
    '        '    lblSuccessMsg.Text = " Please enter Account Number"
    '        '    Exit Sub
    '        'End If
    '        Dim MyDatObject As New DataManagement
    '        Dim mySearch4 As String = "select AccountNo,CustomerNo,AccountName,AssetGL_Control,LiabilityGL_Control,BranchCode,AvailBal,BookBalance,ProductDefCode from [dbo].[tbAccount] where AccountNo = '" & AccountNo & "'" ' ='" & Me.HiddenField1.Value & "' "
    '        Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
    '        If MyReader4.HasRows = True Then
    '            MyReader4.Read()
    '            'lblAccountName.Text = MyReader4("AccountName").ToString
    '            'P_AccountNo = MyReader4("TxnCode").ToString
    '            P_CustomerNo = MyReader4("CustomerNo").ToString
    '            P_AccountName = MyReader4("AccountName").ToString
    '            P_AssetGL_Control = MyReader4("AssetGL_Control").ToString
    '            P_LiabilityGL_Control = MyReader4("LiabilityGL_Control").ToString
    '            P_BranchCode = MyReader4("BranchCode").ToString
    '            P_AvailBal = MyReader4("AvailBal").ToString
    '            P_BookBalance = MyReader4("BookBalance").ToString
    '            P_DefinationCode = MyReader4("ProductDefCode").ToString().Trim()
    '        Else
    '            'lblSuccessMsg.Text = "Account Number does not exist"
    '        End If
    '        MyReader4.Close()
    '    Catch ex As Exception
    '        'txtOrgName.Text = ""
    '        'txtAdress1.Text = ""
    '        '' DropDownList1.Text = ""
    '        'txtOrgCode.ReadOnly = False
    '        'Label4.Text = "Error Pulling Account Details"
    '    End Try
    'End Sub
    'Public Function GetAccountBal(AccountNo)
    '    Dim accountBalance As Decimal = 0.00
    '    Try
    '        Dim MyDatObject As New DataManagement
    '        Dim mySearch4 As String = "select AccountNo,CustomerNo,AccountName,AssetGL_Control,LiabilityGL_Control,BranchCode,AvailBal,BookBalance,ProductDefCode from [dbo].[tbAccount] where AccountNo = '" & AccountNo & "'" ' ='" & Me.HiddenField1.Value & "' "
    '        Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
    '        If MyReader4.HasRows = True Then
    '            MyReader4.Read()

    '            accountBalance = MyReader4("BookBalance").ToString
    '        Else
    '            'lblSuccessMsg.Text = "Account Number does not exist"
    '        End If
    '        MyReader4.Close()
    '    Catch ex As Exception
    '        'txtOrgName.Text = ""
    '        'txtAdress1.Text = ""
    '        '' DropDownList1.Text = ""
    '        'txtOrgCode.ReadOnly = False
    '        'Label4.Text = "Error Pulling Account Details"
    '    End Try
    '    Return accountBalance
    'End Function
    'hapa
    Public Function GetAccountDetails(accountNo) As Hashtable
        P_AccountDetailsHashMap.Clear()

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
                P_AccountDetailsHashMap.Add("ProductDefCode", MyReader4("ProductDefCode").ToString())

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
    Public Function GetAccountGL(AccountNo)
        Dim accountGL As String = ""
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select AccountNo,CustomerNo,AccountName,AssetGL_Control,LiabilityGL_Control,BranchCode,AvailBal,BookBalance,ProductDefCode from [dbo].[tbAccount] where AccountNo = '" & AccountNo & "'" ' ='" & Me.HiddenField1.Value & "' "
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()

                accountGL = MyReader4("LiabilityGL_Control").ToString
            Else
                'lblSuccessMsg.Text = "Account Number does not exist"
            End If
        Catch ex As Exception
            'txtOrgName.Text = ""
            'txtAdress1.Text = ""
            '' DropDownList1.Text = ""
            'txtOrgCode.ReadOnly = False
            'Label4.Text = "Error Pulling Account Details"
        End Try
        Return accountGL
    End Function
    Protected Sub UpdateTempCustomerBalances(accountNo, amount, P_BookBalance, Narration, txnRefNo, ReferenceNo, moduler, liability_GL, custNo, trnRetailCode)
        Try
            Dim newAccBal As String = ""
            newAccBal = P_BookBalance + Val(amount)
            Dim MySqlaction1 As String
            Dim mySqlAction As String = ""
            Dim MyCurrencyName As String = ""
            Dim MyDBAction As New DataManagement
            MySqlaction1 = "update MB_RetailPosting_Temp set AvailBal ='" & newAccBal & "',BookBalance ='" & newAccBal & "', Details = '" & Narration & "', TxnRefNo = '" & txnRefNo & "',Reference = '" & ReferenceNo & "',FX_Rate = '1.00',PayBy ='MATRIX LTD',PostedBy ='WAITARA',Module = '" & moduler & "',LiabilityGL_Control = '" & liability_GL & "',CustomerNo = '" & custNo & "',TxnCode = '" & trnRetailCode & "'  where AccountNo ='" & accountNo & "'"
            MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
            'Dim displayBal As String = ""
            'displayBal = Format(newAccBal, "###,###")
            'txtNewBalance.Text = newAccBal
        Catch ex As Exception
            'Label4.Text = "Error Inserting"
        End Try
    End Sub
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
                    Dim mySearch5 As String = "select GLACCTNO,GLNAME,ACT_BAL,LCY_BAL from GL_DETAIL where GLACCTNO = '" & cashierGL & "' " ' ='" & Me.HiddenField1.Value & "' "
                    Dim MyReader5 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch5, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
                    If MyReader5.HasRows = True Then
                        MyReader5.Read()
                        Dim GLBal As String = ""
                        Dim newGLBal As String = ""
                        GLBal = MyReader5("ACT_BAL").ToString
                        'cashierGLName = MyReader5("GLName").ToString
                        newGLBal = GLBal - amounts
                        Dim MySqlaction1 As String
                        Dim mySqlAction As String = ""
                        Dim MyCurrencyName As String = ""
                        Dim MyDBAction As New DataManagement
                        MySqlaction1 = "update GL_DETAIL set ACT_BAL ='" & newGLBal & "' where GLACCTNO = '" & cashierGL & "'"
                        MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
                    End If
                    MyReader5.Close()
                Catch ex As Exception
                End Try
            End If
        Catch ex As Exception
            'Label4.Text = "Error Pulling Account Details, Commission A/c must start 01M8 .... E6 "
        End Try
    End Sub

    Protected Sub btncheckTranCode_Click(sender As Object, e As EventArgs) Handles btncheckTranCode.Click
        GetTransactionCode(Me.txtTransactionCode.Text.Trim)
    End Sub
    Protected Function GetTransactionCode(transCode)
        Try
            lblMessage.Text = ""
            lblDescription.Text = ""
            'lblAccountName.Text = ""
            If txtTransactionCode.Text = "" Then
                txtTransactionCode.Focus()
                lblMessage.Text = " Please enter Transaction Code"
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
                    lblMessage.Text = "Invalid transaction code"
                    lblDescription.Text = ""
                    Exit Function
                End If
                If P_TxnDesc = "BULK UPLOAD" Then
                Else
                    lblMessage.Text = "Invalid transaction code"
                    lblDescription.Text = ""
                    Exit Function
                End If
            Else
                lblMessage.Text = "Transaction Code does not exist"
                Exit Function
            End If
            MyReader4.Close()
        Catch ex As Exception
            'Label4.Text = "Error Pulling Account Details"
        End Try
    End Function
End Class