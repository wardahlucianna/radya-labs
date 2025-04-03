using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace BinusSchool.User.FnUser.Utils
{
    public static class Generator
    {
        public static string GenerateRandomPassword(int size)
        {
            var random = new Random();
            var letters = "abcdefghijklmnopqrstuvwxyz";
            var numerics = "0123456789";
            var specials = "!@#$%^&*_?><";
            var reserved = new Dictionary<int, char>();

            while (reserved.Count < 3)
            {
                int key = random.Next(1, size);
                switch (reserved.Count)
                {
                    case 0:
                        if (!reserved.ContainsKey(key))
                            reserved.Add(key, letters.ToUpper()[random.Next(0, letters.Length - 1)]);
                        break;
                    case 1:
                        if (!reserved.ContainsKey(key))
                            reserved.Add(key, numerics[random.Next(0, numerics.Length - 1)]);
                        break;
                    case 2:
                        if (!reserved.ContainsKey(key))
                            reserved.Add(key, specials[random.Next(0, specials.Length - 1)]);
                        break;
                    default:
                        break;
                }
            }

            var builder = new StringBuilder();
            for (int i = 1; i <= size; i++)
            {
                if (reserved.ContainsKey(i))
                    builder.Append(reserved[i]);
                else
                    builder.Append(letters[random.Next(0, letters.Length - 1)]);
            }
            return builder.ToString();
        }

        public static byte[] GenerateSalt()
        {
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                var length = 128;
                const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890-_";

                var data = new byte[length];

                // If chars.Length isn't a power of 2 then there is a bias if we simply use the modulus operator. The first characters of chars will be more probable than the last ones.
                // buffer used if we encounter an unusable random byte. We will regenerate it in this buffer
                byte[] buffer = null;

                // Maximum random number that can be used without introducing a bias
                var maxRandom = byte.MaxValue - ((byte.MaxValue + 1) % chars.Length);

                crypto.GetBytes(data);

                var result = new char[length];

                for (int i = 0; i < length; i++)
                {
                    var value = data[i];

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

                return Encoding.Default.GetBytes(new string(result));
            }
        }
    }
}
