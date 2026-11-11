using Microbanker.Domain.Entities.Accounts;

namespace Microbanker.Tests;

public class AccountDomainTests
{
    private static MemberAccount NewAccount(decimal opening = 1000m, bool approved = true)
    {
        var a = new MemberAccount(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "MN-SA-0000001", "Awa Jallow", AccountOptions.Savings);
        a.UpdateTerms(Guid.NewGuid(), "GMD", opening, new DateOnly(2026, 1, 1));
        if (approved) a.Approve(Guid.NewGuid());
        return a;
    }

    [Fact]
    public void Opening_balance_becomes_book_and_available_balance()
    {
        var a = NewAccount(500m, approved: false);
        Assert.Equal(500m, a.BookBalance);
        Assert.Equal(500m, a.AvailableBalance);
        Assert.Equal(AccountOptions.Pending, a.Status);
    }

    [Fact]
    public void Approve_activates_once()
    {
        var a = NewAccount();
        Assert.Equal(AccountOptions.Active, a.Status);
        Assert.True(a.IsApproved);
        Assert.Throws<InvalidOperationException>(() => a.Approve(Guid.NewGuid()));
    }

    [Fact]
    public void Terms_are_locked_after_approval()
    {
        var a = NewAccount();
        Assert.Throws<InvalidOperationException>(() => a.UpdateTerms(Guid.NewGuid(), "GMD", 1m, new DateOnly(2026, 1, 1)));
    }

    [Fact]
    public void Block_moves_money_from_available_to_blocked_and_keeps_book_balance()
    {
        var a = NewAccount(1000m);
        a.BlockAmount(300m);
        Assert.Equal(300m, a.BlockedAmount);
        Assert.Equal(700m, a.AvailableBalance);
        Assert.Equal(1000m, a.BookBalance);
        a.UnblockAmount(100m);
        Assert.Equal(200m, a.BlockedAmount);
        Assert.Equal(800m, a.AvailableBalance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Non_positive_amounts_are_rejected(double amount)
    {
        var a = NewAccount();
        a.BlockAmount(10m);
        Assert.Throws<ArgumentOutOfRangeException>(() => a.BlockAmount((decimal)amount));
        Assert.Throws<ArgumentOutOfRangeException>(() => a.UnblockAmount((decimal)amount));
    }

    [Fact]
    public void Cannot_block_more_than_available_or_release_more_than_blocked()
    {
        var a = NewAccount(100m);
        Assert.Throws<InvalidOperationException>(() => a.BlockAmount(100.01m));
        a.BlockAmount(100m);
        Assert.Equal(0m, a.AvailableBalance);
        Assert.Throws<InvalidOperationException>(() => a.BlockAmount(1m));
        Assert.Throws<InvalidOperationException>(() => a.UnblockAmount(100.01m));
    }

    [Fact]
    public void Blocking_needs_an_active_account()
    {
        var pending = NewAccount(approved: false);
        Assert.Throws<InvalidOperationException>(() => pending.BlockAmount(1m));

        var frozen = NewAccount();
        frozen.Freeze();
        Assert.Throws<InvalidOperationException>(() => frozen.BlockAmount(1m));
    }

    [Fact]
    public void Freeze_and_unfreeze_follow_the_status_rules()
    {
        var a = NewAccount();
        a.Freeze();
        Assert.Equal(AccountOptions.Frozen, a.Status);
        Assert.Throws<InvalidOperationException>(() => a.Freeze());
        a.Unfreeze();
        Assert.Equal(AccountOptions.Active, a.Status);
        Assert.Throws<InvalidOperationException>(() => a.Unfreeze());
    }

    [Fact]
    public void Close_needs_nothing_left_in_the_account()
    {
        var withMoney = NewAccount(10m);
        Assert.Throws<InvalidOperationException>(() => withMoney.Close());

        var empty = NewAccount(0m);
        empty.Close();
        Assert.Equal(AccountOptions.Closed, empty.Status);
        Assert.NotNull(empty.ClosedAt);
        Assert.Throws<InvalidOperationException>(() => empty.Close());
        Assert.Throws<InvalidOperationException>(() => empty.Update("x", null, 0, null, null, null, null, null, null, null, null));
    }

    [Fact]
    public void Only_pending_accounts_can_be_deleted()
    {
        var pending = NewAccount(approved: false);
        pending.SoftDelete();
        Assert.True(pending.IsDeleted);
        Assert.Throws<InvalidOperationException>(() => NewAccount().SoftDelete());
    }

    [Fact]
    public void Every_change_replaces_the_concurrency_stamp()
    {
        var a = NewAccount();
        var before = a.ConcurrencyStamp;
        a.BlockAmount(1m);
        Assert.NotEqual(before, a.ConcurrencyStamp);
    }

    [Fact]
    public void Account_type_prefixes()
    {
        Assert.Equal("SA", AccountOptions.TypePrefix(AccountOptions.Savings));
        Assert.Equal("CA", AccountOptions.TypePrefix(AccountOptions.Current));
        Assert.Equal("FD", AccountOptions.TypePrefix(AccountOptions.FixedDeposit));
        Assert.Throws<ArgumentOutOfRangeException>(() => AccountOptions.TypePrefix("LOAN"));
    }

    [Fact]
    public void Edit_request_can_be_reviewed_once_and_completed_only_when_approved()
    {
        var r = new AccountEditRequest(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Change name");
        Assert.Throws<InvalidOperationException>(() => r.Complete());
        r.Approve(Guid.NewGuid(), null);
        Assert.Throws<InvalidOperationException>(() => r.Reject(Guid.NewGuid(), "no"));
        r.Complete();
        Assert.Equal(AccountOptions.Completed, r.Status);
    }
}
