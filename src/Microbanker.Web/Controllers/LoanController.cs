// src/Microbanker.Web/Controllers/LoanController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Entities.Accounts;
using Microbanker.Domain.Entities.Customers;
using Microbanker.Domain.Entities.Loans;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Models;
using Microbanker.Web.Services;

namespace Microbanker.Web.Controllers;

/// <summary>
/// Loans and loan requests. This module tracks the loan side only: disbursing or repaying a loan does not
/// move money in a member account or the general ledger (that arrives with the posting module).
/// </summary>
public class LoanController : SetupControllerBase
{
    private const int PageSize = 25;

    public LoanController(MicrobankerDbContext db, IEncryptionService enc) : base(db, enc) { }

    private void SetNav(string page = "Loans")
    {
        ViewData["ActiveGroup"] = "Lending";
        ViewData["ActivePage"]  = page;
    }

    private IQueryable<LoanAccount> Detailed(Guid orgId) => _db.LoanAccounts
        .Include(l => l.Customer).Include(l => l.Branch).Include(l => l.ProductDefinition)
        .Where(l => l.OrganizationId == orgId && !l.IsDeleted);

    // ═════════════════════════════════════════════════════════════════════════
    //  LOANS
    // ═════════════════════════════════════════════════════════════════════════

    public async Task<IActionResult> Index(string? status, string? q, int page = 1)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var query = _db.LoanAccounts.Include(l => l.Customer).Where(l => l.OrganizationId == org.Id && !l.IsDeleted);
        if (!string.IsNullOrWhiteSpace(status) && LoanOptions.LoanStatuses.Contains(status))
            query = query.Where(l => l.Status == status);

        q = q?.Trim();
        if (!string.IsNullOrEmpty(q))
            query = query.Where(l => l.LoanNo.Contains(q) || l.LoanName.Contains(q) || l.Customer.CustomerNo.Contains(q)
                || l.Customer.FirstName.Contains(q) || l.Customer.LastName.Contains(q)
                || (l.Customer.BusinessName != null && l.Customer.BusinessName.Contains(q))
                || (l.Customer.GroupName != null && l.Customer.GroupName.Contains(q)));

        var total = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        page = Math.Clamp(page, 1, totalPages);
        var rows = await query.OrderByDescending(l => l.CreatedAt).Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();

