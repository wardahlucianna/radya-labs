using System;

namespace BinusSchool.Common.Utils
{
    public static class TimeSpanUtil
    {
        public static bool IsIntersect(TimeSpan sourceStart, TimeSpan sourceEnd, TimeSpan compareStart, TimeSpan compareEnd)
        {
            if (sourceStart > sourceEnd || compareStart > compareEnd)
                throw new ArgumentOutOfRangeException();

            if (sourceStart == sourceEnd || compareStart == compareEnd)
                return false; // No actual date range

            if (sourceStart == compareStart || sourceEnd == compareEnd)
                return true; // If any set is the same time, then by default there must be some overlap. 

            if (sourceStart < compareStart)
            {
                if (sourceEnd > compareStart && sourceEnd < compareEnd)
                    return true;

                if (sourceEnd > compareEnd)
                    return true;
            }
            else
            {
                if (compareEnd > sourceStart && compareEnd < sourceEnd)
                    return true;

                if (compareEnd > sourceEnd)
                    return true;
            }

            return false;
        }
    }
}