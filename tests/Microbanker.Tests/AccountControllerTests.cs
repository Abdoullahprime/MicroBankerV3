using System.Security.Claims;
using Microbanker.Domain.Entities.Accounts;
using Microbanker.Domain.Entities.Customers;
using Microbanker.Domain.Entities.Organization;
using Microbanker.Domain.Entities.Setup;
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

/// <summary>Runs the real AccountController against in-memory SQLite (no SQL Server needed).</summary>
public sealed class AccountControllerTests : IDisposable
{
    private readonly SqliteConnection _conn = new("DataSource=:memory:");
    private readonly DbContextOptions<MicrobankerDbContext> _options;
    private readonly MicrobankerDbContext _db;
    private readonly IEncryptionService _enc;
    private readonly Organization _org = new("Test Org", "TST");
    private readonly Branch _branch;
    private readonly Customer _customer;
    private readonly ProductDefinition _product;
    private readonly Guid _userId = Guid.NewGuid();

    public AccountControllerTests()
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

        _customer = new Customer(_org.Id, _branch.Id, "CUST-2026-000001", "INDIVIDUAL", "Awa", "Jallow");
        _customer.Update(new CustomerProfile { BranchId = _branch.Id, FirstName = "Awa", LastName = "Jallow" });
        _customer.Approve(Guid.NewGuid(), null);
        _db.Customers.Add(_customer);

