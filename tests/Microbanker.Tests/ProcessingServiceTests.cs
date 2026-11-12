using Microbanker.Domain.Entities.Accounts;
using Microbanker.Domain.Entities.Customers;
using Microbanker.Domain.Entities.GeneralLedger;
using Microbanker.Domain.Entities.Organization;
using Microbanker.Domain.Entities.Setup;
using Microbanker.Domain.Entities.Transactions;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Microbanker.Tests;

/// <summary>Start/end of day, month-end and year-end against in-memory SQLite, with the real posting service.</summary>
public sealed class ProcessingServiceTests : IDisposable
{
    private readonly SqliteConnection _conn = new("DataSource=:memory:");
    private readonly DbContextOptions<MicrobankerDbContext> _options;
    private readonly MicrobankerDbContext _db;
    private readonly Organization _org = new("Test Org", "TST");
    private readonly Branch _branch;
    private readonly ProductType _type;
    private readonly ProductDefinition _product;
    private readonly TransactionCode _dep, _wdl;
    private readonly Guid _user = Guid.NewGuid();
    private static readonly DateOnly Start = new(2026, 1, 15);

    public ProcessingServiceTests()
    {
        _conn.Open();
        _options = new DbContextOptionsBuilder<MicrobankerDbContext>().UseSqlite(_conn).Options;
        _db = new MicrobankerDbContext(_options);
        _db.Database.EnsureCreated();

        _db.Organizations.Add(_org);
        _branch = new Branch(_org.Id, "Main", "MN");
        _db.Branches.Add(_branch);
        _type = new ProductType(_org.Id, "Savings", "SAV", null);
        _db.ProductTypes.Add(_type);
        _product = new ProductDefinition(_org.Id, _type.Id, "Basic", "BSC");
        _product.Update(_type.Id, "Basic", "BSC", 0m, 5.2m, "SIMPLE", 1, 0, 0, null, true);
        _db.ProductDefinitions.Add(_product);
        _dep = new TransactionCode(_org.Id, "DEP", "Deposit", "CR");
        _wdl = new TransactionCode(_org.Id, "WDL", "Withdrawal", "DR");
        _db.TransactionCodes.AddRange(_dep, _wdl);

        Gl("1000", "Cash", "ASSET"); Gl("2000", "Customer deposits", "LIABILITY", control: true);
        Gl("4000", "Fee income", "INCOME"); Gl("5000", "Interest expense", "EXPENSE"); Gl("3000", "Retained earnings", "EQUITY");
        _db.SaveChanges();
    }

    public void Dispose() { _db.Dispose(); _conn.Dispose(); }

    private void Gl(string no, string name, string type, bool control = false)
    {
        var g = new GLAccount(_org.Id, no, name, type, null, 1);
        if (control) g.Update(no, name, type, null, 1, true, true, null);
        _db.GLAccounts.Add(g);
    }

    private MemberAccount Account(string no, decimal balance, string? expenseGl = "5000", ProductDefinition? product = null, DateOnly? openDate = null)
    {
        var cust = new Customer(_org.Id, _branch.Id, "C-" + no, "INDIVIDUAL", "A", "B");
        cust.Update(new CustomerProfile { BranchId = _branch.Id, FirstName = "A", LastName = "B" });
        _db.Customers.Add(cust);
        var p = product ?? _product;
        var a = new MemberAccount(_org.Id, _branch.Id, cust.Id, p.Id, no, "Acc " + no, "SAVINGS");
        a.UpdateTerms(p.Id, "GMD", balance, openDate ?? new DateOnly(2026, 1, 1));
        a.Update(a.AccountName, null, 0m, null, null, null, "2000", null, expenseGl, null, null);
        a.Approve(Guid.NewGuid());
        _db.MemberAccounts.Add(a);
        _db.SaveChanges();
        return a;
    }

    private ProcessingService Svc(MicrobankerDbContext? db = null) { var d = db ?? _db; return new ProcessingService(d, new PostingService(d)); }
    private PostingService Post(MicrobankerDbContext? db = null) => new(db ?? _db);
    private MicrobankerDbContext Fresh() => new(_options);

    private async Task<ProcessingResult> StartFirst(DateOnly? d = null) => await Svc().StartDayAsync(_org.Id, _user, d ?? Start);
    private async Task<ProcessingDate> State() => await Fresh().ProcessingDates.AsNoTracking().SingleAsync();

