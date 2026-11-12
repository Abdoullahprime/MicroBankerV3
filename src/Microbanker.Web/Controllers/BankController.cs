// src/Microbanker.Web/Controllers/BankController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Entities.Organization;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Services;
using Microbanker.Web.Models;
using Microbanker.Domain.Security;
using Microbanker.Web.Security;

namespace Microbanker.Web.Controllers;

[RequirePermission(Modules.Banks)]
public class BankController : BaseController
{
    private readonly MicrobankerDbContext _db;
    private readonly IEncryptionService _enc;

    public BankController(MicrobankerDbContext db, IEncryptionService enc)
    {
        _db = db;
        _enc = enc;
    }

    private async Task<Organization?> GetOrgAsync()
    {
        var companyId = GetCompanyId();
        return await _db.Organizations
            .Where(o => !o.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<IActionResult> Index()
    {
        ViewData["ActiveGroup"] = "Org";
        ViewData["ActivePage"]  = "Banks";

        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var banks = await _db.Banks
            .Where(b => b.OrganizationId == org.Id && !b.IsDeleted)
            .OrderBy(b => b.Name)
            .ToListAsync();

        var vm = banks.Select(b => new BankViewModel
        {
            EncryptedId = _enc.Encrypt(b.Id.ToString()),
            Name        = b.Name,
            Code        = b.Code,
            BranchCode  = b.BranchCode,
            SwiftCode   = b.SwiftCode,
            IsActive    = b.IsActive
        }).ToList();

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewData["ActiveGroup"] = "Org";
        ViewData["ActivePage"]  = "Banks";

        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        return View(new BankFormModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BankFormModel model)
    {
        ViewData["ActiveGroup"] = "Org";
        ViewData["ActivePage"]  = "Banks";

        if (!ModelState.IsValid) return View(model);

        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var bank = new Bank(org.Id, model.Name, model.Code, model.BranchCode, model.SwiftCode);
        if (!model.IsActive)
            bank.Update(model.Name, model.Code, model.BranchCode, model.SwiftCode, false);

        _db.Banks.Add(bank);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Bank created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        ViewData["ActiveGroup"] = "Org";
        ViewData["ActivePage"]  = "Banks";

        var bankId = Guid.Parse(_enc.Decrypt(id));
        var bank   = await _db.Banks.FindAsync(bankId);
        if (bank == null || bank.IsDeleted) return NotFound();

        var model = new BankFormModel
        {
            EncryptedId = id,
            Name        = bank.Name,
            Code        = bank.Code,
            BranchCode  = bank.BranchCode,
            SwiftCode   = bank.SwiftCode,
            IsActive    = bank.IsActive
        };
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, BankFormModel model)
    {
        ViewData["ActiveGroup"] = "Org";
        ViewData["ActivePage"]  = "Banks";

        if (!ModelState.IsValid) { model.EncryptedId = id; return View(model); }

        var bankId = Guid.Parse(_enc.Decrypt(id));
        var bank   = await _db.Banks.FindAsync(bankId);
        if (bank == null || bank.IsDeleted) return NotFound();

        bank.Update(model.Name, model.Code, model.BranchCode, model.SwiftCode, model.IsActive);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Bank updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var bankId = Guid.Parse(_enc.Decrypt(id));
        var bank   = await _db.Banks.FindAsync(bankId);
        if (bank == null || bank.IsDeleted) return NotFound();
        bank.SoftDelete();
        await _db.SaveChangesAsync();
        TempData["Success"] = "Bank deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
