Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Imports System.Data.SqlClient
Public Class ChartOfAccounts_
    Inherits System.Web.UI.Page
    Public Const HEAD = "HEAD"
    Public Const HEAD_TYPE = "HEAD_TYPE"
    Public Const SUB_HEAD_1 = "SUB1"
    Public subhead_ACC_TYPE As String = "", subhead_BAL_TYPE As String = ""
    Public COA_ACC_Exists As Boolean = False

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            Me.LoadGL_Head()
            Me.frmMessage.Text = ""
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
        'Load regions
        Try
            Dim MyDataTable As New Data.DataTable
            Dim MyDBAction As New DataManagement
            MyDataTable = TryCast(MyDBAction.DBAction("SELECT [ACC_NO], [ACC_NAME] FROM [dbo].[ChartOfAccounts] WHERE [HEAD_TYPE]= 1 ", DataManagement.DBActionType.SqlDataTable), Data.DataTable)
            If MyDataTable IsNot Nothing Then
                With HeadGLDropDownList
                    'Me.HeadGLDropDownList
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
                    'Me.HeadGLDropDownList
                    .Items.Clear()
                    .DataSource = MyDataTable
                    .DataTextField = MyDataTable.Columns("ACC_NAME").ToString.Trim
                    .DataValueField = MyDataTable.Columns("ACC_NO").ToString.Trim
                    .DataBind()
                    .Items.Add(New ListItem("Select Account Type", "0"))
                    .SelectedValue = 0
                    '.Items.Add("")
                    '.SelectedValue = ""
                End With
                MyDataTable = Nothing
            End If
        Catch ex As Exception
        End Try
    End Sub
    Private Sub LoadSubHead1(ddlSubhead1 As DropDownList, ByVal headNo As Integer)
        'SELECT [ACC_NO], [ACC_NAME] FROM [dbo].[ChartOfAccounts] WHERE HEAD = headNo And HEAD_TYPE = 2 And DELETED <> "Y"
        Try
            Dim MyDataTable As New Data.DataTable
            Dim MyDBAction As New DataManagement
            MyDataTable = TryCast(MyDBAction.DBAction("SELECT [ACC_NO], [ACC_NAME] FROM [dbo].[ChartOfAccounts] WHERE HEAD = " & headNo & " And HEAD_TYPE = 2 And DELETED <> 'Y'", DataManagement.DBActionType.SqlDataTable), Data.DataTable)
            If MyDataTable IsNot Nothing Then
                With ddlSubhead1
                    'Me.HeadGLDropDownList
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
            'error message
            Exit Sub
        End If

        Me.txtAccountNo.Text = ""
        If headTypeValue = "1" Then ' 1=GL Head
            Me.txtAccountNo.ReadOnly = False
            '  Me.AllowPostingCheckBox.Enabled = True
            Me.AccountTypeDropDownList.Enabled = True

            Me.btnReset.Enabled = True
            ' Me.LoadAvailableHead(HeadDropDownList)
            ' Me.headGlRow.Visible = True
            '  Me.txtAccountNo.Text = CStr(Maintenance.getHeadAcctNo())
        ElseIf headTypeValue = "2" Then '2=Sub Head 1
            Me.txtAccountNo.ReadOnly = False
            'Me.AllowPostingCheckBox.Enabled = True
            Me.HeadGLDropDownList.Enabled = True
            Me.SubHead1DropDownList.Enabled = False
            ' Me.btnSave.Enabled = True
            'Me.txtAccountNo.Text = CStr(Maintenance.getHeadAcctNo())
            Me.btnReset.Enabled = True
        ElseIf headTypeValue = "3" Then '3=Sub Head 2
            Me.txtAccountNo.ReadOnly = False
            'Me.AllowPostingCheckBox.Enabled = True
            Me.HeadGLDropDownList.Enabled = True
            Me.SubHead1DropDownList.Enabled = False

            ' Me.btnSave.Enabled = True
            Me.btnReset.Enabled = True
        End If

    End Sub
    Private Sub HeadDropDownList_SelectedIndexChanged(sender As Object, e As EventArgs) Handles HeadDropDownList.SelectedIndexChanged
        Dim accountNo As String = ""
        Dim subHeadLevel As String = ""
        Dim accountType As String = ""
        Dim balanceType As String = ""
        Me.frmMessage.Text = ""

        Dim headTypeValue As String = Me.GL_HeadTypeDropDownList.SelectedValue

        Dim headNo As String = Me.HeadGLDropDownList.SelectedValue

        subHeadLevel = headTypeValue.Trim()
        accountNo = headNo.Trim()

        If subHeadLevel = "" Or accountNo = "" Then

            Exit Sub
        End If

        If subHeadLevel = 2 Then 'sub 1
            UtilitiesModule.GetSubHeadData("$$", "$$", 1, accountNo.Trim())
        ElseIf subHeadLevel = 3 Then 'sub 2
            UtilitiesModule.GetSubHeadData("$$", "$$", 1, Nothing)
        End If



        '1 - ASSETS - D
        Dim headTpye As Integer = Nothing
        '2 - LIABILITIES - C
        '3 - INCOME - C
        '4 - EXPENSES - D
        If headNo = 0 Then
            Me.SubHead1DropDownList.SelectedValue = "0"
            Me.AccountTypeDropDownList.SelectedValue = "0"
            Me.BalanceTypeDropDownList.SelectedValue = "0"
            Exit Sub
        End If

        Me.AccountTypeDropDownList.SelectedValue = UtilitiesModule.subhead_ACC_TYPE
        Me.BalanceTypeDropDownList.SelectedValue = UtilitiesModule.subhead_BAL_TYPE
        ''1 - ASSETS - D
        'If headNo = 1 Then
        '    Me.AccountTypeDropDownList.SelectedValue = "A"
        '    Me.BalanceTypeDropDownList.SelectedValue = "D"
        '    '  headTpye =
        '    '2 - LIABILITIES - C
        'ElseIf headNo = 2 Then
        '    Me.AccountTypeDropDownList.SelectedValue = "L"
        '    Me.BalanceTypeDropDownList.SelectedValue = "C"
        '    '3 - INCOME - C
        'ElseIf headNo = 3 Then
        '    Me.AccountTypeDropDownList.SelectedValue = "I"
        '    Me.BalanceTypeDropDownList.SelectedValue = "C"
        'ElseIf headNo = 4 Then
        '    '3 - INCOME - C
        '    Me.AccountTypeDropDownList.SelectedValue = "E"
        '    Me.BalanceTypeDropDownList.SelectedValue = "D"
        'End If
        If CInt(GL_HeadTypeProperty) = 2 Then
            'Me.LoadSubHead_1(CInt(GL_HeadTypeProperty), headNo.Trim(), Me.SubHead1DropDownList)
            'Me.SubHead1DropDownList.Enabled = True
            'txtAccountNo.Text = Maintenance.getSub1_Sub2AcctNo(CInt(GL_HeadTypeProperty), CInt(headNo.Trim()), 1)
            'txtAccountNo.Text = Maintenance.getSub1_Sub2AcctNo(CInt(GL_HeadTypeProperty), 10, 1)
        End If
        If CInt(GL_HeadTypeProperty) = 3 Then
            Me.LoadSubHead1(Me.SubHead1DropDownList, headNo.Trim())
            Me.SubHead1DropDownList.Enabled = True
            'txtAccountNo.Text = Maintenance.getSub1_Sub2AcctNo(CInt(GL_HeadTypeProperty), CInt(HeadGLProperty.Trim()), 2, SubHead1Property())
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
        If Not Page.IsPostBack Then
            txtAccountName.Text = "Test"
        End If
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Dim QueryString As String = ""
        COA_ACC_Exists = False
        'Insert-----------
        'insert group details
        'Dim MyDBAction As New DataManagement

        ''waitara check if customer exists
        'Dim MySmsObject As New DataManagement
        'Dim mySearch As String = "select C_CustomerNo from tbCustomer Where C_CustomerNo ='" & txtCustID.Text.Trim & "'"
        'Dim MyReader As SqlDataReader = TryCast(MySmsObject.DBAction(mySearch, DataManagement.DBActionType.SqlDataReader), SqlDataReader)

        'If MyReader.HasRows = True Then
        '    lblGroupName.Text = "This Customer Number Already Exist"
        '    Exit Sub
        'Else
        '    'Label7.Text = ""
        '    'Label4.Text = ""
        'End If
        'MyReader.Close()
        'Dim mySqlAction2 As String = ""
        'mySqlAction2 = "Insert Into tbCustomer (C_CustomerNo,C_CustTypeCode,C_CustomerType,C_FullName,C_Telephone,C_MobileNo,C_Email,C_Address1,C_Address2,C_Address3,C_Region,C_District,C_Area,C_TINNo,B_RegDate,B_RegName,B_NatureofBusiness,B_DirectorName,G_GroupName,G_NoofMembers,G_NoofMale,G_NoofFemale,MakerID,MakerOnDate,CheckerId,CheckedOnDate) VALUES ('" & txtCustID.Text & "','" & drpAccountType.SelectedValue.ToUpper.Trim & "','GROUP ACCOUNT','" & txtFullName.Text.ToUpper.Trim & "','" & txtTelNo.Text.ToUpper.Trim & "','" & txtMobileNo.Text.ToUpper.Trim & "','" & txtEmail.Text.Trim & "','" & txtAddress1.Text.ToUpper.Trim & "','" & txtAddress2.Text.ToUpper.Trim & "','" & txtAddress3.Text.ToUpper.Trim & "','" & drpdRegion.SelectedValue.ToUpper.Trim & "','" & drpdDistrict.SelectedValue.ToUpper.Trim & "','" & drpdArea.SelectedValue.ToUpper.Trim & "','" & txtTillNo.Text.ToUpper.Trim & "','" & txtgrpRegDate.Text.ToUpper.Trim & "','" & txtgrpRegNo.Text.ToUpper.Trim & "','" & txtgrpBizNature.Text.ToUpper.Trim & "','" & txtgrpDirName.Text.ToUpper.Trim & "','" & txtgrpName.Text.ToUpper.Trim & "','" & txtgrpMemberNo.Text.ToUpper.Trim & "' , '" & txtMaleNo.Text.ToUpper.Trim & "' , '" & txtFemaleNo.Text.ToUpper.Trim & "','WAITARA','" & Date.Today.ToString("dd-MMM-yyyy") & "','WAITARA','" & Date.Today.ToString("dd-MMM-yyyy") & "')"
        'MyDBAction.DBAction(mySqlAction2, DataManagement.DBActionType.Insert)
        'End Insert

        '1. Validate
        ' Me.Messagecell.Text = ""
        'Me.ErrorMessageDiv.Visible = False
        ' Me.SuccessMessageDiv.Visible = False
        'Me.AcctNoError.Text = ""
        'Me.AcctNoError.Text = "* Account No Exists"
        'Me.AcctNoError.ForeColor = Drawing.Color.Red
        '1 delete after
        'If GL_HeadTypeProperty <> 1 And Len(AccountNoProperty) < 2 Then
        '    Me.AcctNoError.Text = "* Two Digits required"
        '    Exit Sub
        'End If
        'If GL_HeadTypeProperty = 1 And Len(AccountNoProperty) > 1 Then
        '    Me.AcctNoError.Text = "* Should Be 1 Digit"
        '    Exit Sub
        'End If

        'If IsNumeric(AccountNoProperty) = False Then
        '    Me.AcctNoError.Text = "* Should Be Numeric"
        '    Exit Sub
        'End If
        ' Adding  HEAD GL: 1 | Assets

        If GL_HeadTypeProperty = 1 Then
            Dim head As String = "$$"
            Dim sub1 As String = "$$"

            UtilitiesModule.accountExists(head, sub1, AccountNoProperty.Trim())
            If UtilitiesModule.COA_ACC_Exists = True Then
                Me.frmMessage.Text = "account Exists "
                Exit Sub
            Else
                QueryString = "INSERT INTO ChartOfAccounts " &
                    "(ACC_NO, ACC_TYPE, BAL_TYPE, HEAD_TYPE, ACC_NAME, ACC_DESC, HEAD, SUB1, USER_ID, ADD_DATE, MOD_DATE, DELETED, ALLOW_POST) " &
                    "VALUES ('" & AccountNoProperty.Trim() & "','" & AccountTypeProperty.Trim() & "','" & BalanceTypeProperty.Trim() & "','" & GL_HeadTypeProperty.Trim() & "','" & AccountNameProperty.Trim() & "','" & AccountNameProperty.Trim() & "','" & head.Trim() & "','" & sub1.Trim() & "','Lamin', GetDate() , GetDate() ,'N','" & Me.AllowPostingProperty & "')"
                Me.Save(QueryString)
                Me.LoadGL_Head()
                Me.ResetControls()
                Me.frmMessage.Text = "GL Head " & AccountNameProperty.Trim() & " Successfully Added"

                '  Me.ResetControls()
            End If
        End If
        If GL_HeadTypeProperty = 2 Then
            Dim head As String = UCase(HeadGLProperty)
            Dim sub1 As String = "$$"
            UtilitiesModule.accountExists(head, sub1, AccountNoProperty.Trim())
            If UtilitiesModule.COA_ACC_Exists = True Then
                Me.frmMessage.Text = "account Exists "
                Exit Sub
            Else
                QueryString = "INSERT INTO ChartOfAccounts " &
                "(ACC_NO, ACC_TYPE, BAL_TYPE, HEAD_TYPE, ACC_NAME, ACC_DESC, HEAD, SUB1, USER_ID, ADD_DATE, MOD_DATE, DELETED, ALLOW_POST) " &
                "VALUES ('" & AccountNoProperty.Trim() & "','" & AccountTypeProperty.Trim() & "','" & BalanceTypeProperty.Trim() & "','" & GL_HeadTypeProperty.Trim() & "','" & AccountNameProperty.Trim() & "','" & AccountNameProperty.Trim() & "','" & head.Trim() & "','" & sub1.Trim() & "','Lamin', GetDate() , GetDate() ,'N','" & Me.AllowPostingProperty & "')"
                Me.Save(QueryString)
                Me.ResetControls()
                Me.frmMessage.Text = "GL Sub Head 1 " & AccountNameProperty.Trim() & " Successfully Added"

                'Me.ResetControls()
            End If
        End If

        If GL_HeadTypeProperty = 3 Then
            Dim head As String = UCase(HeadGLProperty)
            Dim sub1 As String = UCase(SubHead1Property)
            UtilitiesModule.accountExists(head, sub1, AccountNoProperty.Trim())
            If UtilitiesModule.COA_ACC_Exists = True Then
                Me.frmMessage.Text = "account Exists "
                Exit Sub
            Else
                QueryString = "INSERT INTO ChartOfAccounts " &
               "(ACC_NO, ACC_TYPE, BAL_TYPE, HEAD_TYPE, ACC_NAME, ACC_DESC, HEAD, SUB1, USER_ID, ADD_DATE, MOD_DATE, DELETED, ALLOW_POST) " &
               "VALUES ('" & AccountNoProperty.Trim() & "','" & AccountTypeProperty.Trim() & "','" & BalanceTypeProperty.Trim() & "','" & GL_HeadTypeProperty.Trim() & "','" & AccountNameProperty.Trim() & "','" & AccountNameProperty.Trim() & "','" & head.Trim() & "','" & sub1.Trim() & "','Lamin', GetDate() , GetDate() ,'N','" & Me.AllowPostingProperty & "')"
                Me.Save(QueryString)
                Me.ResetControls()
                Me.frmMessage.Text = "GL Sub Head 2 " & AccountNameProperty.Trim() & " Successfully Added"

                ' Me.ResetControls()
            End If
        End If
        'If GL_HeadTypeProperty = 1 Then
        '    If Maintenance.DoesGL_HeadExist(AccountNoProperty, GL_HeadTypeProperty) = False Then
        '        Dim head As String = "$$"
        '        Dim sub1 As String = "$$"
        '        QueryString = "INSERT INTO ChartOfAccounts " &
        '        "(ACC_NO, ACC_TYPE, BAL_TYPE, HEAD_TYPE, ACC_NAME, ACC_DESC, HEAD, SUB1, USER_ID, ADD_DATE, MOD_DATE, DELETED, ALLOW_POST) " &
        '        "VALUES ('" & AccountNoProperty.Trim() & "','" & AccountTypeProperty.Trim() & "','" & BalanceTypeProperty.Trim() & "','" & GL_HeadTypeProperty.Trim() & "','" & AccountNameProperty.Trim() & "','" & AccountNameProperty.Trim() & "','" & head.Trim() & "','" & sub1.Trim() & "','Lamin', GetDate() , GetDate() ,'N','" & Me.AllowPostingProperty.Trim() & "')"

        '        If Maintenance.AddNewAccountHead(AccountNoProperty, AccountTypeProperty, BalanceTypeProperty, GL_HeadTypeProperty, AccountNameProperty,
        '                                           AccountNameProperty, head, sub1, Session("userName").trim(), DateTime.Now(), DateTime.Now(),
        '                                           "N", AllowPostingProperty) = True Then
        '            Me.GL_HeadTypeDropDownList.SelectedValue = "0"
        '            Me.ResetAdd_AccountHeadControls()
        '            ' Me.SuccessMessageDiv.Visible = True
        '            Me.Messagecell.Text = "Account Successfully Added"
        '            Me.Messagecell.ForeColor = Drawing.Color.Blue
        '            'Dim message As String = "Account Successfully Added!"
        '            'ScriptManager.RegisterClientScriptBlock(Page, Me.GetType(), "Popup", "ShowPopup('" + message + "');", True)

        '        Else
        '            ' Me.ErrorMessageDiv.Visible = True
        '            Me.Messagecell.Text = "Addition Failed"
        '            Me.Messagecell.ForeColor = Drawing.Color.Red
        '            'Dim message As String = "Addition Failed!"
        '            'ScriptManager.RegisterClientScriptBlock(Page, Me.GetType(), "Popup", "ShowPopup('" + message + "');", True)

        '        End If
        '    Else
        '        Me.AcctNoError.Text = "* Account No Exists"
        '        ' Me.InfoMessageDiv.Visible = True
        '        Me.Messagecell.Text = "Cannot Add: This Account Already Exists"
        '        Me.Messagecell.ForeColor = Drawing.Color.Red
        '        'Dim message As String = "Cannot Add: This Account Already Exists!"
        '        'ScriptManager.RegisterClientScriptBlock(Page, Me.GetType(), "Popup", "ShowPopup('" + message + "');", True)

        '    End If

        '    ' Adding  SUB HEAD 1: 01 | Fixed Assets
        'ElseIf GL_HeadTypeProperty = 2 Then
        '    If Maintenance.DoesSubHead1Exist(AccountNoProperty, GL_HeadTypeProperty, HeadGLProperty) = False Then
        '        Dim head As String = UCase(HeadGLProperty)
        '        Dim sub1 As String = "$$"
        '        If Maintenance.AddNewAccountHead(AccountNoProperty, AccountTypeProperty, BalanceTypeProperty, GL_HeadTypeProperty, AccountNameProperty,
        '                                           AccountNameProperty, head, sub1, Session("userName").trim(), DateTime.Now(), DateTime.Now(),
        '                                           "N", AllowPostingProperty) = True Then
        '            Me.GL_HeadTypeDropDownList.SelectedValue = "0"
        '            Me.ResetAdd_AccountHeadControls()

        '            Me.Messagecell.Text = "Account Successfully Added"
        '            Me.Messagecell.ForeColor = Drawing.Color.Blue
        '            'Dim message As String = "Account Successfully Added!"
        '            'ScriptManager.RegisterClientScriptBlock(Page, Me.GetType(), "Popup", "ShowPopup('" + message + "');", True)

        '        Else
        '            Me.Messagecell.Text = "Addition Failed"
        '            Me.Messagecell.ForeColor = Drawing.Color.Red
        '            '   Me.ErrorMessageDiv.Visible = True
        '            'Me.ErrorMessageDiv.InnerText = "Addition Failed"
        '            'Dim message As String = "Addition Failed!"
        '            'ScriptManager.RegisterClientScriptBlock(Page, Me.GetType(), "Popup", "ShowPopup('" + message + "');", True)

        '        End If
        '    Else
        '        Me.Messagecell.Text = "Cannot Add: This Account Already Exists"
        '        Me.Messagecell.ForeColor = Drawing.Color.Red
        '        ' Me.InfoMessageDiv.Visible = True
        '        ' Me.AcctNoError.Text = "* Account No Exists"
        '        ' Me.InfoMessageDiv.InnerText = "Cannot Add: This Account Already Exists"
        '        'Dim message As String = "Cannot Add: This Account Already Exists!"
        '        'ScriptManager.RegisterClientScriptBlock(Page, Me.GetType(), "Popup", "ShowPopup('" + message + "');", True)

        '    End If

        '    'Adding SUB HEAD 2: 01 | Computer & Accessories 
        '    ' GL accounts Are Added under Sub Head 2: here a 3 digit no is required
        'ElseIf GL_HeadTypeProperty = 3 Then
        '    If Maintenance.DoesSubHead2Exist(AccountNoProperty, HeadGLProperty, GL_HeadTypeProperty, SubHead1Property) = False Then
        '        Dim head As String = UCase(HeadGLProperty)
        '        Dim sub1 As String = UCase(SubHead1Property)
        '        If Maintenance.AddNewAccountHead(AccountNoProperty, AccountTypeProperty, BalanceTypeProperty, GL_HeadTypeProperty, AccountNameProperty,
        '                                           AccountNameProperty, head, sub1, Session("userName").trim(), DateTime.Now(), DateTime.Now(),
        '                                           "N", AllowPostingProperty) = True Then
        '            Me.GL_HeadTypeDropDownList.SelectedValue = "0"
        '            Me.ResetAdd_AccountHeadControls()
        '            Me.Messagecell.Text = "Account Successfully Added"
        '            Me.Messagecell.ForeColor = Drawing.Color.Blue
        '            ' Me.SuccessMessageDiv.Visible = True
        '            ' Me.SuccessMessageDiv.InnerText = "Account Successfully Added"
        '            'Dim message As String = "Account Successfully Added!"
        '            'ScriptManager.RegisterClientScriptBlock(Page, Me.GetType(), "Popup", "ShowPopup('" + message + "');", True)

        '        Else
        '            Me.Messagecell.Text = "Addition Failed"
        '            Me.Messagecell.ForeColor = Drawing.Color.Red
        '            '  Me.ErrorMessageDiv.Visible = True
        '            'Me.ErrorMessageDiv.InnerText = "Addition Failed"
        '            'Dim message As String = "Addition Failed!"
        '            'ScriptManager.RegisterClientScriptBlock(Page, Me.GetType(), "Popup", "ShowPopup('" + message + "');", True)

        '        End If
        '    Else
        '        Me.Messagecell.Text = "Cannot Add: This Account Already Exists"
        '        Me.Messagecell.ForeColor = Drawing.Color.Red
        '        '  Me.InfoMessageDiv.Visible = True
        '        '  Me.AcctNoError.Text = "* Account No Exists"
        '        'Me.InfoMessageDiv.InnerText = "Cannot Add: This Account Already Exists"
        '        Dim message As String = "Cannot Add: This Account Already Exists!"
        '        ScriptManager.RegisterClientScriptBlock(Page, Me.GetType(), "Popup", "ShowPopup('" + message + "');", True)

        '    End If
        '    ' End If

        'End If
    End Sub


    Sub ResetControls()
        Me.frmMessage.Text = ""
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
        '"Insert Into tbCustomer (C_CustomerNo,C_CustTypeCode,C_CustomerType,C_FullName,C_Telephone,C_MobileNo,C_Email,C_Address1,C_Address2,C_Address3,C_Region,C_District,C_Area,C_TINNo,B_RegDate,B_RegName,B_NatureofBusiness,B_DirectorName,G_GroupName,G_NoofMembers,G_NoofMale,G_NoofFemale,MakerID,MakerOnDate,CheckerId,CheckedOnDate) VALUES ('" & txtCustID.Text & "','" & drpAccountType.SelectedValue.ToUpper.Trim & "','GROUP ACCOUNT','" & txtFullName.Text.ToUpper.Trim & "','" & txtTelNo.Text.ToUpper.Trim & "','" & txtMobileNo.Text.ToUpper.Trim & "','" & txtEmail.Text.Trim & "','" & txtAddress1.Text.ToUpper.Trim & "','" & txtAddress2.Text.ToUpper.Trim & "','" & txtAddress3.Text.ToUpper.Trim & "','" & drpdRegion.SelectedValue.ToUpper.Trim & "','" & drpdDistrict.SelectedValue.ToUpper.Trim & "','" & drpdArea.SelectedValue.ToUpper.Trim & "','" & txtTillNo.Text.ToUpper.Trim & "','" & txtgrpRegDate.Text.ToUpper.Trim & "','" & txtgrpRegNo.Text.ToUpper.Trim & "','" & txtgrpBizNature.Text.ToUpper.Trim & "','" & txtgrpDirName.Text.ToUpper.Trim & "','" & txtgrpName.Text.ToUpper.Trim & "','" & txtgrpMemberNo.Text.ToUpper.Trim & "' , '" & txtMaleNo.Text.ToUpper.Trim & "' , '" & txtFemaleNo.Text.ToUpper.Trim & "','WAITARA','" & Date.Today.ToString("dd-MMM-yyyy") & "','WAITARA','" & Date.Today.ToString("dd-MMM-yyyy") & "')"
        MyDBAction.DBAction(mySqlAction2, DataManagement.DBActionType.Insert)

    End Sub

    Private Sub HeadGLDropDownList_SelectedIndexChanged(sender As Object, e As EventArgs) Handles HeadGLDropDownList.SelectedIndexChanged
        Dim headNo As String = Me.HeadGLDropDownList.SelectedValue

        '1 - ASSETS - D
        Dim headTpye As Integer = Nothing
        '2 - LIABILITIES - C
        '3 - INCOME - C
        '4 - EXPENSES - D
        If headNo = 0 Then
            Me.SubHead1DropDownList.SelectedValue = "0"
            Me.AccountTypeDropDownList.SelectedValue = "0"
            Me.BalanceTypeDropDownList.SelectedValue = "0"
            Exit Sub
        End If
        '1 - ASSETS - D
        If headNo = 1 Then
            Me.AccountTypeDropDownList.SelectedValue = "A"
            Me.BalanceTypeDropDownList.SelectedValue = "D"
            '  headTpye =
            '2 - LIABILITIES - C
        ElseIf headNo = 2 Then
            Me.AccountTypeDropDownList.SelectedValue = "L"
            Me.BalanceTypeDropDownList.SelectedValue = "C"
            '3 - INCOME - C
        ElseIf headNo = 3 Then
            Me.AccountTypeDropDownList.SelectedValue = "I"
            Me.BalanceTypeDropDownList.SelectedValue = "C"
        ElseIf headNo = 4 Then
            '3 - INCOME - C
            Me.AccountTypeDropDownList.SelectedValue = "E"
            Me.BalanceTypeDropDownList.SelectedValue = "D"
        End If
        'If CInt(GL_HeadTypeProperty) = 2 Then
        '    Me.LoadSubHead_1(CInt(GL_HeadTypeProperty), headNo.Trim(), Me.SubHead1DropDownList)
        '    Me.SubHead1DropDownList.Enabled = True
        '    ' txtAccountNo.Text = Maintenance.getSub1_Sub2AcctNo(CInt(GL_HeadTypeProperty), CInt(headNo.Trim()), 1)
        '    'txtAccountNo.Text = Maintenance.getSub1_Sub2AcctNo(CInt(GL_HeadTypeProperty), 10, 1)
        'End If
        If CInt(GL_HeadTypeProperty) = 3 Then
            Me.LoadSubHead1(Me.SubHead1DropDownList, headNo.Trim())
            Me.SubHead1DropDownList.Enabled = True
            'txtAccountNo.Text = Maintenance.getSub1_Sub2AcctNo(CInt(GL_HeadTypeProperty), CInt(HeadGLProperty.Trim()), 2, SubHead1Property())
        End If
    End Sub

    Private Sub btnReset_Click(sender As Object, e As EventArgs) Handles btnReset.Click
        ResetControls()
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click

    End Sub
End Class