        return View(new LoanListViewModel
        {
            Status = status, Q = q, Page = page, TotalPages = totalPages, TotalCount = total,
            Statuses = LoanOptions.LoanStatuses.Select(s => new SelectListItem(Pretty(s), s)).ToList(),
            Rows = rows.Select(l => new LoanRowViewModel
            {
                EncryptedId = _enc.Encrypt(l.Id.ToString()), LoanNo = l.LoanNo, LoanName = l.LoanName,
                CustomerNo = l.Customer.CustomerNo, Currency = l.Currency, LoanAmount = l.LoanAmount,
                OutstandingBalance = l.OutstandingBalance, Status = l.Status
            }).ToList()
        });
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Create(string? requestId, string? customerNo)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var model = new LoanFormModel { CustomerNo = customerNo };
        if (!string.IsNullOrEmpty(requestId))
        {
            if (!TryDecodeId(requestId, out var rid)) return NotFound();
            var req = await _db.LoanRequests.Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == rid && r.OrganizationId == org.Id && !r.IsDeleted);
            if (req == null) return NotFound();
            if (req.Status != LoanOptions.Approved)
            {
                TempData["Error"] = "Only an approved loan request can be turned into a loan.";
                return RedirectToAction(nameof(Requests));
            }
            model.LoanRequestId = req.Id;
            model.RequestedAmount = req.RequestedAmount;
            model.CustomerNo = req.Customer.CustomerNo;
            model.CustomerName = req.Customer.DisplayName;
            model.BranchId = req.BranchId;
            model.ProductDefinitionId = req.ProductDefinitionId;
            model.LoanAmount = req.RequestedAmount;
            model.LoanName = $"{req.Customer.DisplayName} loan";
        }
        await FillListsAsync(model, org.Id);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LoanFormModel model)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        model.IsEdit = false;
        Normalise(model);

        var customer = await ResolveCustomerAsync(org.Id, model.CustomerNo);
        if (customer == null)
            ModelState.AddModelError(nameof(model.CustomerNo), "No customer with that number exists.");
        else if (customer.Status != CustomerOptions.Active)
            ModelState.AddModelError(nameof(model.CustomerNo), $"Customer {customer.CustomerNo} is {customer.Status.ToLowerInvariant()}; only active customers can borrow.");

        LoanRequest? request = null;
        if (model.LoanRequestId != null)
        {
            request = await _db.LoanRequests.FirstOrDefaultAsync(r => r.Id == model.LoanRequestId && r.OrganizationId == org.Id && !r.IsDeleted);
            if (request == null || request.Status != LoanOptions.Approved)
                ModelState.AddModelError(string.Empty, "The loan request is no longer available.");
            else
            {
                model.RequestedAmount = request.RequestedAmount;
                if (customer != null && request.CustomerId != customer.Id)
                    ModelState.AddModelError(nameof(model.CustomerNo), "This loan request belongs to a different customer.");
                if (model.LoanAmount > request.RequestedAmount)
                    ModelState.AddModelError(nameof(model.LoanAmount), $"The loan cannot be more than the approved request ({request.RequestedAmount:N2}).");
            }
        }

        await ValidateAsync(model, org.Id, customer);

        if (!ModelState.IsValid)
        {
            model.CustomerName = customer?.DisplayName;
            await FillListsAsync(model, org.Id);
            return View(model);
        }

        // One transaction: the loan exists and its request is marked converted, or neither.
        await using var tx = await _db.Database.BeginTransactionAsync();
        var loan = await AddLoanAsync(org.Id, customer!, model);
        if (request != null)
        {
            request.MarkConverted();
            await _db.SaveChangesAsync();
        }
        await tx.CommitAsync();

        TempData["Success"] = $"Loan {loan.LoanNo} created and is awaiting approval.";
        return RedirectToAction("View", new { id = _enc.Encrypt(loan.Id.ToString()) });
    }

    // ── Edit (pending only) ───────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var loanId)) return NotFound();

        var loan = await Detailed(org.Id).FirstOrDefaultAsync(l => l.Id == loanId);
        if (loan == null) return NotFound();
        if (loan.Status != LoanOptions.Pending)
        {
            TempData["Error"] = "Only a pending loan can be edited.";
            return RedirectToAction("View", new { id });
        }

        var model = new LoanFormModel
        {
            EncryptedId = id, IsEdit = true, LoanNo = loan.LoanNo, LoanRequestId = loan.LoanRequestId,
            CustomerNo = loan.Customer.CustomerNo, CustomerName = loan.Customer.DisplayName,
            CustomerLabel = $"{loan.Customer.CustomerNo} — {loan.Customer.DisplayName}",
            BranchId = loan.BranchId, ProductDefinitionId = loan.ProductDefinitionId, LoanName = loan.LoanName,
            Currency = loan.Currency, LoanAmount = loan.LoanAmount, InterestRate = loan.InterestRate,
            InterestCalcMethod = loan.InterestCalcMethod, GracePeriod = loan.GracePeriod, Tenure = loan.Tenure,
            NoOfInstallments = loan.NoOfInstallments, RepaymentFrequency = loan.RepaymentFrequency,
            RepaymentMode = loan.RepaymentMode, FirstEMIDate = loan.FirstEMIDate, Charges = loan.Charges,
            Collateral = loan.Collateral, GuarantorsAccountNo = loan.GuarantorsAccountNo,
            SettlementAccountNo = loan.SettlementAccountNo, SkipInstallments = loan.SkipInstallments,
            SectorId = loan.SectorId, SubSectorId = loan.SubSectorId
        };
        if (loan.LoanRequestId != null)
            model.RequestedAmount = await _db.LoanRequests.Where(r => r.Id == loan.LoanRequestId).Select(r => (decimal?)r.RequestedAmount).FirstOrDefaultAsync();
        await FillListsAsync(model, org.Id);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, LoanFormModel model)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var loanId)) return NotFound();

        var loan = await Detailed(org.Id).FirstOrDefaultAsync(l => l.Id == loanId);
        if (loan == null) return NotFound();
        if (loan.Status != LoanOptions.Pending)
        {
            TempData["Error"] = "Only a pending loan can be edited.";
            return RedirectToAction("View", new { id });
        }

        // Fixed at creation: whatever the browser posted is ignored.
        model.IsEdit = true;
        model.CustomerNo = loan.Customer.CustomerNo;
        model.BranchId = loan.BranchId;
        model.LoanRequestId = loan.LoanRequestId;
        Normalise(model);

        if (loan.LoanRequestId != null)
        {
            var requested = await _db.LoanRequests.Where(r => r.Id == loan.LoanRequestId).Select(r => (decimal?)r.RequestedAmount).FirstOrDefaultAsync();
            model.RequestedAmount = requested;
            if (requested != null && model.LoanAmount > requested)
                ModelState.AddModelError(nameof(model.LoanAmount), $"The loan cannot be more than the approved request ({requested:N2}).");
        }

        await ValidateAsync(model, org.Id, loan.Customer);

        if (!ModelState.IsValid)
        {
            model.EncryptedId = id;
            model.LoanNo = loan.LoanNo;
            model.CustomerName = loan.Customer.DisplayName;
            model.CustomerLabel = $"{loan.Customer.CustomerNo} — {loan.Customer.DisplayName}";
            await FillListsAsync(model, org.Id);
            return View(model);
        }

        loan.UpdateTerms(ToTerms(model));
        if (!await TrySaveAsync()) return RedirectToAction("View", new { id });
        TempData["Success"] = "Loan updated successfully.";
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
        if (!TryDecodeId(id, out var loanId)) return NotFound();

        var loan = await Detailed(org.Id).FirstOrDefaultAsync(l => l.Id == loanId);
        if (loan == null) return NotFound();
        return View("View", await BuildDetailAsync(loan, id));
    }

    // ── Approve / delete ──────────────────────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(string id)
    {
        var (loan, redirect) = await LoadAsync(id);
        if (loan == null) return redirect!;

        if (loan.Customer.Status != CustomerOptions.Active)
        {
            TempData["Error"] = $"Customer {loan.Customer.CustomerNo} is {loan.Customer.Status.ToLowerInvariant()}; the loan cannot be approved.";
            return RedirectToAction("View", new { id });
        }
        try
        {
            loan.Approve(GetUserId());
            if (!await TrySaveAsync()) return RedirectToAction("View", new { id });
            TempData["Success"] = $"Loan {loan.LoanNo} approved. It can now be disbursed.";
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction("View", new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var (loan, redirect) = await LoadAsync(id);
        if (loan == null) return redirect!;

        if (loan.Status != LoanOptions.Pending)
        {
            TempData["Error"] = "Only a pending loan can be deleted.";
            return RedirectToAction("View", new { id });
        }

        // Give a converted request back so the loan can be re-created.
        if (loan.LoanRequestId != null)
        {
            var req = await _db.LoanRequests.FirstOrDefaultAsync(r => r.Id == loan.LoanRequestId);
            req?.ReopenAfterLoanDeleted();
        }
        loan.SoftDelete();
        if (!await TrySaveAsync()) return RedirectToAction("View", new { id });
        TempData["Success"] = $"Loan {loan.LoanNo} deleted.";
        return RedirectToAction(nameof(Index));
    }

    // ── Disburse ──────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Disburse(string id)
    {
        SetNav();
        var (loan, redirect) = await LoadAsync(id);
        if (loan == null) return redirect!;
        if (loan.Status != LoanOptions.Approved)
        {
            TempData["Error"] = "Only an approved loan can be disbursed.";
            return RedirectToAction("View", new { id });
        }

        var vm = new DisburseViewModel { DisbursementDate = Today(), FirstEmiDate = loan.FirstEMIDate };
        vm.Loan = await BuildDetailAsync(loan, id, preview: (vm.DisbursementDate, vm.FirstEmiDate));
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Disburse(string id, DisburseViewModel model)
    {
        SetNav();
        var (loan, redirect) = await LoadAsync(id);
        if (loan == null) return redirect!;
        if (loan.Status != LoanOptions.Approved)
        {
            TempData["Error"] = "Only an approved loan can be disbursed.";
            return RedirectToAction("View", new { id });
        }

        string? problem = null;
        if (loan.Customer.Status != CustomerOptions.Active)
            problem = $"Customer {loan.Customer.CustomerNo} is {loan.Customer.Status.ToLowerInvariant()}.";
        else if (model.DisbursementDate == default || model.DisbursementDate > Today())
            problem = "The disbursement date cannot be in the future.";
        else if (model.FirstEmiDate != null && model.FirstEmiDate <= model.DisbursementDate)
            problem = "The first installment must fall after the disbursement date.";

        if (problem != null)
        {
            TempData["Error"] = problem;
            model.Loan = await BuildDetailAsync(loan, id, preview: (Today(), null));
            return View(model);
        }

        IReadOnlyList<LoanRepayment> schedule;
        try
        {
            schedule = loan.Disburse(GetUserId(), model.DisbursementDate, model.FirstEmiDate);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("View", new { id });
        }

        _db.LoanRepayments.AddRange(schedule);
        if (!await TrySaveAsync()) return RedirectToAction("View", new { id });
        TempData["Success"] = $"Loan {loan.LoanNo} disbursed. A {schedule.Count}-installment schedule has been created.";
        return RedirectToAction("View", new { id });
    }

    // ── Repayment ─────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Repayment(string id)
    {
        SetNav();
        var (loan, redirect) = await LoadAsync(id);
        if (loan == null) return redirect!;
        if (loan.Status != LoanOptions.Active)
        {
            TempData["Error"] = loan.Status == LoanOptions.Closed ? "This loan is fully repaid." : "Repayments can only be taken on an active loan.";
            return RedirectToAction("View", new { id });
        }

        var vm = new RepaymentViewModel { PaidOn = Today(), Mode = loan.RepaymentMode, Modes = ModeItems() };
        vm.Loan = await BuildDetailAsync(loan, id);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Repayment(string id, RepaymentViewModel model)
    {
        SetNav();
        var (loan, redirect) = await LoadAsync(id);
        if (loan == null) return redirect!;
        if (loan.Status != LoanOptions.Active)
        {
            TempData["Error"] = "Repayments can only be taken on an active loan.";
            return RedirectToAction("View", new { id });
        }

        string? problem = null;
        if (model.Amount <= 0 || decimal.Round(model.Amount, 2) != model.Amount)
            problem = "Enter an amount greater than zero with at most two decimal places.";
        else if (!LoanOptions.RepaymentModes.Contains(model.Mode ?? ""))
            problem = "Please select a valid payment mode.";
        else if (model.PaidOn == default || model.PaidOn > Today())
            problem = "The payment date cannot be in the future.";
        else if (loan.LoanStartDate != null && model.PaidOn < loan.LoanStartDate)
            problem = "The payment date cannot be before the loan was disbursed.";

        LoanPayment? payment = null;
        if (problem == null)
        {
            var installments = await _db.LoanRepayments.Where(r => r.LoanAccountId == loan.Id).ToListAsync();
            try
            {
                payment = loan.ApplyPayment(model.Amount, model.PaidOn, model.Mode!, Clean(model.Reference), GetUserId(), installments);
            }
            catch (InvalidOperationException ex) { problem = ex.Message; }
        }

        if (problem != null)
        {
            TempData["Error"] = problem;
            return RedirectToAction(nameof(Repayment), new { id });
        }

        _db.LoanPayments.Add(payment!);
        if (!await TrySaveAsync()) return RedirectToAction(nameof(Repayment), new { id });

        TempData["Success"] = loan.Status == LoanOptions.Closed
            ? $"{payment!.Amount:N2} {loan.Currency} received. Loan {loan.LoanNo} is now fully repaid and closed."
            : $"{payment!.Amount:N2} {loan.Currency} received on {loan.LoanNo}.";
        return RedirectToAction(loan.Status == LoanOptions.Closed ? "View" : nameof(Repayment), new { id });
    }

    // ── Enquiry ───────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> BalanceEnquiry(string? loanNo)
    {
        SetNav("LoanEnquiry");
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var vm = new LoanEnquiryViewModel { LoanNo = loanNo?.Trim() };
        if (!string.IsNullOrWhiteSpace(vm.LoanNo))
        {
            vm.Searched = true;
            var no = vm.LoanNo.ToUpperInvariant();
            var loan = await Detailed(org.Id).FirstOrDefaultAsync(l => l.LoanNo == no);
            if (loan != null) vm.Loan = await BuildDetailAsync(loan, _enc.Encrypt(loan.Id.ToString()));
        }
        return View(vm);
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  LOAN REQUESTS
    // ═════════════════════════════════════════════════════════════════════════

    public async Task<IActionResult> Requests(string? status, int page = 1)
    {
        SetNav("LoanRequests");
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var query = _db.LoanRequests.Include(r => r.Customer).Include(r => r.ProductDefinition).Include(r => r.Branch)
            .Where(r => r.OrganizationId == org.Id && !r.IsDeleted);
        if (!string.IsNullOrWhiteSpace(status) && LoanOptions.RequestStatuses.Contains(status))
            query = query.Where(r => r.Status == status);

        var total = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        page = Math.Clamp(page, 1, totalPages);
        var rows = await query.OrderByDescending(r => r.CreatedAt).Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();

        return View(new LoanRequestListViewModel
        {
            Status = status, Page = page, TotalPages = totalPages, TotalCount = total,
            Statuses = LoanOptions.RequestStatuses.Select(s => new SelectListItem(Pretty(s), s)).ToList(),
            Rows = rows.Select(r => new LoanRequestRowViewModel
            {
                EncryptedId = _enc.Encrypt(r.Id.ToString()), CustomerNo = r.Customer.CustomerNo, CustomerName = r.Customer.DisplayName,
                ProductName = r.ProductDefinition.Name, BranchName = r.Branch.Name, RequestedAmount = r.RequestedAmount,
                Purpose = r.Purpose, Status = r.Status, ReviewRemarks = r.ReviewRemarks, CreatedAt = r.CreatedAt
            }).ToList()
        });
    }

    [HttpGet]
    public async Task<IActionResult> CreateRequest()
    {
        SetNav("LoanRequests");
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        var model = new LoanRequestFormModel();
        await FillRequestListsAsync(model, org.Id);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRequest(LoanRequestFormModel model)
    {
        SetNav("LoanRequests");
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var customer = await ResolveCustomerAsync(org.Id, model.CustomerNo);
        if (customer == null)
            ModelState.AddModelError(nameof(model.CustomerNo), "No customer with that number exists.");
        else if (customer.Status != CustomerOptions.Active)
            ModelState.AddModelError(nameof(model.CustomerNo), $"Customer {customer.CustomerNo} is {customer.Status.ToLowerInvariant()}; only active customers can request loans.");

        if (decimal.Round(model.RequestedAmount, 2) != model.RequestedAmount)
            ModelState.AddModelError(nameof(model.RequestedAmount), "Use at most two decimal places.");
        if (model.BranchId == null || !await _db.Branches.AnyAsync(b => b.Id == model.BranchId && b.OrganizationId == org.Id && !b.IsDeleted && b.IsActive))
            ModelState.AddModelError(nameof(model.BranchId), "Please select a valid branch.");
        if (model.ProductDefinitionId == null || !await _db.ProductDefinitions.AnyAsync(p => p.Id == model.ProductDefinitionId && p.OrganizationId == org.Id && !p.IsDeleted && p.IsActive))
            ModelState.AddModelError(nameof(model.ProductDefinitionId), "Please select a valid product.");

        if (!ModelState.IsValid)
        {
            await FillRequestListsAsync(model, org.Id);
            return View(model);
        }

        _db.LoanRequests.Add(new LoanRequest(org.Id, model.BranchId!.Value, customer!.Id, model.ProductDefinitionId!.Value,
            model.RequestedAmount, Clean(model.Purpose), GetUserId()));
        await _db.SaveChangesAsync();
        TempData["Success"] = "Loan request submitted for review.";
        return RedirectToAction(nameof(Requests));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> ApproveRequest(string id, string? remarks) => ReviewRequestAsync(id, remarks, approve: true);

    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> RejectRequest(string id, string? remarks) => ReviewRequestAsync(id, remarks, approve: false);

    private async Task<IActionResult> ReviewRequestAsync(string id, string? remarks, bool approve)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var requestId)) return NotFound();

        var req = await _db.LoanRequests.Include(r => r.Customer)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.OrganizationId == org.Id && !r.IsDeleted);
        if (req == null) return NotFound();

        if (req.Status != LoanOptions.Pending)
            TempData["Error"] = "This request has already been reviewed.";
        else if (!approve && string.IsNullOrWhiteSpace(remarks))
            TempData["Error"] = "Please give a reason for rejecting the request.";
        else if (approve && req.Customer.Status != CustomerOptions.Active)
            TempData["Error"] = $"Customer {req.Customer.CustomerNo} is {req.Customer.Status.ToLowerInvariant()}; the request cannot be approved.";
        else
        {
            var note = Clean(remarks);
            if (approve) req.Approve(GetUserId(), note); else req.Reject(GetUserId(), note);
            await _db.SaveChangesAsync();
            TempData["Success"] = approve ? "Request approved. Create the loan from the requests list." : "Request rejected.";
        }
        return RedirectToAction(nameof(Requests));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRequest(string id)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var requestId)) return NotFound();

        var req = await _db.LoanRequests.FirstOrDefaultAsync(r => r.Id == requestId && r.OrganizationId == org.Id && !r.IsDeleted);
        if (req == null) return NotFound();

        try
        {
            req.SoftDelete();
            await _db.SaveChangesAsync();
            TempData["Success"] = "Request deleted.";
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Requests));
    }

    [HttpGet]
    public async Task<IActionResult> LookupCustomer(string? customerNo)
    {
        var org = await GetOrgAsync();
        if (org == null) return Json(new { found = false });
        var c = await ResolveCustomerAsync(org.Id, customerNo);
        return c == null ? Json(new { found = false }) : Json(new { found = true, name = c.DisplayName, status = c.Status, branchId = c.BranchId });
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  Helpers
    // ═════════════════════════════════════════════════════════════════════════

    private static DateOnly Today() => DateOnly.FromDateTime(DateTime.UtcNow);
    private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    private static string Pretty(string code) => string.Join(' ', code.Split('_').Select(w => char.ToUpper(w[0]) + w[1..].ToLower()));
    private List<SelectListItem> ModeItems() => LoanOptions.RepaymentModes.Select(m => new SelectListItem(Pretty(m), m)).ToList();

    private static void Normalise(LoanFormModel m)
    {
        m.Currency = (m.Currency ?? "").Trim().ToUpperInvariant();
        m.InterestCalcMethod = (m.InterestCalcMethod ?? "").Trim().ToUpperInvariant();
        m.RepaymentFrequency = (m.RepaymentFrequency ?? "").Trim().ToUpperInvariant();
        m.RepaymentMode = (m.RepaymentMode ?? "").Trim().ToUpperInvariant();
        m.SettlementAccountNo = m.SettlementAccountNo?.Trim().ToUpperInvariant();
        m.GuarantorsAccountNo = m.GuarantorsAccountNo?.Trim().ToUpperInvariant();
    }

    private static LoanTerms ToTerms(LoanFormModel m) => new()
    {
        ProductDefinitionId = m.ProductDefinitionId!.Value, LoanName = m.LoanName!.Trim(), Currency = m.Currency,
        LoanAmount = m.LoanAmount, InterestRate = m.InterestRate, InterestCalcMethod = m.InterestCalcMethod,
        GracePeriod = m.GracePeriod, Tenure = m.Tenure, NoOfInstallments = m.NoOfInstallments,
        RepaymentFrequency = m.RepaymentFrequency, RepaymentMode = m.RepaymentMode, FirstEMIDate = m.FirstEMIDate,
        Charges = m.Charges, Collateral = Clean(m.Collateral), GuarantorsAccountNo = Clean(m.GuarantorsAccountNo),
        SettlementAccountNo = Clean(m.SettlementAccountNo), SkipInstallments = m.SkipInstallments,
        SectorId = m.SectorId, SubSectorId = m.SubSectorId
    };

    private Task<Customer?> ResolveCustomerAsync(Guid orgId, string? customerNo)
    {
        var no = customerNo?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(no)) return Task.FromResult<Customer?>(null);
        return _db.Customers.FirstOrDefaultAsync(c => c.OrganizationId == orgId && !c.IsDeleted && c.CustomerNo == no);
    }

    private async Task<(LoanAccount? Loan, IActionResult? Redirect)> LoadAsync(string id)
    {
        var org = await GetOrgAsync();
        if (org == null) return (null, RedirectToAction("Index", "Organization"));
        if (!TryDecodeId(id, out var loanId)) return (null, NotFound());
        var loan = await Detailed(org.Id).FirstOrDefaultAsync(l => l.Id == loanId);
        return loan == null ? (null, NotFound()) : (loan, null);
    }

    private async Task<bool> TrySaveAsync()
    {
        try
        {
            await _db.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["Error"] = "This loan was changed by someone else while you were working. Please review it and try again.";
            return false;
        }
    }

    // ── Validation ────────────────────────────────────────────────────────────

    private async Task ValidateAsync(LoanFormModel m, Guid orgId, Customer? customer)
    {
        if (m.BranchId == null || !await _db.Branches.AnyAsync(b => b.Id == m.BranchId && b.OrganizationId == orgId && !b.IsDeleted && b.IsActive))
            ModelState.AddModelError(nameof(m.BranchId), "Please select a valid branch.");
        if (m.ProductDefinitionId == null || !await _db.ProductDefinitions.AnyAsync(p => p.Id == m.ProductDefinitionId && p.OrganizationId == orgId && !p.IsDeleted && p.IsActive))
            ModelState.AddModelError(nameof(m.ProductDefinitionId), "Please select a valid product.");

        if (m.Currency.Length != 3 || !m.Currency.All(char.IsLetter))
            ModelState.AddModelError(nameof(m.Currency), "Use a 3-letter currency code, e.g. GMD.");
        if (!LoanOptions.CalcMethods.Contains(m.InterestCalcMethod))
            ModelState.AddModelError(nameof(m.InterestCalcMethod), "Please select a valid interest method.");
        if (!LoanOptions.Frequencies.Contains(m.RepaymentFrequency))
            ModelState.AddModelError(nameof(m.RepaymentFrequency), "Please select a valid repayment frequency.");
        if (!LoanOptions.RepaymentModes.Contains(m.RepaymentMode))
            ModelState.AddModelError(nameof(m.RepaymentMode), "Please select a valid repayment mode.");

        if (decimal.Round(m.LoanAmount, 2) != m.LoanAmount) ModelState.AddModelError(nameof(m.LoanAmount), "Use at most two decimal places.");
        if (decimal.Round(m.Charges, 2) != m.Charges) ModelState.AddModelError(nameof(m.Charges), "Use at most two decimal places.");
        if (m.InterestRate != decimal.Round(m.InterestRate, 4)) ModelState.AddModelError(nameof(m.InterestRate), "Use at most four decimal places.");
        if (m.FirstEMIDate != null && m.FirstEMIDate <= Today())
            ModelState.AddModelError(nameof(m.FirstEMIDate), "The first installment date must be in the future.");

        // A settlement account is where repayments are taken from, so it must belong to the borrower.
        var needsSettlement = m.RepaymentMode is LoanOptions.Deduction or LoanOptions.StandingOrder;
        if (string.IsNullOrWhiteSpace(m.SettlementAccountNo))
        {
            if (needsSettlement)
                ModelState.AddModelError(nameof(m.SettlementAccountNo), "A settlement account is required for deduction and standing-order repayment.");
        }
        else
        {
            var acc = await _db.MemberAccounts.FirstOrDefaultAsync(a => a.OrganizationId == orgId && !a.IsDeleted && a.AccountNo == m.SettlementAccountNo);
            if (acc == null || (customer != null && acc.CustomerId != customer.Id))
                ModelState.AddModelError(nameof(m.SettlementAccountNo), "Enter an account number that belongs to this customer.");
            else if (acc.Status != AccountOptions.Active)
                ModelState.AddModelError(nameof(m.SettlementAccountNo), "The settlement account must be active.");
            else if (acc.Currency != m.Currency)
                ModelState.AddModelError(nameof(m.SettlementAccountNo), $"The settlement account is in {acc.Currency}, but the loan is in {m.Currency}.");
        }

        // A guarantor stands behind the loan, so their account must be someone else's.
        if (!string.IsNullOrWhiteSpace(m.GuarantorsAccountNo))
        {
            var g = await _db.MemberAccounts.FirstOrDefaultAsync(a => a.OrganizationId == orgId && !a.IsDeleted && a.AccountNo == m.GuarantorsAccountNo);
            if (g == null || g.Status == AccountOptions.Closed || g.Status == AccountOptions.Pending)
                ModelState.AddModelError(nameof(m.GuarantorsAccountNo), "Enter the number of an open account.");
            else if (customer != null && g.CustomerId == customer.Id)
                ModelState.AddModelError(nameof(m.GuarantorsAccountNo), "A borrower cannot guarantee their own loan.");
        }

        if (m.SectorId != null && !await _db.Sectors.AnyAsync(x => x.Id == m.SectorId && x.OrganizationId == orgId && !x.IsDeleted))
            ModelState.AddModelError(nameof(m.SectorId), "Please select a valid sector.");
        if (m.SubSectorId != null && !await _db.SubSectors.AnyAsync(x => x.Id == m.SubSectorId && x.OrganizationId == orgId && !x.IsDeleted && x.SectorId == m.SectorId))
            ModelState.AddModelError(nameof(m.SubSectorId), "Please select a sub-sector that belongs to the chosen sector.");
    }

    private async Task FillListsAsync(LoanFormModel m, Guid orgId)
    {
        m.Branches = await _db.Branches.Where(b => b.OrganizationId == orgId && !b.IsDeleted && b.IsActive).OrderBy(b => b.Name)
            .Select(b => new SelectListItem(b.Name + " (" + b.Code + ")", b.Id.ToString())).ToListAsync();
        m.Products = await _db.ProductDefinitions.Include(p => p.ProductType)
            .Where(p => p.OrganizationId == orgId && !p.IsDeleted && p.IsActive).OrderBy(p => p.Name)
            .Select(p => new ProductOption { Id = p.Id, Name = p.Name + " (" + p.Code + ") · " + p.ProductType.Name, Rate = p.InterestRate }).ToListAsync();
        m.CalcMethods = LoanOptions.CalcMethods.Select(x => new SelectListItem(Pretty(x), x)).ToList();
        m.Frequencies = LoanOptions.Frequencies.Select(x => new SelectListItem(Pretty(x), x)).ToList();
        m.Modes = ModeItems();
        m.Sectors = await SectorItemsAsync(orgId);
        m.SubSectors = await _db.SubSectors.Where(x => x.OrganizationId == orgId && !x.IsDeleted && x.IsActive).OrderBy(x => x.Name)
            .Select(x => new LocationOption { Id = x.Id, ParentId = x.SectorId, Name = x.Name }).ToListAsync();
    }

    private async Task FillRequestListsAsync(LoanRequestFormModel m, Guid orgId)
    {
        m.Branches = await _db.Branches.Where(b => b.OrganizationId == orgId && !b.IsDeleted && b.IsActive).OrderBy(b => b.Name)
            .Select(b => new SelectListItem(b.Name + " (" + b.Code + ")", b.Id.ToString())).ToListAsync();
        m.Products = await _db.ProductDefinitions.Include(p => p.ProductType)
            .Where(p => p.OrganizationId == orgId && !p.IsDeleted && p.IsActive).OrderBy(p => p.Name)
            .Select(p => new SelectListItem(p.Name + " (" + p.Code + ") · " + p.ProductType.Name, p.Id.ToString())).ToListAsync();
    }

    // ── Loan numbers ──────────────────────────────────────────────────────────

    /// <summary>
    /// Next LN-{year}-{seq:D6} from the highest number ever issued this year (deleted rows included), so numbers
    /// are never reused. A unique index backs this up.
    /// </summary>
    private async Task<string> NextLoanNoAsync(Guid orgId)
    {
        var prefix = $"LN-{DateTime.UtcNow.Year}-";
        var last = await _db.LoanAccounts.Where(l => l.OrganizationId == orgId && l.LoanNo.StartsWith(prefix))
            .OrderByDescending(l => l.LoanNo).Select(l => l.LoanNo).FirstOrDefaultAsync();
        var seq = last != null && int.TryParse(last.AsSpan(prefix.Length), out var n) ? n : 0;
        return $"{prefix}{seq + 1:D6}";
    }

    private async Task<LoanAccount> AddLoanAsync(Guid orgId, Customer customer, LoanFormModel m)
    {
        for (var attempt = 1; ; attempt++)
        {
            var loan = new LoanAccount(orgId, m.BranchId!.Value, customer.Id, m.ProductDefinitionId!.Value,
                await NextLoanNoAsync(orgId), m.LoanAmount, GetUserId(), m.LoanRequestId);
            loan.UpdateTerms(ToTerms(m));
            _db.LoanAccounts.Add(loan);
            try
            {
                await _db.SaveChangesAsync();
                return loan;
            }
            catch (DbUpdateException ex) when (attempt < 5 && ex.InnerException is SqlException { Number: 2601 or 2627 })
            {
                _db.Entry(loan).State = EntityState.Detached;
            }
        }
    }

    // ── Detail view data ──────────────────────────────────────────────────────

    private async Task<string?> UserNameAsync(Guid? userId)
    {
        if (userId == null) return null;
        var u = await _db.AppUsers.Where(x => x.Id == userId).Select(x => new { x.FirstName, x.LastName }).FirstOrDefaultAsync();
        return u == null ? null : $"{u.FirstName} {u.LastName}".Trim();
    }

    private async Task<LoanDetailViewModel> BuildDetailAsync(LoanAccount l, string id, (DateOnly Date, DateOnly? FirstDue)? preview = null)
    {
        var vm = new LoanDetailViewModel
        {
            EncryptedId = id, LoanNo = l.LoanNo, LoanName = l.LoanName, Status = l.Status, Currency = l.Currency,
            CustomerEncryptedId = _enc.Encrypt(l.CustomerId.ToString()), CustomerNo = l.Customer.CustomerNo,
            CustomerName = l.Customer.DisplayName, BranchName = l.Branch.Name, ProductName = l.ProductDefinition.Name,
            LoanAmount = l.LoanAmount, DisbursedAmount = l.DisbursedAmount, OutstandingBalance = l.OutstandingBalance,
            InterestRate = l.InterestRate, InterestCalcMethod = l.InterestCalcMethod, GracePeriod = l.GracePeriod, Tenure = l.Tenure,
            NoOfInstallments = l.NoOfInstallments, RepaymentFrequency = l.RepaymentFrequency, RepaymentMode = l.RepaymentMode,
            LoanStartDate = l.LoanStartDate, LoanEndDate = l.LoanEndDate, FirstEMIDate = l.FirstEMIDate, Charges = l.Charges,
            Collateral = l.Collateral, GuarantorsAccountNo = l.GuarantorsAccountNo, SettlementAccountNo = l.SettlementAccountNo,
            SkipInstallments = l.SkipInstallments, IsApproved = l.IsApproved, ApprovedAt = l.ApprovedAt,
            IsDisbursed = l.IsDisbursed, DisbursedAt = l.DisbursedAt
        };
        if (l.SectorId != null) vm.SectorName = await _db.Sectors.Where(x => x.Id == l.SectorId).Select(x => x.Name).FirstOrDefaultAsync();
        if (l.SubSectorId != null) vm.SubSectorName = await _db.SubSectors.Where(x => x.Id == l.SubSectorId).Select(x => x.Name).FirstOrDefaultAsync();
        vm.CreatedByName = await UserNameAsync(l.CreatedByUserId);
        vm.ApprovedByName = await UserNameAsync(l.ApprovedBy);
        vm.DisbursedByName = await UserNameAsync(l.DisbursedBy);

        var today = Today();
        if (l.IsDisbursed)
        {
            var rows = await _db.LoanRepayments.Where(r => r.LoanAccountId == l.Id).OrderBy(r => r.InstallmentNo).ToListAsync();
            vm.Schedule = rows.Select(r => new ScheduleRowViewModel
            {
                No = r.InstallmentNo, DueDate = r.DueDate, PrincipalDue = r.PrincipalDue, InterestDue = r.InterestDue,
                ChargesDue = r.ChargesDue, TotalDue = r.TotalDue, TotalPaid = r.TotalDue - r.TotalRemaining,
                Remaining = r.TotalRemaining, Status = r.Status, IsOverdue = r.IsOverdueOn(today)
            }).ToList();

            var payments = await _db.LoanPayments.Where(p => p.LoanAccountId == l.Id).OrderByDescending(p => p.CreatedAt).ToListAsync();
            foreach (var p in payments)
                vm.Payments.Add(new LoanPaymentRowViewModel
                {
                    PaidOn = p.PaidOn, Amount = p.Amount, PrincipalPaid = p.PrincipalPaid, InterestPaid = p.InterestPaid,
                    ChargesPaid = p.ChargesPaid, Mode = p.Mode, Reference = p.Reference, PostedByName = await UserNameAsync(p.PostedBy)
                });

            vm.TotalPayable = rows.Sum(r => r.TotalDue);
            vm.TotalRemaining = rows.Sum(r => r.TotalRemaining);
            vm.TotalPaid = vm.TotalPayable - vm.TotalRemaining;
            var overdue = rows.Where(r => r.IsOverdueOn(today)).ToList();
            vm.OverdueAmount = overdue.Sum(r => r.TotalRemaining);
            vm.OverdueInstallments = overdue.Count;
        }
        else if (l.Status is LoanOptions.Pending or LoanOptions.Approved && l.Tenure > 0 && l.NoOfInstallments > 0)
        {
            var (date, first) = preview ?? (today, null);
            vm.ScheduleIsPreview = true;
            vm.Schedule = l.PreviewSchedule(date, first).Select(s => new ScheduleRowViewModel
            {
                No = s.No, DueDate = s.DueDate, PrincipalDue = s.Principal, InterestDue = s.Interest, ChargesDue = s.Charges,
                TotalDue = s.Total, Remaining = s.Total
            }).ToList();
            vm.TotalPayable = vm.Schedule.Sum(r => r.TotalDue);
            vm.TotalRemaining = vm.TotalPayable;
        }
        return vm;
    }
}
