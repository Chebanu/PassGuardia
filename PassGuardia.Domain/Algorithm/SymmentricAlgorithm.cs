using PassGuardia.Contracts.Models;
using PassGuardia.Domain.Data;
using System.Security.Cryptography;
using System.Text;

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
            EncryptedPassword = encrypted,
            IV = iv
        };

        return hashedPassword;
    }

    private static byte[] Encrypt(string plainText, byte[] Key, byte[] IV)
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

        using (AesManaged aes = new AesManaged())
        {
            ICryptoTransform decryptor = aes.CreateDecryptor(key, iv);
            using (MemoryStream ms = new MemoryStream(password))
            {
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader reader = new StreamReader(cs))
                    {
                        plaintext = reader.ReadToEnd();
                    }
                }
            }
        }
        return plaintext;
    }
}