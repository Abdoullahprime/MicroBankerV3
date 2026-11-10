Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Imports System.Data.SqlClient
Imports System.Globalization

Public Class EditCustomerAccount
    Inherits System.Web.UI.Page
    Public AccountCode As String = "", AccountPrefix As String = "", AccountName As String = "", DormancyDays As String = "", AssetGL As String = "", LiabilityGL As String = "", IncomeGL As String = "", ExpenseGL As String = "", ChargeGL As String = "", FeeGL As String = "", IntRate As String = "", IntFreq As String = "", IntAccrualFreq As String = "", MinInterestBal As String = "", MinAccountBal As String = "", DebitsNoPerMonth As String = "", Amount As String = ""
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            GetSectors()
            GetSubSectors()
            DisplayDetails("All")
            MultiView1.SetActiveView(View1)
            DOBDatePicker.Visible = False
            BizRegDatePicker.Visible = False
            'GrpRegDatePicker.Visible = False
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
                    txtProductName.Enabled = False
                    btnCheckBranch.Visible = False
                    btnSearchCode.Visible = False
                    cmasearch.Visible = False
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
                            drpRegion.SelectedValue = MyReader("C_Region").ToString
                            drpArea.SelectedValue = MyReader("C_Area").ToString
                            drpDistrict.SelectedValue = MyReader("C_District").ToString
                            txtSex.Text = MyReader("I_Sex").ToString
                            txtPassport.Text = MyReader("I_PassportNO").ToString
                            txtNationality.Text = MyReader("I_Nationality").ToString
                            txtLanguage.Text = MyReader("I_Language").ToString
                            txtDoB.Text = MyReader("I_DoB").ToString
                            txtResidential.Text = MyReader("I_ResidentStatus").ToString
                            txtBizTel.Text = MyReader("B_TelNo").ToString
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
            Dim MyDBAction As New DataManagement
            Dim MyDatable As New Data.DataTable
            Select Case ShowType
                Case "All"
                    'lblGrpResponse.Text = ""
                    SqlAction = "select CustomerNo,AccountType,BranchName,AccountNo,AccountName,Currency,Sector,SubSector,IntRate,DebitNoStatus,BranchCode,AccountPrefix,C_Address1 from tbAccount where Approved != 1 and ProductDefCode = 'SA'"
                    MyDatable = TryCast(MyDBAction.DBAction(SqlAction, DataManagement.DBActionType.DataTable), Data.DataTable)
                    Session("searchtable") = MyDatable
                    GridView1.DataSource = MyDatable
                    GridView1.DataBind()
                Case "Filter"
                    searchStr = txtSearch.Text
                    If RdoSearch.Items(0).Selected = True Then
                        searchStr = "%" & searchStr & "%"
                        SqlAction = "select CustomerNo,AccountType,BranchName,AccountNo,AccountName,Currency,Sector,SubSector,IntRate,DebitNoStatus,BranchCode,AccountPrefix,C_Address1 from tbAccount  where AccountNo LIKE  '" & searchStr & "' and Approved != 1 order by id desc"
                        MyDatable = TryCast(MyDBAction.DBAction(SqlAction, DataManagement.DBActionType.DataTable), Data.DataTable)
                        Session("searchtable") = MyDatable
                        GridView1.DataSource = MyDatable
                        GridView1.DataBind()
                    Else
                        searchStr = "%" & searchStr & "%"
                        SqlAction = "select CustomerNo,AccountType,BranchName,AccountNo,AccountName,Currency,Sector,SubSector,IntRate,DebitNoStatus,BranchCode,AccountPrefix,C_Address1 from tbAccount  where AcountName LIKE  '" & searchStr & "' and Approved != 1  order by id desc"
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
        MySqlaction1 = "update tbAccount set AccountName ='" & txtAcountName.Text.ToUpper.Trim & "', Sector ='" & drpSector.SelectedValue.ToUpper.Trim & "', SubSector ='" & drpSubSector.SelectedValue.ToUpper.Trim & "', AccountType ='" & drpAccountType.Text.ToUpper.Trim & "',  C_Address1 ='" & txtAddress1.Text.ToUpper.Trim & "',C_Address2 ='" & txtAddress2.Text.ToUpper.Trim & "' ,C_Address3 = '" & txtAddress3.Text.ToUpper.Trim & "', C_Email = '" & txtEmail.Text.Trim & "', C_TINNo = '" & txtTillNo.Text.ToUpper.Trim & "', C_Telephone = '" & txtTelNo.Text.ToUpper.Trim & "',C_MobileNo = '" & txtMobileNo.Text.ToUpper.Trim & "',I_FirstName = '" & txtFirstName.Text.ToUpper.Trim & "',I_MiddleName = '" & txtMiddleName.Text.ToUpper.Trim & "',I_LastName = '" & txtLastName.Text.ToUpper.Trim & "',I_NationalID = '" & txtIDNo.Text.ToUpper.Trim & "',C_RelationShipOfficer = '" & txtRelOfficer.Text.ToUpper.Trim & "',I_MaritalStatus = '" & drpMaritalStatus.Text.ToUpper.Trim & "',I_SpouseName = '" & txtSpouseName.Text.ToUpper.Trim & "',I_Dependants = '" & txtDepemdant.Text.ToUpper.Trim & "',I_Occupation = '" & txtOccupation.Text.ToUpper.Trim & "',I_EmployerName = '" & txtEmployer.Text.ToUpper.Trim & "',B_BusinessName = '" & txtBizName.Text.ToUpper.Trim & "',B_RegDate = '" & txtBizRegDate.Text.ToUpper.Trim & "',B_RegName = '" & txtRegNo.Text.ToUpper.Trim & "',B_NatureofBusiness = '" & txtBizNature.Text.ToUpper.Trim & "',B_DirectorName = '" & txtDirName.Text.ToUpper.Trim & "',G_GroupName = '" & txtgrpName.Text.ToUpper.Trim & "',G_NoofMembers = '" & txtgrpMemberNo.Text.ToUpper.Trim & "',G_NoofMale = '" & txtMaleNo.Text.ToUpper.Trim & "',G_NoofFemale = '" & txtFemaleNo.Text.ToUpper.Trim & "',C_Region = '" & drpRegion.SelectedValue.ToUpper.Trim & "' ,C_District = '" & drpDistrict.SelectedValue.ToUpper.Trim & "',C_Area = '" & drpArea.SelectedValue.ToUpper.Trim & "',I_ResidentStatus ='" & txtResidential.Text.ToUpper.Trim & "',I_Sex = '" & txtSex.Text.ToUpper.Trim & "',I_PassportNO = '" & txtPassport.Text.ToUpper.Trim & "',I_Nationality = '" & txtNationality.Text.ToUpper.Trim & "',I_DoB = '" & txtDoB.Text.ToUpper.Trim & "',I_Language = '" & txtLanguage.Text.ToUpper.Trim & "',B_TelNo = '" & txtTelNo.Text.ToUpper.Trim & "' where AccountNo ='" & myaccountno & "'"
        MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
        DisplayDetails("All")
        lblResponse.Text = "Saving Account details have been Updated successfully"
        txtCustomerNo.Enabled = False
        txtBranchCode.Enabled = False
        txtAccountCode.Enabled = False
        txtAccountNo.Enabled = False
        txtBranchName.Enabled = False
        txtCurrency.Enabled = False
        txtProductName.Enabled = False
        btnCheckBranch.Visible = False
        btnSearchCode.Visible = False
        cmasearch.Visible = False
        txtCustomerNo.ReadOnly = False
        txtCustomerNo.Text = ""
        txtAccountCode.Text = ""
        txtAccountNo.Text = ""
        HiddenField1.Value = ""
        txtAcountName.Text = ""
        txtCurrency.Text = ""
        drpSector.SelectedIndex = -1
        drpSubSector.SelectedIndex = -1
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
        drpArea.Text = ""
        drpDistrict.Text = ""
        drpRegion.Text = ""
        txtSex.Text = ""
        txtPassport.Text = ""
        txtNationality.Text = ""
        txtDoB.Text = ""
        txtLanguage.Text = ""
        txtResidential.Text = ""
        txtBizTel.Text = ""
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
    Protected Sub btnCheckBranch_Click(sender As Object, e As EventArgs) Handles btnCheckBranch.Click
        Try
            Dim MyDatObject As New DataManagement
            Dim mySearch4 As String = "select BranchName from Branches where BranchCode = '" & txtBranchCode.Text & "'" ' ='" & Me.HiddenField1.Value & "' "
            Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)

            If MyReader4.HasRows = True Then
                MyReader4.Read()
                txtBranchName.Text = MyReader4("BranchName").ToString
                'txtAccountName.Text = MyReader("AccountName").ToString
                'txtFullName.Text = MyReader("DormancyDays").ToString

            End If
            ' Dim MyDatObject As New DataManagement
            'Dim mySearch5 As String = "select IntRate,LiabilityGL,AssetGL from [dbo].[tbSavingsAccountProduct] where AccountCode = '" & drpAccountType.Text & "'" ' ='" & Me.HiddenField1.Value & "' "
            'Dim MyReader5 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch5, DataManagement.DBActionType.SqlDataReader), SqlDataReader)

            'If MyReader5.HasRows = True Then
            '    MyReader5.Read()
            '    drpdPrefix.Text = MyReader5("IntRate").ToString
            '    txtCustomerNo.Text = MyReader5("LiabilityGL").ToString
            '    drpAccountType.Text = MyReader5("AssetGL").ToString

            'End If
            'Dim MyService As New checkBfub.BFUBEnquiries
            'Dim a As String = MyService.AccountBalanceEnq(txtOrgCode.Text).branchName
            'Dim b As String = MyService.AccountBalanceEnq(txtOrgCode.Text).branchSortCode
            'Dim c As String = MyService.AccountBalanceEnq(txtsuspenceacc.Text).branchName
            'txtOrgName.Text = a

            ''txtAdress1.Text = b

            'b = b.Substring(5, 3)
            'txtAdress1.Text = b
            'txtOrgCode.ReadOnly = True

            'b = "099"
            'txtAdress1.Text = b
            'txtOrgCode.ReadOnly = True

            'If b = "" Then
            '    Label4.Text = "The Commission A/c does not exist "
            '    txtOrgCode.ReadOnly = False
            'End If
            'DropDownList1.Text = c
            MyReader4.Close()
        Catch ex As Exception
            'txtOrgName.Text = ""
            'txtAdress1.Text = ""
            '' DropDownList1.Text = ""
            'txtOrgCode.ReadOnly = False
            'Label4.Text = "Error Pulling Account Details, Commission A/c must start 01M8 .... E6 "
        End Try
    End Sub
    Protected Sub btnSearchCode_Click(sender As Object, e As EventArgs) Handles btnSearchCode.Click
        getProductCodesforTxt()
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
                txtProductName.Text = MyReader5("AccountPrefix").ToString
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
    Protected Sub cmasearch_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmasearch.Click
        Dim mystring As String = ""
        'mystring = txtOrgCode.Text.ToUpper()
        'If Mid(mystring, 1, 4).Contains("01M8") = True Then
        '    Label4.Text = ""
        'getProductCodes()
        'getAcc()
        'createAccountNumber(txtBranchCode.Text, txtCustomerNo.Text, txtProductCode.Text)

        '    Exit Sub
        'Else
        ' Label4.Text = "This is not the correct format of a commission account"
        'End If
    End Sub
    Protected Sub txtAdress3_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs)

    End Sub
    Protected Sub txtEmail_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs)

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