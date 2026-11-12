// src/Microbanker.Web/Services/IProcessingService.cs
using Microbanker.Domain.Entities.GeneralLedger;

namespace Microbanker.Web.Services;

public sealed class ProcessingResult
{
    public bool Success { get; private init; }
    public string Message { get; private init; } = string.Empty;
    /// <summary>What the run did (counts, amounts) or what it skipped and why.</summary>
    public IReadOnlyList<string> Notes { get; private init; } = Array.Empty<string>();

    public static ProcessingResult Ok(string message, IEnumerable<string>? notes = null)
        => new() { Success = true, Message = message, Notes = (notes ?? Array.Empty<string>()).ToList() };
    public static ProcessingResult Fail(string message, IEnumerable<string>? notes = null)
        => new() { Success = false, Message = message, Notes = (notes ?? Array.Empty<string>()).ToList() };
}

/// <summary>What the year-end routine would close, shown before it is run.</summary>
public sealed class YearEndPreview
{
    public int Year { get; init; }
    public string? Problem { get; init; }
    public IReadOnlyList<(string GlAccountNo, string Name, string Type, decimal Balance)> Lines { get; init; }
        = Array.Empty<(string, string, string, decimal)>();
    /// <summary>Income less expenses; positive is a profit.</summary>
    public decimal NetProfit { get; init; }
}

public interface IProcessingService
{
    /// <summary>
    /// Start-of-day. The first run creates the business calendar on <paramref name="firstDate"/>; later runs open the
    /// current date after an end-of-day.
    /// </summary>
    Task<ProcessingResult> StartDayAsync(Guid orgId, Guid userId, DateOnly? firstDate);

    /// <summary>End-of-day: checks the ledger balances, accrues interest, flags dormant accounts and moves to the next date.</summary>
    Task<ProcessingResult> RunEodAsync(Guid orgId, Guid userId);

    /// <summary>Month-end for the oldest unclosed month: pays accrued interest, resets monthly counters, closes the period.</summary>
    Task<ProcessingResult> RunEomAsync(Guid orgId, Guid userId);

    Task<YearEndPreview> PreviewEoyAsync(Guid orgId);

    /// <summary>Year-end: closes income and expenses into the chosen retained-earnings account.</summary>
    Task<ProcessingResult> RunEoyAsync(Guid orgId, Guid userId, string? retainedEarningsGlNo);
}
