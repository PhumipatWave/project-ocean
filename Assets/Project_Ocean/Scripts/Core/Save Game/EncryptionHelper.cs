using System;
using System.Security.Cryptography;
using System.Text;

namespace Game.Testing
{
    public static class EncryptionHelper
    {
        const string KEY = "YourKey_16Chars!";  // ต้องครบ 16 ตัวพอดี
        const string IV  = "YourIV__16Chars!";  // ต้องครบ 16 ตัวพอดี

        public static string Encrypt(string plain)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(KEY);
            aes.IV = Encoding.UTF8.GetBytes(IV);
            byte[] buf = Encoding.UTF8.GetBytes(plain);
            byte[] res = aes.CreateEncryptor().TransformFinalBlock(buf, 0, buf.Length);
            return Convert.ToBase64String(res);
        }

        public static string Decrypt(string cipher)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(KEY);
            aes.IV = Encoding.UTF8.GetBytes(IV);
            byte[] buf = Convert.FromBase64String(cipher);
            byte[] res = aes.CreateDecryptor().TransformFinalBlock(buf, 0, buf.Length);
            return Encoding.UTF8.GetString(res);
        }
    }
}