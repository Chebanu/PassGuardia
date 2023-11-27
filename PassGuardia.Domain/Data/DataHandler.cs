using Microsoft.Extensions.Logging;
using PassGuardia.Domain.Constants;

namespace PassGuardia.Domain.Data;

internal static class DataHandler
{
    private static string keyFilePath = Path.Combine(FilePath.FILE_PATH_DIR, "key.txt");

    public static void DataWriter(byte[] universalKey)
    {
        if (!File.Exists(keyFilePath))
        {
            using (StreamWriter keyWriter = new StreamWriter(keyFilePath))
            {
                keyWriter.WriteLine(Convert.ToBase64String(universalKey));
            }
        }
    }

    public static byte[]? DataReader()
    {
        try
        {
            string key = File.ReadAllText(keyFilePath);
            return Convert.FromBase64String(key);
        }
        catch (FileNotFoundException)
        {
            //log.info
            //а нужно ли оно?*
            Console.WriteLine($"File {keyFilePath} not found.");
        }
        catch (Exception exp)
        {
            //log.error(exp.msg)
            //throw new Exception
            throw new Exception($"Error reading key from file");
        }
        return null;
    }
}
