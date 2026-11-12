using System.Reflection;
using System.Security.Claims;
using Microbanker.Domain.Entities.Auth;
using Microbanker.Domain.Entities.Organization;
using Microbanker.Domain.Security;
using Microbanker.Infrastructure.Persistence;
using Microbanker.Web.Controllers;
using Microbanker.Web.Models;
using Microbanker.Web.Security;
using Microbanker.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microbanker.Tests;

// ═════════════════════════════════════════════════════════════════════════════
//  The rules: which permission an action needs
// ═════════════════════════════════════════════════════════════════════════════

public class RequirePermissionDerivationTests
{
    [Theory]
    [InlineData("Create", "GET", "CREATE")]
    [InlineData("CreateRequest", "POST", "CREATE")]
    [InlineData("AddLine", "POST", "CREATE")]
    [InlineData("Edit", "GET", "EDIT")]
    [InlineData("Update", "POST", "EDIT")]
    [InlineData("Save", "POST", "EDIT")]
    [InlineData("Delete", "POST", "DELETE")]
    [InlineData("Remove", "POST", "DELETE")]
    [InlineData("Approve", "POST", "APPROVE")]
    [InlineData("ApproveWithdrawal", "POST", "APPROVE")]
    [InlineData("Export", "GET", "EXPORT")]
    [InlineData("Index", "GET", "VIEW")]
    [InlineData("View", "GET", "VIEW")]
    [InlineData("TrialBalance", "GET", "VIEW")]
    [InlineData("Index", "HEAD", "VIEW")]
    public void Names_and_methods_map_to_the_expected_permission(string action, string method, string expected)
        => Assert.Equal(expected, RequirePermissionAttribute.Derive(action, method));

    [Theory]
    [InlineData("Freeze")]
    [InlineData("Reactivate")]
    [InlineData("SomethingNobodyClassified")]
    [InlineData("")]
    public void An_unclassified_write_needs_EDIT_never_just_VIEW(string action)
    {
        Assert.Equal("EDIT", RequirePermissionAttribute.Derive(action, "POST"));
        Assert.Equal("EDIT", RequirePermissionAttribute.Derive(action, "PUT"));
        Assert.Equal("EDIT", RequirePermissionAttribute.Derive(action, "DELETE"));
    }

    [Fact]
    public void An_unclassified_read_needs_VIEW()
        => Assert.Equal("VIEW", RequirePermissionAttribute.Derive("SomethingNobodyClassified", "GET"));
}

// ═════════════════════════════════════════════════════════════════════════════
//  The filter itself
// ═════════════════════════════════════════════════════════════════════════════

public class RequirePermissionFilterTests
{
    private sealed class FakePermissions : IPermissionService
    {
        private readonly PermissionSet _set;
        public FakePermissions(PermissionSet set) => _set = set;
        public Task<PermissionSet> GetCurrentAsync() => Task.FromResult(_set);
    }

