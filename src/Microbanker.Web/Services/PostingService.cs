// src/Microbanker.Web/Services/PostingService.cs
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Entities.Accounts;
using Microbanker.Domain.Entities.Setup;
using Microbanker.Domain.Entities.Transactions;
using Microbanker.Infrastructure.Persistence;

namespace Microbanker.Web.Services;

/// <summary>
/// The only code that changes account balances and writes transactions.
///
/// Every posting runs in one database transaction and either fully happens or does not happen at all.
/// Account rows carry a concurrency token, so if another user changes an account while we are working,
/// the posting is retried from scratch against the fresh balances instead of overwriting them.
///
/// The general-ledger side is recorded as <see cref="Transaction.GLAccountNo"/> on each leg; turning that
/// into ledger entries is the job of the end-of-day / ledger phase.
/// </summary>
public class PostingService : IPostingService
{
    private const int MaxAttempts = 4;
    private readonly MicrobankerDbContext _db;

    public PostingService(MicrobankerDbContext db) => _db = db;

    // ═════════════════════════════════════════════════════════════════════════
    //  Post
    // ═════════════════════════════════════════════════════════════════════════

    public async Task<PostingResult> PostAsync(PostingRequest request)
    {
        var problem = ValidateShape(request);
        if (problem != null) return PostingResult.Fail(problem);
        return await RunAsync(() => TryPostAsync(request));
    }

    private static string? ValidateShape(PostingRequest r)
    {
        if (r.Legs.Count == 0) return "There is nothing to post.";
        if (r.Legs.Count > 500) return "A posting can have at most 500 lines.";
        if (!string.IsNullOrEmpty(r.TellerRef) && r.TellerRef.Length > 50) return "The teller reference is too long (50 characters at most).";

        foreach (var (leg, i) in r.Legs.Select((l, i) => (l, i + 1)))
        {
            if (string.IsNullOrWhiteSpace(leg.AccountNo)) return $"Line {i}: the account number is missing.";
            if (!TransactionOptions.DrCrValues.Contains(leg.DrCr)) return $"Line {i}: the direction must be DR or CR.";
            if (leg.Amount <= 0 || decimal.Round(leg.Amount, 2) != leg.Amount)
                return $"Line {i}: the amount must be greater than zero with at most two decimal places.";
            var narration = leg.Narration ?? r.Narration;
            if (string.IsNullOrWhiteSpace(narration)) return $"Line {i}: a narration is required.";
            if (narration.Length > 250) return $"Line {i}: the narration is too long (250 characters at most).";
        }
        return null;
    }

