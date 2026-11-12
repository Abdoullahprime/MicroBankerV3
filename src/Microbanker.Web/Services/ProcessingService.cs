// src/Microbanker.Web/Services/ProcessingService.cs
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Entities.Accounts;
using Microbanker.Domain.Entities.GeneralLedger;
using Microbanker.Domain.Entities.Setup;
using Microbanker.Domain.Entities.Transactions;
using Microbanker.Infrastructure.Persistence;

namespace Microbanker.Web.Services;

/// <summary>
/// The day-end routines. Each one either completes or leaves the books as they were (end-of-day runs in a
/// single database transaction; month-end is resumable because paid-out interest comes off the accrual in the
/// same step as the credit). Every run, successful or not, leaves a row in the EOD log.
///
/// Interest: a deposit account earns its product's annual rate, accrued daily on the closing balance
/// (day count 365). SIMPLE and DAILY_BALANCE use the balance alone; COMPOUND also earns on interest already
/// accrued. MINIMUM_BALANCE needs a per-period minimum that is not tracked, so those accounts are skipped
/// and reported. The product's InterestFrequency is the number of months between payments: interest is paid
/// at the end of every month whose number is a multiple of it (1 = monthly, 3 = quarterly, 12 = yearly).
/// Interest paid to customers is an expense of the bank, posted against the account's expense GL.
/// </summary>
public class ProcessingService : IProcessingService
{
    private const string InterestCode = "INT";
    private const int InterestBatchSize = 400;
    private readonly MicrobankerDbContext _db;
    private readonly IPostingService _posting;

