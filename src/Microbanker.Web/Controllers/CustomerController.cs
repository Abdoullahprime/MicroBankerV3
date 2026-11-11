// src/Microbanker.Web/Controllers/CustomerController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Entities.Customers;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Models;
using Microbanker.Web.Services;

namespace Microbanker.Web.Controllers;

public class CustomerController : BankingControllerBase
{
    public CustomerController(MicrobankerDbContext db, IEncryptionService enc) : base(db, enc) { }

    private void SetNav()
    {
        ViewData["ActiveGroup"] = "Banking";
        ViewData["ActivePage"]  = "Customers";
    }

    private IQueryable<Customer> Detailed(Guid orgId) => _db.Customers
        .Include(c => c.Branch).Include(c => c.Region).Include(c => c.District)
        .Include(c => c.Area).Include(c => c.Sector).Include(c => c.SubSector)
        .Where(c => c.OrganizationId == orgId && !c.IsDeleted);

    // ── List ──────────────────────────────────────────────────────────────────

    public async Task<IActionResult> Index(string? status, string? q, int page = 1)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var query = _db.Customers.Include(c => c.Branch)
            .Where(c => c.OrganizationId == org.Id && !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(status) && CustomerOptions.CustomerStatuses.Contains(status))
            query = query.Where(c => c.Status == status);

        q = q?.Trim();
        if (!string.IsNullOrEmpty(q))
            query = query.Where(c => c.CustomerNo.Contains(q) || c.FirstName.Contains(q) || c.LastName.Contains(q)
                || (c.BusinessName != null && c.BusinessName.Contains(q)) || (c.GroupName != null && c.GroupName.Contains(q))
                || (c.MobileNo != null && c.MobileNo.Contains(q)) || (c.Telephone != null && c.Telephone.Contains(q))
                || (c.NationalId != null && c.NationalId.Contains(q)));

        var total = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        page = Math.Clamp(page, 1, totalPages);

        var rows = await query.OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();

