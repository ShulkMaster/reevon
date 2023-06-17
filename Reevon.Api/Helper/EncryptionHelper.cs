using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class EncryptionHelper
{
    public static string Encrypt(string plainText, string password)
    {
        using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
        {
            aesAlg.Key = GenerateKey(password, aesAlg.KeySize / 8);
            aesAlg.GenerateIV();

            ICryptoTransform encryptor = aesAlg.CreateEncryptor();

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            byte[] encryptedBytes;

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    csEncrypt.Write(plainBytes, 0, plainBytes.Length);
                    csEncrypt.FlushFinalBlock();
                }
                encryptedBytes = msEncrypt.ToArray();
            }

            return Convert.ToBase64String(encryptedBytes);
        }
    }

    public static string Decrypt(string encryptedText, string password)
    {
        byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

        using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
        {
            aesAlg.Key = GenerateKey(password, aesAlg.KeySize / 8);
            aesAlg.Padding = PaddingMode.PKCS7; // Set the padding mode explicitly

            byte[] iv = new byte[aesAlg.BlockSize / 8];
            Buffer.BlockCopy(encryptedBytes, 0, iv, 0, iv.Length);
            aesAlg.IV = iv;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor();

            byte[] decryptedBytes;

            using (MemoryStream msDecrypt = new MemoryStream(encryptedBytes, iv.Length, encryptedBytes.Length - iv.Length))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (MemoryStream msOutput = new MemoryStream())
                    {
                        csDecrypt.CopyTo(msOutput);
                        decryptedBytes = msOutput.ToArray();
                    }
                }
            }

            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }

    private static byte[] GenerateKey(string password, int keySize)
    {
        using (var derivedBytes = new Rfc2898DeriveBytes(password, 16, 1000))
        {
            return derivedBytes.GetBytes(keySize);
        }
    }
}
