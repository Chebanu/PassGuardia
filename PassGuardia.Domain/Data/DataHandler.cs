using PassGuardia.Domain.Constants;

namespace PassGuardia.Domain.Data;

internal static class DataHandler
{
    private static readonly string keyFilePath = Path.Combine(FilePath.FILE_PATH_DIR, "key.txt");

    public static void DataWriter(byte[] universalKey)
    {
        if (!File.Exists(keyFilePath))
        {
            using (StreamWriter keyWriter = new(keyFilePath))
            {
                keyWriter.WriteLine(Convert.ToBase64String(universalKey));
            }
        }
    }

    public static byte[]? DataReader()
    {
        string key = File.ReadAllText(keyFilePath);
        return Convert.FromBase64String(key);
    }
}