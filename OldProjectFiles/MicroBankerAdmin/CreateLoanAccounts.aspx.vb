Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Imports MicroBankerAdmin.Models
Imports System.Data.SqlClient
Imports System.Globalization

Public Class CreateLoanAccounts
    Inherits System.Web.UI.Page
    Public AccountCode As String = "", ProductName As String = "", AccountPrefix As String = "", AccountName As String = "", DormancyDays As String = "", GLType As String = "", AssetGL As String = "", LiabilityGL As String = "", IncomeGL As String = "", ExpenseGL As String = "", ChargeGL As String = "", FeeGL As String = "", IntReceivable As String = "", IntPayable As String = "", IntRate As String = "", IntFreq As String = "", IntAccrualFreq As String = "", MinInterestBal As String = "", MinAccountBal As String = "", DebitsNoPerMonth As String = "", Amount As String = "", test As String = ""
    Dim Account As Account = New Account()
    Public AvailBalance As Decimal = 0.00
    Public BkBalance As Decimal = 0.00

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
            StartDatePicker.Visible = False
            EndDatePicker.Visible = False
            FirstEMIDatePicker.Visible = False
        End If
    End Sub
    Private Sub GetRegion()
        'Load Products
        Try
            Dim MyDataTable As New Data.DataTable
            Dim MyDBAction As New DataManagement
            MyDataTable = TryCast(MyDBAction.DBAction("select RegionCode,RegionName  from Region", DataManagement.DBActionType.SqlDataTable), Data.DataTable)
            If MyDataTable IsNot Nothing Then
                With drpRegion
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
    Private Sub GetArea()
        'Load Products
        Try
            Dim MyDataTable As New Data.DataTable
            Dim MyDBAction As New DataManagement
            MyDataTable = TryCast(MyDBAction.DBAction("select AreaCode,AreaName  from Area", DataManagement.DBActionType.SqlDataTable), Data.DataTable)
            If MyDataTable IsNot Nothing Then
                With drpArea
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
    Private Sub GetDistrict()
        'Load Products
        Try
            Dim MyDataTable As New Data.DataTable
            Dim MyDBAction As New DataManagement
            MyDataTable = TryCast(MyDBAction.DBAction("select DistrictCode,DistrictName  from District", DataManagement.DBActionType.SqlDataTable), Data.DataTable)
            If MyDataTable IsNot Nothing Then
                With drpDistrict
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
                    mySqlAction = "select CustomerNo,AccountType,BranchName,AccountNo,AccountName,Currency,Sector,SubSector,IntRate,DebitNoStatus,BranchCode,AccountPrefix,C_Address1,LoanAmount,LoanStartDate,LoanEndDate,RepaymentPeriod from tbAccount where ProductDefCode = 'LA' and Approved = '0'"
                Case "Filter"
                    If RdoSearch.Items(0).Selected = True Then
                        Processfilter = txtSearch.Text.ToUpper
                        Processfilter = "%" & Processfilter & "%"
                        mySqlAction = "select CustomerNo,AccountType,BranchName,AccountNo,AccountName,Currency,Sector,SubSector,IntRate,DebitNoStatus,BranchCode,AccountPrefix,C_Address1,LoanAmount,LoanStartDate,LoanEndDate,RepaymentPeriod from tbAccount where  AccountNo LIKE  '" & Processfilter & "'"
                    End If
                    If RdoSearch.Items(1).Selected = True Then
                        Processfilter = txtSearch.Text.ToUpper
                        Processfilter = "%" & Processfilter & "%"
                        mySqlAction = "select CustomerNo,AccountType,BranchName,AccountNo,AccountName,Currency,Sector,SubSector,IntRate,DebitNoStatus,BranchCode,AccountPrefix,C_Address1,LoanAmount,LoanStartDate,LoanEndDate,RepaymentPeriod from tbAccount where AccountName  LIKE '" & Processfilter & "'"
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
        Try
            Select Case ShowType
                Case "ViewDetails"
                    If MyReader.HasRows Then
                        MyReader.Read()
                        GetSectors()
                        GetSubSectors()
                        GetRegion()
                        GetArea()
                        GetDistrict()
                        txtBranchCode.Text = MyReader("BranchCode").ToString
                        txtAccountCode.Text = MyReader("AccountCode").ToString
                        txtAccountNo.Text = MyReader("AccountNo").ToString
                        Session("AccountNo") = MyReader("AccountNo").ToString
                        txtAcountName.Text = MyReader("AcountName").ToString
                        txtBranchName.Text = MyReader("BranchName").ToString
                        txtCurrency.Text = MyReader("Currency").ToString
                        txtSector.Text = MyReader("Sector").ToString
                        txtSubSector.Text = MyReader("SubSector").ToString
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
                        drpRegion.SelectedValue = MyReader("C_Region").ToString
                        drpArea.SelectedValue = MyReader("C_Area").ToString
                        drpDistrict.SelectedValue = MyReader("C_District").ToString
                        'added 06082021
                        txtSex.Text = MyReader("I_Sex").ToString
                        txtPassport.Text = MyReader("I_PassportNO").ToString
                        txtNationality.Text = MyReader("I_Nationality").ToString
                        txtLanguage.Text = MyReader("I_Language").ToString
                        txtDoB.Text = MyReader("I_DoB").ToString
                        txtResidential.Text = MyReader("I_ResidentStatus").ToString
                        'txtBizTel.Text = MyReader("B_TelNo").ToString

                        txtGracePeriod.Text = MyReader("GracePeriod").ToString
                        txtLoanStartDate.Text = MyReader("LoanStartDate").ToString
                        txtLoanEndDate.Text = MyReader("LoanEndDate").ToString
                        drpRepaymentFrequency.Text = MyReader("RepaymentFrequency").ToString
                        txtNoInstallments.Text = MyReader("NoInstallments").ToString
                        txtTenure.Text = MyReader("Tenure").ToString
                        txtInterestRate.Text = MyReader("IntRate").ToString
                        txtCharges.Text = MyReader("Charges").ToString
                        txtLoanAmount.Text = MyReader("LoanAmount").ToString
                        txtInterestCalc.Text = MyReader("InterestCalc").ToString
                        txtFirstEMI.Text = MyReader("FirstEMI").ToString
                        txtCollateral.Text = MyReader("Collateral").ToString
                        txtRepaymentMode.Text = MyReader("RepaymentMode").ToString
                        txtRepaymentDate.Text = MyReader("RepaymentDate").ToString
                        txtRepaymentPeriod.Text = MyReader("RepaymentPeriod").ToString
                        txtSkipInstallments.Text = MyReader("SkipInstallments").ToString
                        txtRepaymentAmt.Text = MyReader("RepaymentAmount").ToString
                        txtGuarantorsAccountName.Text = MyReader("GuarantorsAccountName").ToString
                        txtSettlementAccountName.Text = MyReader("SettlementsAccountName").ToString
                        txtGuarantorsAccountNo.Text = MyReader("GuarantorsAccountNo").ToString
                        txtSettlementAccountNo.Text = MyReader("SettlementsAccountNo").ToString
                        drpRepaymentFrequency.SelectedValue = MyReader("RepaymentFrequency").ToString
                        drpSector.SelectedValue = MyReader("Sector").ToString
                        drpSubSector.SelectedValue = MyReader("SubSector").ToString
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
            If txtCustomerNo.Text.ToUpper() = "" Then
                lblCheckdetails.Text = "Please enter customer no"
                Exit Sub
            Else
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
                    Dim FirstEMI, LoanStartDate, LoanEndDate, RepaymentDate, dateToday As DateTime
                    dateToday = DateTime.Now.ToString("yyyy/MM/dd")
                    FirstEMI = dateToday.AddMonths(1)
                    LoanStartDate = dateToday
                    LoanEndDate = dateToday.AddMonths(txtRepaymentPeriod.Text)
                    RepaymentDate = dateToday.AddMonths(1)
                    mySqlAction = "Insert into tbAccount(CustomerNo,AccountType,AccountTypeCode,BranchName,AccountNo,AccountName,Currency,Sector,SubSector,AccountStatus,AccountCode,AccountPrefix,AccountTypeName,ProductDefCode,DormancyDays,GLType,AssetGL_Control,LiabilityGL_Control,IncomeGL,ExpenseGL,ChargeGL,FeeGL,IntReceivable,IntPayable,IntRate,IntFreq,IntAccrualFreq,MinInterestBal,MinAccountBal,DebitsNoPerMonth,BranchCode,I_PreFix,I_FirstName,I_MiddleName,I_LastName,C_Telephone,C_MobileNo,C_Email,C_Address1,C_Address2,C_Address3,C_TINNo,I_NationalID,C_RelationShipOfficer,I_MaritalStatus,I_SpouseName,I_Dependants,I_Occupation,I_EmployerName,B_BusinessName,B_RegDate,B_NatureofBusiness,B_DirectorName,G_GroupName,G_NoofMembers,G_NoofMale,G_NoofFemale,C_Region,C_District,C_Area,I_ResidentStatus,I_Sex,I_PassportNO,I_Nationality,I_DoB,I_Language,B_TelNo,Approved, GracePeriod,LoanStartDate,LoanEndDate,RepaymentFrequency,NoInstallments,OustadingInstallments,Tenure,Charges,LoanAmount,RepaymentAmount,InterestCalc,FirstEMI,Collateral,RepaymentDate,RepaymentPeriod,SkipInstallments,RepaymentMode,SettlementsAccountNo,GuarantorsAccountNo,SettlementsAccountName,GuarantorsAccountName,DisburseStatus,AccountStatusCode,SOD_Status,NextRepaymentDateCalc,NextRepaymentDatePost,RepaymentFreqCode,BookBalance,AvailBal,LastIntCalculateDate,NextRepaymentDate,BlockAmount)values
