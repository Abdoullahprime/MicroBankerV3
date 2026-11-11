namespace Microbanker.Domain.Entities.Setup;

public class Region : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Code { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Region() { }

    public Region(Guid orgId, string name, string? code)
    {
        OrganizationId = orgId;
        Name = name;
        Code = code;
        IsActive = true;
    }

    public void Update(string name, string? code, bool isActive)
    {
        Name = name;
        Code = code;
        IsActive = isActive;
        SetUpdatedAt();
    }

    public void SoftDelete() => SetDeleted();
}