        return View(new CustomerListViewModel
        {
            Status = status, Q = q, Page = page, TotalPages = totalPages, TotalCount = total,
            Statuses = StatusItems(CustomerOptions.CustomerStatuses),
            Rows = rows.Select(c => new CustomerRowViewModel
            {
                EncryptedId = _enc.Encrypt(c.Id.ToString()),
                CustomerNo = c.CustomerNo, DisplayName = c.DisplayName, CustomerType = c.CustomerType,
                BranchName = c.Branch.Name, Phone = c.MobileNo ?? c.Telephone,
                Status = c.Status, CreatedAt = c.CreatedAt
            }).ToList()
        });
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var model = new CustomerFormModel();
        await FillListsAsync(model, org.Id);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerFormModel model)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        await ValidateProfileAsync(model, org.Id);
        if (await NationalIdTakenAsync(org.Id, model.NationalId, Guid.Empty))
            ModelState.AddModelError(nameof(model.NationalId), "Another customer already has this national ID.");

        if (!ModelState.IsValid)
        {
            await FillListsAsync(model, org.Id);
            return View(model);
        }

        var customer = await AddCustomerAsync(org.Id, ProfileMapper.ToProfile(model), GetUserId());
        TempData["Success"] = $"Customer {customer.CustomerNo} created and is awaiting approval.";
        return RedirectToAction("View", new { id = _enc.Encrypt(customer.Id.ToString()) });
    }

    // ── Edit ──────────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var customerId)) return NotFound();

        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == customerId && c.OrganizationId == org.Id && !c.IsDeleted);
        if (customer == null) return NotFound();
        if (customer.Status == CustomerOptions.Closed)
        {
            TempData["Error"] = "A closed customer cannot be edited.";
            return RedirectToAction("View", new { id });
        }

        var model = new CustomerFormModel { EncryptedId = id, CustomerNo = customer.CustomerNo };
        ProfileMapper.Fill(model, customer);
        await FillListsAsync(model, org.Id);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, CustomerFormModel model)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var customerId)) return NotFound();

        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == customerId && c.OrganizationId == org.Id && !c.IsDeleted);
        if (customer == null) return NotFound();
        if (customer.Status == CustomerOptions.Closed)
        {
            TempData["Error"] = "A closed customer cannot be edited.";
            return RedirectToAction("View", new { id });
        }

        // The type is fixed at creation; ignore whatever the browser posted.
        model.CustomerType = customer.CustomerType;

        await ValidateProfileAsync(model, org.Id);
        if (await NationalIdTakenAsync(org.Id, model.NationalId, customerId))
            ModelState.AddModelError(nameof(model.NationalId), "Another customer already has this national ID.");

        if (!ModelState.IsValid)
        {
            model.EncryptedId = id;
            model.CustomerNo = customer.CustomerNo;
            await FillListsAsync(model, org.Id);
            return View(model);
        }

        customer.Update(ProfileMapper.ToProfile(model));
        await _db.SaveChangesAsync();
        TempData["Success"] = "Customer updated successfully.";
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
        if (!TryDecodeId(id, out var customerId)) return NotFound();

        var c = await Detailed(org.Id).FirstOrDefaultAsync(x => x.Id == customerId);
        if (c == null) return NotFound();

        var vm = new CustomerDetailViewModel
        {
            EncryptedId = id, CustomerNo = c.CustomerNo, Status = c.Status,
            IsApproved = c.IsApproved, ApprovedAt = c.ApprovedAt, ApprovalRemarks = c.ApprovalRemarks,
            CreatedByName = await UserNameAsync(c.CreatedByUserId),
            ApprovedByName = await UserNameAsync(c.ApprovedBy)
        };
        FillDetail(vm, c);
        return View("View", vm);
    }

    // ── Status changes ────────────────────────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(string id, string? remarks)
    {
        var (customer, redirect) = await LoadForActionAsync(id);
        if (customer == null) return redirect!;

        if (customer.Status != CustomerOptions.Pending)
            TempData["Error"] = "Only a pending customer can be approved.";
        else
        {
            customer.Approve(GetUserId(), string.IsNullOrWhiteSpace(remarks) ? null : remarks.Trim());
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Customer {customer.CustomerNo} approved and is now active.";
        }
        return RedirectToAction("View", new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Suspend(string id)
    {
        var (customer, redirect) = await LoadForActionAsync(id);
        if (customer == null) return redirect!;

        if (customer.Status != CustomerOptions.Active)
            TempData["Error"] = "Only an active customer can be suspended.";
        else
        {
            customer.Suspend();
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Customer {customer.CustomerNo} suspended.";
        }
        return RedirectToAction("View", new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reactivate(string id)
    {
        var (customer, redirect) = await LoadForActionAsync(id);
        if (customer == null) return redirect!;

        if (customer.Status != CustomerOptions.Suspended)
            TempData["Error"] = "Only a suspended customer can be reactivated.";
        else
        {
            customer.Reactivate();
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Customer {customer.CustomerNo} reactivated.";
        }
        return RedirectToAction("View", new { id });
    }

    // ── Delete (unapproved records only) ──────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var (customer, redirect) = await LoadForActionAsync(id);
        if (customer == null) return redirect!;

        if (customer.Status != CustomerOptions.Pending)
        {
            TempData["Error"] = "Only a pending (unapproved) customer can be deleted. Suspend or close an active customer instead.";
            return RedirectToAction("View", new { id });
        }

        customer.SoftDelete();
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Customer {customer.CustomerNo} deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<(Customer? Customer, IActionResult? Redirect)> LoadForActionAsync(string id)
    {
        var org = await GetOrgAsync();
        if (org == null) return (null, RedirectToAction("Index", "Organization"));
        if (!TryDecodeId(id, out var customerId)) return (null, NotFound());

        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == customerId && c.OrganizationId == org.Id && !c.IsDeleted);
        return customer == null ? (null, NotFound()) : (customer, null);
    }
}