('" & txtCustomerNo.Text.Trim.ToUpper & "','" & drpAccountType.Text.Trim.ToUpper & "','" & accountTypeCode & "','" & txtBranchName.Text.Trim.ToUpper & "',
'" & txtAccountNo.Text.Trim.ToUpper & "','" & txtAcountName.Text.Trim.ToUpper & "','" & txtCurrency.Text.Trim.ToUpper & "',
'" & drpSector.SelectedValue.Trim.ToUpper & "','" & drpSubSector.SelectedValue.Trim.ToUpper & "','Open','" & txtAccountCode.Text.Trim.ToUpper & "','" & AccountPrefix.Trim.ToUpper & "','" & txtProductName.Text.Trim.ToUpper & "','" & test & "','" & DormancyDays.Trim.ToUpper & "','" & GLType.Trim.ToUpper & "',
'" & AssetGL.Trim.ToUpper & "','" & LiabilityGL.Trim.ToUpper & "','" & IncomeGL.Trim.ToUpper & "','" & ExpenseGL.Trim.ToUpper & "',
'" & ChargeGL.Trim.ToUpper & "','" & FeeGL.Trim.ToUpper & "','" & IntReceivable.Trim.ToUpper & "','" & IntPayable.Trim.ToUpper & "','" & txtInterestRate.Text.Trim.ToUpper & "','" & IntFreq.Trim.ToUpper & "','" & IntAccrualFreq.Trim.ToUpper & "',
'" & MinInterestBal.Trim.ToUpper & "','" & MinAccountBal.Trim.ToUpper & "','" & DebitsNoPerMonth.Trim.ToUpper & "','" & txtBranchCode.Text.Trim.ToUpper & "','" & drpdPrefix.Text.Trim.ToUpper & "','" & txtFirstName.Text.Trim.ToUpper & "','" & txtMiddleName.Text.Trim.ToUpper & "',
'" & txtLastName.Text.Trim.ToUpper & "','" & txtTelNo.Text.Trim.ToUpper & "','" & txtMobileNo.Text.Trim.ToUpper & "','" & txtEmail.Text.ToLower.Trim & "','" & txtAddress1.Text.Trim.ToUpper & "','" & txtAddress2.Text.Trim.ToUpper & "','" & txtAddress3.Text.Trim.ToUpper & "',
'" & txtTillNo.Text.Trim.ToUpper & "','" & txtIDNo.Text.Trim.ToUpper & "','" & txtRelOfficer.Text.Trim.ToUpper & "','" & drpMaritalStatus.Text.Trim.ToUpper & "','" & txtSpouseName.Text.Trim.ToUpper & "',
'" & txtDepemdant.Text.Trim.ToUpper & "','" & txtOccupation.Text.Trim.ToUpper & "','" & txtEmployer.Text.Trim.ToUpper & "','" & txtBizName.Text.Trim.ToUpper & "',
'" & txtBizRegDate.Text.Trim.ToUpper & "','" & txtBizNature.Text.Trim.ToUpper & "','" & txtDirName.Text.Trim.ToUpper & "','" & txtgrpName.Text.Trim.ToUpper & "','" & txtgrpMemberNo.Text.Trim.ToUpper & "','" & txtMaleNo.Text.Trim.ToUpper & "','" & txtFemaleNo.Text.Trim.ToUpper & "','" & drpRegion.SelectedValue.Trim.ToUpper & "','" & drpDistrict.SelectedValue.Trim.ToUpper & "','" & drpArea.SelectedValue.Trim.ToUpper & "','" & txtResidential.Text.Trim.ToUpper & "','" & txtSex.Text.Trim.ToUpper & "','" & txtPassport.Text.Trim.ToUpper & "','" & txtNationality.Text.Trim.ToUpper & "','" & txtDoB.Text.Trim.ToUpper & "','" & txtLanguage.Text.Trim.ToUpper & "','" & txtTelNo.Text.Trim.ToUpper & "', 0 ,
'" & txtGracePeriod.Text.Trim.ToUpper & "','" & LoanStartDate & "','" & LoanEndDate & "','" & drpRepaymentFrequency.SelectedValue.Trim.ToUpper & "',
'" & txtNoInstallments.Text.Trim.ToUpper & "','" & txtNoInstallments.Text.Trim.ToUpper & "','" & txtTenure.Text.Trim.ToUpper & "','" & txtCharges.Text.Trim.ToUpper & "','" & txtLoanAmount.Text.Trim.ToUpper & "','" & txtRepaymentAmt.Text.Trim.ToUpper & "',
'" & txtInterestCalc.Text.Trim.ToUpper & "','" & FirstEMI & "','" & txtCollateral.Text.Trim.ToUpper & "','" & RepaymentDate & "',
'" & txtRepaymentPeriod.Text.Trim.ToUpper & "','" & txtSkipInstallments.Text.Trim.ToUpper & "','" & txtRepaymentMode.Text.Trim.ToUpper & "','" & txtSettlementAccountNo.Text.Trim.ToUpper & "','" & txtGuarantorsAccountNo.Text.Trim.ToUpper & "','" & txtSettlementAccountName.Text.Trim.ToUpper & "','" & txtGuarantorsAccountName.Text.Trim.ToUpper & "',0,1,0,'" & FirstEMI & "','" & FirstEMI.AddDays(1) & "',1,0.00,0.00,'" & LoanStartDate & "','" & FirstEMI & "','" & txtGuaranteeAmount.Text.Trim & "')"
                    MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
                    'lblSuccess.Text = "Account Details have been saved successfully"
                    lblCheckdetails.Text = "Loans Account Details have been saved successfully"
                    'ScriptManager.RegisterStartupScript(Me, Me.GetType(), "Pop", "openModal();", True)

                    lblSuccess.Visible = True
                    DisplayDetails("All")
                    txtCustomerNo.ReadOnly = False
                    txtBranchCode.Text = ""
                    txtBranchName.Text = ""
                    txtAccountCode.Text = ""
                    txtProductName.Text = ""
                    txtCustomerNo.Text = ""
                    txtGuaranteeAmount.Text = ""
                    ClearControls()


                Else
                    Dim myaccountno As String = ""
                    myaccountno = Session("AccountNo")
                    Dim trandate As String = Date.Today.ToString("dd-MMM-yy")
                    Dim MyDBAction As New DataManagement
                    Dim MySqlaction1 As String
                    MySqlaction1 = "update tbAccount set AccountName ='" & txtAcountName.Text.ToUpper.Trim & "', Sector ='" & txtSector.Text.ToUpper.Trim & "', SubSector ='" & txtSubSector.Text.ToUpper.Trim & "', AccountType ='" & drpAccountType.Text.ToUpper.Trim & "',  C_Address1 ='" & txtAddress1.Text.ToUpper.Trim & "',C_Address2 ='" & txtAddress2.Text.ToUpper.Trim & "' ,C_Address3 = '" & txtAddress3.Text.ToUpper.Trim & "', C_Email = '" & txtEmail.Text.Trim & "', C_TINNo = '" & txtTillNo.Text.ToUpper.Trim & "', C_Telephone = '" & txtTelNo.Text.ToUpper.Trim & "',C_MobileNo = '" & txtMobileNo.Text.ToUpper.Trim & "',I_FirstName = '" & txtFirstName.Text.ToUpper.Trim & "',I_MiddleName = '" & txtMiddleName.Text.ToUpper.Trim & "',I_LastName = '" & txtLastName.Text.ToUpper.Trim & "',I_NationalID = '" & txtIDNo.Text.ToUpper.Trim & "',C_RelationShipOfficer = '" & txtRelOfficer.Text.ToUpper.Trim & "',I_MaritalStatus = '" & drpMaritalStatus.Text.ToUpper.Trim & "',I_SpouseName = '" & txtSpouseName.Text.ToUpper.Trim & "',I_Dependants = '" & txtDepemdant.Text.ToUpper.Trim & "',I_Occupation = '" & txtOccupation.Text.ToUpper.Trim & "',I_EmployerName = '" & txtEmployer.Text.ToUpper.Trim & "',B_BusinessName = '" & txtBizName.Text.ToUpper.Trim & "',B_RegDate = '" & txtBizRegDate.Text.ToUpper.Trim & "',B_RegName = '" & txtRegNo.Text.ToUpper.Trim & "',B_NatureofBusiness = '" & txtBizNature.Text.ToUpper.Trim & "',B_DirectorName = '" & txtDirName.Text.ToUpper.Trim & "',G_GroupName = '" & txtgrpName.Text.ToUpper.Trim & "',G_NoofMembers = '" & txtgrpMemberNo.Text.ToUpper.Trim & "',G_NoofMale = '" & txtMaleNo.Text.ToUpper.Trim & "',G_NoofFemale = '" & txtFemaleNo.Text.ToUpper.Trim & "',C_Region = '" & drpRegion.SelectedValue.ToUpper.Trim & "' ,C_District = '" & drpDistrict.SelectedValue.ToUpper.Trim & "',C_Area = '" & drpArea.SelectedValue.ToUpper.Trim & "',I_ResidentStatus ='" & txtResidential.Text.ToUpper.Trim & "',I_Sex = '" & txtSex.Text.ToUpper.Trim & "',I_PassportNO = '" & txtPassport.Text.ToUpper.Trim & "',I_Nationality = '" & txtNationality.Text.ToUpper.Trim & "',I_DoB = '" & txtDoB.Text.ToUpper.Trim & "',I_Language = '" & txtLanguage.Text.ToUpper.Trim & "',B_TelNo = '" & txtTelNo.Text.ToUpper.Trim & "',GracePeriod = '" & txtGracePeriod.Text.Trim.ToUpper & "', LoanStartDate = '" & txtLoanStartDate.Text & "', LoanEndDate = '" & txtLoanEndDate.Text & "', RepaymentFrequency = '" & drpRepaymentFrequency.Text.Trim.ToUpper & "',NoInstallments = '" & txtNoInstallments.Text.Trim.ToUpper & "',Tenure = '" & txtTenure.Text.Trim.ToUpper & "',Charges = '" & txtCharges.Text.Trim.ToUpper & "',LoanAmount  = '" & txtLoanAmount.Text.Trim.ToUpper & "', InterestCalc = '" & txtInterestCalc.Text.Trim.ToUpper & "',FirstEMI  = '" & txtFirstEMI.Text & "',Collateral  = '" & txtCollateral.Text.Trim.ToUpper & "',RepaymentDate = '" & txtRepaymentDate.Text & "', RepaymentPeriod = '" & txtRepaymentPeriod.Text.Trim.ToUpper & "', SkipInstallments = '" & txtSkipInstallments.Text.Trim.ToUpper & "',RepaymentMode = '" & txtRepaymentMode.Text.Trim.ToUpper & "',SettlementsAccountNo = '" & txtSettlementAccountNo.Text.Trim.ToUpper & "', GuarantorsAccountNo = '" & txtGuarantorsAccountNo.Text.Trim.ToUpper & "' , SettlementsAccountName = '" & txtSettlementAccountName.Text.Trim.ToUpper & "' , GuarantorsAccountName = '" & txtGuarantorsAccountName.Text.Trim.ToUpper & "' where AccountNo ='" & myaccountno & "'"
                    MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
                    DisplayDetails("All")
                    lblCheckdetails.Text = "Loan account details have been updated successfully"
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
                    txtSector.Text = ""
                    txtSubSector.Text = ""
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
                    drpRegion.Text = ""
                    drpDistrict.Text = ""
                    drpArea.Text = ""
                    txtSex.Text = ""
                    txtPassport.Text = ""
                    txtNationality.Text = ""
                    txtDoB.Text = ""
                    txtLanguage.Text = ""
                    txtResidential.Text = ""
                    txtGracePeriod.Text = ""
                    txtInterestRate.Text = ""
                    txtLoanStartDate.Text = ""
                    txtLoanEndDate.Text = ""
                    drpRepaymentFrequency.SelectedValue = -1
                    txtNoInstallments.Text = ""
                    txtTenure.Text = ""
                    txtCharges.Text = ""
                    txtLoanAmount.Text = ""
                    txtInterestCalc.Text = ""
                    txtFirstEMI.Text = ""
                    txtCollateral.Text = ""
                    txtRepaymentMode.Text = ""
                    txtRepaymentDate.Text = ""
                    txtRepaymentPeriod.Text = ""
                    txtSkipInstallments.Text = ""
                    txtSettlementAccountNo.Text = ""
                    txtSettlementAccountName.Text = ""
                    txtGuarantorsAccountNo.Text = ""
                    txtGuarantorsAccountName.Text = ""
                    drpSector.SelectedValue = -1
                    drpSubSector.SelectedValue = -1

                    txtBranchCode.Text = ""
                    txtAccountCode.Text = ""
                    txtCustomerNo.Text = ""
                    txtBranchName.Text = ""
                    txtProductName.Text = ""

                    'ClearControls()

                End If

            End If

        Catch ex As Exception
        End Try
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
    Protected Sub btnCalculate_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnCalculate.Click
        'ClearControls()
        Dim intAmount = (txtLoanAmount.Text * txtInterestRate.Text) / 100
        Dim repaymentAmount As Double = 0.00
        'txtRepaymentAmt.Text = Math.Round((intAmount + txtLoanAmount.Text) / 12, 2)
        repaymentAmount = Math.Round((intAmount + txtLoanAmount.Text) / txtTenure.Text, 2)
        'txtRepaymentAmt.Text = Math.Round((intAmount + txtLoanAmount.Text) / txtTenure.Text, 2)
        Select Case drpRepaymentFrequency.SelectedValue
            Case "M"
                txtRepaymentAmt.Text = repaymentAmount * 1
            Case "Q"
                txtRepaymentAmt.Text = repaymentAmount * 4
            Case "S"
                txtRepaymentAmt.Text = repaymentAmount * 6
            Case "Y"
                txtRepaymentAmt.Text = repaymentAmount * 12
        End Select
    End Sub
    Protected Sub btnClears_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnClear.Click

        'ScriptManager.RegisterStartupScript(Me, Me.GetType(), "Pop", "openModal();", True)
        'ScriptManager.RegisterClientScriptBlock(Me, Me.GetType(), "none", "<script>$('#mymodal').modal('show');</script>", False)

    End Sub
    Protected Sub txtAdress3_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs)

    End Sub




    Protected Sub drpAccountType_OnSelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        OpenSelectedTab()
    End Sub
    Protected Sub btnCheckBranch_Click(sender As Object, e As EventArgs) Handles btnCheckBranch.Click
        GetBranchDetails()
    End Sub
    Public Sub OpenSelectedTab()
        Select Case drpAccountType.Text.Trim
            Case "INDIVIDUAL ACCOUNT"
                TabContainer2.ActiveTabIndex = 0
            Case "BUSINESS ACCOUNT"
                TabContainer2.ActiveTabIndex = 1

            Case "GROUP ACCOUNT"
                TabContainer2.ActiveTabIndex = 2
            Case Else
                TabContainer2.ActiveTabIndex = 0

        End Select

    End Sub
    Public Sub GetBranchDetails()
        Try
            If txtBranchCode.Text = "" Then
                txtBranchName.Text = ""
                lblCheckdetails.Text = "Enter branch code"
                'ScriptManager.RegisterStartupScript(Me, Me.GetType(), "Pop", "openModal();", True)
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
                'ScriptManager.RegisterStartupScript(Me, Me.GetType(), "Pop", "openModal();", True)
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

                'If accountCount = 0 Then
                '    newAccountSuffix = "01"
                'ElseIf accountCount = 1 Then
                '    newAccountSuffix = "02"
                'ElseIf accountCount = 2 Then
                '    newAccountSuffix = "03"
                'ElseIf accountCount = 3 Then
                '    newAccountSuffix = "04"
                'ElseIf accountCount = 4 Then
                '    newAccountSuffix = "05"
                'End If


                newAccountSuffix = Convert.ToInt32(accountCount) + 1

                Dim newAccountSuffixPad As String = newAccountSuffix.PadLeft(2, "0"c)

                txtAccountNo.Text = BranchCode + "-" + CustomerNo + "-" + AccountPrefix + "-" + newAccountSuffixPad
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
            Me.lblSuccess.Text = "Account Details Have Been Deleted Successfully, Awaiting Approval"
            DisplayDetails("All")
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
                ''ScriptManager.RegisterStartupScript(Me, Me.GetType(), "Pop", "openModal();", True)
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
                GetRegion()
                GetArea()
                GetDistrict()
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
                drpRegion.SelectedValue = MyReader4("C_Region").ToString
                drpDistrict.SelectedValue = MyReader4("C_District").ToString
                drpArea.SelectedValue = MyReader4("C_Area").ToString
                txtSex.Text = MyReader4("I_Sex").ToString
                txtPassport.Text = MyReader4("I_PassportNO").ToString
                txtNationality.Text = MyReader4("I_Nationality").ToString
                txtDoB.Text = MyReader4("I_DoB").ToString
                txtLanguage.Text = MyReader4("I_Language").ToString
                txtResidential.Text = MyReader4("I_ResidentStatus").ToString
                txtRegNo.Text = MyReader4("B_RegName").ToString

                'txtGracePeriod.Text = MyReader4("GracePeriod").ToString
                'txtLoanStartDate.Text = MyReader4("LoanStartDate").ToString
                'txtLoanEndDate.Text = MyReader4("LoanEndDate").ToString
                'drpRepaymentFrequency.Text = MyReader4("RepaymentFrequency").ToString
                'txtNoInstallments.Text = MyReader4("NoInstallments").ToString
                'txtTenure.Text = MyReader4("Tenure").ToString
                'txtCharges.Text = MyReader4("Charges").ToString
                'txtLoanAmount.Text = MyReader4("LoanAmount").ToString
                'txtInterestCalc.Text = MyReader4("InterestCalc").ToString
                'txtFirstEMI.Text = MyReader4("FirstEMI").ToString
                'txtCollateral.Text = MyReader4("Collateral").ToString
                'txtRepaymentMode.Text = MyReader4("RepaymentMode").ToString
                'txtRepaymentDate.Text = MyReader4("RepaymentDate").ToString
                'txtRepaymentPeriod.Text = MyReader4("RepaymentPeriod").ToString
                'txtSkipInstallments.Text = MyReader4("SkipInstallments").ToString
                'txtDRAccInterest.Text = MyReader4("DRAccInterest").ToString
                'txtCRAccInterest.Text = MyReader4("CRAccInterest").ToString


                'drpLoanType.Text = MyReader("G_NoofFemale").ToString
                'drpRepaymentFrequency.Text = MyReader("G_NoofFemale").ToString
                'drpRepaymentMode.Text = MyReader("G_NoofFemale").ToString
                'drpSector.Text = MyReader("G_NoofFemale").ToString
                'drpSubSector.Text = MyReader("G_NoofFemale").ToString

                OpenSelectedTab()




                If txtBranchName.Text = "" And txtProductName.Text = "" Then
                    lblCheckdetails.Text = "Missing branch or product details"
                    'ScriptManager.RegisterStartupScript(Me, Me.GetType(), "Pop", "openModal();", True)
                    ClearControls()
                Else
                    GenerateAccountNumber(txtBranchCode.Text, txtCustomerNo.Text, txtProductCode.Text)
                End If
            Else
                lblCheckdetails.Text = "This customer number does not exist"
                'ScriptManager.RegisterStartupScript(Me, Me.GetType(), "Pop", "openModal();", True)
                ClearControls()
                txtAccountNo.Text = ""
                Exit Sub
            End If

            MyReader4.Close()
        Catch ex As Exception
        End Try
    End Sub


    Protected Sub ClearSettlementDetails(LoanAccountTypes)
        If LoanAccountTypes = 2 Then
            txtGuarantorsAccountNo.Text = ""
            txtGuarantorsAccountName.Text = ""
        ElseIf LoanAccountTypes = 1 Then
            txtSettlementAccountNo.Text = ""
            txtSettlementAccountName.Text = ""
        End If


    End Sub
    Protected Sub ConfirmAccount(LoanAccountTypes, AccountNo)



        Try
            If txtCustomerNo.Text = "" Then
                txtAccountNo.Text = ""
                lblCheckdetails.Text = "Enter Account number"
                'ScriptManager.RegisterStartupScript(Me, Me.GetType(), "Pop", "openModal();", True)
                txtProductCode.Focus()
                ClearControls()
                Exit Sub
            Else
                lblCheckdetails.Text = ""
            End If
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select BookBalance,AvailBal,CustomerNo,Approved,AccountStatus,AccountNo,AccountName,ProductDefCode from tbAccount where AccountNo = '" & AccountNo & "'  " ' ='" & Me.HiddenField1.Value & "' "
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
            If MyReader4.HasRows = True Then
                MyReader4.Read()

                Account.CustomerNo = MyReader4("CustomerNo").ToString
                Account.AccountName = MyReader4("AccountName").ToString
                Account.AccountStatus = MyReader4("AccountStatus").ToString
                Account.ProductDefCode = MyReader4("ProductDefCode").ToString
                Account.Approved = MyReader4("Approved").ToString
                AvailBalance = MyReader4("BookBalance").ToString
                BkBalance = MyReader4("AvailBal").ToString

                If Account.Approved <> 1 Then
                    lblCheckdetails.Text = "This account needs to be approved first"
                    ClearSettlementDetails(LoanAccountTypes)
                    Exit Sub
                ElseIf Account.AccountStatus <> "Open" Then
                    lblCheckdetails.Text = "This account is not open"
                    ClearSettlementDetails(LoanAccountTypes)
                    Exit Sub
                ElseIf LoanAccountTypes = 1 Then
                    txtSettlementAccountName.Text = Account.AccountName
                    If Account.CustomerNo <> Convert.ToInt32(txtCustomerNo.Text) Then
                        lblCheckdetails.Text = "The settlement's account number does not belong to the same customer"
                        ClearSettlementDetails(LoanAccountTypes)
                    End If
                    Exit Sub
                ElseIf LoanAccountTypes = 2 Then
                    'If Account.CustomerNo = Convert.ToInt32(txtCustomerNo.Text) Then
                    '    lblCheckdetails.Text = "The guarantor's account number cannot be from the same customer"
                    '    ClearSettlementDetails(LoanAccountTypes)
                    '    Exit Sub
                    'End If
                    If AvailBalance < 0 Then
                        lblCheckdetails.Text = "This account have insufficient funds"
                        Exit Sub

                        'ElseIf AvailBalance < txtLoanAmount.Text Then
                        '    lblCheckdetails.Text = "This account have insufficient funds to guarantee this loan of D'" & txtLoanAmount.Text.Trim & "'"
                        '    Exit Sub
                    Else
                        txtGuarantorsAccountName.Text = Account.AccountName
                        txtGuaranteeAmount.Text = txtLoanAmount.Text
                    End If
                ElseIf Account.ProductDefCode <> "SA" Then
                    lblCheckdetails.Text = "The account number cannot be a loan account"
                    ClearSettlementDetails(LoanAccountTypes)
                End If
                Exit Sub
            Else
                lblCheckdetails.Text = "This customer number does not exist"
                ClearSettlementDetails(LoanAccountTypes)
                Exit Sub
            End If
            MyReader4.Close()
        Catch ex As Exception
        End Try
    End Sub


    Protected Sub GetProductData()
        Try

            Dim ProductDefCode As String
            If txtAccountCode.Text = "" Then
                txtProductName.Text = ""
                'ScriptManager.RegisterStartupScript(Me, Me.GetType(), "Pop", "openModal();", True)
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
                ProductDefCode = MyReader5("ProductDefCode").ToString

                If ProductDefCode = "LA" Then
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
                    txtInterestRate.Text = MyReader5("IntRate").ToString
                    IntFreq = MyReader5("IntFreq").ToString
                    IntAccrualFreq = MyReader5("IntAccrualFreq").ToString
                    MinInterestBal = MyReader5("MinInterestBal").ToString
                    MinAccountBal = MyReader5("MinAccountBal").ToString
                    DebitsNoPerMonth = MyReader5("DebitsNoPerMonth").ToString
                    Amount = MyReader5("Amount").ToString
                Else
                    'ScriptManager.RegisterStartupScript(Me, Me.GetType(), "Pop", "openModal();", True)
                    lblCheckdetails.Text = "The product you've entered is not a Loan Product"
                    txtProductName.Text = ""
                End If

            Else
                'ScriptManager.RegisterStartupScript(Me, Me.GetType(), "Pop", "openModal();", True)
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

    Protected Sub btnBlockAmount_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnBlockAmount.Click
        Dim blockAmount As Decimal = 0.00
        GetAccountDetailMap(Me.txtGuarantorsAccountNo.Text.Trim())
        P_AvailBal = P_AccountDetailsHashMap("AvailBal")
        P_BookBalance = P_AccountDetailsHashMap("BookBalance")
        blockAmount = P_AccountDetailsHashMap("BlockAmount")
        P_AccountDetailsHashMap.Clear()
        UpdateCustomerBalances(Me.txtGuarantorsAccountNo.Text.Trim(), txtGuaranteeAmount.Text.Trim(), P_AvailBal, blockAmount)
        InsertBlockedAmount(P_AvailBal, P_BookBalance)
        lblCheckdetails.Text = "Amount block successful"
        'ConfirmAccount(LoanAccountTypes.SettlementAccount, txtSettlementAccountNo.Text)
    End Sub
    'Private Function GetAccountDetailMap(accountNo) As Hashtable
    'Private Function GetAccountDetailMap(accountNo As String) As Hashtable

    '    Try
    '        Dim MyDatObject As New DataManagement
    '        Dim mySearch4 As String = "select AccountNo,CustomerNo,AccountName,AssetGL_Control,LiabilityGL_Control,BranchCode,AvailBal,BookBalance,ProductDefCode,BlockAmount from [dbo].[tbAccount] where AccountNo = '" & accountNo.Trim & "'"
    '        Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
    '        If MyReader4.HasRows = True Then
    '            MyReader4.Read()
    '            P_AccountDetailsHashMap.Add("CustomerNo", MyReader4("CustomerNo").ToString)
    '            P_AccountDetailsHashMap.Add("AccountName", MyReader4("AccountName").ToString)
    '            P_AccountDetailsHashMap.Add("AssetGLControl", MyReader4("AssetGL_Control").ToString)
    '            P_AccountDetailsHashMap.Add("LiabilityGLControl", MyReader4("LiabilityGL_Control").ToString)
    '            P_AccountDetailsHashMap.Add("BranchCode", MyReader4("BranchCode").ToString)
    '            P_AccountDetailsHashMap.Add("AvailBal", MyReader4("AvailBal").ToString)
    '            P_AccountDetailsHashMap.Add("BookBalance", MyReader4("BookBalance").ToString)
    '            P_AccountDetailsHashMap.Add("BlockAmount", MyReader4("BlockAmount").ToString)
    '            P_AccountDetailsHashMap.Add("DefinationCode", MyReader4("ProductDefCode").ToString())

    '        End If
    '        MyReader4.Close()
    '    Catch ex As Exception
    '        'Throw New Exception("Error Loading Account Details", ex)
    '        'lblSuccessMsg.Text = ex.ToString
    '        Exit Function
    '    End Try
    '    'Next
    '    Return P_AccountDetailsHashMap
    'End Function

    Private Function GetAccountDetailMap(accountNo As String) As Hashtable
        Try
            P_AccountDetailsHashMap.Clear()

            Dim MyDatObject As New DataManagement
            Dim sql As String =
            "SELECT AccountNo, CustomerNo, AccountName, AssetGL_Control, LiabilityGL_Control, 
                    BranchCode, AvailBal, BookBalance, ProductDefCode, BlockAmount 
             FROM dbo.tbAccount 
             WHERE AccountNo = '" & accountNo.Trim & "'"

            Using MyReader As SqlDataReader =
            CType(MyDatObject.DBAction(sql, DataManagement.DBActionType.SqlDataReader), SqlDataReader)

                If MyReader.Read() Then

                    P_AccountDetailsHashMap("CustomerNo") = MyReader("CustomerNo").ToString()
                    P_AccountDetailsHashMap("AccountName") = MyReader("AccountName").ToString()
                    P_AccountDetailsHashMap("AssetGLControl") = MyReader("AssetGL_Control").ToString()
                    P_AccountDetailsHashMap("LiabilityGLControl") = MyReader("LiabilityGL_Control").ToString()
                    P_AccountDetailsHashMap("BranchCode") = MyReader("BranchCode").ToString()

                    ' 🔐 NUMERIC VALUES — SAFE
                    P_AccountDetailsHashMap("AvailBal") =
                    If(IsDBNull(MyReader("AvailBal")), 0D, Convert.ToDecimal(MyReader("AvailBal")))

                    P_AccountDetailsHashMap("BookBalance") =
                    If(IsDBNull(MyReader("BookBalance")), 0D, Convert.ToDecimal(MyReader("BookBalance")))

                    P_AccountDetailsHashMap("BlockAmount") =
                    If(IsDBNull(MyReader("BlockAmount")), 0D, Convert.ToDecimal(MyReader("BlockAmount")))

                    P_AccountDetailsHashMap("DefinationCode") =
                    MyReader("ProductDefCode").ToString()

                End If
            End Using

        Catch ex As Exception
            Throw New Exception("Error loading account details", ex)
        End Try

        Return P_AccountDetailsHashMap
    End Function

    Protected Sub InsertBlockedAmount(P_AvailBal, P_BookBalance)
        'If BookBal.Text < txtBlockAmount.Text Then
        Try
            Dim mySqlAction As String = ""
            Dim MyDBAction As New DataManagement
            mySqlAction = "Insert into MB_BlockAmount (AccountNo,AccountName,AvailableBal,BookBal,BlockAmount,ReasonToBlock,BlockDate)values('" & txtGuarantorsAccountNo.Text.Trim & "','" & txtGuarantorsAccountName.Text.ToUpper.Trim & "','" & P_AvailBal & "','" & P_BookBalance & "','" & txtGuaranteeAmount.Text.ToUpper.Trim & "','LOAN GUARANTEE','" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "' )" ','" & txtBranchName.Text.ToUpper & "','" & txtAdress2.Text.ToUpper & "','" & txtmobile.Text & "') "
            MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
        Catch ex As Exception
            'Label4.Text = "Error Inserting"
        End Try
        lblCheckdetails.Text = "Amount block have been successfully"
        'Else
        '    lblSuccessMsg.Text = "Amount block is more than book Balance"
        'End If
    End Sub
    Protected Sub UpdateCustomerBalances(accountNo, amount, P_AvailBal, blockedAmount)
        Try
            'If txtAccountNo.Text = "" Then
            '    txtAccountNo.Focus()
            '    lblSuccessMsg.Text = " Please enter Account Number"
            '    Exit Sub
            'End If
            'If txtAccountNo.Text.ToString().Length < 17 Or txtAccountNo.Text.ToString().Length > 17 Then
            '    txtAccountNo.Focus()
            '    lblSuccessMsg.Text = " This Is Not An Account No"
            '    Exit Sub
            'End If
            'If Not txtAccountNo.Text.ToString().Substring(11, 3) = "201" Then
            '    txtAccountNo.Text = ""
            '    txtAccountNo.Focus()
            '    lblSuccessMsg.Text = " This is not a savings Account"
            '    Exit Sub
            'End If

            'If Double.Parse(txtBlockAmount.Text) < 0 Then
            '    lblSuccessMsg.Text = "Block Amount Should Not Be Less Than 0"
            '    txtBlockAmount.Focus()
            '    Exit Sub
            'ElseIf Double.Parse(txtBlockAmount.Text) > Double.Parse(AailBalance.Text) Then
            '    lblSuccessMsg.Text = "Block Amount Should Not Be More Than The Available Balance"
            '    txtBlockAmount.Focus()
            '    Exit Sub
            'Else
            Dim newAccBal As String = ""
            Dim NewBlockAmount As String = ""
            newAccBal = Double.Parse(P_AvailBal) - Val(amount)
            'NewBlockAmount = txtGuaranteeAmount.Text + Val(amount)
            NewBlockAmount = txtGuaranteeAmount.Text + blockedAmount
            Dim MySqlaction1 As String
            Dim mySqlAction As String = ""
            Dim MyCurrencyName As String = ""
            Dim MyDBAction As New DataManagement
            MySqlaction1 = "update tbAccount set AvailBal ='" & newAccBal & "', BlockAmount='" & NewBlockAmount & "', LastActivityDate = '" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "',LastCRactvtyDate ='" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "',LastCRDate = '" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "' where AccountNo ='" & accountNo & "'"
            MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
            ' lblSuccessMsg.Text = "Amount block successfully"
            'End If
            'Dim displayBal As String = ""
            'displayBal = Format(newAccBal, "###,###")
            'txtNewBalance.Text = newAccBal
        Catch ex As Exception
            'Label4.Text = "Error Inserting"
        End Try
        Exit Sub
    End Sub
    Protected Sub btnChckSettlementAccount_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnChckSettlementAccount.Click
        ConfirmAccount(LoanAccountTypes.SettlementAccount, txtSettlementAccountNo.Text)
    End Sub
    Protected Sub btnChckGuarantorsAccount_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnChckGuarantorsAccount.Click
        ConfirmAccount(LoanAccountTypes.GaurantorsAccount, txtGuarantorsAccountNo.Text)
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
        drpSector.Text = ""
        drpSubSector.Text = ""
        drpRepaymentFrequency.Text = ""
        txtAccountNo.Text = ""
        HiddenField1.Value = ""
        txtAcountName.Text = ""
        txtCurrency.Text = ""
        txtSector.Text = ""
        txtSubSector.Text = ""
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
        drpRegion.Text = ""
        drpDistrict.Text = ""
        drpArea.Text = ""
        txtSex.Text = ""
        txtDoB.Text = ""


        txtGracePeriod.Text = ""
        txtInterestRate.Text = ""
        txtLoanStartDate.Text = ""
        txtLoanEndDate.Text = ""
        drpRepaymentFrequency.SelectedValue = -1
        txtNoInstallments.Text = ""
        txtTenure.Text = ""
        txtCharges.Text = ""
        txtLoanAmount.Text = ""
        txtInterestCalc.Text = ""
        txtFirstEMI.Text = ""
        txtCollateral.Text = ""
        txtRepaymentMode.Text = ""
        txtRepaymentDate.Text = ""
        txtRepaymentPeriod.Text = ""
        txtSkipInstallments.Text = ""
        txtSettlementAccountNo.Text = ""
        txtSettlementAccountName.Text = ""
        txtGuarantorsAccountNo.Text = ""
        txtGuarantorsAccountName.Text = ""
        drpSector.SelectedValue = -1
        drpSubSector.SelectedValue = -1

        'drpLoanType.Text = ""
        'drpRepaymentFrequency.Text = ""
        'drpRepaymentMode.Text = ""





    End Sub


    Protected Sub btnDOBcalendar_Click(sender As Object, e As ImageClickEventArgs) Handles btnDOBcalendar.Click
        DOBDatePicker.Visible = True
    End Sub

    Protected Sub btnBizRegDatecalendar_Click(sender As Object, e As ImageClickEventArgs) Handles btnBizRegDatecalendar.Click
        BizRegDatePicker.Visible = True
    End Sub

    Protected Sub btnEMICalendar_Click(sender As Object, e As ImageClickEventArgs) Handles btnEMICalendar.Click
        FirstEMIDatePicker.Visible = True
    End Sub

    Protected Sub btnStartDateCalendar_Click(sender As Object, e As ImageClickEventArgs) Handles btnStartDateCalendar.Click
        StartDatePicker.Visible = True
    End Sub
    Protected Sub btnEndDateCalendar_Click(sender As Object, e As ImageClickEventArgs) Handles btnEndDatecalendar.Click
        EndDatePicker.Visible = True
    End Sub



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

    Protected Sub StartDatePicker_SelectionChanged(sender As Object, e As EventArgs) Handles StartDatePicker.SelectionChanged
        Dim sysUIFormat As String = CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern
        StartDatePicker.Visible = False
        txtLoanStartDate.Text = BizRegDatePicker.SelectedDate.ToString("dd/MM/yyyy")
    End Sub

    Protected Sub EndDatePicker_SelectionChanged(sender As Object, e As EventArgs) Handles EndDatePicker.SelectionChanged
        Dim sysUIFormat As String = CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern
        EndDatePicker.Visible = False
        txtLoanEndDate.Text = BizRegDatePicker.SelectedDate.ToString("dd/MM/yyyy")
    End Sub

    Protected Sub FirstEMIDatePicker_SelectionChanged(sender As Object, e As EventArgs) Handles FirstEMIDatePicker.SelectionChanged
        Dim sysUIFormat As String = CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern
        FirstEMIDatePicker.Visible = False
        txtFirstEMI.Text = BizRegDatePicker.SelectedDate.ToString("dd/MM/yyyy")
    End Sub

End Class