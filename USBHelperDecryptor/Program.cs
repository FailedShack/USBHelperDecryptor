using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace USBHelperDecryptor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: USBHelperDecryptor.exe <datav4 file>");
                return;
            }
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string file = Path.Combine(path, args[0]);
            if (!File.Exists(file))
            {
                Console.WriteLine("Error: File not found.");
                return;
            }
            MemoryStream stream = new MemoryStream(File.ReadAllBytes(file));
            MemoryStream decrypted;
            ZipArchive archive;
            try
            {
                decrypted = DecryptArchive(stream);
                archive = new ZipArchive(decrypted);
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid datav4 file.");
                return;
            }
            Console.WriteLine("Archive decrypted successfully, contains " + archive.Entries.Count + " files.");
            string decryptedFile = Path.GetFileNameWithoutExtension(args[0]) + "_decrypted.zip";
            File.WriteAllBytes(Path.Combine(path, decryptedFile), decrypted.ToArray());
            Console.WriteLine("Decrypted contents written to " + decryptedFile);
        }

        // Key taken from NusHelper.dll in Wii U USB Helper GO! APK
        private static MemoryStream DecryptArchive(MemoryStream source)
        {
            MemoryStream memoryStream = new MemoryStream();
            using (AesCryptoServiceProvider cryptoServiceProvider = new AesCryptoServiceProvider())
            {
                cryptoServiceProvider.Mode = CipherMode.CBC;
                cryptoServiceProvider.Key = Encoding.UTF8.GetBytes("^^BNCJHO¨µod26");
                cryptoServiceProvider.IV = Encoding.UTF8.GetBytes("g^mbkl ty^l*f*£");
                byte[] buffer = new byte[512];
                using (CryptoStream cryptoStream = new CryptoStream(source, cryptoServiceProvider.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    int count;
                    do
                    {
                        count = cryptoStream.Read(buffer, 0, 512);
                        memoryStream.Write(buffer, 0, count);
                    }
                    while (count > 0);
                }
            }
            return memoryStream;
        }
    }
}
