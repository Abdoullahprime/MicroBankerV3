// src/Microbanker.Web/Controllers/TransactionController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Entities.Transactions;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Models;
using Microbanker.Web.Services;

namespace Microbanker.Web.Controllers;

public class TransactionController : SetupControllerBase
{
    private const int PageSize = 25;
    private readonly IPostingService _posting;

    public TransactionController(MicrobankerDbContext db, IEncryptionService enc, IPostingService posting) : base(db, enc)
        => _posting = posting;

    private void SetNav()
    {
        ViewData["ActiveGroup"] = "Teller";
        ViewData["ActivePage"]  = "Transactions";
    }

    // ── Search ────────────────────────────────────────────────────────────────

    public async Task<IActionResult> Index(string? q, string? status, string? drCr, DateOnly? from, DateOnly? to, int page = 1)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var query = from t in _db.Transactions
                    join a in _db.MemberAccounts on t.AccountId equals a.Id
                    where t.OrganizationId == org.Id
                    select new { Txn = t, a.AccountNo, a.AccountName };

        if (!string.IsNullOrWhiteSpace(status) && TransactionOptions.TransactionStatuses.Contains(status))
            query = query.Where(x => x.Txn.Status == status);
        if (drCr is TransactionOptions.Debit or TransactionOptions.Credit)
            query = query.Where(x => x.Txn.DrCr == drCr);
        if (from != null) query = query.Where(x => x.Txn.ValueDate >= from);
        if (to != null) query = query.Where(x => x.Txn.ValueDate <= to);

        q = q?.Trim();
        if (!string.IsNullOrEmpty(q))
            query = query.Where(x => x.Txn.TransactionRef.Contains(q) || (x.Txn.BatchRef != null && x.Txn.BatchRef.Contains(q))
                || (x.Txn.TellerRef != null && x.Txn.TellerRef.Contains(q)) || x.Txn.Narration.Contains(q)
                || x.AccountNo.Contains(q) || x.AccountName.Contains(q));

        var total = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        page = Math.Clamp(page, 1, totalPages);

        var rows = await query.OrderByDescending(x => x.Txn.PostingDate).ThenByDescending(x => x.Txn.TransactionRef)
            .Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();

        // Totals over everything that matches, counting only money that still stands (not reversed, not the reversal itself).
        var standing = query.Where(x => x.Txn.Status == TransactionOptions.Posted && !x.Txn.IsReversal);
        var debits = await standing.Where(x => x.Txn.DrCr == TransactionOptions.Debit).Select(x => (decimal?)x.Txn.Amount).SumAsync() ?? 0m;
        var credits = await standing.Where(x => x.Txn.DrCr == TransactionOptions.Credit).Select(x => (decimal?)x.Txn.Amount).SumAsync() ?? 0m;

