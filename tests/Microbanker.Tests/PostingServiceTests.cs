using Microbanker.Domain.Entities.Accounts;
using Microbanker.Domain.Entities.Customers;
using Microbanker.Domain.Entities.Organization;
using Microbanker.Domain.Entities.Setup;
using Microbanker.Domain.Entities.Transactions;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Microbanker.Tests;

/// <summary>The posting engine against in-memory SQLite: balances, rules, atomicity, reversals.</summary>
public sealed class PostingServiceTests : IDisposable
{
    private readonly SqliteConnection _conn = new("DataSource=:memory:");
    private readonly DbContextOptions<MicrobankerDbContext> _options;
    private readonly MicrobankerDbContext _db;
    private readonly Organization _org = new("Test Org", "TST");
    private readonly Branch _branch;
    private readonly ProductDefinition _product;
    private readonly TransactionCode _drCode;
    private readonly TransactionCode _crCode;
    private readonly TransactionCode _approvalCode;
    private readonly GLAccount _cash;
    private readonly Guid _user = Guid.NewGuid();
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    public PostingServiceTests()
    {
        _conn.Open();
        _options = new DbContextOptionsBuilder<MicrobankerDbContext>().UseSqlite(_conn).Options;
        _db = new MicrobankerDbContext(_options);
        _db.Database.EnsureCreated();

        _db.Organizations.Add(_org);
        _branch = new Branch(_org.Id, "Main", "MN");
        _db.Branches.Add(_branch);
        var type = new ProductType(_org.Id, "Savings", "SAV", null);
        _db.ProductTypes.Add(type);
        _product = new ProductDefinition(_org.Id, type.Id, "Basic", "BSC");
        _db.ProductDefinitions.Add(_product);

        _drCode = new TransactionCode(_org.Id, "WDL", "Cash withdrawal", "DR");
        _crCode = new TransactionCode(_org.Id, "DEP", "Cash deposit", "CR");
        _approvalCode = new TransactionCode(_org.Id, "BIGW", "Large withdrawal", "DR");
        _approvalCode.Update("BIGW", "Large withdrawal", "DR", null, requiresApproval: true, isActive: true);
        _db.TransactionCodes.AddRange(_drCode, _crCode, _approvalCode);

        _cash = new GLAccount(_org.Id, "1000", "Cash", "ASSET", null, 1);
        _db.GLAccounts.Add(_cash);
        _db.SaveChanges();
    }

    public void Dispose() { _db.Dispose(); _conn.Dispose(); }

    private MemberAccount Account(string no, decimal balance = 1000m, bool active = true, string currency = "GMD")
    {
        var cust = new Customer(_org.Id, _branch.Id, "C-" + no, "INDIVIDUAL", "A", "B");
        cust.Update(new CustomerProfile { BranchId = _branch.Id, FirstName = "A", LastName = "B" });
        _db.Customers.Add(cust);
        var a = new MemberAccount(_org.Id, _branch.Id, cust.Id, _product.Id, no, "Acc " + no, "SAVINGS");
        a.UpdateTerms(_product.Id, currency, balance, new DateOnly(2026, 1, 1));
        if (active) a.Approve(Guid.NewGuid());
        _db.MemberAccounts.Add(a);
        _db.SaveChanges();
        return a;
    }

    private PostingService Service(MicrobankerDbContext? db = null) => new(db ?? _db);

    private PostingRequest Req(string drCr, string no, decimal amount, string? contra = "1000", Guid? wr = null) => new()
    {
        OrganizationId = _org.Id, PostedBy = _user, ValueDate = Today, Narration = "Test",
        DebitCodeId = _drCode.Id, CreditCodeId = _crCode.Id, ContraGlAccountNo = contra,
        Legs = new[] { new PostingLeg(no, drCr, amount) }, WithdrawalRequestId = wr
    };

    private async Task<MemberAccount> Fresh(MemberAccount a) => await new MicrobankerDbContext(_options).MemberAccounts.AsNoTracking().SingleAsync(x => x.Id == a.Id);

    // ── Deposits and withdrawals ──────────────────────────────────────────────