    /// <summary>Runs end-of-day then start-of-day, n times.</summary>
    private async Task Days(int n)
    {
        for (var i = 0; i < n; i++)
        {
            var eod = await Svc().RunEodAsync(_org.Id, _user);
            Assert.True(eod.Success, eod.Message);
            var sod = await Svc().StartDayAsync(_org.Id, _user, null);
            Assert.True(sod.Success, sod.Message);
        }
    }

    // ═════════════ Start / end of day ═════════════

    [Fact]
    public async Task The_first_start_of_day_creates_the_calendar_and_opens_the_day()
    {
        var r = await StartFirst();
        Assert.True(r.Success, r.Message);
        var s = await State();
        Assert.Equal(Start, s.CurrentDate);
        Assert.Equal("OPEN", s.EODStatus);
        Assert.NotNull(s.LastSODAt);
        Assert.Equal("SOD", (await Fresh().EODLogs.SingleAsync()).ProcessType);

        Assert.False((await Svc().StartDayAsync(_org.Id, _user, null)).Success);                 // already open
    }

    [Fact]
    public async Task The_first_start_needs_a_date()
    {
        Assert.False((await Svc().StartDayAsync(_org.Id, _user, null)).Success);
        Assert.Empty(Fresh().ProcessingDates);
    }

    [Fact]
    public async Task End_of_day_needs_an_open_day()
    {
        Assert.False((await Svc().RunEodAsync(_org.Id, _user)).Success);          // never started
        await StartFirst();
        Assert.True((await Svc().RunEodAsync(_org.Id, _user)).Success);
        Assert.False((await Svc().RunEodAsync(_org.Id, _user)).Success);          // now closed
    }

    [Fact]
    public async Task End_of_day_moves_the_date_on_and_leaves_the_day_closed_until_start_of_day()
    {
        await StartFirst();
        var r = await Svc().RunEodAsync(_org.Id, _user);
        Assert.True(r.Success, r.Message);
        var s = await State();
        Assert.Equal(Start.AddDays(1), s.CurrentDate);
        Assert.Equal("CLOSED", s.EODStatus);
        Assert.NotNull(s.LastEODAt);

        await Days(0);
        Assert.True((await Svc().StartDayAsync(_org.Id, _user, null)).Success);
        Assert.Equal("OPEN", (await State()).EODStatus);
        Assert.Equal(new[] { "SOD", "EOD", "SOD" }, await Fresh().EODLogs.OrderBy(l => l.StartedAt).Select(l => l.ProcessType).ToListAsync());
    }

    [Fact]
    public async Task Postings_follow_the_business_day()
    {
        var a = Account("A1", 100m);
        PostingRequest Req(DateOnly d) => new()
        {
            OrganizationId = _org.Id, PostedBy = _user, ValueDate = d, Narration = "x", CreditCodeId = _dep.Id, ContraGlAccountNo = "1000",
            Legs = new[] { new PostingLeg("A1", "CR", 10m) }
        };

        // Before the calendar exists only the real date limits postings.
        Assert.True((await Post().PostAsync(Req(DateOnly.FromDateTime(DateTime.UtcNow)))).Success);

        await StartFirst();                                                         // business date 15 Jan 2026
        Assert.True((await Post().PostAsync(Req(Start))).Success);
        Assert.False((await Post().PostAsync(Req(Start.AddDays(1)))).Success);      // after the business date

        await Svc().RunEodAsync(_org.Id, _user);
        var closed = await Post().PostAsync(Req(Start));
        Assert.False(closed.Success);
        Assert.Contains("not open", closed.Error);

        await Svc().StartDayAsync(_org.Id, _user, null);
        Assert.True((await Post().PostAsync(Req(Start.AddDays(1)))).Success);
    }

    // ═════════════ Interest and dormancy ═════════════

    private static decimal Daily(decimal balance, decimal rate) => Math.Round(balance * rate / 100m / 365m, 6);

    [Fact]
    public async Task Interest_accrues_daily_on_the_closing_balance()
    {
        var a = Account("A1", 10000m);
        await StartFirst();
        await Days(2);
        var fresh = await Fresh().MemberAccounts.AsNoTracking().SingleAsync(x => x.Id == a.Id);
        Assert.Equal(Daily(10000m, 5.2m) * 2, fresh.InterestAccruedCR);
        Assert.Equal(10000m, fresh.BookBalance);                                   // nothing is paid until month-end
    }

