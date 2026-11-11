// src/Microbanker.Web/Controllers/TellerController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Entities.Accounts;
using Microbanker.Domain.Entities.Setup;
using Microbanker.Domain.Entities.Transactions;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Models;
using Microbanker.Web.Services;

namespace Microbanker.Web.Controllers;

/// <summary>
/// Counter operations. Every posting goes through <see cref="IPostingService"/>, the only code that changes
/// balances; this controller only gathers and checks what the teller typed.
/// </summary>
public class TellerController : SetupControllerBase
{
    private const int PageSize = 25;
    private const long MaxUploadBytes = 1_000_000;

    private readonly IPostingService _posting;

    public TellerController(MicrobankerDbContext db, IEncryptionService enc, IPostingService posting) : base(db, enc)
        => _posting = posting;

    private void SetNav(string page)
    {
        ViewData["ActiveGroup"] = "Teller";
        ViewData["ActivePage"]  = page;
    }

    private static DateOnly Today() => DateOnly.FromDateTime(DateTime.UtcNow);
    private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    // ═════════════════════════════════════════════════════════════════════════
    //  Deposit
    // ═════════════════════════════════════════════════════════════════════════

    [HttpGet]
    public async Task<IActionResult> Deposit(string? accountNo)
    {
        SetNav("Deposit");
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        var model = new CashPostingFormModel { Kind = "Deposit", AccountNo = accountNo, ValueDate = Today() };
        await FillCashListsAsync(model, org.Id, TransactionOptions.Credit);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Deposit(CashPostingFormModel model)
    {
        SetNav("Deposit");
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        model.Kind = "Deposit";
        model.ValueDate ??= Today();

        if (ModelState.IsValid)
        {
            var result = await _posting.PostAsync(new PostingRequest
            {
                OrganizationId = org.Id, PostedBy = GetUserId(), ValueDate = model.ValueDate.Value,
                CreditCodeId = model.TransactionCodeId, Narration = model.Narration, ContraGlAccountNo = model.CashGlAccountNo,
                TellerRef = model.TellerRef, Legs = new[] { new PostingLeg(model.AccountNo!, TransactionOptions.Credit, model.Amount) }
            });
            if (result.Success) return Posted(result, $"Deposit of {model.Amount:N2} posted to {model.AccountNo!.Trim().ToUpperInvariant()}.");
            ModelState.AddModelError(string.Empty, result.Error!);
        }

        await FillCashListsAsync(model, org.Id, TransactionOptions.Credit);
        return View(model);
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  Withdrawal (and withdrawal requests)
    // ═════════════════════════════════════════════════════════════════════════

    [HttpGet]
    public async Task<IActionResult> Withdrawal(string? accountNo)
    {
        SetNav("Withdrawal");
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        var model = new CashPostingFormModel { Kind = "Withdrawal", AccountNo = accountNo, ValueDate = Today() };
        await FillCashListsAsync(model, org.Id, TransactionOptions.Debit);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdrawal(CashPostingFormModel model)
    {
        SetNav("Withdrawal");
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        model.Kind = "Withdrawal";
        model.ValueDate ??= Today();

        if (ModelState.IsValid)
        {
            var code = await _db.TransactionCodes.FirstOrDefaultAsync(c => c.Id == model.TransactionCodeId
                && c.OrganizationId == org.Id && !c.IsDeleted && c.IsActive && c.DrCr == TransactionOptions.Debit);

            if (code is { RequiresApproval: true })
            {
                // Needs an approver first; the teller pays it out afterwards from the requests page.
                var error = await CreateWithdrawalRequestAsync(org.Id, code, model);
                if (error == null)
                {
                    TempData["Success"] = "This transaction code requires approval, so a withdrawal request was created. Once it is approved you can pay it out from Withdrawal Requests.";
                    return RedirectToAction(nameof(WithdrawalRequests));
                }
                ModelState.AddModelError(string.Empty, error);
            }
            else
            {
                var result = await _posting.PostAsync(new PostingRequest
                {
                    OrganizationId = org.Id, PostedBy = GetUserId(), ValueDate = model.ValueDate.Value,
                    DebitCodeId = model.TransactionCodeId, Narration = model.Narration, ContraGlAccountNo = model.CashGlAccountNo,
                    TellerRef = model.TellerRef, Legs = new[] { new PostingLeg(model.AccountNo!, TransactionOptions.Debit, model.Amount) }
                });
                if (result.Success) return Posted(result, $"Withdrawal of {model.Amount:N2} posted on {model.AccountNo!.Trim().ToUpperInvariant()}.");
                ModelState.AddModelError(string.Empty, result.Error!);
            }
        }

        await FillCashListsAsync(model, org.Id, TransactionOptions.Debit);
        return View(model);
    }

    private async Task<string?> CreateWithdrawalRequestAsync(Guid orgId, TransactionCode code, CashPostingFormModel m)
    {
        if (decimal.Round(m.Amount, 2) != m.Amount) return "Use at most two decimal places.";
        var no = m.AccountNo!.Trim().ToUpperInvariant();
        var account = await _db.MemberAccounts.FirstOrDefaultAsync(a => a.OrganizationId == orgId && !a.IsDeleted && a.AccountNo == no);
        if (account == null) return $"Account {no} was not found.";
        if (account.Status != AccountOptions.Active) return $"Account {no} is {account.Status.ToLowerInvariant()} and cannot be debited.";
        if (m.Amount > account.AvailableBalance) return $"Account {no} has only {account.AvailableBalance:N2} available.";

        _db.WithdrawalRequests.Add(new WithdrawalRequest(orgId, account.BranchId, account.Id, code.Id, m.Amount, GetUserId(), Clean(m.Narration)));
        await _db.SaveChangesAsync();
        return null;
    }

    public async Task<IActionResult> WithdrawalRequests(string? status, int page = 1)
    {
        SetNav("WithdrawalRequests");
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var query = _db.WithdrawalRequests.Where(w => w.OrganizationId == org.Id && !w.IsDeleted);
        if (!string.IsNullOrWhiteSpace(status) && TransactionOptions.WithdrawalStatuses.Contains(status))
            query = query.Where(w => w.Status == status);

        var total = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        page = Math.Clamp(page, 1, totalPages);
        var rows = await query.OrderByDescending(w => w.CreatedAt).Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();

        return View(new WithdrawalRequestListViewModel
        {
            Status = status, Page = page, TotalPages = totalPages, TotalCount = total,
            Statuses = TransactionOptions.WithdrawalStatuses.Select(s => new SelectListItem(char.ToUpper(s[0]) + s[1..].ToLower(), s)).ToList(),
            CashGLs = await CashGlItemsAsync(org.Id),
            Rows = await ToRowsAsync(rows)
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveWithdrawal(string id, string? remarks)
    {
        var (w, redirect) = await LoadRequestAsync(id);
        if (w == null) return redirect!;

        if (w.Status != TransactionOptions.Pending)
            TempData["Error"] = "This request has already been reviewed.";
        else if (w.RequestedBy == GetUserId())
            TempData["Error"] = "You cannot approve a withdrawal you requested yourself. Another user must approve it.";
        else
        {
            w.Approve(GetUserId(), Clean(remarks));
            await _db.SaveChangesAsync();
            TempData["Success"] = "Withdrawal approved. It can now be paid out.";
        }
        return RedirectToAction(nameof(WithdrawalRequests));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectWithdrawal(string id, string? remarks)
    {
        var (w, redirect) = await LoadRequestAsync(id);
        if (w == null) return redirect!;

        if (w.Status != TransactionOptions.Pending)
            TempData["Error"] = "This request has already been reviewed.";
        else if (string.IsNullOrWhiteSpace(remarks))
            TempData["Error"] = "Please give a reason for rejecting the request.";
        else
        {
            w.Reject(GetUserId(), remarks.Trim());
            await _db.SaveChangesAsync();
            TempData["Success"] = "Withdrawal rejected.";
        }
        return RedirectToAction(nameof(WithdrawalRequests));
    }

    /// <summary>The teller pays an approved withdrawal: the debit is posted and the request completed in one step.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PayWithdrawal(string id, string? cashGlAccountNo)
    {
        var (w, redirect) = await LoadRequestAsync(id);
        if (w == null) return redirect!;
        if (w.Status != TransactionOptions.Approved)
        {
            TempData["Error"] = "Only an approved withdrawal can be paid out.";
            return RedirectToAction(nameof(WithdrawalRequests));
        }

        var account = await _db.MemberAccounts.Where(a => a.Id == w.AccountId).Select(a => a.AccountNo).FirstAsync();
        var result = await _posting.PostAsync(new PostingRequest
        {
            OrganizationId = w.OrganizationId, PostedBy = GetUserId(), ValueDate = Today(),
            DebitCodeId = w.TransactionCodeId, Narration = w.Purpose ?? "Withdrawal", ContraGlAccountNo = cashGlAccountNo,
            WithdrawalRequestId = w.Id, Legs = new[] { new PostingLeg(account, TransactionOptions.Debit, w.Amount) }
        });

        if (!result.Success)
        {
            TempData["Error"] = result.Error;
            return RedirectToAction(nameof(WithdrawalRequests));
        }
        return Posted(result, $"Withdrawal of {w.Amount:N2} paid out from {account}.");
    }

    private async Task<(WithdrawalRequest? Request, IActionResult? Redirect)> LoadRequestAsync(string id)
    {
        var org = await GetOrgAsync();
        if (org == null) return (null, RedirectToAction("Index", "Organization"));
        if (!TryDecodeId(id, out var requestId)) return (null, NotFound());
        var w = await _db.WithdrawalRequests.FirstOrDefaultAsync(x => x.Id == requestId && x.OrganizationId == org.Id && !x.IsDeleted);
        return w == null ? (null, NotFound()) : (w, null);
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  Retail posting (account to account)
    // ═════════════════════════════════════════════════════════════════════════

    [HttpGet]
    public async Task<IActionResult> RetailPosting()
    {
        SetNav("RetailPosting");
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        var model = new RetailPostingFormModel { ValueDate = Today() };
        await FillRetailListsAsync(model, org.Id);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RetailPosting(RetailPostingFormModel model)
    {
        SetNav("RetailPosting");
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        model.ValueDate ??= Today();

        if (ModelState.IsValid)
        {
            var result = await _posting.PostAsync(new PostingRequest
            {
                OrganizationId = org.Id, PostedBy = GetUserId(), ValueDate = model.ValueDate.Value,
                DebitCodeId = model.DebitCodeId, CreditCodeId = model.CreditCodeId, Narration = model.Narration, TellerRef = model.TellerRef,
                Legs = new[]
                {
                    new PostingLeg(model.FromAccountNo!, TransactionOptions.Debit, model.Amount),
                    new PostingLeg(model.ToAccountNo!, TransactionOptions.Credit, model.Amount)
                }
            });
            if (result.Success)
                return Posted(result, $"{model.Amount:N2} moved from {model.FromAccountNo!.Trim().ToUpperInvariant()} to {model.ToAccountNo!.Trim().ToUpperInvariant()}.");
            ModelState.AddModelError(string.Empty, result.Error!);
        }

        await FillRetailListsAsync(model, org.Id);
        return View(model);
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  Mixed posting (any number of debits and credits)
    // ═════════════════════════════════════════════════════════════════════════

    [HttpGet]
    public async Task<IActionResult> MixedPosting()
    {
        SetNav("MixedPosting");
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        var model = new MixedPostingFormModel
        {
            ValueDate = Today(),
            Lines = { new PostingLineModel { DrCr = "DR" }, new PostingLineModel { DrCr = "CR" } }
        };
        await FillMixedListsAsync(model, org.Id);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MixedPosting(MixedPostingFormModel model)
    {
        SetNav("MixedPosting");
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        model.ValueDate ??= Today();

        // Rows the teller left completely blank are ignored.
        model.Lines = model.Lines.Where(l => !string.IsNullOrWhiteSpace(l.AccountNo) || l.Amount != 0 || !string.IsNullOrWhiteSpace(l.Narration)).ToList();
        if (model.Lines.Count == 0) ModelState.AddModelError(string.Empty, "Add at least one line.");
        if (model.Lines.Count == 1 && string.IsNullOrWhiteSpace(model.ContraGlAccountNo))
            ModelState.AddModelError(nameof(model.ContraGlAccountNo), "A single line needs a cash / contra account to post against.");

        if (ModelState.IsValid)
        {
            var result = await _posting.PostAsync(new PostingRequest
            {
                OrganizationId = org.Id, PostedBy = GetUserId(), ValueDate = model.ValueDate.Value,
                DebitCodeId = model.DebitCodeId, CreditCodeId = model.CreditCodeId, Narration = model.Narration,
                ContraGlAccountNo = model.ContraGlAccountNo, TellerRef = model.TellerRef,
                Legs = model.Lines.Select(l => new PostingLeg(l.AccountNo ?? "", l.DrCr, l.Amount, Clean(l.Narration))).ToList()
            });
            if (result.Success) return Posted(result, $"{result.Transactions.Count} line(s) posted.");
            ModelState.AddModelError(string.Empty, result.Error!);
        }

        if (model.Lines.Count < 2) while (model.Lines.Count < 2) model.Lines.Add(new PostingLineModel { DrCr = model.Lines.Count == 0 ? "DR" : "CR" });
        await FillMixedListsAsync(model, org.Id);
        return View(model);
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  Bulk upload
    // ═════════════════════════════════════════════════════════════════════════

    [HttpGet]
    public async Task<IActionResult> BulkUpload()
    {
        SetNav("BulkUpload");
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        var model = new BulkUploadFormModel { ValueDate = Today() };
        await FillBulkListsAsync(model, org.Id);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkUpload(BulkUploadFormModel model)
    {
        SetNav("BulkUpload");
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        model.ValueDate ??= Today();
        model.FileErrors = new List<string>();

        List<PostingLeg>? legs = null;
        if (model.File == null || model.File.Length == 0)
            ModelState.AddModelError(nameof(model.File), "Please choose a CSV file.");
        else if (model.File.Length > MaxUploadBytes)
            ModelState.AddModelError(nameof(model.File), "The file is too large (1 MB at most).");
        else
        {
            using var reader = new StreamReader(model.File.OpenReadStream());
            var (parsed, errors) = PostingCsvParser.Parse(reader);
            if (errors.Count > 0) model.FileErrors = errors; else legs = parsed;
        }
        if (string.IsNullOrWhiteSpace(model.Narration) && legs != null && legs.Any(l => string.IsNullOrWhiteSpace(l.Narration)))
            ModelState.AddModelError(nameof(model.Narration), "Give a default narration for the rows that have none.");

        if (ModelState.IsValid && legs != null)
        {
            var result = await _posting.PostAsync(new PostingRequest
            {
                OrganizationId = org.Id, PostedBy = GetUserId(), ValueDate = model.ValueDate.Value,
                DebitCodeId = model.DebitCodeId, CreditCodeId = model.CreditCodeId, Narration = model.Narration,
                ContraGlAccountNo = model.ContraGlAccountNo, TellerRef = model.TellerRef, Legs = legs
            });
            if (result.Success) return Posted(result, $"{result.Transactions.Count} line(s) posted from the file.");
            ModelState.AddModelError(string.Empty, result.Error!);
        }

        await FillBulkListsAsync(model, org.Id);
        return View(model);
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  Account lookup (for the forms)
    // ═════════════════════════════════════════════════════════════════════════

    [HttpGet]
    public async Task<IActionResult> AccountLookup(string? accountNo)
    {
        var org = await GetOrgAsync();
        var no = accountNo?.Trim().ToUpperInvariant();
        if (org == null || string.IsNullOrEmpty(no)) return Json(new { found = false });

        var a = await _db.MemberAccounts.Include(x => x.Customer)
            .Where(x => x.OrganizationId == org.Id && !x.IsDeleted && x.AccountNo == no).FirstOrDefaultAsync();
        return a == null ? Json(new { found = false }) : Json(new
        {
            found = true, name = a.AccountName, customer = a.Customer.DisplayName, status = a.Status, currency = a.Currency,
            book = a.BookBalance, available = a.AvailableBalance
        });
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  Helpers
    // ═════════════════════════════════════════════════════════════════════════

    /// <summary>After a successful posting, show its receipt.</summary>
    private IActionResult Posted(PostingResult result, string message)
    {
        TempData["Success"] = message;
        return RedirectToAction("View", "Transaction", new { id = _enc.Encrypt(result.Transactions[0].Id.ToString()) });
    }

    private async Task<List<SelectListItem>> CodeItemsAsync(Guid orgId, string drCr)
        => await _db.TransactionCodes
            .Where(c => c.OrganizationId == orgId && !c.IsDeleted && c.IsActive && c.DrCr == drCr).OrderBy(c => c.Code)
            .Select(c => new SelectListItem(c.Code + " — " + c.Name + (c.RequiresApproval ? " (needs approval)" : ""), c.Id.ToString()))
            .ToListAsync();

    /// <summary>Cash and other asset accounts that may be posted to directly (control accounts are excluded).</summary>
    private async Task<List<SelectListItem>> CashGlItemsAsync(Guid orgId)
        => await _db.GLAccounts
            .Where(g => g.OrganizationId == orgId && !g.IsDeleted && g.IsActive && !g.IsControlAccount && g.AccountType == "ASSET")
            .OrderBy(g => g.AccountNumber)
            .Select(g => new SelectListItem(g.AccountNumber + " — " + g.AccountName, g.AccountNumber)).ToListAsync();

    /// <summary>Any active, non-control GL account: a one-sided bulk posting may be balanced by income, expense etc.</summary>
    private async Task<List<SelectListItem>> ContraGlItemsAsync(Guid orgId)
        => await _db.GLAccounts
            .Where(g => g.OrganizationId == orgId && !g.IsDeleted && g.IsActive && !g.IsControlAccount)
            .OrderBy(g => g.AccountNumber)
            .Select(g => new SelectListItem(g.AccountNumber + " — " + g.AccountName, g.AccountNumber)).ToListAsync();

    private async Task FillCashListsAsync(CashPostingFormModel m, Guid orgId, string drCr)
    {
        m.Codes = await CodeItemsAsync(orgId, drCr);
        m.CashGLs = await CashGlItemsAsync(orgId);
        if (drCr == TransactionOptions.Debit)
        {
            var rows = await _db.WithdrawalRequests.Where(w => w.OrganizationId == orgId && !w.IsDeleted && w.Status == TransactionOptions.Approved)
                .OrderBy(w => w.CreatedAt).ToListAsync();
            m.ReadyToPay = await ToRowsAsync(rows);
        }
    }

    private async Task FillRetailListsAsync(RetailPostingFormModel m, Guid orgId)
    {
        m.DebitCodes = await CodeItemsAsync(orgId, TransactionOptions.Debit);
        m.CreditCodes = await CodeItemsAsync(orgId, TransactionOptions.Credit);
    }

    private async Task FillMixedListsAsync(MixedPostingFormModel m, Guid orgId)
    {
        m.DebitCodes = await CodeItemsAsync(orgId, TransactionOptions.Debit);
        m.CreditCodes = await CodeItemsAsync(orgId, TransactionOptions.Credit);
        m.ContraGLs = await ContraGlItemsAsync(orgId);
    }

    private async Task FillBulkListsAsync(BulkUploadFormModel m, Guid orgId)
    {
        m.DebitCodes = await CodeItemsAsync(orgId, TransactionOptions.Debit);
        m.CreditCodes = await CodeItemsAsync(orgId, TransactionOptions.Credit);
        m.ContraGLs = await ContraGlItemsAsync(orgId);
    }

    private async Task<List<WithdrawalRequestRowViewModel>> ToRowsAsync(List<WithdrawalRequest> rows)
    {
        var accountIds = rows.Select(r => r.AccountId).Distinct().ToList();
        var accounts = await _db.MemberAccounts.Where(a => accountIds.Contains(a.Id)).ToDictionaryAsync(a => a.Id);
        var codeIds = rows.Select(r => r.TransactionCodeId).Distinct().ToList();
        var codes = await _db.TransactionCodes.Where(c => codeIds.Contains(c.Id)).ToDictionaryAsync(c => c.Id);
        var userIds = rows.SelectMany(r => new[] { (Guid?)r.RequestedBy, r.ApprovedBy }).Where(x => x != null).Select(x => x!.Value).Distinct().ToList();
        var users = await _db.AppUsers.Where(u => userIds.Contains(u.Id)).Select(u => new { u.Id, u.FirstName, u.LastName }).ToListAsync();
        string? Name(Guid? id) => id == null ? null : users.Where(u => u.Id == id).Select(u => $"{u.FirstName} {u.LastName}".Trim()).FirstOrDefault();

        return rows.Select(r => new WithdrawalRequestRowViewModel
        {
            EncryptedId = _enc.Encrypt(r.Id.ToString()),
            AccountNo = accounts.TryGetValue(r.AccountId, out var a) ? a.AccountNo : "",
            AccountName = a?.AccountName ?? "", Currency = a?.Currency ?? "",
            Amount = r.Amount, Purpose = r.Purpose, Status = r.Status,
            CodeLabel = codes.TryGetValue(r.TransactionCodeId, out var c) ? c.Code + " — " + c.Name : "",
            RequestedByName = Name(r.RequestedBy), ReviewedByName = Name(r.ApprovedBy), ReviewRemarks = r.ReviewRemarks,
            CompletedTransactionRef = r.CompletedTransactionRef, CreatedAt = r.CreatedAt
        }).ToList();
    }
}