    [Fact]
    public async Task A_deposit_credits_the_account_and_records_the_transaction()
    {
        var a = Account("A1", 100m);
        var r = await Service().PostAsync(Req("CR", "A1", 250m));

        Assert.True(r.Success, r.Error);
        var t = Assert.Single(r.Transactions);
        Assert.Equal($"TXN-{DateTime.UtcNow:yyyyMMdd}-000001", t.TransactionRef);
        Assert.Equal("CR", t.DrCr);
        Assert.Equal(250m, t.Amount);
        Assert.Equal(350m, t.BalanceAfter);
        Assert.Equal("1000", t.GLAccountNo);
        Assert.Equal("POSTED", t.Status);
        Assert.Null(t.BatchRef);

        var after = await Fresh(a);
        Assert.Equal(350m, after.BookBalance);
        Assert.Equal(350m, after.AvailableBalance);
        Assert.Equal(250m, after.MonthlyCRTurnover);
        Assert.NotNull(after.LastCRDate);
    }

    [Fact]
    public async Task A_withdrawal_debits_the_account_and_cannot_overdraw()
    {
        var a = Account("A1", 100m);
        Assert.True((await Service().PostAsync(Req("DR", "A1", 40m))).Success);
        Assert.Equal(60m, (await Fresh(a)).BookBalance);

        var over = await Service().PostAsync(Req("DR", "A1", 60.01m));
        Assert.False(over.Success);
        Assert.Contains("available", over.Error);
        Assert.Equal(60m, (await Fresh(a)).BookBalance);
        Assert.Single(_db.Transactions);                      // the refused one left nothing behind
    }

    [Fact]
    public async Task Blocked_money_is_not_available_for_withdrawal()
    {
        var a = Account("A1", 100m);
        a.BlockAmount(70m);
        await _db.SaveChangesAsync();

        var r = await Service().PostAsync(Req("DR", "A1", 40m));
        Assert.False(r.Success);
        Assert.True((await Service().PostAsync(Req("DR", "A1", 30m))).Success);
    }

    [Fact]
    public async Task The_product_minimum_balance_is_kept()
    {
        _product.Update(_product.ProductTypeId, _product.Name, _product.Code, 50m, 0m, null, 0, 0, 0, null, true);
        await _db.SaveChangesAsync();
        Account("A1", 100m);

        var r = await Service().PostAsync(Req("DR", "A1", 60m));
        Assert.False(r.Success);
        Assert.Contains("minimum", r.Error);
        Assert.True((await Service().PostAsync(Req("DR", "A1", 50m))).Success);
    }

    [Fact]
    public async Task The_monthly_debit_limit_is_enforced_and_reversed_debits_do_not_count()
    {
        _product.Update(_product.ProductTypeId, _product.Name, _product.Code, 0m, 0m, null, 0, 0, maxDebitPerMonth: 2, null, true);
        await _db.SaveChangesAsync();
        Account("A1", 1000m);

        var first = await Service().PostAsync(Req("DR", "A1", 10m));
        Assert.True(first.Success);
        Assert.True((await Service().PostAsync(Req("DR", "A1", 10m))).Success);
        var third = await Service().PostAsync(Req("DR", "A1", 10m));
        Assert.False(third.Success);
        Assert.Contains("limit", third.Error);

        Assert.True((await Service().ReverseAsync(_org.Id, first.Transactions[0].TransactionRef, _user, "Mistake")).Success);
        Assert.True((await Service().PostAsync(Req("DR", "A1", 10m))).Success);       // the reversed debit freed a slot
    }

    [Theory]
    [InlineData("pending")]
    [InlineData("frozen")]
    [InlineData("closed")]
    public async Task Accounts_that_are_not_open_cannot_be_posted_to(string state)
    {
        var a = Account("A1", state == "closed" ? 0m : 100m, active: state != "pending");
        if (state == "frozen") a.Freeze();
        if (state == "closed") a.Close();
        await _db.SaveChangesAsync();

        Assert.False((await Service().PostAsync(Req("CR", "A1", 10m))).Success);
        Assert.False((await Service().PostAsync(Req("DR", "A1", 10m))).Success);
    }

    [Fact]
    public async Task A_credit_ends_dormancy_but_a_dormant_account_cannot_be_debited()
    {
        var a = Account("A1", 100m);
        typeof(MemberAccount).GetProperty(nameof(MemberAccount.Status))!.SetValue(a, AccountOptions.Dormant);
        await _db.SaveChangesAsync();

        Assert.False((await Service().PostAsync(Req("DR", "A1", 10m))).Success);
        Assert.True((await Service().PostAsync(Req("CR", "A1", 10m))).Success);
        Assert.Equal(AccountOptions.Active, (await Fresh(a)).Status);
    }

