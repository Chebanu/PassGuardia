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
            Console.WriteLine($"File {keyFilePath} not found.");
        }
        catch (Exception exp)
        {
            Console.WriteLine($"Error reading key from file: {exp.Message}");
        }
        return null;
    }
}
