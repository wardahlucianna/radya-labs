using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace BinusSchool.Common.Utils
{
    public static class JsonUtil
    {
        public static IEnumerable<string> CollectIdFromJson(params string[] jsonIds)
        {
            var collected = new List<string>();
            foreach (var json in jsonIds)
            {
                var ids = JsonConvert.DeserializeObject<IEnumerable<string>>(json);
                collected.AddRange(ids);
            }
            return collected.Distinct();
        }
    }
}