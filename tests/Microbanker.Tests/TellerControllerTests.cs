using System.Security.Claims;
using System.Text;
using Microbanker.Domain.Entities.Accounts;
using Microbanker.Domain.Entities.Customers;
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

/// <summary>The teller and transaction screens (controllers + real posting service) against in-memory SQLite.</summary>
public sealed class TellerControllerTests : IDisposable
{
    private readonly SqliteConnection _conn = new("DataSource=:memory:");
    private readonly DbContextOptions<MicrobankerDbContext> _options;
    private readonly MicrobankerDbContext _db;
    private readonly IEncryptionService _enc;
    private readonly Organization _org = new("Test Org", "TST");
    private readonly Branch _branch;
    private readonly ProductDefinition _product;
    private readonly TransactionCode _wdl, _dep, _bigWdl;
    private readonly MemberAccount _a1, _a2;
    private readonly Guid _user = Guid.NewGuid();
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    public TellerControllerTests()
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

        _wdl = new TransactionCode(_org.Id, "WDL", "Cash withdrawal", "DR");
        _dep = new TransactionCode(_org.Id, "DEP", "Cash deposit", "CR");
        _bigWdl = new TransactionCode(_org.Id, "BIGW", "Large withdrawal", "DR");
        _bigWdl.Update("BIGW", "Large withdrawal", "DR", null, true, true);
        _db.TransactionCodes.AddRange(_wdl, _dep, _bigWdl);
        _db.GLAccounts.Add(new GLAccount(_org.Id, "1000", "Cash", "ASSET", null, 1));

