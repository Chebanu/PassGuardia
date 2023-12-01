using System.Security.Cryptography;

namespace PassGuardia.Domain.Algorithm;

public interface IEncryptor
{
    byte[] Encrypt(string plainText, byte[] key);
    string Decrypt(byte[] cipherText, byte[] key);
}

public class AesEncryptor : IEncryptor
{
    public string Decrypt(byte[] cipherText, byte[] key)
    {
        using var aes = Aes.Create();

        var iv = cipherText[^16..];
        var content = cipherText[..^16];

        using var memoryStream = new MemoryStream(content);
        using var decryptor = aes.CreateDecryptor(key, iv);
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var streamReader = new StreamReader(cryptoStream);

        return streamReader.ReadToEnd();
    }

    public byte[] Encrypt(string plainText, byte[] key)
    {
        using var aes = Aes.Create();
        aes.GenerateIV();
        var iv = aes.IV;

        using var memoryStream = new MemoryStream();
        using var encryptor = aes.CreateEncryptor(key, iv);
        using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        using var streamWriter = new StreamWriter(cryptoStream);

        streamWriter.Write(plainText);
        streamWriter.Flush();
        cryptoStream.FlushFinalBlock();
        memoryStream.Flush();

        return [.. memoryStream.ToArray(), .. iv];
    }
}