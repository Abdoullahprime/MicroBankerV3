// src/Microbanker.Web/Controllers/ProcessingController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Entities.GeneralLedger;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Models;
using Microbanker.Web.Services;

namespace Microbanker.Web.Controllers;

/// <summary>Start-of-day, end-of-day, month-end and year-end. The work itself is in <see cref="IProcessingService"/>.</summary>
public class ProcessingController : SetupControllerBase
{
    private readonly IProcessingService _processing;

    public ProcessingController(MicrobankerDbContext db, IEncryptionService enc, IProcessingService processing) : base(db, enc)
        => _processing = processing;

    private void SetNav()
    {
        ViewData["ActiveGroup"] = "Reports";
        ViewData["ActivePage"]  = "Processing";
    }

    // ── Start of day ──────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> SOD()
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        var vm = await FillAsync(new SodViewModel(), org.Id);
        vm.FirstDate = DateOnly.FromDateTime(DateTime.UtcNow);
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SOD(DateOnly? firstDate)
        => await RunAsync(org => _processing.StartDayAsync(org, GetUserId(), firstDate), nameof(SOD));

    // ── End of day ────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> EOD()
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        return View(await FillAsync(new ProcessingStateViewModel(), org.Id));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EOD(bool confirm)
        => await RunAsync(org => _processing.RunEodAsync(org, GetUserId()), nameof(EOD));

    // ── Month end ─────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> EOM()
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        return View(await FillAsync(new ProcessingStateViewModel(), org.Id));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EOM(bool confirm)
        => await RunAsync(org => _processing.RunEomAsync(org, GetUserId()), nameof(EOM));

    // ── Year end ──────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> EOY()
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var vm = await FillAsync(new EoyViewModel(), org.Id);
        var preview = await _processing.PreviewEoyAsync(org.Id);
        vm.Year = preview.Problem == null ? preview.Year : null;
        vm.Problem = preview.Problem;
        vm.NetProfit = preview.NetProfit;
        vm.Lines = preview.Lines.Select(l => new GLBalanceRowViewModel
        {
            GlAccountNo = l.GlAccountNo, Name = l.Name, Type = l.Type,
            // Shown as the balance to be cleared: income on the credit side, expense on the debit side.
            CreditTotal = l.Type == "INCOME" ? l.Balance : 0m, DebitTotal = l.Type == "EXPENSE" ? l.Balance : 0m
        }).ToList();
        vm.EquityAccounts = await _db.GLAccounts.AsNoTracking()
            .Where(g => g.OrganizationId == org.Id && !g.IsDeleted && g.IsActive && g.AccountType == "EQUITY").OrderBy(g => g.AccountNumber)
            .Select(g => new SelectListItem(g.AccountNumber + " — " + g.AccountName, g.AccountNumber)).ToListAsync();
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EOY(string? retainedEarningsGlNo)
        => await RunAsync(org => _processing.RunEoyAsync(org, GetUserId(), retainedEarningsGlNo), nameof(EOY));

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Runs a routine and comes back to its page with the outcome (post / redirect / get).</summary>
    private async Task<IActionResult> RunAsync(Func<Guid, Task<ProcessingResult>> run, string page)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var result = await run(org.Id);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        if (result.Notes.Count > 0) TempData["ProcessingNotes"] = string.Join("\n", result.Notes);
        return RedirectToAction(page);
    }

    private async Task<T> FillAsync<T>(T vm, Guid orgId) where T : ProcessingStateViewModel
    {
        var p = await _db.ProcessingDates.AsNoTracking().FirstOrDefaultAsync(x => x.OrganizationId == orgId && !x.IsDeleted);
        if (p != null)
        {
            vm.Initialised = true;
            vm.CurrentDate = p.CurrentDate;
            vm.Status = p.EODStatus;
            vm.LastSODAt = p.LastSODAt; vm.LastEODAt = p.LastEODAt; vm.LastEOMAt = p.LastEOMAt; vm.LastEOYAt = p.LastEOYAt;
            vm.ClosedThroughPeriod = p.ClosedThroughPeriod;

            var next = p.ClosedThroughPeriod == null ? GLOptions.PeriodOf(p.CurrentDate.AddDays(-1)) : GLOptions.NextPeriod(p.ClosedThroughPeriod);
            vm.NextEomPeriod = next;
            vm.EomPossible = p.EODStatus == GLOptions.Closed && GLOptions.PeriodEnd(next) < p.CurrentDate;
        }

        var logs = await _db.EODLogs.AsNoTracking().Where(l => l.OrganizationId == orgId && !l.IsDeleted)
            .OrderByDescending(l => l.StartedAt).Take(15).ToListAsync();
        var userIds = logs.Select(l => l.RunBy).Distinct().ToList();
        var users = await _db.AppUsers.AsNoTracking().Where(u => userIds.Contains(u.Id)).Select(u => new { u.Id, u.FirstName, u.LastName }).ToListAsync();
        vm.RecentLogs = logs.Select(l => new EodLogRowViewModel
        {
            ProcessType = l.ProcessType, ProcessDate = l.ProcessDate, StartedAt = l.StartedAt, CompletedAt = l.CompletedAt, Status = l.Status, Notes = l.Notes,
            RunByName = users.Where(u => u.Id == l.RunBy).Select(u => $"{u.FirstName} {u.LastName}".Trim()).FirstOrDefault()
        }).ToList();

        if (TempData["ProcessingNotes"] is string notes)
            vm.LastNotes = notes.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
        return vm;
    }
}
