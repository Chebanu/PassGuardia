using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PassGuardia.Contracts.Models;
using PassGuardia.Domain.Data;
using System.Security.Cryptography;
using System.Text;

namespace PassGuardia.Domain.Algorithm;

internal static class SymmentricAlgorithm
{
    public static Password CreatePassword(string password)
    {
        byte[] encrypted = null;
        byte[] iv = null;

        byte[] key = DataHandler.DataReader();

        try
        {
            using (AesManaged aes = new AesManaged())
            {
                if (key == null) key = aes.Key;
                iv = aes.IV;
                encrypted = Encrypt(password, key, iv);
            }
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.Message);
        }

        if (encrypted == null || key == null || iv == null)
        {
            throw new NullReferenceException();
        }

        DataHandler.DataWriter(key);

        var hashedPassword = new Password()
        {
            Id = Guid.NewGuid(),
            EncryptedPassword = Encoding.UTF8.GetString(encrypted),
            IV = Encoding.UTF8.GetString(iv)
        };

        return hashedPassword;
    }

    public static string Decrypt(string password, string iv)
    {
        var key = DataHandler.DataReader();

        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] ivBytes = Encoding.UTF8.GetBytes(iv);

        string plaintext = null;
        // Create AesManaged
        using (AesManaged aes = new AesManaged())
        {
            // Create a decryptor
            ICryptoTransform decryptor = aes.CreateDecryptor(key, ivBytes);
            // Create the streams used for decryption.
            using (MemoryStream ms = new MemoryStream(passwordBytes))
            {
                // Create crypto stream
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    // Read crypto stream
                    using (StreamReader reader = new StreamReader(cs))
                        plaintext = reader.ReadToEnd();
                }
            }
        }
        return plaintext;
    }


    static byte[] Encrypt(string plainText, byte[] Key, byte[] IV)
    {
        byte[] encrypted;
        using (AesManaged aes = new AesManaged())
        {
            ICryptoTransform encryptor = aes.CreateEncryptor(Key, IV);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                        sw.Write(plainText);
                    encrypted = ms.ToArray();
                }
            }
        }
        return encrypted;
    }
}
