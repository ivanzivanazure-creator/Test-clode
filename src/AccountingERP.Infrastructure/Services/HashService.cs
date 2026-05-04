namespace AccountingERP.Infrastructure.Services;

using System.Security.Cryptography;
using System.Text;
using AccountingERP.Domain.Aggregates.Invoice;
using AccountingERP.Domain.Aggregates.JournalEntry;
using AccountingERP.Domain.Interfaces;

/// <summary>
/// SHA-256–based hash service.
/// Journal entry hashes form a tamper-evident chain as required by
/// the Serbian Law on Accounting (Zakon o računovodstvu, Art. 8).
/// </summary>
public sealed class HashService : IHashService
{
    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Computes a deterministic SHA-256 hash for a journal entry.
    /// The hash includes the previous entry's hash so that the entire chain
    /// is invalidated if any earlier entry is tampered with.
    /// Format: "{Number}|{Date:yyyy-MM-dd}|{TotalDebit.Amount}|{previousHash}"
    /// </summary>
    public string ComputeJournalHash(JournalEntry entry, string? previousHash)
    {
        var content = string.Concat(
            entry.Number,
            "|",
            entry.Date.ToString("yyyy-MM-dd"),
            "|",
            entry.TotalDebit.Amount.ToString("F4"),
            "|",
            previousHash ?? "GENESIS");

        return ComputeSha256(content);
    }

    /// <summary>
    /// Computes a SHA-256 hash over the immutable fields of an invoice
    /// (number, issue date, client, and total amount with currency).
    /// </summary>
    public string ComputeInvoiceHash(Invoice invoice)
    {
        var content = string.Concat(
            invoice.Number,
            "|",
            invoice.IssueDate.ToString("yyyy-MM-dd"),
            "|",
            invoice.ClientId.ToString(),
            "|",
            invoice.TotalAmount.Amount.ToString("F4"),
            "|",
            invoice.TotalAmount.Currency);

        return ComputeSha256(content);
    }

    /// <summary>
    /// Computes a SHA-256 hash of a raw JMBG string.
    /// The hash is stored in the database so the JMBG can be looked up
    /// without decrypting the encrypted copy, reducing key-usage frequency.
    /// </summary>
    public string ComputeJMBGHash(string jmbg)
    {
        if (string.IsNullOrWhiteSpace(jmbg))
            throw new ArgumentException("JMBG ne sme biti prazan.", nameof(jmbg));

        return ComputeSha256(jmbg.Trim());
    }

    /// <summary>
    /// Recomputes the SHA-256 hash of <paramref name="content"/> and compares it
    /// with <paramref name="hash"/> using a constant-time comparison to prevent
    /// timing attacks.
    /// </summary>
    public bool VerifyHash(string content, string hash)
    {
        if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(hash))
            return false;

        var computed = ComputeSha256(content);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computed),
            Encoding.UTF8.GetBytes(hash));
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static string ComputeSha256(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash  = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
