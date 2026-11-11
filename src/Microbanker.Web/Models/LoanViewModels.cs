// src/Microbanker.Web/Models/LoanViewModels.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Microbanker.Web.Models;

/// <summary>A product in a dropdown, with the rate used to pre-fill the loan form.</summary>
public class ProductOption
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
}

// ─── LOAN FORM ───────────────────────────────────────────────────────────────

public class LoanFormModel
{
    public string? EncryptedId { get; set; }
    public bool IsEdit { get; set; }
    public string? LoanNo { get; set; }
    public string? CustomerLabel { get; set; }
    public string? CustomerName { get; set; }

    /// <summary>Set when the loan is created from an approved loan request.</summary>
    public Guid? LoanRequestId { get; set; }
    public decimal? RequestedAmount { get; set; }

    [Display(Name = "Customer No.")]
    public string? CustomerNo { get; set; }

    [Required(ErrorMessage = "Please select a branch")]
    [Display(Name = "Branch")]
    public Guid? BranchId { get; set; }

    [Required(ErrorMessage = "Please select a loan product")]
    [Display(Name = "Product")]
    public Guid? ProductDefinitionId { get; set; }

    [Required(ErrorMessage = "Loan name is required"), StringLength(200)]
    [Display(Name = "Loan Name")]
    public string? LoanName { get; set; }

    [StringLength(3, MinimumLength = 3, ErrorMessage = "Use a 3-letter currency code")]
    public string Currency { get; set; } = "GMD";

    [Range(typeof(decimal), "0.01", "999999999999999", ErrorMessage = "Loan amount must be greater than zero")]
    [Display(Name = "Loan Amount")]
    public decimal LoanAmount { get; set; }

    [Range(typeof(decimal), "0", "100", ErrorMessage = "Rate must be between 0 and 100")]
    [Display(Name = "Interest Rate (% p.a.)")]
    public decimal InterestRate { get; set; }

    [Display(Name = "Interest Method")]
    public string InterestCalcMethod { get; set; } = "REDUCING";

    [Range(0, 120, ErrorMessage = "Grace period must be between 0 and 120 months")]
    [Display(Name = "Grace Period (months)")]
    public int GracePeriod { get; set; }

    [Range(1, 600, ErrorMessage = "Tenure must be between 1 and 600 months")]
    [Display(Name = "Tenure (months)")]
    public int Tenure { get; set; } = 12;

    [Range(1, 1000, ErrorMessage = "Installments must be between 1 and 1000")]
    [Display(Name = "No. of Installments")]
    public int NoOfInstallments { get; set; } = 12;

    [Display(Name = "Repayment Frequency")]
    public string RepaymentFrequency { get; set; } = "MONTHLY";

    [Display(Name = "Repayment Mode")]
    public string RepaymentMode { get; set; } = "CASH";

    [DataType(DataType.Date)]
    [Display(Name = "First Installment Date")]
    public DateOnly? FirstEMIDate { get; set; }

    [Range(typeof(decimal), "0", "999999999999999", ErrorMessage = "Charges cannot be negative")]
    public decimal Charges { get; set; }

    [StringLength(500)]
    public string? Collateral { get; set; }

    [StringLength(40)]
    [Display(Name = "Guarantor's Account No.")]
    public string? GuarantorsAccountNo { get; set; }

    [StringLength(40)]
    [Display(Name = "Settlement Account No.")]
    public string? SettlementAccountNo { get; set; }

    [Range(0, 100, ErrorMessage = "Skipped installments must be between 0 and 100")]
    [Display(Name = "Skipped Installments")]
    public int SkipInstallments { get; set; }

    [Display(Name = "Sector")]
    public Guid? SectorId { get; set; }

    [Display(Name = "Sub-Sector")]
    public Guid? SubSectorId { get; set; }

    public List<SelectListItem> Branches { get; set; } = new();
    public List<ProductOption> Products { get; set; } = new();
    public List<SelectListItem> CalcMethods { get; set; } = new();
    public List<SelectListItem> Frequencies { get; set; } = new();
    public List<SelectListItem> Modes { get; set; } = new();
    public List<SelectListItem> Sectors { get; set; } = new();
    public List<LocationOption> SubSectors { get; set; } = new();
}

// ─── DETAIL ──────────────────────────────────────────────────────────────────

