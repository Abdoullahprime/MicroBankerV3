'Public Class Functions
Imports Microsoft.VisualBasic
Imports System.Data.SqlClient
Imports System.Web.Configuration
Imports System.IO
Imports System.Text
Imports System.Security.Cryptography
'Imports Microsoft.Practices.EnterpriseLibrary.Security.Cryptography
Imports System.Data.Odbc
'Imports System.Data.OracleClient
Imports System.Net.Mail
Imports Microsoft
Imports System.Diagnostics
Imports System.Net
'Imports MyFinpayi
'Imports Oracle.DataAccess.Client
Imports System
Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin


Namespace MicroBankerAdmin

    Friend Class MicroBankerAdmin
        Public Class DataManagement
            Public Enum DBActionType
                DataReader = 1
                Dataset = 2
                Scalar = 3
                Delete = 4
                Insert = 5
                Update = 6
                DataTable = 7
                ODBCDataReader = 8
                ODBCScalar = 9
                ODBCDataTable = 10
                SqlDataReader = 11
                SqlScalar = 12
                SqlDataTable = 13

            End Enum
            Public Function DBAction(ByVal MyDBAction As Object, ByVal ActionType As DBActionType) As Object


                Dim myip As String = ""
                Dim myport As String = ""
                Dim myServiceName As String = ""
                Dim myUser As String = ""
                Dim myPassword As String = ""


                myip = WebConfigurationManager.AppSettings("DBip")
                myport = WebConfigurationManager.AppSettings("DBport")
                myServiceName = WebConfigurationManager.AppSettings("ServiceName")

                myUser = WebConfigurationManager.AppSettings("User")
                myPassword = WebConfigurationManager.AppSettings("Password")
                'myPassword = DecryptString(myPassword.ToString, "@123COOP321#")

                Dim myConnection As SqlConnection = New SqlConnection()
                Dim strconn As String

                'strconn = "Data Source = "
                'strconn &= "(DESCRIPTION = "
                'strconn &= " (ADDRESS_LIST ="
                'strconn &= " (ADDRESS = (PROTOCOL = TCP)(HOST = " & myip & ")(PORT = " & myport & "))"

                'strconn &= " )"
                'strconn &= " (CONNECT_DATA ="
                'strconn &= " (SERVICE_NAME = " & myServiceName & ")"
                'strconn &= " )"
                'strconn &= ");"
                'strconn &= "User Id=" & myUser & ";"
                'strconn &= "password=" & myPassword & ";"
                'strconn = "Server=DESKTOP-EAN1J18;database=MicroBanker;MultipleActiveResultSets=True;user id=sa;pwd=mat@123"
                'strconn = "Server=172.26.1.66;database=MicroBanker;MultipleActiveResultSets=True;user id=sa;pwd=M4tr1x@Cl0ud"

                strconn = ConfigurationManager.ConnectionStrings("ConnectionString").ConnectionString


                Dim myODBCConnection As New OdbcConnection
                Dim MySqlconnection As New SqlConnection

                Dim MyReader As SqlDataReader = Nothing
                Dim MyReaderODBC As OdbcDataReader = Nothing
                Dim MyOracleReaderReader As SqlDataReader = Nothing

                Try

                    'myConnection.Open()
                    myConnection.ConnectionString = strconn
                    myConnection.Open()

                    Select Case ActionType
                        Case 3
                            Dim myOracleCommand As New SqlCommand(MyDBAction.ToString, myConnection)
                            Dim ScalarItem As Object = myOracleCommand.ExecuteScalar

                            'MyOracleConnection.Close()

                            Return ScalarItem
                        Case 4, 5, 6
                            Dim myCommand As New SqlCommand(MyDBAction.ToString, myConnection)
                            myCommand.ExecuteNonQuery()
                            myConnection.Close()

                        Case DBActionType.DataTable ' 7 '.......
                            myODBCConnection = Nothing
                            MySqlconnection = Nothing

                            Dim MyAdpater As New SqlDataAdapter(MyDBAction.ToString, myConnection)
                            Dim MyDataTable As New Data.DataTable
                            MyAdpater.Fill(MyDataTable)
                            myConnection.Close()

                            Return MyDataTable

                        Case DBActionType.ODBCDataReader ' 8
                            myODBCConnection = Nothing

                            myODBCConnection.ConnectionString = WebConfigurationManager.ConnectionStrings("DeltaConnection").ConnectionString
                            myODBCConnection.Open()

                            Dim myCommand As New OdbcCommand(MyDBAction.ToString, myODBCConnection)
                            MyReaderODBC = myCommand.ExecuteReader(Data.CommandBehavior.CloseConnection)

                            Return MyReaderODBC

                        Case DBActionType.ODBCScalar ' 9

                            MySqlconnection = Nothing

                            myODBCConnection.ConnectionString = WebConfigurationManager.ConnectionStrings("DeltaConnection").ConnectionString
                            myODBCConnection.Open()

                            Dim myCommand As New OdbcCommand(MyDBAction.ToString, myODBCConnection)
                            Dim ScalarItem As Object = myCommand.ExecuteScalar

                        Case DBActionType.ODBCDataTable '  10

                            MySqlconnection = Nothing

                            myODBCConnection.ConnectionString = WebConfigurationManager.ConnectionStrings("DeltaConnection").ConnectionString
                            myODBCConnection.Open()

                            Dim MyAdpater As New OdbcDataAdapter(MyDBAction.ToString, myODBCConnection)
                            Dim MyDataTable As New Data.DataTable
                            MyAdpater.Fill(MyDataTable)

                            Return MyDataTable

                        Case DBActionType.SqlScalar

                            Dim myOracleCommand As New SqlCommand(MyDBAction.ToString, myConnection)
                            Dim ScalarItem As Object = myOracleCommand.ExecuteScalar

                            MySqlconnection.Close()

                            Return ScalarItem

                        Case DBActionType.SqlDataReader

                            Dim myOracleCommand As New SqlCommand(MyDBAction.ToString, myConnection)
                            MyOracleReaderReader = myOracleCommand.ExecuteReader(Data.CommandBehavior.CloseConnection)

                            Return MyOracleReaderReader

                        Case DBActionType.SqlDataTable

                            Dim MyOracleAdpater As New SqlDataAdapter(MyDBAction.ToString, myConnection)
                            Dim MyDataTable As New Data.DataTable
                            MyOracleAdpater.Fill(MyDataTable)
                            myConnection.Close()
                            Return MyDataTable

                            'myPassword = DecryptString(myPassword, "@123OO321#").ToString()


                            'DecryptString(MyReader("VIRTUAL_AC1").ToString.Trim, "")
                    End Select

                Catch ex As Exception

                    LogError("Micro Banker Admin", ex)
                    MsgBox("Connection problem encountered.")
                    Exit Try
                Finally

                    '    If Not IsNothing(MyReader) OrElse Not IsNothing(MyReaderODBC) OrElse Not IsNothing(MyOracleReaderReader) Then

                    '    ElseIf myConnection.State <> Data.ConnectionState.Closed AndAlso IsNothing(MyReader) Then
                    '        myConnection.Close()
                    '        myConnection.Dispose()

                    '    ElseIf myConnection.State <> Data.ConnectionState.Closed AndAlso IsNothing(MyReaderODBC) Then
                    '        myConnection.Close()
                    '        myConnection.Dispose()

                    '    ElseIf Not IsNothing(myODBCConnection) AndAlso IsNothing(MyReaderODBC) Then
                    '        myODBCConnection.Close()
                    '        myODBCConnection.Dispose()

                    '        myConnection.Close()
                    '        myConnection.Dispose()

                    '    ElseIf Not IsNothing(MyOracleConnection) AndAlso IsNothing(MyOracleReaderReader) Then
                    '        MyOracleConnection.Close()
                    '        MyOracleConnection.Dispose()

                    '        MyOracleConnection.Close()
                    '        MyOracleConnection.Dispose()
                    '    Else

                    '        If myConnection.State <> Data.ConnectionState.Closed Then
                    '            myConnection.Close()
                    '        End If
                    '        myConnection.Dispose()

                    '    End If
                End Try



            End Function
            Public Shared Function DecryptString(ByVal Message As String, ByVal Passphrase As String) As String

                Dim Results() As Byte

                Dim UTF8 As System.Text.UTF8Encoding = New System.Text.UTF8Encoding

                ' Step 1. We hash the Pass phrase using MD5

                ' We use the MD5 hash generator as the result is a 128 bit byte array

                ' which is a valid length for the TripleDES encoder we use below

                Using HashProvider As MD5CryptoServiceProvider = New MD5CryptoServiceProvider()

                    Dim TDESKey() As Byte = HashProvider.ComputeHash(UTF8.GetBytes(Passphrase))

                    ' Step 2. Create a new TripleDESCryptoServiceProvider object

                    ' Step 3. Setup the decoder

                    Using TDESAlgorithm As TripleDESCryptoServiceProvider = New TripleDESCryptoServiceProvider() 'with {.Key = TDESKey, .Mode = CipherMode.ECB, .Padding = PaddingMode.PKCS7}

                        TDESAlgorithm.Key = TDESKey
                        TDESAlgorithm.Mode = CipherMode.ECB
                        TDESAlgorithm.Padding = PaddingMode.PKCS7

                        ' Step 4. Convert the input string to a byte[]

                        Dim DataToDecrypt() As Byte = Convert.FromBase64String(Message)

                        ' Step 5. Attempt to decrypt the string

                        Try

                            Dim Decryptor As ICryptoTransform = TDESAlgorithm.CreateDecryptor

                            Results = Decryptor.TransformFinalBlock(DataToDecrypt, 0, DataToDecrypt.Length)

                        Finally

                            ' Clear the TripleDes and Hash provider services of any sensitive information

                            TDESAlgorithm.Clear()

                            HashProvider.Clear()

                        End Try

                    End Using

                End Using

                ' Step 6. Return the decrypted string in UTF8 format

                Return UTF8.GetString(Results)

            End Function
            Private Sub LogError(ByVal thismodule As String, ByVal ex1 As Exception, Optional ByVal filename As String = "SystemErrors", Optional ByVal extradetails As String = "")
                Try
                    Dim fileslocation As String = WebConfigurationManager.AppSettings("Logslocation")

                    If String.IsNullOrEmpty(fileslocation) Then
                        fileslocation = "D:\Logs\"
                    End If
                    My.Computer.FileSystem.WriteAllText(fileslocation & "\" & filename & "-" & Format(Now, "dd-MM-yyyy") & ".log", ex1.Message.ToString & vbCrLf & "-" & extradetails & "-" & thismodule & "-" & Now & vbCrLf, True)
                Catch ex As Exception

                End Try
            End Sub
        End Class

    End Class
    Public Class MicroBankerSecurity
        Private Const hashProvider As String = "hashprovider"
        Private Const symmProvider As String = "symprovider"

        Private Const symmKeyFileName As String = "SymmetricKeyFile.txt"

        ' Encrypted string
        Private stringToEncrypt As String
        Private encryptedContentsBase64 As String

        ' Generated hash
        Private stringForHash As String
        Private generatedHash As Byte()

        'Public Function EncryptedPass(ByVal PassToEncrypt As String) As String
        '    Me.encryptedContentsBase64 = Cryptographer.EncryptSymmetric(symmProvider, PassToEncrypt)
        '    Return Me.encryptedContentsBase64
        'End Function
        'Public Function DencryptPass(ByVal PassToDecrypt As String) As String
        '    Try
        '        Dim readableString As String = Cryptographer.DecryptSymmetric(symmProvider, PassToDecrypt)
        '        Return readableString

        '    Catch ex As Exception
        '        'MsgBox(ex.Message, MsgBoxStyle.Critical, ex.Source)
        '        Return String.Empty
        '    End Try
        'End Function
        'Public Function GenRandomPassword() As String
        '    Dim KeyGen As RandomKeyGenerator
        '    Dim NumKeys As Integer
        '    'Dim i_Keys As Integer
        '    Dim RandomKey As String

        '    NumKeys = 1

        '    KeyGen = New RandomKeyGenerator
        '    KeyGen.KeyLetters = "abcdefghjkmnpqrstuvwxyz"
        '    KeyGen.KeyNumbers = "123456789"
        '    KeyGen.KeyChars = 7

        '    RandomKey = KeyGen.Generate()

        '    Return RandomKey

        'End Function
        Public Interface ICrypto
            Function BlockSize() As Integer
            Function KeySize() As Integer

            Function Encrypt(ByVal data As String) As String
            Function Decrypt(ByVal data As String) As String
        End Interface
        Public Class CryptoFactory
            Public Function MakeCryptographer(ByVal type As String) As ICrypto
                Select Case type.ToLower
                    Case "des"
                        'Return New DES()
                    Case "tripledes"
                        'Return New TripleDES()
                    Case "rijndael"
                        Return New Rijndael()
                    Case Else
                        'Return New TripleDES()
                End Select
            End Function
        End Class
        Public Class Rijndael
            Implements ICrypto

            ' The key and initialization vector : change them for your application
            Private _key() As Byte = {132, 42, 53, 124, 75, 56, 87, 38, 9, 10, 161, 132, 183,
        91, 105, 16, 117, 218, 149, 230, 221, 212, 235, 64}
            Private _iv() As Byte = {83, 71, 26, 58, 54, 35, 22, 11, 83, 71, 26, 58, 54, 35, 22, 11}

            ' returns the default size, in bits of the iv
            Public Function BlockSize() As Integer Implements ICrypto.BlockSize
                Dim aes As New RijndaelManaged()

                Return aes.BlockSize
            End Function

            ' returns the default size, in bits of the key
            Public Function KeySize() As Integer Implements ICrypto.KeySize
                Dim aes As New RijndaelManaged()

                Return aes.KeySize
            End Function

            ' decrypts a string that was encrypted using the Encrypt method
            Public Function Decrypt(ByVal data As String) As String Implements ICrypto.Decrypt
                Try
                    Dim inBytes() As Byte = Convert.FromBase64String(data)
                    Dim mStream As New MemoryStream(inBytes, 0, inBytes.Length) ' instead of writing the decrypted text

                    Dim aes As New RijndaelManaged()
                    Dim cs As New CryptoStream(mStream, aes.CreateDecryptor(_key, _iv), CryptoStreamMode.Read)

                    Dim sr As New StreamReader(cs)

                    Return sr.ReadToEnd()
                Catch ex As Exception
                    Throw ex
                End Try
            End Function

            ' Encrypts a given string
            Public Function Encrypt(ByVal data As String) As String Implements ICrypto.Encrypt
                Try
                    Dim utf8 As New UTF8Encoding
                    Dim inBytes() As Byte = utf8.GetBytes(data) ' ascii encoding uses 7 
                    'bytes for characters whereas the encryption uses 8 bytes, thus we use utf8
                    Dim ms As New MemoryStream() 'instead of writing the encrypted 
                    'string to a filestream, I will use a memorystream

                    Dim aes As New RijndaelManaged()
                    Dim cs As New CryptoStream(ms, aes.CreateEncryptor(_key, _iv), CryptoStreamMode.Write)

                    cs.Write(inBytes, 0, inBytes.Length) ' encrypt
                    cs.FlushFinalBlock()

                    Return Convert.ToBase64String(ms.GetBuffer(), 0, ms.Length)
                Catch ex As Exception
                    Throw ex
                End Try
            End Function
        End Class
        Public Function GenRandomPassword() As String
            'Dim KeyGen As RandomKeyGenerator
            'Dim NumKeys As Integer
            ''Dim i_Keys As Integer
            'Dim RandomKey As String

            'NumKeys = 1

            'KeyGen = New RandomKeyGenerator
            'KeyGen.KeyLetters = "abcdefghjkmnpqrstuvwxyz"
            'KeyGen.KeyNumbers = "123456789"
            'KeyGen.KeyChars = 7

            'RandomKey = KeyGen.Generate()

            'Return RandomKey

        End Function


    End Class
End Namespace

'End Class
