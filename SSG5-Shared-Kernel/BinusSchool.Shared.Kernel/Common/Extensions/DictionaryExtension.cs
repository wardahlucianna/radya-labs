using System;
using System.Collections.Generic;
using System.Linq;
using BinusSchool.Common.Model;

namespace BinusSchool.Common.Extensions
{
    public static class DictionaryExtension
    {
        public static IDictionary<string, object> AddProperty(this IDictionary<string, object> dictionary, params KeyValuePair<string, object>[] keyValues)
        {
            foreach (var kv in keyValues)
            {
                if (kv.Key != null)
                    dictionary.Add(kv);
            }

            return dictionary;
        }

        public static PaginationProperty GetPaginationProperty(this IDictionary<string, object> dictionary)
        {
            var pagination = new PaginationProperty();
            if (dictionary.TryGetValue("page", out var obj) && int.TryParse(obj.ToString(), out var page))
                pagination.Page = page;
            if (dictionary.TryGetValue("size", out obj) && int.TryParse(obj.ToString(), out var size))
                pagination.Size = size;
            if (dictionary.TryGetValue("totalItem", out obj) && int.TryParse(obj.ToString(), out var totalItem))
                pagination.TotalItem = totalItem;
                
            return pagination;
        }

        public static IDictionary<string, object> AddColumnProperty(this IDictionary<string, object> dictionary, params string[] columns)
        {
            dictionary.Add("columns", columns);
            
            return dictionary;
        }

        public static IDictionary<string, object> AddColumnProperty(this IDictionary<string, object> dictionary, object columns)
        {
            dictionary.Add("columns", columns);

            return dictionary;
        }

        public static UndeletedResult2 GetUndeletedResult2(this IDictionary<string, IEnumerable<string>> dictionary)
        {
            var undeletedResult = new UndeletedResult2
            {
                NotFound = dictionary
                    .Where(x => x.Value.Contains("NotFound"))
                    .ToDictionary(x => x.Key, x => x.Value.First()),
                AlreadyUse = dictionary
                    .Where(x => x.Value.Contains("AlreadyUse"))
                    .ToDictionary(x => x.Key, x => x.Value.First()),
                CurrentlyRun = dictionary
                    .Where(x => x.Value.Contains("CurrentlyRun"))
                    .ToDictionary(x => x.Key, x => x.Value.First())
            };

            return undeletedResult;
        }
    }
}
