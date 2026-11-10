// src/BaseTemplate.Web/Services/IEncryptionService.cs
namespace BaseTemplate.Web.Services;

public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
