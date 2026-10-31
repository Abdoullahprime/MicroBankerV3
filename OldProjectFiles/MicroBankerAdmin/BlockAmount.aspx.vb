Imports System.Data.SqlClient
Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin

Public Class BlockAmount
    Inherits System.Web.UI.Page
    Public BlockedAmount As Decimal = 0.00

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Private Sub btnCheckAccNo_Click(sender As Object, e As EventArgs) Handles btnCheckAccNo.Click
        'If lblAccountName.Text = "" Then
        '    lblSuccessMsg.Text = "Please Check Transaction Code"
        '    Exit Sub
        'Else
        GetAccountDetails(Me.txtAccountNo.Text.Trim)
        'UpdateCustomerBalances(Me.txtAccountNo.Text.Trim(), txtBlockAmount.Text.Trim(), BookBal.Text.Trim())
        'End If

    End Sub

    Public Sub GetAccountDetails(AccountNo)
        Try
            lblSuccessMsg.Text = ""
            lblAccountName.Text = ""
            ' txtAmount.Text = ""
            If txtAccountNo.Text = "" Then
                txtAccountNo.Focus()
                'lblSuccessMsg.Text = " Please enter Account Number"
                Exit Sub
            End If
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select AccountNo,CustomerNo,AccountName,AssetGL_Control,LiabilityGL_Control,BranchCode,AvailBal,BookBalance,ProductDefCode,BlockAmount from [dbo].[tbAccount] where AccountNo = '" & AccountNo & "' and ProductDefCode = 'SA' and Approved = 1" ' ='" & Me.HiddenField1.Value & "' "
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
                AailBalance.Text = MyReader4("AvailBal").ToString
                BookBal.Text = MyReader4("BookBalance").ToString
                BlockedAmount = MyReader4("BlockAmount").ToString
                P_DefinationCode = MyReader4("ProductDefCode").ToString().Trim()
            Else
                lblSuccessMsg.Text = "A/c Number does not exist or not a saving a/c or a/c not approved"
            End If
        Catch ex As Exception
            'txtOrgName.Text = ""
            'txtAdress1.Text = ""
            '' DropDownList1.Text = ""
            'txtOrgCode.ReadOnly = False
            'Label4.Text = "Error Pulling Account Details"
        End Try
    End Sub
    Protected Sub UpdateCustomerBalances(accountNo, amount, P_BookBalance)
        Try
            'If txtAccountNo.Text = "" Then
            '    txtAccountNo.Focus()
            '    lblSuccessMsg.Text = " Please enter Account Number"
            '    Exit Sub
            'End If
            'If txtAccountNo.Text.ToString().Length < 17 Or txtAccountNo.Text.ToString().Length > 17 Then
            '    txtAccountNo.Focus()
            '    lblSuccessMsg.Text = " This Is Not An Account No"
            '    Exit Sub
            'End If
            'If Not txtAccountNo.Text.ToString().Substring(11, 3) = "201" Then
            '    txtAccountNo.Text = ""
            '    txtAccountNo.Focus()
            '    lblSuccessMsg.Text = " This is not a savings Account"
            '    Exit Sub
            'End If

            If Double.Parse(txtBlockAmount.Text) < 0 Then
                lblSuccessMsg.Text = "Block Amount Should Not Be Less Than 0"
                txtBlockAmount.Focus()
                Exit Sub
            ElseIf Double.Parse(txtBlockAmount.Text) > Double.Parse(AailBalance.Text) Then
                lblSuccessMsg.Text = "Block Amount Should Not Be More Than The Available Balance"
                txtBlockAmount.Focus()
                Exit Sub
            Else
                Dim newAccBal As String = ""
                Dim NewBlockAmount As String = ""
                newAccBal = Double.Parse(AailBalance.Text) - Val(amount)
                NewBlockAmount = BlockedAmount + Val(amount)
                Dim MySqlaction1 As String
                Dim mySqlAction As String = ""
                Dim MyCurrencyName As String = ""
                Dim MyDBAction As New DataManagement
                MySqlaction1 = "update tbAccount set AvailBal ='" & newAccBal & "', BlockAmount='" & NewBlockAmount & "', LastActivityDate = '" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "',LastCRactvtyDate ='" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "',LastCRDate = '" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "' where AccountNo ='" & accountNo & "'"
                MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
                lblSuccessMsg.Text = "Amount block successfully"
            End If
            'Dim displayBal As String = ""
            'displayBal = Format(newAccBal, "###,###")
            'txtNewBalance.Text = newAccBal
        Catch ex As Exception
            'Label4.Text = "Error Inserting"
        End Try
        Exit Sub
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
        lblSuccessMsg.Text = "Amount block have been successfully"
        'Else
        '    lblSuccessMsg.Text = "Amount block is more than book Balance"
        'End If
    End Sub

    Protected Sub btnBlock_Click(sender As Object, e As EventArgs) Handles btnBlock.Click
        UpdateCustomerBalances(Me.txtAccountNo.Text.Trim(), txtBlockAmount.Text.Trim(), BookBal.Text.Trim())
        If txtBlockAmount.Text.ToString() = "" Or txtBlockReason.Text.ToString() = "" Then
            lblSuccessMsg.Text = "Please Enter All Fields"
            txtBlockAmount.Focus()
            Exit Sub
        End If
        InsertBlockedAmount()
        lblAccountName.Text = ""
        txtAccountNo.Text = ""
        txtBlockAmount.Text = ""
        txtBlockReason.Text = ""
        BookBal.Text = ""
        AailBalance.Text = ""

    End Sub
End Class