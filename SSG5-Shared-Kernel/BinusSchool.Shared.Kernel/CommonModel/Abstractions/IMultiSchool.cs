using System.Collections.Generic;

namespace BinusSchool.Common.Model.Abstractions
{
    public interface IMultiSchool
    {
        IEnumerable<string> IdSchool { get; set; }
    }
}