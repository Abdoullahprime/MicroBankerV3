// src/Microbanker.Domain/Security/Modules.cs
namespace Microbanker.Domain.Security;

/// <summary>
/// Permission module codes. Each one is a row in AppModules (seeded by DbSeeder) and is what
/// [RequirePermission] checks. A test keeps this list, the seeder and the controllers in step:
/// when you add a secured feature, add its constant here, its seed row, and put the attribute on the controller.
/// </summary>
public static class Modules
{
    public const string Dashboard          = "DASHBOARD";

    // Setup
    public const string Organization       = "ORGANIZATION";
    public const string Branches           = "BRANCHES";
    public const string Departments        = "DEPARTMENTS";
    public const string Units              = "UNITS";
    public const string Banks              = "BANKS";
    public const string Regions            = "REGIONS";
    public const string Districts          = "DISTRICTS";
    public const string Areas              = "AREAS";
    public const string Sectors            = "SECTORS";
    public const string SubSectors         = "SUB_SECTORS";
    public const string ProductTypes       = "PRODUCT_TYPES";
    public const string ProductDefinitions = "PRODUCT_DEFINITIONS";
    public const string TransactionCodes   = "TRANSACTION_CODES";
    public const string ChartOfAccounts    = "CHART_OF_ACCOUNTS";

    // Banking, lending, teller
    public const string Customers          = "CUSTOMERS";
    public const string SelfReg            = "SELF_REG";
    public const string Accounts           = "ACCOUNTS";
    public const string BalanceEnquiry     = "BAL_ENQUIRY";
    public const string Loans              = "LOANS";
    public const string LoanRequests       = "LOAN_REQ";
    public const string Teller             = "TELLER";
    public const string Transactions       = "TRANS_VIEW";

    // Reports
    public const string GeneralLedger      = "GL";
    public const string Processing         = "PROCESSING";
    public const string Reports            = "REPORTS";

    // Administration
    public const string Users              = "USERS";
    public const string Roles              = "ROLES";
    public const string Permissions        = "PERMISSIONS";
}

/// <summary>Permission action codes (the columns of the permission matrix).</summary>
public static class PermissionActions
{
    public const string View    = "VIEW";
    public const string Create  = "CREATE";
    public const string Edit    = "EDIT";
    public const string Delete  = "DELETE";
    public const string Approve = "APPROVE";
    public const string Export  = "EXPORT";
}

/// <summary>Code of the built-in role that bypasses all permission checks.</summary>
public static class SystemRoles
{
    public const string SuperAdmin = "SUPER_ADMIN";
}
