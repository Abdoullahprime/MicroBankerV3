using Microbanker.Domain.Entities.Customers;

namespace Microbanker.Tests;

public class CustomerDomainTests
{
    private static Customer NewCustomer() =>
        new(Guid.NewGuid(), Guid.NewGuid(), "CUST-2026-000001", CustomerOptions.Individual, "Awa", "Jallow");

    [Fact]
    public void New_customer_is_pending_and_unapproved()
    {
        var c = NewCustomer();
        Assert.Equal(CustomerOptions.Pending, c.Status);
        Assert.False(c.IsApproved);
    }

    [Fact]
    public void Approve_activates_and_records_the_approver()
    {
        var c = NewCustomer();
        var approver = Guid.NewGuid();
        c.Approve(approver, "ok");
        Assert.Equal(CustomerOptions.Active, c.Status);
        Assert.True(c.IsApproved);
        Assert.Equal(approver, c.ApprovedBy);
        Assert.NotNull(c.ApprovedAt);
    }

    [Fact]
    public void Approve_twice_is_rejected()
    {
        var c = NewCustomer();
        c.Approve(Guid.NewGuid(), null);
        Assert.Throws<InvalidOperationException>(() => c.Approve(Guid.NewGuid(), null));
    }

    [Fact]
    public void Suspend_and_reactivate_follow_the_status_rules()
    {
        var c = NewCustomer();
        Assert.Throws<InvalidOperationException>(() => c.Suspend());       // still pending
        c.Approve(Guid.NewGuid(), null);
        c.Suspend();
        Assert.Equal(CustomerOptions.Suspended, c.Status);
        Assert.Throws<InvalidOperationException>(() => c.Suspend());       // already suspended
        c.Reactivate();
        Assert.Equal(CustomerOptions.Active, c.Status);
        Assert.Throws<InvalidOperationException>(() => c.Reactivate());    // already active
    }

    [Fact]
    public void Update_keeps_the_customer_type()
    {
        var c = NewCustomer();
        c.Update(new CustomerProfile { BranchId = Guid.NewGuid(), CustomerType = CustomerOptions.Business, BusinessName = "X" });
        Assert.Equal(CustomerOptions.Individual, c.CustomerType);
    }

    [Fact]
    public void Self_registration_can_only_be_reviewed_once()
    {
        var r = new SelfRegistration(Guid.NewGuid(), Guid.NewGuid(), CustomerOptions.Individual);
        r.Reject(Guid.NewGuid(), "incomplete");
        Assert.Equal(CustomerOptions.Rejected, r.Status);
        Assert.Throws<InvalidOperationException>(() => r.Approve(Guid.NewGuid(), Guid.NewGuid()));
        Assert.Throws<InvalidOperationException>(() => r.Update(new CustomerProfile()));
    }

    [Fact]
    public void DisplayName_depends_on_the_customer_type()
    {
        var org = Guid.NewGuid();
        var person = new Customer(org, Guid.NewGuid(), "A", CustomerOptions.Individual, "Awa", "Jallow");
        person.Update(new CustomerProfile { BranchId = Guid.NewGuid(), FirstName = "Awa", MiddleName = "B", LastName = "Jallow" });
        Assert.Equal("Awa B Jallow", person.DisplayName);

        var biz = new Customer(org, Guid.NewGuid(), "B", CustomerOptions.Business, "", "");
        biz.Update(new CustomerProfile { BranchId = Guid.NewGuid(), BusinessName = "Jallow Trading" });
        Assert.Equal("Jallow Trading", biz.DisplayName);
    }
}
