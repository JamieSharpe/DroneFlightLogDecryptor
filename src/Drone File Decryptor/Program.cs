using System.Security.Cryptography;
using System.Text;

namespace Drone_File_Decryptor;
internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("Yuneec Drone Flight Log Decryptor.");
        Console.WriteLine("Version 0.1");

        Console.WriteLine("Enter folder path containing encrypted files:");

        string encryptedFolder = Console.ReadLine() ?? "";

        if (string.IsNullOrEmpty(encryptedFolder) || !Directory.Exists(encryptedFolder))
        {
            Console.WriteLine("Encrypted folder path is not valid.");
            return;
        }

        Console.WriteLine("Enter folder path for decrypted files:");

        string outputFolder = Console.ReadLine() ?? "";

        if (string.IsNullOrEmpty(encryptedFolder) || !Directory.Exists(encryptedFolder))
        {
            Console.WriteLine("Output folder path is not valid.");
            return;
        }

        Console.WriteLine("Decrypting Files:");

        foreach (var filePath in Directory.EnumerateFiles(encryptedFolder))
        {
            string fileName = Path.GetFileName(filePath);

            Console.WriteLine($"\tDecrypting file: {fileName}");

            var decryptedFileContent = await DecryptFile(filePath, "ksYuN2eC", "ksYuN2eC");

            string logFileContent = Encoding.UTF8.GetString(decryptedFileContent);

            /// The log file CSV header is missing the first attribute so we add it manually.
            logFileContent = "time" + logFileContent;

            string outputFilePath = Path.Combine(outputFolder, fileName);

            File.WriteAllText(outputFilePath, logFileContent);
        }

        Console.WriteLine("Decryption complete.");
    }

    /// <summary>
    /// Given a file path, decrypts a file with DES encryption key and IV.
    /// </summary>
    /// <param name="path">File path to encrypted data.</param>
    /// <param name="key">DES key.</param>
    /// <param name="iv">DES IV.</param>
    /// <returns></returns>
    public static async Task<byte[]> DecryptFile(string path, string key = "ksYuN2eC", string iv = "ksYuN2eC")
    {
        var fileCipherBytes = await File.ReadAllBytesAsync(path);

        var filePlainTextBytes = await DecryptBytes(fileCipherBytes, key, iv);

        return filePlainTextBytes;
    }

    /// <summary>
    /// Decrypt a byte array with DES using a password and IV.
    /// </summary>
    /// <param name="cipher">Cipher as a byte array.</param>
    /// <param name="key">DES key.</param>
    /// <param name="iv">DES IV.</param>
    /// <returns></returns>
    public static async Task<byte[]> DecryptBytes(byte[] cipher, string key, string iv)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] ivBytes = Encoding.UTF8.GetBytes(iv);

        using DESCryptoServiceProvider des = new DESCryptoServiceProvider();
        des.Key = keyBytes;
        des.IV = ivBytes;
        des.Padding = PaddingMode.PKCS7;
        des.Mode = CipherMode.CBC;

        using MemoryStream ms = new MemoryStream();
        using CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
        cs.Write(cipher, 0, cipher.Length);
        cs.FlushFinalBlock();
        byte[] plainText = ms.ToArray();
        return plainText;
    }
}
