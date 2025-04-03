using System.Collections.Generic;
using System.Linq;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Common.Extensions
{
    public static class PaginationExtension
    {
        public static IDictionary<string, object> CreatePaginationProperty<T>(this T pagination, long totalItems)
            where T : IGetAll, IReturnCollection
        {
            if (pagination.Return == CollectionType.Lov)
                return new Dictionary<string, object>
                {
                    { nameof(PaginationProperty.TotalItem), totalItems }
                };

            var paginationProp = new PaginationProperty
            {
                Page = pagination.Page,
                Size = pagination.GetAll.HasValue && pagination.GetAll.Value 
                    ? (int)totalItems 
                    : pagination.Size,
                TotalItem = totalItems
            };
            
            return paginationProp.ToDictionary();
        }

        public static IDictionary<string, object> ToDictionary(this PaginationProperty pagination)
        {
            return new Dictionary<string, object>
            {
                { "page", pagination.Page },
                { "size", pagination.Size },
                { "totalPage", pagination.TotalPage },
                { "totalItem", pagination.TotalItem }
            };
        }
    }
}