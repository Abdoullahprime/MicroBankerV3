// src/Microbanker.Web/Models/AccountViewModels.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Microbanker.Web.Models;

// ─── FORM ────────────────────────────────────────────────────────────────────

public class AccountFormModel
{
    public string? EncryptedId { get; set; }

    // Shown, not posted, on edit
    public string? AccountNo { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerLabel { get; set; }

    /// <summary>True when the account is still pending, so product, currency, opening balance and date can change.</summary>
    public bool TermsEditable { get; set; } = true;
    public bool IsEdit { get; set; }

    [Display(Name = "Customer No.")]
    public string? CustomerNo { get; set; }

    [Required(ErrorMessage = "Please select a branch")]
    [Display(Name = "Branch")]
    public Guid? BranchId { get; set; }

    [Required(ErrorMessage = "Please select an account type")]
    [Display(Name = "Account Type")]
    public string AccountType { get; set; } = "SAVINGS";

    [Display(Name = "Product")]
    public Guid? ProductDefinitionId { get; set; }

    [StringLength(200)]
    [Display(Name = "Account Name")]
    public string? AccountName { get; set; }

    [StringLength(3, MinimumLength = 3, ErrorMessage = "Use a 3-letter currency code")]
    public string Currency { get; set; } = "GMD";

    [Range(typeof(decimal), "0", "999999999999999", ErrorMessage = "Opening balance cannot be negative")]
    [Display(Name = "Opening Balance")]
    public decimal OpeningBalance { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Open Date")]
    public DateOnly? OpenDate { get; set; }

    [StringLength(40)]
    [Display(Name = "ATM Account No.")]
    public string? AtmAccountNo { get; set; }

    [Range(typeof(decimal), "0", "999999999999999", ErrorMessage = "ATM limit cannot be negative")]
    [Display(Name = "ATM Daily Limit")]
    public decimal AtmDailyLimit { get; set; }

    [Display(Name = "Sector")]
    public Guid? SectorId { get; set; }

    [Display(Name = "Sub-Sector")]
    public Guid? SubSectorId { get; set; }

    [Display(Name = "Asset Control GL")] public string? GLAssetControl { get; set; }
    [Display(Name = "Liability Control GL")] public string? GLLiabilityControl { get; set; }
    [Display(Name = "Income GL")] public string? GLIncomeAccount { get; set; }
    [Display(Name = "Expense GL")] public string? GLExpenseAccount { get; set; }
    [Display(Name = "Charge GL")] public string? GLChargeAccount { get; set; }
    [Display(Name = "Fee GL")] public string? GLFeeAccount { get; set; }

    // Dropdown data (filled by the controller, never posted)
    public List<SelectListItem> Branches { get; set; } = new();
    public List<SelectListItem> AccountTypes { get; set; } = new();
    public List<SelectListItem> Products { get; set; } = new();
    public List<SelectListItem> Sectors { get; set; } = new();
    public List<LocationOption> SubSectors { get; set; } = new();
    public List<SelectListItem> AssetGLs { get; set; } = new();
    public List<SelectListItem> LiabilityGLs { get; set; } = new();
    public List<SelectListItem> IncomeGLs { get; set; } = new();
    public List<SelectListItem> ExpenseGLs { get; set; } = new();
}

// ─── DETAIL ──────────────────────────────────────────────────────────────────

public class AccountEditRequestViewModel
{
    public string Status { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? RequestedByName { get; set; }
    public DateTime RequestedAt { get; set; }
    public string? ReviewedByName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewRemarks { get; set; }
}

public class AccountDetailViewModel
{
    public string EncryptedId { get; set; } = string.Empty;
    public string AccountNo { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateOnly OpenDate { get; set; }
    public DateTime? ClosedAt { get; set; }

    public string CustomerEncryptedId { get; set; } = string.Empty;
    public string CustomerNo { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;

    public decimal OpeningBalance { get; set; }
    public decimal BookBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal BlockedAmount { get; set; }
    public decimal UnauthorizedDR { get; set; }
    public decimal UnauthorizedCR { get; set; }
    public decimal MonthlyDRTurnover { get; set; }
    public decimal MonthlyCRTurnover { get; set; }
    public decimal InterestAccruedDR { get; set; }
    public decimal InterestAccruedCR { get; set; }
    public DateTime? LastDRDate { get; set; }
    public DateTime? LastCRDate { get; set; }

    public string? AtmAccountNo { get; set; }
    public decimal AtmDailyLimit { get; set; }
    public string? SectorName { get; set; }
    public string? SubSectorName { get; set; }
    public string? GLAssetControl { get; set; }
    public string? GLLiabilityControl { get; set; }
    public string? GLIncomeAccount { get; set; }
    public string? GLExpenseAccount { get; set; }
    public string? GLChargeAccount { get; set; }
    public string? GLFeeAccount { get; set; }

    public bool IsApproved { get; set; }
    public string? CreatedByName { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }

    /// <summary>Latest edit request still relevant (pending or approved and waiting to be used).</summary>
    public AccountEditRequestViewModel? OpenEditRequest { get; set; }
    public bool CanEditNow { get; set; }
}

// ─── LISTS ───────────────────────────────────────────────────────────────────

public class AccountRowViewModel
{
    public string EncryptedId { get; set; } = string.Empty;
    public string AccountNo { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string CustomerNo { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal BookBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class AccountListViewModel
{
    public List<AccountRowViewModel> Rows { get; set; } = new();
    public string? Status { get; set; }
    public string? Type { get; set; }
    public string? Q { get; set; }
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalCount { get; set; }
    public List<SelectListItem> Statuses { get; set; } = new();
    public List<SelectListItem> Types { get; set; } = new();
}

// ─── ENQUIRY / BLOCK / STATEMENT ─────────────────────────────────────────────

public class BalanceEnquiryViewModel
{
    [Display(Name = "Account No.")]
    public string? AccountNo { get; set; }
    public bool Searched { get; set; }
    public AccountDetailViewModel? Account { get; set; }
}

public class BlockAmountViewModel
{
    public string EncryptedId { get; set; } = string.Empty;
    public string AccountNo { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal BookBalance { get; set; }
    public decimal BlockedAmount { get; set; }
    public decimal AvailableBalance { get; set; }
}

public class MiniStatementViewModel
{
    public AccountDetailViewModel Account { get; set; } = new();
}
