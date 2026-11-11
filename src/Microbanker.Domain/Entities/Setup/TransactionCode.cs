namespace Microbanker.Domain.Entities.Setup;

public class TransactionCode : BaseEntity
{
    public static readonly string[] DrCrValues = { "DR", "CR" };

    public Guid OrganizationId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    /// <summary>"DR" or "CR".</summary>
    public string DrCr { get; private set; } = "DR";
    public string? Description { get; private set; }
    public bool RequiresApproval { get; private set; }
    public bool IsActive { get; private set; } = true;

    private TransactionCode() { }

    public TransactionCode(Guid orgId, string code, string name, string drCr)
    {
        OrganizationId = orgId;
        Code = code;
        Name = name;
        DrCr = drCr;
        IsActive = true;
    }

    public void Update(string code, string name, string drCr, string? description, bool requiresApproval, bool isActive)
    {
        Code = code;
        Name = name;
        DrCr = drCr;
        Description = description;
        RequiresApproval = requiresApproval;
        IsActive = isActive;
        SetUpdatedAt();
    }

    public void SoftDelete() => SetDeleted();
}