    private static AuthorizationFilterContext Context(PermissionSet perms, string method, string action, bool authenticated = true,
        params RequirePermissionAttribute[] metadata)
    {
        var services = new ServiceCollection().AddSingleton<IPermissionService>(new FakePermissions(perms)).BuildServiceProvider();
        var http = new DefaultHttpContext { RequestServices = services };
        http.Request.Method = method;
        if (authenticated) http.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("userId", Guid.NewGuid().ToString()) }, "test"));

        var descriptor = new ActionDescriptor { EndpointMetadata = metadata.Cast<object>().ToList() };
        descriptor.RouteValues["action"] = action;
        return new AuthorizationFilterContext(new ActionContext(http, new RouteData(), descriptor), new List<IFilterMetadata>());
    }

    private static PermissionSet Granted(params string[] pairs) => new(false, pairs.ToHashSet());

    private static async Task<IActionResult?> Run(RequirePermissionAttribute attr, PermissionSet perms, string method, string action, bool authenticated = true, params RequirePermissionAttribute[] metadata)
    {
        var ctx = Context(perms, method, action, authenticated, metadata.Length == 0 ? new[] { attr } : metadata);
        await attr.OnAuthorizationAsync(ctx);
        return ctx.Result;
    }

    [Fact]
    public async Task A_user_with_the_permission_passes()
    {
        var attr = new RequirePermissionAttribute(Modules.Customers);
        Assert.Null(await Run(attr, Granted("CUSTOMERS:VIEW"), "GET", "Index"));
    }

    [Fact]
    public async Task A_user_without_it_is_sent_to_the_access_denied_page()
    {
        var attr = new RequirePermissionAttribute(Modules.Customers);
        var result = Assert.IsType<RedirectToActionResult>(await Run(attr, Granted("CUSTOMERS:VIEW"), "POST", "Delete"));
        Assert.Equal("Unauthorised", result.ActionName);
        Assert.Equal("Auth", result.ControllerName);
    }

    [Fact]
    public async Task Holding_a_permission_on_another_module_does_not_help()
    {
        var attr = new RequirePermissionAttribute(Modules.Customers);
        Assert.IsType<RedirectToActionResult>(await Run(attr, Granted("ACCOUNTS:VIEW", "ACCOUNTS:EDIT"), "GET", "Index"));
    }

    [Fact]
    public async Task A_super_administrator_passes_everything()
    {
        var attr = new RequirePermissionAttribute(Modules.Processing, PermissionActions.Approve);
        Assert.Null(await Run(attr, new PermissionSet(true, new HashSet<string>()), "POST", "EOD"));
    }

    [Fact]
    public async Task An_unauthenticated_request_is_left_to_the_login_check()
    {
        var attr = new RequirePermissionAttribute(Modules.Customers);
        Assert.Null(await Run(attr, PermissionSet.None, "GET", "Index", authenticated: false));
    }

    [Fact]
    public async Task The_action_level_attribute_beats_the_class_level_one()
    {
        var onClass = new RequirePermissionAttribute(Modules.Teller);
        var onMethod = new RequirePermissionAttribute(Modules.Teller, PermissionActions.Approve);
        var perms = Granted("TELLER:VIEW");

        // Class-level filter stands aside because the method's attribute is the most specific.
        Assert.Null(await Run(onClass, perms, "POST", "RejectWithdrawal", true, onClass, onMethod));
        // The method-level filter decides, and TELLER:VIEW is not APPROVE.
        Assert.IsType<RedirectToActionResult>(await Run(onMethod, perms, "POST", "RejectWithdrawal", true, onClass, onMethod));
        Assert.Null(await Run(onMethod, Granted("TELLER:APPROVE"), "POST", "RejectWithdrawal", true, onClass, onMethod));
    }

    [Fact]
    public async Task Without_an_explicit_action_the_http_method_decides()
    {
        var attr = new RequirePermissionAttribute(Modules.Accounts);
        var viewer = Granted("ACCOUNTS:VIEW");
        Assert.Null(await Run(attr, viewer, "GET", "MiniStatement"));
        Assert.IsType<RedirectToActionResult>(await Run(attr, viewer, "POST", "MiniStatement"));        // a write needs EDIT
        Assert.Null(await Run(attr, Granted("ACCOUNTS:EDIT"), "POST", "Anything"));
    }
}

// ═════════════════════════════════════════════════════════════════════════════
//  The coverage guard: every controller and action is protected, and nothing refers to an unknown module
// ═════════════════════════════════════════════════════════════════════════════

public class PermissionCoverageTests
{
    private static readonly string[] AuthenticationOnly = { "HomeController", "AuthController" };

    private static IEnumerable<Type> Controllers() => typeof(BaseController).Assembly.GetTypes()
        .Where(t => !t.IsAbstract && typeof(Controller).IsAssignableFrom(t));

    private static IEnumerable<MethodInfo> Actions(Type controller) => controller
        .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
        .Where(m => !m.IsSpecialName && typeof(IActionResult).IsAssignableFrom(m.ReturnType.IsGenericType ? m.ReturnType.GetGenericArguments()[0] : m.ReturnType)
                    && m.GetCustomAttribute<NonActionAttribute>() == null);

    private static string HttpMethodOf(MethodInfo m)
        => m.GetCustomAttributes().OfType<HttpPostAttribute>().Any() ? "POST"
         : m.GetCustomAttributes().OfType<HttpPutAttribute>().Any() ? "PUT"
         : m.GetCustomAttributes().OfType<HttpDeleteAttribute>().Any() ? "DELETE" : "GET";

    private static string ActionName(MethodInfo m) => m.GetCustomAttribute<ActionNameAttribute>()?.Name ?? m.Name;

    /// <summary>(module, action code) an endpoint needs, exactly as the filter works it out.</summary>
    private static (string Module, string Action) Needs(Type controller, MethodInfo m)
    {
        var attr = m.GetCustomAttribute<RequirePermissionAttribute>() ?? controller.GetCustomAttribute<RequirePermissionAttribute>()!;
        return (attr.Module, attr.Action ?? RequirePermissionAttribute.Derive(ActionName(m), HttpMethodOf(m)));
    }

    [Fact]
    public void Every_controller_is_secured_except_the_ones_that_only_need_a_login()
    {
        var unprotected = Controllers()
            .Where(c => c.GetCustomAttribute<RequirePermissionAttribute>() == null && !AuthenticationOnly.Contains(c.Name))
            .Select(c => c.Name).ToList();
        Assert.True(unprotected.Count == 0, "Controllers without [RequirePermission]: " + string.Join(", ", unprotected));
    }

