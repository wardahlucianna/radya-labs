using System.Linq;
using System.Text.RegularExpressions;

namespace BinusSchool.User.FnUser.Utils
{
    public static class PasswordUtil
    {
        public static bool ValidatePassword(this string source)
        {
            var validCriteria = 0;

            if (source.Any(x => char.IsUpper(x)))
                validCriteria++;

            if (source.Any(x => char.IsLower(x)))
                validCriteria++;

            if (source.Any(x => char.IsDigit(x)))
                validCriteria++;

            //if (new Regex("[^a-z0-9]").IsMatch(source))
            //    validCriteria++;

            if (source.Length >= 8)
                validCriteria++;

            return validCriteria == 4;
        }
    }
}
