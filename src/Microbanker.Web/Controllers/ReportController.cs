// src/Microbanker.Web/Controllers/ReportController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Entities.GeneralLedger;
using Microbanker.Domain.Entities.Loans;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Models;
using Microbanker.Web.Services;

namespace Microbanker.Web.Controllers;

public class ReportController : SetupControllerBase
{
    public ReportController(MicrobankerDbContext db, IEncryptionService enc) : base(db, enc) { }

    private void SetNav()
    {
        ViewData["ActiveGroup"] = "Reports";
        ViewData["ActivePage"]  = "Reports";
    }

    public IActionResult Index()
    {
        SetNav();
        return View();
    }

    // ── Trial balance ─────────────────────────────────────────────────────────

    public async Task<IActionResult> TrialBalance(DateOnly? asOf, bool beforeClosing = false)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var date = asOf ?? (await BusinessCalendar.GetAsync(_db, org.Id)).Date;
        var entries = _db.GLEntries.Where(g => g.OrganizationId == org.Id && g.ValueDate <= date);
        if (beforeClosing) entries = entries.Where(g => !g.JournalRef.StartsWith(GLOptions.ClosingJournalPrefix));

        var sums = await entries.GroupBy(g => new { g.GLAccountNo, g.DrCr })
            .Select(g => new { g.Key.GLAccountNo, g.Key.DrCr, Total = g.Sum(x => x.Amount) }).ToListAsync();
        var names = await _db.GLAccounts.AsNoTracking().Where(g => g.OrganizationId == org.Id && !g.IsDeleted)
            .Select(g => new { g.AccountNumber, g.AccountName, g.AccountType }).ToListAsync();

        var rows = new List<TrialBalanceRowViewModel>();
        foreach (var no in sums.Select(s => s.GLAccountNo).Distinct().OrderBy(n => n))
        {
            var net = sums.Where(s => s.GLAccountNo == no && s.DrCr == GLOptions.Debit).Sum(s => s.Total)
                    - sums.Where(s => s.GLAccountNo == no && s.DrCr == GLOptions.Credit).Sum(s => s.Total);
            if (net == 0) continue;
            var info = names.FirstOrDefault(n => n.AccountNumber == no);
            rows.Add(new TrialBalanceRowViewModel
            {
                GlAccountNo = no, Name = info?.AccountName ?? "(account no longer in the chart)", Type = info?.AccountType ?? "",
                Debit = net > 0 ? net : 0m, Credit = net < 0 ? -net : 0m
            });
        }

