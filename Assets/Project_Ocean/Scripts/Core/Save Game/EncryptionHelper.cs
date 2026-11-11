using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Game.Testing
{
    public static class EncryptionHelper
    {
        // ❌ เอา hardcoded KEY/IV ออก
        // ✅ ใช้ PBKDF2 derive key+IV จาก passphrase + salt แทน
        // เปลี่ยน PASSPHRASE ให้เป็นของตัวเอง (ไม่ต้องครบ 16 ตัว)
        private const string PASSPHRASE = "MyGame_ChangeThis_2025";
        private const int    ITERATIONS  = 10_000;
        private const int    KEY_SIZE    = 32; // AES-256
        private const int    IV_SIZE     = 16;

        // Salt ไม่ต้องลับ แต่ต้องคงที่ระหว่าง encrypt/decrypt
        private static readonly byte[] SALT = Encoding.UTF8.GetBytes("SaveGame_Salt_v1");

        // -------------------------------------------------- //

        public static string Encrypt(string plain)
        {
            if (string.IsNullOrEmpty(plain))
                throw new ArgumentNullException(nameof(plain));

            DeriveKeyAndIV(out var key, out var iv);

            using var aes = Aes.Create();
            aes.Key  = key;
            aes.IV   = iv;
            aes.Mode = CipherMode.CBC;

            byte[] buf = Encoding.UTF8.GetBytes(plain);
            byte[] enc = aes.CreateEncryptor().TransformFinalBlock(buf, 0, buf.Length);
            return Convert.ToBase64String(enc);
        }

        public static string Decrypt(string cipher)
        {
            if (string.IsNullOrEmpty(cipher))
                throw new ArgumentNullException(nameof(cipher));

            DeriveKeyAndIV(out var key, out var iv);

            using var aes = Aes.Create();
            aes.Key  = key;
            aes.IV   = iv;
            aes.Mode = CipherMode.CBC;

            byte[] buf = Convert.FromBase64String(cipher);
            byte[] dec = aes.CreateDecryptor().TransformFinalBlock(buf, 0, buf.Length);
            return Encoding.UTF8.GetString(dec);
        }

        // -------------------------------------------------- //

        private static void DeriveKeyAndIV(out byte[] key, out byte[] iv)
        {
            using var deriv = new Rfc2898DeriveBytes(
                PASSPHRASE, SALT, ITERATIONS, HashAlgorithmName.SHA256);

            key = deriv.GetBytes(KEY_SIZE);
            iv  = deriv.GetBytes(IV_SIZE);
        }
    }
}
