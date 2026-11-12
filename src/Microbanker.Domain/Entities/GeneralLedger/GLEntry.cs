namespace Microbanker.Domain.Entities.GeneralLedger;

/// <summary>
/// One line of a journal in the general ledger. The lines of a journal (same <see cref="JournalRef"/>) always
/// have equal debits and credits. Entries are never edited: a reversal adds mirror-image lines and marks the
/// originals REVERSED. Balances therefore sum every entry whatever its status.
/// </summary>
public class GLEntry : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string JournalRef { get; private set; } = string.Empty;
    public string GLAccountNo { get; private set; } = string.Empty;
    public string DrCr { get; private set; } = GLOptions.Debit;
    public decimal Amount { get; private set; }
    public string Narration { get; private set; } = string.Empty;
    public DateOnly ValueDate { get; private set; }
    public DateTime PostingDate { get; private set; }
    public Guid PostedBy { get; private set; }
    /// <summary>TELLER, LOAN or SYSTEM.</summary>
    public string SourceModule { get; private set; } = GLOptions.Teller;
    /// <summary>The transaction reference (or other source document) that produced this line.</summary>
    public string SourceRef { get; private set; } = string.Empty;
    /// <summary>"YYYY-MM" of the value date.</summary>
    public string FiscalPeriod { get; private set; } = string.Empty;
    public string Status { get; private set; } = GLOptions.Posted;

    private GLEntry() { }

    public GLEntry(Guid orgId, string journalRef, string glAccountNo, string drCr, decimal amount, string narration,
        DateOnly valueDate, DateTime postingDate, Guid postedBy, string sourceModule, string sourceRef)
    {
        if (amount <= 0 || decimal.Round(amount, 2) != amount)
            throw new ArgumentOutOfRangeException(nameof(amount), "The amount must be greater than zero with at most two decimal places.");
        if (drCr != GLOptions.Debit && drCr != GLOptions.Credit)
            throw new ArgumentOutOfRangeException(nameof(drCr), drCr, "DrCr must be DR or CR.");
        if (!GLOptions.SourceModules.Contains(sourceModule))
            throw new ArgumentOutOfRangeException(nameof(sourceModule), sourceModule, "Unknown source module.");

        OrganizationId = orgId;
        JournalRef = journalRef;
        GLAccountNo = glAccountNo;
        DrCr = drCr;
        Amount = amount;
        Narration = narration;
        ValueDate = valueDate;
        PostingDate = postingDate;
        PostedBy = postedBy;
        SourceModule = sourceModule;
        SourceRef = sourceRef;
        FiscalPeriod = GLOptions.PeriodOf(valueDate);
        Status = GLOptions.Posted;
    }

    /// <summary>The line that offsets this one: same account and amount, opposite side.</summary>
    public GLEntry Mirror(string journalRef, string sourceRef, string narration, DateOnly valueDate, DateTime postingDate, Guid postedBy)
        => new(OrganizationId, journalRef, GLAccountNo, DrCr == GLOptions.Debit ? GLOptions.Credit : GLOptions.Debit, Amount,
            narration, valueDate, postingDate, postedBy, SourceModule, sourceRef);

    public void MarkReversed()
    {
        if (Status != GLOptions.Posted)
            throw new InvalidOperationException("Only a posted entry can be marked reversed.");
        Status = GLOptions.Reversed;
        SetUpdatedAt();
    }
}
