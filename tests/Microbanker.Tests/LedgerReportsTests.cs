using System.Security.Claims;
using Microbanker.Domain.Entities.Accounts;
using Microbanker.Domain.Entities.Customers;
using Microbanker.Domain.Entities.GeneralLedger;
using Microbanker.Domain.Entities.Loans;
using Microbanker.Domain.Entities.Organization;
using Microbanker.Domain.Entities.Setup;
using Microbanker.Domain.Entities.Transactions;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Controllers;
using Microbanker.Web.Models;
using Microbanker.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Microbanker.Tests;

/// <summary>Ledger, report and processing screens (controllers + real services) against in-memory SQLite.</summary>
public sealed class LedgerReportsTests : IDisposable
{
    private readonly SqliteConnection _conn = new("DataSource=:memory:");
    private readonly DbContextOptions<MicrobankerDbContext> _options;
    private readonly MicrobankerDbContext _db;
    private readonly IEncryptionService _enc;
    private readonly Organization _org = new("Test Org", "TST");
    private readonly Branch _branch;
    private readonly ProductDefinition _product;
    private readonly TransactionCode _dep, _wdl;
    private readonly MemberAccount _a1;
    private readonly Guid _user = Guid.NewGuid();
    private static readonly DateOnly Day = new(2026, 3, 10);

