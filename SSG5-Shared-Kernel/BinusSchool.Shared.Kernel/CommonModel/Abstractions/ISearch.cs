using System.Collections.Generic;

namespace BinusSchool.Common.Model.Abstractions
{
    public interface ISearch
    {
        string Search { get; set; }
        IEnumerable<string> SearchBy { get; set; }
        string SearchPattern();
    }
}