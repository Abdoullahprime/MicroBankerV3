// src/Microbanker.Web/Controllers/ChartOfAccountsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Entities.Setup;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Services;
using Microbanker.Web.Models;
using Microbanker.Domain.Security;
using Microbanker.Web.Security;

namespace Microbanker.Web.Controllers;

[RequirePermission(Modules.ChartOfAccounts)]
public class ChartOfAccountsController : SetupControllerBase
{
    public ChartOfAccountsController(MicrobankerDbContext db, IEncryptionService enc) : base(db, enc) { }

    private void SetNav()
    {
        ViewData["ActiveGroup"] = "Setup";
        ViewData["ActivePage"]  = "ChartOfAccounts";
    }

    public async Task<IActionResult> Index()
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var rows = await _db.GLAccounts.Include(x => x.Parent)
            .Where(x => x.OrganizationId == org.Id && !x.IsDeleted)
            .OrderBy(x => x.AccountNumber).ToListAsync();

        var vm = rows.Select(x => new GLAccountViewModel
        {
            EncryptedId = _enc.Encrypt(x.Id.ToString()),
            AccountNumber = x.AccountNumber, AccountName = x.AccountName, AccountType = x.AccountType,
            ParentLabel = x.Parent == null ? null : $"{x.Parent.AccountNumber} — {x.Parent.AccountName}",
            Level = x.Level, IsControlAccount = x.IsControlAccount, IsActive = x.IsActive
        }).ToList();

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var model = new GLAccountFormModel();
        await FillListsAsync(model, org.Id, Guid.Empty);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GLAccountFormModel model)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var number = model.AccountNumber.Trim().ToUpperInvariant();
        var type = (model.AccountType ?? "").Trim().ToUpperInvariant();
        var (parent, level) = await ValidateAsync(model, org.Id, Guid.Empty, number, type);

        if (!ModelState.IsValid)
        {
            await FillListsAsync(model, org.Id, Guid.Empty);
            return View(model);
        }

        var entity = new GLAccount(org.Id, number, model.AccountName.Trim(), type, parent?.Id, level);
        entity.Update(number, model.AccountName.Trim(), type, parent?.Id, level,
            model.IsControlAccount, model.IsActive, model.Description?.Trim());
        _db.GLAccounts.Add(entity);
        await _db.SaveChangesAsync();
        TempData["Success"] = "GL account created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var self)) return NotFound();

        var entity = await _db.GLAccounts.FirstOrDefaultAsync(x => x.Id == self && x.OrganizationId == org.Id && !x.IsDeleted);
        if (entity == null) return NotFound();

        var model = new GLAccountFormModel
        {
            EncryptedId = id,
            AccountNumber = entity.AccountNumber, AccountName = entity.AccountName, AccountType = entity.AccountType,
            ParentId = entity.ParentId, IsControlAccount = entity.IsControlAccount,
            Description = entity.Description, IsActive = entity.IsActive
        };
        await FillListsAsync(model, org.Id, self);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, GLAccountFormModel model)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var self)) return NotFound();

        var entity = await _db.GLAccounts.FirstOrDefaultAsync(x => x.Id == self && x.OrganizationId == org.Id && !x.IsDeleted);
        if (entity == null) return NotFound();

        var number = model.AccountNumber.Trim().ToUpperInvariant();
        var type = (model.AccountType ?? "").Trim().ToUpperInvariant();
        var (parent, level) = await ValidateAsync(model, org.Id, self, number, type);

        if (!ModelState.IsValid)
        {
            model.EncryptedId = id;
            await FillListsAsync(model, org.Id, self);
            return View(model);
        }

        entity.Update(number, model.AccountName.Trim(), type, parent?.Id, level,
            model.IsControlAccount, model.IsActive, model.Description?.Trim());

        // Descendants follow the (possibly new) level of this account.
        var all = await _db.GLAccounts.Where(x => x.OrganizationId == org.Id && !x.IsDeleted).ToListAsync();
        var kids = all.Where(x => x.ParentId != null).ToLookup(x => x.ParentId!.Value);
        var queue = new Queue<GLAccount>();
        queue.Enqueue(entity);
        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            foreach (var child in kids[node.Id])
            {
                if (child.Level != node.Level + 1) child.SetLevel(node.Level + 1);
                queue.Enqueue(child);
            }
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "GL account updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var self)) return NotFound();

        var entity = await _db.GLAccounts.FirstOrDefaultAsync(x => x.Id == self && x.OrganizationId == org.Id && !x.IsDeleted);
        if (entity == null) return NotFound();

        if (await _db.GLAccounts.AnyAsync(x => x.ParentId == self && !x.IsDeleted))
        {
            TempData["Error"] = "This account has child accounts and cannot be deleted.";
            return RedirectToAction(nameof(Index));
        }

        entity.SoftDelete();
        await _db.SaveChangesAsync();
        TempData["Success"] = "GL account deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Validates the form and returns the parent (if any) and the level this account will have.</summary>
    private async Task<(GLAccount? Parent, int Level)> ValidateAsync(
        GLAccountFormModel model, Guid orgId, Guid self, string number, string type)
    {
        if (!GLAccount.AccountTypes.Contains(type))
            ModelState.AddModelError(nameof(model.AccountType), "Please select a valid account type.");

        if (await _db.GLAccounts.AnyAsync(x => x.OrganizationId == orgId && !x.IsDeleted && x.Id != self && x.AccountNumber == number))
            ModelState.AddModelError(nameof(model.AccountNumber), "Another account already uses this number.");

        GLAccount? parent = null;
        if (model.ParentId != null)
        {
            parent = await _db.GLAccounts.FirstOrDefaultAsync(x =>
                x.Id == model.ParentId && x.OrganizationId == orgId && !x.IsDeleted);
            if (parent == null)
                ModelState.AddModelError(nameof(model.ParentId), "Please select a valid parent account.");
            else if (self != Guid.Empty && await IsSelfOrDescendantAsync(orgId, self, parent.Id))
                ModelState.AddModelError(nameof(model.ParentId), "An account cannot be its own parent or sit under one of its own children.");
        }

        return (parent, parent == null ? 1 : parent.Level + 1);
    }

    /// <summary>True when <paramref name="candidate"/> is <paramref name="self"/> or anywhere below it.</summary>
    private async Task<bool> IsSelfOrDescendantAsync(Guid orgId, Guid self, Guid candidate)
    {
        var parents = await _db.GLAccounts
            .Where(x => x.OrganizationId == orgId && !x.IsDeleted)
            .Select(x => new { x.Id, x.ParentId }).ToDictionaryAsync(x => x.Id, x => x.ParentId);

        var cur = (Guid?)candidate;
        var guard = 0;
        while (cur != null && guard++ <= parents.Count)
        {
            if (cur == self) return true;
            cur = parents.TryGetValue(cur.Value, out var p) ? p : null;
        }
        return false;
    }

    /// <summary>Fills the dropdowns; on edit the account itself and its descendants are excluded as parents.</summary>
    private async Task FillListsAsync(GLAccountFormModel model, Guid orgId, Guid self)
    {
        model.AccountTypes = Options(GLAccount.AccountTypes);

        var all = await _db.GLAccounts
            .Where(x => x.OrganizationId == orgId && !x.IsDeleted && x.IsActive)
            .OrderBy(x => x.AccountNumber).ToListAsync();

        var excluded = new HashSet<Guid>();
        if (self != Guid.Empty)
        {
            excluded.Add(self);
            var grew = true;
            while (grew)
            {
                grew = false;
                foreach (var a in all)
                    if (a.ParentId != null && excluded.Contains(a.ParentId.Value) && excluded.Add(a.Id))
                        grew = true;
            }
        }

        model.Parents = all.Where(a => !excluded.Contains(a.Id))
            .Select(a => new SelectListItem(
                $"{new string(' ', (a.Level - 1) * 3)}{a.AccountNumber} — {a.AccountName}", a.Id.ToString()))
            .ToList();
    }
}
