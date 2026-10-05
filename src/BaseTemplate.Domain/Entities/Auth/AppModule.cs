// src/BaseTemplate.Domain/Entities/Auth/AppModule.cs
using BaseTemplate.Domain.Entities;

namespace BaseTemplate.Domain.Entities.Auth;

/// <summary>Page / feature area that can be secured.</summary>
public class AppModule : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? Icon { get; private set; }
    public string? Group { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    private AppModule() { }

    public AppModule(string name, string code, string? description = null,
        string? icon = null, string? group = null, int sortOrder = 0)
    {
        Name = name.Trim();
        Code = code.Trim().ToUpper();
        Description = description;
        Icon = icon;
        Group = group;
        SortOrder = sortOrder;
    }

    public void Update(string name, string? description, string? icon, string? group, int sortOrder, bool isActive)
    {
        Name = name.Trim();
        Description = description;
        Icon = icon;
        Group = group;
        SortOrder = sortOrder;
        IsActive = isActive;
        SetUpdatedAt();
    }
}