    private async Task<PostingResult> TryPostAsync(PostingRequest r)
    {
        var legs = r.Legs;
        var hasDr = legs.Any(l => l.DrCr == TransactionOptions.Debit);
        var hasCr = legs.Any(l => l.DrCr == TransactionOptions.Credit);

        // ── Transaction codes ──
        var wanted = new[] { r.DebitCodeId, r.CreditCodeId }.Where(x => x != null).Select(x => x!.Value).ToList();
        var codes = await _db.TransactionCodes
            .Where(c => wanted.Contains(c.Id) && c.OrganizationId == r.OrganizationId && !c.IsDeleted && c.IsActive)
            .ToDictionaryAsync(c => c.Id);

        TransactionCode? debitCode = null, creditCode = null;
        if (hasDr && (r.DebitCodeId == null || !codes.TryGetValue(r.DebitCodeId.Value, out debitCode) || debitCode.DrCr != TransactionOptions.Debit))
            return PostingResult.Fail("Please select a valid debit transaction code.");
        if (hasCr && (r.CreditCodeId == null || !codes.TryGetValue(r.CreditCodeId.Value, out creditCode) || creditCode.DrCr != TransactionOptions.Credit))
            return PostingResult.Fail("Please select a valid credit transaction code.");

        // ── Withdrawal request being paid out ──
        WithdrawalRequest? withdrawal = null;
        if (r.WithdrawalRequestId != null)
        {
            withdrawal = await _db.WithdrawalRequests.FirstOrDefaultAsync(w =>
                w.Id == r.WithdrawalRequestId && w.OrganizationId == r.OrganizationId && !w.IsDeleted);
            if (withdrawal == null || withdrawal.Status != TransactionOptions.Approved)
                return PostingResult.Fail("This withdrawal has not been approved, or has already been paid.");
            if (legs.Count != 1 || legs[0].DrCr != TransactionOptions.Debit || legs[0].Amount != withdrawal.Amount
                || r.DebitCodeId != withdrawal.TransactionCodeId)
                return PostingResult.Fail("The posting does not match the approved withdrawal.");
        }
        else
        {
            // Codes flagged "requires approval" may only be used for an approved withdrawal request.
            var needsApproval = new[] { debitCode, creditCode }.FirstOrDefault(c => c is { RequiresApproval: true });
            if (needsApproval != null)
                return PostingResult.Fail($"Transaction code {needsApproval.Code} requires approval. Use a withdrawal request.");
        }

        // ── Accounts ──
        var numbers = legs.Select(l => Norm(l.AccountNo)).ToList();
        var dup = numbers.GroupBy(n => n).FirstOrDefault(g => g.Count() > 1);
        if (dup != null) return PostingResult.Fail($"Account {dup.Key} appears on more than one line.");

        var accounts = await _db.MemberAccounts.Include(a => a.ProductDefinition)
            .Where(a => a.OrganizationId == r.OrganizationId && !a.IsDeleted && numbers.Contains(a.AccountNo))
            .ToListAsync();
        var byNo = accounts.ToDictionary(a => a.AccountNo);
        var missing = numbers.FirstOrDefault(n => !byNo.ContainsKey(n));
        if (missing != null) return PostingResult.Fail($"Account {missing} was not found.");

        if (accounts.Select(a => a.Currency).Distinct().Count() > 1)
            return PostingResult.Fail("All accounts in a posting must be in the same currency.");

        if (withdrawal != null && byNo[numbers[0]].Id != withdrawal.AccountId)
            return PostingResult.Fail("The posting does not match the approved withdrawal.");

        // ── Balanced or one-sided ──
        var debits = legs.Where(l => l.DrCr == TransactionOptions.Debit).Sum(l => l.Amount);
        var credits = legs.Where(l => l.DrCr == TransactionOptions.Credit).Sum(l => l.Amount);
        var oneSided = !(hasDr && hasCr);
        var contraNo = string.IsNullOrWhiteSpace(r.ContraGlAccountNo) ? null : Norm(r.ContraGlAccountNo);

        if (oneSided)
        {
            if (contraNo == null) return PostingResult.Fail("Please select the general-ledger account to post against (cash, for example).");
            var gl = await _db.GLAccounts.FirstOrDefaultAsync(g => g.OrganizationId == r.OrganizationId && !g.IsDeleted && g.IsActive && g.AccountNumber == contraNo);
            if (gl == null) return PostingResult.Fail($"General-ledger account {contraNo} was not found or is not active.");
            if (gl.IsControlAccount) return PostingResult.Fail($"General-ledger account {contraNo} is a control account and cannot be posted to directly.");
        }
        else
        {
            if (contraNo != null) return PostingResult.Fail("A posting whose debits and credits balance cannot also name a general-ledger account.");
            if (debits != credits)
                return PostingResult.Fail($"The posting does not balance: debits are {debits:N2} and credits are {credits:N2}.");
        }

        // ── Value date ──
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (r.ValueDate > today) return PostingResult.Fail("The value date cannot be in the future.");

        // ── Per-account rules ──
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        foreach (var leg in legs)
        {
            var a = byNo[Norm(leg.AccountNo)];
            if (r.ValueDate < a.OpenDate)
                return PostingResult.Fail($"Account {a.AccountNo}: the value date is before the account was opened.");

            if (leg.DrCr == TransactionOptions.Credit)
            {
                if (a.Status != AccountOptions.Active && a.Status != AccountOptions.Dormant)
                    return PostingResult.Fail($"Account {a.AccountNo} is {a.Status.ToLowerInvariant()} and cannot be credited.");
                continue;
            }

            if (a.Status != AccountOptions.Active)
                return PostingResult.Fail($"Account {a.AccountNo} is {a.Status.ToLowerInvariant()} and cannot be debited.");
            if (leg.Amount > a.AvailableBalance)
                return PostingResult.Fail($"Account {a.AccountNo} has only {a.AvailableBalance:N2} available (blocked amounts are not available).");
            if (a.AvailableBalance - leg.Amount < a.ProductDefinition.MinBalance)
                return PostingResult.Fail($"Account {a.AccountNo}: this would take the balance below the product minimum of {a.ProductDefinition.MinBalance:N2}.");

            var limit = a.ProductDefinition.MaxDebitPerMonth;
            if (limit > 0)
            {
                var used = await _db.Transactions.CountAsync(t => t.AccountId == a.Id && t.DrCr == TransactionOptions.Debit
                    && !t.IsReversal && t.Status == TransactionOptions.Posted && t.PostingDate >= monthStart);
                if (used >= limit)
                    return PostingResult.Fail($"Account {a.AccountNo} has reached its limit of {limit} debits this month.");
            }
        }

        // ── Build and apply ──
        var refs = await NextRefsAsync(r.OrganizationId, legs.Count, now);
        var batchRef = legs.Count > 1 ? "BAT-" + refs[0][4..] : null;
        var txns = new List<Transaction>(legs.Count);

        for (var i = 0; i < legs.Count; i++)
        {
            var leg = legs[i];
            var a = byNo[Norm(leg.AccountNo)];
            var isDebit = leg.DrCr == TransactionOptions.Debit;

            if (isDebit) a.Debit(leg.Amount, now); else a.Credit(leg.Amount, now);

            txns.Add(new Transaction(r.OrganizationId, a.BranchId, refs[i],
                isDebit ? debitCode!.Id : creditCode!.Id, a.Id, a.AccountType, leg.DrCr, leg.Amount,
                (leg.Narration ?? r.Narration)!.Trim(), r.ValueDate, r.PostedBy, a.Currency, a.BookBalance,
                string.IsNullOrWhiteSpace(r.TellerRef) ? null : r.TellerRef.Trim(), batchRef,
                oneSided ? contraNo : null, now));
        }

        _db.Transactions.AddRange(txns);
        withdrawal?.Complete(txns[0].TransactionRef);
        await _db.SaveChangesAsync();
        return PostingResult.Ok(txns);
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  Reverse
    // ═════════════════════════════════════════════════════════════════════════

    public async Task<PostingResult> ReverseAsync(Guid organizationId, string transactionRef, Guid reversedBy, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason)) return PostingResult.Fail("Please give a reason for the reversal.");
        if (reason.Length > 200) return PostingResult.Fail("The reason is too long (200 characters at most).");
        return await RunAsync(() => TryReverseAsync(organizationId, Norm(transactionRef), reversedBy, reason.Trim()));
    }

    private async Task<PostingResult> TryReverseAsync(Guid orgId, string txnRef, Guid reversedBy, string reason)
    {
        var first = await _db.Transactions.FirstOrDefaultAsync(t => t.OrganizationId == orgId && t.TransactionRef == txnRef);
        if (first == null) return PostingResult.Fail($"Transaction {txnRef} was not found.");

        // A batch is reversed as a whole, otherwise its legs would no longer balance.
        var originals = first.BatchRef == null
            ? new List<Transaction> { first }
            : await _db.Transactions.Where(t => t.OrganizationId == orgId && t.BatchRef == first.BatchRef).OrderBy(t => t.TransactionRef).ToListAsync();

        if (originals.Any(t => t.IsReversal))
            return PostingResult.Fail("A reversal cannot be reversed. Post a new transaction instead.");
        if (originals.Any(t => t.Status != TransactionOptions.Posted))
            return PostingResult.Fail(first.Status == TransactionOptions.Reversed
                ? $"Transaction {txnRef} has already been reversed."
                : "Only posted transactions can be reversed.");
        if (originals.Any(t => t.AccountType == TransactionOptions.Loan))
            return PostingResult.Fail("Loan transactions cannot be reversed here.");

        var ids = originals.Select(t => t.AccountId).Distinct().ToList();
        var accounts = await _db.MemberAccounts.Where(a => ids.Contains(a.Id)).ToDictionaryAsync(a => a.Id);

        var now = DateTime.UtcNow;
        var refs = await NextRefsAsync(orgId, originals.Count, now);
        var batchRef = originals.Count > 1 ? "REV-" + first.BatchRef : null;
        var today = DateOnly.FromDateTime(now);
        var created = new List<Transaction>(originals.Count);

        // Take money back first, then give it back, so a reversal never depends on money that is about to arrive.
        var order = originals.Select((t, i) => (t, i)).OrderBy(x => x.t.DrCr == TransactionOptions.Credit ? 0 : 1).ToList();
        var newRefByOriginal = new Dictionary<Guid, string>();
        foreach (var (t, i) in order) newRefByOriginal[t.Id] = refs[i];

        foreach (var (t, _) in order)
        {
            var account = accounts[t.AccountId];
            if (t.DrCr == TransactionOptions.Credit) account.UndoCredit(t.Amount); else account.UndoDebit(t.Amount);

            var reversal = Transaction.CreateReversalOf(t, newRefByOriginal[t.Id], reversedBy, today, account.BookBalance, batchRef, reason, now);
            t.Reverse(reversal.TransactionRef, reversedBy, reason);
            created.Add(reversal);
        }

        _db.Transactions.AddRange(created);
        await _db.SaveChangesAsync();
        return PostingResult.Ok(created);
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  Shared
    // ═════════════════════════════════════════════════════════════════════════

    private static string Norm(string s) => s.Trim().ToUpperInvariant();

    /// <summary>
    /// The next <paramref name="count"/> references, TXN-{yyyyMMdd}-{seq:D6}, after the highest issued for that day.
    /// A unique index backs this up: if two postings pick the same numbers the loser is retried.
    /// </summary>
    private async Task<List<string>> NextRefsAsync(Guid orgId, int count, DateTime now)
    {
        var prefix = $"TXN-{now:yyyyMMdd}-";
        var last = await _db.Transactions
            .Where(t => t.OrganizationId == orgId && t.TransactionRef.StartsWith(prefix))
            .OrderByDescending(t => t.TransactionRef)
            .Select(t => t.TransactionRef)
            .FirstOrDefaultAsync();
        var seq = last != null && int.TryParse(last.AsSpan(prefix.Length), out var n) ? n : 0;
        return Enumerable.Range(1, count).Select(i => $"{prefix}{seq + i:D6}").ToList();
    }

    /// <summary>Runs one attempt inside a database transaction, retrying on a concurrent change or reference clash.</summary>
    private async Task<PostingResult> RunAsync(Func<Task<PostingResult>> attemptBody)
    {
        for (var attempt = 1; ; attempt++)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var result = await attemptBody();
                if (result.Success)
                {
                    await tx.CommitAsync();
                    return result;
                }
                await tx.RollbackAsync();
                _db.ChangeTracker.Clear();
                return result;
            }
            catch (InvalidOperationException ex)
            {
                // A balance rule enforced by the account itself (it re-checks what we checked above).
                await tx.RollbackAsync();
                _db.ChangeTracker.Clear();
                return PostingResult.Fail(ex.Message);
            }
            catch (Exception ex) when (IsRetryable(ex))
            {
                await tx.RollbackAsync();
                _db.ChangeTracker.Clear();
                if (attempt >= MaxAttempts)
                    return PostingResult.Fail("The accounts were being changed by someone else. Please try again.");
            }
        }
    }

    private static bool IsRetryable(Exception ex) =>
        ex is DbUpdateConcurrencyException
        || (ex is DbUpdateException { InnerException: SqlException { Number: 2601 or 2627 } });
}
