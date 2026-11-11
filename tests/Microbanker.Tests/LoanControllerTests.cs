using System.Security.Claims;
using Microbanker.Domain.Entities.Accounts;
using Microbanker.Domain.Entities.Customers;
using Microbanker.Domain.Entities.Loans;
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

/// <summary>Runs the real LoanController against in-memory SQLite (no SQL Server needed).</summary>
public sealed class LoanControllerTests : IDisposable
{
    private readonly SqliteConnection _conn = new("DataSource=:memory:");
    private readonly DbContextOptions<MicrobankerDbContext> _options;
    private readonly MicrobankerDbContext _db;
    private readonly IEncryptionService _enc;
    private readonly Organization _org = new("Test Org", "TST");
    private readonly Branch _branch;
    private readonly Customer _customer;
    private readonly Customer _other;
    private readonly ProductDefinition _product;
    private readonly MemberAccount _savings;
    private readonly MemberAccount _othersSavings;
    private readonly Guid _userId = Guid.NewGuid();

    public LoanControllerTests()
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

        _customer = ActiveCustomer("CUST-2026-000001", "Awa", "Jallow");
        _other = ActiveCustomer("CUST-2026-000002", "Bakary", "Ceesay");

        var type = new ProductType(_org.Id, "Loans", "LON", null);
        _db.ProductTypes.Add(type);
        _product = new ProductDefinition(_org.Id, type.Id, "Business Loan", "BL");
        _db.ProductDefinitions.Add(_product);

