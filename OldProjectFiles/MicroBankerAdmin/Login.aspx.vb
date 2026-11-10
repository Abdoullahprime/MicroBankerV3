Imports System.Data.SqlClient
Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Imports System.Drawing
Public Class Login
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Protected Sub Post(sender As Object, e As EventArgs)
        Response.Redirect("Login.aspx")
    End Sub
    Protected Sub CmdLogin_Click(sender As Object, e As EventArgs) Handles cmdLogin.Click
        Try
            Dim LoginReply As String = ""

            If Page.IsValid Then
                If txtUserName.Text = "" And txtPassword.Text = "" Then
                    lblMessage.Text = "Please Enter Login Credentials"

                    Exit Sub
                Else
                    lblMessage.Text = ""
                End If

                If txtUserName.Text = "" Then
                    lblMessage.Text = "Please Enter Username"

                    Exit Sub
                Else
                    lblMessage.Text = ""
                End If

                If txtPassword.Text = "" Then
                    lblMessage.Text = "Please Enter Password"

                    Exit Sub
                Else
                    lblMessage.Text = ""
                End If

                lblMessage.Text = ""
                LoginReply = LoginDetailsTrue()
                'LoginReply = "CannAccess"


                lblMessage.Visible = True

                Select Case LoginReply
                    Case "CannAccess"

                        Try

                            'Dim oD As New DirectoryEntry
                            'Dim DisplayName As String

                            'Dim PGToken As Int32
                            'Dim PN() As String = {"primaryGroupToken"} 'primaryGroupToken  -mwalletad,

                            'oD = GetDirectoryEntry("LDAP://192.168.0.5/CN=users;DC=CO-OPBANK,DC=CO,DC=KE", txtUserName.Text.ToLower, txtPassword.Text)
                            'DisplayName = CType(oD.Properties("displayName").Value, String)

                            'oD.RefreshCache(PN)
                            'PGToken = CType(oD.Properties("PrimaryGroupToken").Value, Int32)

                            '''RegenerateID()

                            'InsertAudit(Session("UserName").ToString, "Login", "Successful Login to Admin Portal", Request.Url.ToString, Session("ip").ToString, Session("sessionid").ToString)

                            Response.Redirect("Dashboard.aspx")
                            'Response.Redirect("Dashboard.aspx")
                            'Response.Redirect("CreateUser.aspx")
                        Catch ex As Exception


                            lblMessage.Text = "Sorry you have used the wrong Credentials or your account is locked. Please check Credentials or contact ICT Help desk"
                            txtUserName.Text = ""

                            'InsertAudit(Session("UserName").ToString, "Login", "Unsuccessful Login to Mwallet Portal", Request.Url.ToString, Session("ip").ToString.Trim, Session("sessionid").ToString.Trim)

                        End Try
                    Case "ProcessRoutineOngoing"
                        'InsertAudit(Session("UserName").ToString, "Login", "Login Account Has Not Been Activated", Request.Url.ToString, Session("ip").ToString, Session("sessionid").ToString)

                        lblMessage.Text = "System Process Routine Ongoing, Please Try Again Later"
                        'Response.Redirect("ChangePasswordActivation.aspx")

                    Case "loggedin"
                        ' InsertAudit(Session("UserName").ToString, "Login", "Login Account Has Not Been Activated", Request.Url.ToString, Session("ip").ToString, Session("sessionid").ToString)
                        lblMessage.Text = "You are already logged in."

                    Case "NeedActivation"
                        'InsertAudit(Session("UserName").ToString, "Login", "Login Account Has Not Been Activated", Request.Url.ToString, Session("ip").ToString, Session("sessionid").ToString)

                        lblMessage.Text = "Your Login Credentials Has Not Been Activiated, Please Contact Admin"
                        'Response.Redirect("ChangePasswordActivation.aspx")
                    Case "InvalidDetails"
                        lblMessage.Text = "Invalid Login Details!"
                        txtUserName.Text = ""
                        txtPassword.Text = ""
                        'InsertAudit(Session("UserName").ToString, "Login", "Unsuccessful Login to Mwallet Portal", Request.Url.ToString, Session("ip").ToString, Session("sessionid").ToString)

                        'AUDIT TRAIL-------
                        ' Dim audit As New DataManagement
                        ' audit.AddAdminAuditTrail(Session("UserName").ToString, "Login", "UNSECCESSFUL Login to Agency Banking Module", _
                        'Request.Url.ToString, Session("ip").ToString, Session("sessionid").ToString)
                        '----END AUDIT TRAIL-----
                        lblMessage.Text = "Invalid Login Details!"
                    Case ""
                        'AUDIT TRAIL-------
                        'Dim audit As New DataManagement
                        ' audit.AddAdminAuditTrail(Session("UserName").ToString, "Login", "UNSUCCESSFUL Login to Internet Banking Admin Module", _
                        ' Request.Url.ToString, Session("ip").ToString, Session("sessionid").ToString)
                        '----END AUDIT TRAIL-----
                        lblMessage.Text = "There apprears to be a connection problem, please try later!"
                End Select

            End If

        Catch ex As Exception

        End Try
    End Sub
    Private Function LoginDetailsTrue() As String
        Dim loggedin1 As String
        'Dim CryptographyFactory As New FinpayiSecurity.CryptoFactory()
        'Dim Cryptographer As FinpayiSecurity.ICrypto = CryptographyFactory.MakeCryptographer(EnctryptionAlgorith)

        Dim ReplyString As String = ""

        Dim MySmsObject As New DataManagement
        'Dim MySec As New FinpayiSecurity

        'Dim mr As oracledatareader = "" 

        Try
            Dim MyActivate As Int16 = 0
            Dim reply As String = ""

            Dim SqlAction As String
            SqlAction = "Select * from MB_SystemUsers where UserName ='" & txtUserName.Text.Trim.ToLower & "'" ' and Approved =1 "
            'Dim Myreader As SqlDataReader = TryCast(MySmsObject.DBAction(SqlAction, DataManagement.DBActionType.DataReader), SqlDataReader)

            Dim Myreader As SqlDataReader = TryCast(MySmsObject.DBAction(SqlAction, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            'Myreader.Read()

            If Myreader.HasRows Then
                Myreader.Read()
                reply = "CannAccess"
                Try
                    Dim MyDatObject As New DataManagement
                    Dim mySearch4 As String = "select STATUS from [dbo].[SYSTEM_DATES]"
                    Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
                    If MyReader4.HasRows = True Then
                        MyReader4.Read()
                        Dim transactionStatus As String = ""
                        transactionStatus = MyReader4("STATUS").ToString
                        If transactionStatus <> "T" And txtUserName.Text.Trim <> "ADMIN" Then
                            reply = "ProcessRoutineOngoing"
                        End If
                    End If
                    MyReader4.Close()
                Catch ex As Exception
                    'Label4.Text = "Error Pulling Account Details"
                End Try
                'Dim MyEncrptedPass As String = Cryptographer.Decrypt(Myreader("Password"))

                'Dim ip
                'ip = Request.ServerVariables("HTTP_X_FORWARDED_FOR")
                'If ip = Nothing Then
                '    ip = ""
                'End If

                '' If there is no proxy, get the standard remote address 

                'If ip = "" Or ip.ToLower() = "unknown" Then
                '    ip = Request.ServerVariables("REMOTE_ADDR").ToString
                'End If
                '---------
                'Dim SqlAction4 As String = ""

                'Dim profilename As String = ""
                'Session("ip") = ip
                'Session("Branch") = Myreader("Branch")
                'Session("MyProfileID") = Myreader("ProfileID")
                'SqlAction4 = "Select PROFILENAME FROM PROFILE WHERE APPROVED = 1 AND ProfileID = '" & Session("MyProfileID") & "'"

                'Dim Myreader4 As SqlDataReader = TryCast(MySmsObject.DBAction(SqlAction4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
                '' Myreader4.Read()

                'If Myreader4.HasRows Then
                '    Myreader4.Read()
                '    profilename = Myreader4("PROFILENAME")
                '    Myreader4.Close()
                'End If

                'loggedin1 = Myreader("LOGGEDIN")
                'Session("Branch") = Myreader("BRANCH")
                'Session("FullNames") = Myreader("EmployeeName") & "-" & profilename
                'Session("MyEmployeeID") = Myreader("EmployeeID")
                Session("UserName") = Myreader("UserName")
                'Session("login") = Now.ToString("hh:mm:ss")
                'Session("IsClientId") = Myreader("ClientId")

                'Session("EmployeeName") = Myreader("EmployeeName").ToString.Trim.ToLower

                ''Session("sessionid") = Cryptographer.Encrypt(Session.SessionID.ToString & Date.Now & ip)

                'Session("sessionid") = Session.SessionID.ToString

                'MyActivate = IIf(IsDBNull(Myreader("Activated")) = True, 0, Myreader("Activated"))

                'Dim MyActivate1 As Boolean = CBool(MyActivate)

                'If MyActivate1 = True Then
                'reply = "CannAccess"
                'Else
                'reply = "NeedActivation"
                'Session("UserName") = ""
                'End If

                'If loggedin1 = "YES" Then
                'reply = "loggedin"
                '    Session("UserName") = ""

                '    'Else
                '    reply = "CannAccess"

                'Dim MyDBAction As New DataManagement
                'Dim mySqlAction2 As String = ""
                'mySqlAction2 = "Update  EMPLOYEES set LOGGEDIN ='YES' where USERNAME ='" & txtUserName.Text & "'"
                'MyDBAction.DBAction(mySqlAction2, DataManagement.DBActionType.Update)
                'Response.Redirect("Login.aspx")

                'End If
                'Else
                'reply = "InvalidDetails"
                'Session("UserName") = ""
                ' End If
                ' Else
                '     reply = "InvalidDetails"

            End If

            'Dim loggedin As String

            Myreader.Close()

            Return reply

        Catch ex As Exception
            Dim Errmsg As String = ex.Message
        End Try
    End Function
End Class