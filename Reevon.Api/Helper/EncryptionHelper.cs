using System.Security.Cryptography;
using System.Text;

public static class EncryptionHelper
{
    private static readonly byte[] _fixedIv = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10 };

    public static string Encrypt(string plainText, string password)
    {
        using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
        {
            aesAlg.Key = GenerateKey(password, aesAlg.KeySize / 8);
            aesAlg.IV = _fixedIv;

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

            aesAlg.IV = _fixedIv;
            Buffer.BlockCopy(encryptedBytes, 0, aesAlg.IV, 0, aesAlg.IV.Length);

            ICryptoTransform decryptor = aesAlg.CreateDecryptor();

            byte[] decryptedBytes;

            using (MemoryStream msDecrypt = new MemoryStream(encryptedBytes, aesAlg.IV.Length, encryptedBytes.Length - aesAlg.IV.Length))
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

    private static byte[] GenerateKey(string password, int keySizeInBytes)
    {
        byte[] key = new byte[keySizeInBytes];
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

        for (int i = 0; i < keySizeInBytes; i++)
        {
            key[i] = passwordBytes[i % passwordBytes.Length];
        }

        return key;
    }
}