    [Fact]
    public void Every_module_an_attribute_names_is_a_seeded_module()
    {
        var seeded = DbSeeder.ModuleCodes.ToHashSet();
        var used = Controllers().SelectMany(c => c.GetCustomAttributes<RequirePermissionAttribute>()
                .Concat(Actions(c).Select(a => a.GetCustomAttribute<RequirePermissionAttribute>()).Where(a => a != null)!))
            .Select(a => a!.Module).Distinct().ToList();
        var unknown = used.Where(m => !seeded.Contains(m)).ToList();
        Assert.True(unknown.Count == 0, "Modules that are not seeded (nobody could ever be granted them): " + string.Join(", ", unknown));
    }

    [Fact]
    public void Every_explicit_action_code_is_a_real_permission_action()
    {
        var valid = new[] { "VIEW", "CREATE", "EDIT", "DELETE", "APPROVE", "EXPORT" };
        var bad = Controllers().SelectMany(c => Actions(c).Select(a => a.GetCustomAttribute<RequirePermissionAttribute>()).Where(a => a?.Action != null))
            .Select(a => a!.Action!).Where(a => !valid.Contains(a)).ToList();
        Assert.Empty(bad);
    }

    [Fact]
    public void The_modules_constants_and_the_seeder_list_match()
    {
        var constants = typeof(Modules).GetFields(BindingFlags.Public | BindingFlags.Static).Select(f => (string)f.GetRawConstantValue()!).ToHashSet();
        Assert.Equal(constants.OrderBy(x => x), DbSeeder.ModuleCodes.ToHashSet().OrderBy(x => x));
    }

    [Fact]
    public void Every_state_changing_action_needs_more_than_view()
    {
        var weak = new List<string>();
        foreach (var c in Controllers().Where(c => !AuthenticationOnly.Contains(c.Name)))
            foreach (var a in Actions(c).Where(a => HttpMethodOf(a) != "GET"))
                if (Needs(c, a).Action == PermissionActions.View) weak.Add($"{c.Name}.{a.Name}");
        Assert.True(weak.Count == 0, "Writes that only need VIEW: " + string.Join(", ", weak));
    }

    [Theory]
    [InlineData("TransactionController", "Reverse", "POST", "TRANS_VIEW", "DELETE")]
    [InlineData("TellerController", "Deposit", "GET", "TELLER", "CREATE")]
    [InlineData("TellerController", "Deposit", "POST", "TELLER", "CREATE")]
    [InlineData("TellerController", "Withdrawal", "POST", "TELLER", "CREATE")]
    [InlineData("TellerController", "RetailPosting", "POST", "TELLER", "CREATE")]
    [InlineData("TellerController", "MixedPosting", "POST", "TELLER", "CREATE")]
    [InlineData("TellerController", "BulkUpload", "POST", "TELLER", "CREATE")]
    [InlineData("TellerController", "PayWithdrawal", "POST", "TELLER", "CREATE")]
    [InlineData("TellerController", "ApproveWithdrawal", "POST", "TELLER", "APPROVE")]
    [InlineData("TellerController", "RejectWithdrawal", "POST", "TELLER", "APPROVE")]
    [InlineData("TellerController", "WithdrawalRequests", "GET", "TELLER", "VIEW")]
    [InlineData("LoanController", "Approve", "POST", "LOANS", "APPROVE")]
    [InlineData("LoanController", "Disburse", "GET", "LOANS", "APPROVE")]
    [InlineData("LoanController", "Disburse", "POST", "LOANS", "APPROVE")]
    [InlineData("LoanController", "Repayment", "POST", "LOANS", "EDIT")]
    [InlineData("LoanController", "ApproveRequest", "POST", "LOAN_REQ", "APPROVE")]
    [InlineData("LoanController", "RejectRequest", "POST", "LOAN_REQ", "APPROVE")]
    [InlineData("LoanController", "CreateRequest", "POST", "LOAN_REQ", "CREATE")]
    [InlineData("AccountController", "Approve", "POST", "ACCOUNTS", "APPROVE")]
    [InlineData("AccountController", "Freeze", "POST", "ACCOUNTS", "APPROVE")]
    [InlineData("AccountController", "Close", "POST", "ACCOUNTS", "APPROVE")]
    [InlineData("AccountController", "ApproveEditRequest", "POST", "ACCOUNTS", "APPROVE")]
    [InlineData("AccountController", "RejectEditRequest", "POST", "ACCOUNTS", "APPROVE")]
    [InlineData("AccountController", "BlockAmount", "GET", "ACCOUNTS", "EDIT")]
    [InlineData("AccountController", "BlockAmount", "POST", "ACCOUNTS", "EDIT")]
    [InlineData("AccountController", "BalanceEnquiry", "GET", "BAL_ENQUIRY", "VIEW")]
    [InlineData("AccountController", "LookupCustomer", "GET", "CUSTOMERS", "VIEW")]
    [InlineData("CustomerController", "Approve", "POST", "CUSTOMERS", "APPROVE")]
    [InlineData("CustomerController", "Delete", "POST", "CUSTOMERS", "DELETE")]
    [InlineData("SelfRegistrationController", "Reject", "POST", "SELF_REG", "APPROVE")]
    [InlineData("ProcessingController", "EOD", "GET", "PROCESSING", "VIEW")]
    [InlineData("ProcessingController", "EOD", "POST", "PROCESSING", "APPROVE")]
    [InlineData("ProcessingController", "EOM", "POST", "PROCESSING", "APPROVE")]
    [InlineData("ProcessingController", "EOY", "POST", "PROCESSING", "APPROVE")]
    [InlineData("ProcessingController", "SOD", "POST", "PROCESSING", "APPROVE")]
    [InlineData("UsersController", "Create", "POST", "USERS", "CREATE")]
    [InlineData("UsersController", "Delete", "POST", "USERS", "DELETE")]
    [InlineData("PermissionsController", "Save", "POST", "PERMISSIONS", "EDIT")]
    [InlineData("GLController", "Index", "GET", "GL", "VIEW")]
    [InlineData("ReportController", "TrialBalance", "GET", "REPORTS", "VIEW")]
    public void Sensitive_endpoints_need_the_permission_they_should(string controller, string action, string method, string module, string code)
    {
        var type = typeof(BaseController).Assembly.GetType("Microbanker.Web.Controllers." + controller)!;
        var m = Actions(type).Single(a => ActionName(a) == action && HttpMethodOf(a) == method);
        Assert.Equal((module, code), Needs(type, m));
    }
}

