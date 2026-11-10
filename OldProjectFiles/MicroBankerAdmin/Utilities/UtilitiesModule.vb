Imports System.Data.SqlClient
Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Module UtilitiesModule
    Public P_CustomerNo, P_AccountName, P_AssetGL_Control, GLAccount, GLName As String
    Public P_LiabilityGL_Control, P_BranchCode, P_AvailBal, P_BookBalance, P_DefinationCode As String
    Public TxnType, TxnCode, PostMode, ContraGL, Contra_GL, TelleID, RefCode, returnValue As String
    Public Amount As Double
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
End Module
