using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace BinusSchool.Common.Utils
{
    public class AESCBCEncryptionUtil
    {
        private const int _keySize = 128;
        private const int _blockSize = 128;
        private const string _key = "V6jZK6PY6xjFU7Bj";
        private const string _iv = "n9baK7n8mjJBpxFR";

        public static byte[] EncryptAlgorithm(string plaintext)
        {
            byte[] encryptedBytes;

            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.KeySize = _keySize;
                aes.BlockSize = _blockSize;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                byte[] keyBytes = Encoding.UTF8.GetBytes(_key);
                byte[] ivBytes = Encoding.UTF8.GetBytes(_iv);

                aes.Key = keyBytes;
                aes.IV = ivBytes;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(plaintextBytes, 0, plaintextBytes.Length);
                        cryptoStream.FlushFinalBlock();
                    }

                    encryptedBytes = memoryStream.ToArray();
                }
            }

            return encryptedBytes;
        }

        public static byte[] DecryptAlgorithm(byte[] ciphertextBytes)
        {
            byte[] decryptedBytes;

            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.KeySize = _keySize;
                aes.BlockSize = _blockSize;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                byte[] keyBytes = Encoding.UTF8.GetBytes(_key);
                byte[] ivBytes = Encoding.UTF8.GetBytes(_iv);

                aes.Key = keyBytes;
                aes.IV = ivBytes;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(ciphertextBytes))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (MemoryStream resultStream = new MemoryStream())
                        {
                            cryptoStream.CopyTo(resultStream);
                            decryptedBytes = resultStream.ToArray();
                        }
                    }
                }
            }

            return decryptedBytes;
        }

        public static string Encrypt(string plaintext)
        {
            byte[] encryptedBytes = EncryptAlgorithm(plaintext);
            return Convert.ToBase64String(encryptedBytes);
        }

        public static string Decrypt(string ciphertext)
        {
            byte[] ciphertextBytes = Convert.FromBase64String(ciphertext);
            byte[] decryptedBytes = DecryptAlgorithm(ciphertextBytes);
            return Encoding.UTF8.GetString(decryptedBytes).TrimEnd('\0');
        }

        public static string EncryptBase64Url(string plaintext)
        {            
            byte[] encryptedBytes = EncryptAlgorithm(plaintext);
            return Base64UrlEncoder.Encode(encryptedBytes);
        }

        public static string DecryptBase64Url(string ciphertext)
        {
            byte[] ciphertextBytes = Base64UrlEncoder.DecodeBytes(ciphertext);
            byte[] decryptedBytes = DecryptAlgorithm(ciphertextBytes);
            return Encoding.UTF8.GetString(decryptedBytes).TrimEnd('\0');
        }
    }
}
