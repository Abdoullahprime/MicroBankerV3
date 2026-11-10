using BaseTemplate.Domain.Security;
using BaseTemplate.Web.Services;

namespace BaseTemplate.Tests;

public class PermissionSetTests
{
    private static PermissionSet With(params string[] granted) => new(false, granted.ToHashSet());

    [Fact]
    public void Can_defaults_to_view_action()
    {
        var set = With($"{Modules.Users}:{PermissionActions.View}");

        Assert.True(set.Can(Modules.Users));
        Assert.False(set.Can(Modules.Users, PermissionActions.Delete));
    }

    [Fact]
    public void Can_is_false_for_ungranted_module()
    {
        var set = With($"{Modules.Users}:{PermissionActions.View}");

        Assert.False(set.Can(Modules.Roles));
    }

    [Fact]
    public void SuperAdmin_can_do_everything()
    {
        var set = new PermissionSet(true, new HashSet<string>());

        Assert.True(set.Can(Modules.SystemSettings, PermissionActions.Delete));
    }

    [Fact]
    public void CanAny_requires_view_on_at_least_one_module()
    {
        var set = With($"{Modules.Banks}:{PermissionActions.Edit}", $"{Modules.Units}:{PermissionActions.View}");

        Assert.True(set.CanAny(Modules.Banks, Modules.Units));
        Assert.False(set.CanAny(Modules.Banks, Modules.Roles));
    }
}
