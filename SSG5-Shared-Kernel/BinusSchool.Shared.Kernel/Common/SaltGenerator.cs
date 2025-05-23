﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace BinusSchool.Common
{
    public static class SaltGenerator
    {
        /// <summary>
        /// Get random string, used for unique/salt
        /// </summary>
        /// <remarks>
        /// Source from https://blog.bitscry.com/2018/04/13/cryptographically-secure-random-string/
        /// </remarks>
        /// <returns></returns>
        public static string GetSalt()
        {
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                int length = 128;
                const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-_";

                byte[] data = new byte[length];

                // If chars.Length isn't a power of 2 then there is a bias if we simply use the modulus operator. The first characters of chars will be more probable than the last ones.
                // buffer used if we encounter an unusable random byte. We will regenerate it in this buffer
                byte[] buffer = null;

                // Maximum random number that can be used without introducing a bias
                int maxRandom = byte.MaxValue - ((byte.MaxValue + 1) % chars.Length);

                crypto.GetBytes(data);

                char[] result = new char[length];

                for (int i = 0; i < length; i++)
                {
                    byte value = data[i];

                    while (value > maxRandom)
                    {
                        if (buffer == null)
                        {
                            buffer = new byte[1];
                        }

                        crypto.GetBytes(buffer);
                        value = buffer[0];
                    }

                    result[i] = chars[value % chars.Length];
                }

                return new string(result);
            }
        }

        /// <summary>
        /// Get byte with default encoding from GetSalt()
        /// </summary>
        /// <returns></returns>
        public static byte[] GetEncodedSalt()
        {
            return Encoding.Default.GetBytes(GetSalt());
        }
    }
}