        return View(new TrialBalanceViewModel
        {
            AsOf = date, BeforeClosing = beforeClosing, Rows = rows,
            TotalDebit = rows.Sum(r => r.Debit), TotalCredit = rows.Sum(r => r.Credit)
        });
    }

    // ── Customer account statement ────────────────────────────────────────────

    public async Task<IActionResult> AccountStatement(string? accountNo, DateOnly? from, DateOnly? to)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var today = (await BusinessCalendar.GetAsync(_db, org.Id)).Date;
        var vm = new AccountStatementViewModel
        {
            AccountNo = accountNo?.Trim().ToUpperInvariant(), From = from ?? new DateOnly(today.Year, today.Month, 1), To = to ?? today
        };
        if (string.IsNullOrEmpty(vm.AccountNo)) return View(vm);

        vm.Searched = true;
        var account = await _db.MemberAccounts.AsNoTracking().Include(a => a.Customer)
            .FirstOrDefaultAsync(a => a.OrganizationId == org.Id && !a.IsDeleted && a.AccountNo == vm.AccountNo);
        if (account == null)
        {
            vm.NotFound = true;
            return View(vm);
        }
        vm.AccountName = account.AccountName;
        vm.CustomerName = account.Customer.DisplayName;
        vm.Currency = account.Currency;
        vm.Status = account.Status;

        var all = _db.Transactions.AsNoTracking().Where(t => t.OrganizationId == org.Id && t.AccountId == account.Id);

        // Opening balance: the balance after the last transaction before the range, or the opening balance if there is none.
        var before = await all.Where(t => t.ValueDate < vm.From).OrderByDescending(t => t.PostingDate).ThenByDescending(t => t.TransactionRef)
            .Select(t => (decimal?)t.BalanceAfter).FirstOrDefaultAsync();
        vm.OpeningBalance = before ?? account.OpeningBalance;

        var lines = await all.Where(t => t.ValueDate >= vm.From && t.ValueDate <= vm.To)
            .OrderBy(t => t.PostingDate).ThenBy(t => t.TransactionRef).ToListAsync();
        vm.Lines = lines.Select(t => new AccountStatementLineViewModel
        {
            ValueDate = t.ValueDate, TransactionRef = t.TransactionRef, Narration = t.Narration, Status = t.Status, IsReversal = t.IsReversal,
            Debit = t.DrCr == "DR" ? t.Amount : 0m, Credit = t.DrCr == "CR" ? t.Amount : 0m, Balance = t.BalanceAfter
        }).ToList();
        vm.TotalDebits = vm.Lines.Sum(l => l.Debit);
        vm.TotalCredits = vm.Lines.Sum(l => l.Credit);
        vm.ClosingBalance = vm.Lines.Count > 0 ? vm.Lines[^1].Balance : vm.OpeningBalance;
        return View(vm);
    }

    /// <summary>Goes to the account's mini statement by account number.</summary>
    public async Task<IActionResult> MiniStatement(string? accountNo)
    {
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");
        var no = accountNo?.Trim().ToUpperInvariant();
        var id = string.IsNullOrEmpty(no) ? Guid.Empty
            : await _db.MemberAccounts.Where(a => a.OrganizationId == org.Id && !a.IsDeleted && a.AccountNo == no).Select(a => a.Id).FirstOrDefaultAsync();
        if (id == Guid.Empty)
        {
            TempData["Error"] = string.IsNullOrEmpty(no) ? "Enter an account number." : $"Account {no} was not found.";
            return RedirectToAction(nameof(Index));
        }
        return RedirectToAction("MiniStatement", "Account", new { id = _enc.Encrypt(id.ToString()) });
    }

    // ── Loan repayment schedule ───────────────────────────────────────────────

    public async Task<IActionResult> LoanSchedule(string? loanNo)
    {
        SetNav();
        var org = await GetOrgAsync();
        if (org == null) return RedirectToAction("Index", "Organization");

        var vm = new LoanScheduleReportViewModel { LoanNo = loanNo?.Trim().ToUpperInvariant() };
        if (string.IsNullOrEmpty(vm.LoanNo)) return View(vm);

        vm.Searched = true;
        var loan = await _db.LoanAccounts.AsNoTracking().Include(l => l.Customer)
            .FirstOrDefaultAsync(l => l.OrganizationId == org.Id && !l.IsDeleted && l.LoanNo == vm.LoanNo);
        if (loan == null)
        {
            vm.NotFound = true;
            return View(vm);
        }

        vm.LoanName = loan.LoanName; vm.CustomerName = loan.Customer.DisplayName; vm.Currency = loan.Currency; vm.Status = loan.Status;
        vm.LoanAmount = loan.LoanAmount; vm.InterestRate = loan.InterestRate; vm.Method = loan.InterestCalcMethod;

        var today = (await BusinessCalendar.GetAsync(_db, org.Id)).Date;
        if (loan.IsDisbursed)
        {
            var rows = await _db.LoanRepayments.AsNoTracking().Where(r => r.LoanAccountId == loan.Id).OrderBy(r => r.InstallmentNo).ToListAsync();
            vm.Rows = rows.Select(r => new ScheduleRowViewModel
            {
                No = r.InstallmentNo, DueDate = r.DueDate, PrincipalDue = r.PrincipalDue, InterestDue = r.InterestDue, ChargesDue = r.ChargesDue,
                TotalDue = r.TotalDue, TotalPaid = r.TotalDue - r.TotalRemaining, Remaining = r.TotalRemaining, Status = r.Status, IsOverdue = r.IsOverdueOn(today)
            }).ToList();
        }
        else if (loan.Status is LoanOptions.Pending or LoanOptions.Approved && loan.Tenure > 0 && loan.NoOfInstallments > 0)
        {
            vm.IsPreview = true;
            vm.Rows = loan.PreviewSchedule(today).Select(s => new ScheduleRowViewModel
            {
                No = s.No, DueDate = s.DueDate, PrincipalDue = s.Principal, InterestDue = s.Interest, ChargesDue = s.Charges, TotalDue = s.Total, Remaining = s.Total
            }).ToList();
        }

        vm.TotalPrincipal = vm.Rows.Sum(r => r.PrincipalDue);
        vm.TotalInterest = vm.Rows.Sum(r => r.InterestDue);
        vm.TotalCharges = vm.Rows.Sum(r => r.ChargesDue);
        vm.TotalDue = vm.Rows.Sum(r => r.TotalDue);
        vm.TotalPaid = vm.Rows.Sum(r => r.TotalPaid);
        return View(vm);
    }
}
