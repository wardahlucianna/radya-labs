using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Common.Comparers
{
    public class UniqueIdComparer<T> : IEqualityComparer<T> where T : IUniqueId
    {
        public bool Equals([AllowNull] T x, [AllowNull] T y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode([DisallowNull] T obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}