// src/Microbanker.Web/Models/TellerViewModels.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Microbanker.Web.Models;

// ─── DEPOSIT / WITHDRAWAL ────────────────────────────────────────────────────

public class CashPostingFormModel
{
    /// <summary>"Deposit" or "Withdrawal"; set by the controller.</summary>
    public string Kind { get; set; } = "Deposit";

    [Required(ErrorMessage = "Account number is required")]
    [Display(Name = "Account No.")]
    public string? AccountNo { get; set; }

    [Required(ErrorMessage = "Please select a transaction code")]
    [Display(Name = "Transaction Code")]
    public Guid? TransactionCodeId { get; set; }

    [Range(typeof(decimal), "0.01", "999999999999999", ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Narration is required"), StringLength(250)]
    public string? Narration { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Value Date")]
    public DateOnly? ValueDate { get; set; }

    [Required(ErrorMessage = "Please select the cash account")]
    [Display(Name = "Cash Account (GL)")]
    public string? CashGlAccountNo { get; set; }

    [StringLength(50)]
    [Display(Name = "Teller / Slip Ref.")]
    public string? TellerRef { get; set; }

    public List<SelectListItem> Codes { get; set; } = new();
    public List<SelectListItem> CashGLs { get; set; } = new();

    /// <summary>Withdrawal only: approved requests the teller can pay out now.</summary>
    public List<WithdrawalRequestRowViewModel> ReadyToPay { get; set; } = new();
}

// ─── RETAIL (TRANSFER) ───────────────────────────────────────────────────────

public class RetailPostingFormModel
{
    [Required(ErrorMessage = "Debit account is required")]
    [Display(Name = "From Account (debit)")]
    public string? FromAccountNo { get; set; }

    [Required(ErrorMessage = "Credit account is required")]
    [Display(Name = "To Account (credit)")]
    public string? ToAccountNo { get; set; }

    [Required(ErrorMessage = "Please select a debit code")]
    [Display(Name = "Debit Code")]
    public Guid? DebitCodeId { get; set; }

    [Required(ErrorMessage = "Please select a credit code")]
    [Display(Name = "Credit Code")]
    public Guid? CreditCodeId { get; set; }

    [Range(typeof(decimal), "0.01", "999999999999999", ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Narration is required"), StringLength(250)]
    public string? Narration { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Value Date")]
    public DateOnly? ValueDate { get; set; }

    [StringLength(50)]
    [Display(Name = "Teller / Slip Ref.")]
    public string? TellerRef { get; set; }

    public List<SelectListItem> DebitCodes { get; set; } = new();
    public List<SelectListItem> CreditCodes { get; set; } = new();
}

// ─── MIXED POSTING ───────────────────────────────────────────────────────────

public class PostingLineModel
{
    public string? AccountNo { get; set; }
    public string DrCr { get; set; } = "DR";
    public decimal Amount { get; set; }
    [StringLength(250)]
    public string? Narration { get; set; }
}

public class MixedPostingFormModel
{
    public List<PostingLineModel> Lines { get; set; } = new();

    [Display(Name = "Debit Code")]
    public Guid? DebitCodeId { get; set; }

    [Display(Name = "Credit Code")]
    public Guid? CreditCodeId { get; set; }

    [Display(Name = "Cash / Contra Account (GL)")]
    public string? ContraGlAccountNo { get; set; }

    [StringLength(250)]
    [Display(Name = "Default Narration")]
    public string? Narration { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Value Date")]
    public DateOnly? ValueDate { get; set; }

    [StringLength(50)]
    [Display(Name = "Teller / Slip Ref.")]
    public string? TellerRef { get; set; }

    public List<SelectListItem> DebitCodes { get; set; } = new();
    public List<SelectListItem> CreditCodes { get; set; } = new();
    public List<SelectListItem> ContraGLs { get; set; } = new();
}

// ─── BULK UPLOAD ─────────────────────────────────────────────────────────────

public class BulkUploadFormModel
{
    [Display(Name = "File (CSV)")]
    public IFormFile? File { get; set; }

    [Display(Name = "Debit Code")]
    public Guid? DebitCodeId { get; set; }

    [Display(Name = "Credit Code")]
    public Guid? CreditCodeId { get; set; }

    [Display(Name = "Cash / Contra Account (GL)")]
    public string? ContraGlAccountNo { get; set; }

    [StringLength(250)]
    [Display(Name = "Default Narration")]
    public string? Narration { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Value Date")]
    public DateOnly? ValueDate { get; set; }

    [StringLength(50)]
    [Display(Name = "Teller / Slip Ref.")]
    public string? TellerRef { get; set; }

    /// <summary>Row problems found in the file; nothing is posted while there are any.</summary>
    public List<string> FileErrors { get; set; } = new();

    public List<SelectListItem> DebitCodes { get; set; } = new();
    public List<SelectListItem> CreditCodes { get; set; } = new();
    public List<SelectListItem> ContraGLs { get; set; } = new();
}

// ─── WITHDRAWAL REQUESTS ─────────────────────────────────────────────────────

public class WithdrawalRequestRowViewModel
{
    public string EncryptedId { get; set; } = string.Empty;
    public string AccountNo { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CodeLabel { get; set; } = string.Empty;
    public string? Purpose { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RequestedByName { get; set; }
    public string? ReviewedByName { get; set; }
    public string? ReviewRemarks { get; set; }
    public string? CompletedTransactionRef { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class WithdrawalRequestListViewModel
{
    public List<WithdrawalRequestRowViewModel> Rows { get; set; } = new();
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalCount { get; set; }
    public List<SelectListItem> Statuses { get; set; } = new();
    public List<SelectListItem> CashGLs { get; set; } = new();
}

// ─── TRANSACTIONS ────────────────────────────────────────────────────────────

public class TransactionRowViewModel
{
    public string EncryptedId { get; set; } = string.Empty;
    public string TransactionRef { get; set; } = string.Empty;
    public DateOnly ValueDate { get; set; }
    public DateTime PostingDate { get; set; }
    public string AccountNo { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string DrCr { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsReversal { get; set; }
    public string Narration { get; set; } = string.Empty;
    public decimal BalanceAfter { get; set; }
}

public class TransactionListViewModel
{
    public List<TransactionRowViewModel> Rows { get; set; } = new();
    public string? Q { get; set; }
    public string? Status { get; set; }
    public string? DrCr { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalCount { get; set; }
    public decimal TotalDebits { get; set; }
    public decimal TotalCredits { get; set; }
    public List<SelectListItem> Statuses { get; set; } = new();
}

public class TransactionDetailViewModel
{
    public TransactionRowViewModel Txn { get; set; } = new();
    public string CodeLabel { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string? AccountEncryptedId { get; set; }
    public string? TellerRef { get; set; }
    public string? BatchRef { get; set; }
    public string? GLAccountNo { get; set; }
    public string? GLAccountName { get; set; }
    public string? PostedByName { get; set; }
    public string? ReversalRef { get; set; }
    public string? ReversalEncryptedId { get; set; }
    public string? ReversedByName { get; set; }
    public DateTime? ReversedAt { get; set; }
    public string? ReversalReason { get; set; }
    /// <summary>All legs posted together (including this one) when the transaction is part of a batch.</summary>
    public List<TransactionRowViewModel> BatchLegs { get; set; } = new();
    public bool CanReverse { get; set; }
}

// ─── MINI STATEMENT LINES (used by the account pages) ────────────────────────

public class StatementLineViewModel
{
    public DateOnly ValueDate { get; set; }
    public string TransactionRef { get; set; } = string.Empty;
    public string Narration { get; set; } = string.Empty;
    public string DrCr { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsReversal { get; set; }
}
