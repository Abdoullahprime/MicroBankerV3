// src/Microbanker.Web/Controllers/AccountController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Entities.Accounts;
using Microbanker.Domain.Entities.Customers;
using Microbanker.Domain.Entities.Organization;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Models;
using Microbanker.Web.Services;

namespace Microbanker.Web.Controllers;

public class AccountController : SetupControllerBase
{
    private const int PageSize = 25;

    public AccountController(MicrobankerDbContext db, IEncryptionService enc) : base(db, enc) { }

    private void SetNav(string page = "Accounts")
    {
        ViewData["ActiveGroup"] = "Banking";
        ViewData["ActivePage"]  = page;
    }

    private IQueryable<MemberAccount> Detailed(Guid orgId) => _db.MemberAccounts
        .Include(a => a.Customer).Include(a => a.Branch).Include(a => a.ProductDefinition)
        .Where(a => a.OrganizationId == orgId && !a.IsDeleted);

    // ── List ──────────────────────────────────────────────────────────────────

    public async Task<IActionResult> Index(string? status, string? type, string? q, int page = 1)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var query = _db.MemberAccounts.Include(a => a.Customer)
            .Where(a => a.OrganizationId == org.Id && !a.IsDeleted);

        if (!string.IsNullOrWhiteSpace(status) && AccountOptions.AccountStatuses.Contains(status))
            query = query.Where(a => a.Status == status);
        if (!string.IsNullOrWhiteSpace(type) && AccountOptions.AccountTypes.Contains(type))
            query = query.Where(a => a.AccountType == type);

        q = q?.Trim();
        if (!string.IsNullOrEmpty(q))
            query = query.Where(a => a.AccountNo.Contains(q) || a.AccountName.Contains(q)
                || a.Customer.CustomerNo.Contains(q) || a.Customer.FirstName.Contains(q) || a.Customer.LastName.Contains(q)
                || (a.Customer.BusinessName != null && a.Customer.BusinessName.Contains(q))
                || (a.Customer.GroupName != null && a.Customer.GroupName.Contains(q)));

        var total = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        page = Math.Clamp(page, 1, totalPages);

        var rows = await query.OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();