    // ── Validation ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Bad_input_is_refused_before_anything_is_touched()
    {
        Account("A1", 100m);
        var s = Service();

        Assert.False((await s.PostAsync(Req("CR", "A1", 0m))).Success);
        Assert.False((await s.PostAsync(Req("CR", "A1", -5m))).Success);
        Assert.False((await s.PostAsync(Req("CR", "A1", 1.005m))).Success);
        Assert.False((await s.PostAsync(Req("XX", "A1", 5m))).Success);
        Assert.False((await s.PostAsync(Req("CR", "NOPE", 5m))).Success);
        Assert.False((await s.PostAsync(Req("CR", "A1", 5m, contra: null))).Success);        // one-sided needs a contra GL
        Assert.False((await s.PostAsync(Req("CR", "A1", 5m, contra: "9999"))).Success);      // unknown GL
        Assert.False((await s.PostAsync(new PostingRequest { OrganizationId = _org.Id, PostedBy = _user, ValueDate = Today, Narration = "x" })).Success);
        var future = Req("CR", "A1", 5m);
        Assert.False((await s.PostAsync(new PostingRequest { OrganizationId = _org.Id, PostedBy = _user, ValueDate = Today.AddDays(1), Narration = "x",
            CreditCodeId = _crCode.Id, ContraGlAccountNo = "1000", Legs = future.Legs })).Success);
        Assert.False((await s.PostAsync(new PostingRequest { OrganizationId = _org.Id, PostedBy = _user, ValueDate = Today, Narration = " ",
            CreditCodeId = _crCode.Id, ContraGlAccountNo = "1000", Legs = future.Legs })).Success);
        Assert.Empty(_db.Transactions);
    }

    [Fact]
    public async Task Transaction_codes_must_match_the_direction_and_not_need_approval()
    {
        Account("A1", 100m);
        var wrong = new PostingRequest { OrganizationId = _org.Id, PostedBy = _user, ValueDate = Today, Narration = "x",
            CreditCodeId = _drCode.Id, ContraGlAccountNo = "1000", Legs = new[] { new PostingLeg("A1", "CR", 5m) } };
        Assert.False((await Service().PostAsync(wrong)).Success);

        var needsApproval = new PostingRequest { OrganizationId = _org.Id, PostedBy = _user, ValueDate = Today, Narration = "x",
            DebitCodeId = _approvalCode.Id, ContraGlAccountNo = "1000", Legs = new[] { new PostingLeg("A1", "DR", 5m) } };
        var r = await Service().PostAsync(needsApproval);
        Assert.False(r.Success);
        Assert.Contains("requires approval", r.Error);
    }

    [Fact]
    public async Task A_control_account_cannot_be_used_as_the_contra()
    {
        _db.GLAccounts.Add(new GLAccount(_org.Id, "2000", "Customer deposits", "LIABILITY", null, 1));
        await _db.SaveChangesAsync();
        var control = await _db.GLAccounts.SingleAsync(g => g.AccountNumber == "2000");
        control.Update("2000", "Customer deposits", "LIABILITY", null, 1, isControlAccount: true, isActive: true, null);
        await _db.SaveChangesAsync();
        Account("A1");
        Assert.False((await Service().PostAsync(Req("CR", "A1", 5m, contra: "2000"))).Success);
    }

    [Fact]
    public async Task Accounts_in_different_currencies_cannot_be_posted_together()
    {
        Account("A1", 100m); Account("U1", 100m, currency: "USD");
        var req = new PostingRequest { OrganizationId = _org.Id, PostedBy = _user, ValueDate = Today, Narration = "x",
            DebitCodeId = _drCode.Id, CreditCodeId = _crCode.Id,
            Legs = new[] { new PostingLeg("A1", "DR", 10m), new PostingLeg("U1", "CR", 10m) } };
        var r = await Service().PostAsync(req);
        Assert.False(r.Success);
        Assert.Contains("currency", r.Error);
    }

    // ── Transfers and mixed postings ──────────────────────────────────────────

