// src/BaseTemplate.Web/Models/EmailConfigViewModels.cs
using System.ComponentModel.DataAnnotations;
using BaseTemplate.Domain.Entities.Organization;

namespace BaseTemplate.Web.Models;

public class EmailConfigFormModel
{
    [Required, MaxLength(200)]
    [Display(Name = "SMTP Server / Host")]
    public string SmtpHost { get; set; } = "";

    [Required, Range(1, 65535)]
    [Display(Name = "Port")]
    public int SmtpPort { get; set; } = 587;

    [Display(Name = "Use SSL / TLS")]
    public bool EnableSsl { get; set; } = true;

    [Required, MaxLength(200), EmailAddress]
    [Display(Name = "From Email Address")]
    public string FromEmail { get; set; } = "";

    [Required, MaxLength(200)]
    [Display(Name = "From Name")]
    public string FromName { get; set; } = "";

    [Required, MaxLength(200)]
    [Display(Name = "Username / Login")]
    public string Username { get; set; } = "";

    [MaxLength(500)]
    [Display(Name = "Password")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    public string Channel { get; set; } = "Core";
    public List<EmailChannelTab> Channels { get; set; } = new();
    public bool IsCore => Channel == "Core";

    public static string Label(EmailChannel c) => c switch
    {
        EmailChannel.Core     => "Core (System)",
        EmailChannel.Portal   => "Portal",
        EmailChannel.External => "External",
        _                     => c.ToString(),
    };

    public static string Purpose(EmailChannel c) => c switch
    {
        EmailChannel.Core     => "Emails sent by the system to its users. Other channels fall back to this one.",
        EmailChannel.Portal   => "Emails sent from a secondary audience such as a self-service portal.",
        EmailChannel.External => "Emails to external or public users.",
        _                     => "",
    };

    public bool HasExistingConfig { get; set; }
    public string? EncryptedId    { get; set; }

    // Test email
    [EmailAddress]
    [Display(Name = "Send Test To")]
    public string? TestEmail { get; set; }
}

public record EmailChannelTab(string Key, string Label, string Purpose, bool Configured,
                              string? Host = null, int? Port = null, string? FromEmail = null, string? FromName = null, DateTime? UpdatedAt = null);
