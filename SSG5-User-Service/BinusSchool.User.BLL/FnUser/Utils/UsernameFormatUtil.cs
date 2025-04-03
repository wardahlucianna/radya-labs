using System;
using System.Collections.Generic;
using System.Linq;

namespace BinusSchool.User.FnUser.Utils
{
    public static class UsernameFormatUtil
    {
        public static bool Validate(this string source)
        {
            var delimiter = "|";
            var variables = new string[] { "S_", "Y_", "M_", "D_", "A_", "B_" };

            var items = source.Split(delimiter);
            if (items.Length == 0)
                return false;

            foreach (var item in items)
            {
                var hasMatch = false;
                foreach (var variable in variables)
                {
                    if (item.Contains(variable))
                    {
                        hasMatch = true;
                        break;
                    }
                }
                if (!hasMatch)
                    return false;
            }

            return true;
        }

        public static string GenerateUsername(this string source, int count, List<string> binusianIds)
        {
            if (string.IsNullOrEmpty(source) || !source.Validate())
                return string.Empty;

            var result = string.Empty;
            var delimiter = "|";

            var items = source.Split(delimiter);
            foreach (var item in items)
            {
                var split = item.Split("_");
                if (split.Length == 2)
                {
                    switch (split[0])
                    {
                        case "S":
                            result += split[1];
                            break;
                        case "Y":
                            result += DateTime.Now.ToString(split[1].ToLower());
                            break;
                        case "M":
                            result += DateTime.Now.ToString(split[1]);
                            break;
                        case "D":
                            result += DateTime.Now.ToString(split[1].ToLower());
                            break;
                        case "A":
                            if (int.TryParse(split[1], out var digit))
                            {

                                var displayed = count.ToString();
                                result += new string('0', digit > displayed.Length ? digit - displayed.Length : 0) + displayed;
                            }
                            break;
                        case "B":
                            result += string.Join("", binusianIds);
                            break;
                    }
                }
            }

            return result;
        }
    }
}