// ═════════════════════════════════════════════════════════════════════════════
//  The permission lookup
// ═════════════════════════════════════════════════════════════════════════════

public sealed class PermissionServiceTests : IDisposable
{
    private readonly SqliteConnection _conn = new("DataSource=:memory:");
    private readonly DbContextOptions<MicrobankerDbContext> _options;
    private readonly MicrobankerDbContext _db;
    private readonly Organization _org = new("Test Org", "TST");
    private readonly AppModule _customers = new("Customers", "CUSTOMERS");
    private readonly AppModule _accounts = new("Accounts", "ACCOUNTS");
    private readonly Dictionary<(string, string), AppAction> _actions = new();

    public PermissionServiceTests()
    {
        _conn.Open();
        _options = new DbContextOptionsBuilder<MicrobankerDbContext>().UseSqlite(_conn).Options;
        _db = new MicrobankerDbContext(_options);
        _db.Database.EnsureCreated();
        _db.Organizations.Add(_org);
        _db.AppModules.AddRange(_customers, _accounts);
        foreach (var m in new[] { _customers, _accounts })
            foreach (var a in new[] { "VIEW", "CREATE", "EDIT", "DELETE", "APPROVE", "EXPORT" })
            {
                var action = new AppAction(m.Id, a, a);
                _actions[(m.Code, a)] = action;
                _db.AppActions.Add(action);
            }
        _db.SaveChanges();
    }

    public void Dispose() { _db.Dispose(); _conn.Dispose(); }

    private Role NewRole(string code, bool active = true, params (string Module, string Action)[] grants)
    {
        var r = new Role(_org.Id, code, code);
        if (!active) r.Update(code, null, false);
        _db.Roles.Add(r);
        _db.SaveChanges();
        foreach (var (module, action) in grants)
        {
            var m = module == "CUSTOMERS" ? _customers : _accounts;
            _db.RolePermissions.Add(new RolePermission(r.Id, m.Id, _actions[(module, action)].Id));
        }
        _db.SaveChanges();
        return r;
    }

    private AppUser NewUser(string name, bool active = true, params Role[] roles)
    {
        var u = new AppUser(_org.Id, name, name + "@x.com", "hash", "F", "L");
        if (!active) u.Update(u.Email, "F", "L", null, false);
        _db.AppUsers.Add(u);
        _db.SaveChanges();
        foreach (var r in roles) _db.UserRoles.Add(new UserRole(u.Id, r.Id));
        _db.SaveChanges();
        return u;
    }

