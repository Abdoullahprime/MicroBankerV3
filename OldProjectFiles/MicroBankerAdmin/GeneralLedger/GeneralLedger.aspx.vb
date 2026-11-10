Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Imports System.Data.SqlClient
Public Class GeneralLedger
    Inherits System.Web.UI.Page
    Dim _accountID As String
    Public GLExists As Boolean = False
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            Me.LoadHeadDropDownList()
            ResetGlForm()
            'Me.LoadCurrencies(Me.currencyDropDownList)
        End If
    End Sub

    Public Property HeadProperty() As String
        Get
            Return UCase(Me.HeadDropDownList.SelectedValue.Trim())
        End Get
        Set(ByVal value As String)
            Me.HeadDropDownList.SelectedValue = value
        End Set
    End Property


    Public Property AccountNoPart_1_Property() As String
        Get
            Return Me.txtAccountNoPart1.Text.Trim()
        End Get
        Set(ByVal value As String)
            Me.txtAccountNoPart1.Text = value
        End Set
    End Property

    Public Property AccountNoPart_2_Property() As String
        Get
            Return Me.txtAccountNoPart2.Text.Trim()
        End Get
        Set(ByVal value As String)
            Me.txtAccountNoPart2.Text = value
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
    Public Property CurrencyProperty() As String
        Get
            Return UCase(Me.currencyDropDownList.SelectedValue.Trim())
        End Get
        Set(ByVal value As String)
            Me.currencyDropDownList.SelectedValue = value
        End Set
    End Property
    Public Property TypeProperty() As String
        Get
            Return UCase(Me.TypeDropDownList.SelectedValue.Trim())
        End Get
        Set(ByVal value As String)
            Me.TypeDropDownList.SelectedValue = value
        End Set
    End Property
    Public Property BalanceTypeProperty() As String
        Get
            Return UCase(Me.BalanceTypeDropDownList.SelectedValue.Trim())
        End Get
        Set(ByVal value As String)
            Me.BalanceTypeDropDownList.SelectedValue = value
        End Set
    End Property
    Public Property StatusProperty() As String
        Get
            Return UCase(Me.StatusDropDownList.SelectedValue.Trim())
        End Get
        Set(ByVal value As String)
            Me.StatusDropDownList.SelectedValue = value
        End Set
    End Property
    Public Property IsBankAccountProperty() As String
        Get
            Return UCase(Me.IsBankAccountDropdownList.SelectedValue.Trim())
        End Get
        Set(ByVal value As String)
            Me.IsBankAccountDropdownList.SelectedValue = value
        End Set
    End Property
    Public Property IsControlProperty() As String
        Get
            Return UCase(Me.IsControlDropDownList.SelectedValue.Trim())
        End Get
        Set(ByVal value As String)
            Me.IsControlDropDownList.SelectedValue = value
        End Set
    End Property

    Public Sub LoadHeadDropDownList()
        'Load regions
        Try
            Dim acctNo As String = ""
            Dim acctName As String = ""
            Dim MyDataTable As New Data.DataTable
            Dim MyDBAction As New DataManagement
            MyDataTable = TryCast(MyDBAction.DBAction("select RTRIM(HEAD)+SUB1+ACC_NO + '-' + ACC_NAME as ACC_NO,ACC_NAME from ChartOfAccounts where  HEAD_TYPE=3 AND Deleted = 'N' order by ACC_NO", DataManagement.DBActionType.SqlDataTable), Data.DataTable)
            If MyDataTable IsNot Nothing Then
                With HeadDropDownList
                    'Me.HeadGLDropDownList
                    .Items.Clear()
                    .DataSource = MyDataTable
                    acctNo = MyDataTable.Columns("ACC_NO").ToString.Trim
                    acctName = MyDataTable.Columns("ACC_NAME").ToString.Trim
                    .DataTextField = MyDataTable.Columns("ACC_NO").ToString.Trim
                    .DataValueField = MyDataTable.Columns("ACC_NO").ToString.Trim
                    .DataBind()
                    .Items.Add(New ListItem("Select...", "0"))
                    .SelectedValue = 0
                    'ddlTypeDropDown.Items.Add(New ListItem("Select...", "0"))
                End With
                MyDataTable = Nothing
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub HeadDropDownList_SelectedIndexChanged(sender As Object, e As EventArgs) Handles HeadDropDownList.SelectedIndexChanged
        Me.AccountNameMsgLabel.Visible = False
        Me._msgLabel.Visible = False
        Me._msgSuccessLabel.Text = ""
        Me.lblAcctNoError.Text = ""
        Dim accountID As String = Me.HeadDropDownList.Text.Trim()
        accountID = accountID.Substring(0, 5)
        Me.txtAccountNoPart1.Text = accountID
        Dim _glNoSuffix As String = Nothing

        Dim head As String = accountID.Substring(0, 1)
        Dim sub1 As String = accountID.Substring(1, 2)
        Dim acctNo As String = accountID.Substring(3, 2)
        Me.currentGLRow.Visible = False
        'Me.LoadCurrentGLDropDownList(CurrentGLDropDownList, accountID.Trim())


        'Dim glNoSuffix As GL_DETAIL = Maintenance.GetNextGLAcctNoSuffix(accountID.Trim())
        'If glNoSuffix IsNot Nothing Then
        '    _glNoSuffix = CStr(CInt(glNoSuffix.GLACCTNO) + 1)
        '    Me.txtAccountNoPart2.Text = _glNoSuffix.Substring(5, 3)
        'Else
        '    _glNoSuffix = "1"
        '    _glNoSuffix = _glNoSuffix.PadLeft(3, "0")
        'End If
        UtilitiesModule.GetGL_COA_DETAILS(head, sub1, acctNo)

        'Dim accountInfo = Maintenance.GetGL_COA_Info(head, sub1, acctNo)
        ' Me.TypeDropDownList.SelectedValue = accountInfo(0).ACC_TYPE
        Me.TypeDropDownList.Text = UtilitiesModule.gl_ACC_TYPE
        Me.BalanceTypeDropDownList.Text = UtilitiesModule.gl_BAL_TYPE
        'Me.BalanceTypeDropDownList.SelectedValue = accountInfo(0).BAL_TYPE
        Me.txtAccountNoPart2.Focus()


    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        ' Me.msgLabel.Text = ""
        ' Validate
        Me.saveNewGL()
    End Sub
    Private Sub saveNewGL()
        Dim MyDBAction As New DataManagement
        Dim MySmsObject As New DataManagement
        Me.lblAcctNoError.Text = ""
        Dim accountNo As String = ""
        Dim accountName As String = ""
        GLExists = False
        accountNo = AccountNoPart_1_Property + AccountNoPart_2_Property
        accountName = AccountNameProperty
        Dim sqlQueryStr As String = ""
        Dim sqlQueryStr1 As String = ""
        If AccountNoPart_1_Property = "" Then
            Me.AccountNameMsgLabel.Visible = False
            Me._msgLabel.Visible = True
            'Me._msgLabel.Visible = True
            ' Me._msgLabel.ForeColor = Drawing.Color.Red
            'Me._msgLabel.Text = "Account Number Is Not Well Formed"
            Exit Sub
        End If
        If AccountNoPart_2_Property = "" Then
            Me.AccountNameMsgLabel.Visible = False
            Me._msgLabel.Visible = True
            ' Me._msgLabel.Visible = True
            ' Me._msgLabel.ForeColor = Drawing.Color.Red
            'Me._msgLabel.Text = "Account Number Is Not Well Formed"
            Exit Sub
        End If
        If accountNo = "" Then
            Me.AccountNameMsgLabel.Visible = False
            Me._msgLabel.Visible = True
            'Me._msgLabel.Visible = True
            'Me._msgLabel.ForeColor = Drawing.Color.Red
            'Me._msgLabel.Text = "Account Number Is Not Well Formed"
            Exit Sub
        End If
        If accountName = "" Then
            Me.AccountNameMsgLabel.Visible = True
            Me._msgLabel.Visible = False
            'Me._msgLabel.Visible = True
            'Me._msgLabel.ForeColor = Drawing.Color.Red
            'Me._msgLabel.Text = "Account Number Is Not Well Formed"
            Exit Sub
        End If

        UtilitiesModule.GL_Exists(accountNo.Trim())

        If UtilitiesModule.GLExists = True Then

            Me.lblAcctNoError.Text = "This GL Account Exists"
        Else
            Me.lblAcctNoError.Text = ""
            sqlQueryStr = "INSERT INTO [dbo].[GL_DETAIL] ([GLACCTNO],[GLNAME],[BALTYPE],[ACCTTYPE],[STATUS],[DELETED],[CCY],[IsBankAccount],[IsControl_GL],[LAST_ACTIVITY],[USER_ID],[OPENDATE],[CURRENTBAL],[ACT_BAL],[LCY_BAL],[EOMBAL],[BUDGET],[YTD_BAL],[PYTD_BAL],[TOT_BUDGET]) " &
                      "VALUES " &
                      "('" & accountNo.Trim() & "','" & accountName.Trim() & "','" & BalanceTypeProperty & "','" & TypeProperty & "','" & StatusProperty & "','N','" & CurrencyProperty & "','" & IsBankAccountProperty & "','" & IsControlProperty & "',GetDate(),'Lamin',GetDate(),'0.0','0.0','0.0','0.0','0.0','0.0','0.0','0.0')"
            'mySqlAction2 = sqlQueryStr.Trim()
            '"Insert Into tbCustomer (C_CustomerNo,C_CustTypeCode,C_CustomerType,C_FullName,C_Telephone,C_MobileNo,C_Email,C_Address1,C_Address2,C_Address3,C_Region,C_District,C_Area,C_TINNo,B_RegDate,B_RegName,B_NatureofBusiness,B_DirectorName,G_GroupName,G_NoofMembers,G_NoofMale,G_NoofFemale,MakerID,MakerOnDate,CheckerId,CheckedOnDate) VALUES ('" & txtCustID.Text & "','" & drpAccountType.SelectedValue.ToUpper.Trim & "','GROUP ACCOUNT','" & txtFullName.Text.ToUpper.Trim & "','" & txtTelNo.Text.ToUpper.Trim & "','" & txtMobileNo.Text.ToUpper.Trim & "','" & txtEmail.Text.Trim & "','" & txtAddress1.Text.ToUpper.Trim & "','" & txtAddress2.Text.ToUpper.Trim & "','" & txtAddress3.Text.ToUpper.Trim & "','" & drpdRegion.SelectedValue.ToUpper.Trim & "','" & drpdDistrict.SelectedValue.ToUpper.Trim & "','" & drpdArea.SelectedValue.ToUpper.Trim & "','" & txtTillNo.Text.ToUpper.Trim & "','" & txtgrpRegDate.Text.ToUpper.Trim & "','" & txtgrpRegNo.Text.ToUpper.Trim & "','" & txtgrpBizNature.Text.ToUpper.Trim & "','" & txtgrpDirName.Text.ToUpper.Trim & "','" & txtgrpName.Text.ToUpper.Trim & "','" & txtgrpMemberNo.Text.ToUpper.Trim & "' , '" & txtMaleNo.Text.ToUpper.Trim & "' , '" & txtFemaleNo.Text.ToUpper.Trim & "','WAITARA','" & Date.Today.ToString("dd-MMM-yyyy") & "','WAITARA','" & Date.Today.ToString("dd-MMM-yyyy") & "')"
            MyDBAction.DBAction(sqlQueryStr, DataManagement.DBActionType.Insert)
            ' (<GLACCTNO, nchar(12),>,<GLNAME, char(30),>,<BAL_DATE, smalldatetime,>,<OPEN_BAL, numeric(18,3),>,<CLOSE_BAL, numeric(18,3),>,<OPEN_BAL_LCY, numeric(18,3),>,<CLOSE_BAL_LCY, numeric(18,3),>)
            sqlQueryStr1 = "INSERT INTO [dbo].[GL_STAT_BAL] ([GLACCTNO],[GLNAME],[BAL_DATE],[OPEN_BAL],[CLOSE_BAL],[OPEN_BAL_LCY],[CLOSE_BAL_LCY]) " &
                           "VALUES " &
                           "('" & accountNo.Trim() & "','" & AccountNameProperty & "',GetDate(),'0.0','0.0','0.0','0.0')"
            MyDBAction.DBAction(sqlQueryStr1, DataManagement.DBActionType.Insert)
            Me.AccountNameMsgLabel.Visible = True
            ResetGlForm()
            Me._msgSuccessLabel.ForeColor = Drawing.Color.Green
            Me._msgSuccessLabel.Text = "GL Account: " & accountNo.Trim() & " " & accountName.Trim().Trim() & " Added"

        End If
    End Sub
    Private Sub ResetGlForm()
        Me.currentGLRow.Visible = False
        Me.HeadDropDownList.SelectedValue = "0"
        Me.txtAccountNoPart1.Text = ""
        Me.txtAccountNoPart2.Text = ""
        Me.txtAccountName.Text = ""
        'Me.currencyDropDownList.SelectedValue = "0"
        Me.TypeDropDownList.SelectedValue = "0"
        Me.BalanceTypeDropDownList.SelectedValue = "0"
        Me.StatusDropDownList.SelectedValue = "O"
        Me.IsBankAccountDropdownList.SelectedValue = "N"
        Me.IsControlDropDownList.SelectedValue = "N"
        Me.MessageCell.Text = ""
        Me.lblAcctNoError.Text = ""
        Me.AccountNameMsgLabel.Visible = False
        Me._msgLabel.Visible = False
        Me._msgSuccessLabel.Text = ""
    End Sub

    Private Sub btnReset_Click(sender As Object, e As EventArgs) Handles btnReset.Click
        Me.ResetGlForm()
    End Sub
End Class