Imports System.ComponentModel.DataAnnotations

Public Class Models




    Public Class Account
        '<StringLength(255)>
        'Public Property id As Integer 
        Public Property CustomerNo As Integer
        Public Property AccountType As String
        Public Property AccountTypeCode As Integer
        Public Property BranchName As String
        Public Property AccountNo As String
        Public Property AcountName As String
        Public Property Currency As String
        Public Property Sector As String
        Public Property SubSector As String
        Public Property IntRate As Decimal
        Public Property DebitNoStatus As String
        Public Property CreditNoStatus As String
        Public Property FreezeStatus As String
        Public Property DormancyDays As Integer
        Public Property DormancyStatus As String
        Public Property OpeningBal As Decimal
        Public Property BlockAmount As Decimal
        Public Property UnathorizedDR As Decimal
        Public Property UnauthorizedCR As Decimal
        Public Property BookBalance As Decimal
        Public Property AvailBal As Decimal
        Public Property MnthDRTrnver As Decimal
        Public Property MnthCRTrnver As Decimal
        Public Property IntAccruedDR As Decimal
        Public Property IntAccruedCR As Decimal
        Public Property LastDRDate As DateTime
        Public Property LastCRDate As DateTime
        Public Property LastCRactvtyDate As DateTime
        Public Property LastDRactvtyDate As DateTime
        Public Property ATMaccNo As String
        Public Property ATMdailyLimit As Decimal
        Public Property AccountStatus As String
        Public Property AccountCode As String
        Public Property AccountPrefix As Integer
        Public Property ProductDefCode As String
        Public Property AccountTypeName As String
        Public Property AccountName As String
        Public Property GLType As String
        Public Property AssetGL_Control As String
        Public Property LiabilityGL_Control As String
        Public Property IncomeGL As String
        Public Property ExpenseGL As String
        Public Property ChargeGL As String
        Public Property FeeGL As String
        Public Property IntFreq As Integer
        Public Property IntAccrualFreq As Integer
        Public Property MinInterestBal As Decimal
        Public Property MinAccountBal As Decimal
        Public Property DebitsNoPerMonth As Integer
        Public Property BranchCode As String
        Public Property C_CustomerNo As Integer
        Public Property I_PreFix As String
        Public Property I_FirstName As String
        Public Property I_MiddleName As String
        Public Property I_LastName As String
        Public Property C_FullName As String
        Public Property C_Telephone As String
        Public Property C_MobileNo As String
        Public Property C_Email As String
        Public Property C_Address1 As String
        Public Property C_Address2 As String
        Public Property C_Address3 As String
        Public Property C_Region As String
        Public Property C_District As String
        Public Property C_Area As String
        Public Property C_TINNo As String
        Public Property I_NationalID As String
        Public Property I_PassportNO As String
        Public Property C_RelationShipOfficer As String
        Public Property I_Nationality As String
        Public Property I_DoB As Date
        Public Property I_ResidentStatus As String
        Public Property I_Language As String
        Public Property I_Sex As String
        Public Property I_MaritalStatus As String
        Public Property I_SpouseName As String
        Public Property I_Dependants As Integer
        Public Property I_Occupation As String
        Public Property I_EmployerName As String
        Public Property B_BusinessName As String
        Public Property B_RegDate As Date
        Public Property B_RegName As String
        Public Property B_TelNo As String
        Public Property B_NatureofBusiness As String
        Public Property B_DirectorName As String
        Public Property G_GroupName As String
        Public Property G_NoofMembers As Integer
        Public Property G_NoofMale As Integer
        Public Property G_NoofFemale As Integer
        Public Property Approved As Integer
        Public Property AccEditReason As String
        Public Property AccApproveRmrks As String
        Public Property LoanAmount As Decimal
        Public Property GracePeriod As Integer
        Public Property LoanStartDate As Date
        Public Property LoanEndDate As Date
        Public Property RepaymentFrequency As String
        Public Property NoInstallments As Integer
        Public Property Tenure As Integer
        Public Property Charges As Decimal
        Public Property InterestCalc As String
        Public Property FirstEMI As Date
        Public Property Collateral As String
        Public Property RepaymentMode As String
        Public Property RepaymentDate As Date
        Public Property RepaymentPeriod As Integer
        Public Property SkipInstallments As Integer
        Public Property SettlementsAccountNo As String
        Public Property GuarantorsAccountNo As String

    End Class


    Enum LoanAccountTypes
        SettlementAccount = 1
        GaurantorsAccount = 2
    End Enum


End Class