    private async Task<PermissionSet> For(AppUser? user)
    {
        var http = new DefaultHttpContext();
        if (user != null) http.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("userId", user.Id.ToString()) }, "test"));
        return await new PermissionService(new MicrobankerDbContext(_options), new HttpContextAccessor { HttpContext = http }).GetCurrentAsync();
    }

    [Fact]
    public async Task A_role_grants_exactly_what_it_was_given()
    {
        var u = NewUser("clerk", roles: NewRole("CLERK", true, ("CUSTOMERS", "VIEW"), ("CUSTOMERS", "CREATE")));
        var p = await For(u);
        Assert.True(p.Can("CUSTOMERS", "VIEW"));
        Assert.True(p.Can("CUSTOMERS", "CREATE"));
        Assert.False(p.Can("CUSTOMERS", "DELETE"));
        Assert.False(p.Can("ACCOUNTS", "VIEW"));
        Assert.False(p.IsSuperAdmin);
        Assert.True(p.CanAny("ACCOUNTS", "CUSTOMERS"));
        Assert.False(p.CanAny("ACCOUNTS"));
    }

    [Fact]
    public async Task Several_roles_add_up()
    {
        var u = NewUser("both", roles: new[] { NewRole("A", true, ("CUSTOMERS", "VIEW")), NewRole("B", true, ("ACCOUNTS", "EDIT")) });
        var p = await For(u);
        Assert.True(p.Can("CUSTOMERS", "VIEW"));
        Assert.True(p.Can("ACCOUNTS", "EDIT"));
        Assert.False(p.Can("ACCOUNTS", "VIEW"));
    }

    [Fact]
    public async Task The_super_administrator_role_can_do_everything()
    {
        var p = await For(NewUser("root", roles: NewRole("SUPER_ADMIN")));
        Assert.True(p.IsSuperAdmin);
        Assert.True(p.Can("ANYTHING", "AT_ALL"));
    }

    [Fact]
    public async Task An_inactive_or_deleted_role_grants_nothing_not_even_super_admin()
    {
        var inactive = NewRole("CLERK", active: false, ("CUSTOMERS", "VIEW"));
        Assert.False((await For(NewUser("u1", roles: inactive))).Can("CUSTOMERS", "VIEW"));

        var inactiveSuper = NewRole("SUPER_ADMIN", active: false);
        Assert.False((await For(NewUser("u2", roles: inactiveSuper))).IsSuperAdmin);

        var gone = NewRole("GONE", true, ("CUSTOMERS", "VIEW"));
        var u3 = NewUser("u3", roles: gone);
        Assert.True((await For(u3)).Can("CUSTOMERS", "VIEW"));
        gone.SoftDelete();
        await _db.SaveChangesAsync();
        Assert.False((await For(u3)).Can("CUSTOMERS", "VIEW"));
    }

    [Fact]
    public async Task A_withdrawn_role_assignment_and_a_revoked_grant_take_effect_at_once()
    {
        var role = NewRole("CLERK", true, ("CUSTOMERS", "VIEW"));
        var u = NewUser("clerk", roles: role);
        Assert.True((await For(u)).Can("CUSTOMERS", "VIEW"));

        var grant = await _db.RolePermissions.SingleAsync();
        grant.SetGranted(false);
        await _db.SaveChangesAsync();
        Assert.False((await For(u)).Can("CUSTOMERS", "VIEW"));          // no re-login needed

        grant.SetGranted(true);
        _db.UserRoles.RemoveRange(_db.UserRoles);
        await _db.SaveChangesAsync();
        Assert.False((await For(u)).Can("CUSTOMERS", "VIEW"));
    }

    [Fact]
    public async Task A_deactivated_user_loses_everything_even_with_a_valid_cookie()
    {
        var super = NewUser("root", roles: NewRole("SUPER_ADMIN"));
        Assert.True((await For(super)).IsSuperAdmin);

        await _db.Database.ExecuteSqlRawAsync("UPDATE AppUsers SET IsActive = 0");
        var p = await For(super);
        Assert.False(p.IsSuperAdmin);
        Assert.False(p.Can("CUSTOMERS", "VIEW"));

        await _db.Database.ExecuteSqlRawAsync("UPDATE AppUsers SET IsActive = 1, IsDeleted = 1");
        Assert.False((await For(super)).IsSuperAdmin);
    }

    [Fact]
    public async Task Anonymous_and_unknown_users_have_no_permissions()
    {
        Assert.False((await For(null)).Can("CUSTOMERS", "VIEW"));
        var http = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("userId", Guid.NewGuid().ToString()) }, "test")) };
        var p = await new PermissionService(new MicrobankerDbContext(_options), new HttpContextAccessor { HttpContext = http }).GetCurrentAsync();
        Assert.False(p.Can("CUSTOMERS", "VIEW"));
    }
}

// ═════════════════════════════════════════════════════════════════════════════
//  Closing the privilege-escalation routes in Users and Permissions
// ═════════════════════════════════════════════════════════════════════════════

