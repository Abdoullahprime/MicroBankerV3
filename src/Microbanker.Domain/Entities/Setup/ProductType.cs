namespace Microbanker.Domain.Entities.Setup;

/// <summary>Broad product family, e.g. Savings, Current, Fixed Deposit, Loan.</summary>
public class ProductType : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    private ProductType() { }

    public ProductType(Guid orgId, string name, string code, string? description)
    {
        OrganizationId = orgId;
        Name = name;
        Code = code;
        Description = description;
        IsActive = true;
    }

    public void Update(string name, string code, string? description, bool isActive)
    {
        Name = name;
        Code = code;
        Description = description;
        IsActive = isActive;
        SetUpdatedAt();
    }

    public void SoftDelete() => SetDeleted();
}
