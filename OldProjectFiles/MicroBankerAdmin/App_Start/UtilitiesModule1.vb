Imports System.Data.SqlClient
Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Module UtilitiesModule
    Public P_CustomerNo, P_AccountName, P_AssetGL_Control, GLAccount, GLName As String
    Public P_LiabilityGL_Control, P_BranchCode, P_AvailBal, P_BookBalance, P_DefinationCode, P_ProductDefCode As String
    Public TxnType, TxnCode, PostMode, ContraGL, Contra_GL, TelleID, RefCode, returnValue As String
    Public Amount As Double
    Public gl_ACC_NO, gl_ACC_TYPE, gl_BAL_TYPE, gl_ACC_NAME, gl_ACC_DESC, gl_SUB1 As String
    Public P_AccountDetailsHashMap, P_TransactionCodeDetails, P_AccountGLDetails, P_TellerGLDetails As New Hashtable
    Public Sub _GetAccountDetails(ByVal accountNo As String)
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select AccountNo,CustomerNo,AccountName,AssetGL_Control,LiabilityGL_Control,BranchCode,AvailBal,BookBalance,ProductDefCode from [dbo].[tbAccount] where AccountNo = '" & accountNo.Trim & "'"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                P_CustomerNo = MyReader4("CustomerNo").ToString
                P_AccountName = MyReader4("AccountName").ToString
                P_AssetGL_Control = MyReader4("AssetGL_Control").ToString
                P_LiabilityGL_Control = MyReader4("LiabilityGL_Control").ToString
                P_BranchCode = MyReader4("BranchCode").ToString
                P_AvailBal = MyReader4("AvailBal").ToString
                P_BookBalance = MyReader4("BookBalance").ToString
                P_DefinationCode = MyReader4("ProductDefCode").ToString().Trim()
            End If
            MyReader4.Close()
        Catch ex As Exception
            Throw New Exception("Error Loading Account Details", ex)
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
            Throw New Exception("Error Loading Account Details", ex)
        End Try
        'Next
        Return P_AccountDetailsHashMap
    End Function
    Public Function GetTransactionCodes(txnCode) As Hashtable
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select TxnCode,TxnDesc,TxnType,ChargeAmt,ChqNo,TxnCash,PostMode,ContraGL,TelleID,AttachedTxnCode,RefCode from tbTransactionCode where TxnCode = '" & txnCode & "'"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                P_TransactionCodeDetails.Add("TransactionType", MyReader4("TxnType").ToString)
                P_TransactionCodeDetails.Add("TransactionCode", MyReader4("TxnCode").ToString)
                P_TransactionCodeDetails.Add("PostMode", MyReader4("PostMode").ToString)
                P_TransactionCodeDetails.Add("ContraGL", MyReader4("ContraGL").ToString)
                P_TransactionCodeDetails.Add("TelleID", MyReader4("TelleID").ToString)
                P_TransactionCodeDetails.Add("RefCode", MyReader4("RefCode").ToString)
            End If
            MyReader4.Close()
        Catch ex As Exception
            Throw New Exception("Error Loading Account Details", ex)
        End Try
        'Next
        Return P_TransactionCodeDetails
    End Function
    Public Function GetAccountGLDetails(glAccount) As Hashtable
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "SELECT GLAccount,GLName,Amount FROM [dbo].tbCashGL where GLAccount = '" & glAccount.Trim() & "'"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                P_AccountGLDetails.Add("GLs_AccountNo", MyReader4("GLAccount").ToString)
                P_AccountGLDetails.Add("GLs_AccountName", MyReader4("GLName").ToString)
                P_AccountGLDetails.Add("Amounts", MyReader4("Amount").ToString)
            End If
            MyReader4.Close()
        Catch ex As Exception
            Throw New Exception("Error Loading Account Details", ex)
        End Try
        'Next
        Return P_AccountGLDetails
    End Function
    Public Function GetTellerGLDetails(glAccount) As Hashtable
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "SELECT GLAccount,GLName,Amount FROM [dbo].tbCashGL where GLAccount = '" & glAccount.Trim() & "'"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                P_AccountGLDetails.Add("GL_AccountNo", MyReader4("GLAccount").ToString)
                P_AccountGLDetails.Add("GL_AccountName", MyReader4("GLName").ToString)
                P_AccountGLDetails.Add("Amount", MyReader4("Amount").ToString)
            End If
            MyReader4.Close()
        Catch ex As Exception
            Throw New Exception("Error Loading Account Details", ex)
        End Try
        'Next
        Return P_TellerGLDetails
    End Function

    Public Sub GetTransactionCodeDetails(ByVal txnCode As String)
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select TxnCode,TxnDesc,TxnType,ChargeAmt,ChqNo,TxnCash,PostMode,ContraGL,TelleID,AttachedTxnCode,RefCode from tbTransactionCode where TxnCode = '" & txnCode & "'"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                TxnType = MyReader4("TxnType").ToString
                txnCode = MyReader4("TxnCode").ToString
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

    Public Sub GetGlAccoutNames(ByVal accountNo As String)
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "SELECT GLAccount,GLName,Amount FROM [dbo].tbCashGL where GLAccount = '" & accountNo.Trim() & "'"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                GLAccount = MyReader4("GLAccount").ToString
                GLName = MyReader4("GLName").ToString
                Amount = CDbl(MyReader4("Amount").ToString)
            End If
            MyReader4.Close()
        Catch ex As Exception
            Throw New Exception("Error GetGlAccoutNames ", ex)
        End Try
    End Sub
    'ConnectionString
    Public Sub GetNextSeqNo(ByVal SeqName As String)
        Using sql As New SqlConnection(ConfigurationManager.ConnectionStrings("ConnectionString").ConnectionString)
            sql.Open()
            Using cmd As New SqlClient.SqlCommand("usp_GetNewSeqVal", sql)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.AddWithValue("@SeqName", SeqName.Trim())
                Dim RetVal As SqlParameter = cmd.Parameters.Add("@siReturnValue", SqlDbType.Int)
                RetVal.Direction = ParameterDirection.ReturnValue
                cmd.ExecuteNonQuery()
                sql.Close()
                returnValue = RetVal.Value.ToString
            End Using
        End Using
    End Sub

    Public Sub getSub1_Sub2AcctNo(headType As Integer, Head As Integer, Flag As Integer, Optional ByVal sub1 As Object = Nothing)
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = ""
            If Flag = 1 Then
                ' mySearch4 = "select AccountNo,CustomerNo,AccountName,AssetGL_Control,LiabilityGL_Control,BranchCode,AvailBal,BookBalance,ProductDefCode from [dbo].[tbAccount] where AccountNo = '" & accountNo.Trim & "'"
            ElseIf Flag = 2 Then
                ' mySearch4 = "select AccountNo,CustomerNo,AccountName,AssetGL_Control,LiabilityGL_Control,BranchCode,AvailBal,BookBalance,ProductDefCode from [dbo].[tbAccount] where AccountNo = '" & accountNo.Trim & "'"
            End If
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                P_CustomerNo = MyReader4("CustomerNo").ToString
                P_AccountName = MyReader4("AccountName").ToString
                P_AssetGL_Control = MyReader4("AssetGL_Control").ToString
                P_LiabilityGL_Control = MyReader4("LiabilityGL_Control").ToString
                P_BranchCode = MyReader4("BranchCode").ToString
                P_AvailBal = MyReader4("AvailBal").ToString
                P_BookBalance = MyReader4("BookBalance").ToString
                P_DefinationCode = MyReader4("ProductDefCode").ToString().Trim()
            End If
            MyReader4.Close()
        Catch ex As Exception
            Throw New Exception("Error Loading Account Details", ex)
        End Try
    End Sub

    Public Sub DoesGL_HeadExist(accountNo As String, headType As String)
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "SELECT GLAccount,GLName,Amount FROM [dbo].tbCashGL where GLAccount = '" & accountNo.Trim() & "'"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                GLAccount = MyReader4("GLAccount").ToString
                GLName = MyReader4("GLName").ToString
                Amount = CDbl(MyReader4("Amount").ToString)
            End If
            MyReader4.Close()
        Catch ex As Exception
            Throw New Exception("Error GetGlAccoutNames ", ex)
        End Try
    End Sub
    Public Sub DoesSubHead1Exist(accountNo As String, headType As String, Head As String)
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "SELECT GLAccount,GLName,Amount FROM [dbo].tbCashGL where GLAccount = '" & accountNo.Trim() & "'"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                GLAccount = MyReader4("GLAccount").ToString
                GLName = MyReader4("GLName").ToString
                Amount = CDbl(MyReader4("Amount").ToString)
            End If
            MyReader4.Close()
        Catch ex As Exception
            Throw New Exception("Error GetGlAccoutNames ", ex)
        End Try
    End Sub

    Public Sub DoesSubHead2Exist(accountNo As String, headType As String, Head As String)
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "SELECT GLAccount,GLName,Amount FROM [dbo].tbCashGL where GLAccount = '" & accountNo.Trim() & "'"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                GLAccount = MyReader4("GLAccount").ToString
                GLName = MyReader4("GLName").ToString
                Amount = CDbl(MyReader4("Amount").ToString)
            End If
            MyReader4.Close()
        Catch ex As Exception
            Throw New Exception("Error GetGlAccoutNames ", ex)
        End Try
    End Sub

    Public Sub GetGL_COA_DETAILS(ByVal head As String, ByVal sub1 As String, ByVal acctNo As String)
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = ""
            mySearch4 = "Select ACC_NO,ACC_TYPE, BAL_TYPE, HEAD_TYPE, ACC_NAME, ACC_DESC,HEAD, SUB1 " &
            "FROM ChartOfAccounts WHERE " &
                       "HEAD='" & head & "' AND SUB1='" & sub1 & "' AND ACC_NO='" & acctNo & "'"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                gl_ACC_NO = MyReader4("ACC_NO").ToString
                gl_ACC_TYPE = MyReader4("ACC_TYPE").ToString
                gl_BAL_TYPE = MyReader4("BAL_TYPE").ToString
                gl_ACC_NAME = MyReader4("ACC_NAME").ToString
                gl_ACC_DESC = MyReader4("ACC_DESC").ToString
                gl_SUB1 = MyReader4("SUB1").ToString

            End If
            MyReader4.Close()
        Catch ex As Exception
            Throw New Exception("Error Loading Transaction Details", ex)
        End Try
    End Sub
End Module
