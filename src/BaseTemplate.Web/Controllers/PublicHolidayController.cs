// src/BaseTemplate.Web/Controllers/PublicHolidayController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BaseTemplate.Domain.Entities.Calendar;
using BaseTemplate.Infrastructure.Persistence;
using BaseTemplate.Web.Services;
using BaseTemplate.Web.Models;

namespace BaseTemplate.Web.Controllers;

[RequirePermission(Modules.PublicHolidays)]
public class PublicHolidayController : BaseController
{
    private readonly AppDbContext      _db;
    private readonly IEncryptionService _enc;

    public PublicHolidayController(AppDbContext db, IEncryptionService enc)
    {
        _db  = db;
        _enc = enc;
    }

    public async Task<IActionResult> Index(int? year)
    {
        ViewData["ActivePage"]  = "PublicHoliday";
        ViewData["ActiveGroup"] = "Calendar";

        var org = await _db.Organizations.Where(o => !o.IsDeleted).FirstOrDefaultAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var y = year ?? DateTime.Today.Year;

        var list = await _db.PublicHolidays
            .Where(h => h.OrganizationId == org.Id && !h.IsDeleted && h.HolidayDate.Year == y)
            .OrderBy(h => h.HolidayDate)
            .ToListAsync();

        var vm = new PublicHolidayListViewModel
        {
            FilterYear = y,
            Holidays = list.Select(h => new PublicHolidayRowModel
            {
                EncryptedId = _enc.Encrypt(h.Id.ToString()),
                Name        = h.Name,
                Date        = h.HolidayDate.ToString("dd MMM yyyy"),
                DayOfWeek   = h.HolidayDate.ToString("dddd"),
                Type        = h.HolidayType == HolidayType.National ? "National" : "Local",
                TypeBadge   = h.HolidayType == HolidayType.National
                                ? "badge-subtle-primary" : "badge-subtle-info",
                IsRecurring = h.IsRecurring,
                IsActive    = h.IsActive,
                Description = h.Description,
            }).ToList()
        };

        return View(vm);
    }

    public IActionResult Create()
    {
        ViewData["ActivePage"]  = "PublicHoliday";
        ViewData["ActiveGroup"] = "Calendar";
        return View(new PublicHolidayFormModel { Date = DateTime.Today.ToString("yyyy-MM-dd"), IsActive = true });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PublicHolidayFormModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var org = await _db.Organizations.Where(o => !o.IsDeleted).FirstOrDefaultAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var date    = DateTime.Parse(model.Date);
        var holiday = new PublicHoliday(org.Id, model.Name.Trim(), date, (HolidayType)model.HolidayType);
        holiday.Update(model.Name.Trim(), date, (HolidayType)model.HolidayType,
            model.IsRecurring, model.Description?.Trim() ?? "", model.IsActive);

        _db.PublicHolidays.Add(holiday);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Holiday '{holiday.Name}' added.";
        return RedirectToAction(nameof(Index), new { year = date.Year });
    }

    public async Task<IActionResult> Edit(string id)
    {
        ViewData["ActivePage"]  = "PublicHoliday";
        ViewData["ActiveGroup"] = "Calendar";

        Guid hid;
        try { hid = Guid.Parse(_enc.Decrypt(id)); } catch { return NotFound(); }

        var h = await _db.PublicHolidays.FindAsync(hid);
        if (h == null || h.IsDeleted) return NotFound();

        return View(new PublicHolidayFormModel
        {
            EncryptedId = id,
            Name        = h.Name,
            Date        = h.HolidayDate.ToString("yyyy-MM-dd"),
            HolidayType = (int)h.HolidayType,
            IsRecurring = h.IsRecurring,
            Description = h.Description,
            IsActive    = h.IsActive,
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PublicHolidayFormModel model)
    {
        if (!ModelState.IsValid) return View(model);

        Guid hid;
        try { hid = Guid.Parse(_enc.Decrypt(model.EncryptedId)); } catch { return NotFound(); }

        var h = await _db.PublicHolidays.FindAsync(hid);
        if (h == null || h.IsDeleted) return NotFound();

        var date = DateTime.Parse(model.Date);
        h.Update(model.Name.Trim(), date, (HolidayType)model.HolidayType,
            model.IsRecurring, model.Description?.Trim() ?? "", model.IsActive);

        await _db.SaveChangesAsync();

        TempData["Success"] = $"Holiday '{h.Name}' updated.";
        return RedirectToAction(nameof(Index), new { year = date.Year });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        Guid hid;
        try { hid = Guid.Parse(_enc.Decrypt(id)); } catch { return NotFound(); }

        var h = await _db.PublicHolidays.FindAsync(hid);
        if (h == null || h.IsDeleted) return NotFound();

        h.SoftDelete();
        await _db.SaveChangesAsync();

        TempData["Success"] = "Holiday deleted.";
        return RedirectToAction(nameof(Index), new { year = h.HolidayDate.Year });
    }
}
