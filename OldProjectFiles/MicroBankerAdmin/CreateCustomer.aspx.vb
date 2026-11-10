Imports System.Data.SqlClient
Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Public Class CreateCustomer

    Inherits System.Web.UI.Page
    Dim pCustNo As String = ""
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            'preloads parameters
            GetRegions()
            GetDistrict()
            GetArea()
        End If
    End Sub
    Public Sub GetRegions()
        'Load regions
        Try
            Dim MyDataTable As New Data.DataTable
            Dim MyDBAction As New DataManagement
            MyDataTable = TryCast(MyDBAction.DBAction(" Select RegionCode,RegionName from Region", DataManagement.DBActionType.SqlDataTable), Data.DataTable)
            If MyDataTable IsNot Nothing Then
                With drpdRegion
                    .Items.Clear()
                    .DataSource = MyDataTable
                    .DataTextField = MyDataTable.Columns("RegionName").ToString.Trim
                    .DataValueField = MyDataTable.Columns("RegionCode").ToString.Trim
                    .DataBind()
                    .Items.Add("")
                    .SelectedValue = ""
                End With
                MyDataTable = Nothing
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub GetDistrict()
        'Load Districts
        Try
            Dim MyDataTable As New Data.DataTable
            Dim MyDBAction As New DataManagement
            MyDataTable = TryCast(MyDBAction.DBAction("select DistrictCode,DistrictName from District", DataManagement.DBActionType.SqlDataTable), Data.DataTable)
            If MyDataTable IsNot Nothing Then
                With drpdDistrict
                    .Items.Clear()
                    .DataSource = MyDataTable
                    .DataTextField = MyDataTable.Columns("DistrictName").ToString.Trim
                    .DataValueField = MyDataTable.Columns("DistrictCode").ToString.Trim
                    .DataBind()
                    .Items.Add("")
                    .SelectedValue = ""
                End With
                MyDataTable = Nothing
            End If
        Catch ex As Exception
        End Try
    End Sub
    Private Sub GetArea()
        'Load Area
        Try
            Dim MyDataTable As New Data.DataTable
            Dim MyDBAction As New DataManagement
            MyDataTable = TryCast(MyDBAction.DBAction("select AreaCode,AreaName from Area", DataManagement.DBActionType.SqlDataTable), Data.DataTable)
            If MyDataTable IsNot Nothing Then
                With drpdArea
                    .Items.Clear()
                    .DataSource = MyDataTable
                    .DataTextField = MyDataTable.Columns("AreaName").ToString.Trim
                    .DataValueField = MyDataTable.Columns("AreaCode").ToString.Trim
                    .DataBind()
                    .Items.Add("")
                    .SelectedValue = ""
                End With
                MyDataTable = Nothing
            End If
        Catch ex As Exception
        End Try
    End Sub

    Function CheckIsNumber(ByVal a As String)
        'Checks if value provided is number only
        Dim cow As Boolean = False
        Dim ContainsAlpha As Boolean
        Dim ContainsNumeric As Boolean
        Dim ContainsInvalidCharacter As Boolean
        For Each Character As Char In a
            If Char.IsNumber(Character) Then
                cow = True
                ContainsNumeric = True
            ElseIf Char.IsLetter(Character) Then
                ContainsAlpha = True
            ElseIf Not Char.IsWhiteSpace(Character) Then
                ContainsInvalidCharacter = True
                Exit For
            End If
        Next
        Return cow
    End Function
    Function EmailAddressCheck(ByVal emailAddress As String) As Boolean
        'Check validity of email provided
        Dim pattern As String = "^[a-zA-Z][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$"
        Dim emailAddressMatch As Match = Regex.Match(emailAddress, pattern)
        If emailAddressMatch.Success Then
            EmailAddressCheck = True
        Else
            EmailAddressCheck = False
        End If

    End Function

    Public Sub btnSaveIndividual_Click(sender As Object, e As EventArgs) Handles btnSaveIndividual.Click
        'insert individual customer creation
        Dim MyDBAction As New DataManagement
        Dim mycustID As String = txtCustID.Text
        If drpAccountType.Text = "" Then
            lblAccountType.Text = "Select Account Type"
            Exit Sub
        Else
            lblAccountType.Text = ""
        End If
        If txtCustID.Text = "" Then
            lblCustIDs.Text = "Please Enter Customer Number"
            Exit Sub
        Else
            lblAccountType.Text = ""
        End If
        If IsNumeric(txtCustID.Text) Then
            lblCustIDs.Text = ""
        Else
            lblCustIDs.Text = "Customer Number must be numeric"
            Exit Sub
        End If
        If txtFullName.Text = "" Then
            lblFullName.Text = "Please Enter Full Name"
            Exit Sub
        Else
            lblFullName.Text = ""
        End If
        If txtAddress1.Text = "" Then
            lblAdress1.Text = "Please Enter Address 1"
            Exit Sub
        Else
            lblAdress1.Text = ""
        End If
        If txtEmail.Text <> "" Then
            If EmailAddressCheck(txtEmail.Text) = True Then
                lblEmail.Text = ""
            Else
                lblEmail.Text = "Enter Correct Email Address"
                Exit Sub
            End If
        End If
        If txtTillNo.Text = "" Then
            lblTinNo.Text = "Please Enter Tin Number"
            Exit Sub
        Else
            lblTinNo.Text = ""
        End If
        If drpdPrefix.Text = "" Then
            lblPrefix.Text = "Select Prefix"
            Exit Sub
        Else
            lblAccountType.Text = ""
        End If
        If txtFirstName.Text = "" Then
            lblFname.Text = "Please Enter First Name"
            Exit Sub
        Else
            lblFname.Text = ""
        End If
        If CheckIsNumber(txtFirstName.Text) = True Or txtFirstName.Text = "" Then
            lblFname.Text = "Please Enter Correct First Name"
            Exit Sub
        Else
            lblFname.Text = ""
        End If
        If txtLastName.Text = "" Then
            lblLname.Text = "Please Enter Last Name"
            Exit Sub
        Else
            lblLname.Text = ""
        End If
        If CheckIsNumber(txtLastName.Text) = True Or txtLastName.Text = "" Then
            lblLname.Text = "Please Enter Correct Last Name"
            Exit Sub
        Else
            lblLname.Text = ""
        End If
        If drpGender.Text = "" Then
            lblGender.Text = "Select Gender"
            Exit Sub
        Else
            lblGender.Text = ""
        End If
        If IsNumeric(txtDepemdant.Text) Then
            lblDependants.Text = ""
        Else
            lblDependants.Text = "Dependants must be numeric"
            Exit Sub
        End If
        'waitara check if customer exists
        Dim MySmsObject As New DataManagement
        Dim mySearch As String = "select C_CustomerNo from tbCustomer Where C_CustomerNo ='" & txtCustID.Text.Trim & "'"
        Dim MyReader As SqlDataReader = TryCast(MySmsObject.DBAction(mySearch, DataManagement.DBActionType.SqlDataReader), SqlDataReader)

        If MyReader.HasRows Then
            lblSuccess.ForeColor = Drawing.Color.Red
            lblSuccess.Text = "This Customer Number Already Exists"
            Exit Sub
        Else

            ' If MyReader.HasRows = True Then
            '    lblSuccess.
            '    lblSuccess.Text = "This Customer Number Already Exist"
            '    Exit Sub
            'Else
            'Label7.Text = ""
            'Label4.Text = ""
        End If
        MyReader.Close()
        If txtDepemdant.Text = "" Then
            txtDepemdant.Text = 0
        End If
        Dim mySqlIndividual As String = ""
        mySqlIndividual = "Insert Into tbCustomer (C_CustomerNo,C_CustTypeCode,C_CustomerType,I_PreFix,I_FirstName,I_MiddleName,I_LastName,C_FullName,C_Telephone,C_MobileNo,C_Email,C_Address1,C_Address2,C_Address3,C_Region,C_District,C_Area,C_TINNo,I_NationalID,I_PassportNO,C_RelationShipOfficer,I_Nationality,I_DoB,I_ResidentStatus,I_Language,I_Sex,I_MaritalStatus,I_SpouseName,I_Dependants,I_Occupation,I_EmployerName,MakerID,MakerOnDate,CheckerId,CheckedOnDate) VALUES ('" & txtCustID.Text & "','" & drpAccountType.SelectedValue.ToUpper.Trim & "','INDIVIDUAL ACCOUNT','" & drpdPrefix.SelectedValue.ToUpper.Trim & "','" & txtFirstName.Text.ToUpper.Trim & "','" & txtMiddleName.Text.ToUpper.Trim & "','" & txtLastName.Text.ToUpper.Trim & "','" & txtFullName.Text.ToUpper.Trim & "','" & txtTelNo.Text.ToUpper.Trim & "','" & txtMobileNo.Text.ToUpper.Trim & "','" & txtEmail.Text.Trim & "','" & txtAddress1.Text.ToUpper.Trim & "','" & txtAddress2.Text.ToUpper.Trim & "','" & txtAddress3.Text.ToUpper.Trim & "','" & drpdRegion.SelectedValue.ToUpper.Trim & "','" & drpdDistrict.SelectedValue.ToUpper.Trim & "' , '" & drpdArea.SelectedValue.ToUpper.Trim & "' , '" & txtTillNo.Text.ToUpper.Trim & "' , '" & txtIDNo.Text.ToUpper.Trim & "' , '" & txtPassNo.Text.ToUpper.Trim & "' , '" & txtRelOfficer.Text.ToUpper.Trim & "' , '" & txtNationality.Text.ToUpper.Trim & "' , '" & txtDoB.Text & "' , '" & txtResStatus.Text.ToUpper.Trim & "' , '" & txtLang.Text.ToUpper.Trim & "' , '" & drpGender.Text.ToUpper.Trim & "' , '" & drpMaritalStatus.Text.ToUpper.Trim & "' , '" & txtSpouseName.Text.ToUpper.Trim & "' , '" & txtDepemdant.Text.ToUpper.Trim & "','" & txtOccupation.Text.ToUpper.Trim & "' , '" & txtEmployer.Text.ToUpper.Trim & "','WAITARA','" & Date.Today.ToString("dd-MMM-yyyy") & "','WAITARA','" & Date.Today.ToString("dd-MMM-yyyy") & "')"
        MyDBAction.DBAction(mySqlIndividual, DataManagement.DBActionType.Insert)
        SaveCustNoSequence(txtCustID.Text)
        lblSuccess.Text = "Customer details have been created successfully"
        'clear controls
        txtCustID.Text = ""
        txtFirstName.Text = ""
        txtMiddleName.Text = ""
        txtLastName.Text = ""
        txtFullName.Text = ""
        txtTelNo.Text = ""
        txtMobileNo.Text = ""
        txtEmail.Text = ""
        txtAddress1.Text = ""
        txtAddress2.Text = ""
        txtAddress3.Text = ""
        drpdRegion.Text = ""
        drpdDistrict.Text = ""
        drpdArea.Text = ""
        txtTillNo.Text = ""
        txtIDNo.Text = ""
        txtPassNo.Text = ""
        txtRelOfficer.Text = ""
        txtNationality.Text = ""
        txtDoB.Text = ""
        txtResStatus.Text = ""
        txtLang.Text = ""
        drpMaritalStatus.Text = ""
        txtSpouseName.Text = ""
        txtDepemdant.Text = ""
        txtOccupation.Text = ""
        txtEmployer.Text = ""
        lblCustIDs.Text = ""
        Session("DOB") = ""
        Session("AccountNo") = ""
        btnManual.Enabled = True
        btnCustNo.Enabled = True
    End Sub

    Protected Sub btnSaveBiz_Click(sender As Object, e As EventArgs) Handles btnSaveBiz.Click
        'Inserts Business customer
        Dim MyDBAction As New DataManagement
        Dim mycustID As String = txtCustID.Text
        'drpdRegion.SelectedValue = ""
        'drpdDistrict.SelectedValue = ""
        If drpAccountType.Text = "" Then
            lblAccountType.Text = "Select Account Type"
            Exit Sub
        Else
            lblAccountType.Text = ""
        End If
        If txtCustID.Text = "" Then
            lblCustIDs.Text = "Please Enter Customer Number"
            Exit Sub
        Else
            lblCustIDs.Text = ""
        End If
        If IsNumeric(txtCustID.Text) Then
            lblCustIDs.Text = ""
        Else
            lblCustIDs.Text = "Customer Number must be numeric"
            Exit Sub
        End If
        If txtFullName.Text = "" Then
            lblFullName.Text = "Please Enter Full Name"
            Exit Sub
        Else
            lblFullName.Text = ""
        End If
        If txtAddress1.Text = "" Then
            lblAdress1.Text = "Please Enter Address 1"
            Exit Sub
        Else
            lblAdress1.Text = ""
        End If
        If txtEmail.Text <> "" Then
            If EmailAddressCheck(txtEmail.Text) = True Then
                lblEmail.Text = ""
            Else
                lblEmail.Text = "Enter Correct Email Address"
                Exit Sub
            End If
        End If
        If drpdRegion.SelectedValue = "" Then
            lblRegion.Text = "Select Region"
            Exit Sub
        Else
            lblRegion.Text = ""
        End If
        If drpdDistrict.SelectedValue = "" Then
            lblDistrict.Text = "Select District"
            Exit Sub
        Else
            lblDistrict.Text = ""
        End If
        If drpdArea.SelectedValue = "" Then
            lblArea.Text = "Select Area"
            Exit Sub
        Else
            lblArea.Text = ""
        End If
        If txtBizName.Text = "" Then
            lblBizName.Text = "Enter  Business Name"
            Exit Sub
        Else
            lblBizName.Text = ""
        End If
        If txtTillNo.Text = "" Then
            lblTinNo.Text = "Please Enter Tin Number"
            Exit Sub
        Else
            lblTinNo.Text = ""
        End If
        If txtRegNo.Text = "" Then
            lblRegNo.Text = "Please Enter Registration Number"
            Exit Sub
        Else
            lblRegNo.Text = ""
        End If
        'waitara check if customer exists
        Dim MySmsObject As New DataManagement
        Dim mySearch As String = "select C_CustomerNo from tbCustomer Where C_CustomerNo ='" & txtCustID.Text.Trim & "'"
        Dim MyReader As SqlDataReader = TryCast(MySmsObject.DBAction(mySearch, DataManagement.DBActionType.SqlDataReader), SqlDataReader)

        If MyReader.HasRows = True Then
            lblBizName.Text = "This Customer Number Already Exist"
            Exit Sub
        Else
            'Label7.Text = ""
            'Label4.Text = ""
        End If
        MyReader.Close()
        Dim mySqlAction2 As String = ""
        mySqlAction2 = "Insert Into tbCustomer (C_CustomerNo,C_CustTypeCode,C_CustomerType,C_FullName,C_Telephone,C_MobileNo,C_Email,C_Address1,C_Address2,C_Address3,C_Region,C_District,C_Area,C_TINNo,B_BusinessName,B_RegDate,B_RegName,B_TelNo,B_NatureofBusiness,B_DirectorName,MakerID,MakerOnDate,CheckerId,CheckedOnDate) VALUES ('" & txtCustID.Text & "','" & drpAccountType.SelectedValue.ToUpper.Trim & "','BUSINESS ACCOUNT','" & txtFullName.Text.ToUpper.Trim & "','" & txtTelNo.Text.ToUpper.Trim & "','" & txtMobileNo.Text.ToUpper.Trim & "','" & txtEmail.Text.Trim & "','" & txtAddress1.Text.ToUpper.Trim & "','" & txtAddress2.Text.ToUpper.Trim & "','" & txtAddress3.Text.ToUpper.Trim & "','" & drpdRegion.SelectedValue.ToUpper.Trim & "','" & drpdDistrict.SelectedValue.ToUpper.Trim & "','" & drpdArea.SelectedValue.ToUpper.Trim & "','" & txtTillNo.Text.ToUpper.Trim & "','" & txtBizName.Text.ToUpper.Trim & "','" & txtBizRegDate.Text.ToUpper.Trim & "' , '" & txtRegNo.Text.ToUpper.Trim & "' , '" & txtTelNo.Text.ToUpper.Trim & "' , '" & txtBizNature.Text.ToUpper.Trim & "' , '" & txtDirName.Text.ToUpper.Trim & "','WAITARA','" & Date.Today.ToString("dd-MMM-yyyy") & "','WAITARA','" & Date.Today.ToString("dd-MMM-yyyy") & "')"
        MyDBAction.DBAction(mySqlAction2, DataManagement.DBActionType.Insert)
        SaveCustNoSequence(txtCustID.Text)
        lblBizName.ForeColor = Drawing.Color.Green
        lblBizName.Text = "Business Customer details have been created successfully"
        txtRegNo.Text = ""
        txtBizName.Text = ""
        txtBizName.Text = ""
        txtBizRegDate.Text = ""
        txtBizName.Text = ""
        txtBizNature.Text = ""
        txtDirName.Text = ""
        txtCustID.Text = ""
        txtFirstName.Text = ""
        txtMiddleName.Text = ""
        txtLastName.Text = ""
        txtFullName.Text = ""
        txtTelNo.Text = ""
        txtMobileNo.Text = ""
        txtEmail.Text = ""
        txtAddress1.Text = ""
        txtAddress2.Text = ""
        txtAddress3.Text = ""
        drpdRegion.Text = ""
        drpdDistrict.Text = ""
        drpdArea.Text = ""
        txtTillNo.Text = ""
        txtIDNo.Text = ""
        txtPassNo.Text = ""
        txtRelOfficer.Text = ""
        txtNationality.Text = ""
        txtDoB.Text = ""
        txtResStatus.Text = ""
        txtLang.Text = ""
        drpMaritalStatus.Text = ""
        txtSpouseName.Text = ""
        txtDepemdant.Text = ""
        txtOccupation.Text = ""
        txtEmployer.Text = ""
        lblCustIDs.Text = ""
        Session("DOB") = ""
        Session("AccountNo") = ""
        btnManual.Enabled = True
        btnCustNo.Enabled = True
    End Sub

    Protected Sub btnSaveGroup_Click(sender As Object, e As EventArgs) Handles btnSaveGroup.Click
        'insert group details
        Dim MyDBAction As New DataManagement
        Dim mycustID As String = txtCustID.Text
        If drpAccountType.Text = "" Then
            lblAccountType.Text = "Select Account Type"
            Exit Sub
        Else
            lblAccountType.Text = ""
        End If
        If txtCustID.Text = "" Then
            lblCustIDs.Text = "Please Enter Customer Number"
            Exit Sub
        Else
            lblCustIDs.Text = ""
        End If
        If IsNumeric(txtCustID.Text) Then
            lblCustIDs.Text = ""
        Else
            lblCustIDs.Text = "Customer Number must be numeric"
            Exit Sub
        End If
        If txtFullName.Text = "" Then
            lblFullName.Text = "Please Enter Full Name"
            Exit Sub
        Else
            lblFullName.Text = ""
        End If
        If txtAddress1.Text = "" Then
            lblAdress1.Text = "Please Enter Address 1"
            Exit Sub
        Else
            lblAdress1.Text = ""
        End If
        If txtEmail.Text <> "" Then
            If EmailAddressCheck(txtEmail.Text) = True Then
                lblEmail.Text = ""
            Else
                lblEmail.Text = "Enter Correct Email Address"
                Exit Sub
            End If
        End If
        If drpdRegion.SelectedValue = "" Then
            lblRegion.Text = "Select Region"
            Exit Sub
        Else
            lblRegion.Text = ""
        End If
        If drpdDistrict.SelectedValue = "" Then
            lblDistrict.Text = "Select District"
            Exit Sub
        Else
            lblDistrict.Text = ""
        End If
        If drpdArea.SelectedValue = "" Then
            lblArea.Text = "Select Area"
            Exit Sub
        Else
            lblArea.Text = ""
        End If
        If txtTillNo.Text = "" Then
            lblTinNo.Text = "Please Enter Tin Number"
            Exit Sub
        Else
            lblTinNo.Text = ""
        End If
        If txtgrpName.Text = "" Then
            lblGroupName.Text = "Please Enter Group Name"
            Exit Sub
        Else
            lblGroupName.Text = ""
        End If
        If txtgrpRegNo.Text = "" Then
            lblgrpRegNo.Text = "Please Enter Registration Number"
            Exit Sub
        Else
            lblgrpRegNo.Text = ""
        End If
        If IsNumeric(txtgrpMemberNo.Text) Then
            lblMembers.Text = ""
        Else
            lblMembers.Text = "Number of Memmbers must be numeric"
            Exit Sub
        End If
        If IsNumeric(txtFemaleNo.Text) Then
            lblFemaleNo.Text = ""
        Else
            lblFemaleNo.Text = "Number Female Members must be numeric"
            Exit Sub
        End If
        If IsNumeric(txtMaleNo.Text) Then
            lblMaleNo.Text = ""
        Else
            lblMaleNo.Text = "Number of Male Members must be numeric"
            Exit Sub
        End If
        'waitara check if customer exists
        Dim MySmsObject As New DataManagement
        Dim mySearch As String = "select C_CustomerNo from tbCustomer Where C_CustomerNo ='" & txtCustID.Text.Trim & "'"
        Dim MyReader As SqlDataReader = TryCast(MySmsObject.DBAction(mySearch, DataManagement.DBActionType.SqlDataReader), SqlDataReader)

        If MyReader.HasRows = True Then
            lblGroupName.Text = "This Customer Number Already Exist"
            Exit Sub
        Else
            'Label7.Text = ""
            'Label4.Text = ""
        End If
        MyReader.Close()
        Dim mySqlAction2 As String = ""
        mySqlAction2 = "Insert Into tbCustomer (C_CustomerNo,C_CustTypeCode,C_CustomerType,C_FullName,C_Telephone,C_MobileNo,C_Email,C_Address1,C_Address2,C_Address3,C_Region,C_District,C_Area,C_TINNo,B_RegDate,B_RegName,B_NatureofBusiness,B_DirectorName,G_GroupName,G_NoofMembers,G_NoofMale,G_NoofFemale,MakerID,MakerOnDate,CheckerId,CheckedOnDate) VALUES ('" & txtCustID.Text & "','" & drpAccountType.SelectedValue.ToUpper.Trim & "','GROUP ACCOUNT','" & txtFullName.Text.ToUpper.Trim & "','" & txtTelNo.Text.ToUpper.Trim & "','" & txtMobileNo.Text.ToUpper.Trim & "','" & txtEmail.Text.Trim & "','" & txtAddress1.Text.ToUpper.Trim & "','" & txtAddress2.Text.ToUpper.Trim & "','" & txtAddress3.Text.ToUpper.Trim & "','" & drpdRegion.SelectedValue.ToUpper.Trim & "','" & drpdDistrict.SelectedValue.ToUpper.Trim & "','" & drpdArea.SelectedValue.ToUpper.Trim & "','" & txtTillNo.Text.ToUpper.Trim & "','" & txtgrpRegDate.Text.ToUpper.Trim & "','" & txtgrpRegNo.Text.ToUpper.Trim & "','" & txtgrpBizNature.Text.ToUpper.Trim & "','" & txtgrpDirName.Text.ToUpper.Trim & "','" & txtgrpName.Text.ToUpper.Trim & "','" & txtgrpMemberNo.Text.ToUpper.Trim & "' , '" & txtMaleNo.Text.ToUpper.Trim & "' , '" & txtFemaleNo.Text.ToUpper.Trim & "','WAITARA','" & Date.Today.ToString("dd-MMM-yyyy") & "','WAITARA','" & Date.Today.ToString("dd-MMM-yyyy") & "')"
        MyDBAction.DBAction(mySqlAction2, DataManagement.DBActionType.Insert)
        SaveCustNoSequence(txtCustID.Text)
        lblGroupName.ForeColor = Drawing.Color.Green
        lblGroupName.Text = "Group Customer details have been created successfully"
        txtgrpRegNo.Text = ""
        txtgrpName.Text = ""
        txtgrpBizNature.Text = ""
        txtgrpDirName.Text = ""
        txtgrpMemberNo.Text = ""
        txtgrpName.Text = ""
        txtgrpRegDate.Text = ""
        drpdArea.Text = ""
        txtMaleNo.Text = ""
        txtFemaleNo.Text = ""
        txtDoB.Text = ""
        txtResStatus.Text = ""
        Session("DOB") = ""
        Session("AccountNo") = ""
        txtCustID.Text = ""
        txtFullName.Text = ""
        txtTelNo.Text = ""
        txtMobileNo.Text = ""
        txtEmail.Text = ""
        txtAddress1.Text = ""
        txtAddress2.Text = ""
        txtAddress3.Text = ""
        drpdRegion.Text = ""
        drpdDistrict.Text = ""
        drpdArea.Text = ""
        txtTillNo.Text = ""
        lblCustIDs.Text = ""
        Session("DOB") = ""
        Session("AccountNo") = ""
        btnManual.Enabled = True
        btnCustNo.Enabled = True
    End Sub
    Protected Sub SaveCustNoSequence(custNo)
        'txtOrgName.Text = ""
        'Label4.Text = ""
        If btnCustNo.Enabled = True Then
            Try

                Dim MyDBAction As New DataManagement
                Dim mySqlAction2 As String = ""
                Dim mycustNo As String = ""
                mycustNo = "Insert Into MB_CustomerNoTracker (CustomerNo,Date,Status,CreatedBy) VALUES ('" & custNo.ToUpper.Trim & "','" & Date.Today.ToString("dd-MMM-yyyy") & "','1','WAITARA')"
                MyDBAction.DBAction(mycustNo, DataManagement.DBActionType.Insert)

            Catch ex As Exception
                'txtOrgName.Text = ""
                'txtAdress1.Text = ""
                '' DropDownList1.Text = ""
                'txtOrgCode.ReadOnly = False
                'Label4.Text = "Error Pulling Account Details, Commission A/c must start 01M8 .... E6 "
            End Try
        End If
    End Sub

    Protected Sub btnIndClear_Click(sender As Object, e As EventArgs) Handles btnIndClear.Click
        txtCustID.Text = ""
        'txtPrefix.Text = ""
        txtFirstName.Text = ""
        txtMiddleName.Text = ""
        txtLastName.Text = ""
        txtFullName.Text = ""
        txtTelNo.Text = ""
        txtMobileNo.Text = ""
        txtEmail.Text = ""
        txtAddress1.Text = ""
        txtAddress2.Text = ""
        txtAddress3.Text = ""
        drpdRegion.Text = ""
        drpdDistrict.Text = ""
        drpdArea.Text = ""
        txtTillNo.Text = ""
        txtIDNo.Text = ""
        txtPassNo.Text = ""
        txtRelOfficer.Text = ""
        txtNationality.Text = ""
        txtDoB.Text = ""
        txtResStatus.Text = ""
        txtLang.Text = ""
        drpMaritalStatus.Text = ""
        txtSpouseName.Text = ""
        txtDepemdant.Text = ""
        txtOccupation.Text = ""
        txtEmployer.Text = ""
        lblCustIDs.Text = ""
        Session("DOB") = ""
        Session("AccountNo") = ""
        lblSuccess.Text = ""
    End Sub

    Protected Sub btnBizClear_Click(sender As Object, e As EventArgs) Handles btnBizClear.Click
        txtRegNo.Text = ""
        txtBizName.Text = ""
        txtBizName.Text = ""
        txtBizRegDate.Text = ""
        txtBizName.Text = ""
        txtBizNature.Text = ""
        txtDirName.Text = ""
        txtCustID.Text = ""
        txtFirstName.Text = ""
        txtMiddleName.Text = ""
        txtLastName.Text = ""
        txtFullName.Text = ""
        txtTelNo.Text = ""
        txtMobileNo.Text = ""
        txtEmail.Text = ""
        txtAddress1.Text = ""
        txtAddress2.Text = ""
        txtAddress3.Text = ""
        drpdRegion.Text = ""
        drpdDistrict.Text = ""
        drpdArea.Text = ""
        txtTillNo.Text = ""
        lblCustIDs.Text = ""
        Session("DOB") = ""
        Session("AccountNo") = ""
        lblBizName.Text = ""
    End Sub

    Protected Sub btnGrpClear_Click(sender As Object, e As EventArgs) Handles btnGrpClear.Click
        txtgrpRegNo.Text = ""
        txtgrpName.Text = ""
        txtgrpBizNature.Text = ""
        txtgrpDirName.Text = ""
        txtgrpMemberNo.Text = ""
        txtgrpName.Text = ""
        txtgrpRegDate.Text = ""
        drpdArea.Text = ""
        txtMaleNo.Text = ""
        txtFemaleNo.Text = ""
        txtDoB.Text = ""
        txtResStatus.Text = ""
        txtLang.Text = ""
        drpMaritalStatus.Text = ""
        txtSpouseName.Text = ""
        txtDepemdant.Text = ""
        txtOccupation.Text = ""
        txtEmployer.Text = ""
        Session("DOB") = ""
        Session("AccountNo") = ""
        txtCustID.Text = ""
        txtFirstName.Text = ""
        txtMiddleName.Text = ""
        txtLastName.Text = ""
        txtFullName.Text = ""
        txtTelNo.Text = ""
        txtMobileNo.Text = ""
        txtEmail.Text = ""
        txtAddress1.Text = ""
        txtAddress2.Text = ""
        txtAddress3.Text = ""
        drpdRegion.Text = ""
        drpdDistrict.Text = ""
        drpdArea.Text = ""
        txtTillNo.Text = ""
        txtIDNo.Text = ""
        lblGroupName.Text = ""

    End Sub

    Protected Sub CmdApprovalList_Click(sender As Object, e As EventArgs) Handles CmdApprovalList.Click
        txtgrpRegNo.Text = ""
        txtgrpName.Text = ""
        txtgrpBizNature.Text = ""
        txtgrpDirName.Text = ""
        txtgrpMemberNo.Text = ""
        txtgrpName.Text = ""
        txtgrpRegDate.Text = ""
        drpdArea.Text = ""
        txtMaleNo.Text = ""
        txtFemaleNo.Text = ""
        txtDoB.Text = ""
        txtResStatus.Text = ""
        txtLang.Text = ""
        drpMaritalStatus.Text = ""
        txtSpouseName.Text = ""
        txtDepemdant.Text = ""
        txtOccupation.Text = ""
        txtEmployer.Text = ""
        Session("DOB") = ""
        Session("AccountNo") = ""
        txtCustID.Text = ""
        txtFirstName.Text = ""
        txtMiddleName.Text = ""
        txtLastName.Text = ""
        txtFullName.Text = ""
        txtTelNo.Text = ""
        txtMobileNo.Text = ""
        txtEmail.Text = ""
        txtAddress1.Text = ""
        txtAddress2.Text = ""
        txtAddress3.Text = ""
        drpdRegion.Text = ""
        drpdDistrict.Text = ""
        drpdArea.Text = ""
        txtTillNo.Text = ""
        txtIDNo.Text = ""
        lblGroupName.Text = ""
        txtRegNo.Text = ""
        txtBizName.Text = ""
        txtBizName.Text = ""
        txtBizRegDate.Text = ""
        txtBizName.Text = ""
        txtBizNature.Text = ""
        txtDirName.Text = ""
        txtCustID.Text = ""
        txtFirstName.Text = ""
        txtMiddleName.Text = ""
        txtLastName.Text = ""
        txtFullName.Text = ""
        txtTelNo.Text = ""
        txtMobileNo.Text = ""
        txtEmail.Text = ""
        txtAddress1.Text = ""
        txtAddress2.Text = ""
        txtAddress3.Text = ""
        drpdRegion.Text = ""
        drpdDistrict.Text = ""
        drpdArea.Text = ""
        txtTillNo.Text = ""
        lblCustIDs.Text = ""
        Session("DOB") = ""
        Session("AccountNo") = ""
        lblBizName.Text = ""
        txtCustID.Text = ""
        'txtPrefix.Text = ""
        txtFirstName.Text = ""
        txtMiddleName.Text = ""
        txtLastName.Text = ""
        txtFullName.Text = ""
        txtTelNo.Text = ""
        txtMobileNo.Text = ""
        txtEmail.Text = ""
        txtAddress1.Text = ""
        txtAddress2.Text = ""
        txtAddress3.Text = ""
        drpdRegion.Text = ""
        drpdDistrict.Text = ""
        drpdArea.Text = ""
        txtTillNo.Text = ""
        txtIDNo.Text = ""
        txtPassNo.Text = ""
        txtRelOfficer.Text = ""
        txtNationality.Text = ""
        txtDoB.Text = ""
        txtResStatus.Text = ""
        txtLang.Text = ""
        drpMaritalStatus.Text = ""
        txtSpouseName.Text = ""
        txtDepemdant.Text = ""
        txtOccupation.Text = ""
        txtEmployer.Text = ""
        lblCustIDs.Text = ""
        Session("DOB") = ""
        Session("AccountNo") = ""
        lblSuccess.Text = ""
        drpAccountType.Text = ""
        drpdPrefix.Text = ""
        drpGender.Text = ""

    End Sub

    Protected Sub btnCustNo_Click(sender As Object, e As EventArgs) Handles btnCustNo.Click
        btnManual.Enabled = False
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "SELECT NEXT VALUE FOR MB_CustomerNumber as CustomerNo" ' ='" & Me.HiddenField1.Value & "' "
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)

            If MyReader4.HasRows = True Then
                MyReader4.Read()
                Dim custID As String = MyReader4("CustomerNo").ToString.Substring(1, 6)
                txtCustID.Text = custID
            End If
            MyReader4.Close()
        Catch ex As Exception
            lblSuccess.Text = ex.ToString
        End Try
    End Sub

    Protected Sub btnManual_Click(sender As Object, e As EventArgs) Handles btnManual.Click
        btnCustNo.Enabled = False
        'txtCustID.Focus()
    End Sub
End Class