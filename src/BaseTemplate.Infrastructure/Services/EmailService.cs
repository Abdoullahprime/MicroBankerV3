// src/BaseTemplate.Infrastructure/Services/EmailService.cs
using System.Net;
using System.Net.Mail;
using BaseTemplate.Domain.Entities.Organization;
using BaseTemplate.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BaseTemplate.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly AppDbContext _db;

    public EmailService(AppDbContext db) => _db = db;

    public Task<(bool Success, string? Error)> SendAsync(
        string toEmail, string toName, string subject, string htmlBody)
        => SendAsync(toEmail, toName, subject, htmlBody, EmailChannel.Core);

    public async Task<(bool Success, string? Error)> SendAsync(
        string toEmail, string toName, string subject, string htmlBody, EmailChannel channel)
    {
        var configs = await _db.EmailConfigurations
            .Where(c => !c.IsDeleted && c.IsActive && (c.Channel == channel || c.Channel == EmailChannel.Core))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        var config = configs.FirstOrDefault(c => c.Channel == channel) ?? configs.FirstOrDefault(c => c.Channel == EmailChannel.Core);

        if (config == null)
            return (false, $"No email configuration found for {channel}. Set it up under Security → Email Configuration.");

        try
        {
            using var client = new SmtpClient(config.SmtpHost, config.SmtpPort)
            {
                EnableSsl   = config.EnableSsl,
                Credentials = new NetworkCredential(config.Username, config.Password),
                Timeout     = 30_000
            };

            var from = new MailAddress(config.FromEmail, config.FromName);
            var to   = new MailAddress(toEmail, toName);

            using var message = new MailMessage(from, to)
            {
                Subject    = subject,
                Body       = htmlBody,
                IsBodyHtml = true
            };

            await client.SendMailAsync(message);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
