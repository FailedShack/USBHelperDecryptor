using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace USBHelperDecryptor
{
    class Program
    {
        private static readonly string[] V4 = new string[] { "Xl5CTkNKSE/CqMK1b2QyNg==", "Z15tYmtsIHR5XmwqZirCow==" };
        private static readonly string[] V6 = new string[] { "Z1be2dJkvBExY7b5tmLpVg==", "+hIiiu0yrVfpQJW9SnpcHg==" };

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Usage: USBHelperDecryptor.exe <encrypt or decrypt> <v4 or v6> <data file>");
                return;
            }
            string extension;
            bool decrypt;
            switch (args[0].ToLower())
            {
                case "encrypt":
                    decrypt = false;
                    extension = "enc";
                    break;
                case "decrypt":
                    decrypt = true;
                    extension = "zip";
                    break;
                default:
                    Console.WriteLine("Error: Invalid mode, use encrypt or decrypt.");
                    return;
            }
            string[] key;
            switch (args[1].ToLower())
            {
                case "v4":
                    key = V4;
                    break;
                case "v6":
                    key = V6;
                    break;
                default:
                    Console.WriteLine("Error: Invalid version, use v4 or v6.");
                    return;
            }
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string file = Path.Combine(path, args[2]);
            if (!File.Exists(file))
            {
                Console.WriteLine("Error: File not found.");
                return;
            }
            MemoryStream stream = new MemoryStream(File.ReadAllBytes(file));
            MemoryStream output;
            try
            {
                output = AesCrypt(stream, Convert.FromBase64String(key[0]), Convert.FromBase64String(key[1]), decrypt);
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("Error: An exception occurred while processing the file.\n" + e.Message);
                return;
            }
            string fileName = Path.GetFileNameWithoutExtension(args[2]);
            string decryptedFile;
            int i = 0;
            do
            {
                StringBuilder sb = new StringBuilder(fileName);
                if (i != 0)
                {
                    sb.Append("(" + i + ")");
                }
                sb.Append('.').Append(extension);
                decryptedFile = sb.ToString();
                i++;
            }
            while (File.Exists(Path.Combine(path, decryptedFile)));
            try
            {
                File.WriteAllBytes(Path.Combine(path, decryptedFile), output.ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: An exception occurred while writing the file.\n" + e.Message);
                return;
            }
            Console.WriteLine("Success: Processed file written to " + decryptedFile);
        }

        private static MemoryStream AesCrypt(MemoryStream source, byte[] key, byte[] iv, bool decrypt)
        {
            MemoryStream memoryStream = new MemoryStream();
            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.Mode = CipherMode.CBC;
                aes.Key = key;
                aes.IV = iv;
                byte[] buffer = new byte[512];
                using (CryptoStream cryptoStream = new CryptoStream(source, decrypt ? aes.CreateDecryptor() : aes.CreateEncryptor(), CryptoStreamMode.Read))
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
