namespace AccountingERP.Infrastructure.Services;

using System.Security.Cryptography;
using System.Text;
using AccountingERP.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

/// <summary>
/// AES-256-GCM symmetric encryption service.
///
/// Ciphertext wire format: "{IV_base64}:{ciphertext_base64}:{tag_base64}"
///
/// A fresh 12-byte IV (nonce) is generated for every Encrypt call — reusing
/// a nonce with the same key under GCM is catastrophic, so we never do it.
/// The 16-byte authentication tag is stored alongside the ciphertext so that
/// Decrypt can verify authenticity before returning plaintext.
///
/// Key source: IConfiguration["Encryption:Key"] — a Base64-encoded 32-byte
/// (256-bit) key.  The key must never be stored in source control; use Azure
/// Key Vault, AWS Secrets Manager, or an equivalent HSM-backed store in production.
/// </summary>
public sealed class EncryptionService : IEncryptionService
{
    private const int IvSize  = 12;   // GCM recommended nonce length
    private const int TagSize = 16;   // AES-GCM authentication tag size

    private readonly byte[] _key;

    public EncryptionService(IConfiguration configuration)
    {
        var keyBase64 = configuration["Encryption:Key"]
            ?? throw new InvalidOperationException(
                "Encryption:Key nije konfigurisan. " +
                "Postavite ga kao environment varijablu ili u Azure Key Vault.");

        _key = Convert.FromBase64String(keyBase64);

        if (_key.Length != 32)
            throw new InvalidOperationException(
                "Encryption:Key mora biti 256-bitni ključ (32 bajta, Base64 enkodovan).");
    }

    // ── IEncryptionService ────────────────────────────────────────────────────

    /// <summary>
    /// Encrypts <paramref name="plaintext"/> with AES-256-GCM.
    /// Returns a string in the format "IV:ciphertext:tag" (all Base64).
    /// </summary>
    public string Encrypt(string plaintext)
    {
        ArgumentException.ThrowIfNullOrEmpty(plaintext, nameof(plaintext));

        var iv         = new byte[IvSize];
        RandomNumberGenerator.Fill(iv);

        var plaintextBytes  = Encoding.UTF8.GetBytes(plaintext);
        var ciphertextBytes = new byte[plaintextBytes.Length];
        var tag             = new byte[TagSize];

        using var aesGcm = new AesGcm(_key, TagSize);
        aesGcm.Encrypt(iv, plaintextBytes, ciphertextBytes, tag);

        return string.Concat(
            Convert.ToBase64String(iv),
            ":",
            Convert.ToBase64String(ciphertextBytes),
            ":",
            Convert.ToBase64String(tag));
    }

    /// <summary>
    /// Decrypts a value previously produced by <see cref="Encrypt"/>.
    /// Throws <see cref="CryptographicException"/> if the tag verification fails
    /// (tampered ciphertext or wrong key).
    /// </summary>
    public string Decrypt(string ciphertext)
    {
        ArgumentException.ThrowIfNullOrEmpty(ciphertext, nameof(ciphertext));

        var parts = ciphertext.Split(':');
        if (parts.Length != 3)
            throw new FormatException(
                "Neispravan format enkriptovanog teksta — očekivano 'IV:ciphertext:tag'.");

        var iv             = Convert.FromBase64String(parts[0]);
        var ciphertextBytes = Convert.FromBase64String(parts[1]);
        var tag            = Convert.FromBase64String(parts[2]);

        if (iv.Length != IvSize)
            throw new FormatException(
                $"Neispravan IV — dužina mora biti {IvSize} bajta.");

        if (tag.Length != TagSize)
            throw new FormatException(
                $"Neispravan tag — dužina mora biti {TagSize} bajta.");

        var plaintextBytes = new byte[ciphertextBytes.Length];

        using var aesGcm = new AesGcm(_key, TagSize);
        // AesGcm.Decrypt throws CryptographicException on tag mismatch.
        aesGcm.Decrypt(iv, ciphertextBytes, tag, plaintextBytes);

        return Encoding.UTF8.GetString(plaintextBytes);
    }
}
