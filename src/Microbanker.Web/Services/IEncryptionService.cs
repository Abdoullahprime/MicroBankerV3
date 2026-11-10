// src/Microbanker.Web/Services/IEncryptionService.cs
namespace Microbanker.Web.Services;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
