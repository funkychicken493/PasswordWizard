using System.Security.Cryptography;

namespace PasswordWizard;

public class AesEncryptionHelper
{
    public static byte[] key;
    public static void TestEncryption()
    {
        try
        {
            using (FileStream fileStream = new("TestData.txt", FileMode.OpenOrCreate))
            {
                using Aes aes = Aes.Create();

                aes.Key = key;

                byte[] iv = aes.IV;
                fileStream.Write(iv, 0, iv.Length);

                using CryptoStream cryptoStream = new(
                    fileStream,
                    aes.CreateEncryptor(),
                    CryptoStreamMode.Write);
                // By default, the StreamWriter uses UTF-8 encoding.
                // To change the text encoding, pass the desired encoding as the second parameter.
                // For example, new StreamWriter(cryptoStream, Encoding.Unicode).
                using StreamWriter encryptWriter = new(cryptoStream);
                encryptWriter.WriteLine("Hello World!");
            }

            Console.WriteLine("The file was encrypted.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"The encryption failed. {ex}");
        }
    }

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
        try
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
        catch (Exception e)
        {
            Console.WriteLine($"Decryption failed: {e}");
            return "null";
        }
    }

    public async static Task TestDecryption()
    {
        Console.WriteLine("yeah yeah im doing the decryption gimme a sec.");
        try
        {
            using FileStream fileStream = new("TestData.txt", FileMode.Open);
            using Aes aes = Aes.Create();
            byte[] iv = new byte[aes.IV.Length];
            int numBytesToRead = aes.IV.Length;
            int numBytesRead = 0;
            while (numBytesToRead > 0)
            {
                int n = fileStream.Read(iv, numBytesRead, numBytesToRead);
                if (n == 0) break;

                numBytesRead += n;
                numBytesToRead -= n;
            }

            using CryptoStream cryptoStream = new(
               fileStream,
               aes.CreateDecryptor(key, iv),
               CryptoStreamMode.Read);
            // By default, the StreamReader uses UTF-8 encoding.
            // To change the text encoding, pass the desired encoding as the second parameter.
            // For example, new StreamReader(cryptoStream, Encoding.Unicode).
            using StreamReader decryptReader = new(cryptoStream);
            string decryptedMessage = await decryptReader.ReadToEndAsync();
            Console.WriteLine($"The decrypted original message: {decryptedMessage}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"The decryption failed. {ex}");
        }
        Console.WriteLine("ok im all done with decryption");
    }

    public static byte[] GetKeyFromPassword(string password)
    {
        byte[] salt = [0x3, 0x1, 0x92, 0x82, 0x11, 0x0, 0x99];
        Rfc2898DeriveBytes pbkdf2 = new(password, salt, 100_000, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(32);
    }
}