    public LedgerReportsTests()
    {
        _conn.Open();
        _options = new DbContextOptionsBuilder<MicrobankerDbContext>().UseSqlite(_conn).Options;
        _db = new MicrobankerDbContext(_options);
        _db.Database.EnsureCreated();
        _enc = new AesEncryptionService(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["EncryptionKey"] = "unit-test-key-0123456789-abcdefgh" }).Build());

        _db.Organizations.Add(_org);
        _branch = new Branch(_org.Id, "Main", "MN");
        _db.Branches.Add(_branch);
        var type = new ProductType(_org.Id, "Savings", "SAV", null);
        _db.ProductTypes.Add(type);
        _product = new ProductDefinition(_org.Id, type.Id, "Basic", "BSC");
        _db.ProductDefinitions.Add(_product);
        _dep = new TransactionCode(_org.Id, "DEP", "Deposit", "CR");
        _wdl = new TransactionCode(_org.Id, "WDL", "Withdrawal", "DR");
        _db.TransactionCodes.AddRange(_dep, _wdl);

        AddGl("1000", "Cash", "ASSET"); AddGl("2000", "Customer deposits", "LIABILITY", true);
        AddGl("4000", "Fee income", "INCOME"); AddGl("3000", "Retained earnings", "EQUITY");

        var cust = new Customer(_org.Id, _branch.Id, "CUST-1", "INDIVIDUAL", "Awa", "Jallow");
        cust.Update(new CustomerProfile { BranchId = _branch.Id, FirstName = "Awa", LastName = "Jallow" });
        _db.Customers.Add(cust);
        _a1 = new MemberAccount(_org.Id, _branch.Id, cust.Id, _product.Id, "MN-SA-0000001", "Awa Jallow", "SAVINGS");
        _a1.UpdateTerms(_product.Id, "GMD", 1000m, new DateOnly(2026, 1, 1));
        _a1.Update(_a1.AccountName, null, 0m, null, null, null, "2000", null, null, null, null);
        _a1.Approve(Guid.NewGuid());
        _db.MemberAccounts.Add(_a1);
        _db.SaveChanges();
    }

    public void Dispose() { _db.Dispose(); _conn.Dispose(); }

    private void AddGl(string no, string name, string type, bool control = false)
    {
        var g = new GLAccount(_org.Id, no, name, type, null, 1);
        if (control) g.Update(no, name, type, null, 1, true, true, null);
        _db.GLAccounts.Add(g);
    }

    private T Wire<T>(T c) where T : Controller
    {
        var http = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("companyId", _org.Id.ToString()), new Claim("userId", _user.ToString())
            }, "test"))
        };
        c.ControllerContext = new ControllerContext(new ActionContext(http, new RouteData(), new ControllerActionDescriptor()));
        c.TempData = new TempDataDictionary(http, new NullTempData());
        return c;
    }

    private PostingService Post() => new(_db);
    private ProcessingService Proc() => new(_db, new PostingService(_db));
    private GLController GL() => Wire(new GLController(_db, _enc));
    private ReportController Reports() => Wire(new ReportController(_db, _enc));
    private ProcessingController Processing() => Wire(new ProcessingController(_db, _enc, Proc()));
    private TellerController Teller() => Wire(new TellerController(_db, _enc, new PostingService(_db)));

    private async Task<PostingResult> Dep(decimal amount, DateOnly? date = null) => await Post().PostAsync(new PostingRequest
    {
        OrganizationId = _org.Id, PostedBy = _user, ValueDate = date ?? Day, Narration = "Cash in", CreditCodeId = _dep.Id,
        ContraGlAccountNo = "1000", Legs = new[] { new PostingLeg(_a1.AccountNo, "CR", amount) }
    });

    private async Task<PostingResult> Wdl(decimal amount, DateOnly? date = null) => await Post().PostAsync(new PostingRequest
    {
        OrganizationId = _org.Id, PostedBy = _user, ValueDate = date ?? Day, Narration = "Cash out", DebitCodeId = _wdl.Id,
        ContraGlAccountNo = "1000", Legs = new[] { new PostingLeg(_a1.AccountNo, "DR", amount) }
    });

    private async Task StartDay(DateOnly? d = null) => Assert.True((await Proc().StartDayAsync(_org.Id, _user, d ?? Day)).Success);

    // ═════════════ General ledger ═════════════

    [Fact]
    public async Task The_general_ledger_shows_balances_as_at_a_date_and_totals_balance()
    {
        await StartDay();
        await Dep(500m);
        await Wdl(200m);

        var vm = (GLListViewModel)((ViewResult)await GL().Index(Day, null)).Model!;
        var cash = vm.Rows.Single(r => r.GlAccountNo == "1000");
        var deposits = vm.Rows.Single(r => r.GlAccountNo == "2000");
        Assert.Equal((500m, 200m), (cash.DebitTotal, cash.CreditTotal));
        Assert.Equal(300m, cash.Net);                         // debit balance: cash held
        Assert.Equal(-300m, deposits.Net);                    // credit balance: owed to customers
        Assert.Equal(vm.TotalDebits, vm.TotalCredits);

        var before = (GLListViewModel)((ViewResult)await GL().Index(Day.AddDays(-1), null)).Model!;
        Assert.All(before.Rows, r => Assert.Equal(0m, r.Net));                   // nothing existed yet

        Assert.Equal(2, ((GLListViewModel)((ViewResult)await GL().Index(Day, null, hideEmpty: true)).Model!).Rows.Count);
        Assert.Single(((GLListViewModel)((ViewResult)await GL().Index(Day, "LIABILITY")).Model!).Rows);
    }

    [Fact]
    public async Task A_gl_account_statement_has_opening_running_and_closing_balances()
    {
        await StartDay();
        await Dep(500m);
        await Wdl(200m);

        var vm = (GLStatementViewModel)((ViewResult)await GL().AccountStatement("1000", Day, Day)).Model!;
        Assert.Equal("Cash", vm.Name);
        Assert.Equal(0m, vm.OpeningNet);
        Assert.Equal(new[] { 500m, 300m }, vm.Lines.Select(l => l.Running).ToArray());
        Assert.Equal(300m, vm.ClosingNet);
        Assert.Equal((500m, 200m), (vm.TotalDebits, vm.TotalCredits));

        var later = (GLStatementViewModel)((ViewResult)await GL().AccountStatement("1000", Day.AddDays(1), Day.AddDays(5))).Model!;
        Assert.Equal(300m, later.OpeningNet);
        Assert.Empty(later.Lines);
        Assert.Equal(300m, later.ClosingNet);

        Assert.True(((GLStatementViewModel)((ViewResult)await GL().AccountStatement("9999", null, null)).Model!).NotFound);
        Assert.False(((GLStatementViewModel)((ViewResult)await GL().AccountStatement(null, null, null)).Model!).Searched);
    }

    [Fact]
    public async Task A_reversed_posting_still_shows_in_the_ledger_and_nets_to_zero()
    {
        await StartDay();
        var posted = await Dep(500m);
        await Post().ReverseAsync(_org.Id, posted.Transactions[0].TransactionRef, _user, "Wrong");

        var vm = (GLStatementViewModel)((ViewResult)await GL().AccountStatement("1000", Day, Day)).Model!;
        Assert.Equal(2, vm.Lines.Count);
        Assert.Contains(vm.Lines, l => l.Status == "REVERSED");
        Assert.Equal(0m, vm.ClosingNet);
    }

    // ═════════════ Trial balance ═════════════

    [Fact]
    public async Task The_trial_balance_balances_and_respects_the_date()
    {
        await StartDay();
        await Dep(500m);
        await Wdl(100m);

        var tb = (TrialBalanceViewModel)((ViewResult)await Reports().TrialBalance(Day, false)).Model!;
        Assert.True(tb.InBalance);
        Assert.Equal(400m, tb.TotalDebit);
        Assert.Equal(400m, tb.TotalCredit);
        Assert.Equal(400m, tb.Rows.Single(r => r.GlAccountNo == "1000").Debit);
        Assert.Equal(400m, tb.Rows.Single(r => r.GlAccountNo == "2000").Credit);

        var early = (TrialBalanceViewModel)((ViewResult)await Reports().TrialBalance(Day.AddDays(-1), false)).Model!;
        Assert.Empty(early.Rows);
        Assert.True(early.InBalance);
    }

    [Fact]
    public async Task The_trial_balance_can_leave_out_year_end_closing_entries()
    {
        // Income of 100 earned in 2026, then closed to retained earnings.
        void Add(string journal, string gl, string drCr, decimal amount) =>
            _db.GLEntries.Add(new GLEntry(_org.Id, journal, gl, drCr, amount, "x", new DateOnly(2026, 6, 1), DateTime.UtcNow, _user, "SYSTEM", journal));
        Add("J1", "1000", "DR", 100m); Add("J1", "4000", "CR", 100m);
        Add("EOY-2026", "4000", "DR", 100m); Add("EOY-2026", "3000", "CR", 100m);
        await _db.SaveChangesAsync();

        var after = (TrialBalanceViewModel)((ViewResult)await Reports().TrialBalance(new DateOnly(2026, 12, 31), false)).Model!;
        Assert.DoesNotContain(after.Rows, r => r.GlAccountNo == "4000");                        // income cleared
        Assert.Equal(100m, after.Rows.Single(r => r.GlAccountNo == "3000").Credit);

        var before = (TrialBalanceViewModel)((ViewResult)await Reports().TrialBalance(new DateOnly(2026, 12, 31), true)).Model!;
        Assert.Equal(100m, before.Rows.Single(r => r.GlAccountNo == "4000").Credit);
        Assert.DoesNotContain(before.Rows, r => r.GlAccountNo == "3000");
        Assert.True(before.InBalance && after.InBalance);
    }

    // ═════════════ Customer statements ═════════════

    [Fact]
    public async Task The_account_statement_has_opening_and_closing_balances_and_shows_reversals()
    {
        await StartDay();
        await Dep(500m);                                                    // 1,500
        var w = await Wdl(200m);                                            // 1,300
        await Post().ReverseAsync(_org.Id, w.Transactions[0].TransactionRef, _user, "Mistake");   // 1,500

        var vm = (AccountStatementViewModel)((ViewResult)await Reports().AccountStatement(_a1.AccountNo.ToLowerInvariant(), Day, Day)).Model!;
        Assert.Equal(1000m, vm.OpeningBalance);                             // the opening balance, before any transaction
        Assert.Equal(3, vm.Lines.Count);
        Assert.Equal(new[] { 1500m, 1300m, 1500m }, vm.Lines.Select(l => l.Balance).ToArray());
        Assert.Equal(1500m, vm.ClosingBalance);
        Assert.Equal((200m, 700m), (vm.TotalDebits, vm.TotalCredits));
        Assert.Contains(vm.Lines, l => l.IsReversal);

        var next = (AccountStatementViewModel)((ViewResult)await Reports().AccountStatement(_a1.AccountNo, Day.AddDays(1), Day.AddDays(2))).Model!;
        Assert.Equal(1500m, next.OpeningBalance);                           // what the last transaction left
        Assert.Empty(next.Lines);
        Assert.Equal(1500m, next.ClosingBalance);

        Assert.True(((AccountStatementViewModel)((ViewResult)await Reports().AccountStatement("NOPE", null, null)).Model!).NotFound);
    }

    [Fact]
    public async Task The_mini_statement_report_redirects_to_the_account_page()
    {
        var ok = Assert.IsType<RedirectToActionResult>(await Reports().MiniStatement(_a1.AccountNo));
        Assert.Equal("Account", ok.ControllerName);
        Assert.Equal("MiniStatement", ok.ActionName);

        var missing = Assert.IsType<RedirectToActionResult>(await Reports().MiniStatement("NOPE"));
        Assert.Equal("Index", missing.ActionName);
    }

    [Fact]
    public async Task The_loan_schedule_report_shows_a_preview_then_the_real_schedule()
    {
        var cust = await _db.Customers.SingleAsync();
        var loan = new LoanAccount(_org.Id, _branch.Id, cust.Id, _product.Id, "LN-2026-000001", 1200m);
        loan.UpdateTerms(new LoanTerms { ProductDefinitionId = _product.Id, LoanName = "L", LoanAmount = 1200m, InterestRate = 12m, InterestCalcMethod = "FLAT", Tenure = 12, NoOfInstallments = 12 });
        _db.LoanAccounts.Add(loan);
        await _db.SaveChangesAsync();

        var preview = (LoanScheduleReportViewModel)((ViewResult)await Reports().LoanSchedule("ln-2026-000001")).Model!;
        Assert.True(preview.IsPreview);
        Assert.Equal(12, preview.Rows.Count);
        Assert.Equal(1344m, preview.TotalDue);

        loan.Approve(_user);
        _db.LoanRepayments.AddRange(loan.Disburse(_user, new DateOnly(2026, 1, 1)));
        await _db.SaveChangesAsync();
        var real = (LoanScheduleReportViewModel)((ViewResult)await Reports().LoanSchedule("LN-2026-000001")).Model!;
        Assert.False(real.IsPreview);
        Assert.Equal(12, real.Rows.Count);

        Assert.True(((LoanScheduleReportViewModel)((ViewResult)await Reports().LoanSchedule("NOPE")).Model!).NotFound);
    }

    // ═════════════ Processing screens ═════════════

    [Fact]
    public async Task The_processing_pages_start_the_day_run_end_of_day_and_report_state()
    {
        var first = (SodViewModel)((ViewResult)await Processing().SOD()).Model!;
        Assert.False(first.Initialised);

        Assert.Equal("SOD", Assert.IsType<RedirectToActionResult>(await Processing().SOD(Day)).ActionName);
        var open = (ProcessingStateViewModel)((ViewResult)await Processing().EOD()).Model!;
        Assert.True(open.Initialised);
        Assert.Equal("OPEN", open.Status);
        Assert.Equal(Day, open.CurrentDate);

        await Dep(10m);
        Assert.Equal("EOD", Assert.IsType<RedirectToActionResult>(await Processing().EOD(true)).ActionName);
        var closed = (ProcessingStateViewModel)((ViewResult)await Processing().EOD()).Model!;
        Assert.Equal("CLOSED", closed.Status);
        Assert.Equal(Day.AddDays(1), closed.CurrentDate);
        Assert.Equal(new[] { "EOD", "SOD" }, closed.RecentLogs.Select(l => l.ProcessType).ToArray());
    }

    [Fact]
    public async Task The_month_end_page_says_when_it_is_possible()
    {
        await StartDay(new DateOnly(2026, 3, 30));
        await Proc().RunEodAsync(_org.Id, _user);                           // 30 Mar closed, date 31 Mar
        var early = (ProcessingStateViewModel)((ViewResult)await Processing().EOM()).Model!;
        Assert.False(early.EomPossible);                                    // 31 March is not closed yet
        Assert.Equal("2026-03", early.NextEomPeriod);

        await Proc().StartDayAsync(_org.Id, _user, null);
        await Proc().RunEodAsync(_org.Id, _user);                           // 31 Mar closed, date 1 Apr
        var ready = (ProcessingStateViewModel)((ViewResult)await Processing().EOM()).Model!;
        Assert.True(ready.EomPossible);

        Assert.Equal("EOM", Assert.IsType<RedirectToActionResult>(await Processing().EOM(true)).ActionName);
        var after = (ProcessingStateViewModel)((ViewResult)await Processing().EOM()).Model!;
        Assert.Equal("2026-03", after.ClosedThroughPeriod);
        Assert.False(after.EomPossible);
    }

    [Fact]
    public async Task The_year_end_page_previews_what_will_be_closed()
    {
        await StartDay(new DateOnly(2027, 1, 1));
        var s = await _db.ProcessingDates.SingleAsync();
        typeof(ProcessingDate).GetProperty(nameof(ProcessingDate.ClosedThroughPeriod))!.SetValue(s, "2026-12");
        _db.GLEntries.Add(new GLEntry(_org.Id, "J1", "1000", "DR", 80m, "x", new DateOnly(2026, 5, 1), DateTime.UtcNow, _user, "SYSTEM", "J1"));
        _db.GLEntries.Add(new GLEntry(_org.Id, "J1", "4000", "CR", 80m, "x", new DateOnly(2026, 5, 1), DateTime.UtcNow, _user, "SYSTEM", "J1"));
        await _db.SaveChangesAsync();

        var vm = (EoyViewModel)((ViewResult)await Processing().EOY()).Model!;
        Assert.Null(vm.Problem);
        Assert.Equal(2026, vm.Year);
        Assert.Equal(80m, vm.NetProfit);
        Assert.Single(vm.EquityAccounts);

        Assert.Equal("EOY", Assert.IsType<RedirectToActionResult>(await Processing().EOY("3000")).ActionName);
        Assert.Contains(await _db.GLEntries.Select(g => g.JournalRef).ToListAsync(), j => j == "EOY-2026");
        Assert.NotNull(((EoyViewModel)((ViewResult)await Processing().EOY()).Model!).Problem);     // nothing left to close
    }

    // ═════════════ Business date in the teller screens ═════════════

    [Fact]
    public async Task Teller_forms_default_to_the_business_date()
    {
        await StartDay(new DateOnly(2026, 3, 10));
        var form = (CashPostingFormModel)((ViewResult)await Teller().Deposit((string?)null)).Model!;
        Assert.Equal(new DateOnly(2026, 3, 10), form.ValueDate);

        // Posting through the screen while the day is closed is refused with the reason.
        await Proc().RunEodAsync(_org.Id, _user);
        var c = Teller();
        var result = await c.Deposit(new CashPostingFormModel
        {
            AccountNo = _a1.AccountNo, TransactionCodeId = _dep.Id, Amount = 5m, Narration = "x", CashGlAccountNo = "1000"
        });
        Assert.IsType<ViewResult>(result);
        Assert.Contains("not open", c.ModelState[string.Empty]!.Errors[0].ErrorMessage);
    }

    private sealed class NullTempData : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
