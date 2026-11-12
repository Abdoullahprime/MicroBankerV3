// src/Microbanker.Web/Controllers/GLController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Entities.GeneralLedger;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Models;
using Microbanker.Web.Services;
using Microbanker.Domain.Security;
using Microbanker.Web.Security;

namespace Microbanker.Web.Controllers;

[RequirePermission(Modules.GeneralLedger)]
public class GLController : SetupControllerBase
{
    private const int MaxStatementLines = 2000;

    public GLController(MicrobankerDbContext db, IEncryptionService enc) : base(db, enc) { }

    private void SetNav()
    {
        ViewData["ActiveGroup"] = "Reports";
        ViewData["ActivePage"]  = "GL";
    }

    // ── Chart of accounts with balances ───────────────────────────────────────

    public async Task<IActionResult> Index(DateOnly? asOf, string? type, bool hideEmpty = false)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var date = asOf ?? (await BusinessCalendar.GetAsync(_db, org.Id)).Date;
        var accounts = await _db.GLAccounts.AsNoTracking()
            .Where(g => g.OrganizationId == org.Id && !g.IsDeleted && g.IsActive)
            .OrderBy(g => g.AccountNumber).ToListAsync();
        if (!string.IsNullOrWhiteSpace(type)) accounts = accounts.Where(a => a.AccountType == type).ToList();

        var sums = await _db.GLEntries.Where(g => g.OrganizationId == org.Id && g.ValueDate <= date)
            .GroupBy(g => new { g.GLAccountNo, g.DrCr })
            .Select(g => new { g.Key.GLAccountNo, g.Key.DrCr, Total = g.Sum(x => x.Amount) }).ToListAsync();
        decimal Sum(string no, string drCr) => sums.Where(s => s.GLAccountNo == no && s.DrCr == drCr).Sum(s => s.Total);

        var rows = accounts.Select(a => new GLBalanceRowViewModel
        {
            GlAccountNo = a.AccountNumber, Name = a.AccountName, Type = a.AccountType, Level = a.Level, IsControlAccount = a.IsControlAccount,
            DebitTotal = Sum(a.AccountNumber, GLOptions.Debit), CreditTotal = Sum(a.AccountNumber, GLOptions.Credit)
        }).ToList();
        if (hideEmpty) rows = rows.Where(r => r.DebitTotal != 0 || r.CreditTotal != 0).ToList();

        return View(new GLListViewModel
        {
            AsOf = date, Type = type, HideEmpty = hideEmpty, Rows = rows,
            TotalDebits = rows.Sum(r => r.DebitTotal), TotalCredits = rows.Sum(r => r.CreditTotal),
            Types = new[] { "ASSET", "LIABILITY", "EQUITY", "INCOME", "EXPENSE" }.Select(t => new SelectListItem(char.ToUpper(t[0]) + t[1..].ToLower(), t)).ToList()
        });
    }

    // ── One account's entries ─────────────────────────────────────────────────

    public async Task<IActionResult> AccountStatement(string? glAccountNo, DateOnly? from, DateOnly? to)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var today = (await BusinessCalendar.GetAsync(_db, org.Id)).Date;
        var vm = new GLStatementViewModel
        {
            GlAccountNo = glAccountNo?.Trim().ToUpperInvariant(), From = from ?? new DateOnly(today.Year, today.Month, 1), To = to ?? today,
            Accounts = await _db.GLAccounts.AsNoTracking().Where(g => g.OrganizationId == org.Id && !g.IsDeleted && g.IsActive)
                .OrderBy(g => g.AccountNumber).Select(g => new SelectListItem(g.AccountNumber + " — " + g.AccountName, g.AccountNumber)).ToListAsync()
        };
        if (string.IsNullOrEmpty(vm.GlAccountNo)) return View(vm);

        vm.Searched = true;
        var account = await _db.GLAccounts.AsNoTracking().FirstOrDefaultAsync(g => g.OrganizationId == org.Id && !g.IsDeleted && g.AccountNumber == vm.GlAccountNo);
        if (account == null)
        {
            vm.NotFound = true;
            return View(vm);
        }
        vm.Name = account.AccountName;
        vm.Type = account.AccountType;

        var entries = _db.GLEntries.AsNoTracking().Where(g => g.OrganizationId == org.Id && g.GLAccountNo == account.AccountNumber);

        // Opening position: everything dated before the start of the range.
        var before = await entries.Where(g => g.ValueDate < vm.From).GroupBy(g => g.DrCr)
            .Select(g => new { DrCr = g.Key, Total = g.Sum(x => x.Amount) }).ToListAsync();
        vm.OpeningNet = before.Where(b => b.DrCr == GLOptions.Debit).Sum(b => b.Total) - before.Where(b => b.DrCr == GLOptions.Credit).Sum(b => b.Total);

        var inRange = await entries.Where(g => g.ValueDate >= vm.From && g.ValueDate <= vm.To)
            .OrderBy(g => g.ValueDate).ThenBy(g => g.PostingDate).ThenBy(g => g.JournalRef).Take(MaxStatementLines + 1).ToListAsync();
        vm.Truncated = inRange.Count > MaxStatementLines;
        if (vm.Truncated) inRange = inRange.Take(MaxStatementLines).ToList();

        var running = vm.OpeningNet;
        foreach (var e in inRange)
        {
            running += e.DrCr == GLOptions.Debit ? e.Amount : -e.Amount;
            vm.Lines.Add(new GLStatementLineViewModel
            {
                ValueDate = e.ValueDate, JournalRef = e.JournalRef, SourceRef = e.SourceRef, SourceModule = e.SourceModule,
                Narration = e.Narration, Status = e.Status,
                Debit = e.DrCr == GLOptions.Debit ? e.Amount : 0m, Credit = e.DrCr == GLOptions.Credit ? e.Amount : 0m, Running = running
            });
        }
        vm.TotalDebits = vm.Lines.Sum(l => l.Debit);
        vm.TotalCredits = vm.Lines.Sum(l => l.Credit);
        vm.ClosingNet = running;
        return View(vm);
    }
}