    [Fact]
    public async Task A_transfer_moves_money_between_accounts_as_one_batch()
    {
        var a = Account("A1", 500m); var b = Account("B1", 100m);
        var r = await Service().PostAsync(new PostingRequest
        {
            OrganizationId = _org.Id, PostedBy = _user, ValueDate = Today, Narration = "Transfer",
            DebitCodeId = _drCode.Id, CreditCodeId = _crCode.Id,
            Legs = new[] { new PostingLeg("A1", "DR", 200m), new PostingLeg("B1", "CR", 200m) }
        });

        Assert.True(r.Success, r.Error);
        Assert.Equal(2, r.Transactions.Count);
        Assert.NotNull(r.BatchRef);
        Assert.All(r.Transactions, t => { Assert.Equal(r.BatchRef, t.BatchRef); Assert.Null(t.GLAccountNo); });
        Assert.Equal(300m, (await Fresh(a)).BookBalance);
        Assert.Equal(300m, (await Fresh(b)).BookBalance);
    }

    [Fact]
    public async Task Unbalanced_or_over_specified_postings_are_refused()
    {
        Account("A1", 500m); Account("B1", 100m);
        PostingRequest Mixed(decimal dr, decimal cr, string? contra = null) => new()
        {
            OrganizationId = _org.Id, PostedBy = _user, ValueDate = Today, Narration = "x",
            DebitCodeId = _drCode.Id, CreditCodeId = _crCode.Id, ContraGlAccountNo = contra,
            Legs = new[] { new PostingLeg("A1", "DR", dr), new PostingLeg("B1", "CR", cr) }
        };
        var unbalanced = await Service().PostAsync(Mixed(100m, 90m));
        Assert.False(unbalanced.Success);
        Assert.Contains("balance", unbalanced.Error);
        Assert.False((await Service().PostAsync(Mixed(100m, 100m, contra: "1000"))).Success);   // balanced AND a contra
    }

    [Fact]
    public async Task A_posting_is_all_or_nothing()
    {
        var a = Account("A1", 50m); var b = Account("B1", 100m); var c = Account("C1", 100m);
        var r = await Service().PostAsync(new PostingRequest
        {
            OrganizationId = _org.Id, PostedBy = _user, ValueDate = Today, Narration = "x",
            DebitCodeId = _drCode.Id, CreditCodeId = _crCode.Id,
            // B credited fine, but A cannot fund the 80 debit: nothing may post.
            Legs = new[] { new PostingLeg("A1", "DR", 80m), new PostingLeg("B1", "CR", 50m), new PostingLeg("C1", "CR", 30m) }
        });
        Assert.False(r.Success);
        Assert.Equal(50m, (await Fresh(a)).BookBalance);
        Assert.Equal(100m, (await Fresh(b)).BookBalance);
        Assert.Equal(100m, (await Fresh(c)).BookBalance);
        Assert.Empty(_db.Transactions);
    }

    [Fact]
    public async Task The_same_account_cannot_appear_twice()
    {
        Account("A1", 100m);
        var r = await Service().PostAsync(new PostingRequest
        {
            OrganizationId = _org.Id, PostedBy = _user, ValueDate = Today, Narration = "x",
            DebitCodeId = _drCode.Id, CreditCodeId = _crCode.Id,
            Legs = new[] { new PostingLeg("A1", "DR", 10m), new PostingLeg("a1", "CR", 10m) }
        });
        Assert.False(r.Success);
    }

    [Fact]
    public async Task References_are_sequential_within_a_day()
    {
        Account("A1", 100m);
        var r1 = await Service().PostAsync(Req("CR", "A1", 1m));
        var r2 = await Service().PostAsync(Req("CR", "A1", 1m));
        Assert.EndsWith("000001", r1.Transactions[0].TransactionRef);
        Assert.EndsWith("000002", r2.Transactions[0].TransactionRef);
    }

    // ── Withdrawal requests ───────────────────────────────────────────────────

    private async Task<WithdrawalRequest> ApprovedRequest(MemberAccount a, decimal amount)
    {
        var w = new WithdrawalRequest(_org.Id, _branch.Id, a.Id, _approvalCode.Id, amount, _user, "Rent");
        w.Approve(Guid.NewGuid(), null);
        _db.WithdrawalRequests.Add(w);
        await _db.SaveChangesAsync();
        return w;
    }

    private PostingRequest PayOut(MemberAccount a, WithdrawalRequest w, decimal? amount = null) => new()
    {
        OrganizationId = _org.Id, PostedBy = _user, ValueDate = Today, Narration = "Withdrawal", DebitCodeId = _approvalCode.Id,
        ContraGlAccountNo = "1000", WithdrawalRequestId = w.Id, Legs = new[] { new PostingLeg(a.AccountNo, "DR", amount ?? w.Amount) }
    };