        _a1 = Account("MN-SA-0000001", 1000m);
        _a2 = Account("MN-SA-0000002", 50m);
        _db.SaveChanges();
    }

    public void Dispose() { _db.Dispose(); _conn.Dispose(); }

    private MemberAccount Account(string no, decimal balance)
    {
        var cust = new Customer(_org.Id, _branch.Id, "C-" + no, "INDIVIDUAL", "A", "B");
        cust.Update(new CustomerProfile { BranchId = _branch.Id, FirstName = "A", LastName = "B" });
        _db.Customers.Add(cust);
        var a = new MemberAccount(_org.Id, _branch.Id, cust.Id, _product.Id, no, "Acc " + no, "SAVINGS");
        a.UpdateTerms(_product.Id, "GMD", balance, new DateOnly(2026, 1, 1));
        a.Approve(Guid.NewGuid());
        _db.MemberAccounts.Add(a);
        return a;
    }

    private T Wire<T>(T c, Guid? user = null) where T : Controller
    {
        var http = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim("companyId", _org.Id.ToString()), new Claim("userId", (user ?? _user).ToString())
            }, "test"))
        };
        c.ControllerContext = new ControllerContext(new ActionContext(http, new RouteData(), new ControllerActionDescriptor()));
        c.TempData = new TempDataDictionary(http, new NullTempData());
        return c;
    }

    private TellerController Teller(Guid? user = null, MicrobankerDbContext? db = null)
    {
        var d = db ?? _db;
        return Wire(new TellerController(d, _enc, new PostingService(d)), user);
    }

    private TransactionController Txns(MicrobankerDbContext? db = null)
    {
        var d = db ?? _db;
        return Wire(new TransactionController(d, _enc, new PostingService(d)));
    }

    private string Id(Guid id) => _enc.Encrypt(id.ToString());
    private async Task<WithdrawalRequest> FreshRequest(Guid id) => await new MicrobankerDbContext(_options).WithdrawalRequests.AsNoTracking().SingleAsync(x => x.Id == id);
    private async Task<MemberAccount> Fresh(MemberAccount a) => await new MicrobankerDbContext(_options).MemberAccounts.AsNoTracking().SingleAsync(x => x.Id == a.Id);

    private CashPostingFormModel Cash(string account, Guid code, decimal amount) => new()
    {
        AccountNo = account, TransactionCodeId = code, Amount = amount, Narration = "Counter", CashGlAccountNo = "1000"
    };

    private static IFormFile Csv(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "File", "payments.csv");
    }

    // ── Deposit / withdrawal ──────────────────────────────────────────────────

    [Fact]
    public async Task Deposit_posts_and_redirects_to_the_receipt()
    {
        var result = await Teller().Deposit(Cash(_a1.AccountNo.ToLowerInvariant(), _dep.Id, 250m));
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Transaction", redirect.ControllerName);
        Assert.Equal("View", redirect.ActionName);
        Assert.Equal(1250m, (await Fresh(_a1)).BookBalance);

        var receipt = Assert.IsType<ViewResult>(await Txns().Details((string)redirect.RouteValues!["id"]!));
        var vm = Assert.IsType<TransactionDetailViewModel>(receipt.Model);
        Assert.Equal(250m, vm.Txn.Amount);
        Assert.Equal("1000", vm.GLAccountNo);
        Assert.Equal("Cash", vm.GLAccountName);
        Assert.True(vm.CanReverse);
    }

    [Fact]
    public async Task A_failed_deposit_stays_on_the_form_with_the_reason()
    {
        var c = Teller();
        var result = await c.Deposit(Cash("NOPE", _dep.Id, 10m));
        Assert.IsType<ViewResult>(result);
        Assert.False(c.ModelState.IsValid);
        Assert.Empty(_db.Transactions);

        var wrongCode = Teller();
        Assert.IsType<ViewResult>(await wrongCode.Deposit(Cash(_a1.AccountNo, _wdl.Id, 10m)));   // a debit code for a deposit
        Assert.Empty(_db.Transactions);
    }

    [Fact]
    public async Task Withdrawal_debits_and_refuses_to_overdraw()
    {
        Assert.IsType<RedirectToActionResult>(await Teller().Withdrawal(Cash(_a1.AccountNo, _wdl.Id, 300m)));
        Assert.Equal(700m, (await Fresh(_a1)).BookBalance);

        var c = Teller();
        Assert.IsType<ViewResult>(await c.Withdrawal(Cash(_a2.AccountNo, _wdl.Id, 60m)));        // only 50
        Assert.Equal(50m, (await Fresh(_a2)).BookBalance);
    }

    // ── Withdrawal requests (approval) ────────────────────────────────────────

    [Fact]
    public async Task A_code_that_needs_approval_creates_a_request_instead_of_posting()
    {
        var result = await Teller().Withdrawal(Cash(_a1.AccountNo, _bigWdl.Id, 400m));
        Assert.Equal("WithdrawalRequests", Assert.IsType<RedirectToActionResult>(result).ActionName);
        Assert.Empty(_db.Transactions);
        Assert.Equal(1000m, (await Fresh(_a1)).BookBalance);
        var req = await _db.WithdrawalRequests.SingleAsync();
        Assert.Equal("PENDING", req.Status);
        Assert.Equal(400m, req.Amount);
    }

    [Fact]
    public async Task A_request_needs_a_different_approver_then_the_teller_pays_it_once()
    {
        await Teller().Withdrawal(Cash(_a1.AccountNo, _bigWdl.Id, 400m));
        var req = await _db.WithdrawalRequests.SingleAsync();

        await Teller().ApproveWithdrawal(Id(req.Id), null);                   // the requester cannot approve their own
        Assert.Equal("PENDING", (await FreshRequest(req.Id)).Status);

        var approver = Guid.NewGuid();
        await Teller(approver).ApproveWithdrawal(Id(req.Id), "ok");
        var approved = await FreshRequest(req.Id);
        Assert.Equal("APPROVED", approved.Status);
        Assert.Equal(approver, approved.ApprovedBy);

        await Teller().PayWithdrawal(Id(req.Id), null);                       // no cash account chosen
        Assert.Empty(_db.Transactions);

        var paid = await Teller().PayWithdrawal(Id(req.Id), "1000");
        Assert.Equal("View", Assert.IsType<RedirectToActionResult>(paid).ActionName);
        Assert.Equal(600m, (await Fresh(_a1)).BookBalance);
        Assert.Equal("COMPLETED", (await FreshRequest(req.Id)).Status);

        await Teller().PayWithdrawal(Id(req.Id), "1000");                     // not twice
        Assert.Equal(600m, (await Fresh(_a1)).BookBalance);
        Assert.Single(_db.Transactions);
    }

    [Fact]
    public async Task Rejecting_a_request_needs_a_reason_and_a_rejected_request_cannot_be_paid()
    {
        await Teller().Withdrawal(Cash(_a1.AccountNo, _bigWdl.Id, 400m));
        var req = await _db.WithdrawalRequests.SingleAsync();

        await Teller(Guid.NewGuid()).RejectWithdrawal(Id(req.Id), " ");
        Assert.Equal("PENDING", (await FreshRequest(req.Id)).Status);

        await Teller(Guid.NewGuid()).RejectWithdrawal(Id(req.Id), "No funds source");
        Assert.Equal("REJECTED", (await FreshRequest(req.Id)).Status);

        await Teller().PayWithdrawal(Id(req.Id), "1000");
        Assert.Empty(_db.Transactions);
    }

    [Fact]
    public async Task A_request_for_more_than_the_balance_is_refused_up_front()
    {
        var c = Teller();
        Assert.IsType<ViewResult>(await c.Withdrawal(Cash(_a2.AccountNo, _bigWdl.Id, 500m)));
        Assert.Empty(_db.WithdrawalRequests);
    }

    // ── Retail / mixed / bulk ─────────────────────────────────────────────────

    [Fact]
    public async Task Retail_posting_transfers_between_accounts()
    {
        var result = await Teller().RetailPosting(new RetailPostingFormModel
        {
            FromAccountNo = _a1.AccountNo, ToAccountNo = _a2.AccountNo, DebitCodeId = _wdl.Id, CreditCodeId = _dep.Id,
            Amount = 200m, Narration = "Rent"
        });
        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(800m, (await Fresh(_a1)).BookBalance);
        Assert.Equal(250m, (await Fresh(_a2)).BookBalance);
        Assert.Equal(2, await _db.Transactions.CountAsync());
        Assert.Single(await _db.Transactions.Select(t => t.BatchRef).Distinct().ToListAsync());
    }

    [Fact]
    public async Task Retail_posting_to_the_same_account_or_over_the_balance_is_refused()
    {
        var same = Teller();
        Assert.IsType<ViewResult>(await same.RetailPosting(new RetailPostingFormModel
        {
            FromAccountNo = _a1.AccountNo, ToAccountNo = _a1.AccountNo, DebitCodeId = _wdl.Id, CreditCodeId = _dep.Id, Amount = 1m, Narration = "x"
        }));
        var over = Teller();
        Assert.IsType<ViewResult>(await over.RetailPosting(new RetailPostingFormModel
        {
            FromAccountNo = _a2.AccountNo, ToAccountNo = _a1.AccountNo, DebitCodeId = _wdl.Id, CreditCodeId = _dep.Id, Amount = 51m, Narration = "x"
        }));
        Assert.Empty(_db.Transactions);
    }

    [Fact]
    public async Task Mixed_posting_ignores_blank_rows_and_posts_a_balanced_set()
    {
        var model = new MixedPostingFormModel
        {
            DebitCodeId = _wdl.Id, CreditCodeId = _dep.Id, Narration = "Settlement",
            Lines =
            {
                new PostingLineModel { AccountNo = _a1.AccountNo, DrCr = "DR", Amount = 100m },
                new PostingLineModel { AccountNo = _a2.AccountNo, DrCr = "CR", Amount = 100m, Narration = "Own narration" },
                new PostingLineModel()
            }
        };
        Assert.IsType<RedirectToActionResult>(await Teller().MixedPosting(model));
        Assert.Equal(900m, (await Fresh(_a1)).BookBalance);
        Assert.Equal(150m, (await Fresh(_a2)).BookBalance);
        var narrations = await _db.Transactions.OrderBy(t => t.TransactionRef).Select(t => t.Narration).ToListAsync();
        Assert.Equal(new[] { "Settlement", "Own narration" }, narrations);
    }

    [Fact]
    public async Task Mixed_posting_that_does_not_balance_posts_nothing()
    {
        var c = Teller();
        var model = new MixedPostingFormModel
        {
            DebitCodeId = _wdl.Id, CreditCodeId = _dep.Id, Narration = "x",
            Lines = { new PostingLineModel { AccountNo = _a1.AccountNo, DrCr = "DR", Amount = 100m }, new PostingLineModel { AccountNo = _a2.AccountNo, DrCr = "CR", Amount = 99m } }
        };
        var result = await c.MixedPosting(model);
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(2, ((MixedPostingFormModel)view.Model!).Lines.Count);                 // the lines are kept for correction
        Assert.Empty(_db.Transactions);
    }

    [Fact]
    public async Task A_one_sided_mixed_posting_goes_against_the_contra_account()
    {
        var model = new MixedPostingFormModel
        {
            CreditCodeId = _dep.Id, ContraGlAccountNo = "1000", Narration = "Cash in",
            Lines = { new PostingLineModel { AccountNo = _a1.AccountNo, DrCr = "CR", Amount = 10m }, new PostingLineModel { AccountNo = _a2.AccountNo, DrCr = "CR", Amount = 20m } }
        };
        Assert.IsType<RedirectToActionResult>(await Teller().MixedPosting(model));
        Assert.All(_db.Transactions, t => Assert.Equal("1000", t.GLAccountNo));
    }

    [Fact]
    public async Task Bulk_upload_posts_a_balanced_file_atomically()
    {
        var file = Csv($"AccountNo,DrCr,Amount,Narration\n{_a1.AccountNo},DR,100,Payroll\n{_a2.AccountNo},CR,100,\n");
        var result = await Teller().BulkUpload(new BulkUploadFormModel { File = file, DebitCodeId = _wdl.Id, CreditCodeId = _dep.Id, Narration = "Bulk" });
        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(900m, (await Fresh(_a1)).BookBalance);
        Assert.Equal(150m, (await Fresh(_a2)).BookBalance);
        Assert.Equal(new[] { "Payroll", "Bulk" }, await _db.Transactions.OrderBy(t => t.TransactionRef).Select(t => t.Narration).ToListAsync());
    }

    [Fact]
    public async Task Bulk_upload_with_a_bad_row_or_a_failing_line_posts_nothing()
    {
        var c = Teller();
        var bad = Csv($"{_a1.AccountNo},CR,10\n{_a2.AccountNo},CR,notanumber\n");
        var view = Assert.IsType<ViewResult>(await c.BulkUpload(new BulkUploadFormModel { File = bad, CreditCodeId = _dep.Id, ContraGlAccountNo = "1000", Narration = "x" }));
        Assert.Single(((BulkUploadFormModel)view.Model!).FileErrors);

        // The second line cannot be funded, so the first must not post either.
        var failing = Csv($"{_a1.AccountNo},CR,10\n{_a2.AccountNo},DR,500\n");
        Assert.IsType<ViewResult>(await Teller().BulkUpload(new BulkUploadFormModel { File = failing, DebitCodeId = _wdl.Id, CreditCodeId = _dep.Id, Narration = "x" }));
        Assert.Empty(_db.Transactions);
        Assert.Equal(1000m, (await Fresh(_a1)).BookBalance);

        var none = Teller();
        Assert.IsType<ViewResult>(await none.BulkUpload(new BulkUploadFormModel { DebitCodeId = _wdl.Id }));
        Assert.True(none.ModelState.ContainsKey("File"));
    }

    [Fact]
    public async Task Account_lookup_returns_the_balance_or_not_found()
    {
        var found = Assert.IsType<JsonResult>(await Teller().AccountLookup(_a1.AccountNo.ToLowerInvariant()));
        var json = System.Text.Json.JsonSerializer.Serialize(found.Value);
        Assert.Contains("\"found\":true", json);
        Assert.Contains("\"available\":1000", json);
        Assert.Contains("\"found\":false", System.Text.Json.JsonSerializer.Serialize(((JsonResult)await Teller().AccountLookup("NOPE")).Value));
    }

    // ── Transactions: search, receipt, reverse ────────────────────────────────

    [Fact]
    public async Task Reversing_from_the_receipt_page_offsets_the_posting_and_shows_the_link()
    {
        await Teller().Deposit(Cash(_a1.AccountNo, _dep.Id, 100m));
        var original = await _db.Transactions.SingleAsync();

        Assert.IsType<RedirectToActionResult>(await Txns().Reverse(Id(original.Id), "Wrong slip"));
        Assert.Equal(1000m, (await Fresh(_a1)).BookBalance);

        await Txns().Reverse(Id(original.Id), "again");                       // second attempt changes nothing
        Assert.Equal(2, await new MicrobankerDbContext(_options).Transactions.CountAsync());

        var vm = (TransactionDetailViewModel)((ViewResult)await Txns().Details(Id(original.Id))).Model!;
        Assert.Equal("REVERSED", vm.Txn.Status);
        Assert.False(vm.CanReverse);
        Assert.NotNull(vm.ReversalEncryptedId);
        Assert.Equal("Wrong slip", vm.ReversalReason);

        var rev = await new MicrobankerDbContext(_options).Transactions.SingleAsync(t => t.IsReversal);
        var revVm = (TransactionDetailViewModel)((ViewResult)await Txns().Details(Id(rev.Id))).Model!;
        Assert.False(revVm.CanReverse);
        Assert.Equal(original.TransactionRef, revVm.ReversalRef);
    }

    [Fact]
    public async Task A_batch_receipt_lists_every_leg()
    {
        await Teller().RetailPosting(new RetailPostingFormModel
        {
            FromAccountNo = _a1.AccountNo, ToAccountNo = _a2.AccountNo, DebitCodeId = _wdl.Id, CreditCodeId = _dep.Id, Amount = 5m, Narration = "x"
        });
        var first = await _db.Transactions.OrderBy(t => t.TransactionRef).FirstAsync();
        var vm = (TransactionDetailViewModel)((ViewResult)await Txns().Details(Id(first.Id))).Model!;
        Assert.Equal(2, vm.BatchLegs.Count);
        Assert.NotNull(vm.BatchRef);
    }

    [Fact]
    public async Task Search_filters_by_text_status_direction_and_dates_and_totals_only_standing_money()
    {
        await Teller().Deposit(Cash(_a1.AccountNo, _dep.Id, 100m));
        await Teller().Withdrawal(Cash(_a1.AccountNo, _wdl.Id, 30m));
        var dep = await _db.Transactions.SingleAsync(t => t.DrCr == "CR");
        await Txns().Reverse(Id(dep.Id), "x");                                // deposit reversed: 3 rows now

        var all = (TransactionListViewModel)((ViewResult)await Txns().Index(null, null, null, null, null)).Model!;
        Assert.Equal(3, all.TotalCount);
        Assert.Equal(30m, all.TotalDebits);
        Assert.Equal(0m, all.TotalCredits);                                   // the reversed deposit and its reversal do not count

        Assert.Single(((TransactionListViewModel)((ViewResult)await Txns().Index(null, "REVERSED", null, null, null)).Model!).Rows);
        Assert.Equal(2, ((TransactionListViewModel)((ViewResult)await Txns().Index(null, null, "DR", null, null)).Model!).Rows.Count);   // withdrawal + the reversal's debit
        Assert.Equal(3, ((TransactionListViewModel)((ViewResult)await Txns().Index(_a1.AccountNo, null, null, null, null)).Model!).Rows.Count);
        Assert.Empty(((TransactionListViewModel)((ViewResult)await Txns().Index("NO-SUCH", null, null, null, null)).Model!).Rows);
        Assert.Empty(((TransactionListViewModel)((ViewResult)await Txns().Index(null, null, null, Today.AddDays(1), null)).Model!).Rows);
        Assert.Equal(3, ((TransactionListViewModel)((ViewResult)await Txns().Index(null, null, null, Today, Today)).Model!).Rows.Count);
    }

    [Fact]
    public async Task Tampered_ids_and_other_organisations_are_not_found()
    {
        Assert.IsType<NotFoundResult>(await Txns().Details("%%%"));
        Assert.IsType<NotFoundResult>(await Txns().Reverse("junk", "x"));
        Assert.IsType<NotFoundResult>(await Teller().ApproveWithdrawal("junk", null));

        var other = new Organization("Other", "OTH");
        _db.Organizations.Add(other);
        var foreign = new Transaction(other.Id, _branch.Id, "TXN-20260101-000001", _dep.Id, _a1.Id, "SAVINGS", "CR", 5m, "x", new DateOnly(2026, 1, 1), _user);
        _db.Transactions.Add(foreign);
        await _db.SaveChangesAsync();
        Assert.IsType<NotFoundResult>(await Txns().Details(Id(foreign.Id)));
    }

    // ── Mini statement (Account) ──────────────────────────────────────────────

    [Fact]
    public async Task The_mini_statement_lists_recent_transactions_newest_first()
    {
        await Teller().Deposit(Cash(_a1.AccountNo, _dep.Id, 100m));
        await Teller().Withdrawal(Cash(_a1.AccountNo, _wdl.Id, 30m));

        var accounts = Wire(new AccountController(_db, _enc));
        var vm = (MiniStatementViewModel)((ViewResult)await accounts.MiniStatement(Id(_a1.Id))).Model!;
        Assert.Equal(2, vm.Lines.Count);
        Assert.Equal("DR", vm.Lines[0].DrCr);
        Assert.Equal(1070m, vm.Lines[0].BalanceAfter);
        Assert.Equal(1100m, vm.Lines[1].BalanceAfter);
    }

    private sealed class NullTempData : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
