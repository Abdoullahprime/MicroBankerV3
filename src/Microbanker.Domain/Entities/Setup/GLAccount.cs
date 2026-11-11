namespace Microbanker.Domain.Entities.Setup;

/// <summary>Chart of accounts entry. Level is 1 for top-level accounts and parent.Level + 1 below.</summary>
public class GLAccount : BaseEntity
{
    public static readonly string[] AccountTypes = { "ASSET", "LIABILITY", "INCOME", "EXPENSE", "EQUITY" };

    public Guid OrganizationId { get; private set; }
    public string AccountNumber { get; private set; } = string.Empty;
    public string AccountName { get; private set; } = string.Empty;
    public string AccountType { get; private set; } = "ASSET";
    public Guid? ParentId { get; private set; }
    public int Level { get; private set; } = 1;
    public bool IsControlAccount { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? Description { get; private set; }

    public GLAccount? Parent { get; private set; }

    private GLAccount() { }

    public GLAccount(Guid orgId, string accountNumber, string accountName, string accountType, Guid? parentId, int level)
    {
        OrganizationId = orgId;
        AccountNumber = accountNumber;
        AccountName = accountName;
        AccountType = accountType;
        ParentId = parentId;
        Level = level;
        IsActive = true;
    }

    public void Update(string accountNumber, string accountName, string accountType, Guid? parentId, int level,
        bool isControlAccount, bool isActive, string? description)
    {
        AccountNumber = accountNumber;
        AccountName = accountName;
        AccountType = accountType;
        ParentId = parentId;
        Level = level;
        IsControlAccount = isControlAccount;
        IsActive = isActive;
        Description = description;
        SetUpdatedAt();
    }

    /// <summary>Used when an ancestor is re-parented and descendant levels must follow.</summary>
    public void SetLevel(int level)
    {
        Level = level;
        SetUpdatedAt();
    }

    public void SoftDelete() => SetDeleted();
}
