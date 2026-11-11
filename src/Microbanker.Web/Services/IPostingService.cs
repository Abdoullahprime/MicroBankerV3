// src/Microbanker.Web/Services/IPostingService.cs
using Microbanker.Domain.Entities.Transactions;

namespace Microbanker.Web.Services;

/// <summary>One leg of a posting: money in or out of one member account.</summary>
public sealed record PostingLeg(string AccountNo, string DrCr, decimal Amount, string? Narration = null);

/// <summary>
/// A set of legs posted together, all or nothing.
///
/// Two shapes are accepted:
///  • balanced   — debits equal credits across the member accounts (a transfer or mixed posting); no contra GL;
///  • one-sided  — every leg goes the same way (a deposit, a withdrawal, a bulk credit); a contra GL account
///                 (cash, say) is then required and receives the other side.
/// </summary>
public sealed class PostingRequest
{
    public Guid OrganizationId { get; init; }
    public Guid PostedBy { get; init; }
    public DateOnly ValueDate { get; init; }
    public Guid? DebitCodeId { get; init; }
    public Guid? CreditCodeId { get; init; }
    public IReadOnlyList<PostingLeg> Legs { get; init; } = Array.Empty<PostingLeg>();
    /// <summary>Narration used for legs that do not carry their own.</summary>
    public string? Narration { get; init; }
    public string? ContraGlAccountNo { get; init; }
    public string? TellerRef { get; init; }
    /// <summary>
    /// Set when paying out an approved withdrawal request. The request is checked against the leg and marked
    /// completed in the same database transaction as the debit, so it cannot be paid twice.
    /// </summary>
    public Guid? WithdrawalRequestId { get; init; }
}

public sealed class PostingResult
{
    public bool Success { get; private init; }
    public string? Error { get; private init; }
    public IReadOnlyList<Transaction> Transactions { get; private init; } = Array.Empty<Transaction>();
    public string? BatchRef => Transactions.Count > 1 ? Transactions[0].BatchRef : null;

    public static PostingResult Ok(IReadOnlyList<Transaction> transactions) => new() { Success = true, Transactions = transactions };
    public static PostingResult Fail(string error) => new() { Success = false, Error = error };
}

public interface IPostingService
{
    /// <summary>Validates and posts the legs, updating account balances, atomically.</summary>
    Task<PostingResult> PostAsync(PostingRequest request);

    /// <summary>
    /// Reverses a posted transaction, or every leg of its batch, by posting offsetting transactions.
    /// A reversal that would break a balance rule (the credited money has been spent, say) is refused.
    /// </summary>
    Task<PostingResult> ReverseAsync(Guid organizationId, string transactionRef, Guid reversedBy, string reason);
}
