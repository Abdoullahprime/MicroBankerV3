// src/BaseTemplate.Web/Services/AesEncryptionService.cs
using System.Security.Cryptography;
using System.Text;

namespace BaseTemplate.Web.Services;

public class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public AesEncryptionService(IConfiguration configuration)
    {
        var raw = configuration["EncryptionKey"]
            ?? throw new InvalidOperationException("EncryptionKey is not configured.");

        // Derive a 32-byte key from whatever string is provided
        using var sha = SHA256.Create();
        _key = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
    }

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Prepend IV so we can decrypt later: [16 bytes IV][cipher bytes]
        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(result)
            .Replace('+', '-').Replace('/', '_').Replace("=", "");
    }

    public string Decrypt(string cipherText)
    {
        // Restore Base64Url → Base64
        var base64 = cipherText.Replace('-', '+').Replace('_', '/');
        var padded = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');

        var allBytes = Convert.FromBase64String(padded);

        using var aes = Aes.Create();
        aes.Key = _key;

        var iv = new byte[16];
        Buffer.BlockCopy(allBytes, 0, iv, 0, 16);
        aes.IV = iv;

        var cipherBytes = new byte[allBytes.Length - 16];
        Buffer.BlockCopy(allBytes, 16, cipherBytes, 0, cipherBytes.Length);

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }
}
