Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Imports System.Data.SqlClient
Imports System.Globalization

Public Class CreateAccount
    Inherits System.Web.UI.Page
    Public AccountCode As String = "", ProductName As String = "", AccountPrefix As String = "", AccountName As String = "", DormancyDays As String = "", GLType As String = "", AssetGL As String = "", LiabilityGL As String = "", IncomeGL As String = "", ExpenseGL As String = "", ChargeGL As String = "", FeeGL As String = "", IntReceivable As String = "", IntPayable As String = "", IntRate As String = "", IntFreq As String = "", IntAccrualFreq As String = "", MinInterestBal As String = "", MinAccountBal As String = "", DebitsNoPerMonth As String = "", Amount As String = "", test As String = ""
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Dim MyAcctDetails As String = ""
        If Not Page.IsPostBack Then
            GetSectors()
            GetSubSectors()
            TabContainer1.ActiveTabIndex = 0
            DisplayDetails("All")
            txtCustomerNo.ReadOnly = False

            DOBDatePicker.Visible = False
            BizRegDatePicker.Visible = False
            'GrpRegDatePicker.Visible = False
        End If
    End Sub
    Public Sub GetSectors()
        'Load regions
        Try
            Dim MyDataTable As New Data.DataTable
            Dim MyDBAction As New DataManagement
            MyDataTable = TryCast(MyDBAction.DBAction(" Select SectorCode,SectorName from MB_Sector", DataManagement.DBActionType.SqlDataTable), Data.DataTable)
            If MyDataTable IsNot Nothing Then
                With drpSector
                    .Items.Clear()
                    .DataSource = MyDataTable
                    .DataTextField = MyDataTable.Columns("SectorName").ToString.Trim
                    .DataValueField = MyDataTable.Columns("SectorCode").ToString.Trim
                    .DataBind()
                    .Items.Add("")
                    .SelectedValue = ""
                End With
                MyDataTable = Nothing
            End If
        Catch ex As Exception
        End Try
    End Sub
    Public Sub GetSubSectors()
        'Load regions
        Try
            Dim MyDataTable As New Data.DataTable
            Dim MyDBAction As New DataManagement
            MyDataTable = TryCast(MyDBAction.DBAction(" Select SubSectorCode,SubSectorName from MB_SubSector", DataManagement.DBActionType.SqlDataTable), Data.DataTable)
            If MyDataTable IsNot Nothing Then
                With drpSubSector
                    .Items.Clear()
                    .DataSource = MyDataTable
                    .DataTextField = MyDataTable.Columns("SubSectorName").ToString.Trim
                    .DataValueField = MyDataTable.Columns("SubSectorCode").ToString.Trim
                    .DataBind()
                    .Items.Add("")
                    .SelectedValue = ""
                End With
                MyDataTable = Nothing
            End If
        Catch ex As Exception
        End Try
    End Sub
    'Public Sub GetSectors()
    '    'Load regions
    '    Try
    '        Dim MyDataTable As New Data.DataTable
    '        Dim MyDBAction As New DataManagement
    '        MyDataTable = TryCast(MyDBAction.DBAction(" Select SectorCode,SectorName from MB_Sector", DataManagement.DBActionType.SqlDataTable), Data.DataTable)
    '        If MyDataTable IsNot Nothing Then
    '            With drpdSector
    '                .Items.Clear()
    '                .DataSource = MyDataTable
    '                .DataTextField = MyDataTable.Columns("SectorName").ToString.Trim
    '                .DataValueField = MyDataTable.Columns("SectorCode").ToString.Trim
    '                .DataBind()
    '                .Items.Add("")
    '                .SelectedValue = ""
    '            End With
    '            MyDataTable = Nothing
    '        End If
    '    Catch ex As Exception
    '    End Try
    ' End Sub
    Public Function LogError(ByVal strErrorMessage As String, ByVal strErrorID As String, ByVal strModuleName As String, ByVal strMethodName As String, ByVal strTimeOccur As Date, ByVal strOperation As String) As Boolean
        Try
            Dim strErrorLog As System.IO.StreamWriter = Nothing
            Dim fileName As String = ""
            Dim DateT = Format(DateTime.Now(), "dd-MMM-yyyy")
            fileName = DateT & ".log"
            Dim path As String = "C:\Logs"
            If path.Substring(path.LastIndexOf("\") + 1).Contains("\") <> False Then
                fileName = path & fileName
            Else
                fileName = path & "\" & fileName
            End If
            If System.IO.File.Exists(fileName) = False Then
                System.IO.File.Create(fileName).Close()
            End If
            strErrorLog = New System.IO.StreamWriter(fileName, True)
            strErrorLog.WriteLine("----------------------------------------------------------------------------------------------------")
            strErrorLog.WriteLine(strErrorMessage & " : " & strErrorID & " : " & strModuleName & " : " & strMethodName & " : " & strTimeOccur & " : " & strOperation)
            strErrorLog.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~")
            strErrorLog.WriteLine()
            strErrorLog.Dispose()
        Catch ex As Exception

        End Try
    End Function
    Private Sub DisplayDetails(ByVal ShowType As String)
        Dim searchStr As String = ""
        Dim SqlAction As String = ""
        Dim Processfilter As String = ""
        Dim ApprovalStatus As String = ""
        Try
            Dim mySqlAction As String = ""
            Select Case ShowType
                Case "All"
                    mySqlAction = "select CustomerNo,AccountType,BranchName,AccountNo,AccountName,Currency,Sector,SubSector,IntRate,DebitNoStatus,BranchCode,AccountPrefix,C_Address1,LoanAmount,LoanStartDate,LoanEndDate,RepaymentPeriod from tbAccount where ProductDefCode = 'SA' and Approved = '0'"
                Case "Filter"
                    If RdoSearch.Items(0).Selected = True Then
                        Processfilter = txtSearch.Text.ToUpper
                        Processfilter = "%" & Processfilter & "%"
                        mySqlAction = "select CustomerNo,AccountType,BranchName,AccountNo,AccountName,Currency,Sector,SubSector,IntRate,DebitNoStatus,BranchCode,AccountPrefix,C_Address1,LoanAmount,LoanStartDate,LoanEndDate,RepaymentPeriod from tbAccount where AccountNo LIKE '" & Processfilter & "' and ProductDefCode = 'SA' and Approved = '0'"
                    End If
                    If RdoSearch.Items(1).Selected = True Then
                        Processfilter = txtSearch.Text.ToUpper
                        Processfilter = "%" & Processfilter & "%"
                        mySqlAction = "select CustomerNo,AccountType,BranchName,AccountNo,AccountName,Currency,Sector,SubSector,IntRate,DebitNoStatus,BranchCode,AccountPrefix,C_Address1,LoanAmount,LoanStartDate,LoanEndDate,RepaymentPeriod from tbAccount where AccountName  LIKE '" & Processfilter & "' and ProductDefCode = 'SA' and Approved = '0'"
                    End If
            End Select
            Dim MyDBAction As New DataManagement
            Dim MyDatable As New Data.DataTable
            MyDatable = TryCast(MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.DataTable), Data.DataTable)
            Session("searchtable") = MyDatable
            GridView1.DataSource = MyDatable
            GridView1.DataBind()
        Catch ex As Exception

        End Try
    End Sub
    Protected Sub GridView1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles GridView1.Load

    End Sub
    Protected Sub GridView1_PageIndexChanging(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewPageEventArgs) Handles GridView1.PageIndexChanging
        GridView1.DataSource = Session("searchtable")
        GridView1.PageIndex = e.NewPageIndex
        GridView1.DataBind()
    End Sub
    Protected Sub GridView1_PreRender(ByVal sender As Object, ByVal e As System.EventArgs) Handles GridView1.PreRender

    End Sub
    Protected Sub GridView1_RowCommand(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewCommandEventArgs) Handles GridView1.RowCommand
        Try
            HiddenField1.Value = ""
            Dim MyID As String = e.CommandArgument
            HiddenField1.Value = MyID
            Select Case e.CommandName
                Case "edit"
                    txtCustomerNo.Enabled = False
                    txtBranchCode.Enabled = False
                    txtAccountCode.Enabled = False
                    txtAccountNo.Enabled = False
                    txtBranchName.Enabled = False
                    txtCurrency.Enabled = False
                    txtProductCode.Enabled = False
                    btnCheckBranch.Visible = False
                    btnSearchCode.Visible = False
                    cmasearchCust.Visible = False
                    ShowMatchDetails("Select   * From tbAccount where AccountNo= '" & HiddenField1.Value.ToString & "' ", "ViewDetails")
                    TabContainer1.ActiveTabIndex = 0
                    DisplayDetails("All")
                Case "select"
                    txtCustomerNo.Enabled = True
                    txtBranchCode.Enabled = True
                    txtAccountCode.Enabled = True
                    txtAccountNo.Enabled = True
                    txtBranchName.Enabled = True
                    txtCurrency.Enabled = True
                    txtProductCode.Enabled = True
                    btnCheckBranch.Visible = True
                    btnSearchCode.Visible = True
                    cmasearchCust.Visible = True
                    ShowMatchDetails("Select  * From tbAccount where AccountNo= '" & HiddenField1.Value.ToString & "' ", "ViewDetails")
                    TabContainer1.ActiveTabIndex = 0
                    DisplayDetails("All")
            End Select
        Catch ex As Exception
            Dim ErrMsg As String = ex.Message
        End Try
    End Sub
    Protected Sub GridView1_RowDeleting(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewDeleteEventArgs) Handles GridView1.RowDeleting

    End Sub
    Protected Sub GridView1_RowEditing(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewEditEventArgs) Handles GridView1.RowEditing

    End Sub
    Private Sub ShowMatchDetails(ByVal mySearch As String, ByVal ShowType As String)
        Dim MySmsObject As New DataManagement
        Dim MyTransProcessed As Boolean = False
        Dim MyReader As SqlDataReader = TryCast(MySmsObject.DBAction(mySearch, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
        GetSectors()
        GetSubSectors()


        Try
            Select Case ShowType
                Case "ViewDetails"
                    If MyReader.HasRows Then
                        MyReader.Read()

                        txtBranchCode.Text = MyReader("BranchCode").ToString
                        txtAccountCode.Text = MyReader("AccountCode").ToString
                        txtAccountNo.Text = MyReader("AccountNo").ToString
                        Session("AccountNo") = MyReader("AccountNo").ToString
                        txtAcountName.Text = MyReader("AcountName").ToString
                        txtBranchName.Text = MyReader("BranchName").ToString
                        txtCurrency.Text = MyReader("Currency").ToString
                        drpSector.SelectedValue = MyReader("Sector").ToString
                        drpSubSector.SelectedValue = MyReader("SubSector").ToString
                        txtProductName.Text = MyReader("AccountTypeName").ToString
                        txtAcountName.Text = MyReader("AccountName").ToString
                        txtCustomerNo.Text = MyReader("CustomerNo").ToString
                        drpAccountType.SelectedValue = MyReader("AccountType").ToString
                        drpdPrefix.SelectedValue = MyReader("I_PreFix").ToString
                        txtFirstName.Text = MyReader("I_FirstName").ToString
                        txtMiddleName.Text = MyReader("I_MiddleName").ToString
                        txtLastName.Text = MyReader("I_LastName").ToString
                        txtTelNo.Text = MyReader("C_Telephone").ToString
                        txtMobileNo.Text = MyReader("C_MobileNo").ToString
                        txtEmail.Text = MyReader("C_Email").ToString
                        txtAddress1.Text = MyReader("C_Address1").ToString
                        txtAddress2.Text = MyReader("C_Address2").ToString
                        txtAddress3.Text = MyReader("C_Address3").ToString
                        txtTillNo.Text = MyReader("C_TINNo").ToString
                        txtIDNo.Text = MyReader("I_NationalID").ToString
                        txtRelOfficer.Text = MyReader("C_RelationShipOfficer").ToString
                        drpMaritalStatus.SelectedValue = MyReader("I_MaritalStatus").ToString
                        txtSpouseName.Text = MyReader("I_SpouseName").ToString
                        txtDepemdant.Text = MyReader("I_Dependants").ToString
                        txtOccupation.Text = MyReader("I_Occupation").ToString
                        txtEmployer.Text = MyReader("I_EmployerName").ToString
                        txtBizName.Text = MyReader("B_BusinessName").ToString
                        txtBizRegDate.Text = MyReader("B_RegDate").ToString
                        txtBizNature.Text = MyReader("B_NatureofBusiness").ToString
                        txtDirName.Text = MyReader("B_DirectorName").ToString
                        txtRegNo.Text = MyReader("B_RegName").ToString
                        txtgrpName.Text = MyReader("G_GroupName").ToString
                        txtgrpMemberNo.Text = MyReader("G_NoofMembers").ToString
                        txtMaleNo.Text = MyReader("G_NoofMale").ToString
                        txtFemaleNo.Text = MyReader("G_NoofFemale").ToString
                        'added 06082021
                        txtRegion.Text = MyReader("C_Region").ToString
                        txtDistrict.Text = MyReader("C_District").ToString
                        txtArea.Text = MyReader("C_Area").ToString
                        txtSex.Text = MyReader("I_Sex").ToString
                        txtPassport.Text = MyReader("I_PassportNO").ToString
                        txtNationality.Text = MyReader("I_Nationality").ToString
                        txtLanguage.Text = MyReader("I_Language").ToString
                        txtDoB.Text = MyReader("I_DoB").ToString
                        txtResidential.Text = MyReader("I_ResidentStatus").ToString
                        'txtBizTel.Text = MyReader("B_TelNo").ToString
                    End If
            End Select
        Catch ex As Exception
        Finally
            If Not IsNothing(MyReader) Then
                MyReader.Close()
            End If
        End Try
    End Sub
    Private Function BranchcodeExist(ByVal strBranchcode As String) As Boolean
        Dim MyDBAction As New DataManagement
        Dim MyTable As Data.DataTable
        Dim MySqlAction As String = "Select BRANCHCODE from BRANCHES where BRANCHCODE = '" & strBranchcode & "'"
        MyTable = TryCast(MyDBAction.DBAction(MySqlAction, DataManagement.DBActionType.SqlDataTable), Data.DataTable)
        If MyTable.Rows.Count > 0 Then
            Return True
        Else
            Return False
        End If
    End Function
    Protected Sub btnSave_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSave.Click
        GetProductData()
        Try
            If Me.HiddenField1.Value = "" Then
                Dim mystring As String = ""
                mystring = txtCustomerNo.Text.ToUpper()
                Dim accountTypeCode As String = ""
                Select Case drpAccountType.Text.Trim
                    Case "INDIVIDUAL ACCOUNT"
                        accountTypeCode = "1"
                    Case "BUSINESS ACCOUNT"
                        accountTypeCode = "2"
                    Case Else
                        accountTypeCode = "3"
                End Select
                Dim mySqlAction As String = ""
                Dim MyDBAction As New DataManagement
                mySqlAction = "Insert into tbAccount(CustomerNo,AccountType,AccountTypeCode,BranchName,AccountNo,AccountName,Currency,Sector,SubSector,AccountStatus,AccountCode,AccountPrefix,AccountTypeName,ProductDefCode,DormancyDays,GLType,AssetGL_Control,LiabilityGL_Control,IncomeGL,ExpenseGL,ChargeGL,FeeGL,IntReceivable,IntPayable,IntRate,IntFreq,IntAccrualFreq,MinInterestBal,MinAccountBal,DebitsNoPerMonth,BranchCode,I_PreFix,I_FirstName,I_MiddleName,I_LastName,C_Telephone,C_MobileNo,C_Email,C_Address1,C_Address2,C_Address3,C_TINNo,I_NationalID,C_RelationShipOfficer,I_MaritalStatus,I_SpouseName,I_Dependants,I_Occupation,I_EmployerName,B_BusinessName,B_RegDate,B_NatureofBusiness,B_DirectorName,G_GroupName,G_NoofMembers,G_NoofMale,G_NoofFemale,C_Region,C_District,C_Area,I_ResidentStatus,I_Sex,I_PassportNO,I_Nationality,I_DoB,I_Language,B_TelNo,Approved,IntAccruedCR,IntAccruedDR,MinMonthlyBal)values
('" & txtCustomerNo.Text.Trim.ToUpper & "','" & drpAccountType.Text.Trim.ToUpper & "','" & accountTypeCode & "','" & txtBranchName.Text.Trim.ToUpper & "',
'" & txtAccountNo.Text.Trim.ToUpper & "','" & txtAcountName.Text.Trim.ToUpper & "','" & txtCurrency.Text.Trim.ToUpper & "',
'" & drpSector.SelectedValue.Trim.ToUpper & "','" & drpSubSector.SelectedValue.Trim.ToUpper & "','Open','" & txtAccountCode.Text.Trim.ToUpper & "','" & AccountPrefix.Trim.ToUpper & "','" & txtProductName.Text.Trim.ToUpper & "','" & test & "','" & DormancyDays.Trim.ToUpper & "','" & GLType.Trim.ToUpper & "',
'" & AssetGL.Trim.ToUpper & "','" & LiabilityGL.Trim.ToUpper & "','" & IncomeGL.Trim.ToUpper & "','" & ExpenseGL.Trim.ToUpper & "',
'" & ChargeGL.Trim.ToUpper & "','" & FeeGL.Trim.ToUpper & "','" & IntReceivable.Trim.ToUpper & "','" & IntPayable.Trim.ToUpper & "','" & IntRate.Trim.ToUpper & "','" & IntFreq.Trim.ToUpper & "','" & IntAccrualFreq.Trim.ToUpper & "',
'" & MinInterestBal.Trim.ToUpper & "','" & MinAccountBal.Trim.ToUpper & "','" & DebitsNoPerMonth.Trim.ToUpper & "','" & txtBranchCode.Text.Trim.ToUpper & "','" & drpdPrefix.Text.Trim.ToUpper & "','" & txtFirstName.Text.Trim.ToUpper & "','" & txtMiddleName.Text.Trim.ToUpper & "',
'" & txtLastName.Text.Trim.ToUpper & "','" & txtTelNo.Text.Trim.ToUpper & "','" & txtMobileNo.Text.Trim.ToUpper & "','" & txtEmail.Text.ToLower.Trim & "','" & txtAddress1.Text.Trim.ToUpper & "','" & txtAddress2.Text.Trim.ToUpper & "','" & txtAddress3.Text.Trim.ToUpper & "',
'" & txtTillNo.Text.Trim.ToUpper & "','" & txtIDNo.Text.Trim.ToUpper & "','" & txtRelOfficer.Text.Trim.ToUpper & "','" & drpMaritalStatus.Text.Trim.ToUpper & "','" & txtSpouseName.Text.Trim.ToUpper & "',
'" & txtDepemdant.Text.Trim.ToUpper & "','" & txtOccupation.Text.Trim.ToUpper & "','" & txtEmployer.Text.Trim.ToUpper & "','" & txtBizName.Text.Trim.ToUpper & "',
'" & txtBizRegDate.Text.Trim.ToUpper & "','" & txtBizNature.Text.Trim.ToUpper & "','" & txtDirName.Text.Trim.ToUpper & "','" & txtgrpName.Text.Trim.ToUpper & "','" & txtgrpMemberNo.Text.Trim.ToUpper & "','" & txtMaleNo.Text.Trim.ToUpper & "','" & txtFemaleNo.Text.Trim.ToUpper & "','" & txtRegion.Text.Trim.ToUpper & "','" & txtDistrict.Text.Trim.ToUpper & "','" & txtArea.Text.Trim.ToUpper & "','" & txtResidential.Text.Trim.ToUpper & "','" & txtSex.Text.Trim.ToUpper & "','" & txtPassport.Text.Trim.ToUpper & "','" & txtNationality.Text.Trim.ToUpper & "','" & txtDoB.Text.Trim.ToUpper & "','" & txtLanguage.Text.Trim.ToUpper & "','" & txtTelNo.Text.Trim.ToUpper & "',0,0.00,0.00,0.00)" ')"
                MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
                lblSuccess.ForeColor = Drawing.Color.Green
                lblSuccess.Text = "Account Details have been saved successfully"
                DisplayDetails("All")
                txtCustomerNo.ReadOnly = False
            Else
                'Dim MySqlaction1 As String
                'Dim mySqlAction As String = ""
                'Dim MyCurrencyName As String = ""
                'Dim MyDBAction As New DataManagement
                'MySqlaction1 = "update tbAccount set AccountName ='" & txtAcountName.Text.ToUpper.Trim & "', Sector ='" & txtSector.Text.ToUpper.Trim & "', SubSector ='" & txtSubSector.Text.ToUpper.Trim & "', C_CustomerType ='" & drpAccountType.Text.ToUpper.Trim & "',  C_Address1 ='" & txtAddress1.Text.ToUpper.Trim & "',C_Address2 ='" & txtAddress2.Text.ToUpper.Trim & "' ,C_Address3 = '" & txtAddress3.Text.ToUpper.Trim & "', C_Email = '" & txtEmail.Text.Trim & "', C_TINNo = '" & txtTillNo.Text.ToUpper.Trim & "', C_Telephone = '" & txtTelNo.Text.ToUpper.Trim & "',C_MobileNo = '" & txtMobileNo.Text.ToUpper.Trim & "',I_FirstName = '" & txtFirstName.Text.ToUpper.Trim & "',I_MiddleName = '" & txtMiddleName.Text.ToUpper.Trim & "',I_LastName = '" & txtLastName.Text.ToUpper.Trim & "',I_NationalID = '" & txtIDNo.Text.ToUpper.Trim & "',C_RelationShipOfficer = '" & txtRelOfficer.Text.ToUpper.Trim & "',I_MaritalStatus = '" & drpMaritalStatus.Text.ToUpper.Trim & "',I_SpouseName = '" & txtSpouseName.Text.ToUpper.Trim & "',I_Dependants = '" & txtDepemdant.Text.ToUpper.Trim & "',I_Occupation = '" & txtOccupation.Text.ToUpper.Trim & "',I_EmployerName = '" & txtEmployer.Text.ToUpper.Trim & "',B_BusinessName = '" & txtBizName.Text.ToUpper.Trim & "',B_RegDate = '" & txtBizRegDate.Text.ToUpper.Trim & "',B_RegName = '" & txtRegNo.Text.ToUpper.Trim & "',B_NatureofBusiness = '" & txtBizNature.Text.ToUpper.Trim & "',B_DirectorName = '" & txtDirName.Text.ToUpper.Trim & "',G_GroupName = '" & txtgrpName.Text.ToUpper.Trim & "',G_NoofMembers = '" & txtgrpMemberNo.Text.ToUpper.Trim & "',G_NoofMale = '" & txtMaleNo.Text.ToUpper.Trim & "',G_NoofFemale = '" & txtFemaleNo.Text.ToUpper.Trim & "' where AccountNo ='" & txtAccountNo.Text & "'"
                'MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
                'DisplayDetails("All")
                'lblSuccess.Text = "Account Account details have been Updated successfully"
                'txtCustomerNo.ReadOnly = False
                Dim myaccountno As String = ""
                myaccountno = Session("AccountNo")
                Dim trandate As String = Date.Today.ToString("dd-MMM-yy")
                Dim MyDBAction As New DataManagement
                Dim MySqlaction1 As String
                MySqlaction1 = "update tbAccount set AccountName ='" & txtAcountName.Text.ToUpper.Trim & "', Sector ='" & drpSector.SelectedValue.ToUpper.Trim & "', SubSector ='" & drpSubSector.SelectedValue.ToUpper.Trim & "', AccountType ='" & drpAccountType.Text.ToUpper.Trim & "',  C_Address1 ='" & txtAddress1.Text.ToUpper.Trim & "',C_Address2 ='" & txtAddress2.Text.ToUpper.Trim & "' ,C_Address3 = '" & txtAddress3.Text.ToUpper.Trim & "', C_Email = '" & txtEmail.Text.Trim & "', C_TINNo = '" & txtTillNo.Text.ToUpper.Trim & "', C_Telephone = '" & txtTelNo.Text.ToUpper.Trim & "',C_MobileNo = '" & txtMobileNo.Text.ToUpper.Trim & "',I_FirstName = '" & txtFirstName.Text.ToUpper.Trim & "',I_MiddleName = '" & txtMiddleName.Text.ToUpper.Trim & "',I_LastName = '" & txtLastName.Text.ToUpper.Trim & "',I_NationalID = '" & txtIDNo.Text.ToUpper.Trim & "',C_RelationShipOfficer = '" & txtRelOfficer.Text.ToUpper.Trim & "',I_MaritalStatus = '" & drpMaritalStatus.Text.ToUpper.Trim & "',I_SpouseName = '" & txtSpouseName.Text.ToUpper.Trim & "',I_Dependants = '" & txtDepemdant.Text.ToUpper.Trim & "',I_Occupation = '" & txtOccupation.Text.ToUpper.Trim & "',I_EmployerName = '" & txtEmployer.Text.ToUpper.Trim & "',B_BusinessName = '" & txtBizName.Text.ToUpper.Trim & "',B_RegDate = '" & txtBizRegDate.Text.ToUpper.Trim & "',B_RegName = '" & txtRegNo.Text.ToUpper.Trim & "',B_NatureofBusiness = '" & txtBizNature.Text.ToUpper.Trim & "',B_DirectorName = '" & txtDirName.Text.ToUpper.Trim & "',G_GroupName = '" & txtgrpName.Text.ToUpper.Trim & "',G_NoofMembers = '" & txtgrpMemberNo.Text.ToUpper.Trim & "',G_NoofMale = '" & txtMaleNo.Text.ToUpper.Trim & "',G_NoofFemale = '" & txtFemaleNo.Text.ToUpper.Trim & "',C_Region = '" & txtRegion.Text.ToUpper.Trim & "' ,C_District = '" & txtDistrict.Text.ToUpper.Trim & "',C_Area = '" & txtArea.Text.ToUpper.Trim & "',I_ResidentStatus ='" & txtResidential.Text.ToUpper.Trim & "',I_Sex = '" & txtSex.Text.ToUpper.Trim & "',I_PassportNO = '" & txtPassport.Text.ToUpper.Trim & "',I_Nationality = '" & txtNationality.Text.ToUpper.Trim & "',I_DoB = '" & txtDoB.Text.ToUpper.Trim & "',I_Language = '" & txtLanguage.Text.ToUpper.Trim & "',B_TelNo = '" & txtTelNo.Text.ToUpper.Trim & "' where AccountNo ='" & myaccountno & "'"
                MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
                DisplayDetails("All")
                lblSuccess.ForeColor = Drawing.Color.Green
                lblSuccess.Text = "Saving Account details have been Updated successfully"
                txtCustomerNo.Enabled = False
                txtBranchCode.Enabled = False
                txtAccountCode.Enabled = False
                txtAccountNo.Enabled = False
                txtBranchName.Enabled = False
                txtCurrency.Enabled = False
                txtProductName.Enabled = False
                btnCheckBranch.Visible = False
                btnSearchCode.Visible = False
                'cmasearch.Visible = False
                txtCustomerNo.ReadOnly = False
                txtCustomerNo.Text = ""
                txtAccountCode.Text = ""
                txtAccountNo.Text = ""
                HiddenField1.Value = ""
                txtAcountName.Text = ""
                txtCurrency.Text = ""
                drpSector.Text = ""
                drpSubSector.Text = ""
                txtCustomerNo.Text = ""
                txtBranchCode.Text = ""
                txtCustomerNo.Text = ""
                drpAccountType.Text = ""
                txtFirstName.Text = ""
                drpdPrefix.Text = ""
                txtFirstName.Text = ""
                txtMiddleName.Text = ""
                'txtFreezeStatus.Text = ""
                'txtFullName.Text = ""
                txtTelNo.Text = ""
                txtMobileNo.Text = ""
                txtEmail.Text = ""
                txtAddress1.Text = ""
                txtAddress2.Text = ""
                txtAddress3.Text = ""
                txtTillNo.Text = ""
                'txtNationality.Text = ""
                txtRelOfficer.Text = ""
                drpMaritalStatus.Text = ""
                txtSpouseName.Text = ""
                txtDepemdant.Text = ""
                txtOccupation.Text = ""
                txtEmployer.Text = ""
                txtBizName.Text = ""
                'txtLastCRactvtyDate.Text = ""
                'txtgrpDirName.Text = ""
                txtgrpMemberNo.Text = ""
                txtMaleNo.Text = ""
                txtBranchCode.Text = ""
                txtFemaleNo.Text = ""
                txtBranchName.Text = ""
                txtProductName.Text = ""
                txtLastName.Text = ""
                txtIDNo.Text = ""
                txtBizRegDate.Text = ""
                txtRegNo.Text = ""
                txtBizNature.Text = ""
                txtDirName.Text = ""
                txtgrpName.Text = ""
                txtRegion.Text = ""
                txtDistrict.Text = ""
                txtArea.Text = ""
                txtSex.Text = ""
                txtPassport.Text = ""
                txtNationality.Text = ""
                txtDoB.Text = ""
                txtLanguage.Text = ""
                txtResidential.Text = ""
                'txtBizTel.Text = ""
            End If
        Catch ex As Exception
        End Try
        txtBranchCode.Text = ""
        txtBranchName.Text = ""
        txtAccountCode.Text = ""
        txtProductName.Text = ""
        txtCustomerNo.Text = ""
        ClearControls()
    End Sub
    Protected Sub DropDownList1_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        DisplayDetails("All")
    End Sub
    Protected Sub CmdApprovalList_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        ClearControls()
        txtBranchCode.Text = ""
        txtBranchName.Text = ""
        txtAccountCode.Text = ""
        txtProductName.Text = ""
        txtCustomerNo.Text = ""
    End Sub
    Protected Sub btnClear_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnClear.Click
        ClearControls()
    End Sub
    Protected Sub txtAdress3_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs)

    End Sub
    Protected Sub btnCheckBranch_Click(sender As Object, e As EventArgs) Handles btnCheckBranch.Click
        GetBranchDetails()
    End Sub
    Public Sub GetBranchDetails()
        Try
            If txtBranchCode.Text = "" Then
                txtBranchName.Text = ""
                lblCheckdetails.Text = "Enter branch code"
                txtBranchCode.Focus()
                Exit Sub
            Else
                lblCheckdetails.Text = ""

            End If
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select BranchName from Branches where BranchCode = '" & txtBranchCode.Text & "'"
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                txtBranchName.Text = MyReader4("BranchName").ToString
                lblCheckdetails.Text = ""
            Else
                lblCheckdetails.Text = "This branch code does not exist"
                txtBranchName.Text = ""
                Exit Sub
            End If
            MyReader4.Close()
        Catch ex As Exception
            'Label4.Text = "Error Pulling Account Details, "
        End Try
    End Sub
    Protected Sub GenerateAccountNumber(BranchCode, CustomerNo, AccountPrefix)
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch5 As String = "select count(AccountNo) as AccountCount from tbAccount where CustomerNo = '" & txtCustomerNo.Text.Trim & "'"
            Dim MyReader5 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch5, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader5.HasRows = True Then
                MyReader5.Read()
                Dim accountCount As String = MyReader5("AccountCount").ToString
                Dim newAccountSuffix As String = ""
                If accountCount = 0 Then
                    newAccountSuffix = "01"
                ElseIf accountCount = 1 Then
                    newAccountSuffix = "02"
                ElseIf accountCount = 2 Then
                    newAccountSuffix = "03"
                ElseIf accountCount = 3 Then
                    newAccountSuffix = "04"
                ElseIf accountCount = 4 Then
                    newAccountSuffix = "05"
                End If
                txtAccountNo.Text = BranchCode + "-" + CustomerNo + "-" + AccountPrefix + "-" + newAccountSuffix
            End If
            MyReader5.Close()
        Catch ex As Exception

        End Try
    End Sub
    Protected Sub GridView1_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)

    End Sub
    Protected Sub LinkButton1_Click(ByVal sender As Object, ByVal e As System.EventArgs)

    End Sub
    Protected Sub cmddelete_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmddelete.Click
        GridView1.EditIndex = -1
        DisplayDetails("All")
        Try
            Dim mySqlAction As String = ""
            Dim MyDBAction As New DataManagement
            mySqlAction = "Delete from tbAccount where AccountNo='" & HiddenField1.Value.ToString & "'"
            MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Delete)
            Me.lblSuccess.Text = "Account Details Have Been Deleted Successfully"
            DisplayDetails("All")
            txtBranchCode.Text = ""
            txtBranchName.Text = ""
            txtAccountCode.Text = ""
            txtProductName.Text = ""
            txtCustomerNo.Text = ""
            ClearControls()
        Catch ex As Exception
        End Try
    End Sub
    Protected Sub txtOrgCode_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs)
    End Sub
    Protected Sub GetCustomerData()
        Try
            If txtCustomerNo.Text = "" Then
                txtAccountNo.Text = ""
                lblCheckdetails.Text = "Enter customer number"
                txtProductCode.Focus()
                ClearControls()
                Exit Sub
            Else
                lblCheckdetails.Text = ""
            End If
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select C_FullName,C_CustomerNo,C_CustomerType,I_PreFix,I_FirstName,I_MiddleName,I_LastName,C_FullName,C_Telephone,C_MobileNo,C_Email,C_Address1,C_Address2,C_Address3,C_Region,C_District,C_Area,C_TINNo,I_NationalID,I_PassportNO,C_RelationShipOfficer,I_Nationality,I_DoB,I_ResidentStatus,I_Language,I_Sex,I_MaritalStatus,I_SpouseName,I_Dependants,I_Occupation,I_EmployerName,B_BusinessName,B_RegDate,B_RegName,B_TelNo,B_NatureofBusiness,B_DirectorName,G_GroupName,G_NoofMembers,G_NoofMale,G_NoofFemale from tbCustomer where C_CustomerNo = '" & txtCustomerNo.Text & "'" ' ='" & Me.HiddenField1.Value & "' "
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()
                txtAcountName.Text = MyReader4("C_FullName").ToString
                txtCustomerNo.Text = MyReader4("C_CustomerNo").ToString
                drpAccountType.SelectedValue = MyReader4("C_CustomerType").ToString
                drpdPrefix.SelectedValue = MyReader4("I_PreFix").ToString
                txtFirstName.Text = MyReader4("I_FirstName").ToString
                txtMiddleName.Text = MyReader4("I_MiddleName").ToString
                txtLastName.Text = MyReader4("I_LastName").ToString
                txtTelNo.Text = MyReader4("C_Telephone").ToString
                txtMobileNo.Text = MyReader4("C_MobileNo").ToString
                txtEmail.Text = MyReader4("C_Email").ToString
                txtAddress1.Text = MyReader4("C_Address1").ToString
                txtAddress2.Text = MyReader4("C_Address2").ToString
                txtAddress3.Text = MyReader4("C_Address3").ToString
                txtTillNo.Text = MyReader4("C_TINNo").ToString
                txtIDNo.Text = MyReader4("I_NationalID").ToString
                txtRelOfficer.Text = MyReader4("C_RelationShipOfficer").ToString
                drpMaritalStatus.SelectedValue = MyReader4("I_MaritalStatus").ToString
                txtSpouseName.Text = MyReader4("I_SpouseName").ToString
                txtAcountName.Text = MyReader4("C_FullName").ToString
                txtDepemdant.Text = MyReader4("I_Dependants").ToString
                txtOccupation.Text = MyReader4("I_Occupation").ToString
                txtEmployer.Text = MyReader4("I_EmployerName").ToString
                txtBizName.Text = MyReader4("B_BusinessName").ToString
                txtBizRegDate.Text = MyReader4("B_RegDate").ToString
                txtBizNature.Text = MyReader4("B_NatureofBusiness").ToString
                txtDirName.Text = MyReader4("B_DirectorName").ToString
                txtgrpName.Text = MyReader4("G_GroupName").ToString
                txtgrpMemberNo.Text = MyReader4("G_NoofMembers").ToString
                txtMaleNo.Text = MyReader4("G_NoofMale").ToString
                txtFemaleNo.Text = MyReader4("G_NoofFemale").ToString
                txtRegion.Text = MyReader4("C_Region").ToString
                txtDistrict.Text = MyReader4("C_District").ToString
                txtArea.Text = MyReader4("C_Area").ToString
                txtSex.Text = MyReader4("I_Sex").ToString
                txtPassport.Text = MyReader4("I_PassportNO").ToString
                txtNationality.Text = MyReader4("I_Nationality").ToString
                txtDoB.Text = MyReader4("I_DoB").ToString
                txtLanguage.Text = MyReader4("I_Language").ToString
                txtResidential.Text = MyReader4("I_ResidentStatus").ToString
                txtRegNo.Text = MyReader4("B_RegName").ToString
                If txtBranchName.Text = "" And txtProductName.Text = "" Then
                    lblCheckdetails.Text = "Missing branch or product details"
                    ClearControls()
                Else
                    GenerateAccountNumber(txtBranchCode.Text, txtCustomerNo.Text, txtProductCode.Text)
                End If
            Else
                lblCheckdetails.Text = "This customer number does not exist"
                ClearControls()
                txtAccountNo.Text = ""
                Exit Sub
            End If
            MyReader4.Close()
        Catch ex As Exception
        End Try
    End Sub
    Protected Sub GetProductData()
        Try
            If txtAccountCode.Text = "" Then
                txtProductName.Text = ""
                lblCheckdetails.Text = "Enter product code"
                txtProductCode.Focus()
                Exit Sub
            Else
                lblCheckdetails.Text = ""
            End If
            Dim MyDatObject As New DataManagement
            Dim mySearch5 As String = "select AccountCode,AccountPrefix,ProductDefCode,AccountName,DormancyDays,GLType,AssetGL,LiabilityGL,IncomeGL,ExpenseGL,ChargeGL,FeeGL,IntReceivable,IntPayable,IntRate,IntFreq,IntAccrualFreq,MinInterestBal,MinAccountBal,DebitsNoPerMonth,Amount from [dbo].[tbSavingsAccountProduct] where AccountCode = '" & txtAccountCode.Text & "'"
            Dim MyReader5 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch5, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader5.HasRows = True Then
                MyReader5.Read()
                txtProductCode.Text = MyReader5("AccountPrefix").ToString
                txtProductName.Text = MyReader5("AccountName").ToString
                'AccountCode = mySearch5("AccountCode").ToString
                AccountPrefix = MyReader5("AccountPrefix").ToString
                AccountName = MyReader5("AccountName").ToString
                test = MyReader5("ProductDefCode").ToString
                DormancyDays = MyReader5("DormancyDays").ToString
                GLType = MyReader5("GLType").ToString
                AssetGL = MyReader5("AssetGL").ToString
                LiabilityGL = MyReader5("LiabilityGL").ToString
                IncomeGL = MyReader5("IncomeGL").ToString
                ExpenseGL = MyReader5("ExpenseGL").ToString
                ChargeGL = MyReader5("ChargeGL").ToString
                FeeGL = MyReader5("FeeGL").ToString
                IntReceivable = MyReader5("IntReceivable").ToString
                IntPayable = MyReader5("IntPayable").ToString
                IntRate = MyReader5("IntRate").ToString
                IntFreq = MyReader5("IntFreq").ToString
                IntAccrualFreq = MyReader5("IntAccrualFreq").ToString
                MinInterestBal = MyReader5("MinInterestBal").ToString
                MinAccountBal = MyReader5("MinAccountBal").ToString
                DebitsNoPerMonth = MyReader5("DebitsNoPerMonth").ToString
                Amount = MyReader5("Amount").ToString
            Else
                lblCheckdetails.Text = "This product code does not exist"
                txtProductName.Text = ""
                Exit Sub
            End If
            MyReader5.Close()
        Catch ex As Exception

        End Try
    End Sub
    Protected Sub cmasearchCust_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmasearchCust.Click
        GetCustomerData()

    End Sub
    Protected Sub Button3_Click(ByVal sender As Object, ByVal e As System.EventArgs)

    End Sub
    Protected Sub txtEmail_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs)

    End Sub
    Protected Sub RdoSearch_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)

    End Sub
    Protected Sub CmdSearch_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        DisplayDetails("Filter")
    End Sub
    Protected Sub CmdViewAll_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        DisplayDetails("All")
    End Sub
    Protected Sub txtSearch_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs)

    End Sub

    Protected Sub txtAccountCode_TextChanged(sender As Object, e As EventArgs) Handles txtCustomerNo.TextChanged

    End Sub
    Protected Sub btnSearchCode_Click(sender As Object, e As EventArgs) Handles btnSearchCode.Click
        GetProductData()
    End Sub
    Protected Sub ClearControls()
        txtAccountNo.Text = ""
        HiddenField1.Value = ""
        txtAcountName.Text = ""
        txtCurrency.Text = ""
        drpSector.Text = ""
        drpSubSector.Text = ""
        drpAccountType.Text = ""
        txtFirstName.Text = ""
        drpdPrefix.Text = ""
        txtFirstName.Text = ""
        txtMiddleName.Text = ""
        txtTelNo.Text = ""
        txtMobileNo.Text = ""
        txtEmail.Text = ""
        txtAddress1.Text = ""
        txtAddress2.Text = ""
        txtAddress3.Text = ""
        txtTillNo.Text = ""
        txtRelOfficer.Text = ""
        drpMaritalStatus.Text = ""
        txtSpouseName.Text = ""
        txtDepemdant.Text = ""
        txtOccupation.Text = ""
        txtEmployer.Text = ""
        txtBizName.Text = ""
        txtgrpMemberNo.Text = ""
        txtMaleNo.Text = ""
        txtFemaleNo.Text = ""
        txtLastName.Text = ""
        txtIDNo.Text = ""
        txtBizRegDate.Text = ""
        txtRegNo.Text = ""
        txtBizNature.Text = ""
        txtDirName.Text = ""
        txtgrpName.Text = ""
        txtRegion.Text = ""
        txtDistrict.Text = ""
        txtArea.Text = ""
        txtSex.Text = ""
        txtDoB.Text = ""
    End Sub



    Protected Sub btncalendars_Click(sender As Object, e As ImageClickEventArgs) Handles btncalendars.Click
        DOBDatePicker.Visible = True
    End Sub

    Protected Sub btncalendar_Click(sender As Object, e As ImageClickEventArgs) Handles btncalendar.Click
        BizRegDatePicker.Visible = True
    End Sub


    'Protected Sub btncalendar__Click(sender As Object, e As ImageClickEventArgs) Handles btncalendar_.Click
    '    GrpRegDatePicker.Visible = True
    'End Sub


    Protected Sub DOBDatePicker_SelectionChanged(sender As Object, e As EventArgs) Handles DOBDatePicker.SelectionChanged
        Dim sysUIFormat As String = CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern
        DOBDatePicker.Visible = False
        txtDoB.Text = DOBDatePicker.SelectedDate.ToString("dd/MM/yyyy")
    End Sub

    Protected Sub BizRegDatePicker_SelectionChanged(sender As Object, e As EventArgs) Handles BizRegDatePicker.SelectionChanged
        Dim sysUIFormat As String = CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern
        BizRegDatePicker.Visible = False
        txtBizRegDate.Text = BizRegDatePicker.SelectedDate.ToString("dd/MM/yyyy")
    End Sub

    'Protected Sub GrpRegDatePicker_SelectionChanged(sender As Object, e As EventArgs) Handles GrpRegDatePicker.SelectionChanged
    '    Dim sysUIFormat As String = CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern
    '    GrpRegDatePicker.Visible = False
    '    txtgrpRegDate.Text = GrpRegDatePicker.SelectedDate.ToString("dd/MM/yyyy")
    'End Sub
End Class