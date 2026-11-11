namespace Microbanker.Domain.Entities.Setup;

public class Area : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid DistrictId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Code { get; private set; }
    public bool IsActive { get; private set; } = true;

    public District District { get; private set; } = null!;

    private Area() { }

    public Area(Guid orgId, Guid districtId, string name, string? code)
    {
        OrganizationId = orgId;
        DistrictId = districtId;
        Name = name;
        Code = code;
        IsActive = true;
    }

    public void Update(Guid districtId, string name, string? code, bool isActive)
    {
        DistrictId = districtId;
        Name = name;
        Code = code;
        IsActive = isActive;
        SetUpdatedAt();
    }

    public void SoftDelete() => SetDeleted();
}
