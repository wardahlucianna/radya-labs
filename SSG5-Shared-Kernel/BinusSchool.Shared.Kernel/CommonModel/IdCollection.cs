using System.Collections.Generic;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Common.Model
{
    public class IdCollection : IIdCollection
    {
        public IdCollection() {}
        
        public IdCollection(IEnumerable<string> ids)
        {
            Ids = ids;
        }

        public IEnumerable<string> Ids { get; set; }
    }
}