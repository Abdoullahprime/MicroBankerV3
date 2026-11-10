// src/Microbanker.Web/Controllers/OrganizationController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Entities.Organization;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Services;
using Microbanker.Web.Models;

namespace Microbanker.Web.Controllers;

public class OrganizationController : BaseController
{
    private readonly MicrobankerDbContext _db;
    private readonly IEncryptionService _enc;

    public OrganizationController(MicrobankerDbContext db, IEncryptionService enc)
    {
        _db = db;
        _enc = enc;
    }

    public async Task<IActionResult> Index()
    {
        var org = await _db.Organizations
            .Where(o => !o.IsDeleted)
            .FirstOrDefaultAsync();

        var vm = new OrganizationViewModel
        {
            HasOrganization    = org != null,
            EncryptedId        = org != null ? _enc.Encrypt(org.Id.ToString()) : null,
            Name               = org?.Name,
            ShortName          = org?.ShortName,
            Address            = org?.Address,
            Phone              = org?.Phone,
            Email              = org?.Email,
            Website            = org?.Website,
            RegistrationNumber = org?.RegistrationNumber,
            TaxNumber          = org?.TaxNumber,
            Industry           = org?.Industry,
            EstablishedDate    = org?.EstablishedDate
        };
        return View(vm);
    }

    public IActionResult Create() => View(new OrganizationFormModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrganizationFormModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var exists = await _db.Organizations
            .AnyAsync(o => !o.IsDeleted);
        if (exists)
        {
            ModelState.AddModelError("", "An organization already exists.");
            return View(model);
        }

        // Use a default code derived from short name or name
        var code = (!string.IsNullOrEmpty(model.ShortName)
            ? model.ShortName
            : model.Name.Length >= 6 ? model.Name[..6] : model.Name)
            .ToUpper().Replace(" ", "");

        var org = new Organization(model.Name, code);
        org.Update(model.Name, code, model.ShortName, model.Address,
            model.Phone, model.Phone, model.Email, model.Website,
            model.RegistrationNumber, model.TaxNumber, model.Industry,
            model.EstablishedDate, true);

        _db.Organizations.Add(org);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Organization created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var orgId = Guid.Parse(_enc.Decrypt(id));
        var org   = await _db.Organizations.FindAsync(orgId);
        if (org == null || org.IsDeleted) return NotFound();

        var model = new OrganizationFormModel
        {
            Name               = org.Name,
            ShortName          = org.ShortName,
            Address            = org.Address,
            Phone              = org.Phone,
            Email              = org.Email,
            Website            = org.Website,
            RegistrationNumber = org.RegistrationNumber,
            TaxNumber          = org.TaxNumber,
            Industry           = org.Industry,
            EstablishedDate    = org.EstablishedDate,
            EncryptedId        = id
        };
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, OrganizationFormModel model)
    {
        if (!ModelState.IsValid) { model.EncryptedId = id; return View(model); }

        var orgId = Guid.Parse(_enc.Decrypt(id));
        var org   = await _db.Organizations.FindAsync(orgId);
        if (org == null || org.IsDeleted) return NotFound();

        var code = (!string.IsNullOrEmpty(model.ShortName)
            ? model.ShortName
            : org.Code); // keep existing code on edit

        org.Update(model.Name, code, model.ShortName, model.Address,
            model.Phone, model.Phone, model.Email, model.Website,
            model.RegistrationNumber, model.TaxNumber, model.Industry,
            model.EstablishedDate, true);

        await _db.SaveChangesAsync();
        TempData["Success"] = "Organization updated successfully.";
        return RedirectToAction(nameof(Index));
    }
}
