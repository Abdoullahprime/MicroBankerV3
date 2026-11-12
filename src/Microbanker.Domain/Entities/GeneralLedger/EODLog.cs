namespace Microbanker.Domain.Entities.GeneralLedger;

/// <summary>A record of one start-of-day / end-of-day / month-end / year-end run, successful or not.</summary>
public class EODLog : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public DateOnly ProcessDate { get; private set; }
    /// <summary>SOD, EOD, EOM or EOY.</summary>
    public string ProcessType { get; private set; } = GLOptions.Eod;
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    /// <summary>RUNNING, COMPLETED or FAILED.</summary>
    public string Status { get; private set; } = GLOptions.Running;
    public Guid RunBy { get; private set; }
    public string? Notes { get; private set; }

    private EODLog() { }

    public EODLog(Guid orgId, DateOnly processDate, string processType, Guid runBy, DateTime? startedAt = null)
    {
        if (!GLOptions.ProcessTypes.Contains(processType))
            throw new ArgumentOutOfRangeException(nameof(processType), processType, "Unknown process type.");
        OrganizationId = orgId;
        ProcessDate = processDate;
        ProcessType = processType;
        RunBy = runBy;
        StartedAt = startedAt ?? DateTime.UtcNow;
        Status = GLOptions.Running;
    }

    public void Complete(string? notes)
    {
        Status = GLOptions.Completed;
        CompletedAt = DateTime.UtcNow;
        Notes = Trim(notes);
        SetUpdatedAt();
    }

    public void Fail(string? notes)
    {
        Status = GLOptions.Failed;
        CompletedAt = DateTime.UtcNow;
        Notes = Trim(notes);
        SetUpdatedAt();
    }

    private static string? Trim(string? s) => s is { Length: > 2000 } ? s[..2000] : s;
}
