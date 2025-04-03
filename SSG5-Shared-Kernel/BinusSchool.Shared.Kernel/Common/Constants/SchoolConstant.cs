using System;
using System.Collections.Generic;

namespace BinusSchool.Common.Constants
{
    public class SchoolConstant
    {
        private static readonly Lazy<string[]> _idSchools = new Lazy<string[]>(new[] { "1","2","3","4" });

        public static IReadOnlyList<string> IdSchools => _idSchools.Value;
    }
}
