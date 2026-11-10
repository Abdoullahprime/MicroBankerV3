Imports System.Data.SqlClient
Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin

Public Class UnBlockAmount
    Inherits System.Web.UI.Page
    Public BlockedAmount As Decimal = 0.00
    Public AvailBalance As Decimal = 0.00
    Public BkBalance As Decimal = 0.00
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        DisplayDetails("All")
    End Sub

    Sub DisplayLoan()

    End Sub

    Protected Sub RdoSearch_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
    End Sub
    Protected Sub txtSearch_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs)
    End Sub
    Protected Sub CmdSearch_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        DisplayDetails("Filter")
    End Sub
    Protected Sub CmdViewAll_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        DisplayDetails("All")
    End Sub
    Protected Sub GridView1_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
    End Sub
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
                    mySqlAction = "select * from MB_BlockAmount where BlockStatus='0' "
                Case "Filter"
                    If RdoSearch.Items(0).Selected = True Then
                        Processfilter = txtSearch.Text.ToUpper
                        Processfilter = "%" & Processfilter & "%"
                        mySqlAction = "select  * from MB_BlockAmount where  AccountName LIKE '" & Processfilter & "' and BlockStatus='0'"
                    End If
                    If RdoSearch.Items(1).Selected = True Then
                        Processfilter = txtSearch.Text.ToUpper
                        Processfilter = "%" & Processfilter & "%"
                        mySqlAction = "select * from MB_BlockAmount  where AccountNo LIKE '" & Processfilter & "' and BlockStatus='0'"
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
    ' Private Sub btnCheckAccNo_Click(sender As Object, e As EventArgs) Handles btnCheckAccNo.Click
    'If lblAccountName.Text = "" Then
    '    lblSuccessMsg.Text = "Please Check Transaction Code"
    '    Exit Sub
    'Else
    ' GetAccountDetails(Me.txtAccountNo.Text.Trim)
    'UpdateCustomerBalances(Me.txtAccountNo.Text.Trim(), txtBlockAmount.Text.Trim(), BookBal.Text.Trim())
    'End If

    'End Sub

    Private Sub ShowMatchDetails(ByVal mySearch As String, ByVal ShowType As String)
        Dim MySmsObject As New DataManagement
        Dim MyTransProcessed As Boolean = False
        Dim MyReader As SqlDataReader = TryCast(MySmsObject.DBAction(mySearch, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
        Try
            Select Case ShowType
                Case "ViewDetails"
                    'Load data from gridview 
                    If MyReader.HasRows Then
                        UnblockTbl.Visible = True
                        GridView1.Visible = False
                        searchRow.Visible = False
                        RadioButtControls.Visible = False
                        MyReader.Read()
                        txtAccountNo.Text = MyReader("AccountNo").ToString
                        GetAccountDetails(txtAccountNo.Text.Trim)
                        lblAccountName.Text = MyReader("AccountName").ToString
                        AailBalance.Text = AvailBalance
                        'AailBalance.Text = MyReader("AvailableBal").ToString
                        BookBal.Text = BkBalance
                        txtBlockAmount.Text = MyReader("BlockAmount").ToString

                    End If
            End Select
        Catch ex As Exception
        Finally
            If Not IsNothing(MyReader) Then
                MyReader.Close()
            End If
        End Try
    End Sub
    Protected Sub GridView1_RowCommand(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewCommandEventArgs) Handles GridView1.RowCommand
        Try
            'Gridview data selection
            HiddenField1.Value = ""
            Dim MyID As String = e.CommandArgument
            HiddenField1.Value = MyID
            Select Case e.CommandName
                Case "edit"
                    'txtOrgCode.Enabled = False
                    'txtCurrency.Enabled = False
                    ShowMatchDetails("Select   * From MB_BlockAmount where id= '" & HiddenField1.Value.ToString().Trim() & "' ", "ViewDetails")
                    'TabContainer1.ActiveTabIndex = 0
                    DisplayDetails("All")
                Case "select"
                    'txtOrgCode.Enabled = True
                    'txtCurrency.Enabled = True
                    ShowMatchDetails("Select  * From MB_BlockAmount where id= '" & HiddenField1.Value.ToString & "' ", "ViewDetails")
                    TabContainer1.ActiveTabIndex = 0
                    DisplayDetails("All")
            End Select
        Catch ex As Exception
            Dim ErrMsg As String = ex.Message
        End Try
    End Sub
    Protected Sub GridView1_RowEditing(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewEditEventArgs) Handles GridView1.RowEditing
    End Sub
    Public Sub GetAccountDetails(AccountNo)
        Try
            lblSuccessMsg.Text = ""
            lblAccountName.Text = ""
            ' txtAmount.Text = ""
            'If txtAccountNo.Text = "" Then
            '    txtAccountNo.Focus()
            '    'lblSuccessMsg.Text = " Please enter Account Number"
            '    Exit Sub
            'End If

            'If txtAccountNo.Text.ToString().Length < 17 Or txtAccountNo.Text.ToString().Length > 17 Then
            '    txtAccountNo.Focus()
            '    lblSuccessMsg.Text = "A/c Number does not exist or not a saving a/c or a/c not approved"
            '    Exit Sub
            'End If
            'If Not txtAccountNo.Text.ToString().Substring(11, 3) = "201" Then
            '    txtAccountNo.Text = ""
            '    txtAccountNo.Focus()
            '    lblSuccessMsg.Text = "A/c Number does not exist or not a saving a/c or a/c not approved"
            '    Exit Sub
            'End If
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select AccountNo,CustomerNo,AccountName,AssetGL_Control,LiabilityGL_Control,BranchCode,AvailBal,BookBalance,ProductDefCode,BlockAmount from [dbo].[tbAccount] where AccountNo = '" & AccountNo & "'" ' ='" & Me.HiddenField1.Value & "' "
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
                'AailBalance.Text = MyReader4("AvailBal").ToString
                AvailBalance = MyReader4("AvailBal").ToString
                BkBalance = MyReader4("BookBalance").ToString

                BlockedAmount = MyReader4("BlockAmount").ToString
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
    Sub updateMB_Block()
        Dim newAccBal As String = ""
        Dim newUnblockAmount As String = ""
        Dim newblockAmount As String = ""
        newAccBal = Double.Parse(AailBalance.Text) + Val(Amount)
        newblockAmount = BlockedAmount - Val(Amount)

        Dim MySqlaction1 As String
        Dim mySqlAction As String = ""
        Dim mySqlAction2 As String
        Dim MyDBAction As New DataManagement
        mySqlAction2 = "update MB_BlockAmount set BlockStatus ='1', ReasonToUnblock='" & txtBlockReason.Text & "', UnblockDate='" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "' where AccountNo= '" & txtAccountNo.Text.ToString().Trim() & "' and id = '" & HiddenField1.Value.ToString().Trim() & "'"
        MyDBAction.DBAction(mySqlAction2, DataManagement.DBActionType.Update)

    End Sub
    Protected Sub UpdateCustomerBalances(accountNo, amount, P_BookBalance)
        Try

            'If txtBlockAmount.Text.ToString() = "" Or txtBlockReason.Text.ToString() = "" Then
            '    lblSuccessMsg.Text = "Please Enter All Fields"
            '    txtBlockAmount.Focus()
            '    Exit Sub
            'End If
            'If Double.Parse(txtBlockAmount.Text.ToString()) <= 0 Then
            '    lblSuccessMsg.Text = "UnBlock Amount Should Be More Than 0"
            '    txtBlockAmount.Focus()
            '    Exit Sub
            'ElseIf Double.Parse(txtBlockAmount.Text) > Double.Parse(P_BlockAmount) Then
            '    lblSuccessMsg.Text = "UnBlock Amount Should Not Be More Than The Blocked Amount"
            '    txtBlockAmount.Focus()
            '    Exit Sub
            '    ' txtBlockAmount.Focus();
            '    'ElseIf txtBlockAmount.Text = "" Or txtBlockReason.Text = "" Then
            '    '    lblSuccessMsg.Text = "Please Enter All Required Fields"
            'Else
            GetAccountDetails(txtAccountNo.Text.Trim)
                Dim newAccBal As String = ""
                Dim newUnblockAmount As String = ""
                Dim newblockAmount As String = ""
                newAccBal = AvailBalance + Val(amount)
                newblockAmount = BlockedAmount - Val(amount)

                Dim MySqlaction1 As String
                Dim mySqlAction As String = ""
                Dim mySqlAction2 As String
                Dim MyCurrencyName As String = ""
                Dim MyDBAction As New DataManagement
                MySqlaction1 = "update tbAccount set AvailBal ='" & newAccBal & "', BlockAmount='" & newblockAmount & "', LastActivityDate = '" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "',LastCRactvtyDate ='" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "',LastCRDate = '" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "' where AccountNo ='" & accountNo & "'"
                MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
                lblAccountName.Text = ""
                txtAccountNo.Text = ""
                txtBlockAmount.Text = ""
                txtBlockReason.Text = ""
                BookBal.Text = ""
            AailBalance.Text = ""
            lblSuccessMsg.Text = "Amount unblock have been successfully"
            'lblAccountName.Text = "Unblock Successsfully"
            'End If
            'Dim displayBal As String = ""
            'displayBal = Format(newAccBal, "###,###")
            'txtNewBalance.Text = newAccBal
        Catch ex As Exception
            'Label4.Text = "Error Inserting"
        End Try
    End Sub
    Protected Sub InsertBlockedAmount()
        'If BookBal.Text < txtBlockAmount.Text Then
        Try
            Dim mySqlAction As String = ""
            Dim MyDBAction As New DataManagement
            mySqlAction = "Insert into MB_BlockAmount (AccountNo,AccountName,BookBal,AvailableBal,BlockAmount,ReasonToBlock,BlockDate)values('" & txtAccountNo.Text.Trim & "','" & lblAccountName.Text.ToUpper.Trim & "','" & BookBal.Text.ToUpper.Trim & "','" & AailBalance.Text.ToUpper.Trim & "','" & txtBlockAmount.Text.ToUpper.Trim & "','" & txtBlockReason.Text.ToUpper.Trim & "','" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "' )" ','" & txtBranchName.Text.ToUpper & "','" & txtAdress2.Text.ToUpper & "','" & txtmobile.Text & "') "
            MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
        Catch ex As Exception
            'Label4.Text = "Error Inserting"
        End Try
        lblSuccessMsg.Text = "Amount unblock have been successfully"
        'Else
        '    lblSuccessMsg.Text = "Amount block is more than book Balance"
        'End If
    End Sub

    Protected Sub btnBlock_Click(sender As Object, e As EventArgs) Handles btnBlock.Click
        If txtBlockAmount.Text.ToString() = "" Or txtBlockReason.Text.ToString() = "" Then
            lblSuccessMsg.Text = "Please Enter All Fields"
            txtBlockAmount.Focus()
            Exit Sub
        End If
        'InsertBlockedAmount()
        updateMB_Block()
        UpdateCustomerBalances(Me.txtAccountNo.Text.Trim(), txtBlockAmount.Text.Trim(), AvailBalance)
        'lblAccountName.Text = ""
        'txtAccountNo.Text = ""
        txtBlockAmount.Text = ""
        'txtBlockReason.Text = ""
        'BookBal.Text = ""
        'AailBalance.Text = ""

    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class