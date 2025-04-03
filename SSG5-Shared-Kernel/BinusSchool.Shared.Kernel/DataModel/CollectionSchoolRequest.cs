using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Data.Model
{
    public class CollectionSchoolRequest : CollectionRequest, IMultiSchool
    {
        public IEnumerable<string> IdSchool { get; set; }
    }
}
