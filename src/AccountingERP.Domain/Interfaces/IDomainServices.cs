namespace AccountingERP.Domain.Interfaces;

using AccountingERP.Domain.Aggregates.Invoice;
using AccountingERP.Domain.Aggregates.JournalEntry;

public interface IHashService
{
    string ComputeJournalHash(JournalEntry entry, string? previousHash);
    string ComputeInvoiceHash(Invoice invoice);
    string ComputeJMBGHash(string jmbg);
    bool   VerifyHash(string content, string hash);
}

public interface IEncryptionService
{
    string Encrypt(string plaintext);
    string Decrypt(string ciphertext);
}
