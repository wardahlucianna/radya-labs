using System;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace BinusSchool.Common.Utils
{
    public static class StringUtil
    {
        private const string _urlPattern = @"^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$";

        private static readonly Lazy<Regex> _regexUrlPattern = new Lazy<Regex>(new Regex(_urlPattern, RegexOptions.IgnoreCase));

        public static bool IsValidUrl(string url)
        {
            return _regexUrlPattern.Value.IsMatch(url);
        }

        public static bool IsValidEmailAddress(string email)
        {
            try
            {
                new MailAddress(email);
            }
            catch 
            {
                return false;
            }

            return true;
        }
    }
}
