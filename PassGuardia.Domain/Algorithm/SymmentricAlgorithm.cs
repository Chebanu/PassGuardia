using System.Security.Cryptography;

using PassGuardia.Contracts.Models;
using PassGuardia.Domain.Data;

namespace PassGuardia.Domain.Algorithm;

public static class SymmentricAlgorithm
{
    public static Password CreatePassword(string password)
    {
        byte[] encrypted = null;
        byte[] iv = null;

        byte[] key = DataHandler.DataReader();

        try
        {
            using (AesManaged aes = new())
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
            EncryptedPassword = encrypted,
            IV = iv
        };

        return hashedPassword;
    }

    private static byte[] Encrypt(string plainText, byte[] Key, byte[] IV)
    {
        byte[] encrypted;
        using (AesManaged aes = new())
        {
            ICryptoTransform encryptor = aes.CreateEncryptor(Key, IV);
            using (MemoryStream ms = new())
            {
                using (CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new(cs))
                    {
                        sw.Write(plainText);
                    }
                    encrypted = ms.ToArray();
                }
            }
        }
        return encrypted;
    }

    public static string Decrypt(byte[] password, byte[] iv)
    {
        var key = DataHandler.DataReader();

        string plaintext = null;

        using (AesManaged aes = new())
        {
            ICryptoTransform decryptor = aes.CreateDecryptor(key, iv);
            using (MemoryStream ms = new(password))
            {
                using (CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader reader = new(cs))
                    {
                        plaintext = reader.ReadToEnd();
                    }
                }
            }
        }
        return plaintext;
    }
}