        var type = new ProductType(_org.Id, "Savings", "SAV", null);
        _db.ProductTypes.Add(type);
        _product = new ProductDefinition(_org.Id, type.Id, "Basic Savings", "BSV");
        _db.ProductDefinitions.Add(_product);
        _db.SaveChanges();
    }

    public void Dispose() { _db.Dispose(); _conn.Dispose(); }

    private AccountController Accounts(MicrobankerDbContext? db = null)
    {
        var c = new AccountController(db ?? _db, _enc);
        var http = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("companyId", _org.Id.ToString()), new Claim("userId", _userId.ToString())
            }, "test"))
        };
        c.ControllerContext = new ControllerContext(new ActionContext(http, new RouteData(), new ControllerActionDescriptor()));
        c.TempData = new TempDataDictionary(http, new NullTempData());
        return c;
    }

    private string Id(Guid id) => _enc.Encrypt(id.ToString());

    private AccountFormModel Form(string type = "SAVINGS", decimal opening = 0m) => new()
    {
        CustomerNo = _customer.CustomerNo, BranchId = _branch.Id, AccountType = type,
        ProductDefinitionId = _product.Id, Currency = "GMD", OpeningBalance = opening
    };

    private async Task<MemberAccount> OpenAccount(decimal opening = 1000m, bool approve = true)
    {
        var result = await Accounts().Create(Form(opening: opening));
        Assert.IsType<RedirectToActionResult>(result);
        var account = await _db.MemberAccounts.OrderByDescending(a => a.CreatedAt).FirstAsync();
        if (approve)
        {
            await Accounts().Approve(Id(account.Id));
            await _db.Entry(account).ReloadAsync();
        }
        return account;
    }

    private async Task<MemberAccount> Reload(MemberAccount a) { await _db.Entry(a).ReloadAsync(); return a; }

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_issues_branch_type_sequence_numbers_per_type()
    {
        await Accounts().Create(Form("SAVINGS"));
        await Accounts().Create(Form("SAVINGS"));
        await Accounts().Create(Form("CURRENT"));

        var nos = await _db.MemberAccounts.OrderBy(a => a.AccountNo).Select(a => a.AccountNo).ToListAsync();
        Assert.Equal(new[] { "MN-CA-0000001", "MN-SA-0000001", "MN-SA-0000002" }, nos);
    }

    [Fact]
    public async Task New_accounts_are_pending_and_default_the_name_to_the_customer()
    {
        var a = await OpenAccount(250m, approve: false);
        Assert.Equal("PENDING", a.Status);
        Assert.Equal("Awa Jallow", a.AccountName);
        Assert.Equal(250m, a.BookBalance);
        Assert.Equal(_userId, a.CreatedByUserId);
    }

    [Fact]
    public async Task Numbers_are_not_reused_after_a_delete()
    {
        var first = await OpenAccount(approve: false);
        await Accounts().Delete(Id(first.Id));
        await Accounts().Create(Form());
        var live = await _db.MemberAccounts.SingleAsync(a => !a.IsDeleted);
        Assert.EndsWith("0000002", live.AccountNo);
    }

    [Fact]
    public async Task Create_rejects_unknown_and_inactive_customers()
    {
        var c = Accounts();
        var f = Form(); f.CustomerNo = "CUST-9999-000001";
        Assert.IsType<ViewResult>(await c.Create(f));
        Assert.True(c.ModelState.ContainsKey(nameof(AccountFormModel.CustomerNo)));

        var pending = new Customer(_org.Id, _branch.Id, "CUST-2026-000002", "INDIVIDUAL", "Bakary", "Ceesay");
        pending.Update(new CustomerProfile { BranchId = _branch.Id, FirstName = "Bakary", LastName = "Ceesay" });
        _db.Customers.Add(pending);
        await _db.SaveChangesAsync();

        var c2 = Accounts();
        var f2 = Form(); f2.CustomerNo = pending.CustomerNo;
        Assert.IsType<ViewResult>(await c2.Create(f2));
        Assert.True(c2.ModelState.ContainsKey(nameof(AccountFormModel.CustomerNo)));
        Assert.Empty(_db.MemberAccounts);
    }

    [Fact]
    public async Task Create_enforces_the_product_minimum_balance()
    {
        var def = await _db.ProductDefinitions.FirstAsync();
        def.Update(def.ProductTypeId, def.Name, def.Code, 100m, 0m, null, 0, 0, 0, null, true);
        await _db.SaveChangesAsync();

        var c = Accounts();
        Assert.IsType<ViewResult>(await c.Create(Form(opening: 50m)));
        Assert.True(c.ModelState.ContainsKey(nameof(AccountFormModel.OpeningBalance)));

        Assert.IsType<RedirectToActionResult>(await Accounts().Create(Form(opening: 100m)));
    }

    [Fact]
    public async Task Create_checks_gl_account_types_and_dates()
    {
        _db.GLAccounts.Add(new GLAccount(_org.Id, "2000", "Deposits", "LIABILITY", null, 1));
        _db.GLAccounts.Add(new GLAccount(_org.Id, "1000", "Cash", "ASSET", null, 1));
        await _db.SaveChangesAsync();

        var wrong = Form(); wrong.GLLiabilityControl = "1000";           // an asset where a liability is needed
        var c = Accounts();
        Assert.IsType<ViewResult>(await c.Create(wrong));
        Assert.True(c.ModelState.ContainsKey(nameof(AccountFormModel.GLLiabilityControl)));

        var future = Form(); future.OpenDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3));
        var c2 = Accounts();
        Assert.IsType<ViewResult>(await c2.Create(future));
        Assert.True(c2.ModelState.ContainsKey(nameof(AccountFormModel.OpenDate)));

        var ok = Form(); ok.GLLiabilityControl = "2000";
        Assert.IsType<RedirectToActionResult>(await Accounts().Create(ok));
    }

    // ── Approve / delete ──────────────────────────────────────────────────────

    [Fact]
    public async Task Approve_activates_and_active_accounts_cannot_be_deleted()
    {
        var a = await OpenAccount();
        Assert.Equal("ACTIVE", a.Status);
        Assert.Equal(_userId, a.ApprovedBy);

        await Accounts().Delete(Id(a.Id));
        await Reload(a);
        Assert.False(a.IsDeleted);
    }

    [Fact]
    public async Task A_tampered_id_is_not_found_instead_of_throwing()
    {
        Assert.IsType<NotFoundResult>(await Accounts().Edit("not-a-valid-id"));
        Assert.IsType<NotFoundResult>(await Accounts().Details("%%%"));
        Assert.IsType<NotFoundResult>(await Accounts().BlockAmount("junk"));
    }

    // ── Editing after approval ────────────────────────────────────────────────

    [Fact]
    public async Task Approved_accounts_need_an_approved_edit_request_and_it_is_single_use()
    {
        var a = await OpenAccount();
        var id = Id(a.Id);

        // No request yet: the edit page bounces back to the account.
        Assert.IsType<RedirectToActionResult>(await Accounts().Edit(id));

        await Accounts().RequestEdit(id, "Change the name");
        await Reload(a);
        Assert.True(a.EditRequestPending);
        Assert.IsType<RedirectToActionResult>(await Accounts().Edit(id));        // still pending review

        await Accounts().ApproveEditRequest(id, null);
        await Reload(a);
        Assert.False(a.EditRequestPending);
        Assert.IsType<ViewResult>(await Accounts().Edit(id));                    // now allowed

        var form = Form(); form.AccountName = "Awa B. Jallow"; form.OpeningBalance = 999999m;   // tries to tamper with a locked field
        var saved = await Accounts().Edit(id, form);
        Assert.IsType<RedirectToActionResult>(saved);
        await Reload(a);
        Assert.Equal("Awa B. Jallow", a.AccountName);
        Assert.Equal(1000m, a.OpeningBalance);                                    // locked field ignored
        Assert.Equal(1000m, a.BookBalance);
        Assert.Equal("COMPLETED", (await _db.AccountEditRequests.SingleAsync()).Status);

        Assert.IsType<RedirectToActionResult>(await Accounts().Edit(id));        // the approval is used up
    }

    [Fact]
    public async Task Edit_requests_need_a_reason_and_only_one_can_be_open()
    {
        var a = await OpenAccount();
        var id = Id(a.Id);
        await Accounts().RequestEdit(id, "  ");
        Assert.Empty(_db.AccountEditRequests);

        await Accounts().RequestEdit(id, "First");
        await Accounts().RequestEdit(id, "Second");
        Assert.Single(_db.AccountEditRequests);

        await Accounts().RejectEditRequest(id, "");                              // a rejection needs a reason
        Assert.Equal("PENDING", (await _db.AccountEditRequests.SingleAsync()).Status);
        await Accounts().RejectEditRequest(id, "Not justified");
        Assert.Equal("REJECTED", (await _db.AccountEditRequests.SingleAsync()).Status);
    }

    [Fact]
    public async Task Pending_accounts_can_change_their_terms_directly()
    {
        var a = await OpenAccount(100m, approve: false);
        var f = Form(opening: 400m); f.AccountName = "Renamed";
        Assert.IsType<RedirectToActionResult>(await Accounts().Edit(Id(a.Id), f));
        await Reload(a);
        Assert.Equal(400m, a.OpeningBalance);
        Assert.Equal(400m, a.BookBalance);
        Assert.Equal("Renamed", a.AccountName);
    }

    // ── Block / unblock ───────────────────────────────────────────────────────

    [Fact]
    public async Task Block_and_release_update_the_balances()
    {
        var a = await OpenAccount(1000m);
        await Accounts().BlockAmount(Id(a.Id), 400m);
        await Reload(a);
        Assert.Equal(400m, a.BlockedAmount);
        Assert.Equal(600m, a.AvailableBalance);

        await Accounts().UnblockAmount(Id(a.Id), 150m);
        await Reload(a);
        Assert.Equal(250m, a.BlockedAmount);
        Assert.Equal(750m, a.AvailableBalance);
    }

    [Fact]
    public async Task Invalid_block_amounts_change_nothing()
    {
        var a = await OpenAccount(100m);
        foreach (var bad in new[] { 0m, -1m, 100.01m, 10.005m })
            await Accounts().BlockAmount(Id(a.Id), bad);
        await Accounts().UnblockAmount(Id(a.Id), 5m);                            // nothing blocked yet
        await Reload(a);
        Assert.Equal(0m, a.BlockedAmount);
        Assert.Equal(100m, a.AvailableBalance);
    }

    [Fact]
    public async Task Concurrent_changes_to_the_same_account_do_not_overwrite_each_other()
    {
        var a = await OpenAccount(1000m);

        using var other = new MicrobankerDbContext(_options);
        var stale = await other.MemberAccounts.SingleAsync(x => x.Id == a.Id);

        await Accounts().BlockAmount(Id(a.Id), 100m);                            // first user wins

        stale.BlockAmount(200m);                                                  // second user worked from the old copy
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => other.SaveChangesAsync());

        await Reload(a);
        Assert.Equal(100m, a.BlockedAmount);
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Freeze_unfreeze_and_close_follow_the_rules()
    {
        var a = await OpenAccount(0m);
        await Accounts().Freeze(Id(a.Id));
        await Reload(a);
        Assert.Equal("FROZEN", a.Status);
        await Accounts().Unfreeze(Id(a.Id));
        await Reload(a);
        Assert.Equal("ACTIVE", a.Status);
        await Accounts().Close(Id(a.Id));
        await Reload(a);
        Assert.Equal("CLOSED", a.Status);

        var funded = await OpenAccount(50m);
        await Accounts().Close(Id(funded.Id));                                   // still holds money
        await Reload(funded);
        Assert.Equal("ACTIVE", funded.Status);
    }

    // ── Enquiry and listing ───────────────────────────────────────────────────

    [Fact]
    public async Task Balance_enquiry_finds_accounts_by_number_in_any_case()
    {
        var a = await OpenAccount(321m);

        var found = (BalanceEnquiryViewModel)((ViewResult)await Accounts().BalanceEnquiry(a.AccountNo.ToLowerInvariant())).Model!;
        Assert.True(found.Searched);
        Assert.Equal(321m, found.Account!.BookBalance);

        var missing = (BalanceEnquiryViewModel)((ViewResult)await Accounts().BalanceEnquiry("NOPE-1")).Model!;
        Assert.True(missing.Searched);
        Assert.Null(missing.Account);

        var blank = (BalanceEnquiryViewModel)((ViewResult)await Accounts().BalanceEnquiry(null)).Model!;
        Assert.False(blank.Searched);
    }

    [Fact]
    public async Task Accounts_of_another_organisation_are_not_reachable()
    {
        var other = new Organization("Other", "OTH");
        _db.Organizations.Add(other);
        var ob = new Branch(other.Id, "B", "B1");
        _db.Branches.Add(ob);
        var oc = new Customer(other.Id, ob.Id, "CUST-2026-000001", "INDIVIDUAL", "X", "Y");
        oc.Update(new CustomerProfile { BranchId = ob.Id, FirstName = "X", LastName = "Y" });
        _db.Customers.Add(oc);
        var ot = new ProductType(other.Id, "S", "S", null);
        _db.ProductTypes.Add(ot);
        var op = new ProductDefinition(other.Id, ot.Id, "P", "P");
        _db.ProductDefinitions.Add(op);
        var foreign = new MemberAccount(other.Id, ob.Id, oc.Id, op.Id, "B1-SA-0000001", "X Y", "SAVINGS");
        foreign.UpdateTerms(op.Id, "GMD", 10m, new DateOnly(2026, 1, 1));
        _db.MemberAccounts.Add(foreign);
        await _db.SaveChangesAsync();

        Assert.IsType<NotFoundResult>(await Accounts().Details(Id(foreign.Id)));
        var enquiry = (BalanceEnquiryViewModel)((ViewResult)await Accounts().BalanceEnquiry("B1-SA-0000001")).Model!;
        Assert.Null(enquiry.Account);
    }

    [Fact]
    public async Task Index_filters_by_search_status_and_type()
    {
        await OpenAccount(10m);                                       // active savings
        await Accounts().Create(Form("CURRENT"));                     // pending current

        var all = (AccountListViewModel)((ViewResult)await Accounts().Index(null, null, null)).Model!;
        Assert.Equal(2, all.TotalCount);
        var active = (AccountListViewModel)((ViewResult)await Accounts().Index("ACTIVE", null, null)).Model!;
        Assert.Single(active.Rows);
        var current = (AccountListViewModel)((ViewResult)await Accounts().Index(null, "CURRENT", null)).Model!;
        Assert.Equal("MN-CA-0000001", current.Rows.Single().AccountNo);
        var byNumber = (AccountListViewModel)((ViewResult)await Accounts().Index(null, null, "MN-SA")).Model!;
        Assert.Single(byNumber.Rows);
    }

    private sealed class NullTempData : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