public class ScheduleRowViewModel
{
    public int No { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal PrincipalDue { get; set; }
    public decimal InterestDue { get; set; }
    public decimal ChargesDue { get; set; }
    public decimal TotalDue { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal Remaining { get; set; }
    public string Status { get; set; } = "PENDING";
    public bool IsOverdue { get; set; }
}

public class LoanPaymentRowViewModel
{
    public DateOnly PaidOn { get; set; }
    public decimal Amount { get; set; }
    public decimal PrincipalPaid { get; set; }
    public decimal InterestPaid { get; set; }
    public decimal ChargesPaid { get; set; }
    public string Mode { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string? PostedByName { get; set; }
}

public class LoanDetailViewModel
{
    public string EncryptedId { get; set; } = string.Empty;
    public string LoanNo { get; set; } = string.Empty;
    public string LoanName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string CustomerEncryptedId { get; set; } = string.Empty;
    public string CustomerNo { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;

    public decimal LoanAmount { get; set; }
    public decimal DisbursedAmount { get; set; }
    public decimal OutstandingBalance { get; set; }
    public decimal InterestRate { get; set; }
    public string InterestCalcMethod { get; set; } = string.Empty;
    public int GracePeriod { get; set; }
    public int Tenure { get; set; }
    public int NoOfInstallments { get; set; }
    public string RepaymentFrequency { get; set; } = string.Empty;
    public string RepaymentMode { get; set; } = string.Empty;
    public DateOnly? LoanStartDate { get; set; }
    public DateOnly? LoanEndDate { get; set; }
    public DateOnly? FirstEMIDate { get; set; }
    public decimal Charges { get; set; }
    public string? Collateral { get; set; }
    public string? GuarantorsAccountNo { get; set; }
    public string? SettlementAccountNo { get; set; }
    public int SkipInstallments { get; set; }
    public string? SectorName { get; set; }
    public string? SubSectorName { get; set; }

    public bool IsApproved { get; set; }
    public string? CreatedByName { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public bool IsDisbursed { get; set; }
    public string? DisbursedByName { get; set; }
    public DateTime? DisbursedAt { get; set; }

    /// <summary>The real schedule once disbursed, otherwise a preview computed from today.</summary>
    public List<ScheduleRowViewModel> Schedule { get; set; } = new();
    public bool ScheduleIsPreview { get; set; }
    public List<LoanPaymentRowViewModel> Payments { get; set; } = new();

    public decimal TotalPayable { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalRemaining { get; set; }
    public decimal OverdueAmount { get; set; }
    public int OverdueInstallments { get; set; }
}

// ─── LISTS ───────────────────────────────────────────────────────────────────

public class LoanRowViewModel
{
    public string EncryptedId { get; set; } = string.Empty;
    public string LoanNo { get; set; } = string.Empty;
    public string LoanName { get; set; } = string.Empty;
    public string CustomerNo { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal LoanAmount { get; set; }
    public decimal OutstandingBalance { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class LoanListViewModel
{
    public List<LoanRowViewModel> Rows { get; set; } = new();
    public string? Status { get; set; }
    public string? Q { get; set; }
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalCount { get; set; }
    public List<SelectListItem> Statuses { get; set; } = new();
}

// ─── DISBURSE / REPAYMENT / ENQUIRY ──────────────────────────────────────────

public class DisburseViewModel
{
    public LoanDetailViewModel Loan { get; set; } = new();

    [DataType(DataType.Date)]
    [Display(Name = "Disbursement Date")]
    public DateOnly DisbursementDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "First Installment Date")]
    public DateOnly? FirstEmiDate { get; set; }
}

public class RepaymentViewModel
{
    public LoanDetailViewModel Loan { get; set; } = new();

    [Display(Name = "Amount")]
    public decimal Amount { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Payment Date")]
    public DateOnly PaidOn { get; set; }

    public string Mode { get; set; } = "CASH";

    [StringLength(100)]
    public string? Reference { get; set; }

    public List<SelectListItem> Modes { get; set; } = new();
}

public class LoanEnquiryViewModel
{
    [Display(Name = "Loan No.")]
    public string? LoanNo { get; set; }
    public bool Searched { get; set; }
    public LoanDetailViewModel? Loan { get; set; }
}

// ─── LOAN REQUESTS ───────────────────────────────────────────────────────────

public class LoanRequestRowViewModel
{
    public string EncryptedId { get; set; } = string.Empty;
    public string CustomerNo { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public string? Purpose { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ReviewRemarks { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LoanRequestListViewModel
{
    public List<LoanRequestRowViewModel> Rows { get; set; } = new();
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalCount { get; set; }
    public List<SelectListItem> Statuses { get; set; } = new();
}

public class LoanRequestFormModel
{
    [Required(ErrorMessage = "Customer number is required")]
    [Display(Name = "Customer No.")]
    public string? CustomerNo { get; set; }

    [Required(ErrorMessage = "Please select a branch")]
    [Display(Name = "Branch")]
    public Guid? BranchId { get; set; }

    [Required(ErrorMessage = "Please select a loan product")]
    [Display(Name = "Product")]
    public Guid? ProductDefinitionId { get; set; }

    [Range(typeof(decimal), "0.01", "999999999999999", ErrorMessage = "Amount must be greater than zero")]
    [Display(Name = "Requested Amount")]
    public decimal RequestedAmount { get; set; }

    [StringLength(500)]
    public string? Purpose { get; set; }

    public List<SelectListItem> Branches { get; set; } = new();
    public List<SelectListItem> Products { get; set; } = new();
}
