// src/BaseTemplate.Infrastructure/Services/IEmailService.cs
using BaseTemplate.Domain.Entities.Organization;

namespace BaseTemplate.Infrastructure.Services;

public interface IEmailService
{
    Task<(bool Success, string? Error)> SendAsync(
        string toEmail, string toName, string subject, string htmlBody);

    // Sends through the mail server configured for this channel, or the Core one if it has none.
    Task<(bool Success, string? Error)> SendAsync(
        string toEmail, string toName, string subject, string htmlBody, EmailChannel channel);
}
