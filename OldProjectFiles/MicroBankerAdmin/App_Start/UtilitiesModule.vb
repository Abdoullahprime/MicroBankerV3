Imports System.Data.SqlClient
Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Module UtilitiesModule
    Public P_CustomerNo, P_AccountName, P_AssetGL_Control, GLAccount, GLName As String
    Public P_TransactionDate As String
    Public P_LiabilityGL_Control, P_BranchCode, P_AvailBal, P_UnblockAmount, P_BlockAmount, P_BookBalance, P_DefinationCode, P_ProductDefCode As String
    Public TxnType, TxnCode, PostMode, ContraGL, Contra_GL, TelleID, RefCode, returnValue As String
    Public Amount As Double
    Public gl_ACC_NO, gl_ACC_TYPE, gl_BAL_TYPE, gl_ACC_NAME, gl_ACC_DESC, gl_SUB1 As String
    Public subhead_ACC_TYPE, subhead_BAL_TYPE As String
    Public COA_ACC_Exists, subHead1_Exists, subHead2_Exists, GLExists As Boolean
    Public P_AccountDetailsHashMap As New Hashtable

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
            Dim mySearch4 As String = "SELECT GLNAME,GLACCTNO,ACT_BAL FROM GL_DETAIL where GLACCTNO = '" & accountNo.Trim() & "'"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                GLAccount = MyReader4("GLACCTNO").ToString
                GLName = MyReader4("GLNAME").ToString
                Amount = CDbl(MyReader4("ACT_BAL").ToString)
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

    'Public Sub getSub1_Sub2AcctNo(headType As Integer, Head As Integer, Flag As Integer, Optional ByVal sub1 As Object = Nothing)
    '    Try
    '        Dim MyDatObject As New DataManagement
    '        Dim mySearch4 As String = ""
    '        If Flag = 1 Then
    '            ' mySearch4 = "select AccountNo,CustomerNo,AccountName,AssetGL_Control,LiabilityGL_Control,BranchCode,AvailBal,BookBalance,ProductDefCode from [dbo].[tbAccount] where AccountNo = '" & accountNo.Trim & "'"
    '        ElseIf Flag = 2 Then
    '            ' mySearch4 = "select AccountNo,CustomerNo,AccountName,AssetGL_Control,LiabilityGL_Control,BranchCode,AvailBal,BookBalance,ProductDefCode from [dbo].[tbAccount] where AccountNo = '" & accountNo.Trim & "'"
    '        End If
    '        Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
    '        If MyReader4.HasRows = True Then
    '            MyReader4.Read()
    '            P_CustomerNo = MyReader4("CustomerNo").ToString
    '            P_AccountName = MyReader4("AccountName").ToString
    '            P_AssetGL_Control = MyReader4("AssetGL_Control").ToString
    '            P_LiabilityGL_Control = MyReader4("LiabilityGL_Control").ToString
    '            P_BranchCode = MyReader4("BranchCode").ToString
    '            P_AvailBal = MyReader4("AvailBal").ToString
    '            P_BookBalance = MyReader4("BookBalance").ToString
    '            P_DefinationCode = MyReader4("ProductDefCode").ToString().Trim()
    '        End If
    '        MyReader4.Close()
    '    Catch ex As Exception
    '        Throw New Exception("Error Loading Account Details", ex)
    '    End Try
    'End Sub

    Public Sub accountExists(ByVal Head As String, ByVal subHead As String, ByVal accountNo As String)
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "Select * FROM ChartOfAccounts WHERE " &
                       "HEAD='" & Head & "' AND SUB1='" & subHead & "' AND ACC_NO='" & accountNo & "'"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                COA_ACC_Exists = True
            Else
                COA_ACC_Exists = False
                'MyReader4.Read()
                'GLAccount = MyReader4("GLAccount").ToString
                'GLName = MyReader4("GLName").ToString
                'Amount = CDbl(MyReader4("Amount").ToString)
            End If
            MyReader4.Close()
        Catch ex As Exception
            Throw New Exception("Error GetGlAccoutNames ", ex)
        End Try
    End Sub

    Public Sub GL_Exists(ByVal accountNo As String)
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "SELECT * FROM GL_DETAIL WHERE GLACCTNO='" & accountNo & "'"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                GLExists = True
            Else
                GLExists = False

            End If
            MyReader4.Close()
        Catch ex As Exception
            Throw New Exception("Error GetGlAccoutNames ", ex)
        End Try
    End Sub
    'Public Sub DoesSubHead1Exist(accountNo As String, headType As String, Head As String)
    '    Try
    '        Dim MyDatObject As New DataManagement
    '        Dim mySearch4 As String = "Select ACC_NO,ACC_TYPE, BAL_TYPE, HEAD_TYPE, ACC_NAME, ACC_DESC,HEAD, SUB1 " &
    '        "FROM ChartOfAccounts WHERE " &
    '                   "HEAD='" & Head & "' AND SUB1='" & sub1 & "' AND ACC_NO='" & acctNo & "'"
    '        Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
    '        If MyReader4.HasRows = True Then
    '            subHead1_Exists = True
    '        Else
    '            subHead1_Exists = False
    '            'MyReader4.Read()
    '            'GLAccount = MyReader4("GLAccount").ToString
    '            'GLName = MyReader4("GLName").ToString
    '            'Amount = CDbl(MyReader4("Amount").ToString)
    '        End If
    '        MyReader4.Close()
    '    Catch ex As Exception
    '        Throw New Exception("Error GetGlAccoutNames ", ex)
    '    End Try
    'End Sub

    'Public Sub DoesSubHead2Exist(accountNo As String, headType As String, Head As String)
    '    Try
    '        Dim MyDatObject As New DataManagement
    '        Dim mySearch4 As String = "Select ACC_NO,ACC_TYPE, BAL_TYPE, HEAD_TYPE, ACC_NAME, ACC_DESC,HEAD, SUB1 " &
    '        "FROM ChartOfAccounts WHERE " &
    '                   "HEAD='" & Head & "' AND SUB1='" & sub1 & "' AND ACC_NO='" & acctNo & "'"
    '        Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
    '        If MyReader4.HasRows = True Then
    '            subHead1_Exists = True
    '        Else
    '            subHead1_Exists = False
    '            'MyReader4.Read()
    '            'GLAccount = MyReader4("GLAccount").ToString
    '            'GLName = MyReader4("GLName").ToString
    '            'Amount = CDbl(MyReader4("Amount").ToString)
    '        End If
    '        MyReader4.Close()
    '    Catch ex As Exception
    '        Throw New Exception("Error GetGlAccoutNames ", ex)
    '    End Try
    'End Sub


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

    Public Sub GetSubHeadData(Head As Integer, subHead As String, flag As Integer, Optional ByVal accountNo As String = Nothing)
        Try
            Dim MyDatObject As New DataManagement
            Dim queryStr As String = ""
            If flag = 1 Then ' Sub Head1
                queryStr = "select ACC_TYPE,BAL_TYPE from chartofaccounts where head='" & Head & "' and sub1='" & subHead & "' and ACC_NO='" & accountNo & "'"
            ElseIf flag = 2 Then ' Sub Head2
                queryStr = "select ACC_TYPE,BAL_TYPE from chartofaccounts where head='" & Head & "' and sub1='" & subHead & "'"
            End If
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(queryStr, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                subhead_ACC_TYPE = MyReader4("ACC_TYPE").ToString
                subhead_BAL_TYPE = MyReader4("BAL_TYPE").ToString

            End If
            MyReader4.Close()
        Catch ex As Exception
            Throw New Exception("Error Loading Account Details", ex)
        End Try
    End Sub
    Public Sub GetSubHeadData(ByVal head As String, ByVal sub1 As String, ByVal acctNo As String)
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
    '    Select ACC_NO,ACC_TYPE, BAL_TYPE, HEAD_TYPE, ACC_NAME, ACC_DESC,HEAD, SUB1 
    'FROM ChartOfAccounts
    'WHERE
    ' HEAD='' AND SUB1='' AND ACC_NO=''

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
            Throw New Exception("ERROR EXECUTING ExecuteDataTable", ex)
        End Try
        Return dt
    End Function
    Public Function GetTransctionDate() As String
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select SOD from SYSTEM_DATES"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                P_TransactionDate = MyReader4("SOD").ToString
            End If
            MyReader4.Close()
        Catch ex As Exception

        End Try
        'Next
        Return P_TransactionDate
    End Function
    Public Function GetAccountDetailMap(accountNo) As Hashtable
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
            P_AccountDetailsHashMap.Add("error", ex)
        End Try
        'Next
        Return P_AccountDetailsHashMap
    End Function
End Module