        _savings = ActiveAccount(_customer, "MN-SA-0000001");
        _othersSavings = ActiveAccount(_other, "MN-SA-0000002");
        _db.SaveChanges();
    }

    public void Dispose() { _db.Dispose(); _conn.Dispose(); }

    private Customer ActiveCustomer(string no, string first, string last)
    {
        var c = new Customer(_org.Id, _branch.Id, no, "INDIVIDUAL", first, last);
        c.Update(new CustomerProfile { BranchId = _branch.Id, FirstName = first, LastName = last });
        c.Approve(Guid.NewGuid(), null);
        _db.Customers.Add(c);
        return c;
    }

    private MemberAccount ActiveAccount(Customer c, string no)
    {
        var a = new MemberAccount(_org.Id, _branch.Id, c.Id, _product.Id, no, c.DisplayName, "SAVINGS");
        a.UpdateTerms(_product.Id, "GMD", 100m, new DateOnly(2026, 1, 1));
        a.Approve(Guid.NewGuid());
        _db.MemberAccounts.Add(a);
        return a;
    }

    private LoanController Loans(MicrobankerDbContext? db = null)
    {
        var c = new LoanController(db ?? _db, _enc);
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
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    private LoanFormModel Form(decimal amount = 1200m) => new()
    {
        CustomerNo = _customer.CustomerNo, BranchId = _branch.Id, ProductDefinitionId = _product.Id, LoanName = "Stock loan",
        Currency = "GMD", LoanAmount = amount, InterestRate = 12m, InterestCalcMethod = "FLAT", Tenure = 12, NoOfInstallments = 12,
        RepaymentFrequency = "MONTHLY", RepaymentMode = "CASH"
    };

    private async Task<LoanAccount> NewLoan(decimal amount = 1200m, bool approve = false, bool disburse = false)
    {
        Assert.IsType<RedirectToActionResult>(await Loans().Create(Form(amount)));
        var loan = await _db.LoanAccounts.OrderByDescending(l => l.CreatedAt).FirstAsync();
        if (approve || disburse) { await Loans().Approve(Id(loan.Id)); await Reload(loan); }
        if (disburse) { await Loans().Disburse(Id(loan.Id), new DisburseViewModel { DisbursementDate = Today }); await Reload(loan); }
        return loan;
    }

    private async Task<T> Reload<T>(T entity) where T : class { await _db.Entry(entity).ReloadAsync(); return entity; }

    // ── Create / numbering ────────────────────────────────────────────────────

    [Fact]
    public async Task Create_issues_sequential_loan_numbers_and_starts_pending()
    {
        await NewLoan();
        await NewLoan();
        var nos = await _db.LoanAccounts.OrderBy(l => l.LoanNo).Select(l => l.LoanNo).ToListAsync();
        var year = DateTime.UtcNow.Year;
        Assert.Equal(new[] { $"LN-{year}-000001", $"LN-{year}-000002" }, nos);
        Assert.All(_db.LoanAccounts, l => Assert.Equal("PENDING", l.Status));
        Assert.Equal(_userId, (await _db.LoanAccounts.FirstAsync()).CreatedByUserId);
    }

    [Fact]
    public async Task Numbers_are_not_reused_after_a_delete()
    {
        var first = await NewLoan();
        await Loans().Delete(Id(first.Id));
        await NewLoan();
        Assert.EndsWith("000002", (await _db.LoanAccounts.SingleAsync(l => !l.IsDeleted)).LoanNo);
    }

    [Fact]
    public async Task Create_rejects_unknown_and_inactive_customers()
    {
        var c = Loans();
        var f = Form(); f.CustomerNo = "CUST-9999-000001";
        Assert.IsType<ViewResult>(await c.Create(f));
        Assert.True(c.ModelState.ContainsKey(nameof(LoanFormModel.CustomerNo)));

        _customer.Suspend();
        await _db.SaveChangesAsync();
        var c2 = Loans();
        Assert.IsType<ViewResult>(await c2.Create(Form()));
        Assert.True(c2.ModelState.ContainsKey(nameof(LoanFormModel.CustomerNo)));
        Assert.Empty(_db.LoanAccounts);
    }

    [Fact]
    public async Task Create_validates_terms_dates_and_precision()
    {
        var bad = Form(); bad.InterestCalcMethod = "BULLET"; bad.RepaymentFrequency = "YEARLY"; bad.RepaymentMode = "BARTER";
        bad.FirstEMIDate = Today; bad.LoanAmount = 100.123m; bad.Currency = "G1";
        var c = Loans();
        Assert.IsType<ViewResult>(await c.Create(bad));
        foreach (var field in new[] { nameof(LoanFormModel.InterestCalcMethod), nameof(LoanFormModel.RepaymentFrequency), nameof(LoanFormModel.RepaymentMode),
                     nameof(LoanFormModel.FirstEMIDate), nameof(LoanFormModel.LoanAmount), nameof(LoanFormModel.Currency) })
            Assert.True(c.ModelState.ContainsKey(field), field);
        Assert.Empty(_db.LoanAccounts);
    }

    [Fact]
    public async Task Settlement_account_must_be_the_borrowers_active_account_in_the_loan_currency()
    {
        var wrongOwner = Form(); wrongOwner.RepaymentMode = "DEDUCTION"; wrongOwner.SettlementAccountNo = _othersSavings.AccountNo;
        var c = Loans();
        Assert.IsType<ViewResult>(await c.Create(wrongOwner));
        Assert.True(c.ModelState.ContainsKey(nameof(LoanFormModel.SettlementAccountNo)));

        var missing = Form(); missing.RepaymentMode = "STANDING_ORDER";            // required for this mode
        var c2 = Loans();
        Assert.IsType<ViewResult>(await c2.Create(missing));
        Assert.True(c2.ModelState.ContainsKey(nameof(LoanFormModel.SettlementAccountNo)));

        var currency = Form(); currency.RepaymentMode = "DEDUCTION"; currency.SettlementAccountNo = _savings.AccountNo; currency.Currency = "USD";
        var c3 = Loans();
        Assert.IsType<ViewResult>(await c3.Create(currency));
        Assert.True(c3.ModelState.ContainsKey(nameof(LoanFormModel.SettlementAccountNo)));

        var ok = Form(); ok.RepaymentMode = "DEDUCTION"; ok.SettlementAccountNo = _savings.AccountNo.ToLowerInvariant();
        Assert.IsType<RedirectToActionResult>(await Loans().Create(ok));
    }

    [Fact]
    public async Task A_borrower_cannot_guarantee_their_own_loan()
    {
        var own = Form(); own.GuarantorsAccountNo = _savings.AccountNo;
        var c = Loans();
        Assert.IsType<ViewResult>(await c.Create(own));
        Assert.True(c.ModelState.ContainsKey(nameof(LoanFormModel.GuarantorsAccountNo)));

        var other = Form(); other.GuarantorsAccountNo = _othersSavings.AccountNo;
        Assert.IsType<RedirectToActionResult>(await Loans().Create(other));
    }

    // ── Edit / approve / delete ───────────────────────────────────────────────

    [Fact]
    public async Task Pending_loans_can_be_edited_but_approved_loans_cannot()
    {
        var loan = await NewLoan();
        var f = Form(2400m); f.LoanName = "Renamed";
        Assert.IsType<RedirectToActionResult>(await Loans().Edit(Id(loan.Id), f));
        await Reload(loan);
        Assert.Equal(2400m, loan.LoanAmount);
        Assert.Equal("Renamed", loan.LoanName);

        await Loans().Approve(Id(loan.Id));
        Assert.IsType<RedirectToActionResult>(await Loans().Edit(Id(loan.Id)));              // bounced
        Assert.IsType<RedirectToActionResult>(await Loans().Edit(Id(loan.Id), Form(5m)));
        await Reload(loan);
        Assert.Equal(2400m, loan.LoanAmount);
    }

    [Fact]
    public async Task Approving_needs_an_active_customer_and_only_pending_loans_can_be_deleted()
    {
        var loan = await NewLoan();
        _customer.Suspend();
        await _db.SaveChangesAsync();
        await Loans().Approve(Id(loan.Id));
        await Reload(loan);
        Assert.Equal("PENDING", loan.Status);

        _customer.Reactivate();
        await _db.SaveChangesAsync();
        await Loans().Approve(Id(loan.Id));
        await Reload(loan);
        Assert.Equal("APPROVED", loan.Status);
        Assert.Equal(_userId, loan.ApprovedBy);

        await Loans().Delete(Id(loan.Id));
        await Reload(loan);
        Assert.False(loan.IsDeleted);
    }

    // ── Disburse ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Disburse_creates_the_schedule_and_activates_the_loan()
    {
        var loan = await NewLoan(approve: true);
        var result = await Loans().Disburse(Id(loan.Id), new DisburseViewModel { DisbursementDate = Today });
        Assert.IsType<RedirectToActionResult>(result);

        await Reload(loan);
        Assert.Equal("ACTIVE", loan.Status);
        Assert.Equal(1200m, loan.OutstandingBalance);
        var rows = await _db.LoanRepayments.Where(r => r.LoanAccountId == loan.Id).OrderBy(r => r.InstallmentNo).ToListAsync();
        Assert.Equal(12, rows.Count);
        Assert.Equal(1200m, rows.Sum(r => r.PrincipalDue));
        Assert.Equal(144m, rows.Sum(r => r.InterestDue));
        Assert.Equal(_userId, loan.DisbursedBy);
    }

    [Fact]
    public async Task Disburse_rejects_future_dates_an_early_first_installment_and_a_second_attempt()
    {
        var loan = await NewLoan(approve: true);
        await Loans().Disburse(Id(loan.Id), new DisburseViewModel { DisbursementDate = Today.AddDays(5) });
        await Loans().Disburse(Id(loan.Id), new DisburseViewModel { DisbursementDate = Today, FirstEmiDate = Today });
        await Reload(loan);
        Assert.Equal("APPROVED", loan.Status);
        Assert.Empty(_db.LoanRepayments);

        await Loans().Disburse(Id(loan.Id), new DisburseViewModel { DisbursementDate = Today });
        await Loans().Disburse(Id(loan.Id), new DisburseViewModel { DisbursementDate = Today });   // already active
        Assert.Equal(12, await _db.LoanRepayments.CountAsync());                                    // not duplicated
    }

    [Fact]
    public async Task A_pending_loan_cannot_be_disbursed()
    {
        var loan = await NewLoan();
        await Loans().Disburse(Id(loan.Id), new DisburseViewModel { DisbursementDate = Today });
        await Reload(loan);
        Assert.Equal("PENDING", loan.Status);
        Assert.Empty(_db.LoanRepayments);
    }

    // ── Repayment ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Repayment_applies_the_payment_records_a_receipt_and_reduces_the_balance()
    {
        var loan = await NewLoan(disburse: true);                 // 1,200 flat 12%: 100 principal + 12 interest per installment
        await Loans().Repayment(Id(loan.Id), new RepaymentViewModel { Amount = 112m, PaidOn = Today, Mode = "CASH", Reference = "RCT-1" });

        await Reload(loan);
        Assert.Equal(1100m, loan.OutstandingBalance);
        var first = await _db.LoanRepayments.SingleAsync(r => r.LoanAccountId == loan.Id && r.InstallmentNo == 1);
        Assert.Equal("PAID", first.Status);
        var receipt = await _db.LoanPayments.SingleAsync();
        Assert.Equal(112m, receipt.Amount);
        Assert.Equal(100m, receipt.PrincipalPaid);
        Assert.Equal(12m, receipt.InterestPaid);
        Assert.Equal("RCT-1", receipt.Reference);
        Assert.Equal(_userId, receipt.PostedBy);
    }

    [Fact]
    public async Task Invalid_repayments_change_nothing()
    {
        var loan = await NewLoan(disburse: true);
        foreach (var amount in new[] { 0m, -5m, 10.005m, 1344.01m })
            await Loans().Repayment(Id(loan.Id), new RepaymentViewModel { Amount = amount, PaidOn = Today, Mode = "CASH" });
        await Loans().Repayment(Id(loan.Id), new RepaymentViewModel { Amount = 10m, PaidOn = Today.AddDays(1), Mode = "CASH" });     // future date
        await Loans().Repayment(Id(loan.Id), new RepaymentViewModel { Amount = 10m, PaidOn = Today.AddDays(-1), Mode = "CASH" });    // before disbursement
        await Loans().Repayment(Id(loan.Id), new RepaymentViewModel { Amount = 10m, PaidOn = Today, Mode = "BARTER" });

        await Reload(loan);
        Assert.Equal(1200m, loan.OutstandingBalance);
        Assert.Empty(_db.LoanPayments);
    }

    [Fact]
    public async Task Paying_the_whole_loan_closes_it_and_blocks_further_repayments()
    {
        var loan = await NewLoan(disburse: true);
        await Loans().Repayment(Id(loan.Id), new RepaymentViewModel { Amount = 1344m, PaidOn = Today, Mode = "CASH" });
        await Reload(loan);
        Assert.Equal("CLOSED", loan.Status);
        Assert.Equal(0m, loan.OutstandingBalance);

        await Loans().Repayment(Id(loan.Id), new RepaymentViewModel { Amount = 1m, PaidOn = Today, Mode = "CASH" });
        Assert.Single(_db.LoanPayments);
    }

    [Fact]
    public async Task Repayments_on_a_loan_that_is_not_active_are_refused()
    {
        var loan = await NewLoan(approve: true);
        await Loans().Repayment(Id(loan.Id), new RepaymentViewModel { Amount = 10m, PaidOn = Today, Mode = "CASH" });
        Assert.Empty(_db.LoanPayments);
    }

    [Fact]
    public async Task Concurrent_repayments_on_the_same_loan_do_not_overwrite_each_other()
    {
        var loan = await NewLoan(disburse: true);

        using var other = new MicrobankerDbContext(_options);
        var stale = await other.LoanAccounts.SingleAsync(l => l.Id == loan.Id);
        var staleRows = await other.LoanRepayments.Where(r => r.LoanAccountId == loan.Id).ToListAsync();

        await Loans().Repayment(Id(loan.Id), new RepaymentViewModel { Amount = 112m, PaidOn = Today, Mode = "CASH" });   // first user wins

        other.LoanPayments.Add(stale.ApplyPayment(112m, Today, "CASH", null, Guid.NewGuid(), staleRows));
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => other.SaveChangesAsync());

        await Reload(loan);
        Assert.Equal(1100m, loan.OutstandingBalance);
    }

    // ── Loan requests ─────────────────────────────────────────────────────────

    private async Task<LoanRequest> NewApprovedRequest(decimal amount = 5000m)
    {
        var c = Loans();
        Assert.IsType<RedirectToActionResult>(await c.CreateRequest(new LoanRequestFormModel
        {
            CustomerNo = _customer.CustomerNo, BranchId = _branch.Id, ProductDefinitionId = _product.Id, RequestedAmount = amount, Purpose = "Stock"
        }));
        var req = await _db.LoanRequests.OrderByDescending(r => r.CreatedAt).FirstAsync();
        await Loans().ApproveRequest(Id(req.Id), "ok");
        return await Reload(req);
    }

    [Fact]
    public async Task A_loan_request_flows_through_review_to_a_loan()
    {
        var req = await NewApprovedRequest(5000m);
        Assert.Equal("APPROVED", req.Status);

        // Create GET pre-fills from the request.
        var view = Assert.IsType<ViewResult>(await Loans().Create(Id(req.Id), null));
        var prefilled = Assert.IsType<LoanFormModel>(view.Model);
        Assert.Equal(5000m, prefilled.LoanAmount);
        Assert.Equal(_customer.CustomerNo, prefilled.CustomerNo);

        var f = Form(4000m); f.LoanRequestId = req.Id;
        Assert.IsType<RedirectToActionResult>(await Loans().Create(f));
        await Reload(req);
        Assert.Equal("CONVERTED", req.Status);
        Assert.Equal(req.Id, (await _db.LoanAccounts.SingleAsync()).LoanRequestId);
    }

    [Fact]
    public async Task A_loan_cannot_exceed_the_approved_request_or_use_it_twice()
    {
        var req = await NewApprovedRequest(1000m);
        var tooBig = Form(1000.01m); tooBig.LoanRequestId = req.Id;
        var c = Loans();
        Assert.IsType<ViewResult>(await c.Create(tooBig));
        Assert.True(c.ModelState.ContainsKey(nameof(LoanFormModel.LoanAmount)));

        var ok = Form(1000m); ok.LoanRequestId = req.Id;
        Assert.IsType<RedirectToActionResult>(await Loans().Create(ok));
        var again = Form(500m); again.LoanRequestId = req.Id;
        Assert.IsType<ViewResult>(await Loans().Create(again));                 // request already converted
        Assert.Single(_db.LoanAccounts);
    }

    [Fact]
    public async Task Deleting_a_pending_loan_gives_its_request_back()
    {
        var req = await NewApprovedRequest();
        var f = Form(); f.LoanRequestId = req.Id;
        await Loans().Create(f);
        var loan = await _db.LoanAccounts.SingleAsync();
        await Loans().Delete(Id(loan.Id));
        await Reload(req);
        Assert.Equal("APPROVED", req.Status);
    }

    [Fact]
    public async Task Requests_need_an_active_customer_and_a_reason_to_reject()
    {
        var c = Loans();
        Assert.IsType<ViewResult>(await c.CreateRequest(new LoanRequestFormModel
        {
            CustomerNo = "CUST-9999-000001", BranchId = _branch.Id, ProductDefinitionId = _product.Id, RequestedAmount = 10m
        }));
        Assert.True(c.ModelState.ContainsKey(nameof(LoanRequestFormModel.CustomerNo)));

        await Loans().CreateRequest(new LoanRequestFormModel { CustomerNo = _customer.CustomerNo, BranchId = _branch.Id, ProductDefinitionId = _product.Id, RequestedAmount = 10m });
        var req = await _db.LoanRequests.SingleAsync();
        await Loans().RejectRequest(Id(req.Id), " ");
        await Reload(req);
        Assert.Equal("PENDING", req.Status);
        await Loans().RejectRequest(Id(req.Id), "Too risky");
        await Reload(req);
        Assert.Equal("REJECTED", req.Status);
        await Loans().ApproveRequest(Id(req.Id), null);                          // already reviewed
        await Reload(req);
        Assert.Equal("REJECTED", req.Status);
    }

    // ── Lookups / isolation ───────────────────────────────────────────────────

    [Fact]
    public async Task A_tampered_id_is_not_found_instead_of_throwing()
    {
        Assert.IsType<NotFoundResult>(await Loans().Details("%%%"));
        Assert.IsType<NotFoundResult>(await Loans().Edit("junk"));
        Assert.IsType<NotFoundResult>(await Loans().Repayment("junk"));
        Assert.IsType<NotFoundResult>(await Loans().Disburse("junk"));
    }

    [Fact]
    public async Task Loans_of_another_organisation_are_not_reachable()
    {
        var other = new Organization("Other", "OTH");
        _db.Organizations.Add(other);
        var ob = new Branch(other.Id, "B", "B1");
        _db.Branches.Add(ob);
        var oc = new Customer(other.Id, ob.Id, "CUST-2026-000001", "INDIVIDUAL", "X", "Y");
        oc.Update(new CustomerProfile { BranchId = ob.Id, FirstName = "X", LastName = "Y" });
        _db.Customers.Add(oc);
        var ot = new ProductType(other.Id, "L", "L", null);
        _db.ProductTypes.Add(ot);
        var op = new ProductDefinition(other.Id, ot.Id, "P", "P");
        _db.ProductDefinitions.Add(op);
        var foreign = new LoanAccount(other.Id, ob.Id, oc.Id, op.Id, "LN-2026-000001", 100m);
        foreign.UpdateTerms(new LoanTerms { ProductDefinitionId = op.Id, LoanName = "X", LoanAmount = 100m, Tenure = 6, NoOfInstallments = 6 });
        _db.LoanAccounts.Add(foreign);
        await _db.SaveChangesAsync();

        Assert.IsType<NotFoundResult>(await Loans().Details(Id(foreign.Id)));
        var enquiry = (LoanEnquiryViewModel)((ViewResult)await Loans().BalanceEnquiry("LN-2026-000001")).Model!;
        Assert.Null(enquiry.Loan);
    }

    [Fact]
    public async Task Enquiry_finds_a_loan_by_number_in_any_case_and_shows_the_schedule()
    {
        var loan = await NewLoan(disburse: true);
        var found = (LoanEnquiryViewModel)((ViewResult)await Loans().BalanceEnquiry(loan.LoanNo.ToLowerInvariant())).Model!;
        Assert.True(found.Searched);
        Assert.Equal(12, found.Loan!.Schedule.Count);
        Assert.False(found.Loan.ScheduleIsPreview);
        Assert.Equal(1344m, found.Loan.TotalRemaining);

        var missing = (LoanEnquiryViewModel)((ViewResult)await Loans().BalanceEnquiry("NOPE")).Model!;
        Assert.True(missing.Searched);
        Assert.Null(missing.Loan);
    }

    [Fact]
    public async Task A_loan_that_is_not_disbursed_shows_a_preview_schedule()
    {
        var loan = await NewLoan();
        var vm = (LoanDetailViewModel)((ViewResult)await Loans().Details(Id(loan.Id))).Model!;
        Assert.True(vm.ScheduleIsPreview);
        Assert.Equal(12, vm.Schedule.Count);
        Assert.Equal(1344m, vm.TotalPayable);
    }

    [Fact]
    public async Task Index_filters_by_status_and_search()
    {
        await NewLoan(approve: true);
        await NewLoan();
        var all = (LoanListViewModel)((ViewResult)await Loans().Index(null, null)).Model!;
        Assert.Equal(2, all.TotalCount);
        Assert.Single(((LoanListViewModel)((ViewResult)await Loans().Index("APPROVED", null)).Model!).Rows);
        Assert.Equal(2, ((LoanListViewModel)((ViewResult)await Loans().Index(null, "LN-")).Model!).Rows.Count);
    }

    private sealed class NullTempData : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
