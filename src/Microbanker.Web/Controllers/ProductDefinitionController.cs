// src/Microbanker.Web/Controllers/ProductDefinitionController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Entities.Setup;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Services;
using Microbanker.Web.Models;

namespace Microbanker.Web.Controllers;

public class ProductDefinitionController : SetupControllerBase
{
    public ProductDefinitionController(MicrobankerDbContext db, IEncryptionService enc) : base(db, enc) { }

    private void SetNav()
    {
        ViewData["ActiveGroup"] = "Setup";
        ViewData["ActivePage"]  = "ProductDefinitions";
    }

    public async Task<IActionResult> Index()
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var rows = await _db.ProductDefinitions.Include(x => x.ProductType)
            .Where(x => x.OrganizationId == org.Id && !x.IsDeleted)
            .OrderBy(x => x.ProductType.Name).ThenBy(x => x.Name).ToListAsync();

        var vm = rows.Select(x => new ProductDefinitionViewModel
        {
            EncryptedId = _enc.Encrypt(x.Id.ToString()),
            Name = x.Name, Code = x.Code, ProductTypeName = x.ProductType.Name,
            MinBalance = x.MinBalance, InterestRate = x.InterestRate,
            InterestCalcMethod = x.InterestCalcMethod, IsActive = x.IsActive
        }).ToList();

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var model = new ProductDefinitionFormModel();
        model.ProductTypes = await ProductTypeItemsAsync(org.Id);
        model.CalcMethods = Options(ProductDefinition.CalcMethods);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductDefinitionFormModel model)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var self = Guid.Empty;
        var code = NormCode(model.Code)!;
        var calc = string.IsNullOrWhiteSpace(model.InterestCalcMethod) ? null : model.InterestCalcMethod.Trim();
        if (calc != null && !ProductDefinition.CalcMethods.Contains(calc))
            ModelState.AddModelError(nameof(model.InterestCalcMethod), "Please select a valid calculation method.");
        if (await _db.ProductDefinitions.AnyAsync(x => x.OrganizationId == org.Id && !x.IsDeleted && x.Id != self && x.Code == code))
            ModelState.AddModelError(nameof(model.Code), "Another record already uses this code.");
        if (model.ProductTypeId != null && !await _db.ProductTypes.AnyAsync(x => x.Id == model.ProductTypeId && x.OrganizationId == org.Id && !x.IsDeleted && x.IsActive))
            ModelState.AddModelError(nameof(model.ProductTypeId), "Please select a valid product type.");

        if (!ModelState.IsValid)
        {
            model.ProductTypes = await ProductTypeItemsAsync(org.Id);
            model.CalcMethods = Options(ProductDefinition.CalcMethods);
            return View(model);
        }

        var entity = new ProductDefinition(org.Id, model.ProductTypeId!.Value, model.Name.Trim(), code);
        entity.Update(model.ProductTypeId!.Value, model.Name.Trim(), code, model.MinBalance, model.InterestRate, calc,
            model.InterestFrequency, model.DormancyDays, model.MaxDebitPerMonth, model.Description?.Trim(), model.IsActive);
        _db.ProductDefinitions.Add(entity);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Product definition created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var self)) return NotFound();

        var entity = await _db.ProductDefinitions.FirstOrDefaultAsync(x => x.Id == self && x.OrganizationId == org.Id && !x.IsDeleted);
        if (entity == null) return NotFound();

        var model = new ProductDefinitionFormModel
        {
            EncryptedId = id,
            ProductTypeId = entity.ProductTypeId, Name = entity.Name, Code = entity.Code,
            MinBalance = entity.MinBalance, InterestRate = entity.InterestRate,
            InterestCalcMethod = entity.InterestCalcMethod, InterestFrequency = entity.InterestFrequency,
            DormancyDays = entity.DormancyDays, MaxDebitPerMonth = entity.MaxDebitPerMonth,
            Description = entity.Description, IsActive = entity.IsActive
        };
        model.ProductTypes = await ProductTypeItemsAsync(org.Id);
        model.CalcMethods = Options(ProductDefinition.CalcMethods);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, ProductDefinitionFormModel model)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var self)) return NotFound();

        var entity = await _db.ProductDefinitions.FirstOrDefaultAsync(x => x.Id == self && x.OrganizationId == org.Id && !x.IsDeleted);
        if (entity == null) return NotFound();

        var code = NormCode(model.Code)!;
        var calc = string.IsNullOrWhiteSpace(model.InterestCalcMethod) ? null : model.InterestCalcMethod.Trim();
        if (calc != null && !ProductDefinition.CalcMethods.Contains(calc))
            ModelState.AddModelError(nameof(model.InterestCalcMethod), "Please select a valid calculation method.");
        if (await _db.ProductDefinitions.AnyAsync(x => x.OrganizationId == org.Id && !x.IsDeleted && x.Id != self && x.Code == code))
            ModelState.AddModelError(nameof(model.Code), "Another record already uses this code.");
        if (model.ProductTypeId != null && !await _db.ProductTypes.AnyAsync(x => x.Id == model.ProductTypeId && x.OrganizationId == org.Id && !x.IsDeleted && x.IsActive))
            ModelState.AddModelError(nameof(model.ProductTypeId), "Please select a valid product type.");

        if (!ModelState.IsValid)
        {
            model.EncryptedId = id;
            model.ProductTypes = await ProductTypeItemsAsync(org.Id);
            model.CalcMethods = Options(ProductDefinition.CalcMethods);
            return View(model);
        }

        entity.Update(model.ProductTypeId!.Value, model.Name.Trim(), code, model.MinBalance, model.InterestRate, calc,
            model.InterestFrequency, model.DormancyDays, model.MaxDebitPerMonth, model.Description?.Trim(), model.IsActive);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Product definition updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        if (!TryDecodeId(id, out var self)) return NotFound();

        var entity = await _db.ProductDefinitions.FirstOrDefaultAsync(x => x.Id == self && x.OrganizationId == org.Id && !x.IsDeleted);
        if (entity == null) return NotFound();

        entity.SoftDelete();
        await _db.SaveChangesAsync();
        TempData["Success"] = "Product definition deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
