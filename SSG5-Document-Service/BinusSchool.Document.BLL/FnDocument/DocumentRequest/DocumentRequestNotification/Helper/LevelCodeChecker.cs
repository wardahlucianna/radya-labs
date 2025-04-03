using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Document.BLL.FnDocument.DocumentRequest.DocumentRequestNotification.Helper
{
    internal static class LevelCodeChecker
    {
        private static readonly HashSet<string> highSchoolCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "HS",
            "S",
            "Secondary",
        };

        internal static bool IsHighSchool(string levelCode)
        {
            if (string.IsNullOrWhiteSpace(levelCode))
            {
                return false;
            }

            return highSchoolCodes.Contains(levelCode);
        }
    }
}
