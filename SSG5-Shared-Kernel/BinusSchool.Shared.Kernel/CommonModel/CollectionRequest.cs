using System.Collections.Generic;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Common.Model
{
    public class CollectionRequest : Pagination, IIdCollection, ISearch, IOrderBy, IGetAll, IReturnCollection
    {
        public IEnumerable<string> Ids { get; set; }
        public string Search { get; set; }
        public IEnumerable<string> SearchBy { get; set; }
        public string OrderBy { get; set; }
        public OrderType OrderType { get; set; }
        public bool? GetAll { get; set; }
        public CollectionType Return { get; set; }

        private string _searchPattern;
        public string SearchPattern()
        {
            return _searchPattern ??= !string.IsNullOrWhiteSpace(Search) ? $"%{Search}%" : "%";
        }

        public bool CanCountWithoutFetchDb(int itemsCount)
        {
            return Return == CollectionType.Lov || (GetAll.HasValue && GetAll.Value) || (Page == 1 && itemsCount < Size);
        }
    }
}
