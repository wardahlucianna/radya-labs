using System.Collections.Generic;

namespace BinusSchool.Attendance.FnAttendance.Utils
{
    public static class StringReplaceUtil
    {
        public static string ReplaceVariable(this string source, IDictionary<string, object> replace)
        {
            foreach (var item in replace)
            {
                source = source.Replace("{{" + item.Key + "}}", item.Value.ToString());
            }

            return source;
        }
    }
}
