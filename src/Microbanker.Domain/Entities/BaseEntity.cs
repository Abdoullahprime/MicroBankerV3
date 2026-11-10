// src/Microbanker.Domain/Entities/BaseEntity.cs
namespace Microbanker.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public bool IsDeleted { get; protected set; } = false;

    protected void SetUpdatedAt()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    protected void SetDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