public sealed class PermissionEscalationTests : IDisposable
{
    private readonly SqliteConnection _conn = new("DataSource=:memory:");
    private readonly DbContextOptions<MicrobankerDbContext> _options;
    private readonly MicrobankerDbContext _db;
    private readonly IEncryptionService _enc;
    private readonly Organization _org = new("Test Org", "TST");
    private readonly AppModule _customers = new("Customers", "CUSTOMERS");
    private readonly AppModule _accounts = new("Accounts", "ACCOUNTS");
    private readonly Dictionary<(string, string), AppAction> _actions = new();
    private readonly Role _superRole, _managerRole, _clerkRole, _tellerRole;
    private readonly AppUser _superUser, _manager, _clerk;

    private sealed class FakePermissions : IPermissionService
    {
        private readonly PermissionSet _set;
        public FakePermissions(PermissionSet set) => _set = set;
        public Task<PermissionSet> GetCurrentAsync() => Task.FromResult(_set);
    }

    public PermissionEscalationTests()
    {
        _conn.Open();
        _options = new DbContextOptionsBuilder<MicrobankerDbContext>().UseSqlite(_conn).Options;
        _db = new MicrobankerDbContext(_options);
        _db.Database.EnsureCreated();
        _enc = new AesEncryptionService(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["EncryptionKey"] = "unit-test-key-0123456789-abcdefgh" }).Build());

        _db.Organizations.Add(_org);
        _db.AppModules.AddRange(_customers, _accounts);
        foreach (var m in new[] { _customers, _accounts })
            foreach (var a in new[] { "VIEW", "CREATE", "EDIT", "DELETE", "APPROVE", "EXPORT" })
            {
                var action = new AppAction(m.Id, a, a);
                _actions[(m.Code, a)] = action;
                _db.AppActions.Add(action);
            }
        _superRole = new Role(_org.Id, "Super", "SUPER_ADMIN", null, true);
        _managerRole = new Role(_org.Id, "Manager", "MANAGER");
        _clerkRole = new Role(_org.Id, "Clerk", "CLERK");
        _tellerRole = new Role(_org.Id, "Teller", "TELLER");
        _db.Roles.AddRange(_superRole, _managerRole, _clerkRole, _tellerRole);
        _db.SaveChanges();

        void Grant(Role r, string module, string action) =>
            _db.RolePermissions.Add(new RolePermission(r.Id, (module == "CUSTOMERS" ? _customers : _accounts).Id, _actions[(module, action)].Id));
        Grant(_managerRole, "CUSTOMERS", "VIEW"); Grant(_managerRole, "CUSTOMERS", "CREATE"); Grant(_managerRole, "ACCOUNTS", "VIEW");
        Grant(_clerkRole, "CUSTOMERS", "VIEW");
        Grant(_tellerRole, "ACCOUNTS", "VIEW"); Grant(_tellerRole, "ACCOUNTS", "APPROVE");     // APPROVE is not held by the manager
        _db.SaveChanges();

        _superUser = AddUser("root", _superRole);
        _manager = AddUser("manager", _managerRole);
        _clerk = AddUser("clerk", _clerkRole);
    }

    public void Dispose() { _db.Dispose(); _conn.Dispose(); }

    private AppUser AddUser(string name, Role role)
    {
        var u = new AppUser(_org.Id, name, name + "@x.com", "hash", "F", "L");
        _db.AppUsers.Add(u);
        _db.SaveChanges();
        _db.UserRoles.Add(new UserRole(u.Id, role.Id));
        _db.SaveChanges();
        return u;
    }

    private static PermissionSet Held(params string[] p) => new(false, p.ToHashSet());
    private string Id(Guid id) => _enc.Encrypt(id.ToString());

    private T Wire<T>(T c, Guid userId) where T : Controller
    {
        var http = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("companyId", _org.Id.ToString()), new Claim("userId", userId.ToString())
            }, "test"))
        };
        c.ControllerContext = new ControllerContext(new ActionContext(http, new RouteData(), new ControllerActionDescriptor()));
        c.TempData = new TempDataDictionary(http, new NullTempData());
        return c;
    }

    private UsersController Users(AppUser caller, PermissionSet perms) =>
        Wire(new UsersController(_db, _enc, new FakePermissions(perms)), caller.Id);
    private PermissionsController PermissionsFor(AppUser caller, PermissionSet perms) =>
        Wire(new PermissionsController(_db, _enc, new FakePermissions(perms)), caller.Id);

    private static UserFormModel Form(string name, Guid? roleId) => new()
    {
        Username = name, Email = name + "@x.com", FirstName = "F", LastName = "L", Password = "Password1!", RoleId = roleId, IsActive = true
    };

    private static PermissionSet SuperSet => new(true, new HashSet<string>());
    private static PermissionSet ManagerSet => Held("USERS:VIEW", "USERS:CREATE", "USERS:EDIT", "USERS:DELETE", "CUSTOMERS:VIEW", "CUSTOMERS:CREATE", "ACCOUNTS:VIEW");

    // ── Users ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Someone_who_can_create_users_cannot_make_a_super_administrator()
    {
        var c = Users(_manager, ManagerSet);
        Assert.IsType<ViewResult>(await c.Create(Form("evil", _superRole.Id)));
        Assert.True(c.ModelState.ContainsKey("RoleId"));
        Assert.False(await _db.AppUsers.AnyAsync(u => u.Username == "evil"));
    }

    [Fact]
    public async Task A_super_administrator_can_assign_any_role_including_super_administrator()
    {
        Assert.IsType<RedirectToActionResult>(await Users(_superUser, SuperSet).Create(Form("deputy", _superRole.Id)));
        var user = await _db.AppUsers.SingleAsync(u => u.Username == "deputy");
        Assert.True(await _db.UserRoles.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == _superRole.Id));
    }

    [Fact]
    public async Task A_role_can_only_be_assigned_if_you_hold_everything_it_grants()
    {
        var c = Users(_manager, ManagerSet);
        Assert.IsType<ViewResult>(await c.Create(Form("t1", _tellerRole.Id)));              // teller has ACCOUNTS:APPROVE, manager does not
        Assert.True(c.ModelState.ContainsKey("RoleId"));

        Assert.IsType<RedirectToActionResult>(await Users(_manager, ManagerSet).Create(Form("c1", _clerkRole.Id)));   // a subset: fine
        Assert.IsType<RedirectToActionResult>(await Users(_manager, ManagerSet).Create(Form("m1", _managerRole.Id)));  // an equal set: fine
    }

    [Fact]
    public async Task A_role_that_does_not_exist_or_belongs_to_nobody_here_is_refused()
    {
        var c = Users(_superUser, SuperSet);
        Assert.IsType<ViewResult>(await c.Create(Form("ghost", Guid.NewGuid())));
        Assert.True(c.ModelState.ContainsKey("RoleId"));

        var otherOrg = new Organization("Other", "OTH");
        _db.Organizations.Add(otherOrg);
        var foreign = new Role(otherOrg.Id, "Foreign", "FOREIGN");
        _db.Roles.Add(foreign);
        await _db.SaveChangesAsync();
        Assert.IsType<ViewResult>(await Users(_superUser, SuperSet).Create(Form("alien", foreign.Id)));
    }

    [Fact]
    public async Task Editing_cannot_be_used_to_promote_yourself()
    {
        var c = Users(_clerk, Held("USERS:VIEW", "USERS:EDIT", "CUSTOMERS:VIEW"));
        var result = await c.Edit(Id(_clerk.Id), Form("clerk", _superRole.Id));
        Assert.IsType<ViewResult>(result);
        Assert.False(await _db.UserRoles.AnyAsync(ur => ur.UserId == _clerk.Id && ur.RoleId == _superRole.Id && !ur.IsDeleted));
        Assert.True(await _db.UserRoles.AnyAsync(ur => ur.UserId == _clerk.Id && ur.RoleId == _clerkRole.Id));
    }

    [Fact]
    public async Task Only_a_super_administrator_can_change_or_delete_a_super_administrator()
    {
        var edit = Users(_manager, ManagerSet);
        var form = Form("root", _clerkRole.Id);                                              // demote them, or reset the password
        form.Password = "Hacked123!";
        Assert.IsType<RedirectToActionResult>(await edit.Edit(Id(_superUser.Id), form));
        Assert.True(await _db.UserRoles.AnyAsync(ur => ur.UserId == _superUser.Id && ur.RoleId == _superRole.Id && !ur.IsDeleted));
        Assert.Equal("hash", (await new MicrobankerDbContext(_options).AppUsers.AsNoTracking().SingleAsync(u => u.Id == _superUser.Id)).PasswordHash);

        await Users(_manager, ManagerSet).Delete(Id(_superUser.Id));
        Assert.False((await new MicrobankerDbContext(_options).AppUsers.AsNoTracking().SingleAsync(u => u.Id == _superUser.Id)).IsDeleted);

        // A super administrator may.
        await Users(_superUser, SuperSet).Delete(Id(_clerk.Id));
        Assert.True((await new MicrobankerDbContext(_options).AppUsers.AsNoTracking().SingleAsync(u => u.Id == _clerk.Id)).IsDeleted);
    }

    [Fact]
    public async Task Ordinary_edits_of_ordinary_users_still_work()
    {
        var form = Form("clerk", _clerkRole.Id);
        form.FirstName = "Renamed";
        Assert.IsType<RedirectToActionResult>(await Users(_manager, ManagerSet).Edit(Id(_clerk.Id), form));
        Assert.Equal("Renamed", (await new MicrobankerDbContext(_options).AppUsers.AsNoTracking().SingleAsync(u => u.Id == _clerk.Id)).FirstName);
    }

    // ── Permissions ───────────────────────────────────────────────────────────

    private string Cell(string module, string action) =>
        $"{(module == "CUSTOMERS" ? _customers : _accounts).Id}|{_actions[(module, action)].Id}";

    private async Task<List<string>> Grants(Role r)
    {
        var db = new MicrobankerDbContext(_options);
        return await db.RolePermissions.Where(rp => rp.RoleId == r.Id).Select(rp => rp.AppModule.Code + ":" + rp.AppAction.Code).OrderBy(x => x).ToListAsync();
    }

    [Fact]
    public async Task Nobody_can_grant_a_permission_they_do_not_hold()
    {
        var c = PermissionsFor(_manager, Held("PERMISSIONS:VIEW", "PERMISSIONS:EDIT", "CUSTOMERS:VIEW", "CUSTOMERS:CREATE", "ACCOUNTS:VIEW"));
        var before = await Grants(_clerkRole);

        var result = await c.Save(new SavePermissionsModel
        {
            EncryptedRoleId = Id(_clerkRole.Id),
            Granted = new List<string> { Cell("CUSTOMERS", "VIEW"), Cell("CUSTOMERS", "DELETE"), Cell("ACCOUNTS", "APPROVE") }   // the last two are not held
        });
        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(before, await Grants(_clerkRole));                        // nothing changed
    }

    [Fact]
    public async Task Granting_what_you_hold_and_revoking_anything_is_allowed()
    {
        var c = PermissionsFor(_manager, Held("PERMISSIONS:EDIT", "CUSTOMERS:VIEW", "CUSTOMERS:CREATE", "ACCOUNTS:VIEW"));
        await c.Save(new SavePermissionsModel
        {
            EncryptedRoleId = Id(_clerkRole.Id),
            Granted = new List<string> { Cell("CUSTOMERS", "VIEW"), Cell("CUSTOMERS", "CREATE"), Cell("ACCOUNTS", "VIEW") }
        });
        Assert.Equal(new[] { "ACCOUNTS:VIEW", "CUSTOMERS:CREATE", "CUSTOMERS:VIEW" }, await Grants(_clerkRole));

        // Revoke everything: allowed even though the manager holds none of the teller role's extra grants.
        await PermissionsFor(_manager, Held("PERMISSIONS:EDIT", "CUSTOMERS:VIEW")).Save(new SavePermissionsModel { EncryptedRoleId = Id(_tellerRole.Id), Granted = new List<string>() });
        Assert.Empty(await Grants(_tellerRole));
    }

    [Fact]
    public async Task Saving_a_role_that_already_has_a_permission_you_lack_does_not_trip_the_check()
    {
        // The teller role holds ACCOUNTS:APPROVE. A manager (who lacks it) re-saves the form unchanged: not a new grant.
        var c = PermissionsFor(_manager, Held("PERMISSIONS:EDIT", "ACCOUNTS:VIEW"));
        await c.Save(new SavePermissionsModel
        {
            EncryptedRoleId = Id(_tellerRole.Id),
            Granted = new List<string> { Cell("ACCOUNTS", "VIEW"), Cell("ACCOUNTS", "APPROVE") }
        });
        Assert.Equal(new[] { "ACCOUNTS:APPROVE", "ACCOUNTS:VIEW" }, await Grants(_tellerRole));
    }

    [Fact]
    public async Task A_super_administrator_can_grant_anything_and_junk_entries_are_ignored()
    {
        var c = PermissionsFor(_superUser, SuperSet);
        await c.Save(new SavePermissionsModel
        {
            EncryptedRoleId = Id(_clerkRole.Id),
            Granted = new List<string> { Cell("ACCOUNTS", "APPROVE"), "not-a-pair", $"{Guid.NewGuid()}|{Guid.NewGuid()}", $"{_customers.Id}|{_actions[("ACCOUNTS", "VIEW")].Id}" }
        });
        // Only the real pair counts; an action that belongs to a different module is not a valid cell.
        Assert.Equal(new[] { "ACCOUNTS:APPROVE" }, await Grants(_clerkRole));
    }

    [Fact]
    public async Task A_tampered_role_id_is_not_found()
    {
        Assert.IsType<NotFoundResult>(await PermissionsFor(_superUser, SuperSet).Save(new SavePermissionsModel { EncryptedRoleId = "junk" }));
    }

    private sealed class NullTempData : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
