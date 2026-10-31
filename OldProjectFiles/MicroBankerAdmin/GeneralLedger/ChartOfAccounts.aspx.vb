Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Imports System.Data.SqlClient
Public Class ChartOfAccounts
    Inherits System.Web.UI.Page
    Public Const HEAD = "HEAD"
    Public Const HEAD_TYPE = "HEAD_TYPE"
    Public Const SUB_HEAD_1 = "SUB1"
    Public subhead_ACC_TYPE As String = "", subhead_BAL_TYPE As String = ""
    Public COA_ACC_Exists As Boolean = False

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            Me.LoadGL_Head()

        End If

    End Sub

    Public Property GL_HeadTypeProperty() As String
        Get
            Return Me.GL_HeadTypeDropDownList.SelectedValue.Trim()
        End Get
        Set(ByVal value As String)
            Me.GL_HeadTypeDropDownList.SelectedValue = value
        End Set
    End Property


    Public Property AccountNoProperty() As String
        Get
            Return Me.txtAccountNo.Text.Trim()
        End Get
        Set(ByVal value As String)
            Me.txtAccountNo.Text = value
        End Set
    End Property


    Public Property AllowPostingProperty() As Integer
        Get
            Return IIf(Me.AllowPostingDropDownList.SelectedValue.Trim() = "Y", 1, 0)

        End Get
        Set(ByVal value As Integer)
            Me.AllowPostingDropDownList.SelectedValue = value
        End Set
    End Property


    Public Property HeadGLProperty() As String
        Get
            Return Me.HeadGLDropDownList.SelectedValue.Trim()
        End Get
        Set(ByVal value As String)
            Me.HeadGLDropDownList.SelectedValue = value
        End Set
    End Property


    Public Property SubHead1Property() As String
        Get
            Return Me.SubHead1DropDownList.SelectedValue.Trim()
        End Get
        Set(ByVal value As String)
            Me.SubHead1DropDownList.SelectedValue = value
        End Set
    End Property


    Public Property AccountTypeProperty() As String
        Get
            Return Me.AccountTypeDropDownList.SelectedValue.Trim()
        End Get
        Set(ByVal value As String)
            Me.AccountTypeDropDownList.SelectedValue = value
        End Set
    End Property


    Public Property BalanceTypeProperty() As String
        Get
            Return Me.BalanceTypeDropDownList.SelectedValue.Trim()
        End Get
        Set(ByVal value As String)
            Me.BalanceTypeDropDownList.SelectedValue = value
        End Set
    End Property


    Public Property AccountNameProperty() As String
        Get
            Return UCase(Me.txtAccountName.Text.Trim())
        End Get
        Set(ByVal value As String)
            Me.txtAccountName.Text = value
        End Set
    End Property
    Public Sub LoadGL_Head()
        Try
            Dim MyDataTable As New Data.DataTable
            Dim MyDBAction As New DataManagement
            MyDataTable = TryCast(MyDBAction.DBAction("SELECT [ACC_NO], [ACC_NAME] FROM [dbo].[ChartOfAccounts] WHERE [HEAD_TYPE]= 1 ", DataManagement.DBActionType.SqlDataTable), Data.DataTable)
            If MyDataTable IsNot Nothing Then
                With HeadGLDropDownList

                    .Items.Clear()
                    .DataSource = MyDataTable
                    .DataTextField = MyDataTable.Columns("ACC_NAME").ToString.Trim
                    .DataValueField = MyDataTable.Columns("ACC_NO").ToString.Trim
                    .DataBind()
                    .Items.Add(New ListItem("Select Account Type", "0"))
                    .SelectedValue = 0
                End With
                MyDataTable = Nothing
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub LoadSubHead_1(headType As Integer, headNo As Integer, ddl_Subhead1 As DropDownList)
        Try
            Dim MyDataTable As New Data.DataTable
            Dim MyDBAction As New DataManagement
            MyDataTable = TryCast(MyDBAction.DBAction("SELECT [ACC_NO], [ACC_NAME] FROM [dbo].[ChartOfAccounts] WHERE HEAD = " & headNo & " And HEAD_TYPE = " & headType & " And DELETED <> 'Y'", DataManagement.DBActionType.SqlDataTable), Data.DataTable)
            If MyDataTable IsNot Nothing Then
                With ddl_Subhead1

                    .Items.Clear()
                    .DataSource = MyDataTable
                    .DataTextField = MyDataTable.Columns("ACC_NAME").ToString.Trim
                    .DataValueField = MyDataTable.Columns("ACC_NO").ToString.Trim
                    .DataBind()
                    .Items.Add(New ListItem("Select Account Type", "0"))
                    .SelectedValue = 0

                End With
                MyDataTable = Nothing
            End If
        Catch ex As Exception
        End Try
    End Sub
    Private Sub LoadSubHead1(ddlSubhead1 As DropDownList, ByVal headNo As Integer)
        Try
            Dim MyDataTable As New Data.DataTable
            Dim MyDBAction As New DataManagement
            MyDataTable = TryCast(MyDBAction.DBAction("SELECT [ACC_NO], [ACC_NAME] FROM [dbo].[ChartOfAccounts] WHERE HEAD = " & headNo & " And HEAD_TYPE = 2 And DELETED <> 'Y'", DataManagement.DBActionType.SqlDataTable), Data.DataTable)
            If MyDataTable IsNot Nothing Then
                With ddlSubhead1
                    .Items.Clear()
                    .DataSource = MyDataTable
                    .DataTextField = MyDataTable.Columns("ACC_NAME").ToString.Trim
                    .DataValueField = MyDataTable.Columns("ACC_NO").ToString.Trim
                    .DataBind()
                    .Items.Add(New ListItem("Select Sub Head", "0"))
                    .SelectedValue = 0
                End With
                MyDataTable = Nothing
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub GL_HeadTypeDropDownList_SelectedIndexChanged(sender As Object, e As EventArgs) Handles GL_HeadTypeDropDownList.SelectedIndexChanged
        Dim headTypeValue As String = Me.GL_HeadTypeDropDownList.SelectedValue
        Me.ResetControls()
        Me.GL_HeadTypeDropDownList.SelectedValue = headTypeValue
        If headTypeValue = "0" Then
            Exit Sub
        End If

        Me.txtAccountNo.Text = ""
        If headTypeValue = "1" Then
            Me.txtAccountNo.ReadOnly = False
            Me.AccountTypeDropDownList.Enabled = True
            Me.btnReset.Enabled = True
        ElseIf headTypeValue = "2" Then
            Me.txtAccountNo.ReadOnly = False
            Me.HeadGLDropDownList.Enabled = True
            Me.SubHead1DropDownList.Enabled = False
            Me.btnReset.Enabled = True
        ElseIf headTypeValue = "3" Then
            Me.txtAccountNo.ReadOnly = False
            Me.HeadGLDropDownList.Enabled = True
            Me.SubHead1DropDownList.Enabled = False

            Me.btnReset.Enabled = True
        End If
    End Sub
    Private Sub HeadDropDownList_SelectedIndexChanged(sender As Object, e As EventArgs) Handles HeadDropDownList.SelectedIndexChanged
        Dim accountNo As String = ""
        Dim subHeadLevel As String = ""
        Dim accountType As String = ""
        Dim balanceType As String = ""
        Me.LabelErrorMsgLabel.Text = ""
        Me._msgSuccessLabel.Text = ""

        Dim headTypeValue As String = Me.GL_HeadTypeDropDownList.SelectedValue

        Dim headNo As String = Me.HeadGLDropDownList.SelectedValue

        subHeadLevel = headTypeValue.Trim()
        accountNo = headNo.Trim()

        If subHeadLevel = "" Or accountNo = "" Then

            Exit Sub
        End If

        If subHeadLevel = 2 Then
            UtilitiesModule.GetSubHeadData("$$", "$$", 1, accountNo.Trim())
        ElseIf subHeadLevel = 3 Then 'sub 2
            UtilitiesModule.GetSubHeadData("$$", "$$", 1, Nothing)
        End If

        Dim headTpye As Integer = Nothing

        If headNo = 0 Then
            Me.SubHead1DropDownList.SelectedValue = "0"
            Me.AccountTypeDropDownList.SelectedValue = "0"
            Me.BalanceTypeDropDownList.SelectedValue = "0"
            Exit Sub
        End If

        Me.AccountTypeDropDownList.SelectedValue = UtilitiesModule.subhead_ACC_TYPE
        Me.BalanceTypeDropDownList.SelectedValue = UtilitiesModule.subhead_BAL_TYPE

        If CInt(GL_HeadTypeProperty) = 2 Then
        End If
        If CInt(GL_HeadTypeProperty) = 3 Then
            Me.LoadSubHead1(Me.SubHead1DropDownList, headNo.Trim())
            Me.SubHead1DropDownList.Enabled = True
        End If
    End Sub

    Private Sub AccountTypeDropDownList_SelectedIndexChanged(sender As Object, e As EventArgs) Handles AccountTypeDropDownList.SelectedIndexChanged
        Dim accountTypeValue As String = Me.AccountTypeDropDownList.SelectedValue
        If accountTypeValue = "I" Then
            Me.BalanceTypeDropDownList.SelectedValue = "C"
        ElseIf accountTypeValue = "L" Then
            Me.BalanceTypeDropDownList.SelectedValue = "C"
        ElseIf accountTypeValue = "E" Then
            Me.BalanceTypeDropDownList.SelectedValue = "D"
        ElseIf accountTypeValue = "A" Then
            Me.BalanceTypeDropDownList.SelectedValue = "D"
        End If
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Me._msgSuccessLabel.Text = ""
        Me.LabelErrorMsgLabel.Text = ""
        Dim QueryString As String = ""
        COA_ACC_Exists = False

        If Me.GL_HeadTypeDropDownList.SelectedValue = "0" Then
            Me.LabelErrorMsgLabel.Text = "Select Head"
            Exit Sub

        End If


        If Me.GL_HeadTypeDropDownList.SelectedValue = "1" Then
            If Me.txtAccountNo.Text = "" Or Me.AccountTypeDropDownList.SelectedValue = "0" Or AccountNameProperty = "" Then
                Me.LabelErrorMsgLabel.Text = "Select Account Type, Account No. and Account"
                Exit Sub
            End If

        ElseIf Me.GL_HeadTypeDropDownList.SelectedValue = "2" Then
            If Me.txtAccountNo.Text = "" Or Me.HeadGLDropDownList.SelectedValue = "0" Or AccountNameProperty = "" Then
                Me.LabelErrorMsgLabel.Text = "Select Account Type, Account No. and Account"
                Exit Sub
            End If
            'Me.txtAccountNo.ReadOnly = False
            'Me.HeadGLDropDownList.Enabled = True
            'Me.SubHead1DropDownList.Enabled = False
            'Me.btnReset.Enabled = True
        ElseIf Me.GL_HeadTypeDropDownList.SelectedValue = "3" Then
            If Me.txtAccountNo.Text = "" Or Me.HeadGLDropDownList.SelectedValue = "0" Or AccountNameProperty = "" Or Me.SubHead1DropDownList.SelectedValue = "0" Then
                Me.LabelErrorMsgLabel.Text = "Select Account Type, Sub Head 1, Account No. and Account Name"
                Exit Sub
            End If
            'Me.txtAccountNo.ReadOnly = False
            'Me.HeadGLDropDownList.Enabled = True
            'Me.SubHead1DropDownList.Enabled = False


        End If
        If GL_HeadTypeProperty = 1 Then
            Dim head As String = "$$"
            Dim sub1 As String = "$$"

            UtilitiesModule.accountExists(head, sub1, AccountNoProperty.Trim())
            If UtilitiesModule.COA_ACC_Exists = True Then
                Me.LabelErrorMsgLabel.Text = "account Exists "
                Exit Sub
            Else
                QueryString = "INSERT INTO ChartOfAccounts " &
                    "(ACC_NO, ACC_TYPE, BAL_TYPE, HEAD_TYPE, ACC_NAME, ACC_DESC, HEAD, SUB1, USER_ID, ADD_DATE, MOD_DATE, DELETED, ALLOW_POST) " &
                    "VALUES ('" & AccountNoProperty.Trim() & "','" & AccountTypeProperty.Trim() & "','" & BalanceTypeProperty.Trim() & "','" & GL_HeadTypeProperty.Trim() & "','" & AccountNameProperty.Trim() & "','" & AccountNameProperty.Trim() & "','" & head.Trim() & "','" & sub1.Trim() & "','Lamin', GetDate() , GetDate() ,'N','" & Me.AllowPostingProperty & "')"
                Me.Save(QueryString)
                Me.LoadGL_Head()
                Me.ResetControls()
                Me._msgSuccessLabel.Text = "GL Head " & AccountNameProperty.Trim() & " Successfully Added"
            End If
        End If
        If GL_HeadTypeProperty = 2 Then
            Dim head As String = UCase(HeadGLProperty)
            Dim sub1 As String = "$$"
            UtilitiesModule.accountExists(head, sub1, AccountNoProperty.Trim())
            If UtilitiesModule.COA_ACC_Exists = True Then
                Me.LabelErrorMsgLabel.Text = "account Exists "
                Exit Sub
            Else
                QueryString = "INSERT INTO ChartOfAccounts " &
                "(ACC_NO, ACC_TYPE, BAL_TYPE, HEAD_TYPE, ACC_NAME, ACC_DESC, HEAD, SUB1, USER_ID, ADD_DATE, MOD_DATE, DELETED, ALLOW_POST) " &
                "VALUES ('" & AccountNoProperty.Trim() & "','" & AccountTypeProperty.Trim() & "','" & BalanceTypeProperty.Trim() & "','" & GL_HeadTypeProperty.Trim() & "','" & AccountNameProperty.Trim() & "','" & AccountNameProperty.Trim() & "','" & head.Trim() & "','" & sub1.Trim() & "','Lamin', GetDate() , GetDate() ,'N','" & Me.AllowPostingProperty & "')"
                Me.Save(QueryString)
                Me.ResetControls()
                Me._msgSuccessLabel.Text = "GL Sub Head 1 " & AccountNameProperty.Trim() & " Successfully Added"
            End If
        End If

        If GL_HeadTypeProperty = 3 Then
            Dim head As String = UCase(HeadGLProperty)
            Dim sub1 As String = UCase(SubHead1Property)
            UtilitiesModule.accountExists(head, sub1, AccountNoProperty.Trim())
            If UtilitiesModule.COA_ACC_Exists = True Then
                Me.LabelErrorMsgLabel.Text = "account Exists "
                Exit Sub
            Else
                QueryString = "INSERT INTO ChartOfAccounts " &
               "(ACC_NO, ACC_TYPE, BAL_TYPE, HEAD_TYPE, ACC_NAME, ACC_DESC, HEAD, SUB1, USER_ID, ADD_DATE, MOD_DATE, DELETED, ALLOW_POST) " &
               "VALUES ('" & AccountNoProperty.Trim() & "','" & AccountTypeProperty.Trim() & "','" & BalanceTypeProperty.Trim() & "','" & GL_HeadTypeProperty.Trim() & "','" & AccountNameProperty.Trim() & "','" & AccountNameProperty.Trim() & "','" & head.Trim() & "','" & sub1.Trim() & "','Lamin', GetDate() , GetDate() ,'N','" & Me.AllowPostingProperty & "')"
                Me.Save(QueryString)
                Me.ResetControls()
                Me._msgSuccessLabel.Text = "GL Sub Head 2 " & AccountNameProperty.Trim() & " Successfully Added"

            End If
        End If

    End Sub


    Sub ResetControls()
        Me.LabelErrorMsgLabel.Text = ""
        Me._msgSuccessLabel.Text = ""
        Me.headGlRow.Visible = False
        Me.existingSubHeadRow.Visible = False
        Me.txtAccountNo.Text = ""
        Me.txtAccountName.Text = ""
        Me.txtAccountNo.ReadOnly = True
        Me.AllowPostingDropDownList.Enabled = False
        Me.HeadGLDropDownList.Enabled = False
        Me.SubHead1DropDownList.Enabled = False
        Me.AccountTypeDropDownList.Enabled = False
        Me.BalanceTypeDropDownList.Enabled = False
        Me.HeadGLDropDownList.SelectedValue = "0"
        Me.SubHead1DropDownList.SelectedValue = "0"
        Me.AccountTypeDropDownList.SelectedIndex = 0
        Me.BalanceTypeDropDownList.SelectedIndex = 0
        Me.GL_HeadTypeDropDownList.SelectedIndex = 0
        Me.GL_HeadTypeDropDownList.Focus()
    End Sub
    Private Sub Save(ByVal sqlQueryStr As String)
        Dim MyDBAction As New DataManagement
        Dim MySmsObject As New DataManagement
        Dim mySqlAction2 As String = ""
        mySqlAction2 = sqlQueryStr.Trim()
        MyDBAction.DBAction(mySqlAction2, DataManagement.DBActionType.Insert)
    End Sub

    Private Sub HeadGLDropDownList_SelectedIndexChanged(sender As Object, e As EventArgs) Handles HeadGLDropDownList.SelectedIndexChanged
        Dim headNo As String = Me.HeadGLDropDownList.SelectedValue

        Dim headTpye As Integer = Nothing
        If headNo = 0 Then
            Me.SubHead1DropDownList.SelectedValue = "0"
            Me.AccountTypeDropDownList.SelectedValue = "0"
            Me.BalanceTypeDropDownList.SelectedValue = "0"
            Exit Sub
        End If

        If headNo = 1 Then
            Me.AccountTypeDropDownList.SelectedValue = "A"
            Me.BalanceTypeDropDownList.SelectedValue = "D"

        ElseIf headNo = 2 Then
            Me.AccountTypeDropDownList.SelectedValue = "L"
            Me.BalanceTypeDropDownList.SelectedValue = "C"
        ElseIf headNo = 3 Then
            Me.AccountTypeDropDownList.SelectedValue = "I"
            Me.BalanceTypeDropDownList.SelectedValue = "C"
        ElseIf headNo = 4 Then

            Me.AccountTypeDropDownList.SelectedValue = "E"
            Me.BalanceTypeDropDownList.SelectedValue = "D"
        End If

        If CInt(GL_HeadTypeProperty) = 3 Then
            Me.LoadSubHead1(Me.SubHead1DropDownList, headNo.Trim())
            Me.SubHead1DropDownList.Enabled = True
        End If
    End Sub

    Private Sub btnReset_Click(sender As Object, e As EventArgs) Handles btnReset.Click
        ResetControls()
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click

    End Sub
End Class