    [Fact]
    public async Task Compound_interest_earns_on_interest_already_accrued()
    {
        var compound = new ProductDefinition(_org.Id, _type.Id, "Compound", "CMP");
        compound.Update(_type.Id, "Compound", "CMP", 0m, 5.2m, "COMPOUND", 1, 0, 0, null, true);
        _db.ProductDefinitions.Add(compound);
        await _db.SaveChangesAsync();
        var a = Account("A1", 10000m, product: compound);
        await StartFirst();
        await Days(2);

        var day1 = Daily(10000m, 5.2m);
        var day2 = Daily(10000m + day1, 5.2m);
        var fresh = await Fresh().MemberAccounts.AsNoTracking().SingleAsync(x => x.Id == a.Id);
        Assert.Equal(day1 + day2, fresh.InterestAccruedCR);
        Assert.True(day2 > day1);
    }

    [Fact]
    public async Task Accounts_with_no_rate_no_frequency_a_zero_balance_or_an_unsupported_method_accrue_nothing()
    {
        var none = new ProductDefinition(_org.Id, _type.Id, "NoRate", "NR");           // rate 0
        var noFreq = new ProductDefinition(_org.Id, _type.Id, "NoFreq", "NF");
        noFreq.Update(_type.Id, "NoFreq", "NF", 0m, 5m, "SIMPLE", 0, 0, 0, null, true);
        var minBal = new ProductDefinition(_org.Id, _type.Id, "MinBal", "MB");
        minBal.Update(_type.Id, "MinBal", "MB", 0m, 5m, "MINIMUM_BALANCE", 1, 0, 0, null, true);
        _db.ProductDefinitions.AddRange(none, noFreq, minBal);
        await _db.SaveChangesAsync();
        Account("A1", 1000m, product: none); Account("A2", 1000m, product: noFreq); Account("A3", 1000m, product: minBal); Account("A4", 0m);

        await StartFirst();
        var eod = await Svc().RunEodAsync(_org.Id, _user);
        Assert.True(eod.Success, eod.Message);
        Assert.Contains(eod.Notes, n => n.Contains("MINIMUM_BALANCE"));
        Assert.All(Fresh().MemberAccounts, a => Assert.Equal(0m, a.InterestAccruedCR));
    }

    [Fact]
    public async Task An_account_with_no_activity_for_longer_than_the_dormancy_limit_becomes_dormant()
    {
        var p = new ProductDefinition(_org.Id, _type.Id, "Dormant30", "D30");
        p.Update(_type.Id, "Dormant30", "D30", 0m, 0m, null, 0, 30, 0, null, true);
        _db.ProductDefinitions.Add(p);
        await _db.SaveChangesAsync();
        var idle = Account("A1", 100m, product: p, openDate: new DateOnly(2026, 1, 1));            // 14 days idle at start
        var busy = Account("A2", 100m, product: p, openDate: new DateOnly(2026, 1, 1));
        var never = Account("A3", 100m, product: _product);                                        // limit 0 = never

        await StartFirst(new DateOnly(2026, 2, 1));                                                // 31 days after opening
        var r = await Svc().RunEodAsync(_org.Id, _user);
        Assert.True(r.Success, r.Message);

        var f = Fresh().MemberAccounts.AsNoTracking();
        Assert.Equal("DORMANT", (await f.SingleAsync(a => a.Id == idle.Id)).Status);
        Assert.Equal("DORMANT", (await f.SingleAsync(a => a.Id == busy.Id)).Status);
        Assert.Equal("ACTIVE", (await f.SingleAsync(a => a.Id == never.Id)).Status);
    }

