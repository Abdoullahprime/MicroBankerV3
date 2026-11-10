// src/BaseTemplate.Web/Controllers/CalendarController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BaseTemplate.Domain.Entities.Calendar;
using BaseTemplate.Domain.Entities.Organization;
using BaseTemplate.Infrastructure.Persistence;
using BaseTemplate.Web.Services;
using BaseTemplate.Web.Models;

namespace BaseTemplate.Web.Controllers;

[RequirePermission(Modules.WorkCalendar)]
public class CalendarController : BaseController
{
    private readonly AppDbContext      _db;
    private readonly IEncryptionService _enc;

    public CalendarController(AppDbContext db, IEncryptionService enc)
    {
        _db  = db;
        _enc = enc;
    }

    private async Task<Organization?> GetOrgAsync()
    {
        return await _db.Organizations.Where(o => !o.IsDeleted).FirstOrDefaultAsync();
    }

    private static string MonthName(int m) =>
        new DateTime(2000, m, 1).ToString("MMMM");

    public async Task<IActionResult> Index()
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        ViewData["ActivePage"]  = "WorkCalendar";
        ViewData["ActiveGroup"] = "Calendar";

        var calendars = await _db.WorkCalendars
            .Where(c => c.OrganizationId == org.Id && !c.IsDeleted)
            .OrderByDescending(c => c.Year).ThenBy(c => c.Month)
            .ToListAsync();

        var vm = new CalendarIndexModel
        {
            Calendars = calendars.Select(c => new CalendarRowModel
            {
                EncryptedId  = _enc.Encrypt(c.Id.ToString()),
                Month        = c.Month,
                MonthName    = MonthName(c.Month),
                Year         = c.Year,
                WorkingDays  = c.WorkingDays,
                WorkingHours = c.WorkingHours
            }).ToList()
        };

        return View(vm);
    }

    public IActionResult Create()
    {
        ViewData["ActivePage"]  = "WorkCalendar";
        ViewData["ActiveGroup"] = "Calendar";
        return View(new CalendarFormModel { Year = DateTime.Today.Year });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CalendarFormModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var exists = await _db.WorkCalendars.AnyAsync(c =>
            c.OrganizationId == org.Id && c.Month == model.Month &&
            c.Year == model.Year && !c.IsDeleted);

        if (exists)
        {
            ModelState.AddModelError("", $"{MonthName(model.Month)} {model.Year} already has a calendar entry.");
            return View(model);
        }

        var cal = new WorkCalendar(org.Id, model.Month, model.Year, model.WorkingDays, model.WorkingHours);
        _db.WorkCalendars.Add(cal);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Calendar for {MonthName(model.Month)} {model.Year} created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var calId = Guid.Parse(_enc.Decrypt(id));
        var cal   = await _db.WorkCalendars.FindAsync(calId);
        if (cal == null || cal.IsDeleted) return NotFound();

        ViewData["ActivePage"]  = "WorkCalendar";
        ViewData["ActiveGroup"] = "Calendar";

        return View(new CalendarFormModel
        {
            EncryptedId  = id,
            Month        = cal.Month,
            Year         = cal.Year,
            WorkingDays  = cal.WorkingDays,
            WorkingHours = cal.WorkingHours
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, CalendarFormModel model)
    {
        model.EncryptedId = id;
        if (!ModelState.IsValid) return View(model);

        var calId = Guid.Parse(_enc.Decrypt(id));
        var cal   = await _db.WorkCalendars.FindAsync(calId);
        if (cal == null || cal.IsDeleted) return NotFound();

        cal.Update(model.WorkingDays, model.WorkingHours);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Calendar for {MonthName(cal.Month)} {cal.Year} updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var calId = Guid.Parse(_enc.Decrypt(id));
        var cal   = await _db.WorkCalendars.FindAsync(calId);
        if (cal == null || cal.IsDeleted) return NotFound();

        cal.SoftDelete();
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Calendar for {MonthName(cal.Month)} {cal.Year} removed.";
        return RedirectToAction(nameof(Index));
    }
}
