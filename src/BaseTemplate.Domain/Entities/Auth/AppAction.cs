// src/BaseTemplate.Domain/Entities/Auth/AppAction.cs
using BaseTemplate.Domain.Entities;

namespace BaseTemplate.Domain.Entities.Auth;

/// <summary>Operation (View, Create, Edit, Delete, Approve, Export) on a module.</summary>
public class AppAction : BaseEntity
{
    public Guid AppModuleId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation
    public AppModule AppModule { get; private set; } = null!;

    private AppAction() { }

    public AppAction(Guid appModuleId, string name, string code, int sortOrder = 0)
    {
        AppModuleId = appModuleId;
        Name = name.Trim();
        Code = code.Trim().ToUpper();
        SortOrder = sortOrder;
    }
}