    [Fact]
    public async Task A_customer_deposit_counts_as_activity_but_bank_credited_interest_does_not()
    {
        var p = new ProductDefinition(_org.Id, _type.Id, "Dormant30", "D30");
        p.Update(_type.Id, "Dormant30", "D30", 0m, 0m, null, 0, 30, 0, null, true);
        _db.ProductDefinitions.Add(p);
        await _db.SaveChangesAsync();
        var a = Account("A1", 100m, product: p, openDate: new DateOnly(2026, 1, 1));

        await StartFirst(new DateOnly(2026, 1, 20));
        Assert.True((await Post().PostAsync(new PostingRequest
        {
            OrganizationId = _org.Id, PostedBy = _user, ValueDate = new DateOnly(2026, 1, 20), Narration = "x", CreditCodeId = _dep.Id,
            ContraGlAccountNo = "1000", Legs = new[] { new PostingLeg("A1", "CR", 5m) }
        })).Success);

        // 25 days later (day 45 from opening, but only 25 from the deposit): still active.
        for (var i = 0; i < 25; i++) { await Svc().RunEodAsync(_org.Id, _user); await Svc().StartDayAsync(_org.Id, _user, null); }
        Assert.Equal("ACTIVE", (await Fresh().MemberAccounts.AsNoTracking().SingleAsync(x => x.Id == a.Id)).Status);
    }

    // ═════════════ Ledger integrity ═════════════

    [Fact]
    public async Task Every_posting_writes_a_balanced_journal_to_the_ledger()
    {
        Account("A1", 100m);
        await StartFirst();
        await Post().PostAsync(new PostingRequest
        {
            OrganizationId = _org.Id, PostedBy = _user, ValueDate = Start, Narration = "Cash in", CreditCodeId = _dep.Id,
            ContraGlAccountNo = "1000", Legs = new[] { new PostingLeg("A1", "CR", 250m) }
        });

        var lines = await Fresh().GLEntries.OrderBy(g => g.GLAccountNo).ToListAsync();
        Assert.Equal(2, lines.Count);
        var cash = lines.Single(l => l.GLAccountNo == "1000");
        var deposits = lines.Single(l => l.GLAccountNo == "2000");
        Assert.Equal("DR", cash.DrCr);                    // the bank holds more cash
        Assert.Equal("CR", deposits.DrCr);                // and owes the customer more
        Assert.All(lines, l => { Assert.Equal(250m, l.Amount); Assert.Equal("TELLER", l.SourceModule); Assert.Equal("2026-01", l.FiscalPeriod); Assert.Equal("POSTED", l.Status); });
        Assert.Single(lines.Select(l => l.JournalRef).Distinct());
    }

    [Fact]
    public async Task A_reversal_mirrors_the_journal_and_marks_the_original_lines_reversed()
    {
        Account("A1", 100m);
        await StartFirst();
        var posted = await Post().PostAsync(new PostingRequest
        {
            OrganizationId = _org.Id, PostedBy = _user, ValueDate = Start, Narration = "Cash in", CreditCodeId = _dep.Id,
            ContraGlAccountNo = "1000", Legs = new[] { new PostingLeg("A1", "CR", 250m) }
        });
        var rev = await Post().ReverseAsync(_org.Id, posted.Transactions[0].TransactionRef, _user, "Wrong slip");
        Assert.True(rev.Success, rev.Error);

        var lines = await Fresh().GLEntries.ToListAsync();
        Assert.Equal(4, lines.Count);
        Assert.Equal(2, lines.Count(l => l.Status == "REVERSED"));
        // Net effect on every account is zero: balances sum all lines whatever their status.
        foreach (var g in lines.GroupBy(l => l.GLAccountNo))
            Assert.Equal(0m, g.Sum(l => l.DrCr == "DR" ? l.Amount : -l.Amount));
        Assert.All(lines.Where(l => l.Status == "POSTED"), l => Assert.StartsWith("Reversal of", l.Narration));
    }

    [Fact]
    public async Task An_account_without_a_ledger_account_cannot_be_posted_to()
    {
        var a = Account("A1", 100m);
        var cust = new Customer(_org.Id, _branch.Id, "C-X", "INDIVIDUAL", "A", "B");
        cust.Update(new CustomerProfile { BranchId = _branch.Id, FirstName = "A", LastName = "B" });
        _db.Customers.Add(cust);
        var bare = new MemberAccount(_org.Id, _branch.Id, cust.Id, _product.Id, "BARE", "Bare", "SAVINGS");
        bare.UpdateTerms(_product.Id, "GMD", 50m, new DateOnly(2026, 1, 1));
        bare.Approve(Guid.NewGuid());
        _db.MemberAccounts.Add(bare);
        await _db.SaveChangesAsync();

        var r = await Post().PostAsync(new PostingRequest
        {
            OrganizationId = _org.Id, PostedBy = _user, ValueDate = DateOnly.FromDateTime(DateTime.UtcNow), Narration = "x", CreditCodeId = _dep.Id,
            ContraGlAccountNo = "1000", Legs = new[] { new PostingLeg("BARE", "CR", 5m) }
        });
        Assert.False(r.Success);
        Assert.Contains("liability control", r.Error);
        Assert.Empty(_db.Transactions);
    }

