using Microbanker.Domain.Entities.Customers;
using Microbanker.Domain.Entities.Organization;
using Microbanker.Domain.Entities.Setup;

namespace Microbanker.Domain.Entities.Accounts;

/// <summary>
/// A customer's savings, current or fixed-deposit account. The AccountType column is the discriminator.
///
/// Balances: <see cref="BookBalance"/> is the ledger balance; <see cref="AvailableBalance"/> is what the
/// customer can use, i.e. book balance minus <see cref="BlockedAmount"/>. Balances only move through the
/// methods on this class so that block/unblock can never leave the three figures inconsistent.
/// </summary>
public class MemberAccount : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid BranchId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid ProductDefinitionId { get; private set; }

    /// <summary>{BranchCode}-{TypePrefix}-{seq:D7}; never changes once issued.</summary>
    public string AccountNo { get; private set; } = string.Empty;
    public string AccountName { get; private set; } = string.Empty;
    public string AccountType { get; private set; } = AccountOptions.Savings;
    public string Currency { get; private set; } = AccountOptions.DefaultCurrency;
    public string Status { get; private set; } = AccountOptions.Pending;

    // ── Balances ──
    public decimal OpeningBalance { get; private set; }
    public decimal BookBalance { get; private set; }
    public decimal AvailableBalance { get; private set; }
    public decimal BlockedAmount { get; private set; }
    public decimal UnauthorizedDR { get; private set; }
    public decimal UnauthorizedCR { get; private set; }
    public decimal MonthlyDRTurnover { get; private set; }
    public decimal MonthlyCRTurnover { get; private set; }
    public decimal InterestAccruedDR { get; private set; }
    public decimal InterestAccruedCR { get; private set; }

    // ── Activity (maintained by posting / end-of-day processing in later phases) ──
    public DateTime? LastDRDate { get; private set; }
    public DateTime? LastCRDate { get; private set; }
    public DateTime? LastDRActivityDate { get; private set; }
    public DateTime? LastCRActivityDate { get; private set; }

    // ── ATM ──
    public string? AtmAccountNo { get; private set; }
    public decimal AtmDailyLimit { get; private set; }

    // ── Classification ──
    public Guid? SectorId { get; private set; }
    public Guid? SubSectorId { get; private set; }

    // ── GL mapping (GLAccount.AccountNumber values) ──
    public string? GLAssetControl { get; private set; }
    public string? GLLiabilityControl { get; private set; }
    public string? GLIncomeAccount { get; private set; }
    public string? GLExpenseAccount { get; private set; }
    public string? GLChargeAccount { get; private set; }
    public string? GLFeeAccount { get; private set; }

    // ── Approval / maintenance ──
    public bool IsApproved { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public bool EditRequestPending { get; private set; }
    public string? EditRequestReason { get; private set; }
    public Guid? CreatedByUserId { get; private set; }
    public DateOnly OpenDate { get; private set; }
    public DateTime? ClosedAt { get; private set; }

    /// <summary>
    /// Optimistic-concurrency token, replaced on every change. Two users blocking amounts on the same
    /// account at once cannot both win: the second save fails instead of silently overwriting.
    /// </summary>
    public Guid ConcurrencyStamp { get; private set; } = Guid.NewGuid();

    public Customer Customer { get; private set; } = null!;
    public Branch Branch { get; private set; } = null!;
    public ProductDefinition ProductDefinition { get; private set; } = null!;

    private MemberAccount() { }

    public MemberAccount(Guid orgId, Guid branchId, Guid customerId, Guid productDefId,
        string accountNo, string accountName, string accountType, Guid? createdByUserId = null)
    {
        if (!AccountOptions.AccountTypes.Contains(accountType))
            throw new ArgumentOutOfRangeException(nameof(accountType), accountType, "Unknown account type.");

        OrganizationId = orgId;
        BranchId = branchId;
        CustomerId = customerId;
        ProductDefinitionId = productDefId;
        AccountNo = accountNo;
        AccountName = accountName;
        AccountType = accountType;
        Status = AccountOptions.Pending;
        CreatedByUserId = createdByUserId;
        OpenDate = DateOnly.FromDateTime(DateTime.UtcNow);
    }

    // ── Editing ───────────────────────────────────────────────────────────────

    /// <summary>The terms that are fixed once the account is approved: product, currency, opening balance, open date.</summary>
    public void UpdateTerms(Guid productDefinitionId, string currency, decimal openingBalance, DateOnly openDate)
    {
        if (Status != AccountOptions.Pending)
            throw new InvalidOperationException("The terms of an approved account cannot be changed.");
        if (openingBalance < 0)
            throw new ArgumentOutOfRangeException(nameof(openingBalance), "Opening balance cannot be negative.");

        ProductDefinitionId = productDefinitionId;
        Currency = currency;
        OpenDate = openDate;
        OpeningBalance = openingBalance;
        BookBalance = openingBalance;
        AvailableBalance = openingBalance - BlockedAmount;
        Touch();
    }

    /// <summary>Everything that stays editable after approval (through an approved edit request).</summary>
    public void Update(string accountName, string? atmAccountNo, decimal atmDailyLimit,
        Guid? sectorId, Guid? subSectorId,
        string? glAssetControl, string? glLiabilityControl, string? glIncomeAccount,
        string? glExpenseAccount, string? glChargeAccount, string? glFeeAccount)
    {
        if (Status == AccountOptions.Closed)
            throw new InvalidOperationException("A closed account cannot be edited.");
        if (atmDailyLimit < 0)
            throw new ArgumentOutOfRangeException(nameof(atmDailyLimit), "ATM limit cannot be negative.");

        AccountName = accountName;
        AtmAccountNo = atmAccountNo;
        AtmDailyLimit = atmDailyLimit;
        SectorId = sectorId;
        SubSectorId = subSectorId;
        GLAssetControl = glAssetControl;
        GLLiabilityControl = glLiabilityControl;
        GLIncomeAccount = glIncomeAccount;
        GLExpenseAccount = glExpenseAccount;
        GLChargeAccount = glChargeAccount;
        GLFeeAccount = glFeeAccount;
        Touch();
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    public void Approve(Guid approvedBy)
    {
        if (Status != AccountOptions.Pending)
            throw new InvalidOperationException("Only a pending account can be approved.");
        IsApproved = true;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
        Status = AccountOptions.Active;
        Touch();
    }

    public void Freeze()
    {
        if (Status != AccountOptions.Active && Status != AccountOptions.Dormant)
            throw new InvalidOperationException("Only an active or dormant account can be frozen.");
        Status = AccountOptions.Frozen;
        Touch();
    }

    public void Unfreeze()
    {
        if (Status != AccountOptions.Frozen)
            throw new InvalidOperationException("Only a frozen account can be unfrozen.");
        Status = AccountOptions.Active;
        Touch();
    }

    /// <summary>An account can close only when nothing is held in it and nothing is owed on it.</summary>
    public void Close()
    {
        if (Status == AccountOptions.Pending || Status == AccountOptions.Closed)
            throw new InvalidOperationException("Only an approved, open account can be closed.");
        if (BookBalance != 0 || BlockedAmount != 0 || InterestAccruedDR != 0 || PayableInterest != 0
            || UnauthorizedDR != 0 || UnauthorizedCR != 0)
            throw new InvalidOperationException("The account still has a balance, a blocked amount, accrued interest or unauthorised items.");
        InterestAccruedCR = 0m;      // less than one cent is written off
        Status = AccountOptions.Closed;
        ClosedAt = DateTime.UtcNow;
        Touch();
    }

    // ── Blocking ──────────────────────────────────────────────────────────────

    public void BlockAmount(decimal amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
        if (Status != AccountOptions.Active)
            throw new InvalidOperationException("Amounts can only be blocked on an active account.");
        if (amount > AvailableBalance)
            throw new InvalidOperationException("The amount exceeds the available balance.");
        BlockedAmount += amount;
        AvailableBalance -= amount;
        Touch();
    }

    public void UnblockAmount(decimal amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
        if (Status == AccountOptions.Closed || Status == AccountOptions.Pending)
            throw new InvalidOperationException("Amounts can only be released on an open account.");
        if (amount > BlockedAmount)
            throw new InvalidOperationException("The amount exceeds the blocked amount.");
        BlockedAmount -= amount;
        AvailableBalance += amount;
        Touch();
    }

    // ── Posting ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Adds money to the account. Allowed on active and dormant accounts; a credit ends dormancy.
    /// Use through the posting service, which records the matching transaction.
    /// </summary>
    public void Credit(decimal amount, DateTime when, bool isInterest = false)
    {
        RequirePostable(amount);
        if (Status != AccountOptions.Active && Status != AccountOptions.Dormant)
            throw new InvalidOperationException($"The account is {Status.ToLowerInvariant()} and cannot be credited.");

        BookBalance += amount;
        AvailableBalance += amount;
        LastCRDate = when;
        // Interest the bank credits is not the customer's own activity: it neither counts as turnover nor ends dormancy.
        if (!isInterest)
        {
            MonthlyCRTurnover += amount;
            LastCRActivityDate = when;
            if (Status == AccountOptions.Dormant) Status = AccountOptions.Active;
        }
        Touch();
    }

    /// <summary>Takes money out of the account, never beyond the available balance (there are no overdrafts).</summary>
    public void Debit(decimal amount, DateTime when)
    {
        RequirePostable(amount);
        if (Status != AccountOptions.Active)
            throw new InvalidOperationException($"The account is {Status.ToLowerInvariant()} and cannot be debited.");
        if (amount > AvailableBalance)
            throw new InvalidOperationException("Insufficient available balance.");

        BookBalance -= amount;
        AvailableBalance -= amount;
        MonthlyDRTurnover += amount;
        LastDRDate = when;
        LastDRActivityDate = when;
        Touch();
    }

    /// <summary>Takes back a credit that is being reversed. Fails if the money has already been used or blocked.</summary>
    public void UndoCredit(decimal amount)
    {
        RequirePostable(amount);
        RequireOpenForReversal();
        if (amount > AvailableBalance)
            throw new InvalidOperationException("The credited money is no longer available, so the credit cannot be reversed.");

        BookBalance -= amount;
        AvailableBalance -= amount;
        MonthlyCRTurnover = Math.Max(0m, MonthlyCRTurnover - amount);
        Touch();
    }

    /// <summary>Puts back a debit that is being reversed.</summary>
    public void UndoDebit(decimal amount)
    {
        RequirePostable(amount);
        RequireOpenForReversal();

        BookBalance += amount;
        AvailableBalance += amount;
        MonthlyDRTurnover = Math.Max(0m, MonthlyDRTurnover - amount);
        Touch();
    }

    // ── Day-end processing ────────────────────────────────────────────────────

    /// <summary>Adds one day's interest to what is owed to the customer. Kept to six decimal places so that rounding does not drift.</summary>
    public void AccrueInterest(decimal amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Accrued interest cannot be negative.");
        if (Status != AccountOptions.Active && Status != AccountOptions.Dormant)
            throw new InvalidOperationException($"The account is {Status.ToLowerInvariant()} and cannot accrue interest.");
        InterestAccruedCR = Math.Round(InterestAccruedCR + amount, 6);
        Touch();
    }

    /// <summary>The accrued interest that can be paid now: whole cents, rounded down so the remainder carries forward.</summary>
    public decimal PayableInterest => Math.Floor(InterestAccruedCR * 100m) / 100m;

    /// <summary>Takes paid-out interest off the accrual. The matching credit is posted separately.</summary>
    public void SettleAccruedInterest(decimal amount)
    {
        RequirePostable(amount);
        if (amount > PayableInterest)
            throw new InvalidOperationException("The amount is more than the interest accrued on this account.");
        InterestAccruedCR = Math.Round(InterestAccruedCR - amount, 6);
        Touch();
    }

    /// <summary>Flags an unused account as dormant. Only an active account can become dormant.</summary>
    public void MarkDormant()
    {
        if (Status != AccountOptions.Active)
            throw new InvalidOperationException("Only an active account can become dormant.");
        Status = AccountOptions.Dormant;
        Touch();
    }

    public void ResetMonthlyTurnover()
    {
        MonthlyDRTurnover = 0m;
        MonthlyCRTurnover = 0m;
        Touch();
    }

    /// <summary>The date of the customer's last own debit or credit, or the opening date if there has been none.</summary>
    public DateOnly LastActivityDate
    {
        get
        {
            var last = new[] { LastDRActivityDate, LastCRActivityDate }.Where(d => d != null).Select(d => d!.Value).DefaultIfEmpty(DateTime.MinValue).Max();
            var date = last == DateTime.MinValue ? OpenDate : DateOnly.FromDateTime(last);
            return date < OpenDate ? OpenDate : date;
        }
    }

    private static void RequirePostable(decimal amount)
    {
        if (amount <= 0 || decimal.Round(amount, 2) != amount)
            throw new ArgumentOutOfRangeException(nameof(amount), "The amount must be greater than zero with at most two decimal places.");
    }

    private void RequireOpenForReversal()
    {
        if (Status == AccountOptions.Pending || Status == AccountOptions.Closed)
            throw new InvalidOperationException($"The account is {Status.ToLowerInvariant()}, so postings on it cannot be reversed.");
    }

    // ── Edit requests ─────────────────────────────────────────────────────────

    public void MarkEditRequested(string reason)
    {
        EditRequestPending = true;
        EditRequestReason = reason;
        Touch();
    }

    public void ClearEditRequest()
    {
        EditRequestPending = false;
        EditRequestReason = null;
        Touch();
    }

    public void SoftDelete()
    {
        if (Status != AccountOptions.Pending)
            throw new InvalidOperationException("Only a pending account can be deleted.");
        SetDeleted();
        Touch();
    }

    private void Touch()
    {
        SetUpdatedAt();
        ConcurrencyStamp = Guid.NewGuid();
    }
}
