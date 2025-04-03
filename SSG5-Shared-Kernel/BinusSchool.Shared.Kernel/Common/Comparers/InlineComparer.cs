using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BinusSchool.Common.Comparers
{
    public class InlineComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _getEquals;
        private readonly Func<T, int> _getHashCode;

        public InlineComparer(Func<T, T, bool> equals, Func<T, int> hashCode)
        {
            _getEquals = equals;
            _getHashCode = hashCode;
        }

        public bool Equals([AllowNull] T x, [AllowNull]T y)
        {
            return _getEquals(x, y);
        }

        public int GetHashCode([DisallowNull] T obj)
        {
            return _getHashCode(obj);
        }
    }
}