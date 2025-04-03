using System.Collections.Generic;

namespace BinusSchool.Attendance.BLL.FnAttendance.Extensions
{
    public static class EnumerableExtension
    {
        public static int IndexOf<T>(this IEnumerable<T> source, T item)
        {
            return source.IndexOf(item, EqualityComparer<T>.Default);
        }
        public static int IndexOf<T>(this IEnumerable<T> source, T item, IEqualityComparer<T> comparer)
        {
            IList<T> list = source as IList<T>;
            if (list != null)
                return list.IndexOf(item);

            int i = 0;
            foreach (T x in source)
            {
                if (comparer.Equals(x, item))
                    return i;
            }
            return -1;
        }
    }
}
