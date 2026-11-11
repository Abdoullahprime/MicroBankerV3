using Microbanker.Web.Models;
using Microbanker.Web.Services;

namespace Microbanker.Tests;

public class ProfileMapperTests
{
    private static CustomerFormModel Form(string type) => new()
    {
        BranchId = Guid.NewGuid(), CustomerType = type,
        FirstName = " Awa ", LastName = "Jallow", Dependants = 3, DateOfBirth = new DateOnly(1990, 1, 2),
        BusinessName = "Jallow Trading", BusinessRegNo = "R1",
        GroupName = "Women's Group", NoOfMembers = 10, NoOfMale = 4, NoOfFemale = 6,
        MobileNo = "  7000000 ", Email = "   "
    };

    [Fact]
    public void Individual_drops_business_and_group_fields()
    {
        var p = ProfileMapper.ToProfile(Form("INDIVIDUAL"));
        Assert.Equal("Awa", p.FirstName);
        Assert.Equal(3, p.Dependants);
        Assert.Null(p.BusinessName);
        Assert.Null(p.GroupName);
        Assert.Equal(0, p.NoOfMembers);
    }

    [Fact]
    public void Business_drops_personal_and_group_fields()
    {
        var p = ProfileMapper.ToProfile(Form("BUSINESS"));
        Assert.Equal("Jallow Trading", p.BusinessName);
        Assert.Equal(string.Empty, p.FirstName);
        Assert.Equal(0, p.Dependants);
        Assert.Null(p.DateOfBirth);
        Assert.Null(p.GroupName);
    }

    [Fact]
    public void Group_keeps_counts_and_drops_the_rest()
    {
        var p = ProfileMapper.ToProfile(Form("GROUP"));
        Assert.Equal("Women's Group", p.GroupName);
        Assert.Equal(10, p.NoOfMembers);
        Assert.Null(p.BusinessName);
        Assert.Equal(string.Empty, p.LastName);
    }

    [Fact]
    public void Shared_contact_fields_are_trimmed_and_blank_becomes_null()
    {
        var p = ProfileMapper.ToProfile(Form("INDIVIDUAL"));
        Assert.Equal("7000000", p.MobileNo);
        Assert.Null(p.Email);
    }
}
