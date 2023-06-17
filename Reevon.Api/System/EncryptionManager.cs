using System.Security.Cryptography;

namespace Reevon.Api.System
{
    public class EncryptionManager
    {
        public static string Encrypt(string plainText, string password)
        {
            byte[] salt = GenerateSalt();
            byte[] key = GenerateKey(password, salt);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    memoryStream.Write(aes.IV, 0, aes.IV.Length);

                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                    {
                        streamWriter.Write(plainText);
                    }

                    byte[] encryptedData = memoryStream.ToArray();
                    return Convert.ToBase64String(encryptedData);
                }
            }
        }

        public static string Decrypt(string encryptedText, string password)
        {
            byte[] encryptedData = Convert.FromBase64String(encryptedText);
            byte[] salt = GenerateSalt();
            byte[] key = GenerateKey(password, salt);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;

                byte[] iv = new byte[aes.IV.Length];
                Buffer.BlockCopy(encryptedData, 0, iv, 0, aes.IV.Length);
                aes.IV = iv;


                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(encryptedData))
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                using (StreamReader streamReader = new StreamReader(cryptoStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        private static byte[] GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(salt);
            }
            return salt;
        }

        private static byte[] GenerateKey(string password, byte[] salt)
        {
            using (Rfc2898DeriveBytes keyGenerator = new Rfc2898DeriveBytes(password, salt, 10000))
            {
                return keyGenerator.GetBytes(32); 
            }
        }
    }
}
