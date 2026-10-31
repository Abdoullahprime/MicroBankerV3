Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Imports System.Data.SqlClient
Imports System.Globalization
Imports System.Text

Public Class ProcessRoutineSOD
    Inherits System.Web.UI.Page

    ' Constants for better maintainability
    Private Const LOAN_PRODUCT_CODE As String = "LA"
    Private Const REPAYMENT_TXN_CODE As String = "C90"
    Private Const REPAYMENT_DESCRIPTION As String = "REPAYMENTS"
    Private Const DATE_FORMAT As String = "dd/MM/yyyy"

    ' Class-level variables
    Private EOD As Date, SOD As Date, EOM As Date, EOY As Date
    Private ProcessStatus As New ArrayList()
    Private connectionString As String = ""

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            connectionString = ConfigurationManager.ConnectionStrings("ConnectionString").ConnectionString
            EODDatePicker.Visible = False
            LoadProcessRoutines()
            LoadSystemDates()
        End If
    End Sub

    ' ==================== DATE MANAGEMENT ====================

    Private Sub LoadSystemDates()
        Try
            Dim query As String = "SELECT EOD, SOD, EOM, EOY FROM [dbo].[SYSTEM_DATES]"

            Using connection As New SqlConnection(connectionString)
                Using command As New SqlCommand(query, connection)
                    connection.Open()
                    Using reader As SqlDataReader = command.ExecuteReader()
                        If reader.HasRows AndAlso reader.Read() Then
                            txtEOD.Text = Convert.ToDateTime(reader("EOD")).ToString(DATE_FORMAT)
                            txtSOD.Text = Convert.ToDateTime(reader("SOD")).ToString(DATE_FORMAT)
                            txtEOM.Text = Convert.ToDateTime(reader("EOM")).ToString(DATE_FORMAT)
                            txtEOY.Text = Convert.ToDateTime(reader("EOY")).ToString(DATE_FORMAT)
                        Else
                            ShowMessage("Could not retrieve system dates", True)
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            LogError(ex, "LoadSystemDates")
            ShowMessage("Error loading system dates", True)
        End Try
    End Sub

    Private Sub UpdateSystemDates()
        Try
            If String.IsNullOrEmpty(txtSOD.Text) Then
                ShowMessage("SOD date cannot be empty", True)
                Return
            End If

            Dim parsedDate As Date
            If Not Date.TryParseExact(txtSOD.Text, DATE_FORMAT, CultureInfo.InvariantCulture,
                                      DateTimeStyles.None, parsedDate) Then
                ShowMessage("Invalid date format. Use DD/MM/YYYY", True)
                Return
            End If

            Dim newSOD As DateTime = parsedDate.AddDays(1)
            Dim query As String = "UPDATE [dbo].[SYSTEM_DATES] SET SOD = @SOD"

            Using connection As New SqlConnection(connectionString)
                Using command As New SqlCommand(query, connection)
                    command.Parameters.AddWithValue("@SOD", newSOD)
                    connection.Open()
                    command.ExecuteNonQuery()
                End Using
            End Using

            ShowMessage("System dates updated successfully", False)
            LoadSystemDates() ' Refresh display

        Catch ex As Exception
            LogError(ex, "UpdateSystemDates")
            ShowMessage("Error updating system dates", True)
        End Try
    End Sub

    ' ==================== PROCESS ROUTINE MANAGEMENT ====================

    Private Sub LoadProcessRoutines()
        Try
            Dim processGroup As String = "SOD"
            Dim query As String = "SELECT idx, ProcessID, ProcessName, ProcessFunction, ProcessGroup, ExecutionOrder 
                                  FROM MB_ProcessRoutine 
                                  WHERE ProcessGroup = @ProcessGroup 
                                  ORDER BY ExecutionOrder"

            Using connection As New SqlConnection(connectionString)
                Using command As New SqlCommand(query, connection)
                    command.Parameters.AddWithValue("@ProcessGroup", processGroup)

                    Dim adapter As New SqlDataAdapter(command)
                    Dim dataTable As New DataTable()
                    adapter.Fill(dataTable)

                    SODGridView.DataSource = dataTable
                    SODGridView.DataBind()
                End Using
            End Using

        Catch ex As Exception
            LogError(ex, "LoadProcessRoutines")
            ShowMessage("Error loading process routines", True)
        End Try
    End Sub

    ' ==================== TRANSACTION PROCESSING ====================

    Protected Sub btnProcess_Click(sender As Object, e As EventArgs)
        Try
            Dim lb As LinkButton = DirectCast(sender, LinkButton)
            Dim gvRow As GridViewRow = DirectCast(lb.NamingContainer, GridViewRow)
            Dim selectCheck As CheckBox = CType(gvRow.FindControl("checkSelect"), CheckBox)

            If selectCheck Is Nothing OrElse Not selectCheck.Checked Then
                ShowMessage("Please select a process to execute", True)
                Return
            End If

            If String.IsNullOrEmpty(txtDate.Text) Then
                ShowMessage("Please enter the current date", True)
                txtDate.Focus()
                Return
            End If

            ' Validate date format
            Dim currentDate As Date
            If Not Date.TryParseExact(txtDate.Text, DATE_FORMAT, CultureInfo.InvariantCulture,
                                      DateTimeStyles.None, currentDate) Then
                ShowMessage("Invalid date format. Use DD/MM/YYYY", True)
                Return
            End If

            ' Process all selected loans
            Dim result As Boolean = ProcessLoanRepayments()

            If result Then
                ShowMessage("Process completed successfully", False)
            Else
                ShowMessage("Process completed with errors. Check logs.", True)
            End If

        Catch ex As Exception
            LogError(ex, "btnProcess_Click")
            ShowMessage("Error processing transaction", True)
        End Try
    End Sub

    Private Function ProcessLoanRepayments() As Boolean
        Dim successCount As Integer = 0
        Dim failCount As Integer = 0
        Dim accountsProcessed As New List(Of String)()

        Try
            ' Get all active loan accounts with SOD_Status = 0
            Dim loansToProcess As DataTable = GetLoanAccountsToProcess()

            If loansToProcess.Rows.Count = 0 Then
                ShowMessage("No loan accounts found to process", False)
                Return True
            End If

            ' Get transaction reference once
            Dim transactionRefCode As String = GetTransactionRefCode()
            If String.IsNullOrEmpty(transactionRefCode) Then
                ShowMessage("Unable to get transaction reference code", True)
                Return False
            End If

            ' Get sequence number once
            Dim seqNo As String = GetNextSequenceNumber()
            If String.IsNullOrEmpty(seqNo) Then
                ShowMessage("Unable to get sequence number", True)
                Return False
            End If

            Dim txnDate As DateTime = DateTime.Now
            Dim actionDate As String = UtilitiesModule.GetTransctionDate()

            For Each loanRow As DataRow In loansToProcess.Rows
                Try
                    ProcessSingleLoan(loanRow, transactionRefCode, seqNo, txnDate, actionDate)
                    successCount += 1
                    accountsProcessed.Add(loanRow("AccountNo").ToString())

                Catch ex As Exception
                    failCount += 1
                    LogError(ex, $"Processing account: {loanRow("AccountNo")}")
                End Try
            Next

            ShowMessage($"Processing complete. Success: {successCount}, Failed: {failCount}", failCount > 0)
            Return failCount = 0

        Catch ex As Exception
            LogError(ex, "ProcessLoanRepayments")
            Return False
        End Try
    End Function

    Private Function GetLoanAccountsToProcess() As DataTable
        Dim query As String = "SELECT AccountNo, AccountName, BranchCode, CustomerNo, SettlementsAccountNo, 
                                     RepaymentAmount, AssetGL_Control, LiabilityGL_Control, ProductDefCode, 
                                     AvailBal, BookBalance 
                              FROM tbAccount 
                              WHERE SOD_Status = 0 AND ProductDefCode = @ProductCode"

        Using connection As New SqlConnection(connectionString)
            Using command As New SqlCommand(query, connection)
                command.Parameters.AddWithValue("@ProductCode", LOAN_PRODUCT_CODE)

                Dim adapter As New SqlDataAdapter(command)
                Dim dataTable As New DataTable()
                adapter.Fill(dataTable)

                Return dataTable
            End Using
        End Using
    End Function

    Private Sub ProcessSingleLoan(loanRow As DataRow, transactionRefCode As String,
                                 seqNo As String, txnDate As DateTime, actionDate As String)

        Using transaction As SqlTransaction = BeginTransaction()
            Try
                ' Get loan account details
                Dim loanAccountNo As String = loanRow("AccountNo").ToString()
                Dim loanProductCode As String = loanRow("ProductDefCode").ToString()
                Dim loanGL As String = loanRow("AssetGL_Control").ToString()
                Dim repaymentAmount As Decimal = Convert.ToDecimal(loanRow("RepaymentAmount"))
                Dim loanBalance As Decimal = Convert.ToDecimal(loanRow("AvailBal"))
                Dim loanAccountName As String = loanRow("AccountName").ToString()
                Dim loanBranch As String = loanRow("BranchCode").ToString()
                Dim loanCustomerNo As String = loanRow("CustomerNo").ToString()

                ' Get settlement account details
                Dim settlementAccountNo As String = loanRow("SettlementsAccountNo").ToString()
                Dim settlementDetails As Dictionary(Of String, Object) = GetAccountDetails(settlementAccountNo)

                If settlementDetails Is Nothing Then
                    Throw New Exception($"Settlement account {settlementAccountNo} not found")
                End If

                Dim settlementBalance As Decimal = Convert.ToDecimal(settlementDetails("AvailBal"))
                Dim settlementGL As String = settlementDetails("LiabilityGLControl").ToString()
                Dim settlementAccountName As String = settlementDetails("AccountName").ToString()
                Dim settlementBranch As String = settlementDetails("BranchCode").ToString()
                Dim settlementCustomerNo As String = settlementDetails("CustomerNo").ToString()
                Dim settlementProductCode As String = settlementDetails("ProductDefCode").ToString()

                ' Validate sufficient funds in settlement account
                If settlementBalance < repaymentAmount Then
                    Throw New Exception($"Insufficient funds in settlement account {settlementAccountNo}. " &
                                       $"Balance: {settlementBalance}, Required: {repaymentAmount}")
                End If

                ' Get module codes
                Dim loanModuleCode As String = GetModuleRefCode(loanProductCode, transaction)
                Dim settlementModuleCode As String = GetModuleRefCode(settlementProductCode, transaction)

                ' Create reference numbers
                Dim dayPart As String = txnDate.Day.ToString().PadLeft(2, "0")
                Dim monthPart As String = txnDate.Month.ToString().PadLeft(2, "0")
                Dim yearPart As String = txnDate.Year.ToString().Substring(2, 2)
                Dim baseRef As String = transactionRefCode.ToUpper() & seqNo & dayPart & monthPart & yearPart

                ' Process loan account (Credit)
                ProcessAccountTransaction(
                    transaction, loanAccountNo, loanAccountName, loanBranch, loanCustomerNo,
                    repaymentAmount, "C", loanGL, loanModuleCode, baseRef, seqNo,
                    $"{REPAYMENT_DESCRIPTION} - {loanAccountNo}", loanBalance + repaymentAmount,
                    loanBalance + repaymentAmount, REPAYMENT_TXN_CODE, "1"
                )

                ' Process settlement account (Debit)
                ProcessAccountTransaction(
                    transaction, settlementAccountNo, settlementAccountName, settlementBranch, settlementCustomerNo,
                    repaymentAmount, "D", settlementGL, settlementModuleCode, baseRef, seqNo,
                    $"{REPAYMENT_DESCRIPTION} - {settlementAccountNo}", settlementBalance - repaymentAmount,
                    settlementBalance - repaymentAmount, REPAYMENT_TXN_CODE, "2"
                )

                transaction.Commit()

            Catch ex As Exception
                transaction.Rollback()
                Throw
            End Try
        End Using
    End Sub

    Private Function BeginTransaction() As SqlTransaction
        Dim connection As New SqlConnection(connectionString)
        connection.Open()
        Return connection.BeginTransaction()
    End Function

    Private Sub ProcessAccountTransaction(transaction As SqlTransaction, accountNo As String,
                                         accountName As String, branch As String, customerNo As String,
                                         amount As Decimal, txnType As String, glAccount As String,
                                         moduleCode As String, baseRef As String, seqNo As String,
                                         description As String, newBookBalance As Decimal,
                                         newAvailBalance As Decimal, txnCode As String, rowNum As String)

        Dim connection As SqlConnection = transaction.Connection

        ' Generate unique reference numbers
        Dim txnRefNo As String = $"{moduleCode}{baseRef}{rowNum}"
        Dim journalRef As String = $"{moduleCode}{baseRef}"

        ' Insert into temp table
        InsertIntoTempTable(connection, transaction, rowNum, seqNo, glAccount, txnCode, accountNo,
                           accountName, txnType, branch, amount, description, moduleCode, txnRefNo,
                           journalRef, newBookBalance, newAvailBalance, customerNo)

        ' Insert into daily tables
        InsertIntoDailyTables(connection, transaction, accountNo)

        ' Update account balances
        UpdateAccountBalance(connection, transaction, accountNo, amount, txnType)

        ' Update GL
        UpdateGLBalance(connection, transaction, glAccount, amount, txnType)

        ' Update SOD status
        UpdateSODStatus(connection, transaction, accountNo)
    End Sub

    Private Sub InsertIntoTempTable(connection As SqlConnection, transaction As SqlTransaction,
                                   rowNum As String, seqNo As String, glAccount As String, txnCode As String,
                                   accountNo As String, accountName As String, txnType As String,
                                   branch As String, amount As Decimal, description As String,
                                   moduleCode As String, txnRefNo As String, journalRef As String,
                                   bookBalance As Decimal, availBalance As Decimal, customerNo As String)

        Dim query As String = "INSERT INTO MB_RetailPosting_Temp (
            RowNum, SeqNo, LiabilityGL_Control, TxnCode, AccountNo, AccountName, TxnType, TxnBranch,
            AccBanch, Acc_CCY, Amount, FX_Rate, LCY_Amount, Details, RCT_No, INV_No, Module, Reference,
            PayBy, ChequeNo, DR_GL, CR_GL, PostedBy, TxnDate, ActDate, Journal, TxnRefNo, ExtSeqNo,
            BookBalance, AvailBal, CustomerNo
        ) VALUES (
            @RowNum, @SeqNo, @LiabilityGL_Control, @TxnCode, @AccountNo, @AccountName, @TxnType, @TxnBranch,
            '', 'GMD', @Amount, '', @Amount, @Details, '00', '', @Module, @Reference,
            '', '', '', '', '', @TxnDate, @ActDate, @Journal, @TxnRefNo, '',
            @BookBalance, @AvailBal, @CustomerNo
        )"

        Using command As New SqlCommand(query, connection, transaction)
            command.Parameters.AddWithValue("@RowNum", rowNum)
            command.Parameters.AddWithValue("@SeqNo", seqNo)
            command.Parameters.AddWithValue("@LiabilityGL_Control", glAccount)
            command.Parameters.AddWithValue("@TxnCode", txnCode)
            command.Parameters.AddWithValue("@AccountNo", accountNo)
            command.Parameters.AddWithValue("@AccountName", accountName)
            command.Parameters.AddWithValue("@TxnType", txnType)
            command.Parameters.AddWithValue("@TxnBranch", branch)
            command.Parameters.AddWithValue("@Amount", amount)
            command.Parameters.AddWithValue("@Details", description)
            command.Parameters.AddWithValue("@Module", moduleCode)
            command.Parameters.AddWithValue("@Reference", txnRefNo)
            command.Parameters.AddWithValue("@TxnDate", DateTime.Now)
            command.Parameters.AddWithValue("@ActDate", DateTime.Now)
            command.Parameters.AddWithValue("@Journal", journalRef)
            command.Parameters.AddWithValue("@TxnRefNo", txnRefNo)
            command.Parameters.AddWithValue("@BookBalance", bookBalance)
            command.Parameters.AddWithValue("@AvailBal", availBalance)
            command.Parameters.AddWithValue("@CustomerNo", customerNo)

            command.ExecuteNonQuery()
        End Using
    End Sub

    Private Sub InsertIntoDailyTables(connection As SqlConnection, transaction As SqlTransaction, accountNo As String)
        Dim tables As String() = {
            "MB_Acc_DailyTransactions",
            "MB_Acc_DailyTransactions_History",
            "MB_GL_DailyTransaction",
            "MB_GL_DailyTransaction_History",
            "MB_All_DailyTransactions",
            "MB_All_DailyTransactions_History"
        }

        For Each tableName As String In tables
            Dim query As String = GetInsertQueryForTable(tableName)
            If Not String.IsNullOrEmpty(query) Then
                Using command As New SqlCommand(query, connection, transaction)
                    command.Parameters.AddWithValue("@AccountNo", accountNo)
                    command.ExecuteNonQuery()
                End Using
            End If
        Next
    End Sub

    Private Function GetInsertQueryForTable(tableName As String) As String
        Select Case tableName
            Case "MB_Acc_DailyTransactions"
                Return "INSERT INTO MB_Acc_DailyTransactions (SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,RCT_No,INV_No,Module,Reference,PayBy,ChequeNo,DR_GL,CR_GL,PostedBy,TxnDate,ActDate,Journal,SrNo,TxnRefNo,ExtSeqNo,BookBalance,AvailBal) SELECT SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,RCT_No,INV_No,Module,Reference,PayBy,ChequeNo,DR_GL,CR_GL,PostedBy,TxnDate,ActDate,Journal,RowNum,TxnRefNo,ExtSeqNo,BookBalance,AvailBal FROM MB_RetailPosting_Temp WHERE AccountNo = @AccountNo"

            Case "MB_Acc_DailyTransactions_History"
                Return "INSERT INTO MB_Acc_DailyTransactions_History (SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,RCT_No,INV_No,Module,Reference,PayBy,ChequeNo,DR_GL,CR_GL,PostedBy,TxnDate,ActDate,Journal,SrNo,TxnRefNo,ExtSeqNo,BookBalance,AvailBal) SELECT SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,RCT_No,INV_No,Module,Reference,PayBy,ChequeNo,DR_GL,CR_GL,PostedBy,TxnDate,ActDate,Journal,RowNum,TxnRefNo,ExtSeqNo,BookBalance,AvailBal FROM MB_RetailPosting_Temp WHERE AccountNo = @AccountNo"

            Case "MB_GL_DailyTransaction"
                Return "INSERT INTO MB_GL_DailyTransaction (SeqNo,GLAccNo,GLName,TxnType,TxnBranch,AccountBranch,AcCCY,Amount,FX_Rate,LCY_Amount,Description,Module,Reference,UserName,TxnDate,SrNo,ActDate,TxnRefNo,ExtRefNo) SELECT SeqNo,LiabilityGL_Control,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,Module,Reference,PostedBy,TxnDate,RowNum,ActDate,TxnRefNo,ExtSeqNo FROM MB_RetailPosting_Temp WHERE AccountNo = @AccountNo"

            Case "MB_GL_DailyTransaction_History"
                Return "INSERT INTO MB_GL_DailyTransaction_History (SeqNo,GLAccNo,GLName,TxnType,TxnBranch,AccountBranch,AcCCY,Amount,FX_Rate,LCY_Amount,Description,Module,Reference,UserName,TxnDate,SrNo,ActDate,TxnRefNo,ExtRefNo) SELECT SeqNo,LiabilityGL_Control,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,Module,Reference,PostedBy,TxnDate,RowNum,ActDate,TxnRefNo,ExtSeqNo FROM MB_RetailPosting_Temp WHERE AccountNo = @AccountNo"

            Case "MB_All_DailyTransactions"
                Return "INSERT INTO MB_All_DailyTransactions (SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccountBranch,AcCCY,Amount,FX_Rate,LCY_Amount,Description,Module,Reference,UserName,TxnDate,ActDate,SrNo,TxnRefNo,ExtRefNo) SELECT SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,Module,Reference,PostedBy,TxnDate,ActDate,RowNum,TxnRefNo,ExtSeqNo FROM MB_RetailPosting_Temp WHERE AccountNo = @AccountNo"

            Case "MB_All_DailyTransactions_History"
                Return "INSERT INTO MB_All_DailyTransactions_History (SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccountBranch,AcCCY,Amount,FX_Rate,LCY_Amount,Description,Module,Reference,UserName,TxnDate,ActDate,SrNo,TxnRefNo,ExtRefNo) SELECT SeqNo,AccountNo,AccountName,TxnType,TxnBranch,AccBanch,Acc_CCY,Amount,FX_Rate,LCY_Amount,Details,Module,Reference,PostedBy,TxnDate,ActDate,RowNum,TxnRefNo,ExtSeqNo FROM MB_RetailPosting_Temp WHERE AccountNo = @AccountNo"

            Case Else
                Return String.Empty
        End Select
    End Function

    Private Sub UpdateAccountBalance(connection As SqlConnection, transaction As SqlTransaction,
                                    accountNo As String, amount As Decimal, txnType As String)

        Dim updateQuery As String
        If txnType = "C" Then
            updateQuery = "UPDATE tbAccount SET AvailBal = AvailBal + @Amount, BookBalance = BookBalance + @Amount, 
                          LastActivityDate = @ActivityDate, LastCRactvtyDate = @ActivityDate, LastCRDate = @ActivityDate 
                          WHERE AccountNo = @AccountNo"
        Else
            updateQuery = "UPDATE tbAccount SET AvailBal = AvailBal - @Amount, BookBalance = BookBalance - @Amount, 
                          LastActivityDate = @ActivityDate, LastDRactvtyDate = @ActivityDate, LastDRDate = @ActivityDate 
                          WHERE AccountNo = @AccountNo"
        End If

        Using command As New SqlCommand(updateQuery, connection, transaction)
            command.Parameters.AddWithValue("@Amount", amount)
            command.Parameters.AddWithValue("@AccountNo", accountNo)
            command.Parameters.AddWithValue("@ActivityDate", DateTime.Now)
            command.ExecuteNonQuery()
        End Using
    End Sub

    Private Sub UpdateGLBalance(connection As SqlConnection, transaction As SqlTransaction,
                               glAccount As String, amount As Decimal, txnType As String)

        Dim updateQuery As String
        If txnType = "C" Then
            updateQuery = "UPDATE GL_DETAIL SET ACT_BAL = ACT_BAL + @Amount, LCY_BAL = LCY_BAL + @Amount 
                          WHERE GLACCTNO = @GLAccount"
        Else
            updateQuery = "UPDATE GL_DETAIL SET ACT_BAL = ACT_BAL - @Amount, LCY_BAL = LCY_BAL - @Amount 
                          WHERE GLACCTNO = @GLAccount"
        End If

        Using command As New SqlCommand(updateQuery, connection, transaction)
            command.Parameters.AddWithValue("@Amount", amount)
            command.Parameters.AddWithValue("@GLAccount", glAccount)
            command.ExecuteNonQuery()
        End Using
    End Sub

    Private Sub UpdateSODStatus(connection As SqlConnection, transaction As SqlTransaction, accountNo As String)
        Dim query As String = "UPDATE tbAccount SET SOD_Status = 1 WHERE AccountNo = @AccountNo"

        Using command As New SqlCommand(query, connection, transaction)
            command.Parameters.AddWithValue("@AccountNo", accountNo)
            command.ExecuteNonQuery()
        End Using
    End Sub

    ' ==================== HELPER METHODS ====================

    Private Function GetAccountDetails(accountNo As String) As Dictionary(Of String, Object)
        Dim query As String = "SELECT AccountNo, CustomerNo, AccountName, AssetGL_Control, LiabilityGL_Control, 
                                     BranchCode, AvailBal, BookBalance, ProductDefCode 
                              FROM tbAccount 
                              WHERE AccountNo = @AccountNo"

        Using connection As New SqlConnection(connectionString)
            Using command As New SqlCommand(query, connection)
                command.Parameters.AddWithValue("@AccountNo", accountNo)
                connection.Open()

                Using reader As SqlDataReader = command.ExecuteReader()
                    If reader.HasRows AndAlso reader.Read() Then
                        Dim details As New Dictionary(Of String, Object)()
                        details("CustomerNo") = reader("CustomerNo").ToString()
                        details("AccountName") = reader("AccountName").ToString()
                        details("AssetGLControl") = reader("AssetGL_Control").ToString()
                        details("LiabilityGLControl") = reader("LiabilityGL_Control").ToString()
                        details("BranchCode") = reader("BranchCode").ToString()
                        details("AvailBal") = Convert.ToDecimal(reader("AvailBal"))
                        details("BookBalance") = Convert.ToDecimal(reader("BookBalance"))
                        details("ProductDefCode") = reader("ProductDefCode").ToString()
                        Return details
                    End If
                End Using
            End Using
        End Using

        Return Nothing
    End Function

    Private Function GetTransactionRefCode() As String
        Dim query As String = "SELECT RefCode FROM tbTransactionCode WHERE TxnCode = @TxnCode"

        Using connection As New SqlConnection(connectionString)
            Using command As New SqlCommand(query, connection)
                command.Parameters.AddWithValue("@TxnCode", REPAYMENT_TXN_CODE)
                connection.Open()

                Dim result As Object = command.ExecuteScalar()
                Return If(result IsNot Nothing, result.ToString(), String.Empty)
            End Using
        End Using
    End Function

    Private Function GetModuleRefCode(productCode As String, transaction As SqlTransaction) As String
        Dim query As String = "SELECT RefCode FROM MB_ProductDefinitions WHERE ProductDefCode = @ProductCode"

        Using command As New SqlCommand(query, transaction.Connection, transaction)
            command.Parameters.AddWithValue("@ProductCode", productCode)

            Dim result As Object = command.ExecuteScalar()
            Return If(result IsNot Nothing, result.ToString(), String.Empty)
        End Using
    End Function

    Private Function GetNextSequenceNumber() As String
        ' Call the method to generate the next sequence number
        UtilitiesModule.GetNextSeqNo("LAST_SEQNO")
        ' Return the value stored in the global variable
        Return UtilitiesModule.returnValue
    End Function

    ' ==================== UI EVENT HANDLERS ====================

    Protected Sub btChanageDates_Click(sender As Object, e As EventArgs) Handles btChanageDates.Click
        UpdateSystemDates()
    End Sub

    Protected Sub btncalendars_Click(sender As Object, e As ImageClickEventArgs) Handles btncalendars.Click
        EODDatePicker.Visible = True
    End Sub

    Protected Sub EODDatePicker_SelectionChanged(sender As Object, e As EventArgs) Handles EODDatePicker.SelectionChanged
        EODDatePicker.Visible = False
        txtDate.Text = EODDatePicker.SelectedDate.ToString(DATE_FORMAT)
    End Sub

    Protected Sub SODGridView_RowCreated(sender As Object, e As GridViewRowEventArgs)
        If e.Row.RowType = DataControlRowType.DataRow Then
            Dim dt As DataTable = DirectCast(ViewState("CurrentTable"), DataTable)
            If dt IsNot Nothing Then
                Dim lb As LinkButton = DirectCast(e.Row.FindControl("checkSelect"), LinkButton)
                If lb IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                    If e.Row.RowIndex = dt.Rows.Count - 1 Then
                        lb.Visible = False
                    End If
                End If
            End If
        End If
    End Sub

    ' ==================== UTILITY METHODS ====================

    Private Sub ShowMessage(message As String, isError As Boolean)
        lblSuccessMsg.Text = message
        lblSuccessMsg.ForeColor = If(isError, Drawing.Color.Red, Drawing.Color.Green)

        If isError Then
            ' Clear after 5 seconds for errors
            System.Web.UI.ScriptManager.RegisterStartupScript(Me, GetType(Page), "clearMessage",
                "setTimeout(function(){ document.getElementById('" & lblSuccessMsg.ClientID & "').innerHTML = ''; }, 5000);", True)
        End If
    End Sub

    Private Sub LogError(ex As Exception, context As String)
        ' Implement logging to file, database, or event log
        Dim logMessage As String = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - ERROR - {context} - {ex.Message} - {ex.StackTrace}"

        ' Example: Write to event log
        Try
            System.Diagnostics.EventLog.WriteEntry("MicroBanker", logMessage,
                System.Diagnostics.EventLogEntryType.Error)
        Catch
            ' If event log fails, at least write to trace
            System.Diagnostics.Trace.WriteLine(logMessage)
        End Try
    End Sub
End Class