        return View(new TransactionListViewModel
        {
            Q = q, Status = status, DrCr = drCr, From = from, To = to, Page = page, TotalPages = totalPages, TotalCount = total,
            TotalDebits = debits, TotalCredits = credits,
            Statuses = TransactionOptions.TransactionStatuses.Select(s => new SelectListItem(char.ToUpper(s[0]) + s[1..].ToLower(), s)).ToList(),
            Rows = rows.Select(x => ToRow(x.Txn, x.AccountNo, x.AccountName)).ToList()
        });
    }

    // ── View (receipt) ────────────────────────────────────────────────────────
    // Named Details in code because Controller.View(...) would clash; the route action is still "View".

    [HttpGet, ActionName("View")]
    public async Task<IActionResult> Details(string id)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var txnId)) return NotFound();

        var t = await _db.Transactions.FirstOrDefaultAsync(x => x.Id == txnId && x.OrganizationId == org.Id);
        if (t == null) return NotFound();

        var account = await _db.MemberAccounts.Where(a => a.Id == t.AccountId).Select(a => new { a.AccountNo, a.AccountName }).FirstOrDefaultAsync();
        var vm = new TransactionDetailViewModel
        {
            Txn = ToRow(t, account?.AccountNo ?? "", account?.AccountName ?? ""),
            AccountEncryptedId = _enc.Encrypt(t.AccountId.ToString()),
            TellerRef = t.TellerRef, BatchRef = t.BatchRef, GLAccountNo = t.GLAccountNo,
            ReversalRef = t.ReversalRef, ReversedAt = t.ReversedAt, ReversalReason = t.ReversalReason,
            CanReverse = t.Status == TransactionOptions.Posted && !t.IsReversal && t.AccountType != TransactionOptions.Loan
        };

        var code = await _db.TransactionCodes.Where(c => c.Id == t.TransactionCodeId).Select(c => c.Code + " — " + c.Name).FirstOrDefaultAsync();
        vm.CodeLabel = code ?? "";
        vm.BranchName = await _db.Branches.Where(b => b.Id == t.BranchId).Select(b => b.Name).FirstOrDefaultAsync() ?? "";
        if (t.GLAccountNo != null)
            vm.GLAccountName = await _db.GLAccounts.Where(g => g.OrganizationId == org.Id && !g.IsDeleted && g.AccountNumber == t.GLAccountNo)
                .Select(g => g.AccountName).FirstOrDefaultAsync();
        vm.PostedByName = await UserNameAsync(t.PostedBy);
        vm.ReversedByName = await UserNameAsync(t.ReversedBy);

        if (t.ReversalRef != null)
        {
            var other = await _db.Transactions.Where(x => x.OrganizationId == org.Id && x.TransactionRef == t.ReversalRef).Select(x => x.Id).FirstOrDefaultAsync();
            if (other != Guid.Empty) vm.ReversalEncryptedId = _enc.Encrypt(other.ToString());
        }

        if (t.BatchRef != null)
        {
            var legs = await (from x in _db.Transactions
                              join a in _db.MemberAccounts on x.AccountId equals a.Id
                              where x.OrganizationId == org.Id && x.BatchRef == t.BatchRef
                              orderby x.TransactionRef
                              select new { Txn = x, a.AccountNo, a.AccountName }).ToListAsync();
            vm.BatchLegs = legs.Select(x => ToRow(x.Txn, x.AccountNo, x.AccountName)).ToList();
        }
        return View("View", vm);
    }

    // ── Reverse ───────────────────────────────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reverse(string id, string? reason)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var txnId)) return NotFound();

        var t = await _db.Transactions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == txnId && x.OrganizationId == org.Id);
        if (t == null) return NotFound();

        var result = await _posting.ReverseAsync(org.Id, t.TransactionRef, GetUserId(), reason ?? "");
        if (result.Success)
            TempData["Success"] = result.Transactions.Count > 1
                ? $"{result.Transactions.Count} transactions reversed (the whole batch)."
                : $"Transaction {t.TransactionRef} reversed.";
        else
            TempData["Error"] = result.Error;
        return RedirectToAction("View", new { id });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private TransactionRowViewModel ToRow(Transaction t, string accountNo, string accountName) => new()
    {
        EncryptedId = _enc.Encrypt(t.Id.ToString()), TransactionRef = t.TransactionRef, ValueDate = t.ValueDate, PostingDate = t.PostingDate,
        AccountNo = accountNo, AccountName = accountName, DrCr = t.DrCr, Amount = t.Amount, Currency = t.Currency,
        Status = t.Status, IsReversal = t.IsReversal, Narration = t.Narration, BalanceAfter = t.BalanceAfter
    };

    private async Task<string?> UserNameAsync(Guid? userId)
    {
        if (userId == null) return null;
        var u = await _db.AppUsers.Where(x => x.Id == userId).Select(x => new { x.FirstName, x.LastName }).FirstOrDefaultAsync();
        return u == null ? null : $"{u.FirstName} {u.LastName}".Trim();
    }
}
