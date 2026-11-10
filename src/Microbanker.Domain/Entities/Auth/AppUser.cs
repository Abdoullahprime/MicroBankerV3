// src/Microbanker.Domain/Entities/Auth/AppUser.cs
using Microbanker.Domain.Entities;

namespace Microbanker.Domain.Entities.Auth;

public class AppUser : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool MustChangePassword { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Initials => $"{(FirstName.Length > 0 ? FirstName[0] : ' ')}{(LastName.Length > 0 ? LastName[0] : ' ')}".Trim().ToUpper();

    private AppUser() { }

    public AppUser(Guid organizationId, string username, string email,
        string passwordHash, string firstName, string lastName)
    {
        OrganizationId = organizationId;
        Username = username.Trim().ToLower();
        Email = email.Trim().ToLower();
        PasswordHash = passwordHash;
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    public void Update(string email, string firstName, string lastName,
        string? phone, bool isActive)
    {
        Email = email.Trim().ToLower();
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Phone = phone;
        IsActive = isActive;
        SetUpdatedAt();
    }

    public void SetPassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        MustChangePassword = false;
        SetUpdatedAt();
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void SoftDelete() => SetDeleted();
}