    [Fact]
    public async Task End_of_day_refuses_to_close_when_the_ledger_is_out_of_balance()
    {
        await StartFirst();
        _db.GLEntries.Add(new GLEntry(_org.Id, "JNL-X", "1000", "DR", 50m, "Broken", Start, DateTime.UtcNow, _user, "SYSTEM", "X"));
        await _db.SaveChangesAsync();

        var r = await Svc().RunEodAsync(_org.Id, _user);
        Assert.False(r.Success);
        Assert.Contains("out of balance", r.Message);
        var s = await State();
        Assert.Equal("OPEN", s.EODStatus);
        Assert.Equal(Start, s.CurrentDate);
        var log = await Fresh().EODLogs.SingleAsync(l => l.ProcessType == "EOD");
        Assert.Equal("FAILED", log.Status);
    }

    [Fact]
    public async Task Two_users_cannot_run_end_of_day_at_once()
    {
        await StartFirst();
        using var other = Fresh();
        var stale = await other.ProcessingDates.SingleAsync();           // the second user's view of the day

        Assert.True((await Svc().RunEodAsync(_org.Id, _user)).Success);   // the first user closes it

        stale.BeginEod();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => other.SaveChangesAsync());
    }

    // ═════════════ Month end ═════════════

    [Fact]
    public async Task Month_end_needs_a_finished_month_and_a_closed_day()
    {
        Assert.False((await Svc().RunEomAsync(_org.Id, _user)).Success);          // not started
        await StartFirst();
        Assert.False((await Svc().RunEomAsync(_org.Id, _user)).Success);          // day open
        await Days(0);
        await Svc().RunEodAsync(_org.Id, _user);
        var early = await Svc().RunEomAsync(_org.Id, _user);                       // 16 Jan: January not over
        Assert.False(early.Success);
        Assert.Contains("not over", early.Message);
    }

    private async Task ReachFirstOfFebruary()
    {
        await StartFirst();
        await Days(16);                              // 15 Jan open … 31 Jan closed → business date 1 Feb, closed
        await Svc().RunEodAsync(_org.Id, _user);
    }

    [Fact]
    public async Task Month_end_pays_whole_cents_of_interest_resets_counters_and_closes_the_period()
    {
        var a = Account("A1", 10000m);
        await StartFirst();
        for (var i = 0; i < 17; i++)                                                // 17 days of accrual: 15 Jan … 31 Jan
        {
            var eod = await Svc().RunEodAsync(_org.Id, _user);
            Assert.True(eod.Success, eod.Message);
            if (i < 16) await Svc().StartDayAsync(_org.Id, _user, null);
        }
        Assert.Equal(new DateOnly(2026, 2, 1), (await State()).CurrentDate);

        var accrued = Daily(10000m, 5.2m) * 17;
        var payable = Math.Floor(accrued * 100m) / 100m;

        var r = await Svc().RunEomAsync(_org.Id, _user);
        Assert.True(r.Success, r.Message);

        var fresh = await Fresh().MemberAccounts.AsNoTracking().SingleAsync(x => x.Id == a.Id);
        Assert.Equal(10000m + payable, fresh.BookBalance);
        Assert.Equal(accrued - payable, fresh.InterestAccruedCR);                   // the fraction of a cent carries forward
        Assert.Equal(0m, fresh.MonthlyCRTurnover);
        Assert.Equal("2026-01", (await State()).ClosedThroughPeriod);

        var interest = await Fresh().Transactions.SingleAsync();
        Assert.Equal(payable, interest.Amount);
        Assert.Equal("5000", interest.GLAccountNo);
        Assert.Equal(new DateOnly(2026, 1, 31), interest.ValueDate);
        var gl = await Fresh().GLEntries.ToListAsync();
        Assert.All(gl, g => Assert.Equal("SYSTEM", g.SourceModule));
        Assert.Equal("DR", gl.Single(g => g.GLAccountNo == "5000").DrCr);          // interest is an expense to the bank
        Assert.Equal("CR", gl.Single(g => g.GLAccountNo == "2000").DrCr);
        Assert.Equal("EOM", (await Fresh().EODLogs.OrderByDescending(l => l.StartedAt).FirstAsync()).ProcessType);
    }

    [Fact]
    public async Task Interest_is_not_paid_twice_and_a_closed_period_cannot_be_posted_into()
    {
        var a = Account("A1", 10000m);
        await StartFirst();
        for (var i = 0; i < 17; i++) { await Svc().RunEodAsync(_org.Id, _user); if (i < 16) await Svc().StartDayAsync(_org.Id, _user, null); }
        Assert.True((await Svc().RunEomAsync(_org.Id, _user)).Success);

        var again = await Svc().RunEomAsync(_org.Id, _user);                        // 1 Feb: February is not over
        Assert.False(again.Success);
        Assert.Single(Fresh().Transactions);

        await Svc().StartDayAsync(_org.Id, _user, null);
        var late = await Post().PostAsync(new PostingRequest
        {
            OrganizationId = _org.Id, PostedBy = _user, ValueDate = new DateOnly(2026, 1, 31), Narration = "x", CreditCodeId = _dep.Id,
            ContraGlAccountNo = "1000", Legs = new[] { new PostingLeg("A1", "CR", 5m) }
        });
        Assert.False(late.Success);
        Assert.Contains("closed", late.Error);
        Assert.True((await Post().PostAsync(new PostingRequest
        {
            OrganizationId = _org.Id, PostedBy = _user, ValueDate = new DateOnly(2026, 2, 1), Narration = "x", CreditCodeId = _dep.Id,
            ContraGlAccountNo = "1000", Legs = new[] { new PostingLeg("A1", "CR", 5m) }
        })).Success);
    }

    [Fact]
    public async Task Interest_is_paid_only_in_months_that_match_the_products_frequency()
    {
        var bimonthly = new ProductDefinition(_org.Id, _type.Id, "Bi", "BI");
        bimonthly.Update(_type.Id, "Bi", "BI", 0m, 5.2m, "SIMPLE", 2, 0, 0, null, true);          // every 2 months: February, April…
        _db.ProductDefinitions.Add(bimonthly);
        await _db.SaveChangesAsync();
        var a = Account("A1", 10000m, product: bimonthly);

        await StartFirst();
        for (var i = 0; i < 17; i++) { await Svc().RunEodAsync(_org.Id, _user); if (i < 16) await Svc().StartDayAsync(_org.Id, _user, null); }
        var jan = await Svc().RunEomAsync(_org.Id, _user);
        Assert.True(jan.Success, jan.Message);
        Assert.Empty(Fresh().Transactions);                                           // January: nothing paid
        Assert.True((await Fresh().MemberAccounts.AsNoTracking().SingleAsync(x => x.Id == a.Id)).InterestAccruedCR > 0);

        await Svc().StartDayAsync(_org.Id, _user, null);
        while ((await State()).CurrentDate < new DateOnly(2026, 3, 1))
        {
            await Svc().RunEodAsync(_org.Id, _user);
            if ((await State()).CurrentDate < new DateOnly(2026, 3, 1)) await Svc().StartDayAsync(_org.Id, _user, null);
        }
        var feb = await Svc().RunEomAsync(_org.Id, _user);
        Assert.True(feb.Success, feb.Message);
        Assert.Single(Fresh().Transactions);                                          // February pays both months' interest
    }

    [Fact]
    public async Task Interest_without_an_expense_account_stays_accrued_and_is_reported()
    {
        var a = Account("A1", 10000m, expenseGl: null);
        await StartFirst();
        for (var i = 0; i < 17; i++) { await Svc().RunEodAsync(_org.Id, _user); if (i < 16) await Svc().StartDayAsync(_org.Id, _user, null); }
        var r = await Svc().RunEomAsync(_org.Id, _user);
        Assert.True(r.Success, r.Message);
        Assert.Contains(r.Notes, n => n.Contains("no expense GL"));
        Assert.Empty(Fresh().Transactions);
        Assert.True((await Fresh().MemberAccounts.AsNoTracking().SingleAsync(x => x.Id == a.Id)).InterestAccruedCR > 0);
    }

    [Fact]
    public async Task Month_end_stops_and_can_be_repeated_when_the_interest_code_is_unusable()
    {
        Account("A1", 10000m);
        var broken = new TransactionCode(_org.Id, "INT", "Wrong way round", "DR");
        _db.TransactionCodes.Add(broken);
        await _db.SaveChangesAsync();

        await StartFirst();
        for (var i = 0; i < 17; i++) { await Svc().RunEodAsync(_org.Id, _user); if (i < 16) await Svc().StartDayAsync(_org.Id, _user, null); }

        var failed = await Svc().RunEomAsync(_org.Id, _user);
        Assert.False(failed.Success);
        Assert.Null((await State()).ClosedThroughPeriod);
        Assert.Equal("FAILED", (await Fresh().EODLogs.SingleAsync(l => l.ProcessType == "EOM")).Status);

        var code = await _db.TransactionCodes.SingleAsync(c => c.Code == "INT");
        code.SoftDelete();
        await _db.SaveChangesAsync();
        var ok = await Svc().RunEomAsync(_org.Id, _user);                             // a fresh INT code is created
        Assert.True(ok.Success, ok.Message);
        Assert.Single(Fresh().Transactions);
    }

    [Fact]
    public async Task Bank_credited_interest_does_not_wake_a_dormant_account()
    {
        var p = new ProductDefinition(_org.Id, _type.Id, "Dormant10", "D10");
        p.Update(_type.Id, "Dormant10", "D10", 0m, 5.2m, "SIMPLE", 1, 10, 0, null, true);
        _db.ProductDefinitions.Add(p);
        await _db.SaveChangesAsync();
        var a = Account("A1", 10000m, product: p, openDate: new DateOnly(2026, 1, 1));

        await StartFirst();
        for (var i = 0; i < 17; i++) { await Svc().RunEodAsync(_org.Id, _user); if (i < 16) await Svc().StartDayAsync(_org.Id, _user, null); }
        Assert.Equal("DORMANT", (await Fresh().MemberAccounts.AsNoTracking().SingleAsync(x => x.Id == a.Id)).Status);

        Assert.True((await Svc().RunEomAsync(_org.Id, _user)).Success);
        var fresh = await Fresh().MemberAccounts.AsNoTracking().SingleAsync(x => x.Id == a.Id);
        Assert.Equal("DORMANT", fresh.Status);                                         // interest paid, still dormant
        Assert.True(fresh.BookBalance > 10000m);
    }

    [Fact]
    public async Task System_interest_postings_cannot_be_reversed_by_hand()
    {
        Account("A1", 10000m);
        await StartFirst();
        for (var i = 0; i < 17; i++) { await Svc().RunEodAsync(_org.Id, _user); if (i < 16) await Svc().StartDayAsync(_org.Id, _user, null); }
        await Svc().RunEomAsync(_org.Id, _user);
        await Svc().StartDayAsync(_org.Id, _user, null);

        var interest = await Fresh().Transactions.SingleAsync();
        var r = await Post().ReverseAsync(_org.Id, interest.TransactionRef, _user, "Undo");
        Assert.False(r.Success);
        Assert.Contains("System postings", r.Error);
    }

    // ═════════════ Year end ═════════════

    private async Task CloseThrough(string period, DateOnly date)
    {
        // Gets the calendar to "all of the year closed" without running 365 days.
        await StartFirst(date);
        var s = await _db.ProcessingDates.SingleAsync();
        typeof(ProcessingDate).GetProperty(nameof(ProcessingDate.ClosedThroughPeriod))!.SetValue(s, period);
        typeof(ProcessingDate).GetProperty(nameof(ProcessingDate.EODStatus))!.SetValue(s, "CLOSED");
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();
    }

    private void Journal(string reference, DateOnly date, params (string Gl, string DrCr, decimal Amount)[] lines)
    {
        foreach (var l in lines)
            _db.GLEntries.Add(new GLEntry(_org.Id, reference, l.Gl, l.DrCr, l.Amount, "Test", date, DateTime.UtcNow, _user, "SYSTEM", reference));
        _db.SaveChanges();
    }

    [Fact]
    public async Task Year_end_closes_income_and_expenses_into_retained_earnings()
    {
        Journal("J1", new DateOnly(2026, 3, 1), ("1000", "DR", 1000m), ("4000", "CR", 1000m));       // fee income 1,000
        Journal("J2", new DateOnly(2026, 6, 1), ("5000", "DR", 400m), ("1000", "CR", 400m));         // interest expense 400
        await CloseThrough("2026-12", new DateOnly(2027, 1, 1));

        var preview = await Svc().PreviewEoyAsync(_org.Id);
        Assert.Null(preview.Problem);
        Assert.Equal(2026, preview.Year);
        Assert.Equal(600m, preview.NetProfit);

        var r = await Svc().RunEoyAsync(_org.Id, _user, "3000");
        Assert.True(r.Success, r.Message);

        var closing = await Fresh().GLEntries.Where(g => g.JournalRef == "EOY-2026").ToListAsync();
        Assert.Equal(3, closing.Count);
        Assert.Equal(("DR", 1000m), (closing.Single(c => c.GLAccountNo == "4000").DrCr, closing.Single(c => c.GLAccountNo == "4000").Amount));
        Assert.Equal(("CR", 400m), (closing.Single(c => c.GLAccountNo == "5000").DrCr, closing.Single(c => c.GLAccountNo == "5000").Amount));
        Assert.Equal(("CR", 600m), (closing.Single(c => c.GLAccountNo == "3000").DrCr, closing.Single(c => c.GLAccountNo == "3000").Amount));
        Assert.Equal(closing.Where(c => c.DrCr == "DR").Sum(c => c.Amount), closing.Where(c => c.DrCr == "CR").Sum(c => c.Amount));
        Assert.All(closing, c => { Assert.Equal(new DateOnly(2026, 12, 31), c.ValueDate); Assert.Equal("2026-12", c.FiscalPeriod); });
        Assert.NotNull((await State()).LastEOYAt);

        var again = await Svc().RunEoyAsync(_org.Id, _user, "3000");                  // already closed
        Assert.False(again.Success);
        Assert.Equal(3, await Fresh().GLEntries.CountAsync(g => g.JournalRef == "EOY-2026"));
    }

    [Fact]
    public async Task Year_end_moves_a_loss_to_retained_earnings_as_a_debit()
    {
        Journal("J1", new DateOnly(2026, 3, 1), ("5000", "DR", 300m), ("1000", "CR", 300m));
        Journal("J2", new DateOnly(2026, 4, 1), ("1000", "DR", 100m), ("4000", "CR", 100m));
        await CloseThrough("2026-12", new DateOnly(2027, 1, 1));

        Assert.True((await Svc().RunEoyAsync(_org.Id, _user, "3000")).Success);
        var re = await Fresh().GLEntries.SingleAsync(g => g.JournalRef == "EOY-2026" && g.GLAccountNo == "3000");
        Assert.Equal(("DR", 200m), (re.DrCr, re.Amount));
    }

    [Fact]
    public async Task Year_end_needs_a_closed_december_an_equity_account_and_runs_once()
    {
        Journal("J1", new DateOnly(2026, 3, 1), ("1000", "DR", 100m), ("4000", "CR", 100m));
        await StartFirst(new DateOnly(2026, 6, 1));
        Assert.False((await Svc().RunEoyAsync(_org.Id, _user, "3000")).Success);        // no month closed yet
        Assert.NotNull((await Svc().PreviewEoyAsync(_org.Id)).Problem);

        await CloseThrough("2026-11", new DateOnly(2026, 12, 15));
        Assert.False((await Svc().RunEoyAsync(_org.Id, _user, "3000")).Success);        // December not closed

        var s = await _db.ProcessingDates.SingleAsync();
        typeof(ProcessingDate).GetProperty(nameof(ProcessingDate.ClosedThroughPeriod))!.SetValue(s, "2026-12");
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();

        Assert.False((await Svc().RunEoyAsync(_org.Id, _user, null)).Success);          // no retained-earnings account
        Assert.False((await Svc().RunEoyAsync(_org.Id, _user, "1000")).Success);        // an asset account
        Assert.False((await Svc().RunEoyAsync(_org.Id, _user, "9999")).Success);        // unknown
        Assert.True((await Svc().RunEoyAsync(_org.Id, _user, "3000")).Success);
    }

    [Fact]
    public async Task A_year_with_nothing_to_close_still_closes()
    {
        await CloseThrough("2026-12", new DateOnly(2027, 1, 1));
        var r = await Svc().RunEoyAsync(_org.Id, _user, "3000");
        Assert.True(r.Success, r.Message);
        Assert.Empty(Fresh().GLEntries);
        Assert.Contains(r.Notes, n => n.Contains("no income or expense"));
    }
}
