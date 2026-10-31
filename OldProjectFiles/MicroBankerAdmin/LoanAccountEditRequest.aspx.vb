Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Imports System.Data.SqlClient

Public Class LoanAccountEditRequest
    Inherits System.Web.UI.Page
    Public AccountCode As String = "", AccountPrefix As String = "", AccountName As String = "", DormancyDays As String = "", AssetGL As String = "", LiabilityGL As String = "", IncomeGL As String = "", ExpenseGL As String = "", ChargeGL As String = "", FeeGL As String = "", IntRate As String = "", IntFreq As String = "", IntAccrualFreq As String = "", MinInterestBal As String = "", MinAccountBal As String = "", DebitsNoPerMonth As String = "", Amount As String = ""
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            DisplayDetails("All")
            MultiView1.SetActiveView(View1)
        End If
    End Sub
    Protected Sub GridView1_RowCommand(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewCommandEventArgs) Handles GridView1.RowCommand
        Try
            Dim MyID As String = e.CommandArgument
            HiddenField1.Value = MyID
            Select Case e.CommandName
                Case "select"
                    txtCustomerNo.Enabled = False
                    txtBranchCode.Enabled = False
                    txtAccountCode.Enabled = False
                    txtAccountNo.Enabled = False
                    txtBranchName.Enabled = False
                    txtCurrency.Enabled = False
                    txtProductCode.Enabled = False
                    'btnCheckBranch.Visible = False
                    'btnSearchCode.Visible = False
                    'cmasearch.Visible = False
                    ShowAgentDetails("select * from tbAccount where AccountNo='" & HiddenField1.Value.ToString & "'  ", "ViewDetails")
                    'ShowAgentDetails("select CustomerNo,AccountType,BranchName,AccountNo,AcountName,Currency,Sector,SubSector,DRGL,CRGL,IntRate,DebitNoStatus,BranchCode,AccountPrefix,C_Address1 from tbAccount where AccountNo='" & HiddenField1.Value.ToString & "'  ", "ViewDetails")
                    MultiView1.SetActiveView(View2)
            End Select
        Catch ex As Exception
            Dim ErrMsg As String = ex.Message
        End Try
    End Sub
    Private Sub ShowAgentDetails(ByVal mySearch As String, ByVal ShowType As String)
        Dim MySmsObject As New DataManagement
        Dim MyTransProcessed As Boolean = False
        Dim MyReader As SqlDataReader = TryCast(MySmsObject.DBAction(mySearch, DataManagement.DBActionType.SqlDataReader), SqlDataReader)
        Try
            Select Case ShowType
                Case "ViewDetails"
                    If MyReader.HasRows Then
                        MyReader.Read()
                        Dim acctimage As String = ""
                        Try
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
                            txtBizTel.Text = MyReader("B_TelNo").ToString

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

                            txtGuarantorsAccountName.Text = MyReader("GuarantorsAccountName").ToString
                            txtSettlementAccountName.Text = MyReader("SettlementsAccountName").ToString
                            txtGuarantorsAccountNo.Text = MyReader("GuarantorsAccountNo").ToString
                            txtSettlementAccountNo.Text = MyReader("SettlementsAccountNo").ToString




                            drpRepaymentFrequency.SelectedValue = MyReader("RepaymentFrequency").ToString
                            drpSector.SelectedValue = MyReader("Sector").ToString
                            drpSubSector.SelectedValue = MyReader("SubSector").ToString
                        Catch ex As Exception
                        End Try
                    End If
            End Select
        Catch ex As Exception
        Finally
            If Not IsNothing(MyReader) Then
                MyReader.Close()
            End If
        End Try
    End Sub
    Private Sub DisplayDetails(ByVal ShowType As String)
        Try
            Dim mySqlAction As String = ""
            Dim SqlAction As String = ""
            Dim searchStr As String = ""
            Dim productType As String = "LA"
            Dim MyDBAction As New DataManagement
            Dim MyDatable As New Data.DataTable
            Select Case ShowType
                Case "All"
                    'lblGrpResponse.Text = ""
                    SqlAction = "select CustomerNo,AccountType,BranchName,AccountNo,AccountName,Currency,Sector,SubSector,IntRate,DebitNoStatus,BranchCode,AccountPrefix,C_Address1,LoanAmount,LoanStartDate,LoanEndDate,RepaymentPeriod from tbAccount where Approved = 1 and ProductDefCode = '" & productType & "'"
                    MyDatable = TryCast(MyDBAction.DBAction(SqlAction, DataManagement.DBActionType.DataTable), Data.DataTable)
                    Session("searchtable") = MyDatable
                    GridView1.DataSource = MyDatable
                    GridView1.DataBind()
                Case "Filter"
                    searchStr = txtSearch.Text
                    If RdoSearch.Items(0).Selected = True Then
                        searchStr = "%" & searchStr & "%"
                        SqlAction = "select CustomerNo,AccountType,BranchName,AccountNo,AccountName,Currency,Sector,SubSector,IntRate,DebitNoStatus,BranchCode,AccountPrefix,C_Address1,LoanAmount,LoanStartDate,LoanEndDate,RepaymentPeriod from tbAccount  where AccountNo LIKE  '" & searchStr & "' and Approved = 1 and ProductDefCode = '" & productType & "' order by id desc"
                        MyDatable = TryCast(MyDBAction.DBAction(SqlAction, DataManagement.DBActionType.DataTable), Data.DataTable)
                        Session("searchtable") = MyDatable
                        GridView1.DataSource = MyDatable
                        GridView1.DataBind()
                    Else
                        searchStr = "%" & searchStr & "%"
                        SqlAction = "select CustomerNo,AccountType,BranchName,AccountNo,AccountName,Currency,Sector,SubSector,IntRate,DebitNoStatus,BranchCode,AccountPrefix,C_Address1,LoanAmount,LoanStartDate,LoanEndDate,RepaymentPeriod from tbAccount  where AcountName LIKE  '" & searchStr & "' and Approved = 1 and ProductDefCode = '" & productType & "' order by id desc"
                        MyDatable = TryCast(MyDBAction.DBAction(SqlAction, DataManagement.DBActionType.DataTable), Data.DataTable)
                        Session("searchtable") = MyDatable
                        GridView1.DataSource = MyDatable
                        GridView1.DataBind()
                    End If
            End Select
        Catch ex As Exception
        End Try
    End Sub
    Protected Sub CmdApprovalList_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        DisplayDetails("All")
        MultiView1.SetActiveView(View1)
    End Sub
    Protected Sub MultiView1_ActiveViewChanged(ByVal sender As Object, ByVal e As System.EventArgs)
    End Sub
    Protected Sub Wizard1_FinishButtonClick(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.WizardNavigationEventArgs)
    End Sub
    Protected Sub Wizard2_FinishButtonClick(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.WizardNavigationEventArgs)
    End Sub
    Protected Sub RdoSearch_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
    End Sub
    Protected Sub txtSearch_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs)
    End Sub
    Protected Sub btnSearch_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        DisplayDetails("Filter")
    End Sub
    Protected Sub Button10_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        DisplayDetails("All")
    End Sub
    Protected Sub GridView1_PageIndexChanging(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewPageEventArgs) Handles GridView1.PageIndexChanging
        GridView1.DataSource = Session("searchtable")
        GridView1.PageIndex = e.NewPageIndex
        GridView1.DataBind()
    End Sub
    'Protected Sub Button4_DataBinding(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnEditGroup.DataBinding
    'End Sub

    'Protected Sub Button4_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnEditGroup.Disposed
    'End Sub
    Protected Sub GridView29_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
    End Sub
    Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Dim myaccountno As String = ""
        myaccountno = Session("AccountNo")
        Dim trandate As String = Date.Today.ToString("dd-MMM-yy")
        Dim MyDBAction As New DataManagement
        Dim MySqlaction1 As String
        If radioApprove.Checked = True Then
            MySqlaction1 = "update tbAccount set  AccEditReason = '" & txtEditReason.Text.ToUpper.Trim & "', Approved = 0 where AccountNo ='" & myaccountno & "'"
            MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
            DisplayDetails("All")
            lblResponse.Text = "Account Edit Request have been sent successfully"
        End If
        txtCustomerNo.Enabled = False
        txtBranchCode.Enabled = False
        txtAccountCode.Enabled = False
        txtAccountNo.Enabled = False
        txtBranchName.Enabled = False
        txtCurrency.Enabled = False
        txtProductCode.Enabled = False
        'btnCheckBranch.Visible = False
        'btnSearchCode.Visible = False
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
        txtGrpMemberNo.Text = ""
        txtMaleNo.Text = ""
        txtBranchCode.Text = ""
        txtFemaleNo.Text = ""
        txtBranchName.Text = ""
        txtProductCode.Text = ""
        txtLastName.Text = ""
        txtIDNo.Text = ""
        txtBizRegDate.Text = ""
        txtRegNo.Text = ""
        txtBizNature.Text = ""
        txtDirName.Text = ""
        txtGrpName.Text = ""
        txtEditReason.Text = ""
        radioApprove.Checked = False
        txtCustomerNo.Enabled = False
        txtBranchCode.Enabled = False
        txtAccountCode.Enabled = False
        txtAccountNo.Enabled = False
        txtBranchName.Enabled = False
        txtCurrency.Enabled = False
        txtProductCode.Enabled = False
        'btnCheckBranch.Visible = False
        'btnSearchCode.Visible = False
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
        txtProductCode.Text = ""
        txtLastName.Text = ""
        txtProductCode.Text = ""
        txtProductName.Text = ""
        txtResidential.Text = ""
        txtPassport.Text = ""
        txtNationality.Text = ""
        txtLanguage.Text = ""
        txtBizTel.Text = ""
        drpSector.Text = ""
        drpSubSector.Text = ""
        txtIDNo.Text = ""
        txtBizRegDate.Text = ""
        txtRegNo.Text = ""
        txtBizNature.Text = ""
        txtDirName.Text = ""
        txtgrpName.Text = ""
        radioApprove.Checked = False
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
        txtRegion.Text = ""
        txtDistrict.Text = ""
        txtArea.Text = ""
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
    End Sub
    'Protected Sub btnEditBiz_Click(sender As Object, e As EventArgs) Handles btnEditBiz.Click
    '    Dim mycustomerno As String = ""
    '    mycustomerno = Session("CUSTOMERNO")
    '    Dim trandate As String = Date.Today.ToString("dd-MMM-yy")
    '    Dim MyDBAction As New DataManagement
    '    Dim mySqlAction As String = ""
    '    Dim mySqlAction4 As String = ""
    '    mySqlAction4 = "update tbCustomer set  C_FullName = '" & txtBizFullName.Text.ToUpper.Trim & "',C_Telephone = '" & txtBizTel.Text.ToUpper.Trim & "',C_MobileNo = '" & txtBizMobile.Text.ToUpper.Trim & "',C_Email = '" & txtBizEmail.Text.Trim & "',C_Address1 = '" & txtBizAddress1.Text.ToUpper.Trim & "',C_Address2 = '" & txtBizAdress2.Text.ToUpper.Trim & "',C_Address3 = '" & txtBizAddress3.Text.ToUpper.Trim & "',C_Region = '" & txtBizregion.Text.ToUpper.Trim & "',C_District = '" & txtBizDistrict.Text.ToUpper.Trim & "',C_Area = '" & txtBizArea.Text.ToUpper.Trim & "',C_TINNo = '" & txtBizTinNo.Text.Trim & "',B_BusinessName = '" & txtBizName.Text.Trim & "',B_RegDate = '" & txtBizRegDate.Text.Trim & "',B_RegName = '" & txtBizRegNo.Text.Trim & "',B_NatureofBusiness = '" & txtBizNature.Text.Trim & "',B_DirectorName = '" & txtBizDirectorName.Text.Trim & "' where C_CustomerNo = '" & Session("C_CustomerNo") & "' "
    '    MyDBAction.DBAction(mySqlAction4, DataManagement.DBActionType.Update)
    '    lblBizResponse.Text = "Business details have been Updated successfully"
    '    txtBizFullName.Text = ""
    '    txtBizTel.Text = ""
    '    txtBizMobile.Text = ""
    '    txtBizEmail.Text = ""
    '    txtBizAddress1.Text = ""
    '    txtBizAdress2.Text = ""
    '    txtBizAddress3.Text = ""
    '    txtBizregion.Text = ""
    '    txtBizDistrict.Text = ""
    '    txtBizArea.Text = ""
    '    txtBizTinNo.Text = ""
    '    txtBizRegDate.Text = ""
    '    txtBizRegNo.Text = ""
    '    txtBizDirectorName.Text = ""
    '    txtBizNature.Text = ""
    'End Sub
    'Protected Sub btnDeleteBiz_Click1(sender As Object, e As EventArgs) Handles btnDeleteBiz.Click
    '    Dim mySqlAction5 As String = ""
    '    Dim MyDBAction As New DataManagement
    '    mySqlAction5 = "delete from tbCustomer  where C_CustomerNo = '" & Session("C_CustomerNo") & "' "
    '    MyDBAction.DBAction(mySqlAction5, DataManagement.DBActionType.Delete)
    '    txtBizFullName.Text = ""
    '    txtBizTel.Text = ""
    '    txtBizMobile.Text = ""
    '    txtBizEmail.Text = ""
    '    txtBizAddress1.Text = ""
    '    txtBizAdress2.Text = ""
    '    txtBizAddress3.Text = ""
    '    txtBizregion.Text = ""
    '    txtBizDistrict.Text = ""
    '    txtBizArea.Text = ""
    '    txtBizTinNo.Text = ""
    '    txtBizRegDate.Text = ""
    '    txtBizRegNo.Text = ""
    '    txtBizDirectorName.Text = ""
    '    txtBizNature.Text = ""
    'End Sub
    'Protected Sub btnEditGroup_Click(sender As Object, e As EventArgs) Handles btnEditGroup.Click
    '    Dim mycustomerno As String = ""
    '    mycustomerno = Session("CUSTOMERNO")
    '    Dim trandate As String = Date.Today.ToString("dd-MMM-yy")
    '    Dim MyDBAction As New DataManagement
    '    Dim mySqlAction As String = ""
    '    Dim mySqlAction4 As String = ""
    '    mySqlAction4 = "update tbCustomer set  C_FullName = '" & txtGrpFullName.Text.ToUpper.Trim & "',C_Telephone = '" & txtGrpTel.Text.ToUpper.Trim & "',C_MobileNo = '" & txtGrpMobileNo.Text.ToUpper.Trim & "',C_Email = '" & txtGrpEmail.Text.Trim & "',C_Address1 = '" & txtGrpAddress1.Text.ToUpper.Trim & "',C_Address2 = '" & txtGrpAddress2.Text.ToUpper.Trim & "',C_Address3 = '" & txtGrpAddress3.Text.ToUpper.Trim & "',C_Region = '" & txtGrpRegion.Text.ToUpper.Trim & "',C_District = '" & txtGrpDistrict.Text.ToUpper.Trim & "',C_Area = '" & txtGrpArea.Text.ToUpper.Trim & "',C_TINNo = '" & txtGrpTinNo.Text.Trim & "',B_BusinessName = '" & txtGrpName.Text.ToUpper.Trim & "',B_RegDate = '" & txtGrpregDate.Text.ToUpper.Trim & "',B_RegName = '" & txtGrpRegNo.Text.ToUpper.Trim & "',B_NatureofBusiness = '" & txtGrpBizNature.Text.ToUpper.Trim & "',B_DirectorName = '" & txtGrpDirectorName.Text.ToUpper.Trim & "' where C_CustomerNo = '" & Session("C_CustomerNo") & "' "
    '    MyDBAction.DBAction(mySqlAction4, DataManagement.DBActionType.Update)
    '    lblResponse.Text = "Group details have been Updated successfully"
    '    txtGrpCustNo.Text = ""
    '    txtGrpAcType.Text = ""
    '    txtGrpFullName.Text = ""
    '    txtGrpTel.Text = ""
    '    txtGrpMobileNo.Text = ""
    '    txtGrpEmail.Text = ""
    '    txtGrpAddress2.Text = ""
    '    txtGrpAddress1.Text = ""
    '    txtGrpAddress3.Text = ""
    '    txtGrpRegion.Text = ""
    '    txtGrpDistrict.Text = ""
    '    txtGrpArea.Text = ""
    '    txtGrpTinNo.Text = ""
    '    txtGrpregDate.Text = ""
    '    txtGrpRegNo.Text = ""
    '    txtGrpDirectorName.Text = ""
    '    txtGrpBizNature.Text = ""
    '    txtGrpMaleNo.Text = ""
    '    txtGrpFemaleNo.Text = ""
    '    txtGrpMemberNo.Text = ""
    '    txtGrpName.Text = ""
    'End Sub
    'Protected Sub btnDeleteGroup_Click1(sender As Object, e As EventArgs) Handles btnDeleteGroup.Click
    '    Dim mySqlAction5 As String = ""
    '    Dim MyDBAction As New DataManagement
    '    mySqlAction5 = "delete from tbCustomer  where C_CustomerNo = '" & Session("C_CustomerNo") & "' "
    '    MyDBAction.DBAction(mySqlAction5, DataManagement.DBActionType.Delete)
    '    lblGrpResponse.Text = "Group details have been Updated successfully"
    '    txtGrpCustNo.Text = ""
    '    txtGrpAcType.Text = ""
    '    txtGrpFullName.Text = ""
    '    txtGrpTel.Text = ""
    '    txtGrpMobileNo.Text = ""
    '    txtGrpEmail.Text = ""
    '    txtGrpAddress2.Text = ""
    '    txtGrpAddress1.Text = ""
    '    txtGrpAddress3.Text = ""
    '    txtGrpRegion.Text = ""
    '    txtGrpDistrict.Text = ""
    '    txtGrpArea.Text = ""
    '    txtGrpTinNo.Text = ""
    '    txtGrpregDate.Text = ""
    '    txtGrpRegNo.Text = ""
    '    txtGrpDirectorName.Text = ""
    '    txtGrpBizNature.Text = ""
    '    txtGrpMaleNo.Text = ""
    '    txtGrpFemaleNo.Text = ""
    '    txtGrpMemberNo.Text = ""
    '    txtGrpName.Text = ""
    'End Sub
    Protected Sub btnIndDelete_Click1(sender As Object, e As EventArgs) Handles btnIndDelete.Click
        Dim mySqlAction5 As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction5 = "delete from tbCustomer  where C_CustomerNo = '" & Session("C_CustomerNo") & "' "
        MyDBAction.DBAction(mySqlAction5, DataManagement.DBActionType.Delete)
        lblResponse.Text = "Customer details have been deleted successfully"
        'txtPrefix.Text = ""
        'txtFirstName.Text = ""
        'txtMiddleName.Text = ""
        'txtLastName.Text = ""
        'txtFullName.Text = ""
        'txtTel.Text = ""
        'txtMobileNo.Text = ""
        'txtEmail.Text = ""
        'txtAddress1.Text = ""
        'txtAddress2.Text = ""
        'txtAddress3.Text = ""
        'txtRegion.Text = ""
        'txtDistrict.Text = ""
        'txtArea.Text = ""
        'txtTINNo.Text = ""
        'txtNationalID.Text = ""
        'txtPassportNO.Text = ""
        'txtRelationShipOfficer.Text = ""
        'txtSex.Text = ""
        'txtResidentStatus.Text = ""
        'txtDependants.Text = ""
        'txtEmployerName.Text = ""
        'txtNationality.Text = ""
        'txtDoB.Text = ""
        'txtMaritalStatus.Text = ""
        'txtSpouseName.Text = ""
        'txtOccupation.Text = ""
        'txtLanguage.Text = ""
    End Sub
    Protected Sub Button1_Click(sender As Object, e As EventArgs) Handles Button12.Click
    End Sub
    Protected Sub Button6_Click(sender As Object, e As EventArgs) Handles Button2.Click
    End Sub
    Protected Sub txtAccountCode_TextChanged(sender As Object, e As EventArgs) Handles txtCustomerNo.TextChanged

    End Sub

    Protected Sub getProductCodesforTxt()
        'txtOrgName.Text = ""
        'Label4.Text = ""
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch5 As String = "select AccountCode,AccountPrefix,AccountName,DormancyDays,AssetGL,LiabilityGL,IncomeGL,ExpenseGL,ChargeGL,FeeGL,IntRate,IntFreq,IntAccrualFreq,MinInterestBal,MinAccountBal,DebitsNoPerMonth,Amount from [dbo].[tbSavingsAccountProduct] where AccountCode = '" & txtAccountCode.Text & "'" ' ='" & Me.HiddenField1.Value & "' "
            Dim MyReader5 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch5, DataManagement.DBActionType.SqlDataReader), SqlDataReader)

            If MyReader5.HasRows = True Then
                MyReader5.Read()
                txtProductCode.Text = MyReader5("AccountPrefix").ToString
                AccountCode = mySearch5("AccountCode").ToString
                'txtCustomerNo.Text = MyReader5("AssetGL").ToString
                'drpAccountType.Text = MyReader5("LiabilityGL").ToString
                'drpdPrefix.Text = MyReader5("IntRate").ToString
                'txtFemaleNo.Text = MyReader5("AccountPrefix").ToString

            Else
                'update
                Dim MySqlaction1 As String
                Dim mySqlAction As String = ""
                Dim MyCurrencyName As String = ""
                Dim MyDBAction As New DataManagement
                'MySqlaction1 = "update tbAccount set CustomerNo ='" & txtCustomerNo.Text.ToUpper.Trim & "', AccountType ='" & txtAccountCode.Text.ToUpper.Trim & "', BranchName ='" & txtBranchCode.Text.ToUpper.Trim & "', AccountNo ='" & txtAccountNo.Text.ToUpper.Trim & "',  AcountName ='" & txtAcountName.Text.ToUpper.Trim & "',Currency ='" & txtCurrency.Text.ToUpper.Trim & "' ,Sector = '" & txtSector.Text.ToUpper.Trim & "', SubSector = '" & txtSubSector.Text.ToUpper.Trim & "', DRGL = '" & txtCustomerNo.Text.ToUpper.Trim & "', CRGL = '" & drpAccountType.Text.ToUpper.Trim & "',IntRate = '" & drpdPrefix.Text.ToUpper.Trim & "',DebitNoStatus = '" & txtFirstName.Text.ToUpper.Trim & "' where CustomerNo ='" & txtCustomerNo.Text & "'"
                MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
                DisplayDetails("All")
                Label7.Text = "Saving Account details have been Updated successfully"

                txtCustomerNo.ReadOnly = False


                'DisplayDetails("All")
                'AUDIT TRAIL-------
                'Dim audit As New DataManagement
                'audit.AddAdminAuditTrail(Session("UserName").ToString, "Action", "Successful Saving of branch  '" & txtOrgName.Text.ToUpper & "'", _
                'Request.Url.ToString, Session("ip").ToString, Session("sessionid").ToString)
                ''----END AUDIT TRAIL-----


            End If
            MyReader5.Close()
        Catch ex As Exception

        End Try
    End Sub
    Protected Sub drpAccountType_OnSelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
        OpenSelectedTab()
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
    Protected Sub txtAdress3_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs)

    End Sub
    Protected Sub txtEmail_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs)

    End Sub

End Class