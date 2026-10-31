Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Imports System.Data.SqlClient

Public Class EditCustomerDetails
    Inherits System.Web.UI.Page
    Dim drpRegions As String = ""
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            GetRegion()
            GetArea()
            DisplayDetails("All")
            MultiView1.SetActiveView(View1)
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
    Private Sub GetBizRegion()
        'Load Products
        Try
            Dim MyDataTable As New Data.DataTable
            Dim MyDBAction As New DataManagement
            MyDataTable = TryCast(MyDBAction.DBAction("select RegionCode,RegionName  from Region", DataManagement.DBActionType.SqlDataTable), Data.DataTable)
            If MyDataTable IsNot Nothing Then
                With drpBizRegion
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
    Private Sub GetBizArea()
        'Load Products
        Try
            Dim MyDataTable As New Data.DataTable
            Dim MyDBAction As New DataManagement
            MyDataTable = TryCast(MyDBAction.DBAction("select AreaCode,AreaName  from Area", DataManagement.DBActionType.SqlDataTable), Data.DataTable)
            If MyDataTable IsNot Nothing Then
                With drpBizArea
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
    Private Sub GetBizDistrict()
        'Load Products
        Try
            Dim MyDataTable As New Data.DataTable
            Dim MyDBAction As New DataManagement
            MyDataTable = TryCast(MyDBAction.DBAction("select DistrictCode,DistrictName  from District", DataManagement.DBActionType.SqlDataTable), Data.DataTable)
            If MyDataTable IsNot Nothing Then
                With drpBizDistrict
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
    Private Sub GetGroupRegion()
        'Load Products
        Try
            Dim MyDataTable As New Data.DataTable
            Dim MyDBAction As New DataManagement
            MyDataTable = TryCast(MyDBAction.DBAction("select RegionCode,RegionName  from Region", DataManagement.DBActionType.SqlDataTable), Data.DataTable)
            If MyDataTable IsNot Nothing Then
                With drpGrpRegion
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
    Private Sub GetGroupArea()
        'Load Products
        Try
            Dim MyDataTable As New Data.DataTable
            Dim MyDBAction As New DataManagement
            MyDataTable = TryCast(MyDBAction.DBAction("select AreaCode,AreaName  from Area", DataManagement.DBActionType.SqlDataTable), Data.DataTable)
            If MyDataTable IsNot Nothing Then
                With DrpGrpArea
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
    Private Sub GetGroupDistrict()
        'Load Products
        Try
            Dim MyDataTable As New Data.DataTable
            Dim MyDBAction As New DataManagement
            MyDataTable = TryCast(MyDBAction.DBAction("select DistrictCode,DistrictName  from District", DataManagement.DBActionType.SqlDataTable), Data.DataTable)
            If MyDataTable IsNot Nothing Then
                With drpGrpDistrict
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

    Protected Sub GridView1_RowCommand(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewCommandEventArgs) Handles GridView1.RowCommand
        Try
            Dim MyID As String = e.CommandArgument
            HiddenField1.Value = MyID
            Select Case e.CommandName
                Case "select"

                    ShowAgentDetails("Select C_CustomerNo,C_CustomerType,I_PreFix,I_FirstName,I_MiddleName,I_LastName,C_FullName,C_Telephone,C_MobileNo,C_Email,C_Address1,C_Address2,C_Address3,C_Region,C_District,C_Area,C_TINNo,I_NationalID,I_PassportNO,C_RelationShipOfficer,I_Nationality,I_DoB,I_ResidentStatus,I_Language,I_Sex,I_MaritalStatus,I_SpouseName,I_Dependants,I_Occupation,I_EmployerName,B_BusinessName,B_RegDate,B_RegName,B_TelNo,B_NatureofBusiness,B_DirectorName,G_GroupName,G_NoofMembers,G_NoofMale,G_NoofFemale from tbCustomer where C_CustomerNo='" & HiddenField1.Value.ToString & "'  ", "ViewDetails")
                    If drpAccountType.Text = "INDIVIDUAL ACCOUNT" Then
                        MultiView1.SetActiveView(View2)
                    ElseIf drpAccountType.Text = "BUSINESS ACCOUNT" Then
                        MultiView1.SetActiveView(View3)
                    Else
                        MultiView1.SetActiveView(View4)
                    End If
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
                            GetRegion()
                            GetArea()
                            GetDistrict()
                            GetBizRegion()
                            GetBizArea()
                            GetBizDistrict()
                            GetGroupArea()
                            GetGroupRegion()
                            GetGroupDistrict()
                            txtCustNo.Text = MyReader("C_CustomerNo").ToString
                            txtBizCuztNo.Text = MyReader("C_CustomerNo").ToString
                            txtGrpCustNo.Text = MyReader("C_CustomerNo").ToString
                            Session("C_CustomerNo") = MyReader("C_CustomerNo").ToString
                            drpAccountType.SelectedValue = MyReader("C_CustomerType").ToString
                            txtBizAcTpype.Text = MyReader("C_CustomerType").ToString
                            txtGrpAcType.Text = MyReader("C_CustomerType").ToString
                            Session("C_CustomerType") = MyReader("C_CustomerType").ToString
                            acctimage = MyReader("I_PreFix").ToString
                            Session("I_PreFix") = MyReader("I_PreFix").ToString
                            drpdPrefix.SelectedValue = MyReader("I_PreFix").ToString
                            Session("I_FirstName") = MyReader("I_FirstName").ToString
                            txtFirstName.Text = MyReader("I_FirstName").ToString '& " " & MyReader("secondname").ToString
                            Session("I_MiddleName") = MyReader("I_MiddleName").ToString
                            txtMiddleName.Text = MyReader("I_MiddleName").ToString
                            Session("I_LastName") = MyReader("I_LastName").ToString
                            txtLastName.Text = MyReader("I_LastName").ToString
                            'waitara 01112021
                            drpRegion.SelectedValue = MyReader("C_Region").ToString
                            drpArea.SelectedValue = MyReader("C_Area").ToString
                            drpDistrict.SelectedValue = MyReader("C_District").ToString
                            drpBizArea.SelectedValue = MyReader("C_Area").ToString
                            drpBizDistrict.SelectedValue = MyReader("C_District").ToString
                            drpBizRegion.SelectedValue = MyReader("C_Region").ToString
                            DrpGrpArea.SelectedValue = MyReader("C_Area").ToString
                            drpGrpDistrict.SelectedValue = MyReader("C_District").ToString
                            drpGrpRegion.SelectedValue = MyReader("C_Region").ToString
                            Try
                                'txtDOB.Text = CDate(MyReader("DateofBirth")).ToString("dd-MMM-yy").Trim
                                'Session("DateOfBirth") = CDate(MyReader("DateofBirth")).ToString("dd-MMM-yy").Trim
                                'txtDOB.Text = txtDOB.Text.ToUpper
                                'Session("DOB") = CDate(MyReader("DateofBirth").ToString)
                            Catch ex As Exception
                            End Try
                            Session("C_FullName") = MyReader("C_FullName").ToString
                            txtFullName.Text = MyReader("C_FullName").ToString
                            txtBizFullName.Text = MyReader("C_FullName").ToString
                            txtGrpFullName.Text = MyReader("C_FullName").ToString
                            Session("C_Telephone") = MyReader("C_Telephone").ToString
                            txtTel.Text = MyReader("C_Telephone").ToString
                            txtBizTel.Text = MyReader("C_Telephone").ToString
                            txtGrpTel.Text = MyReader("C_Telephone").ToString
                            Session("C_MobileNo") = MyReader("C_MobileNo").ToString
                            txtMobileNo.Text = MyReader("C_MobileNo").ToString
                            txtBizMobile.Text = MyReader("C_MobileNo").ToString
                            txtGrpMobileNo.Text = MyReader("C_MobileNo").ToString
                            Session("C_Email") = MyReader("C_Email").ToString
                            txtEmail.Text = MyReader("C_Email").ToString
                            txtBizEmail.Text = MyReader("C_Email").ToString
                            txtGrpEmail.Text = MyReader("C_Email").ToString
                            Session("C_Address1") = MyReader("C_Address1").ToString
                            txtAddress1.Text = MyReader("C_Address1").ToString
                            txtBizAddress1.Text = MyReader("C_Address1").ToString
                            txtGrpAddress1.Text = MyReader("C_Address1").ToString
                            Session("C_Address2") = MyReader("C_Address2").ToString
                            txtAddress2.Text = MyReader("C_Address2").ToString
                            txtBizAdress2.Text = MyReader("C_Address2").ToString
                            txtGrpAddress2.Text = MyReader("C_Address2").ToString
                            Session("C_Address3") = MyReader("C_Address3").ToString
                            txtAddress3.Text = MyReader("C_Address3").ToString
                            txtBizAddress3.Text = MyReader("C_Address3").ToString
                            txtGrpAddress3.Text = MyReader("C_Address3").ToString
                            Session("C_Region") = MyReader("C_Region").ToString
                            'txtRegion.Text = MyReader("C_Region").ToString
                            'txtBizregion.Text = MyReader("C_Region").ToString
                            'txtGrpRegion.Text = MyReader("C_Region").ToString
                            Session("C_District") = MyReader("C_District").ToString
                            'drpDistrict.Text = MyReader("C_District").ToString
                            'txtBizDistrict.Text = MyReader("C_District").ToString
                            'txtGrpDistrict.Text = MyReader("C_District").ToString
                            'txtArea.Text = MyReader("C_Area").ToString
                            'txtBizArea.Text = MyReader("C_Area").ToString
                            'txtGrpArea.Text = MyReader("C_Area").ToString
                            Session("C_Area") = MyReader("C_Area").ToString
                            txtTINNo.Text = MyReader("C_TINNo").ToString
                            txtBizTinNo.Text = MyReader("C_TINNo").ToString
                            txtGrpTinNo.Text = MyReader("C_TINNo").ToString
                            Session("C_TINNo") = MyReader("C_TINNo").ToString
                            txtNationalID.Text = MyReader("I_NationalID").ToString
                            Session("I_NationalID") = MyReader("I_NationalID").ToString
                            txtPassportNO.Text = MyReader("I_PassportNO").ToString
                            Session("I_PassportNO") = MyReader("I_PassportNO").ToString
                            txtRelationShipOfficer.Text = MyReader("C_RelationShipOfficer").ToString
                            Session("C_RelationShipOfficer") = MyReader("C_RelationShipOfficer").ToString
                            txtNationality.Text = MyReader("I_Nationality").ToString
                            Session("I_Nationality") = MyReader("I_Nationality").ToString
                            txtDoB.Text = MyReader("I_DoB").ToString
                            Session("I_DoB") = MyReader("I_DoB").ToString
                            txtResidentStatus.Text = MyReader("I_ResidentStatus").ToString
                            Session("I_ResidentStatus") = MyReader("I_ResidentStatus").ToString
                            txtLanguage.Text = MyReader("I_Language").ToString
                            Session("I_Language") = MyReader("I_Language").ToString
                            drpGender.SelectedValue = MyReader("I_Sex").ToString
                            Session("I_Sex") = MyReader("I_Sex").ToString
                            drpMaritalStatus.SelectedValue = MyReader("I_MaritalStatus").ToString
                            Session("I_MaritalStatus") = MyReader("I_MaritalStatus").ToString
                            txtSpouseName.Text = MyReader("I_SpouseName").ToString
                            Session("I_SpouseName") = MyReader("I_SpouseName").ToString
                            txtDependants.Text = MyReader("I_Dependants").ToString
                            Session("I_Dependants") = MyReader("I_Dependants").ToString
                            txtOccupation.Text = MyReader("I_Occupation").ToString
                            Session("I_Occupation") = MyReader("I_Occupation").ToString
                            txtEmployerName.Text = MyReader("I_EmployerName").ToString
                            Session("I_EmployerName") = MyReader("I_EmployerName").ToString
                            'other fields for business account
                            txtBizName.Text = MyReader("B_BusinessName").ToString
                            Session("B_BusinessName") = MyReader("B_BusinessName").ToString
                            txtBizRegDate.Text = MyReader("B_RegDate").ToString
                            Session("B_RegDate") = MyReader("B_RegDate").ToString
                            txtBizRegNo.Text = MyReader("B_RegName").ToString
                            Session("B_RegName") = MyReader("B_RegName").ToString
                            txtBizNature.Text = MyReader("B_NatureofBusiness").ToString
                            Session("B_NatureofBusiness") = MyReader("B_NatureofBusiness").ToString
                            txtBizDirectorName.Text = MyReader("B_DirectorName").ToString
                            Session("B_DirectorName") = MyReader("B_DirectorName").ToString
                            'other fields for Group account
                            txtGrpName.Text = MyReader("G_GroupName").ToString
                            Session("G_GroupName") = MyReader("G_GroupName").ToString
                            txtGrpregDate.Text = MyReader("B_RegDate").ToString
                            Session("B_RegDate") = MyReader("B_RegDate").ToString
                            txtGrpRegNo.Text = MyReader("B_RegName").ToString
                            Session("B_RegName") = MyReader("B_RegName").ToString
                            txtGrpBizNature.Text = MyReader("B_NatureofBusiness").ToString
                            Session("B_NatureofBusiness") = MyReader("B_NatureofBusiness").ToString
                            txtGrpDirectorName.Text = MyReader("B_DirectorName").ToString
                            Session("B_DirectorName") = MyReader("B_DirectorName").ToString
                            txtGrpMemberNo.Text = MyReader("G_NoofMembers").ToString
                            Session("G_NoofMembers") = MyReader("G_NoofMembers").ToString
                            txtGrpMaleNo.Text = MyReader("G_NoofMale").ToString
                            Session("G_NoofMale") = MyReader("G_NoofMale").ToString
                            txtGrpFemaleNo.Text = MyReader("G_NoofFemale").ToString
                            Session("G_NoofFemale") = MyReader("G_NoofFemale").ToString
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
                    lblGrpResponse.Text = ""
                    SqlAction = "select C_CustomerNo,C_CustomerType,I_PreFix,I_FirstName,I_MiddleName,I_LastName,C_FullName,C_Telephone,C_MobileNo,C_Email,C_Address1,C_Address2,C_Address3,C_Region,C_District,C_Area,C_TINNo,I_NationalID,I_PassportNO,C_RelationShipOfficer,I_Nationality,I_DoB,I_ResidentStatus,I_Language,I_Sex,I_MaritalStatus,I_SpouseName,I_Dependants,I_Occupation,I_EmployerName from tbCustomer"
                    MyDatable = TryCast(MyDBAction.DBAction(SqlAction, DataManagement.DBActionType.DataTable), Data.DataTable)
                    Session("searchtable") = MyDatable
                    GridView1.DataSource = MyDatable
                    GridView1.DataBind()
                Case "Filter"
                    searchStr = txtSearch.Text
                    If RdoSearch.Items(0).Selected = True Then
                        searchStr = "%" & searchStr & "%"
                        SqlAction = "select C_CustomerNo,C_CustomerType,I_PreFix,I_FirstName,I_MiddleName,I_LastName,C_FullName,C_Telephone,C_MobileNo,C_Email,C_Address1,C_Address2,C_Address3,C_Region,C_District,C_Area,C_TINNo,I_NationalID,I_PassportNO,C_RelationShipOfficer,I_Nationality,I_DoB,I_ResidentStatus,I_Language,I_Sex,I_MaritalStatus,I_SpouseName,I_Dependants,I_Occupation,I_EmployerName from tbCustomer  where C_CustomerNo LIKE  '" & searchStr & "'  order by id desc"
                        MyDatable = TryCast(MyDBAction.DBAction(SqlAction, DataManagement.DBActionType.DataTable), Data.DataTable)
                        Session("searchtable") = MyDatable
                        GridView1.DataSource = MyDatable
                        GridView1.DataBind()
                    Else
                        searchStr = "%" & searchStr & "%"
                        SqlAction = "select C_CustomerNo,C_CustomerType,I_PreFix,I_FirstName,I_MiddleName,I_LastName,C_FullName,C_Telephone,C_MobileNo,C_Email,C_Address1,C_Address2,C_Address3,C_Region,C_District,C_Area,C_TINNo,I_NationalID,I_PassportNO,C_RelationShipOfficer,I_Nationality,I_DoB,I_ResidentStatus,I_Language,I_Sex,I_MaritalStatus,I_SpouseName,I_Dependants,I_Occupation,I_EmployerName from tbCustomer  where C_FullName LIKE  '" & searchStr & "'  order by id desc"
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
    Protected Sub Button4_DataBinding(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnEditGroup.DataBinding
    End Sub

    Protected Sub Button4_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnEditGroup.Disposed
    End Sub
    Protected Sub GridView29_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs)
    End Sub
    Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click, btnEditBiz.Click
        Dim mycustomerno As String = ""
        mycustomerno = Session("CUSTOMERNO")
        Dim trandate As String = Date.Today.ToString("dd-MMM-yy")
        Dim MyDBAction As New DataManagement
        Dim mySqlAction As String = ""
        Dim mySqlAction4 As String = ""
        mySqlAction4 = "update tbCustomer set I_PreFix = '" & drpdPrefix.SelectedValue.ToUpper.Trim & "',I_FirstName = '" & txtFirstName.Text.ToUpper.Trim & "',I_MiddleName = '" & txtMiddleName.Text.ToUpper.Trim & "',C_FullName = '" & txtFullName.Text.ToUpper.Trim & "',C_Telephone = '" & txtTel.Text.ToUpper.Trim & "',C_MobileNo = '" & txtMobileNo.Text.ToUpper.Trim & "',C_Email = '" & txtEmail.Text.Trim & "',C_Address1 = '" & txtAddress1.Text.ToUpper.Trim & "',C_Address2 = '" & txtAddress2.Text.ToUpper.Trim & "',C_Address3 = '" & txtAddress3.Text.ToUpper.Trim & "',C_Region = '" & drpRegion.SelectedValue.ToUpper.Trim & "',C_District = '" & drpDistrict.SelectedValue.ToUpper.Trim & "',C_Area = '" & drpArea.SelectedValue.ToUpper.Trim & "',C_TINNo = '" & txtTINNo.Text.Trim & "',I_NationalID = '" & txtNationalID.Text.Trim & "',I_PassportNO = '" & txtPassportNO.Text.Trim & "',C_RelationShipOfficer = '" & txtRelationShipOfficer.Text.ToUpper.Trim & "',I_Nationality = '" & txtNationality.Text.ToUpper.Trim & "',I_ResidentStatus = '" & txtResidentStatus.Text.ToUpper.Trim & "',I_Language = '" & txtLanguage.Text.ToUpper.Trim & "',I_Sex = '" & drpGender.SelectedValue.ToUpper.Trim & "',I_MaritalStatus = '" & drpMaritalStatus.SelectedValue.ToUpper.Trim & "',I_SpouseName = '" & txtSpouseName.Text.ToUpper.Trim & "',I_Dependants = '9',I_Occupation = '" & txtOccupation.Text.ToUpper.Trim & "',I_EmployerName = '" & txtEmployerName.Text.ToUpper.Trim & "'  where C_CustomerNo = '" & Session("C_CustomerNo") & "' "
        MyDBAction.DBAction(mySqlAction4, DataManagement.DBActionType.Update)
        lblResponse.Text = "Customer details have been Updated successfully"
        drpdPrefix.Text = ""
        txtFirstName.Text = ""
        txtMiddleName.Text = ""
        txtLastName.Text = ""
        txtFullName.Text = ""
        txtTel.Text = ""
        txtMobileNo.Text = ""
        txtEmail.Text = ""
        txtAddress1.Text = ""
        txtAddress2.Text = ""
        txtAddress3.Text = ""
        drpRegion.Text = ""
        drpDistrict.Text = ""
        drpArea.Text = ""
        txtTINNo.Text = ""
        txtLanguage.Text = ""
        txtNationalID.Text = ""
        txtPassportNO.Text = ""
        txtRelationShipOfficer.Text = ""
        drpGender.Text = ""
        txtResidentStatus.Text = ""
        txtDependants.Text = ""
        txtEmployerName.Text = ""
        txtNationality.Text = ""
        txtDoB.Text = ""
        drpMaritalStatus.Text = ""
        txtSpouseName.Text = ""
        txtOccupation.Text = ""
        drpAccountType.Text = ""
        txtCustNo.Text = ""
    End Sub
    Protected Sub btnEditBiz_Click(sender As Object, e As EventArgs) Handles btnEditBiz.Click
        Dim mycustomerno As String = ""
        mycustomerno = Session("CUSTOMERNO")
        Dim trandate As String = Date.Today.ToString("dd-MMM-yy")
        Dim MyDBAction As New DataManagement
        Dim mySqlAction As String = ""
        Dim mySqlAction4 As String = ""
        mySqlAction4 = "update tbCustomer set  C_FullName = '" & txtBizFullName.Text.ToUpper.Trim & "',C_Telephone = '" & txtBizTel.Text.ToUpper.Trim & "',C_MobileNo = '" & txtBizMobile.Text.ToUpper.Trim & "',C_Email = '" & txtBizEmail.Text.Trim & "',C_Address1 = '" & txtBizAddress1.Text.ToUpper.Trim & "',C_Address2 = '" & txtBizAdress2.Text.ToUpper.Trim & "',C_Address3 = '" & txtBizAddress3.Text.ToUpper.Trim & "',C_Region = '" & drpBizRegion.SelectedValue.ToUpper.Trim & "',C_District = '" & drpBizDistrict.SelectedValue.ToUpper.Trim & "',C_Area = '" & drpBizArea.SelectedValue.ToUpper.Trim & "',C_TINNo = '" & txtBizTinNo.Text.Trim & "',B_BusinessName = '" & txtBizName.Text.Trim & "',B_RegDate = '" & txtBizRegDate.Text.Trim & "',B_RegName = '" & txtBizRegNo.Text.Trim & "',B_NatureofBusiness = '" & txtBizNature.Text.Trim & "',B_DirectorName = '" & txtBizDirectorName.Text.Trim & "' where C_CustomerNo = '" & Session("C_CustomerNo") & "' "
        MyDBAction.DBAction(mySqlAction4, DataManagement.DBActionType.Update)
        lblBizResponse.Text = "Business details have been Updated successfully"
        txtBizFullName.Text = ""
        txtBizTel.Text = ""
        txtBizMobile.Text = ""
        txtBizEmail.Text = ""
        txtBizAddress1.Text = ""
        txtBizAdress2.Text = ""
        txtBizAddress3.Text = ""
        drpBizRegion.Text = ""
        drpBizDistrict.Text = ""
        drpBizArea.Text = ""
        txtBizTinNo.Text = ""
        txtBizRegDate.Text = ""
        txtBizRegNo.Text = ""
        txtBizDirectorName.Text = ""
        txtBizNature.Text = ""
        txtBizAcTpype.Text = ""
        txtBizCuztNo.Text = ""
    End Sub
    Protected Sub btnDeleteBiz_Click1(sender As Object, e As EventArgs) Handles btnDeleteBiz.Click
        Dim mySqlAction5 As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction5 = "delete from tbCustomer  where C_CustomerNo = '" & Session("C_CustomerNo") & "' "
        MyDBAction.DBAction(mySqlAction5, DataManagement.DBActionType.Delete)
        txtBizFullName.Text = ""
        txtBizTel.Text = ""
        txtBizMobile.Text = ""
        txtBizEmail.Text = ""
        txtBizAddress1.Text = ""
        txtBizAdress2.Text = ""
        txtBizAddress3.Text = ""
        drpBizRegion.Text = ""
        drpBizDistrict.Text = ""
        drpBizArea.Text = ""
        txtBizTinNo.Text = ""
        txtBizRegDate.Text = ""
        txtBizRegNo.Text = ""
        txtBizDirectorName.Text = ""
        txtBizNature.Text = ""
    End Sub
    Protected Sub btnEditGroup_Click(sender As Object, e As EventArgs) Handles btnEditGroup.Click
        Dim mycustomerno As String = ""
        mycustomerno = Session("CUSTOMERNO")
        Dim trandate As String = Date.Today.ToString("dd-MMM-yy")
        Dim MyDBAction As New DataManagement
        Dim mySqlAction As String = ""
        Dim mySqlAction4 As String = ""
        mySqlAction4 = "update tbCustomer set  C_FullName = '" & txtGrpFullName.Text.ToUpper.Trim & "',C_Telephone = '" & txtGrpTel.Text.ToUpper.Trim & "',C_MobileNo = '" & txtGrpMobileNo.Text.ToUpper.Trim & "',C_Email = '" & txtGrpEmail.Text.Trim & "',C_Address1 = '" & txtGrpAddress1.Text.ToUpper.Trim & "',C_Address2 = '" & txtGrpAddress2.Text.ToUpper.Trim & "',C_Address3 = '" & txtGrpAddress3.Text.ToUpper.Trim & "',C_Region = '" & drpGrpRegion.SelectedValue.ToUpper.Trim & "',C_District = '" & drpDistrict.SelectedValue.ToUpper.Trim & "',C_Area = '" & DrpGrpArea.SelectedValue.ToUpper.Trim & "',C_TINNo = '" & txtGrpTinNo.Text.Trim & "',B_BusinessName = '" & txtGrpName.Text.ToUpper.Trim & "',B_RegDate = '" & txtGrpregDate.Text.ToUpper.Trim & "',B_RegName = '" & txtGrpRegNo.Text.ToUpper.Trim & "',B_NatureofBusiness = '" & txtGrpBizNature.Text.ToUpper.Trim & "',B_DirectorName = '" & txtGrpDirectorName.Text.ToUpper.Trim & "' where C_CustomerNo = '" & Session("C_CustomerNo") & "' "
        MyDBAction.DBAction(mySqlAction4, DataManagement.DBActionType.Update)
        lblResponse.Text = "Group details have been Updated successfully"
        txtGrpCustNo.Text = ""
        txtGrpAcType.Text = ""
        txtGrpFullName.Text = ""
        txtGrpTel.Text = ""
        txtGrpMobileNo.Text = ""
        txtGrpEmail.Text = ""
        txtGrpAddress2.Text = ""
        txtGrpAddress1.Text = ""
        txtGrpAddress3.Text = ""
        drpGrpRegion.Text = ""
        drpGrpDistrict.Text = ""
        DrpGrpArea.Text = ""
        txtGrpTinNo.Text = ""
        txtGrpregDate.Text = ""
        txtGrpRegNo.Text = ""
        txtGrpDirectorName.Text = ""
        txtGrpBizNature.Text = ""
        txtGrpMaleNo.Text = ""
        txtGrpFemaleNo.Text = ""
        txtGrpMemberNo.Text = ""
        txtGrpName.Text = ""
        txtGrpAcType.Text = ""
        txtGrpCustNo.Text = ""
    End Sub
    Protected Sub btnDeleteGroup_Click1(sender As Object, e As EventArgs) Handles btnDeleteGroup.Click
        Dim mySqlAction5 As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction5 = "delete from tbCustomer  where C_CustomerNo = '" & Session("C_CustomerNo") & "' "
        MyDBAction.DBAction(mySqlAction5, DataManagement.DBActionType.Delete)
        lblGrpResponse.Text = "Group details have been Updated successfully"
        txtGrpCustNo.Text = ""
        txtGrpAcType.Text = ""
        txtGrpFullName.Text = ""
        txtGrpTel.Text = ""
        txtGrpMobileNo.Text = ""
        txtGrpEmail.Text = ""
        txtGrpAddress2.Text = ""
        txtGrpAddress1.Text = ""
        txtGrpAddress3.Text = ""
        drpGrpRegion.Text = ""
        drpGrpDistrict.Text = ""
        DrpGrpArea.Text = ""
        txtGrpTinNo.Text = ""
        txtGrpregDate.Text = ""
        txtGrpRegNo.Text = ""
        txtGrpDirectorName.Text = ""
        txtGrpBizNature.Text = ""
        txtGrpMaleNo.Text = ""
        txtGrpFemaleNo.Text = ""
        txtGrpMemberNo.Text = ""
        txtGrpName.Text = ""
    End Sub
    Protected Sub btnIndDelete_Click1(sender As Object, e As EventArgs) Handles btnIndDelete.Click
        Dim mySqlAction5 As String = ""
        Dim MyDBAction As New DataManagement
        mySqlAction5 = "delete from tbCustomer  where C_CustomerNo = '" & Session("C_CustomerNo") & "' "
        MyDBAction.DBAction(mySqlAction5, DataManagement.DBActionType.Delete)
        lblResponse.Text = "Customer details have been deleted successfully"
        drpdPrefix.Text = ""
        txtFirstName.Text = ""
        txtMiddleName.Text = ""
        txtLastName.Text = ""
        txtFullName.Text = ""
        txtTel.Text = ""
        txtMobileNo.Text = ""
        txtEmail.Text = ""
        txtAddress1.Text = ""
        txtAddress2.Text = ""
        txtAddress3.Text = ""
        drpRegion.Text = ""
        drpDistrict.Text = ""
        drpArea.Text = ""
        txtTINNo.Text = ""
        txtNationalID.Text = ""
        txtPassportNO.Text = ""
        txtRelationShipOfficer.Text = ""
        drpGender.Text = ""
        txtResidentStatus.Text = ""
        txtDependants.Text = ""
        txtEmployerName.Text = ""
        txtNationality.Text = ""
        txtDoB.Text = ""
        drpMaritalStatus.Text = ""
        txtSpouseName.Text = ""
        txtOccupation.Text = ""
        txtLanguage.Text = ""
    End Sub
    Protected Sub Button1_Click(sender As Object, e As EventArgs) Handles Button6.Click
    End Sub
    Protected Sub Button6_Click(sender As Object, e As EventArgs) Handles Button2.Click
    End Sub
End Class