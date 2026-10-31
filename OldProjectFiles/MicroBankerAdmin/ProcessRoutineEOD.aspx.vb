Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Imports System.Data.SqlClient
Imports System.Globalization

Public Class ProcessRoutineEOD
    Inherits System.Web.UI.Page
    Public P_TxnCode As String = "", P_TxnDesc As String = "", P_TxnType As String = "", P_ChargeAmt As String = "", P_ChqNo As String = "", P_TxnCash As String = "", P_PostMode As String = "", P_ContraGL As String = "", P_TelleID As String = "", P_AttachedTxnCode As String = "", P_RefCode As String = ""
    Public P_ProdRefCode As String = "", P_refNo As String = ""
    Public P_AccountDetailsHashMap As New Hashtable
    Dim EOD As Date, SOD As Date, EOM As Date, EOY As Date
    Dim GridRows As Integer

    'Dim ProcessStatus(GridRows) As Boolean
    Dim ProcessStatus As ArrayList = New ArrayList

    Dim arrParam() As SqlParameter
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If Page.IsPostBack = False Then
            EODDatePicker.Visible = False

            Post()
            GetProcessRoutineDates()

        End If
    End Sub

    Public Sub GetProcessRoutineDates()
        Try

            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select * from [dbo].[SYSTEM_DATES]"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                txtEOD.Text = Convert.ToDateTime(MyReader4("EOD")).ToString("dd/MM/yyyy")
                txtSOD.Text = Convert.ToDateTime(MyReader4("SOD")).ToString("dd/MM/yyyy")
                txtEOM.Text = Convert.ToDateTime(MyReader4("EOM")).ToString("dd/MM/yyyy")
                txtEOY.Text = Convert.ToDateTime(MyReader4("EOY")).ToString("dd/MM/yyyy")
            Else
                '  lblSuccessMsg.Text = "Could not Get Process Routines Dates"
            End If
            MyReader4.Close()
        Catch ex As Exception
        End Try
    End Sub


    Private Sub UpdateDates()
        'If ProcessRoutineDates() = True Then

        If txtDate.Text = "" Then
            txtDate.Text = ""
            lblSuccessMsgEOD.Text = "Enter Current Date"
            txtDate.Focus()
            Exit Sub
        Else
            Try

                'Dim s As String = txtEOD.Text.Trim
                Dim s As String = txtSOD.Text.Trim
                Dim pattern As String = "dd/MM/yyyy"
                Dim _Date As Date = Date.ParseExact(s, pattern, Nothing)
                Dim NewEOD As DateTime
                'NewEOD = EOD.AddDays(1)
                'NewEOD = _Date.AddDays(1)
                Dim MySqlaction1 As String
                Dim mySqlAction As String = ""
                Dim MyDBAction As New DataManagement
                MySqlaction1 = "update [dbo].[SYSTEM_DATES] set EOD ='" & _Date & "'"
                MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
                lblSuccessMsgEOD.Text = "End of Day Date Updated successfully"
                'Dim displayBal As String = ""
                'displayBal = Format(newAccBal, "###,###")
                'txtNewBalance.Text = newAccBal
            Catch ex As Exception
                'Label4.Text = "Error Inserting"
                lblSuccessMsgEOD.Text = "End of Day Date Change Failed"

            End Try
        End If

        'End If
    End Sub




    Function ProcessRoutineDates() As Boolean
        Dim Value As Boolean = False
        Try
            Dim pattern As String = "dd/MM/yyyy"

            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select * from [dbo].[SYSTEM_DATES]"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                EOD = Date.ParseExact(MyReader4("EOD").ToString("dd/MM/yyyy"), pattern, Nothing)
                SOD = Date.ParseExact(MyReader4("SOD").ToString("dd/MM/yyyy"), pattern, Nothing)
                EOM = Date.ParseExact(MyReader4("EOM").ToString("dd/MM/yyyy"), pattern, Nothing)
                EOY = Date.ParseExact(MyReader4("EOY").ToString("dd/MM/yyyy"), pattern, Nothing)
                Value = True
            Else
                'lblSuccessMsg.Text = "Could not Get Process Routines Dates"
                Value = False
            End If

            MyReader4.Close()
        Catch ex As Exception
            Value = False

        End Try

        Return Value

    End Function

    Protected Sub btnProcess_Click(sender As Object, e As EventArgs)

        If txtDate.Text = "" Then
            txtDate.Text = ""
            lblSuccessMsgEOD.Text = "Enter Current Date"
            txtDate.Focus()
            Exit Sub
        Else
            Dim lb As LinkButton = DirectCast(sender, LinkButton)
            Dim paramList As List(Of SqlParameter) = New List(Of SqlParameter)
            'paramList.Add(New SqlParameter("@EodDate", "30-oct-2021"))
            'Dim _Date As Date = Convert.ToDateTime(txtEOM.Text).ToString("dd/MM/yyyy")
            Dim Records As DataTable
            Dim s As String = txtDate.Text.Trim
            Dim pattern As String = "dd/MM/yyyy"
            Dim _Date As Date = Date.ParseExact(s, pattern, Nothing)

            paramList.Add(New SqlParameter("@EodDate", _Date))


            Dim b As LinkButton = CType(sender, LinkButton)
            Dim rows As GridViewRow = CType(b.NamingContainer, GridViewRow)

            Dim selectCheck As CheckBox = CType(rows.FindControl("checkSelect"), CheckBox)
            Dim ProcessName As String = CType(rows.FindControl("ProcessName"), Label).Text
            Dim ExecutionOrder As String = CType(rows.FindControl("ExecutionOrder"), Label).Text
            Dim ProcessFunction As String = CType(rows.FindControl("ProcessFunction"), Label).Text

            If selectCheck.Checked = True Then
                If (ExecutionOrder.Trim() = "1") Then
                    Records = GetReport(ProcessFunction, paramList)
                    If Records.Rows.Count > 0 Then
                        rows.FindControl("success").Visible = True
                        CType(rows.FindControl("lblStatus"), Label).Text = "Success"
                        ProcessStatus.Add(True)

                    Else
                        rows.FindControl("fail").Visible = True
                        ProcessStatus.Add(False)

                    End If
                ElseIf (ExecutionOrder.Trim() = "2") Then
                    Records = GetReport(ProcessFunction)
                    If Records.Rows.Count > 0 Then
                        rows.FindControl("success").Visible = True
                        CType(rows.FindControl("lblStatus"), Label).Text = "Success"
                        ProcessStatus.Add(True)


                    Else
                        rows.FindControl("fail").Visible = True
                        CType(rows.FindControl("lblStatus"), Label).Text = "Failed"
                        ProcessStatus.Add(False)

                    End If


                ElseIf (ExecutionOrder.Trim() = "3") Then
                    Dim Value As Boolean = GetReport(ProcessFunction, "1")
                    If Value = True Then
                        rows.FindControl("success").Visible = True
                        CType(rows.FindControl("lblStatus"), Label).Text = "Success"
                        ProcessStatus.Add(True)


                    Else
                        rows.FindControl("fail").Visible = True
                        CType(rows.FindControl("lblStatus"), Label).Text = "Failed"
                        ProcessStatus.Add(False)

                    End If
                End If
            End If
        End If




        'If Records.Rows.Count > 0 Then
        '    Me.lblSuccessMsgEOD.Text = "End of day: Successful"
        'End If
    End Sub

    Private Sub btChanageDates_Click(sender As Object, e As EventArgs) Handles btChanageDates.Click

        'If ProcessStatus.Contains(False) Or ProcessStatus.Count = 0 = True Then
        '    lblSuccessMsgEOD.Text = "One or More Process Routinte was not Successful"
        'Else
        UpdateDates()
        'End If
    End Sub

    Private Sub Post()

        Dim dtCurrentTable As DataTable = DirectCast(ViewState("CurrentTable"), DataTable)
        Dim dt As DataTable = New DataTable()

        ' Dim rowCount As Integer = dtCurrentTable.Rows.Count()
        'Dim rowCount As Integer = dtCurrentTable.Rows.Count()
        Dim ProcessGroup As String = "EOD"
        Dim MyDatObject As New DataManagement
        Dim mySearch4 As String = "select idx,ProcessID,ProcessName,ProcessFunction, ProcessGroup,ExecutionOrder from MB_ProcessRoutine where ProcessGroup = '" & ProcessGroup & "'"
        'Dim MyReader4 As SqlDataReader = MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataTable)
        Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataTable), SqlDataReader)

        Dim MyDatable As DataTable = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.DataTable), Data.DataTable)
        SODGridView.DataSource = MyDatable
        SODGridView.DataBind()

    End Sub



    Protected Sub btncalendars_Click(sender As Object, e As ImageClickEventArgs) Handles btncalendars.Click
        EODDatePicker.Visible = True

    End Sub

    Protected Sub EODDatePicker_SelectionChanged(sender As Object, e As EventArgs) Handles EODDatePicker.SelectionChanged
        Dim sysUIFormat As String = CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern
        EODDatePicker.Visible = False
        'txtDate.Text = EODDatePicker.SelectedDate.ToString(sysUIFormat)
        txtDate.Text = EODDatePicker.SelectedDate.ToString("dd/MM/yyyy")


    End Sub




    'Private Sub btnSod_Click(sender As Object, e As EventArgs) Handles btnSod.Click
    '    'lblDate.Text = Date.Now.ToString("yyyy/MM/dd")
    '    'Dim dateToday As Date = Date.Now.ToString("yyyy/MM/dd")
    '    'If lblDate.Text <> dateToday Then
    '    'lblSuccessMsg.Text = "Start of day process Date is not correct"
    '    'Else
    '    PostTransactions()
    '    UpdateBackSOD_Status()
    '    'End If

    '    lblSuccessMsg.Text = "Start of Day Process Completed Successfully"
    'End Sub
    Private Sub UpdateBackSOD_Status()
        Dim dateToday, nextRepaymentDatePost As DateTime
        dateToday = DateTime.Now.ToString("yyyy/MM/dd")
        nextRepaymentDatePost = dateToday.AddMonths(1)
        Dim mySqlAction1 As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction1 = "update tbAccount set SOD_Status = 0, NextRepaymentDatePost = '" & nextRepaymentDatePost & "'"
        MyDBAction.DBAction(mySqlAction1, DataManagement.DBActionType.Update)
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
                    Dim mySearch4 As String = "select AccountNo,AccountName,BranchCode,SettlementsAccountNo,RepaymentAmount,AssetGL_Control,LiabilityGL_Control,ProductDefCode,AvailBal,BookBalance from tbAccount where SOD_Status = 0 and ProductDefCode = 'LA'"
                    Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
                    If MyReader4.HasRows = True Then
                        MyReader4.Read()
                        Dim TransCode As String = ""
                        Dim liabilityGL As String = ""
                        Dim loanAccGL As String = ""
                        Dim settlementAccGL As String = ""
                        Dim loanTransType As String = "C"
                        Dim settlementTransType As String = "D"
                        Dim loanTransCode As String = "C90"
                        Dim settlementTransCode As String = "C90"
                        Dim loanTransDescription As String = "REPAYMENT POSTINGS"
                        Dim settlementTransDescription As String = "REPAYMENT POSTINGS"
                        Dim repaymentAmount As Decimal = 0
                        Dim repaymentAmountGlobal As Decimal = 0
                        Dim loanAccountBal As Decimal = 0.00
                        Dim loanAccName As String = ""
                        Dim settleAccName As String = ""
                        Dim loanAccBranch As String = ""
                        Dim settleAccBranch As String = ""
                        Dim _module As String = "77"
                        Dim moduler As String = "PR"
                        Dim TxnCode As String = "88"
                        Dim settlementAccountBal As Decimal = 0.00
                        Dim loanAccNo As String = ""
                        Dim settlementAccNo As String = ""
                        Dim loanRefNo As String = ""
                        Dim settlementRefNo As String = ""
                        Dim seqNo As String = ""
                        Dim loanRowNum As String = "1"
                        Dim settlementRowNum As String = "2"
                        Dim productDefCode As String = ""
                        Dim _day As String = ""
                        Dim _month As String = ""
                        Dim _year As String = ""
                        Dim TxnDate As DateTime = Date.Now()
                        Dim actionDate As DateTime = Date.Now()
                        _day = (TxnDate.Day).ToString().PadLeft(2, "0")
                        _month = (TxnDate.Month).ToString().PadLeft(2, "0")
                        _year = (TxnDate.Year).ToString().Substring(2, 2)
                        ' P_RefCode = MyReader4("Journal").ToString
                        ' P_AccountNo = MyReader4("AccountNo").ToString
                        UtilitiesModule.GetNextSeqNo("LAST_SEQNO")
                        seqNo = UtilitiesModule.returnValue
                        'TxnReference = _module & (TxnCode).ToUpper() & seqNo & rowNum & _day & _month & _year

                        loanAccNo = MyReader4("AccountNo").ToString
                        productDefCode = MyReader4("ProductDefCode").ToString
                        loanAccGL = MyReader4("AssetGL_Control").ToString
                        repaymentAmount = MyReader4("RepaymentAmount").ToString
                        settlementAccNo = MyReader4("SettlementsAccountNo").ToString
                        loanAccountBal = MyReader4("AvailBal").ToString
                        loanAccountBal = MyReader4("AvailBal").ToString
                        loanAccountBal = MyReader4("AvailBal").ToString
                        loanAccName = MyReader4("AccountName").ToString
                        loanAccBranch = MyReader4("BranchCode").ToString
                        'settlementAccountBal = MyReader4("SettlementsAccountNo").ToString
                        'insert loan account details
                        loanRefNo = _module & (TxnCode).ToUpper() & seqNo & loanRowNum & _day & _month & _year
                        InsertGridRecords(loanAccNo, repaymentAmount, loanTransType, loanAccGL, loanRefNo, seqNo, loanTransDescription, moduler, loanRowNum, loanAccName, loanAccBranch)
                        PostRecord(loanAccNo)
                        UpdateAccountBalances(loanAccNo, repaymentAmount, loanTransType, loanAccountBal)
                        UpdateCustomerGL(loanAccGL, repaymentAmount, loanTransType, loanAccNo)
                        'get settlement account GL and balances
                        GetAccountDetails(settlementAccNo)
                        settlementAccountBal = P_AccountDetailsHashMap("AvailBal")
                        settlementAccGL = P_AccountDetailsHashMap("LiabilityGLControl")
                        settleAccName = P_AccountDetailsHashMap("AccountName")
                        settleAccBranch = P_AccountDetailsHashMap("BranchCode")
                        'insert settlement account details
                        settlementRefNo = _module & (TxnCode).ToUpper() & seqNo & loanRowNum & _day & _month & _year
                        InsertGridRecords(settlementAccNo, repaymentAmount, settlementTransType, settlementAccGL, settlementRefNo, seqNo, settlementTransDescription, moduler, settlementRowNum, settleAccName, settleAccBranch)
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
            'lblSuccessMsg.Text = ex.ToString
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
            ' lblSuccessMsg.Text = ex.ToString
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
                    '   lblSuccessMsg.Text = "Invalid transaction code"
                    'lblDescription.Text = ""
                    Exit Sub
                End If
            Else
                '  lblSuccessMsg.Text = "Transaction Code does not exist"
                Exit Sub
            End If
            MyReader4.Close()
        Catch ex As Exception
            'Label4.Text = "Error Pulling Account Details"
        End Try
    End Sub
    Private Sub InsertGridRecords(accsNo, amounts, transType, accountGL, P_refNo, seqNo, transDesc, moduler, rownum, accName, accBranch)
        Dim mySqlAction As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction = "Insert into MB_RetailPosting_Temp values('" & rownum & "','" & seqNo & "','" & accountGL & "','', " &
                                        "'" & accsNo & "','" & accName & "','" & transType & "', " &
                    "'" & accBranch & "','', " &
                    " '" & "GMD" & "','" & amounts & "', " &
                    "'','" & amounts & "', " &
                    "'" & transDesc & "','" & "00" & "',''," &
                    "'" & moduler & "','" & P_refNo & "', " &
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

    'Private Sub btnEOD_Click(sender As Object, e As EventArgs) Handles btnEOD.Click
    '    Dim paramList As List(Of SqlParameter) = New List(Of SqlParameter)
    '    'paramList.Add(New SqlParameter("@EodDate", "30-oct-2021"))
    '    paramList.Add(New SqlParameter("@EodDate", txtDate.Text))
    '    Dim Records As DataTable

    '    For Each row As GridViewRow In SODGridView.Rows

    '        ' Dim cell As GridViewCheckBoxCell = TryCast(row.Cells(2), DataGridViewCheckBoxCell)
    '        Dim selectCheck As CheckBox = CType(row.FindControl("checkSelect"), CheckBox)
    '        Dim ProcessName As String = CType(row.FindControl("ProcessName"), Label).Text
    '        Dim ExecutionOrder As String = CType(row.FindControl("ExecutionOrder"), Label).Text
    '        Dim ProcessFunction As String = CType(row.FindControl("ProcessFunction"), Label).Text


    '        ' Dim Object As Object = ProcessFunction.Trim()


    '        'If selectCheck.Checked = True Then

    '        If (ExecutionOrder.Trim() = "1") Then
    '            Records = GetReport(ProcessFunction, paramList)
    '        ElseIf (ExecutionOrder.Trim() = "2") Then
    '            Records = GetReport(ProcessFunction)
    '        End If

    '        If Records.Rows.Count > 0 Then
    '            row.FindControl("success").Visible = True
    '        Else
    '            row.FindControl("fail").Visible = True
    '        End If
    '        'End If
    '    Next


    '    If Records.Rows.Count > 0 Then
    '        Me.lblSuccessMsgEOD.Text = "End of day: Successful"
    '    End If
    '    'Message success/false [dbo].[EOD]'27-sep-2021'
    'End Sub

    Public Function GetReport(ByVal storedProcedureName _
                As String,
                ByVal paramList As List(Of SqlParameter)) As DataTable
        Dim dt As DataTable
        Try
            Using cnn As New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ConnectionString)
                cnn.Open()

                ' Define the command 
                Using cmd As New SqlCommand
                    cmd.Connection = cnn
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.CommandText = storedProcedureName

                    ' Handle the parameters 
                    If paramList IsNot Nothing Then
                        For Each param As SqlParameter In paramList
                            cmd.Parameters.Add(param)
                        Next
                    End If

                    ' Define the data adapter and fill the dataset

                    Using da As New SqlDataAdapter(cmd)
                        dt = New DataTable
                        da.Fill(dt)

                    End Using
                End Using
            End Using
        Catch ex As Exception
            dt.Clear()
            Return dt
            Throw New Exception("ERROR EXECUTING ExecuteDataTable", ex)
        End Try
        Return dt
    End Function


    Public Function GetReport(ByVal storedProcedureName _
                As String) As DataTable
        Dim dt As DataTable
        Try
            Using cnn As New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ConnectionString)
                cnn.Open()

                ' Define the command 
                Using cmd As New SqlCommand
                    cmd.Connection = cnn
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.CommandText = storedProcedureName



                    ' Define the data adapter and fill the dataset

                    Using da As New SqlDataAdapter(cmd)
                        dt = New DataTable
                        da.Fill(dt)

                    End Using
                End Using
            End Using
        Catch ex As Exception
            dt.Clear()
            Return dt
            Throw New Exception("ERROR EXECUTING ExecuteDataTable", ex)
        End Try
        Return dt
    End Function



    Public Function GetReport(ByVal storedProcedureName _
                As String, ByVal Type As String) As Boolean
        Dim dt As DataTable
        Dim Value As Boolean = False
        Try
            Using cnn As New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ConnectionString)
                cnn.Open()

                ' Define the command 
                Using cmd As New SqlCommand
                    cmd.Connection = cnn
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.CommandText = storedProcedureName



                    ' Define the data adapter and fill the dataset

                    Using da As New SqlDataAdapter(cmd)
                        dt = New DataTable
                        da.Fill(dt)
                        Value = True
                        Return Value
                    End Using
                End Using
            End Using
        Catch ex As Exception
            dt.Clear()
            Throw New Exception("ERROR EXECUTING ExecuteDataTable", ex)
        End Try
        Return Value
    End Function








End Class