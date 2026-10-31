Imports MicroBankerAdmin.MicroBankerAdmin.MicroBankerAdmin
Imports System.Data.SqlClient
Imports System.Globalization

Public Class LoanRequest
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
                    'txtCustomerNo.Enabled = False
                    'txtBranchCode.Enabled = False
                    'txtAccountCode.Enabled = False
                    'txtAccountNo.Enabled = False
                    'txtBranchName.Enabled = False
                    txtCurrency.Enabled = False
                    'txtProductName.Enabled = False
                    'btnCheckBranch.Visible = False
                    'btnSearchCode.Visible = False
                    'cmasearch.Visible = False
                    ShowAgentDetails("select * from [dbo].[MB_LoanRequest] where AccountNo='" & HiddenField1.Value.ToString & "'  ", "ViewDetails")
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
                            'GetSectors()
                            'GetSubSectors()
                            'GetRegion()
                            'GetArea()
                            'GetDistrict()
                            'txtBranchCode.Text = MyReader("BranchCode").ToString
                            'txtAccountCode.Text = MyReader("AccountCode").ToString
                            txtAccountNos.Text = MyReader("AccountNo").ToString
                            ''Session("AccountNo") = MyReader("AccountNo").ToString
                            'txtAcountName.Text = MyReader("AcountName").ToString
                            'txtBranchName.Text = MyReader("BranchName").ToString
                            'txtCurrency.Text = MyReader("Currency").ToString
                            'drpSector.SelectedValue = MyReader("Sector").ToString
                            'drpSubSector.SelectedValue = MyReader("SubSector").ToString
                            'txtProductName.Text = MyReader("AccountTypeName").ToString
                            'txtAcountName.Text = MyReader("AccountName").ToString
                            'txtCustomerNo.Text = MyReader("CustomerNo").ToString
                            'drpAccountType.SelectedValue = MyReader("AccountType").ToString
                            'drpdPrefix.SelectedValue = MyReader("I_PreFix").ToString
                            'txtFirstName.Text = MyReader("I_FirstName").ToString
                            'txtMiddleName.Text = MyReader("I_MiddleName").ToString
                            'txtLastName.Text = MyReader("I_LastName").ToString
                            'txtTelNo.Text = MyReader("C_Telephone").ToString
                            txtMobileNos.Text = MyReader("MobileNo").ToString
                            'txtEmail.Text = MyReader("C_Email").ToString
                            'txtBookBal.Text = MyReader("C_Address1").ToString
                            'txtBlockedAmount.Text = MyReader("C_Address2").ToString
                            'txtAddress3.Text = MyReader("C_Address3").ToString
                            txtReason.Text = MyReader("Description").ToString
                            txtAccountName.Text = MyReader("AccountName").ToString
                            'txtRelOfficer.Text = MyReader("C_RelationShipOfficer").ToString
                            'drpMaritalStatus.SelectedValue = MyReader("I_MaritalStatus").ToString
                            'txtSpouseName.Text = MyReader("I_SpouseName").ToString
                            ' txtDepemdant.Text = MyReader("I_Dependants").ToString
                            'txtOccupation.Text = MyReader("I_Occupation").ToString
                            'txtEmployer.Text = MyReader("I_EmployerName").ToString
                            'txtBizName.Text = MyReader("B_BusinessName").ToString
                            txtAmount.Text = MyReader("Amount").ToString
                            txtBizRegDate.Text = MyReader("ExpectedDate").ToString
                            txtDoB.Text = MyReader("RequestDate").ToString
                            'txtBizNature.Text = MyReader("B_NatureofBusiness").ToString
                            'txtDirName.Text = MyReader("B_DirectorName").ToString
                            'txtRegNo.Text = MyReader("B_RegName").ToString
                            'txtgrpName.Text = MyReader("G_GroupName").ToString
                            'txtgrpMemberNo.Text = MyReader("G_NoofMembers").ToString
                            'txtMaleNo.Text = MyReader("G_NoofMale").ToString
                            'txtFemaleNo.Text = MyReader("G_NoofFemale").ToString
                            'added 06082021
                            'drpRegion.SelectedValue = MyReader("C_Region").ToString
                            'drpArea.SelectedValue = MyReader("C_Area").ToString
                            'drpDistrict.SelectedValue = MyReader("C_District").ToString
                            'txtSex.Text = MyReader("I_Sex").ToString



                            'txtNationality.Text = MyReader("I_Nationality").ToString
                            'txtLanguage.Text = MyReader("I_Language").ToString
                            'txtDoB.Text = MyReader("DoB").ToString
                            'txtResidential.Text = MyReader("I_ResidentStatus").ToString
                            'txtBizTel.Text = MyReader("B_TelNo").ToString
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
                    SqlAction = "SELECT [id]
      ,[AccountNo]
      ,[CustomerNo]
      ,[AccountName]
      ,[MobileNo]
      ,[Amount]
      ,[RefNo]
      ,[RequestDate]
      ,[ExpectedDate]
      ,[Description]
      ,[RequestStatus]
      ,[DisburseStatus]
      ,[UserName]
  FROM [dbo].[MB_LoanRequest] where RequestStatus <> 1"
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

    Protected Sub btnClear_Click(sender As Object, e As EventArgs) Handles btnClear.Click

    End Sub

    Protected Sub cmddelete_Click(sender As Object, e As EventArgs) Handles cmddelete.Click

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
    Protected Sub btnValidate_Click(sender As Object, e As EventArgs) Handles btnValidate.Click

        Try
            Dim MyDatObject As New DataManagement
            'Dim mySearch5 As String = "select I_Sex,B_DirectorName,B_NatureofBusiness,CustomerNo,AccountType,BranchName,AccountNo,AccountName,Currency,Sector,SubSector,IntRate,DebitNoStatus,BranchCode,AccountPrefix,C_Address1,LoanAmount,LoanStartDate,LoanEndDate,RepaymentPeriod from tbAccount where AccountNo = '100-000025-201-01'" ' ='" & Me.HiddenField1.Value & "' "
            Dim mySearch5 As String = "select * from tbAccount where AccountNo = '" & txtAccountNos.Text.Trim & "'" ' ='" & Me.HiddenField1.Value & "' "
            Dim MyReader5 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch5, DataManagement.DBActionType.SqlDataReader), SqlDataReader)

            If MyReader5.HasRows = True Then
                MyReader5.Read()
                TabContainer2.Visible = True
                btnClear.Visible = False
                btnSave.Visible = True
                'txtCustomerNo.Visible = True
                cmddelete.Visible = False
                General.Visible = True
                GetAccounts(MyReader5("CustomerNo").ToString)
                GetSectors()
                GetSubSectors()
                GetRegion()
                GetArea()
                GetDistrict()
                txtAvailBal.Text = MyReader5("AvailBal").ToString
                If txtAvailBal.Text > txtAmount.Text Then
                    txtBranchCode.Text = MyReader5("BranchCode").ToString
                    txtAccountCode.Text = MyReader5("AccountCode").ToString
                    txtAccountNo.Text = MyReader5("AccountNo").ToString
                    'Session("AccountNo") = MyReader("AccountNo").ToString
                    txtAcountName.Text = MyReader5("AcountName").ToString
                    txtBranchName.Text = MyReader5("BranchName").ToString
                    txtCurrency.Text = MyReader5("Currency").ToString
                    drpSector.SelectedValue = MyReader5("Sector").ToString
                    drpSubSector.SelectedValue = MyReader5("SubSector").ToString
                    txtProductName.Text = MyReader5("AccountTypeName").ToString
                    txtAcountName.Text = MyReader5("AccountName").ToString
                    'txtCustomerNo.Text = MyReader5("CustomerNo").ToString
                    'drpAccountType.SelectedValue = MyReader5("AccountType").ToString
                    drpdPrefix.SelectedValue = MyReader5("I_PreFix").ToString
                    txtFirstName.Text = MyReader5("I_FirstName").ToString
                    txtMiddleName.Text = MyReader5("I_MiddleName").ToString
                    txtLastName.Text = MyReader5("I_LastName").ToString
                    txtBookBal.Text = MyReader5("C_Telephone").ToString
                    'txtMobileNo.Text = MyReader5("C_MobileNo").ToString
                    'txtEmail.Text = MyReader5("C_Email").ToString
                    txtBookBal.Text = MyReader5("BookBalance").ToString

                    txtBlockedAmount.Text = MyReader5("BlockAmount").ToString
                    'txtAddress3.Text = MyReader5("C_Address3").ToString
                    'txtReason.Text = MyReader5("C_TINNo").ToString
                    'txtAccountName.Text = MyReader5("I_NationalID").ToString
                    txtRelOfficer.Text = MyReader5("C_RelationShipOfficer").ToString
                    drpMaritalStatus.SelectedValue = MyReader5("I_MaritalStatus").ToString
                    txtSpouseName.Text = MyReader5("I_SpouseName").ToString
                    txtDepemdant.Text = MyReader5("I_Dependants").ToString
                    txtOccupation.Text = MyReader5("I_Occupation").ToString
                    ''txtEmployer.Text = MyReader5("I_EmployerName").ToString
                    txtBizName.Text = MyReader5("B_BusinessName").ToString
                    'txtBizRegDate.Text = MyReader("RegDate").ToString
                    txtBizNature.Text = MyReader5("B_NatureofBusiness").ToString
                    txtDirName.Text = MyReader5("B_DirectorName").ToString
                    txtRegNo.Text = MyReader5("B_RegName").ToString
                    txtgrpName.Text = MyReader5("G_GroupName").ToString
                    txtgrpMemberNo.Text = MyReader5("G_NoofMembers").ToString
                    txtMaleNo.Text = MyReader5("G_NoofMale").ToString
                    txtFemaleNo.Text = MyReader5("G_NoofFemale").ToString
                    'added 06082021
                    drpRegion.SelectedValue = MyReader5("C_Region").ToString
                    drpArea.SelectedValue = MyReader5("C_Area").ToString
                    drpDistrict.SelectedValue = MyReader5("C_District").ToString
                    txtSex.Text = MyReader5("I_Sex").ToString
                    'txtAmount.Text = MyReader5("I_PassportNo").ToString
                    txtNationality.Text = MyReader5("I_Nationality").ToString
                    txtLanguage.Text = MyReader5("I_Language").ToString
                    'txtDoB.Text = MyReader5("I_DoB").ToString
                    txtResidential.Text = MyReader5("I_ResidentStatus").ToString
                    txtBizTel.Text = MyReader5("B_TelNo")
                Else
                    lblResponse.Text = "The accont have insuffient funds"
                End If
                '
                'txtRelOfficers.Text = MyReader5("C_RelationShipOfficer")
                'Post(txtCustomerNo.Text.Trim)
            Else
                lblResponse.Text = "Loan request details does not exist in the core"
            End If
        Catch ex As Exception

        End Try
        'Dim myaccountno As String = ""
        'myaccountno = Session("AccountNo")
        'Dim trandate As String = Date.Today.ToString("dd-MMM-yy")
        'Dim MyDBAction As New DataManagement
        'Dim MySqlaction1 As String
        'MySqlaction1 = "update tbAccount set  Sector ='" & drpSector.SelectedValue.ToUpper.Trim & "', SubSector ='" & drpSubSector.SelectedValue.ToUpper.Trim & "', AccountType ='" & drpAccountType.Text.ToUpper.Trim & "', C_Email = '" & txtEmail.Text.Trim & "', C_TINNo = '" & txtReason.Text.ToUpper.Trim & "',C_MobileNo = '" & txtMobileNo.Text.ToUpper.Trim & "',I_FirstName = '" & txtFirstName.Text.ToUpper.Trim & "',I_MiddleName = '" & txtMiddleName.Text.ToUpper.Trim & "',I_LastName = '" & txtLastName.Text.ToUpper.Trim & "',I_NationalID = '" & txtAccountName.Text.ToUpper.Trim & "',C_RelationShipOfficer = '" & txtRelOfficer.Text.ToUpper.Trim & "',I_MaritalStatus = '" & drpMaritalStatus.Text.ToUpper.Trim & "',I_SpouseName = '" & txtSpouseName.Text.ToUpper.Trim & "',I_Dependants = '" & txtDepemdant.Text.ToUpper.Trim & "',I_Occupation = '" & txtOccupation.Text.ToUpper.Trim & "',I_EmployerName = '" & txtEmployer.Text.ToUpper.Trim & "',B_BusinessName = '" & txtBizName.Text.ToUpper.Trim & "',B_RegDate = '" & txtBizRegDate.Text.ToUpper.Trim & "',B_RegName = '" & txtRegNo.Text.ToUpper.Trim & "',B_NatureofBusiness = '" & txtBizNature.Text.ToUpper.Trim & "',B_DirectorName = '" & txtDirName.Text.ToUpper.Trim & "',G_GroupName = '" & txtgrpName.Text.ToUpper.Trim & "',G_NoofMembers = '" & txtgrpMemberNo.Text.ToUpper.Trim & "',G_NoofMale = '" & txtMaleNo.Text.ToUpper.Trim & "',G_NoofFemale = '" & txtFemaleNo.Text.ToUpper.Trim & "',C_Region = '" & drpRegion.SelectedValue.ToUpper.Trim & "' ,C_District = '" & drpDistrict.SelectedValue.ToUpper.Trim & "',C_Area = '" & drpArea.SelectedValue.ToUpper.Trim & "',I_ResidentStatus ='" & txtResidential.Text.ToUpper.Trim & "',I_Sex = '" & txtSex.Text.ToUpper.Trim & "',I_PassportNO = '" & txtAmount.Text.ToUpper.Trim & "',I_Nationality = '" & txtNationality.Text.ToUpper.Trim & "',I_DoB = '" & txtDoB.Text.ToUpper.Trim & "',I_Language = '" & txtLanguage.Text.ToUpper.Trim & "' where AccountNo ='" & myaccountno & "'"
        'MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)

    End Sub
    'Private Sub Post(custNo)

    '    Dim dtCurrentTable As DataTable = DirectCast(ViewState("CurrentTable"), DataTable)
    '    Dim dt As DataTable = New DataTable()

    '    ' Dim rowCount As Integer = dtCurrentTable.Rows.Count()
    '    'Dim rowCount As Integer = dtCurrentTable.Rows.Count()
    '    Dim ProcessGroup As String = "EOD"
    '    Dim MyDatObject As New DataManagement
    '    'Dim mySearch4 As String = "select idx,ProcessID,ProcessName,ProcessFunction, ProcessGroup,ExecutionOrder from MB_ProcessRoutine where ProcessGroup = '" & ProcessGroup & "'"
    '    'Dim mySearch4 As String = "select AccountNo from tbAccount where CustomerNo = '" & txtCustomerNo.Text.Trim & "'"
    '    Dim mySearch4 As String = "select AccountNo,AccountName,BranchCode from tbAccount where CustomerNo = '" & txtCustomerNo.Text.Trim & "'"
    '    'Dim MyReader4 As SqlDataReader = MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataTable)
    '    Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataTable), SqlDataReader)

    '    Dim MyDatable As DataTable = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.DataTable), Data.DataTable)
    '    SODGridView.DataSource = MyDatable
    '    SODGridView.DataBind()

    'End Sub
    Protected Sub btnProcess_Click(sender As Object, e As EventArgs)
        Dim lb As LinkButton = DirectCast(sender, LinkButton)
        Dim paramList As List(Of SqlParameter) = New List(Of SqlParameter)
        'paramList.Add(New SqlParameter("@EodDate", "30-oct-2021"))
        'Dim _Date As Date = Convert.ToDateTime(txtEOM.Text).ToString("dd/MM/yyyy")
        Dim Records As DataTable
        'Dim s As String = txtDate.Text.Trim
        Dim pattern As String = "dd/MM/yyyy"
        'Dim _Date As Date = Date.ParseExact(s, pattern, Nothing)

        ' paramList.Add(New SqlParameter("@EodDate", _Date))


        Dim b As LinkButton = CType(sender, LinkButton)
        Dim rows As GridViewRow = CType(b.NamingContainer, GridViewRow)

        Dim selectCheck As CheckBox = CType(rows.FindControl("checkSelect"), CheckBox)
        Dim ProcessName As String = CType(rows.FindControl("ProcessName"), Label).Text
        Dim ExecutionOrder As String = CType(rows.FindControl("ExecutionOrder"), Label).Text
        Dim ProcessFunction As String = CType(rows.FindControl("ProcessFunction"), Label).Text

        If selectCheck.Checked = True Then
            If (ExecutionOrder.Trim() = "1") Then
                'Records = GetReport(ProcessFunction, paramList)
                'If Records.Rows.Count > 0 Then
                '    rows.FindControl("success").Visible = True
                '    CType(rows.FindControl("lblStatus"), Label).Text = "Success"
                '    ProcessStatus.Add(True)

                'Else
                '    rows.FindControl("fail").Visible = True
                '    ProcessStatus.Add(False)

                'End If
            ElseIf (ExecutionOrder.Trim() = "2") Then
                'Records = GetReport(ProcessFunction)
                'If Records.Rows.Count > 0 Then
                '    rows.FindControl("success").Visible = True
                '    CType(rows.FindControl("lblStatus"), Label).Text = "Success"
                '    ProcessStatus.Add(True)

                'Else
                '    rows.FindControl("fail").Visible = True
                '    CType(rows.FindControl("lblStatus"), Label).Text = "Failed"
                '    'ProcessStatus.Add(False)

                'End If
            End If
        End If



        'If Records.Rows.Count > 0 Then
        '    Me.lblSuccessMsgEOD.Text = "End of day: Successful"
        'End If
    End Sub

    Private Sub GetAccounts(CustomerNo As String)
        'Load Products
        Try
            Dim MyDataTable As New Data.DataTable
            Dim MyDBAction As New DataManagement

            Dim SqlAction As String = ""
            SqlAction = "select AccountNo,AccountName,BranchName,C_Address1  from tbAccount where AccountNo = '" & CustomerNo & "'"
            MyDataTable = TryCast(MyDBAction.DBAction(SqlAction, DataManagement.DBActionType.DataTable), Data.DataTable)
            Session("searchtable") = MyDataTable
            GridView1.DataSource = MyDataTable
            GridView1.DataBind()
        Catch ex As Exception
        End Try
    End Sub

    Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        ''InsertUser()
        AppoveLoanRequest()
        UpdateAccount()
        DisplayDetails("All")
        lblResponse.Text = "Loan approved successfully"
        'txtCustomerNo.Enabled = False
        txtCurrency.Enabled = False
        'txtProductName.Enabled = False
        'btnCheckBranch.Visible = False
        'btnSearchCode.Visible = False
        'cmasearch.Visible = False
        ''txtCustomerNo.ReadOnly = False
        'txtCustomerNo.Text = ""
        'txtAccountCode.Text = ""
        txtAccountNos.Text = ""
        HiddenField1.Value = ""
        txtCurrency.Text = ""
        drpSector.SelectedIndex = -1
        drpSubSector.SelectedIndex = -1
        ''txtCustomerNo.Text = ""
        'txtBranchCode.Text = ""
        'txtCustomerNo.Text = ""
        'drpAccountType.Text = ""
        txtFirstName.Text = ""
        drpdPrefix.Text = ""
        txtFirstName.Text = ""
        txtMiddleName.Text = ""
        'txtFreezeStatus.Text = ""
        'txtFullName.Text = ""
        txtMobileNos.Text = ""
        'txtEmail.Text = ""
        txtReason.Text = ""
        'txtNationality.Text = ""
        txtRelOfficer.Text = ""
        drpMaritalStatus.Text = ""
        txtSpouseName.Text = ""
        txtDepemdant.Text = ""
        txtOccupation.Text = ""
        'txtEmployer.Text = ""
        txtBizName.Text = ""
        'txtLastCRactvtyDate.Text = ""
        'txtgrpDirName.Text = ""
        txtgrpMemberNo.Text = ""
        txtMaleNo.Text = ""
        'txtBranchCode.Text = ""
        txtFemaleNo.Text = ""
        'txtBranchName.Text = ""
        'txtProductName.Text = ""
        txtLastName.Text = ""
        txtAccountName.Text = ""
        txtBizRegDate.Text = ""
        txtRegNo.Text = ""
        txtBizNature.Text = ""
        txtDirName.Text = ""
        txtgrpName.Text = ""
        drpArea.Text = ""
        drpDistrict.Text = ""
        drpRegion.Text = ""
        txtSex.Text = ""
        txtAmount.Text = ""
        txtNationality.Text = ""
        txtDoB.Text = ""
        txtLanguage.Text = ""
        txtResidential.Text = ""
        txtBizTel.Text = ""
    End Sub
    'Private Sub InsertUser()
    '    Dim mySqlAction As String = ""
    '    Dim MyDBAction As New DataManagement
    '    mySqlAction = "INSERT INTO MB_MobileUsers (UserName,LastLogin,Password,isLoggedIn,Active,CustomerNo,MobileNo) values ('" & txtCustomerNo.Text.Trim & "','" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "','12345','1','1','" & txtCustomerNo.Text.Trim & "','" & txtMobileNos.Text.Trim & "')"
    '    MyDBAction.DBAction(mySqlAction, DataManagement.DBActionType.Insert)
    'End Sub
    Protected Sub AppoveLoanRequest()
        Dim MySqlaction1 As String
        Dim MyDBAction As New DataManagement
        MySqlaction1 = "update MB_LoanRequest set RequestStatus ='1',RequestStatusDesc = 'APPROVED', ApprovedDate = '" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "', ApprovedBy= '" & Session("UserName") & "'  where AccountNo ='" & txtAccountNos.Text.Trim & "'"
        MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
    End Sub
    Protected Sub UpdateAccount()
        ''AccessMobileApp
        'If CType(SODGridView.Rows(0).FindControl("checkSelect"), CheckBox).Checked Then
        '    Dim test As String
        '    test = "Sparta"
        '    'throw the message

        'End If
        'Dim selectCheck As CheckBox = CType(SODGridView.FindControl("checkSelect"), CheckBox)
        'If selectCheck.Checked = True Then
        '    Dim test As String
        '    test = "Sparta"

        'End If
        'Dim selectCheck As CheckBox = CType(rows.FindControl("checkSelect"), CheckBox)
        'Dim MySqlaction1 As String
        'Dim MyDBAction As New DataManagement
        'MySqlaction1 = "update tbAccount set AccessMobileApp ='1', AccessMobileAppDate = '" & DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") & "',AccessMobileAppBy = 'WAITARA' where CustomerNo ='" & txtCustomerNo.Text.Trim & "'"
        'MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
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
        'txtBookBal.Text = ""
        'txtBlockedAmount.Text = ""
        'txtAddress3.Text = ""
        'txtRegion.Text = ""
        'txtDistrict.Text = ""
        'txtArea.Text = ""
        'txtTINNo.Text = ""
        'txtNationalID.Text = ""
        'txtAmountNO.Text = ""
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
    'Protected Sub txtAccountCode_TextChanged(sender As Object, e As EventArgs) Handles txtCustomerNo.TextChanged

    'End Sub
    'Protected Sub btnCheckBranch_Click(sender As Object, e As EventArgs) Handles btnCheckBranch.Click
    '    Try
    '        Dim MyDatObject As New DataManagement
    '        'Dim mySearch4 As String = "select BranchName from Branches where BranchCode = '" & txtBranchCode.Text & "'" ' ='" & Me.HiddenField1.Value & "' "
    '        Dim MyReader4 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch4, DataManagement.DBActionType.SqlDataReader), SqlDataReader)

    '        If MyReader4.HasRows = True Then
    '            MyReader4.Read()
    '            txtBranchName.Text = MyReader4("BranchName").ToString
    '            'txtAccountName.Text = MyReader("AccountName").ToString
    '            'txtFullName.Text = MyReader("DormancyDays").ToString

    '        End If
    '        ' Dim MyDatObject As New DataManagement
    '        'Dim mySearch5 As String = "select IntRate,LiabilityGL,AssetGL from [dbo].[tbSavingsAccountProduct] where AccountCode = '" & drpAccountType.Text & "'" ' ='" & Me.HiddenField1.Value & "' "
    '        'Dim MyReader5 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch5, DataManagement.DBActionType.SqlDataReader), SqlDataReader)

    '        'If MyReader5.HasRows = True Then
    '        '    MyReader5.Read()
    '        '    drpdPrefix.Text = MyReader5("IntRate").ToString
    '        '    txtCustomerNo.Text = MyReader5("LiabilityGL").ToString
    '        '    drpAccountType.Text = MyReader5("AssetGL").ToString

    '        'End If
    '        'Dim MyService As New checkBfub.BFUBEnquiries
    '        'Dim a As String = MyService.AccountBalanceEnq(txtOrgCode.Text).branchName
    '        'Dim b As String = MyService.AccountBalanceEnq(txtOrgCode.Text).branchSortCode
    '        'Dim c As String = MyService.AccountBalanceEnq(txtsuspenceacc.Text).branchName
    '        'txtOrgName.Text = a

    '        ''txtAdress1.Text = b

    '        'b = b.Substring(5, 3)
    '        'txtAdress1.Text = b
    '        'txtOrgCode.ReadOnly = True

    '        'b = "099"
    '        'txtAdress1.Text = b
    '        'txtOrgCode.ReadOnly = True

    '        'If b = "" Then
    '        '    Label4.Text = "The Commission A/c does not exist "
    '        '    txtOrgCode.ReadOnly = False
    '        'End If
    '        'DropDownList1.Text = c
    '        MyReader4.Close()
    '    Catch ex As Exception
    '        'txtOrgName.Text = ""
    '        'txtAdress1.Text = ""
    '        '' DropDownList1.Text = ""
    '        'txtOrgCode.ReadOnly = False
    '        'Label4.Text = "Error Pulling Account Details, Commission A/c must start 01M8 .... E6 "
    '    End Try
    'End Sub
    'Protected Sub btnSearchCode_Click(sender As Object, e As EventArgs) Handles btnSearchCode.Click
    '    getProductCodesforTxt()
    'End Sub
    Protected Sub getProductCodesforTxt()
        'txtOrgName.Text = ""
        'Label4.Text = ""
        Try
            Dim MyDatObject As New DataManagement
            'Dim mySearch5 As String = "select AccountCode,AccountPrefix,AccountName,DormancyDays,AssetGL,LiabilityGL,IncomeGL,ExpenseGL,ChargeGL,FeeGL,IntRate,IntFreq,IntAccrualFreq,MinInterestBal,MinAccountBal,DebitsNoPerMonth,Amount from [dbo].[tbSavingsAccountProduct] where AccountCode = '" & txtAccountCode.Text & "'" ' ='" & Me.HiddenField1.Value & "' "
            'Dim MyReader5 As SqlDataReader = TryCast(MyDatObject.DBAction(mySearch5, DataManagement.DBActionType.SqlDataReader), SqlDataReader)

            'If MyReader5.HasRows = True Then
            '    MyReader5.Read()
            '    'txtProductName.Text = MyReader5("AccountPrefix").ToString
            '    AccountCode = mySearch5("AccountCode").ToString
            '    'txtCustomerNo.Text = MyReader5("AssetGL").ToString
            '    'drpAccountType.Text = MyReader5("LiabilityGL").ToString
            '    'drpdPrefix.Text = MyReader5("IntRate").ToString
            '    'txtFemaleNo.Text = MyReader5("AccountPrefix").ToString

            'Else
            'update
            Dim MySqlaction1 As String
            Dim mySqlAction As String = ""
            Dim MyCurrencyName As String = ""
            Dim MyDBAction As New DataManagement
            'MySqlaction1 = "update tbAccount set CustomerNo ='" & txtCustomerNo.Text.ToUpper.Trim & "', AccountType ='" & txtAccountCode.Text.ToUpper.Trim & "', BranchName ='" & txtBranchCode.Text.ToUpper.Trim & "', AccountNo ='" & txtAccountNo.Text.ToUpper.Trim & "',  AcountName ='" & txtAcountName.Text.ToUpper.Trim & "',Currency ='" & txtCurrency.Text.ToUpper.Trim & "' ,Sector = '" & txtSector.Text.ToUpper.Trim & "', SubSector = '" & txtSubSector.Text.ToUpper.Trim & "', DRGL = '" & txtCustomerNo.Text.ToUpper.Trim & "', CRGL = '" & drpAccountType.Text.ToUpper.Trim & "',IntRate = '" & drpdPrefix.Text.ToUpper.Trim & "',DebitNoStatus = '" & txtFirstName.Text.ToUpper.Trim & "' where CustomerNo ='" & txtCustomerNo.Text & "'"
            MyDBAction.DBAction(MySqlaction1, DataManagement.DBActionType.Update)
            DisplayDetails("All")
            'Label7.Text = "Saving Account details have been Updated successfully"

            'txtCustomerNo.ReadOnly = False


            'DisplayDetails("All")
            'AUDIT TRAIL-------
            'Dim audit As New DataManagement
            'audit.AddAdminAuditTrail(Session("UserName").ToString, "Action", "Successful Saving of branch  '" & txtOrgName.Text.ToUpper & "'", _
            'Request.Url.ToString, Session("ip").ToString, Session("sessionid").ToString)
            ''----END AUDIT TRAIL-----


            'End If
            'MyReader5.Close()
        Catch ex As Exception

        End Try
    End Sub
    'Protected Sub cmasearch_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmasearch.Click
    '    Dim mystring As String = ""
    '    'mystring = txtOrgCode.Text.ToUpper()
    '    'If Mid(mystring, 1, 4).Contains("01M8") = True Then
    '    '    Label4.Text = ""
    '    'getProductCodes()
    '    'getAcc()
    '    'createAccountNumber(txtBranchCode.Text, txtCustomerNo.Text, txtProductCode.Text)

    '    '    Exit Sub
    '    'Else
    '    ' Label4.Text = "This is not the correct format of a commission account"
    '    'End If
    'End Sub
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