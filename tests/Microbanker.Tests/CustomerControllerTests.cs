using System.Security.Claims;
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

/// <summary>Runs the real controllers against an in-memory SQLite database (no SQL Server needed).</summary>
public sealed class CustomerControllerTests : IDisposable
{
    private readonly SqliteConnection _conn = new("DataSource=:memory:");
    private readonly MicrobankerDbContext _db;
    private readonly IEncryptionService _enc;
    private readonly Organization _org = new("Test Org", "TST");
    private readonly Branch _branch;
    private readonly Guid _userId = Guid.NewGuid();

    public CustomerControllerTests()
    {
        _conn.Open();
        _db = new MicrobankerDbContext(new DbContextOptionsBuilder<MicrobankerDbContext>().UseSqlite(_conn).Options);
        _db.Database.EnsureCreated();
        _enc = new AesEncryptionService(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["EncryptionKey"] = "unit-test-key-0123456789-abcdefgh" }).Build());

        _db.Organizations.Add(_org);
        _branch = new Branch(_org.Id, "Main", "MN");
        _db.Branches.Add(_branch);
        _db.SaveChanges();
    }

    public void Dispose() { _db.Dispose(); _conn.Dispose(); }

    private T WithContext<T>(T controller) where T : Controller
    {
        var http = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("companyId", _org.Id.ToString()), new Claim("userId", _userId.ToString())
            }, "test"))
        };
        controller.ControllerContext = new ControllerContext(new ActionContext(http, new RouteData(), new ControllerActionDescriptor()));
        controller.TempData = new TempDataDictionary(http, new NullTempData());
        return controller;
    }

    private CustomerController Customers() => WithContext(new CustomerController(_db, _enc));
    private SelfRegistrationController Registrations() => WithContext(new SelfRegistrationController(_db, _enc));

    private CustomerFormModel Person(string first = "Awa", string last = "Jallow", string? nationalId = null) => new()
    {
        BranchId = _branch.Id, CustomerType = "INDIVIDUAL", FirstName = first, LastName = last, NationalId = nationalId
    };

    private string Id(Guid id) => _enc.Encrypt(id.ToString());

    [Fact]
    public async Task Create_generates_sequential_numbers_and_starts_pending()
    {
        var c = Customers();
        Assert.IsType<RedirectToActionResult>(await c.Create(Person()));
        Assert.IsType<RedirectToActionResult>(await c.Create(Person("Bakary", "Ceesay")));

        var nos = await _db.Customers.OrderBy(x => x.CustomerNo).Select(x => x.CustomerNo).ToListAsync();
        var year = DateTime.UtcNow.Year;
        Assert.Equal(new[] { $"CUST-{year}-000001", $"CUST-{year}-000002" }, nos);
        Assert.All(_db.Customers, x => Assert.Equal("PENDING", x.Status));
    }

    [Fact]
    public async Task Numbers_are_not_reused_after_a_delete()
    {
        var c = Customers();
        await c.Create(Person());
        var first = await _db.Customers.SingleAsync();
        await c.Delete(Id(first.Id));                      // pending, so deletable
        await c.Create(Person("Bakary", "Ceesay"));

        var live = await _db.Customers.SingleAsync(x => !x.IsDeleted);
        Assert.EndsWith("000002", live.CustomerNo);
    }

    [Fact]
    public async Task Create_rejects_a_missing_name_for_an_individual()
    {
        var c = Customers();
        var result = await c.Create(Person(first: "", last: ""));
        Assert.IsType<ViewResult>(result);
        Assert.False(c.ModelState.IsValid);
        Assert.Empty(_db.Customers);
    }

    [Fact]
    public async Task Create_rejects_a_duplicate_national_id()
    {
        var c = Customers();
        await c.Create(Person(nationalId: "NID-1"));
        var again = Customers();
        var result = await again.Create(Person("Bakary", "Ceesay", "NID-1"));
        Assert.IsType<ViewResult>(result);
        Assert.True(again.ModelState.ContainsKey(nameof(CustomerFormModel.NationalId)));
        Assert.Single(_db.Customers);
    }

    [Fact]
    public async Task Create_rejects_a_district_that_is_not_under_the_chosen_region()
    {
        var r1 = new Region(_org.Id, "R1", "R1"); var r2 = new Region(_org.Id, "R2", "R2");
        _db.Regions.AddRange(r1, r2);
        var d = new District(_org.Id, r1.Id, "D1", "D1");
        _db.Districts.Add(d);
        await _db.SaveChangesAsync();

        var m = Person(); m.RegionId = r2.Id; m.DistrictId = d.Id;
        var c = Customers();
        Assert.IsType<ViewResult>(await c.Create(m));
        Assert.True(c.ModelState.ContainsKey(nameof(CustomerFormModel.DistrictId)));

        var ok = Person(); ok.RegionId = r1.Id; ok.DistrictId = d.Id;
        Assert.IsType<RedirectToActionResult>(await Customers().Create(ok));
    }

    [Fact]
    public async Task Group_counts_must_add_up()
    {
        var m = new CustomerFormModel { BranchId = _branch.Id, CustomerType = "GROUP", GroupName = "G", NoOfMembers = 10, NoOfMale = 3, NoOfFemale = 3 };
        var c = Customers();
        Assert.IsType<ViewResult>(await c.Create(m));
        Assert.True(c.ModelState.ContainsKey(nameof(CustomerFormModel.NoOfMembers)));
    }

    [Fact]
    public async Task Approve_then_active_customers_cannot_be_deleted()
    {
        await Customers().Create(Person());
        var cust = await _db.Customers.SingleAsync();

        await Customers().Approve(Id(cust.Id), "fine");
        await _db.Entry(cust).ReloadAsync();
        Assert.Equal("ACTIVE", cust.Status);
        Assert.Equal(_userId, cust.ApprovedBy);

        await Customers().Delete(Id(cust.Id));
        await _db.Entry(cust).ReloadAsync();
        Assert.False(cust.IsDeleted);
    }

    [Fact]
    public async Task A_tampered_id_is_not_found_instead_of_throwing()
    {
        Assert.IsType<NotFoundResult>(await Customers().Edit("not-a-valid-id"));
        Assert.IsType<NotFoundResult>(await Customers().Details("%%%"));
    }

    [Fact]
    public async Task Customers_of_another_organisation_are_not_reachable()
    {
        var other = new Organization("Other", "OTH");
        _db.Organizations.Add(other);
        var otherBranch = new Branch(other.Id, "B", "B1");
        _db.Branches.Add(otherBranch);
        var foreign = new Customer(other.Id, otherBranch.Id, "CUST-2026-000001", "INDIVIDUAL", "X", "Y");
        foreign.Update(new CustomerProfile { BranchId = otherBranch.Id, FirstName = "X", LastName = "Y" });
        _db.Customers.Add(foreign);
        await _db.SaveChangesAsync();

        Assert.IsType<NotFoundResult>(await Customers().Details(Id(foreign.Id)));
    }

    [Fact]
    public async Task Index_filters_and_pages()
    {
        var c = Customers();
        await c.Create(Person("Awa", "Jallow"));
        await c.Create(Person("Bakary", "Ceesay"));

        var all = (CustomerListViewModel)((ViewResult)await Customers().Index(null, null)).Model!;
        Assert.Equal(2, all.TotalCount);

        var found = (CustomerListViewModel)((ViewResult)await Customers().Index(null, "Ceesay")).Model!;  // exact case: SQLite compares case-sensitively; SQL Server collation does not
        Assert.Single(found.Rows);
        Assert.Equal("Bakary Ceesay", found.Rows[0].DisplayName);

        var none = (CustomerListViewModel)((ViewResult)await Customers().Index("ACTIVE", null)).Model!;
        Assert.Empty(none.Rows);
    }

    // ── Self-registration ─────────────────────────────────────────────────────

    private async Task<SelfRegistration> NewRegistration()
    {
        await Registrations().Create(new SelfRegFormModel { BranchId = _branch.Id, CustomerType = "INDIVIDUAL", FirstName = "Mariama", LastName = "Sowe", NationalId = "NID-9" });
        return await _db.SelfRegistrations.SingleAsync(r => r.Status == "PENDING");
    }

    [Fact]
    public async Task Approving_a_registration_creates_an_active_linked_customer()
    {
        var reg = await NewRegistration();
        await Registrations().Approve(Id(reg.Id));

        await _db.Entry(reg).ReloadAsync();
        Assert.Equal("APPROVED", reg.Status);
        var cust = await _db.Customers.SingleAsync();
        Assert.Equal(reg.CustomerId, cust.Id);
        Assert.Equal("ACTIVE", cust.Status);
        Assert.Equal("Mariama Sowe", cust.DisplayName);
        Assert.Equal("NID-9", cust.NationalId);
    }

    [Fact]
    public async Task A_registration_cannot_be_approved_twice()
    {
        var reg = await NewRegistration();
        await Registrations().Approve(Id(reg.Id));
        await Registrations().Approve(Id(reg.Id));
        Assert.Single(_db.Customers);
    }

    [Fact]
    public async Task Approval_is_blocked_when_the_national_id_is_already_a_customer()
    {
        await Customers().Create(Person(nationalId: "NID-9"));
        var reg = await NewRegistration();
        await Registrations().Approve(Id(reg.Id));

        await _db.Entry(reg).ReloadAsync();
        Assert.Equal("PENDING", reg.Status);
        Assert.Single(_db.Customers);
    }

    [Fact]
    public async Task Reject_needs_a_reason_and_approved_registrations_cannot_be_deleted()
    {
        var reg = await NewRegistration();
        await Registrations().Reject(Id(reg.Id), "  ");
        await _db.Entry(reg).ReloadAsync();
        Assert.Equal("PENDING", reg.Status);

        await Registrations().Reject(Id(reg.Id), "Photo unreadable");
        await _db.Entry(reg).ReloadAsync();
        Assert.Equal("REJECTED", reg.Status);
        Assert.Equal("Photo unreadable", reg.RejectionReason);

        var reg2 = await NewRegistration();
        await Registrations().Approve(Id(reg2.Id));
        await Registrations().Delete(Id(reg2.Id));
        await _db.Entry(reg2).ReloadAsync();
        Assert.False(reg2.IsDeleted);
    }

    private sealed class NullTempData : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
