namespace Reevon.Api.Encryption;

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class KeyManagerStorage
{
    public static void SaveKeyPair(string privateKey, string privateKeyFilePath, string? password)
    {
        SaveEncryptedPrivateKey(privateKey, privateKeyFilePath, password);
    }

    private static void SaveEncryptedPrivateKey(string privateKey, string privateKeyFilePath, string? password)
    {
        byte[] privateKeyBytes = Encoding.UTF8.GetBytes(privateKey);
        byte[] ivBytes;

        using (Aes aes = Aes.Create())
        {
            aes.GenerateIV();
            aes.GenerateKey();

            byte[] salt = new byte[16];
            RandomNumberGenerator.Fill(salt);

            int iterations = 10000;
            using (Rfc2898DeriveBytes keyDerivation = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                byte[] keyBytes = keyDerivation.GetBytes(32);
                ivBytes = aes.IV;

                using (ICryptoTransform encryptor = aes.CreateEncryptor(keyBytes, ivBytes))
                {
                    using (MemoryStream encryptedStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(encryptedStream, encryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(privateKeyBytes, 0, privateKeyBytes.Length);
                            cryptoStream.FlushFinalBlock();
                        }

                        byte[] encryptedKeyBytes = encryptedStream.ToArray();

                        byte[] resultBytes = new byte[salt.Length + ivBytes.Length + encryptedKeyBytes.Length];
                        Buffer.BlockCopy(salt, 0, resultBytes, 0, salt.Length);
                        Buffer.BlockCopy(ivBytes, 0, resultBytes, salt.Length, ivBytes.Length);
                        Buffer.BlockCopy(encryptedKeyBytes, 0, resultBytes, salt.Length + ivBytes.Length, encryptedKeyBytes.Length);

                        File.WriteAllBytes(privateKeyFilePath, resultBytes);
                    }
                }
            }
        }
    }
}
