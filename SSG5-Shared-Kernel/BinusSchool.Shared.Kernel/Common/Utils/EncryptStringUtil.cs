using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BinusSchool.Common.Utils
{
    public class EncryptStringUtil
    {
        private const string _stringKey = "New51S";

        /// <summary>
        /// Encrypt
        /// </summary>
        /// <param name="stringInputText"></param>
        /// <returns></returns>
        public static string Encrypt(string stringInputText)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(stringInputText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(_stringKey, new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
        });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    stringInputText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return stringInputText;
        }

        /// <summary>
        /// Encrypt
        /// </summary>
        /// <param name="stringInputText"></param>
        /// <param name="stringKey"></param>
        /// <returns></returns>
        public static string Encrypt(string stringInputText, string stringKey)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            byte[] plainText = System.Text.Encoding.Unicode.GetBytes(stringInputText);
            byte[] salt = Encoding.ASCII.GetBytes(stringKey.Length.ToString());
            PasswordDeriveBytes secretKey = new PasswordDeriveBytes(stringKey, salt);
            ICryptoTransform encryptor = rijndaelCipher.CreateEncryptor(secretKey.GetBytes(32), secretKey.GetBytes(16));
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainText, 0, plainText.Length);
            cryptoStream.FlushFinalBlock();
            byte[] cipherBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(cipherBytes);
        }

        /// <summary>
        /// Decrypt
        /// </summary>
        /// <param name="stringInputText"></param>
        /// <returns></returns>
        public static string Decrypt(string stringInputText)
        {
            stringInputText = stringInputText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(stringInputText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(_stringKey, new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
        });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    stringInputText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return stringInputText;
        }

        /// <summary>
        /// Decrypt
        /// </summary>
        /// <param name="stringInputText"></param>
        /// <param name="stringKey"></param>
        /// <returns></returns>
        public static string Decrypt(string stringInputText, string stringKey)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            stringInputText = stringInputText.Replace(" ", "+");
            byte[] encryptedData = Convert.FromBase64String(stringInputText);
            byte[] salt = Encoding.ASCII.GetBytes(stringKey.Length.ToString());
            PasswordDeriveBytes secretKey = new PasswordDeriveBytes(stringKey, salt);
            ICryptoTransform decryptor = rijndaelCipher.CreateDecryptor(secretKey.GetBytes(32), secretKey.GetBytes(16));

            using (MemoryStream memoryStream = new MemoryStream(encryptedData))
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
            using (MemoryStream resultStream = new MemoryStream())
            {
                cryptoStream.CopyTo(resultStream);
                byte[] decryptedData = resultStream.ToArray();
                string decryptedString = Encoding.Unicode.GetString(decryptedData);

                return decryptedString;
            }
        }
    }
}