    [Fact]
    public async Task An_approved_withdrawal_can_be_paid_once_and_completes_in_the_same_transaction()
    {
        var a = Account("A1", 1000m);
        var w = await ApprovedRequest(a, 400m);

        var r = await Service().PostAsync(PayOut(a, w));
        Assert.True(r.Success, r.Error);
        Assert.Equal(600m, (await Fresh(a)).BookBalance);
        var done = await new MicrobankerDbContext(_options).WithdrawalRequests.SingleAsync();
        Assert.Equal("COMPLETED", done.Status);
        Assert.Equal(r.Transactions[0].TransactionRef, done.CompletedTransactionRef);

        var again = await Service().PostAsync(PayOut(a, w));        // cannot be paid twice
        Assert.False(again.Success);
        Assert.Equal(600m, (await Fresh(a)).BookBalance);
    }

    [Fact]
    public async Task A_withdrawal_payout_must_match_the_approval_and_a_failed_payout_stays_approved()
    {
        var a = Account("A1", 100m);
        var w = await ApprovedRequest(a, 400m);                      // more than the account holds

        Assert.False((await Service().PostAsync(PayOut(a, w, amount: 50m))).Success);   // wrong amount
        var short_ = await Service().PostAsync(PayOut(a, w));                           // right amount, no funds
        Assert.False(short_.Success);
        Assert.Equal("APPROVED", (await new MicrobankerDbContext(_options).WithdrawalRequests.SingleAsync()).Status);
        Assert.Empty(_db.Transactions);

        var pending = new WithdrawalRequest(_org.Id, _branch.Id, a.Id, _approvalCode.Id, 10m, _user, null);   // never approved
        _db.WithdrawalRequests.Add(pending);
        await _db.SaveChangesAsync();
        Assert.False((await Service().PostAsync(PayOut(a, pending))).Success);
    }

    // ── Reversal ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Reversing_a_deposit_removes_the_money_and_links_the_two_transactions()
    {
        var a = Account("A1", 100m);
        var posted = (await Service().PostAsync(Req("CR", "A1", 75m))).Transactions[0];

        var rev = await Service().ReverseAsync(_org.Id, posted.TransactionRef.ToLowerInvariant(), _user, "Wrong account");
        Assert.True(rev.Success, rev.Error);
        Assert.Equal(100m, (await Fresh(a)).BookBalance);

        var all = await new MicrobankerDbContext(_options).Transactions.OrderBy(t => t.TransactionRef).ToListAsync();
        var original = all[0]; var reversal = all[1];
        Assert.Equal("REVERSED", original.Status);
        Assert.Equal(reversal.TransactionRef, original.ReversalRef);
        Assert.Equal("Wrong account", original.ReversalReason);
        Assert.Equal(_user, original.ReversedBy);
        Assert.True(reversal.IsReversal);
        Assert.Equal("DR", reversal.DrCr);
        Assert.Equal(original.TransactionRef, reversal.ReversalRef);
        Assert.Equal("POSTED", reversal.Status);
        Assert.Equal(100m, reversal.BalanceAfter);
        Assert.Equal("1000", reversal.GLAccountNo);
    }

    [Fact]
    public async Task Reversing_a_withdrawal_puts_the_money_back()
    {
        var a = Account("A1", 100m);
        var posted = (await Service().PostAsync(Req("DR", "A1", 30m))).Transactions[0];
        Assert.True((await Service().ReverseAsync(_org.Id, posted.TransactionRef, _user, "Customer changed mind")).Success);
        Assert.Equal(100m, (await Fresh(a)).BookBalance);
    }

    [Fact]
    public async Task A_transaction_cannot_be_reversed_twice_and_a_reversal_cannot_be_reversed()
    {
        Account("A1", 100m);
        var posted = (await Service().PostAsync(Req("CR", "A1", 75m))).Transactions[0];
        var rev = await Service().ReverseAsync(_org.Id, posted.TransactionRef, _user, "x");

        var twice = await Service().ReverseAsync(_org.Id, posted.TransactionRef, _user, "x");
        Assert.False(twice.Success);
        Assert.Contains("already been reversed", twice.Error);

        var undo = await Service().ReverseAsync(_org.Id, rev.Transactions[0].TransactionRef, _user, "x");
        Assert.False(undo.Success);
    }