    public ProcessingService(MicrobankerDbContext db, IPostingService posting)
    {
        _db = db;
        _posting = posting;
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  Start of day
    // ═════════════════════════════════════════════════════════════════════════

    public async Task<ProcessingResult> StartDayAsync(Guid orgId, Guid userId, DateOnly? firstDate)
    {
        var state = await _db.ProcessingDates.FirstOrDefaultAsync(p => p.OrganizationId == orgId && !p.IsDeleted);
        try
        {
            if (state == null)
            {
                if (firstDate == null) return ProcessingResult.Fail("Choose the first business date.");
                state = new ProcessingDate(orgId, firstDate.Value);
                _db.ProcessingDates.Add(state);
            }
            else if (state.EODStatus == GLOptions.EodRunning)
                return ProcessingResult.Fail("End-of-day is running.");
            else if (state.EODStatus == GLOptions.Open)
                return ProcessingResult.Fail($"{state.CurrentDate:dd MMM yyyy} is already open.");

            state.StartDay();
            var log = new EODLog(orgId, state.CurrentDate, GLOptions.Sod, userId);
            log.Complete(null);
            _db.EODLogs.Add(log);
            await _db.SaveChangesAsync();
            return ProcessingResult.Ok($"{state.CurrentDate:dddd dd MMM yyyy} is open for business.");
        }
        catch (DbUpdateConcurrencyException)
        {
            _db.ChangeTracker.Clear();
            return ProcessingResult.Fail("Someone else changed the business day at the same time. Please refresh.");
        }
        catch (DbUpdateException) when (state != null)
        {
            // The unique index on the organisation: another user created the calendar first.
            _db.ChangeTracker.Clear();
            return ProcessingResult.Fail("The business day was started by someone else. Please refresh.");
        }
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  End of day
    // ═════════════════════════════════════════════════════════════════════════

    public async Task<ProcessingResult> RunEodAsync(Guid orgId, Guid userId)
    {
        var state = await _db.ProcessingDates.FirstOrDefaultAsync(p => p.OrganizationId == orgId && !p.IsDeleted);
        if (state == null) return ProcessingResult.Fail("The business day has not been started yet. Run start-of-day first.");
        if (state.EODStatus != GLOptions.Open)
            return ProcessingResult.Fail(state.EODStatus == GLOptions.EodRunning ? "End-of-day is already running." : "The day is closed. Start the day before running end-of-day.");

        var date = state.CurrentDate;
        var startedAt = DateTime.UtcNow;
        var notes = new List<string>();

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            // Claim the run first: if another user is doing the same, one of the two saves fails.
            state.BeginEod();
            await _db.SaveChangesAsync();

            // 1. The ledger must balance before the day is closed.
            var drTotal = await _db.GLEntries.Where(g => g.OrganizationId == orgId && g.DrCr == GLOptions.Debit).Select(g => (decimal?)g.Amount).SumAsync() ?? 0m;
            var crTotal = await _db.GLEntries.Where(g => g.OrganizationId == orgId && g.DrCr == GLOptions.Credit).Select(g => (decimal?)g.Amount).SumAsync() ?? 0m;
            if (drTotal != crTotal)
                throw new InvalidOperationException($"The general ledger is out of balance (debits {drTotal:N2}, credits {crTotal:N2}). Day-end was stopped; investigate before continuing.");
            notes.Add($"Ledger in balance: {drTotal:N2} debits = {crTotal:N2} credits.");

            // 2. Interest accrual and 3. dormancy
            var accounts = await _db.MemberAccounts.Include(a => a.ProductDefinition)
                .Where(a => a.OrganizationId == orgId && !a.IsDeleted && (a.Status == AccountOptions.Active || a.Status == AccountOptions.Dormant))
                .ToListAsync();

            int accrued = 0, unsupported = 0;
            decimal accruedTotal = 0m;
            foreach (var a in accounts)
            {
                var p = a.ProductDefinition;
                if (p.InterestRate <= 0 || p.InterestFrequency <= 0 || a.BookBalance <= 0) continue;
                if (p.InterestCalcMethod == "MINIMUM_BALANCE") { unsupported++; continue; }

                var basis = p.InterestCalcMethod == "COMPOUND" ? a.BookBalance + a.InterestAccruedCR : a.BookBalance;
                var daily = Math.Round(basis * p.InterestRate / 100m / 365m, 6);
                if (daily <= 0) continue;
                a.AccrueInterest(daily);
                accrued++;
                accruedTotal += daily;
            }
            notes.Add($"Interest accrued on {accrued} account(s), {accruedTotal:N4} in total.");
            if (unsupported > 0)
                notes.Add($"{unsupported} account(s) use the MINIMUM_BALANCE method, which is not supported, and earned no interest.");

            var dormant = 0;
            foreach (var a in accounts.Where(a => a.Status == AccountOptions.Active))
            {
                var limit = a.ProductDefinition.DormancyDays;
                if (limit > 0 && date.DayNumber - a.LastActivityDate.DayNumber > limit)
                {
                    a.MarkDormant();
                    dormant++;
                }
            }
            notes.Add($"{dormant} account(s) became dormant.");

            // 4. Move on
            state.CompleteEod();
            var log = new EODLog(orgId, date, GLOptions.Eod, userId, startedAt);
            log.Complete(string.Join("\n", notes));
            _db.EODLogs.Add(log);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return ProcessingResult.Ok($"{date:dd MMM yyyy} is closed. The business date is now {state.CurrentDate:dd MMM yyyy}; start the day to open it.", notes);
        }
        catch (Exception ex) when (ex is DbUpdateConcurrencyException or InvalidOperationException or DbUpdateException)
        {
            await tx.RollbackAsync();
            _db.ChangeTracker.Clear();
            var message = ex is DbUpdateConcurrencyException
                ? "Someone else is running end-of-day at the same time."
                : ex.Message;
            await RecordFailureAsync(orgId, date, GLOptions.Eod, userId, startedAt, message);
            return ProcessingResult.Fail(message);
        }
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  Month end
    // ═════════════════════════════════════════════════════════════════════════

    public async Task<ProcessingResult> RunEomAsync(Guid orgId, Guid userId)
    {
        var state = await _db.ProcessingDates.AsNoTracking().FirstOrDefaultAsync(p => p.OrganizationId == orgId && !p.IsDeleted);
        if (state == null) return ProcessingResult.Fail("The business day has not been started yet.");
        if (state.EODStatus != GLOptions.Closed)
            return ProcessingResult.Fail("Month-end runs between end-of-day and start-of-day. Close the day first.");

        var period = state.ClosedThroughPeriod == null
            ? GLOptions.PeriodOf(state.CurrentDate.AddDays(-1))
            : GLOptions.NextPeriod(state.ClosedThroughPeriod);
        var periodEnd = GLOptions.PeriodEnd(period);
        if (periodEnd >= state.CurrentDate)
            return ProcessingResult.Fail($"Period {period} is not over yet: its last day, {periodEnd:dd MMM yyyy}, has not been closed.");

        var startedAt = DateTime.UtcNow;
        var notes = new List<string>();
        try
        {
            // ── 1. Pay accrued interest ──
            var month = periodEnd.Month;
            var accounts = await _db.MemberAccounts.AsNoTracking().Include(a => a.ProductDefinition)
                .Where(a => a.OrganizationId == orgId && !a.IsDeleted && (a.Status == AccountOptions.Active || a.Status == AccountOptions.Dormant))
                .ToListAsync();

            var due = accounts.Where(a => a.ProductDefinition.InterestFrequency > 0 && month % a.ProductDefinition.InterestFrequency == 0 && a.PayableInterest >= 0.01m).ToList();
            var noGl = due.Where(a => string.IsNullOrWhiteSpace(a.GLExpenseAccount)).ToList();
            if (noGl.Count > 0)
                notes.Add($"{noGl.Count} account(s) have interest to pay but no expense GL account set, so it stays accrued: {string.Join(", ", noGl.Take(10).Select(a => a.AccountNo))}{(noGl.Count > 10 ? ", …" : "")}.");

            var payable = due.Where(a => !string.IsNullOrWhiteSpace(a.GLExpenseAccount)).ToList();
            decimal paidTotal = 0m;
            var paidCount = 0;

            if (payable.Count > 0)
            {
                var code = await EnsureInterestCodeAsync(orgId);
                if (code == null)
                    return await FailAsync(orgId, state.CurrentDate, GLOptions.Eom, userId, startedAt,
                        $"A transaction code \"{InterestCode}\" exists but is not an active credit code without approval. Fix it under Transaction Codes, then run month-end again.");

                foreach (var group in payable.GroupBy(a => a.GLExpenseAccount!))
                {
                    foreach (var chunk in group.Chunk(InterestBatchSize))
                    {
                        var result = await _posting.PostAsync(new PostingRequest
                        {
                            OrganizationId = orgId, PostedBy = userId, ValueDate = periodEnd, IsSystem = true,
                            CreditCodeId = code.Id, ContraGlAccountNo = group.Key, Narration = $"Interest for {period}",
                            Legs = chunk.Select(a => new PostingLeg(a.AccountNo, TransactionOptions.Credit, a.PayableInterest, null, SettlesAccruedInterest: true)).ToList()
                        });
                        if (!result.Success)
                        {
                            // Interest already paid stays paid and is off the accrual, so running month-end again continues from here.
                            return await FailAsync(orgId, state.CurrentDate, GLOptions.Eom, userId, startedAt,
                                $"Interest could not be paid against GL {group.Key}: {result.Error} Month-end was stopped; fix the cause and run it again.", notes);
                        }
                        paidCount += result.Transactions.Count;
                        paidTotal += result.Transactions.Sum(t => t.Amount);
                    }
                }
            }
            notes.Add($"Interest of {paidTotal:N2} paid to {paidCount} account(s).");

            // ── 2 & 3. Reset monthly counters and close the period ──
            await using var tx = await _db.Database.BeginTransactionAsync();
            _db.ChangeTracker.Clear();
            var open = await _db.MemberAccounts.Where(a => a.OrganizationId == orgId && !a.IsDeleted && a.Status != AccountOptions.Closed && a.Status != AccountOptions.Pending).ToListAsync();
            foreach (var a in open) a.ResetMonthlyTurnover();
            notes.Add($"Monthly turnover reset on {open.Count} account(s).");

            var live = await _db.ProcessingDates.FirstAsync(p => p.OrganizationId == orgId && !p.IsDeleted);
            live.CompleteEom(period);
            var log = new EODLog(orgId, periodEnd, GLOptions.Eom, userId, startedAt);
            log.Complete(string.Join("\n", notes));
            _db.EODLogs.Add(log);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return ProcessingResult.Ok($"Period {period} is closed. Nothing more can be posted into it.", notes);
        }
        catch (Exception ex) when (ex is DbUpdateConcurrencyException or InvalidOperationException or DbUpdateException)
        {
            _db.ChangeTracker.Clear();
            return await FailAsync(orgId, state.CurrentDate, GLOptions.Eom, userId, startedAt,
                ex is DbUpdateConcurrencyException ? "Someone changed the books while month-end was running. Please run it again." : ex.Message, notes);
        }
    }

    /// <summary>The interest transaction code, created on first use. Null when one exists but is unusable.</summary>
    private async Task<TransactionCode?> EnsureInterestCodeAsync(Guid orgId)
    {
        var code = await _db.TransactionCodes.FirstOrDefaultAsync(c => c.OrganizationId == orgId && !c.IsDeleted && c.Code == InterestCode);
        if (code == null)
        {
            code = new TransactionCode(orgId, InterestCode, "Interest credited", TransactionOptions.Credit);
            _db.TransactionCodes.Add(code);
            await _db.SaveChangesAsync();
            _db.Entry(code).State = EntityState.Detached;
            return code;
        }
        return code is { DrCr: TransactionOptions.Credit, IsActive: true, RequiresApproval: false } ? code : null;
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  Year end
    // ═════════════════════════════════════════════════════════════════════════

    public async Task<YearEndPreview> PreviewEoyAsync(Guid orgId)
    {
        var (year, problem) = await YearToCloseAsync(orgId);
        if (year == null) return new YearEndPreview { Problem = problem };

        var lines = await ProfitAndLossBalancesAsync(orgId, new DateOnly(year.Value, 12, 31));
        var profit = lines.Where(l => l.Type == "INCOME").Sum(l => l.Balance) - lines.Where(l => l.Type == "EXPENSE").Sum(l => l.Balance);
        return new YearEndPreview { Year = year.Value, Lines = lines, NetProfit = profit };
    }

    public async Task<ProcessingResult> RunEoyAsync(Guid orgId, Guid userId, string? retainedEarningsGlNo)
    {
        var (year, problem) = await YearToCloseAsync(orgId);
        if (year == null) return ProcessingResult.Fail(problem!);

        var reNo = retainedEarningsGlNo?.Trim().ToUpperInvariant();
        var re = string.IsNullOrEmpty(reNo) ? null : await _db.GLAccounts.AsNoTracking()
            .FirstOrDefaultAsync(g => g.OrganizationId == orgId && !g.IsDeleted && g.IsActive && g.AccountNumber == reNo);
        if (re == null) return ProcessingResult.Fail("Please choose an active retained-earnings account.");
        if (re.AccountType != "EQUITY") return ProcessingResult.Fail("The retained-earnings account must be an EQUITY account.");

        var yearEnd = new DateOnly(year.Value, 12, 31);
        var startedAt = DateTime.UtcNow;
        var notes = new List<string>();

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var lines = await ProfitAndLossBalancesAsync(orgId, yearEnd);
            var journalRef = $"{GLOptions.ClosingJournalPrefix}{year}";
            var now = DateTime.UtcNow;
            var entries = new List<GLEntry>();
            decimal toRetained = 0m;                                   // net credit to retained earnings (profit)

            foreach (var l in lines.Where(l => l.Balance != 0))
            {
                // Income has a credit balance: debit it to clear it. Expense has a debit balance: credit it.
                var clearWith = l.Type == "INCOME" ? GLOptions.Debit : GLOptions.Credit;
                var amount = Math.Abs(l.Balance);
                var side = l.Balance > 0 ? clearWith : (clearWith == GLOptions.Debit ? GLOptions.Credit : GLOptions.Debit);
                entries.Add(new GLEntry(orgId, journalRef, l.GlAccountNo, side, amount, $"Year-end closing {year}", yearEnd, now, userId, GLOptions.System, journalRef));
                toRetained += side == GLOptions.Debit ? amount : -amount;
            }

            if (toRetained != 0)
                entries.Add(new GLEntry(orgId, journalRef, re.AccountNumber, toRetained > 0 ? GLOptions.Credit : GLOptions.Debit, Math.Abs(toRetained),
                    $"Year-end closing {year}: {(toRetained > 0 ? "profit" : "loss")} to retained earnings", yearEnd, now, userId, GLOptions.System, journalRef));

            var dr = entries.Where(e => e.DrCr == GLOptions.Debit).Sum(e => e.Amount);
            var cr = entries.Where(e => e.DrCr == GLOptions.Credit).Sum(e => e.Amount);
            if (dr != cr) throw new InvalidOperationException("The closing journal does not balance. Nothing was posted.");

            _db.GLEntries.AddRange(entries);
            var live = await _db.ProcessingDates.FirstAsync(p => p.OrganizationId == orgId && !p.IsDeleted);
            live.CompleteEoy();

            notes.Add(entries.Count == 0
                ? "There were no income or expense balances to close."
                : $"{lines.Count(l => l.Balance != 0)} income / expense account(s) closed; {(toRetained >= 0 ? "profit" : "loss")} of {Math.Abs(toRetained):N2} moved to {re.AccountNumber} {re.AccountName}.");

            var log = new EODLog(orgId, yearEnd, GLOptions.Eoy, userId, startedAt);
            log.Complete(string.Join("\n", notes));
            _db.EODLogs.Add(log);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return ProcessingResult.Ok($"Fiscal year {year} is closed.", notes);
        }
        catch (Exception ex) when (ex is DbUpdateConcurrencyException or InvalidOperationException or DbUpdateException)
        {
            await tx.RollbackAsync();
            _db.ChangeTracker.Clear();
            return await FailAsync(orgId, yearEnd, GLOptions.Eoy, userId, startedAt, ex.Message);
        }
    }

    /// <summary>The fiscal year that can be closed now: the one whose December has been closed by month-end and that has not been closed yet.</summary>
    private async Task<(int? Year, string? Problem)> YearToCloseAsync(Guid orgId)
    {
        var state = await _db.ProcessingDates.AsNoTracking().FirstOrDefaultAsync(p => p.OrganizationId == orgId && !p.IsDeleted);
        if (state == null) return (null, "The business day has not been started yet.");
        if (state.EODStatus == GLOptions.EodRunning) return (null, "End-of-day is running.");
        if (state.ClosedThroughPeriod == null) return (null, "No month has been closed yet.");

        var done = await _db.EODLogs.Where(l => l.OrganizationId == orgId && l.ProcessType == GLOptions.Eoy && l.Status == GLOptions.Completed)
            .Select(l => l.ProcessDate.Year).ToListAsync();

        // The earliest year with ledger activity whose December has been closed and that has no completed year-end.
        var closedThrough = GLOptions.PeriodStart(state.ClosedThroughPeriod);
        var lastClosedYear = closedThrough.Month == 12 ? closedThrough.Year : closedThrough.Year - 1;
        var firstEntry = await _db.GLEntries.Where(g => g.OrganizationId == orgId).Select(g => (DateOnly?)g.ValueDate).MinAsync();
        var startYear = firstEntry?.Year ?? lastClosedYear;
        for (var y = startYear; y <= lastClosedYear; y++)
            if (!done.Contains(y)) return (y, null);

        return (null, "There is no closed fiscal year waiting to be closed. Run month-end for December first.");
    }

    /// <summary>Net balance of every income and expense account up to the date, as positive numbers (income: credits − debits; expense: debits − credits).</summary>
    private async Task<List<(string GlAccountNo, string Name, string Type, decimal Balance)>> ProfitAndLossBalancesAsync(Guid orgId, DateOnly upTo)
    {
        var accounts = await _db.GLAccounts.AsNoTracking()
            .Where(g => g.OrganizationId == orgId && !g.IsDeleted && (g.AccountType == "INCOME" || g.AccountType == "EXPENSE"))
            .OrderBy(g => g.AccountNumber).ToListAsync();

        var sums = await _db.GLEntries.Where(g => g.OrganizationId == orgId && g.ValueDate <= upTo)
            .GroupBy(g => new { g.GLAccountNo, g.DrCr })
            .Select(g => new { g.Key.GLAccountNo, g.Key.DrCr, Total = g.Sum(x => x.Amount) }).ToListAsync();

        decimal Sum(string no, string drCr) => sums.Where(s => s.GLAccountNo == no && s.DrCr == drCr).Sum(s => s.Total);

        return accounts.Select(a =>
        {
            var dr = Sum(a.AccountNumber, GLOptions.Debit);
            var cr = Sum(a.AccountNumber, GLOptions.Credit);
            return (a.AccountNumber, a.AccountName, a.AccountType, a.AccountType == "INCOME" ? cr - dr : dr - cr);
        }).ToList();
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  Failures
    // ═════════════════════════════════════════════════════════════════════════

    private async Task RecordFailureAsync(Guid orgId, DateOnly date, string type, Guid userId, DateTime startedAt, string message)
    {
        var log = new EODLog(orgId, date, type, userId, startedAt);
        log.Fail(message);
        _db.EODLogs.Add(log);
        await _db.SaveChangesAsync();
    }

    private async Task<ProcessingResult> FailAsync(Guid orgId, DateOnly date, string type, Guid userId, DateTime startedAt, string message, IEnumerable<string>? notes = null)
    {
        _db.ChangeTracker.Clear();
        await RecordFailureAsync(orgId, date, type, userId, startedAt, message);
        return ProcessingResult.Fail(message, notes);
    }
}
