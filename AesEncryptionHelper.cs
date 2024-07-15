using System.Security.Cryptography;

namespace PasswordWizard;

public class AesEncryptionHelper
{
    public static byte[] key;

    public static void EncryptIntoFile(string toEncrypt, string fileLocation)
    {
        try
        {
            using FileStream fileStream = new(fileLocation, FileMode.OpenOrCreate);
            using Aes aes = Aes.Create();

            aes.Key = key;

            byte[] iv = aes.IV;
            fileStream.Write(iv, 0, iv.Length);

            using CryptoStream cryptoStream = new(fileStream, aes.CreateEncryptor(), CryptoStreamMode.Write);

            using StreamWriter streamWriter = new(cryptoStream);
            streamWriter.WriteLine(toEncrypt);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Encryption failed: {e}");
        }
    }

    public static string DecryptFromFile(string fileLocation)
    {
        using FileStream fileStream = new(fileLocation, FileMode.Open);
        using Aes aes = Aes.Create();

        byte[] iv = new byte[aes.IV.Length];
        int numBytesToRead = aes.IV.Length;
        int numBytesRead = 0;
        while (numBytesToRead > 0)
        {
            int n = fileStream.Read(iv, numBytesRead, numBytesToRead);
            if (n == 0) { break; }

            numBytesRead += n;
            numBytesToRead -= n;
        }

        using CryptoStream cryptoStream = new(fileStream, aes.CreateDecryptor(key, iv), CryptoStreamMode.Read);
        using StreamReader streamReader = new(cryptoStream);
        string decryptedString = streamReader.ReadToEnd();
        return decryptedString;
    }

    public static byte[] GetKeyFromPassword(string password)
    {
        byte[] salt = [0x3, 0x1, 0x92, 0x82, 0x11, 0x0, 0x99];
        Rfc2898DeriveBytes pbkdf2 = new(password, salt, 100_000, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(32);
    }
}