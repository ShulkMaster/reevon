namespace Reevon.Api.Encryption;

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class KeyGenerator
{

    public static string GenerateKeyPair(string? id,string? secret)
    {
        using (RSA rsa = RSA.Create())
        {
            string publicKeyPem = ConvertToPemFormat(rsa.ExportRSAPublicKey());
            string privateKeyPem = ConvertToEncryptedPemFormat(rsa.ExportRSAPrivateKey(), secret);

            string keyFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Encryption/Keys");
            Directory.CreateDirectory(keyFolderPath);
            
            string privateKeyFilePath = Path.Combine(keyFolderPath, $"{id}.pem");

            KeyManagerStorage.SaveKeyPair( privateKeyPem, privateKeyFilePath, secret);

            return publicKeyPem;
        }
    }
    
    private static string ConvertToPemFormat(byte[] keyBytes)
    {
        StringBuilder builder = new StringBuilder();

        builder.AppendLine("-----BEGIN PUBLIC KEY-----");
        builder.AppendLine(Convert.ToBase64String(keyBytes));
        builder.AppendLine("-----END PUBLIC KEY-----");

        return builder.ToString();
    }

    private static string ConvertToEncryptedPemFormat(byte[] keyBytes, string? secret)
    {
        byte[] encryptedKeyBytes;

        using (var rsa = RSA.Create())
        {
            rsa.ImportRSAPrivateKey(keyBytes, out _);
            
            byte[] salt = GenerateRandomBytes(16); 
            byte[] iv = GenerateRandomBytes(16); 
            byte[] key = GenerateKey(secret, salt);

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (var encryptor = aes.CreateEncryptor())
                {
                    using (var outputStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write))
                        {
                            using (var binaryWriter = new BinaryWriter(cryptoStream))
                            {
                                binaryWriter.Write(iv);
                                binaryWriter.Write(rsa.ExportPkcs8PrivateKey());

                                binaryWriter.Flush();
                                cryptoStream.FlushFinalBlock();
                            }
                        }

                        encryptedKeyBytes = outputStream.ToArray();
                    }
                }
            }
        }

        StringBuilder builder = new StringBuilder();

        builder.AppendLine("-----BEGIN ENCRYPTED PRIVATE KEY-----");
        builder.AppendLine(Convert.ToBase64String(encryptedKeyBytes));
        builder.AppendLine("-----END ENCRYPTED PRIVATE KEY-----");

        return builder.ToString();
    }

    private static byte[] GenerateRandomBytes(int length)
    {
        byte[] randomBytes = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return randomBytes;
    }

    private static byte[] GenerateKey(string? secret, byte[] salt)
    {
        int iterations = 10000;
        int keySize = 256 / 8;

        using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(secret, salt, iterations, HashAlgorithmName.SHA256))
        {
            return rfc2898DeriveBytes.GetBytes(keySize);
        }
    }

}
