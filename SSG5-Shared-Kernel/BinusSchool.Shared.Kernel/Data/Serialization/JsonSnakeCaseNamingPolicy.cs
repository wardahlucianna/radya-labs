using System;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace BinusSchool.Data.Serialization
{
    /// <summary>
    /// Snake Case naming policy for JSON serialization
    /// This is from PR <see href="https://github.com/dotnet/corefx/pull/41354">#41354</see>
    /// </summary>
    public class JsonSnakeCaseNamingPolicy : JsonNamingPolicy
    {
        private static Lazy<JsonSnakeCaseNamingPolicy> _lazyInstance = new Lazy<JsonSnakeCaseNamingPolicy>(() => new JsonSnakeCaseNamingPolicy());
        public static JsonSnakeCaseNamingPolicy Instance => _lazyInstance.Value;

        public override string ConvertName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            // Allocates a string builder with the guessed result length,
            // where 5 is the average word length in English, and
            // max(2, length / 5) is the number of underscores.
            StringBuilder builder = new StringBuilder(name.Length + Math.Max(2, name.Length / 5));
            UnicodeCategory? previousCategory = null;

            for (int currentIndex = 0; currentIndex < name.Length; currentIndex++)
            {
                char currentChar = name[currentIndex];
                if (currentChar == '_')
                {
                    builder.Append('_');
                    previousCategory = null;
                    continue;
                }

                UnicodeCategory currentCategory = char.GetUnicodeCategory(currentChar);

                switch (currentCategory)
                {
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.TitlecaseLetter:
                        if (previousCategory == UnicodeCategory.SpaceSeparator ||
                            previousCategory == UnicodeCategory.LowercaseLetter ||
                            previousCategory != UnicodeCategory.DecimalDigitNumber &&
                            currentIndex > 0 &&
                            currentIndex + 1 < name.Length &&
                            char.IsLower(name[currentIndex + 1]))
                        {
                            builder.Append('_');
                        }

                        currentChar = char.ToLower(currentChar);
                        break;

                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.DecimalDigitNumber:
                        if (previousCategory == UnicodeCategory.SpaceSeparator)
                        {
                            builder.Append('_');
                        }
                        break;

                    case UnicodeCategory.Surrogate:
                        break;

                    default:
                        if (previousCategory != null)
                        {
                            previousCategory = UnicodeCategory.SpaceSeparator;
                        }
                        continue;
                }

                builder.Append(currentChar);
                previousCategory = currentCategory;
            }

            return builder.ToString();
        }
    }
}