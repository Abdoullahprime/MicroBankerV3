namespace Microbanker.Domain.Entities.GeneralLedger;

/// <summary>
/// The bank's business day. One row per organisation, created when the first start-of-day is run.
/// Until then postings are only limited by the calendar date; afterwards they need an OPEN day, may not be
/// dated after <see cref="CurrentDate"/>, and may not fall in a closed fiscal period.
/// </summary>
public class ProcessingDate : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public DateOnly CurrentDate { get; private set; }
    public DateTime? LastSODAt { get; private set; }
    public DateTime? LastEODAt { get; private set; }
    public DateTime? LastEOMAt { get; private set; }
    public DateTime? LastEOYAt { get; private set; }
    /// <summary>OPEN, EOD_RUNNING or CLOSED.</summary>
    public string EODStatus { get; private set; } = GLOptions.Closed;
    /// <summary>The last fiscal period ("YYYY-MM") closed by month-end processing; nothing may be posted into it.</summary>
    public string? ClosedThroughPeriod { get; private set; }

    /// <summary>Two users starting end-of-day together cannot both succeed.</summary>
    public Guid ConcurrencyStamp { get; private set; } = Guid.NewGuid();

    private ProcessingDate() { }

    /// <summary>Creates the business calendar, with the first day not yet open.</summary>
    public ProcessingDate(Guid orgId, DateOnly firstDate)
    {
        OrganizationId = orgId;
        CurrentDate = firstDate;
        EODStatus = GLOptions.Closed;
    }

    public bool IsOpen => EODStatus == GLOptions.Open;

    /// <summary>True when the date falls in a fiscal period that has been closed.</summary>
    public bool IsPeriodClosed(DateOnly valueDate)
        => ClosedThroughPeriod != null && string.CompareOrdinal(GLOptions.PeriodOf(valueDate), ClosedThroughPeriod) <= 0;

    public void StartDay()
    {
        if (EODStatus != GLOptions.Closed)
            throw new InvalidOperationException("The day is already open or end-of-day is running.");
        EODStatus = GLOptions.Open;
        LastSODAt = DateTime.UtcNow;
        Touch();
    }

    public void BeginEod()
    {
        if (EODStatus != GLOptions.Open)
            throw new InvalidOperationException("End-of-day can only run on an open day.");
        EODStatus = GLOptions.EodRunning;
        Touch();
    }

    /// <summary>Finishes the day: the calendar moves on to the next date, which stays closed until start-of-day.</summary>
    public void CompleteEod()
    {
        if (EODStatus != GLOptions.EodRunning)
            throw new InvalidOperationException("End-of-day is not running.");
        CurrentDate = CurrentDate.AddDays(1);
        EODStatus = GLOptions.Closed;
        LastEODAt = DateTime.UtcNow;
        Touch();
    }

    public void CompleteEom(string period)
    {
        if (EODStatus != GLOptions.Closed)
            throw new InvalidOperationException("Month-end can only run while the day is closed.");
        if (ClosedThroughPeriod != null && string.CompareOrdinal(period, ClosedThroughPeriod) <= 0)
            throw new InvalidOperationException($"Period {period} is already closed.");
        ClosedThroughPeriod = period;
        LastEOMAt = DateTime.UtcNow;
        Touch();
    }

    public void CompleteEoy()
    {
        LastEOYAt = DateTime.UtcNow;
        Touch();
    }

    private void Touch()
    {
        SetUpdatedAt();
        ConcurrencyStamp = Guid.NewGuid();
    }
}
