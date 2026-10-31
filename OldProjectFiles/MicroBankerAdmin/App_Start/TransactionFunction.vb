Imports System.Data.SqlClient
Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Module TransactionFunction
    Public P_TxnCode As String = "", P_TxnDesc As String = "", P_TxnType As String = "", P_ChargeAmt As String = "", P_ChqNo As String = "", P_TxnCash As String = "", P_PostMode As String = "", P_ContraGL As String = "", P_TelleID As String = "", P_AttachedTxnCode As String = "", P_RefCode As String = ""
    Public P_TransactionCodeMap As New Hashtable
    Public Function GetTransactionCodeMap(transCode) As Hashtable
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select TxnCode,TxnDesc,TxnType,ChargeAmt,ChqNo,TxnCash,PostMode,ContraGL,TelleID,AttachedTxnCode,RefCode,LoanDisbursement,AccountType from MB_TransactionCode  where TxnCode = '" & transCode & "'"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                P_TransactionCodeMap.Add("TxnCode", MyReader4("TxnCode").ToString)
                P_TransactionCodeMap.Add("TxnDesc", MyReader4("TxnDesc").ToString)
                P_TransactionCodeMap.Add("TxnType", MyReader4("TxnType").ToString)
                P_TransactionCodeMap.Add("ChargeAmt", MyReader4("ChargeAmt").ToString)
                P_TransactionCodeMap.Add("ChqNo", MyReader4("ChqNo").ToString)
                P_TransactionCodeMap.Add("TxnCash", MyReader4("TxnCash").ToString)
                P_TransactionCodeMap.Add("PostMode", MyReader4("PostMode").ToString)
                P_TransactionCodeMap.Add("ContraGL", MyReader4("ContraGL").ToString)
                P_TransactionCodeMap.Add("TelleID", MyReader4("TelleID").ToString)
                P_TransactionCodeMap.Add("AttachedTxnCode", MyReader4("AttachedTxnCode").ToString)
                P_TransactionCodeMap.Add("RefCode", MyReader4("RefCode").ToString)
                P_TransactionCodeMap.Add("LoanDisbursement", MyReader4("LoanDisbursement").ToString)
                P_TransactionCodeMap.Add("AccountType", MyReader4("AccountType").ToString)
            End If
            MyReader4.Close()
        Catch ex As Exception
            P_TransactionCodeMap.Add("error", ex)

        End Try
        'Next
        Return P_TransactionCodeMap
    End Function

End Module
