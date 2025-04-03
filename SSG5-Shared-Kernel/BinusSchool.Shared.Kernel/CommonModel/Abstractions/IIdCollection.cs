using System.Collections.Generic;

namespace BinusSchool.Common.Model.Abstractions
{
    public interface IIdCollection
    {
        IEnumerable<string> Ids { get; set; }
    }
}