        return View(new AccountListViewModel
        {
            Status = status, Type = type, Q = q, Page = page, TotalPages = totalPages, TotalCount = total,
            Statuses = AccountOptions.AccountStatuses.Select(s => new SelectListItem(Pretty(s), s)).ToList(),
            Types = AccountOptions.AccountTypes.Select(s => new SelectListItem(Pretty(s), s)).ToList(),
            Rows = rows.Select(a => new AccountRowViewModel
            {
                EncryptedId = _enc.Encrypt(a.Id.ToString()),
                AccountNo = a.AccountNo, AccountName = a.AccountName, CustomerNo = a.Customer.CustomerNo,
                AccountType = a.AccountType, Currency = a.Currency, BookBalance = a.BookBalance,
                AvailableBalance = a.AvailableBalance, Status = a.Status
            }).ToList()
        });
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Create(string? customerNo)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var model = new AccountFormModel { CustomerNo = customerNo, OpenDate = Today() };
        await FillListsAsync(model, org.Id);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AccountFormModel model)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        model.IsEdit = false;
        model.TermsEditable = true;
        model.AccountType = (model.AccountType ?? "").Trim().ToUpperInvariant();
        model.Currency = (model.Currency ?? "").Trim().ToUpperInvariant();
        model.OpenDate ??= Today();

        var customer = await ResolveCustomerAsync(org.Id, model.CustomerNo);
        if (customer == null)
            ModelState.AddModelError(nameof(model.CustomerNo), "No customer with that number exists.");
        else if (customer.Status != CustomerOptions.Active)
            ModelState.AddModelError(nameof(model.CustomerNo), $"Customer {customer.CustomerNo} is {customer.Status.ToLowerInvariant()}; only active customers can open accounts.");

        if (!AccountOptions.AccountTypes.Contains(model.AccountType))
            ModelState.AddModelError(nameof(model.AccountType), "Please select a valid account type.");

        await ValidateAsync(model, org.Id, validateTerms: true);

        if (!ModelState.IsValid)
        {
            model.CustomerName = customer?.DisplayName;
            await FillListsAsync(model, org.Id);
            return View(model);
        }

        var account = await AddAccountAsync(org.Id, customer!, model);
        TempData["Success"] = $"Account {account.AccountNo} created and is awaiting approval.";
        return RedirectToAction("View", new { id = _enc.Encrypt(account.Id.ToString()) });
    }

    /// <summary>JSON helper for the customer-number box on the create form.</summary>
    [HttpGet]
    public async Task<IActionResult> LookupCustomer(string? customerNo)
    {
        var org = await GetOrgAsync();
        if (org == null) return Json(new { found = false });
        var c = await ResolveCustomerAsync(org.Id, customerNo);
        return c == null
            ? Json(new { found = false })
            : Json(new { found = true, name = c.DisplayName, status = c.Status, branchId = c.BranchId });
    }

    // ── Edit ──────────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var accountId)) return NotFound();

        var account = await Detailed(org.Id).FirstOrDefaultAsync(a => a.Id == accountId);
        if (account == null) return NotFound();

        var gate = await EditGateAsync(account);
        if (gate != null)
        {
            TempData["Error"] = gate;
            return RedirectToAction("View", new { id });
        }

        var model = new AccountFormModel
        {
            EncryptedId = id, IsEdit = true, TermsEditable = account.Status == AccountOptions.Pending,
            AccountNo = account.AccountNo, CustomerName = account.Customer.DisplayName,
            CustomerLabel = $"{account.Customer.CustomerNo} — {account.Customer.DisplayName}",
            CustomerNo = account.Customer.CustomerNo, BranchId = account.BranchId, AccountType = account.AccountType,
            ProductDefinitionId = account.ProductDefinitionId, AccountName = account.AccountName, Currency = account.Currency,
            OpeningBalance = account.OpeningBalance, OpenDate = account.OpenDate,
            AtmAccountNo = account.AtmAccountNo, AtmDailyLimit = account.AtmDailyLimit,
            SectorId = account.SectorId, SubSectorId = account.SubSectorId,
            GLAssetControl = account.GLAssetControl, GLLiabilityControl = account.GLLiabilityControl,
            GLIncomeAccount = account.GLIncomeAccount, GLExpenseAccount = account.GLExpenseAccount,
            GLChargeAccount = account.GLChargeAccount, GLFeeAccount = account.GLFeeAccount
        };
        await FillListsAsync(model, org.Id);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, AccountFormModel model)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var accountId)) return NotFound();

        var account = await Detailed(org.Id).FirstOrDefaultAsync(a => a.Id == accountId);
        if (account == null) return NotFound();

        var gate = await EditGateAsync(account);
        if (gate != null)
        {
            TempData["Error"] = gate;
            return RedirectToAction("View", new { id });
        }

        var pending = account.Status == AccountOptions.Pending;
        // Fixed at creation (or at approval): whatever the browser posted is ignored.
        model.IsEdit = true;
        model.TermsEditable = pending;
        model.AccountType = account.AccountType;
        model.BranchId = account.BranchId;
        model.CustomerNo = account.Customer.CustomerNo;
        if (!pending)
        {
            model.ProductDefinitionId = account.ProductDefinitionId;
            model.Currency = account.Currency;
            model.OpeningBalance = account.OpeningBalance;
            model.OpenDate = account.OpenDate;
        }
        model.Currency = (model.Currency ?? "").Trim().ToUpperInvariant();
        model.OpenDate ??= account.OpenDate;

        await ValidateAsync(model, org.Id, validateTerms: pending);

        if (!ModelState.IsValid)
        {
            model.EncryptedId = id;
            model.AccountNo = account.AccountNo;
            model.CustomerName = account.Customer.DisplayName;
            model.CustomerLabel = $"{account.Customer.CustomerNo} — {account.Customer.DisplayName}";
            await FillListsAsync(model, org.Id);
            return View(model);
        }

        if (pending)
            account.UpdateTerms(model.ProductDefinitionId!.Value, model.Currency, model.OpeningBalance, model.OpenDate!.Value);

        account.Update(
            string.IsNullOrWhiteSpace(model.AccountName) ? account.Customer.DisplayName : model.AccountName.Trim(),
            Clean(model.AtmAccountNo), model.AtmDailyLimit, model.SectorId, model.SubSectorId,
            Clean(model.GLAssetControl), Clean(model.GLLiabilityControl), Clean(model.GLIncomeAccount),
            Clean(model.GLExpenseAccount), Clean(model.GLChargeAccount), Clean(model.GLFeeAccount));

        if (!pending)
        {
            // The approved request is single-use.
            var request = await _db.AccountEditRequests.FirstOrDefaultAsync(r => r.AccountId == accountId && r.Status == AccountOptions.Approved);
            request?.Complete();
        }

        if (!await TrySaveAsync()) return RedirectToAction("View", new { id });
        TempData["Success"] = "Account updated successfully.";
        return RedirectToAction("View", new { id });
    }

    // ── View ──────────────────────────────────────────────────────────────────
    // Named Details in code because Controller.View(...) would clash; the route action is still "View".

    [HttpGet, ActionName("View")]
    public async Task<IActionResult> Details(string id)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var accountId)) return NotFound();

        var account = await Detailed(org.Id).FirstOrDefaultAsync(a => a.Id == accountId);
        if (account == null) return NotFound();

        return View("View", await BuildDetailAsync(account, id));
    }

    // ── Approve / lifecycle ───────────────────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Approve(string id) => ChangeAsync(id, a =>
    {
        a.Approve(GetUserId());
        return $"Account {a.AccountNo} approved and is now active.";
    });

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Freeze(string id) => ChangeAsync(id, a => { a.Freeze(); return $"Account {a.AccountNo} frozen."; });

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Unfreeze(string id) => ChangeAsync(id, a => { a.Unfreeze(); return $"Account {a.AccountNo} unfrozen."; });

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Close(string id) => ChangeAsync(id, a => { a.Close(); return $"Account {a.AccountNo} closed."; });

    // ── Delete (pending only) ─────────────────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var accountId)) return NotFound();

        var account = await _db.MemberAccounts.FirstOrDefaultAsync(a => a.Id == accountId && a.OrganizationId == org.Id && !a.IsDeleted);
        if (account == null) return NotFound();

        if (account.Status != AccountOptions.Pending)
        {
            TempData["Error"] = "Only a pending (unapproved) account can be deleted. Close an open account instead.";
            return RedirectToAction("View", new { id });
        }

        account.SoftDelete();
        if (!await TrySaveAsync()) return RedirectToAction("View", new { id });
        TempData["Success"] = $"Account {account.AccountNo} deleted.";
        return RedirectToAction(nameof(Index));
    }

    // ── Edit requests (maker / checker for approved accounts) ─────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestEdit(string id, string? reason)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var accountId)) return NotFound();

        var account = await _db.MemberAccounts.FirstOrDefaultAsync(a => a.Id == accountId && a.OrganizationId == org.Id && !a.IsDeleted);
        if (account == null) return NotFound();

        if (account.Status == AccountOptions.Pending)
            TempData["Error"] = "A pending account can be edited directly.";
        else if (account.Status == AccountOptions.Closed)
            TempData["Error"] = "A closed account cannot be edited.";
        else if (string.IsNullOrWhiteSpace(reason))
            TempData["Error"] = "Please give a reason for the change.";
        else if (await _db.AccountEditRequests.AnyAsync(r => r.AccountId == accountId
                    && (r.Status == AccountOptions.Pending || r.Status == AccountOptions.Approved)))
            TempData["Error"] = "This account already has an open edit request.";
        else
        {
            _db.AccountEditRequests.Add(new AccountEditRequest(org.Id, accountId, GetUserId(), reason.Trim()));
            account.MarkEditRequested(reason.Trim());
            if (!await TrySaveAsync()) return RedirectToAction("View", new { id });
            TempData["Success"] = "Edit request submitted for approval.";
        }
        return RedirectToAction("View", new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> ApproveEditRequest(string id, string? remarks) => ReviewEditAsync(id, remarks, approve: true);

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> RejectEditRequest(string id, string? remarks) => ReviewEditAsync(id, remarks, approve: false);

    private async Task<IActionResult> ReviewEditAsync(string id, string? remarks, bool approve)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var accountId)) return NotFound();

        var account = await _db.MemberAccounts.FirstOrDefaultAsync(a => a.Id == accountId && a.OrganizationId == org.Id && !a.IsDeleted);
        if (account == null) return NotFound();

        var request = await _db.AccountEditRequests.FirstOrDefaultAsync(r => r.AccountId == accountId && r.Status == AccountOptions.Pending);
        if (request == null)
        {
            TempData["Error"] = "There is no pending edit request for this account.";
            return RedirectToAction("View", new { id });
        }
        if (!approve && string.IsNullOrWhiteSpace(remarks))
        {
            TempData["Error"] = "Please give a reason for rejecting the request.";
            return RedirectToAction("View", new { id });
        }

        var note = string.IsNullOrWhiteSpace(remarks) ? null : remarks.Trim();
        if (approve) request.Approve(GetUserId(), note); else request.Reject(GetUserId(), note);
        account.ClearEditRequest();

        if (!await TrySaveAsync()) return RedirectToAction("View", new { id });
        TempData["Success"] = approve ? "Edit request approved. The account can now be edited once." : "Edit request rejected.";
        return RedirectToAction("View", new { id });
    }

    // ── Block / unblock ───────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> BlockAmount(string id)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var accountId)) return NotFound();

        var account = await _db.MemberAccounts.FirstOrDefaultAsync(a => a.Id == accountId && a.OrganizationId == org.Id && !a.IsDeleted);
        if (account == null) return NotFound();

        return View(BlockModel(account, id));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> BlockAmount(string id, decimal amount) => AdjustBlockAsync(id, amount, block: true);

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> UnblockAmount(string id, decimal amount) => AdjustBlockAsync(id, amount, block: false);

    private async Task<IActionResult> AdjustBlockAsync(string id, decimal amount, bool block)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var accountId)) return NotFound();

        var account = await _db.MemberAccounts.FirstOrDefaultAsync(a => a.Id == accountId && a.OrganizationId == org.Id && !a.IsDeleted);
        if (account == null) return NotFound();

        if (amount <= 0 || decimal.Round(amount, 2) != amount)
        {
            TempData["Error"] = "Enter an amount greater than zero with at most two decimal places.";
            return RedirectToAction(nameof(BlockAmount), new { id });
        }

        try
        {
            if (block) account.BlockAmount(amount); else account.UnblockAmount(amount);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(BlockAmount), new { id });
        }

        if (!await TrySaveAsync()) return RedirectToAction(nameof(BlockAmount), new { id });
        TempData["Success"] = block
            ? $"{amount:N2} {account.Currency} blocked on {account.AccountNo}."
            : $"{amount:N2} {account.Currency} released on {account.AccountNo}.";
        return RedirectToAction(nameof(BlockAmount), new { id });
    }

    // ── Balance enquiry / mini statement ──────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> BalanceEnquiry(string? accountNo)
    {
        SetNav("BalanceEnquiry");
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var vm = new BalanceEnquiryViewModel { AccountNo = accountNo?.Trim() };
        if (!string.IsNullOrWhiteSpace(vm.AccountNo))
        {
            vm.Searched = true;
            var no = vm.AccountNo.ToUpperInvariant();
            var account = await Detailed(org.Id).FirstOrDefaultAsync(a => a.AccountNo == no);
            if (account != null)
                vm.Account = await BuildDetailAsync(account, _enc.Encrypt(account.Id.ToString()), includeAudit: false);
        }
        return View(vm);
    }

    /// <summary>The account with its ten most recent transactions, newest first.</summary>
    [HttpGet]
    public async Task<IActionResult> MiniStatement(string id)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var accountId)) return NotFound();

        var account = await Detailed(org.Id).FirstOrDefaultAsync(a => a.Id == accountId);
        if (account == null) return NotFound();

        var lines = await _db.Transactions
            .Where(t => t.OrganizationId == org.Id && t.AccountId == account.Id)
            .OrderByDescending(t => t.PostingDate).ThenByDescending(t => t.TransactionRef)
            .Take(10)
            .Select(t => new StatementLineViewModel
            {
                ValueDate = t.ValueDate, TransactionRef = t.TransactionRef, Narration = t.Narration, DrCr = t.DrCr,
                Amount = t.Amount, BalanceAfter = t.BalanceAfter, Status = t.Status, IsReversal = t.IsReversal
            }).ToListAsync();

        return View(new MiniStatementViewModel { Account = await BuildDetailAsync(account, id, includeAudit: false), Lines = lines });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static DateOnly Today() => DateOnly.FromDateTime(DateTime.UtcNow);
    private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    private static string Pretty(string code) => string.Join(' ', code.Split('_').Select(w => char.ToUpper(w[0]) + w[1..].ToLower()));

    private Task<Customer?> ResolveCustomerAsync(Guid orgId, string? customerNo)
    {
        var no = customerNo?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(no)) return Task.FromResult<Customer?>(null);
        return _db.Customers.FirstOrDefaultAsync(c => c.OrganizationId == orgId && !c.IsDeleted && c.CustomerNo == no);
    }

    /// <summary>Null when the account may be edited now, otherwise the reason it may not.</summary>
    private async Task<string?> EditGateAsync(MemberAccount account)
    {
        if (account.Status == AccountOptions.Pending) return null;
        if (account.Status == AccountOptions.Closed) return "A closed account cannot be edited.";
        var approved = await _db.AccountEditRequests.AnyAsync(r => r.AccountId == account.Id && r.Status == AccountOptions.Approved);
        return approved ? null : "This account is approved. Submit an edit request and have it approved before editing.";
    }

    private async Task<IActionResult> ChangeAsync(string id, Func<MemberAccount, string> change)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var accountId)) return NotFound();

        var account = await _db.MemberAccounts.FirstOrDefaultAsync(a => a.Id == accountId && a.OrganizationId == org.Id && !a.IsDeleted);
        if (account == null) return NotFound();

        try
        {
            var message = change(account);
            if (!await TrySaveAsync()) return RedirectToAction("View", new { id });
            TempData["Success"] = message;
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction("View", new { id });
    }

    /// <summary>Saves, turning a concurrent change by someone else into a friendly message.</summary>
    private async Task<bool> TrySaveAsync()
    {
        try
        {
            await _db.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["Error"] = "This account was changed by someone else while you were working. Please review it and try again.";
            return false;
        }
    }

    private BlockAmountViewModel BlockModel(MemberAccount a, string id) => new()
    {
        EncryptedId = id, AccountNo = a.AccountNo, AccountName = a.AccountName, Status = a.Status, Currency = a.Currency,
        BookBalance = a.BookBalance, BlockedAmount = a.BlockedAmount, AvailableBalance = a.AvailableBalance
    };

    private async Task<AccountDetailViewModel> BuildDetailAsync(MemberAccount a, string id, bool includeAudit = true)
    {
        var vm = new AccountDetailViewModel
        {
            EncryptedId = id, AccountNo = a.AccountNo, AccountName = a.AccountName, AccountType = a.AccountType,
            Currency = a.Currency, Status = a.Status, OpenDate = a.OpenDate, ClosedAt = a.ClosedAt,
            CustomerEncryptedId = _enc.Encrypt(a.CustomerId.ToString()), CustomerNo = a.Customer.CustomerNo,
            CustomerName = a.Customer.DisplayName, BranchName = a.Branch.Name, ProductName = a.ProductDefinition.Name,
            OpeningBalance = a.OpeningBalance, BookBalance = a.BookBalance, AvailableBalance = a.AvailableBalance,
            BlockedAmount = a.BlockedAmount, UnauthorizedDR = a.UnauthorizedDR, UnauthorizedCR = a.UnauthorizedCR,
            MonthlyDRTurnover = a.MonthlyDRTurnover, MonthlyCRTurnover = a.MonthlyCRTurnover,
            InterestAccruedDR = a.InterestAccruedDR, InterestAccruedCR = a.InterestAccruedCR,
            LastDRDate = a.LastDRDate, LastCRDate = a.LastCRDate,
            AtmAccountNo = a.AtmAccountNo, AtmDailyLimit = a.AtmDailyLimit,
            GLAssetControl = a.GLAssetControl, GLLiabilityControl = a.GLLiabilityControl, GLIncomeAccount = a.GLIncomeAccount,
            GLExpenseAccount = a.GLExpenseAccount, GLChargeAccount = a.GLChargeAccount, GLFeeAccount = a.GLFeeAccount,
            IsApproved = a.IsApproved, ApprovedAt = a.ApprovedAt
        };
        if (!includeAudit) return vm;

        if (a.SectorId != null) vm.SectorName = await _db.Sectors.Where(x => x.Id == a.SectorId).Select(x => x.Name).FirstOrDefaultAsync();
        if (a.SubSectorId != null) vm.SubSectorName = await _db.SubSectors.Where(x => x.Id == a.SubSectorId).Select(x => x.Name).FirstOrDefaultAsync();
        vm.CreatedByName = await UserNameAsync(a.CreatedByUserId);
        vm.ApprovedByName = await UserNameAsync(a.ApprovedBy);

        var request = await _db.AccountEditRequests
            .Where(r => r.AccountId == a.Id && (r.Status == AccountOptions.Pending || r.Status == AccountOptions.Approved))
            .OrderByDescending(r => r.CreatedAt).FirstOrDefaultAsync();
        if (request != null)
            vm.OpenEditRequest = new AccountEditRequestViewModel
            {
                Status = request.Status, Reason = request.RequestReason, RequestedAt = request.CreatedAt,
                RequestedByName = await UserNameAsync(request.RequestedBy),
                ReviewedByName = await UserNameAsync(request.ReviewedBy), ReviewedAt = request.ReviewedAt,
                ReviewRemarks = request.ReviewRemarks
            };
        vm.CanEditNow = a.Status == AccountOptions.Pending
            || (a.Status != AccountOptions.Closed && request?.Status == AccountOptions.Approved);
        return vm;
    }

    private async Task<string?> UserNameAsync(Guid? userId)
    {
        if (userId == null) return null;
        var u = await _db.AppUsers.Where(x => x.Id == userId).Select(x => new { x.FirstName, x.LastName }).FirstOrDefaultAsync();
        return u == null ? null : $"{u.FirstName} {u.LastName}".Trim();
    }

    // ── Dropdowns & validation ────────────────────────────────────────────────

    private async Task FillListsAsync(AccountFormModel m, Guid orgId)
    {
        m.Branches = await _db.Branches
            .Where(b => b.OrganizationId == orgId && !b.IsDeleted && b.IsActive).OrderBy(b => b.Name)
            .Select(b => new SelectListItem(b.Name + " (" + b.Code + ")", b.Id.ToString())).ToListAsync();
        m.AccountTypes = AccountOptions.AccountTypes.Select(t => new SelectListItem(Pretty(t), t)).ToList();
        m.Products = await _db.ProductDefinitions.Include(p => p.ProductType)
            .Where(p => p.OrganizationId == orgId && !p.IsDeleted && p.IsActive).OrderBy(p => p.Name)
            .Select(p => new SelectListItem(p.Name + " (" + p.Code + ") · " + p.ProductType.Name, p.Id.ToString())).ToListAsync();
        m.Sectors = await SectorItemsAsync(orgId);
        m.SubSectors = await _db.SubSectors
            .Where(x => x.OrganizationId == orgId && !x.IsDeleted && x.IsActive).OrderBy(x => x.Name)
            .Select(x => new LocationOption { Id = x.Id, ParentId = x.SectorId, Name = x.Name }).ToListAsync();

        var gls = await _db.GLAccounts
            .Where(g => g.OrganizationId == orgId && !g.IsDeleted && g.IsActive).OrderBy(g => g.AccountNumber).ToListAsync();
        List<SelectListItem> Of(string type) => gls.Where(g => g.AccountType == type)
            .Select(g => new SelectListItem($"{g.AccountNumber} — {g.AccountName}", g.AccountNumber)).ToList();
        m.AssetGLs = Of("ASSET");
        m.LiabilityGLs = Of("LIABILITY");
        m.IncomeGLs = Of("INCOME");
        m.ExpenseGLs = Of("EXPENSE");
    }

    private async Task ValidateAsync(AccountFormModel m, Guid orgId, bool validateTerms)
    {
        if (m.BranchId == null ||
            !await _db.Branches.AnyAsync(b => b.Id == m.BranchId && b.OrganizationId == orgId && !b.IsDeleted && b.IsActive))
            ModelState.AddModelError(nameof(m.BranchId), "Please select a valid branch.");

        if (validateTerms)
        {
            var product = m.ProductDefinitionId == null ? null : await _db.ProductDefinitions
                .FirstOrDefaultAsync(p => p.Id == m.ProductDefinitionId && p.OrganizationId == orgId && !p.IsDeleted && p.IsActive);
            if (product == null)
                ModelState.AddModelError(nameof(m.ProductDefinitionId), "Please select a valid product.");
            else if (m.OpeningBalance < product.MinBalance)
                ModelState.AddModelError(nameof(m.OpeningBalance), $"This product needs an opening balance of at least {product.MinBalance:N2}.");

            if (m.Currency is not { Length: 3 } || !m.Currency.All(char.IsLetter))
                ModelState.AddModelError(nameof(m.Currency), "Use a 3-letter currency code, e.g. GMD.");
            if (decimal.Round(m.OpeningBalance, 2) != m.OpeningBalance)
                ModelState.AddModelError(nameof(m.OpeningBalance), "Use at most two decimal places.");
            if (m.OpenDate > Today())
                ModelState.AddModelError(nameof(m.OpenDate), "The open date cannot be in the future.");
        }

        if (decimal.Round(m.AtmDailyLimit, 2) != m.AtmDailyLimit)
            ModelState.AddModelError(nameof(m.AtmDailyLimit), "Use at most two decimal places.");

        if (m.SectorId != null &&
            !await _db.Sectors.AnyAsync(x => x.Id == m.SectorId && x.OrganizationId == orgId && !x.IsDeleted))
            ModelState.AddModelError(nameof(m.SectorId), "Please select a valid sector.");
        if (m.SubSectorId != null &&
            !await _db.SubSectors.AnyAsync(x => x.Id == m.SubSectorId && x.OrganizationId == orgId && !x.IsDeleted && x.SectorId == m.SectorId))
            ModelState.AddModelError(nameof(m.SubSectorId), "Please select a sub-sector that belongs to the chosen sector.");

        // Each GL field must name an active account of the right type.
        await CheckGlAsync(orgId, nameof(m.GLAssetControl), m.GLAssetControl, "ASSET");
        await CheckGlAsync(orgId, nameof(m.GLLiabilityControl), m.GLLiabilityControl, "LIABILITY");
        await CheckGlAsync(orgId, nameof(m.GLIncomeAccount), m.GLIncomeAccount, "INCOME");
        await CheckGlAsync(orgId, nameof(m.GLExpenseAccount), m.GLExpenseAccount, "EXPENSE");
        await CheckGlAsync(orgId, nameof(m.GLChargeAccount), m.GLChargeAccount, "INCOME");
        await CheckGlAsync(orgId, nameof(m.GLFeeAccount), m.GLFeeAccount, "INCOME");
    }

    private async Task CheckGlAsync(Guid orgId, string field, string? number, string type)
    {
        var n = Clean(number);
        if (n == null) return;
        if (!await _db.GLAccounts.AnyAsync(g => g.OrganizationId == orgId && !g.IsDeleted && g.IsActive
                                                && g.AccountNumber == n && g.AccountType == type))
            ModelState.AddModelError(field, $"Please select an active {type.ToLowerInvariant()} account.");
    }

    // ── Account numbers ───────────────────────────────────────────────────────

    /// <summary>
    /// Next {BranchCode}-{TypePrefix}-{seq:D7}, based on the highest number ever issued for that branch and
    /// type (deleted rows included) so numbers are never reused. A unique index backs this up.
    /// </summary>
    private async Task<string> NextAccountNoAsync(Guid orgId, string branchCode, string accountType)
    {
        var prefix = $"{branchCode}-{AccountOptions.TypePrefix(accountType)}-";
        var last = await _db.MemberAccounts
            .Where(a => a.OrganizationId == orgId && a.AccountNo.StartsWith(prefix))
            .OrderByDescending(a => a.AccountNo)
            .Select(a => a.AccountNo)
            .FirstOrDefaultAsync();
        var seq = last != null && int.TryParse(last.AsSpan(prefix.Length), out var n) ? n : 0;
        return $"{prefix}{seq + 1:D7}";
    }

    private async Task<MemberAccount> AddAccountAsync(Guid orgId, Customer customer, AccountFormModel m)
    {
        var branch = await _db.Branches.FirstAsync(b => b.Id == m.BranchId);
        var name = string.IsNullOrWhiteSpace(m.AccountName) ? customer.DisplayName : m.AccountName.Trim();

        for (var attempt = 1; ; attempt++)
        {
            var account = new MemberAccount(orgId, branch.Id, customer.Id, m.ProductDefinitionId!.Value,
                await NextAccountNoAsync(orgId, branch.Code, m.AccountType), name, m.AccountType, GetUserId());
            account.UpdateTerms(m.ProductDefinitionId!.Value, m.Currency, m.OpeningBalance, m.OpenDate!.Value);
            account.Update(name, Clean(m.AtmAccountNo), m.AtmDailyLimit, m.SectorId, m.SubSectorId,
                Clean(m.GLAssetControl), Clean(m.GLLiabilityControl), Clean(m.GLIncomeAccount),
                Clean(m.GLExpenseAccount), Clean(m.GLChargeAccount), Clean(m.GLFeeAccount));
            _db.MemberAccounts.Add(account);
            try
            {
                await _db.SaveChangesAsync();
                return account;
            }
            catch (DbUpdateException ex) when (attempt < 5 && ex.InnerException is SqlException { Number: 2601 or 2627 })
            {
                _db.Entry(account).State = EntityState.Detached;
            }
        }
    }
}
