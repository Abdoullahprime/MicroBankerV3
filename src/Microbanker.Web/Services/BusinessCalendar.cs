// src/Microbanker.Web/Services/BusinessCalendar.cs
using Microsoft.EntityFrameworkCore;
using Microbanker.Domain.Entities.GeneralLedger;
using Microbanker.Infrastructure.Persistence;

namespace Microbanker.Web.Services;

/// <summary>The bank's business day as the rest of the system sees it.</summary>
/// <param name="Initialised">False until the first start-of-day has been run; before that only the calendar date limits postings.</param>
public sealed record BusinessDay(bool Initialised, DateOnly Date, string Status, string? ClosedThroughPeriod)
{
    public bool IsOpen => !Initialised || Status == GLOptions.Open;
    public bool IsClosedPeriod(DateOnly valueDate)
        => ClosedThroughPeriod != null && string.CompareOrdinal(GLOptions.PeriodOf(valueDate), ClosedThroughPeriod) <= 0;
}

public static class BusinessCalendar
{
    /// <summary>Reads the business day without tracking it, so callers that are modifying it are not disturbed.</summary>
    public static async Task<BusinessDay> GetAsync(MicrobankerDbContext db, Guid orgId)
    {
        var p = await db.ProcessingDates.AsNoTracking().Where(x => x.OrganizationId == orgId && !x.IsDeleted).FirstOrDefaultAsync();
        return p == null
            ? new BusinessDay(false, DateOnly.FromDateTime(DateTime.UtcNow), GLOptions.Open, null)
            : new BusinessDay(true, p.CurrentDate, p.EODStatus, p.ClosedThroughPeriod);
    }
}
