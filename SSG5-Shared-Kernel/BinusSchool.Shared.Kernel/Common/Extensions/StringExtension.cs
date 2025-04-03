using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace BinusSchool.Common.Extensions
{
    public static class StringExtension
    {
        public static string ToUpperFirst(this string input)
        {
            return input switch
            {
                null => null,
                "" => string.Empty,
                _ => char.ToUpper(input[0]) + input[1..]
            };
        
        }
        
        public static string ToLowerFirst(this string input)
        {
            return input switch
            {
                null => null,
                "" => string.Empty,
                _ => char.ToLower(input[0]) + input[1..]
            };
        }

        public static string GetInitialName(this string fullname)
        {
            // Extract the first character out of each block of non-whitespace
            // exept name suffixes, e.g. Jr., III. The number of initials is not limited.
            return Regex.Replace(fullname, @"(?i)(?:^|\s|-)+([^\s-])[^\s-]*(?:(?:\s+)(?:the\s+)?(?:jr|sr|II|2nd|III|3rd|IV|4th)\.?$)?", "$1").ToUpper();
        }

        public static KeyValuePair<string, object> WithValue(this string key, object value)
        {
            return KeyValuePair.Create(key, value);
        }

        public static KeyValuePair<string, T> WithValueOf<T>(this string key, T value)
        {
            return KeyValuePair.Create(key, value);
        }

        public static T DeserializeToDictionaryAndReturn<T>(this string source, string key, bool nullWhenNotFound = true)
            where T : class, new()
        {
            if (source is null)
                return nullWhenNotFound ? default : new T();
            var dict = JsonConvert.DeserializeObject<IDictionary<string, T>>(source);

            return dict.TryGetValue(key, out var result) ? result : nullWhenNotFound ? default : new T();
        }

        public static bool IsConstantOf<T>(this string source)
            where T : class
        {
            var data = new List<FieldInfo>();
            var fieldInfos = typeof(T).GetFields(
                // Gets all public and static fields
                BindingFlags.Public | BindingFlags.Static |
                // This tells it to get the fields from all base types as well
                BindingFlags.FlattenHierarchy);

            // Go through the list and only pick out the constants
            foreach (var fi in fieldInfos)
            {
                // IsLiteral determines if its value is written at 
                //   compile time and not changeable
                // IsInitOnly determine if the field can be set 
                //   in the body of the constructor
                // for C# a field which is readonly keyword would have both true 
                //   but a const field would have only IsLiteral equal to true
                if (fi.IsLiteral && !fi.IsInitOnly)
                    data.Add(fi);
            }

            return data.Where(x => x.GetValue(null).ToString() == source).Count() > 0;
        }

        /// <summary>
        /// Create new hash string using SHA256
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToSHA256(this string s)
        {
            using (SHA256 shaManager = new SHA256Managed())
            {
                string hash = string.Empty;
                byte[] bytes = shaManager.ComputeHash(Encoding.ASCII.GetBytes(s), 0, Encoding.ASCII.GetByteCount(s));
                foreach (byte b in bytes)
                {
                    hash += b.ToString("x2");
                }

                return hash;
            }
        }

        /// <summary>
        /// Create new hash string using SHA512
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToSHA512(this string s)
        {
            using (SHA512 shaManager = new SHA512Managed())
            {
                string hash = string.Empty;
                byte[] bytes = shaManager.ComputeHash(Encoding.ASCII.GetBytes(s), 0, Encoding.ASCII.GetByteCount(s));
                foreach (byte b in bytes)
                {
                    hash += b.ToString("x2");
                }

                return hash;
            }
        }

        public static string GetDescription<T>(this T enumValue) 
            where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                return null;

            var description = enumValue.ToString();
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            if (fieldInfo != null)
            {
                var attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (attrs != null && attrs.Length > 0)
                {
                    description = ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return description;
        }

        public static string GetPreviousAcademicYear(this string source, int tailLength)
        {
            if (tailLength >= source.Length)
                return source;

            int year = int.Parse(source.Substring(source.Length - tailLength));
            var school = source.Substring(0, source.Length - tailLength);
            year = year - 1;

            return $"{school}{year}";
        }
    }
}