    [Fact]
    public async Task A_deposit_cannot_be_reversed_once_the_money_has_been_spent()
    {
        var a = Account("A1", 0m);
        var dep = (await Service().PostAsync(Req("CR", "A1", 100m))).Transactions[0];
        await Service().PostAsync(Req("DR", "A1", 80m));

        var r = await Service().ReverseAsync(_org.Id, dep.TransactionRef, _user, "Mistake");
        Assert.False(r.Success);
        Assert.Equal(20m, (await Fresh(a)).BookBalance);
        Assert.Equal("POSTED", (await new MicrobankerDbContext(_options).Transactions.SingleAsync(t => t.TransactionRef == dep.TransactionRef)).Status);
    }

    [Fact]
    public async Task Reversing_one_leg_of_a_batch_reverses_the_whole_batch()
    {
        var a = Account("A1", 500m); var b = Account("B1", 100m);
        var posted = await Service().PostAsync(new PostingRequest
        {
            OrganizationId = _org.Id, PostedBy = _user, ValueDate = Today, Narration = "Transfer",
            DebitCodeId = _drCode.Id, CreditCodeId = _crCode.Id,
            Legs = new[] { new PostingLeg("A1", "DR", 200m), new PostingLeg("B1", "CR", 200m) }
        });

        var rev = await Service().ReverseAsync(_org.Id, posted.Transactions[1].TransactionRef, _user, "Wrong payee");
        Assert.True(rev.Success, rev.Error);
        Assert.Equal(2, rev.Transactions.Count);
        Assert.Equal(500m, (await Fresh(a)).BookBalance);
        Assert.Equal(100m, (await Fresh(b)).BookBalance);
        Assert.All(new MicrobankerDbContext(_options).Transactions.Where(t => !t.IsReversal), t => Assert.Equal("REVERSED", t.Status));
    }

    [Fact]
    public async Task A_batch_reversal_is_refused_entirely_if_one_leg_cannot_be_reversed()
    {
        var a = Account("A1", 500m); var b = Account("B1", 0m);
        var posted = await Service().PostAsync(new PostingRequest
        {
            OrganizationId = _org.Id, PostedBy = _user, ValueDate = Today, Narration = "Transfer",
            DebitCodeId = _drCode.Id, CreditCodeId = _crCode.Id,
            Legs = new[] { new PostingLeg("A1", "DR", 200m), new PostingLeg("B1", "CR", 200m) }
        });
        await Service().PostAsync(Req("DR", "B1", 150m));            // B spends the money

        var rev = await Service().ReverseAsync(_org.Id, posted.Transactions[0].TransactionRef, _user, "x");
        Assert.False(rev.Success);
        Assert.Equal(300m, (await Fresh(a)).BookBalance);            // A's debit was not given back
        Assert.Equal(50m, (await Fresh(b)).BookBalance);
    }

    [Fact]
    public async Task Reversal_needs_a_reason_and_an_existing_transaction()
    {
        Account("A1", 100m);
        var posted = (await Service().PostAsync(Req("CR", "A1", 5m))).Transactions[0];
        Assert.False((await Service().ReverseAsync(_org.Id, posted.TransactionRef, _user, "  ")).Success);
        Assert.False((await Service().ReverseAsync(_org.Id, "TXN-00000000-000001", _user, "x")).Success);
        Assert.False((await Service().ReverseAsync(Guid.NewGuid(), posted.TransactionRef, _user, "x")).Success);   // another organisation
    }

    // ── Concurrency ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Two_debits_racing_for_the_same_money_cannot_both_succeed()
    {
        var a = Account("A1", 100m);

        // A second user has the account open (stale copy) while the first posts a withdrawal.
        using var other = new MicrobankerDbContext(_options);
        var stale = await other.MemberAccounts.SingleAsync(x => x.Id == a.Id);

        Assert.True((await Service().PostAsync(Req("DR", "A1", 80m))).Success);

        stale.Debit(80m, DateTime.UtcNow);
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => other.SaveChangesAsync());
        Assert.Equal(20m, (await Fresh(a)).BookBalance);
    }

    [Fact]
    public async Task The_database_itself_refuses_a_negative_balance()
    {
        var a = Account("A1", 10m);
        // Bypass the domain rules on purpose: the check constraint is the last line of defence.
        typeof(MemberAccount).GetProperty(nameof(MemberAccount.BookBalance))!.SetValue(a, -1m);
        await Assert.ThrowsAsync<DbUpdateException>(() => _db.SaveChangesAsync());
    }
}
