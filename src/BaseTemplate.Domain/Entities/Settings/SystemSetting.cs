namespace BaseTemplate.Domain.Entities.Settings;

public class SystemSetting : BaseEntity
{
    public string  Key         { get; private set; } = string.Empty;
    public string  Value       { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    private SystemSetting() { }

    public SystemSetting(string key, string value, string? description = null)
    {
        Key         = key;
        Value       = value;
        Description = description;
    }

    public void Update(string value, string? description)
    {
        Value       = value;
        Description = description;
        SetUpdatedAt();
    }

    public void SoftDelete() => SetDeleted();
}
