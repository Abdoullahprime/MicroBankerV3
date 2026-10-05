// src/BaseTemplate.Domain/Entities/Organization/EmailConfiguration.cs
using BaseTemplate.Domain.Entities;

namespace BaseTemplate.Domain.Entities.Organization;

// Which audience a mail server is used for. A channel with no configuration falls back to Core.
public enum EmailChannel
{
    Core     = 0, // Default system mail server; other channels fall back to it
    Portal   = 1, // Optional second audience (e.g. a self-service portal)
    External = 2, // Optional third audience (e.g. public or external users)
}

public class EmailConfiguration : BaseEntity
{
    public Guid   OrganizationId { get; private set; }
    public string SmtpHost       { get; private set; } = string.Empty;
    public int    SmtpPort       { get; private set; } = 587;
    public bool   EnableSsl      { get; private set; } = true;
    public string FromEmail      { get; private set; } = string.Empty;
    public string FromName       { get; private set; } = string.Empty;
    public string Username       { get; private set; } = string.Empty;
    public string Password       { get; private set; } = string.Empty;
    public bool   IsActive       { get; private set; } = true;
    public EmailChannel Channel  { get; private set; } = EmailChannel.Core;

    private EmailConfiguration() { }

    public EmailConfiguration(Guid organizationId, string smtpHost, int smtpPort,
        bool enableSsl, string fromEmail, string fromName, string username, string password,
        EmailChannel channel = EmailChannel.Core)
    {
        Channel        = channel;
        OrganizationId = organizationId;
        SmtpHost       = smtpHost;
        SmtpPort       = smtpPort;
        EnableSsl      = enableSsl;
        FromEmail      = fromEmail;
        FromName       = fromName;
        Username       = username;
        Password       = password;
    }

    public void Update(string smtpHost, int smtpPort, bool enableSsl,
        string fromEmail, string fromName, string username, string? newPassword)
    {
        SmtpHost  = smtpHost;
        SmtpPort  = smtpPort;
        EnableSsl = enableSsl;
        FromEmail = fromEmail;
        FromName  = fromName;
        Username  = username;
        if (newPassword != null)
            Password = newPassword;
        IsActive  = true;
        SetUpdatedAt();
    }

    public void SoftDelete() => SetDeleted();
}
