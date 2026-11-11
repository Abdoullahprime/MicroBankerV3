namespace Microbanker.Domain.Entities.Setup;

public class District : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid RegionId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Code { get; private set; }
    public bool IsActive { get; private set; } = true;

    public Region Region { get; private set; } = null!;

    private District() { }

    public District(Guid orgId, Guid regionId, string name, string? code)
    {
        OrganizationId = orgId;
        RegionId = regionId;
        Name = name;
        Code = code;
        IsActive = true;
    }

    public void Update(Guid regionId, string name, string? code, bool isActive)
    {
        RegionId = regionId;
        Name = name;
        Code = code;
        IsActive = isActive;
        SetUpdatedAt();
    }

    public void SoftDelete() => SetDeleted();
}
