namespace Microbanker.Domain.Entities.Setup;

public class SubSector : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public Guid SectorId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Code { get; private set; }
    public bool IsActive { get; private set; } = true;

    public Sector Sector { get; private set; } = null!;

    private SubSector() { }

    public SubSector(Guid orgId, Guid sectorId, string name, string? code)
    {
        OrganizationId = orgId;
        SectorId = sectorId;
        Name = name;
        Code = code;
        IsActive = true;
    }

    public void Update(Guid sectorId, string name, string? code, bool isActive)
    {
        SectorId = sectorId;
        Name = name;
        Code = code;
        IsActive = isActive;
        SetUpdatedAt();
    }

    public void SoftDelete() => SetDeleted();
}
