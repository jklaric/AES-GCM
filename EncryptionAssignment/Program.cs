using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main()
    {
        bool shouldKeepRunning = true;

        while (shouldKeepRunning)
        {
            Console.WriteLine("1. Encrypt and save to a file");
            Console.WriteLine("2. Read and decrypt from a file");
            Console.WriteLine("3. Exit");

            string option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    EncryptAndSaveToFile();
                    break;
                case "2":
                    ReadAndDecryptFromFile();
                    break;
                case "3":
                    shouldKeepRunning = false;
                    break;
                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }
        }
    }

    static void EncryptAndSaveToFile()
    {
        Console.Write("Enter the passphrase (16 characters): ");
        string passphrase = Console.ReadLine();

        if (passphrase.Length != 16)
        {
            Console.WriteLine("Passphrase must be exactly 16 characters long.");
            return;
        }

        Console.Write("Enter the message to encrypt: ");
        string message = Console.ReadLine();
        Console.Write("Enter the output file name: ");
        string outputFile = Console.ReadLine();

        byte[] key = Encoding.UTF8.GetBytes(passphrase);
        byte[] nonce = new byte[12];

        using (AesGcm aesGcm = new AesGcm(key))
        {
            byte[] encryptedMessage = new byte[message.Length];
            byte[] tag = new byte[16];

            aesGcm.Encrypt(nonce, Encoding.UTF8.GetBytes(message), encryptedMessage, tag);

            using (FileStream fileStream = File.Create(outputFile))
            {
                fileStream.Write(nonce);
                fileStream.Write(tag);
                fileStream.Write(encryptedMessage);
            }
        }

        Console.WriteLine("Message encrypted and saved to file.");
    }

    static void ReadAndDecryptFromFile()
    {
        Console.Write("Enter the passphrase (16 characters): ");
        string passphrase = Console.ReadLine();

        if (passphrase.Length != 16)
        {
            Console.WriteLine("Passphrase must be exactly 16 characters long.");
            return;
        }

        Console.Write("Enter the input file name: ");
        string inputFile = Console.ReadLine();

        byte[] key = Encoding.UTF8.GetBytes(passphrase);
        byte[] nonce = new byte[12]; 

        try
        {
            using (AesGcm aesGcm = new AesGcm(key))
            using (FileStream fileStream = File.OpenRead(inputFile))
            {
                byte[] nonceRead = new byte[12];
                byte[] tag = new byte[16];
                byte[] encryptedMessage = new byte[fileStream.Length - 28]; 

                
                fileStream.Read(nonceRead, 0, 12);
                fileStream.Read(tag, 0, 16);
                fileStream.Read(encryptedMessage, 0, encryptedMessage.Length);

                byte[] decryptedMessage = new byte[encryptedMessage.Length];
                aesGcm.Decrypt(nonceRead, encryptedMessage, tag, decryptedMessage);

                string decryptedText = Encoding.UTF8.GetString(decryptedMessage);
                Console.WriteLine("Decrypted Message:");
                Console.WriteLine(decryptedText);
            }
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("File not found.");
        }
        catch (Exception)
        {
            Console.WriteLine("Decryption failed. Incorrect passphrase or corrupted file.");
        }
    }
}
