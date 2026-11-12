// src/Microbanker.Web/Controllers/SelfRegistrationController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Entities.Customers;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Models;
using Microbanker.Web.Services;
using Microbanker.Domain.Security;
using Microbanker.Web.Security;

namespace Microbanker.Web.Controllers;

[RequirePermission(Modules.SelfReg)]
public class SelfRegistrationController : BankingControllerBase
{
    public SelfRegistrationController(MicrobankerDbContext db, IEncryptionService enc) : base(db, enc) { }

    private void SetNav()
    {
        ViewData["ActiveGroup"] = "Banking";
        ViewData["ActivePage"]  = "SelfReg";
    }

    // ── List ──────────────────────────────────────────────────────────────────

    public async Task<IActionResult> Index(string? status, int page = 1)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var query = _db.SelfRegistrations.Include(r => r.Branch)
            .Where(r => r.OrganizationId == org.Id && !r.IsDeleted);
        if (!string.IsNullOrWhiteSpace(status) && CustomerOptions.SelfRegStatuses.Contains(status))
            query = query.Where(r => r.Status == status);

        var total = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        page = Math.Clamp(page, 1, totalPages);

        var rows = await query.OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();

        return View(new SelfRegListViewModel
        {
            Status = status, Page = page, TotalPages = totalPages, TotalCount = total,
            Statuses = StatusItems(CustomerOptions.SelfRegStatuses),
            Rows = rows.Select(r => new SelfRegRowViewModel
            {
                EncryptedId = _enc.Encrypt(r.Id.ToString()),
                DisplayName = r.DisplayName, CustomerType = r.CustomerType, BranchName = r.Branch.Name,
                Phone = r.MobileNo ?? r.Telephone, Status = r.Status, CreatedAt = r.CreatedAt
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

        var model = new SelfRegFormModel();
        await FillListsAsync(model, org.Id);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SelfRegFormModel model)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        await ValidateProfileAsync(model, org.Id);
        if (!ModelState.IsValid)
        {
            await FillListsAsync(model, org.Id);
            return View(model);
        }

        var profile = ProfileMapper.ToProfile(model);
        var reg = new SelfRegistration(org.Id, profile.BranchId, profile.CustomerType);
        reg.Update(profile);
        _db.SelfRegistrations.Add(reg);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Registration submitted and is awaiting review.";
        return RedirectToAction("View", new { id = _enc.Encrypt(reg.Id.ToString()) });
    }

    // ── Edit (pending only) ───────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var regId)) return NotFound();

        var reg = await _db.SelfRegistrations.FirstOrDefaultAsync(r => r.Id == regId && r.OrganizationId == org.Id && !r.IsDeleted);
        if (reg == null) return NotFound();
        if (reg.Status != CustomerOptions.Pending)
        {
            TempData["Error"] = "Only a pending registration can be edited.";
            return RedirectToAction("View", new { id });
        }

        var model = new SelfRegFormModel { EncryptedId = id };
        ProfileMapper.Fill(model, reg);
        await FillListsAsync(model, org.Id);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, SelfRegFormModel model)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var regId)) return NotFound();

        var reg = await _db.SelfRegistrations.FirstOrDefaultAsync(r => r.Id == regId && r.OrganizationId == org.Id && !r.IsDeleted);
        if (reg == null) return NotFound();
        if (reg.Status != CustomerOptions.Pending)
        {
            TempData["Error"] = "Only a pending registration can be edited.";
            return RedirectToAction("View", new { id });
        }

        model.CustomerType = reg.CustomerType;   // fixed at creation
        await ValidateProfileAsync(model, org.Id);
        if (!ModelState.IsValid)
        {
            model.EncryptedId = id;
            await FillListsAsync(model, org.Id);
            return View(model);
        }

        reg.Update(ProfileMapper.ToProfile(model));
        await _db.SaveChangesAsync();
        TempData["Success"] = "Registration updated successfully.";
        return RedirectToAction("View", new { id });
    }

    // ── View ──────────────────────────────────────────────────────────────────

    [HttpGet, ActionName("View")]
    public async Task<IActionResult> Details(string id)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var regId)) return NotFound();

        var r = await _db.SelfRegistrations
            .Include(x => x.Branch).Include(x => x.Region).Include(x => x.District)
            .Include(x => x.Area).Include(x => x.Sector).Include(x => x.SubSector)
            .FirstOrDefaultAsync(x => x.Id == regId && x.OrganizationId == org.Id && !x.IsDeleted);
        if (r == null) return NotFound();

        var vm = new SelfRegDetailViewModel
        {
            EncryptedId = id, Status = r.Status, RejectionReason = r.RejectionReason,
            ReviewedAt = r.ReviewedAt, ReviewedByName = await UserNameAsync(r.ReviewedBy)
        };
        if (r.CustomerId != null)
        {
            vm.CustomerEncryptedId = _enc.Encrypt(r.CustomerId.Value.ToString());
            vm.CustomerNo = await _db.Customers.Where(c => c.Id == r.CustomerId).Select(c => c.CustomerNo).FirstOrDefaultAsync();
        }
        FillDetail(vm, r);
        return View("View", vm);
    }

    // ── Approve: creates the customer ─────────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(string id)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var regId)) return NotFound();

        var reg = await _db.SelfRegistrations.FirstOrDefaultAsync(r => r.Id == regId && r.OrganizationId == org.Id && !r.IsDeleted);
        if (reg == null) return NotFound();
        if (reg.Status != CustomerOptions.Pending)
        {
            TempData["Error"] = "This registration has already been reviewed.";
            return RedirectToAction("View", new { id });
        }

        // Same checks a staff-entered customer gets, applied to the stored data.
        if (await NationalIdTakenAsync(org.Id, reg.NationalId, Guid.Empty))
        {
            TempData["Error"] = "Another customer already has this national ID. Edit the registration or reject it.";
            return RedirectToAction("View", new { id });
        }
        if (!await _db.Branches.AnyAsync(b => b.Id == reg.BranchId && !b.IsDeleted && b.IsActive))
        {
            TempData["Error"] = "The selected branch is no longer active. Edit the registration and choose another.";
            return RedirectToAction("View", new { id });
        }

        var reviewer = GetUserId();
        // One transaction: either the customer exists and the registration is approved, or neither.
        await using var tx = await _db.Database.BeginTransactionAsync();
        var customer = await AddCustomerAsync(org.Id, reg.ToProfile(), reviewer);
        // The reviewer is also the approver of the customer record created from the registration.
        customer.Approve(reviewer, "Created from approved self-registration.");
        reg.Approve(reviewer, customer.Id);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        TempData["Success"] = $"Registration approved. Customer {customer.CustomerNo} created and active.";
        return RedirectToAction("View", new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission(Modules.SelfReg, PermissionActions.Approve)]
    public async Task<IActionResult> Reject(string id, string? reason)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var regId)) return NotFound();

        var reg = await _db.SelfRegistrations.FirstOrDefaultAsync(r => r.Id == regId && r.OrganizationId == org.Id && !r.IsDeleted);
        if (reg == null) return NotFound();

        if (reg.Status != CustomerOptions.Pending)
            TempData["Error"] = "This registration has already been reviewed.";
        else if (string.IsNullOrWhiteSpace(reason))
            TempData["Error"] = "Please give a reason for rejecting the registration.";
        else
        {
            reg.Reject(GetUserId(), reason.Trim());
            await _db.SaveChangesAsync();
            TempData["Success"] = "Registration rejected.";
        }
        return RedirectToAction("View", new { id });
    }

    // ── Delete (pending or rejected only) ─────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var regId)) return NotFound();

        var reg = await _db.SelfRegistrations.FirstOrDefaultAsync(r => r.Id == regId && r.OrganizationId == org.Id && !r.IsDeleted);
        if (reg == null) return NotFound();

        if (reg.Status == CustomerOptions.Approved)
        {
            TempData["Error"] = "An approved registration is the audit trail for its customer and cannot be deleted.";
            return RedirectToAction("View", new { id });
        }

        reg.SoftDelete();
        await _db.SaveChangesAsync();
        TempData["Success"] = "Registration deleted.";
        return RedirectToAction(nameof(Index));
    }
}
