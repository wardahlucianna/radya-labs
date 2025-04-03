using System;
using System.Collections.Generic;

namespace BinusSchool.Common.Model
{
    public class UndeletedResult2
    {
        public IDictionary<string, string> NotFound { get; set; }
        public IDictionary<string, string> AlreadyUse { get; set; }
        public IDictionary<string, string> CurrentlyRun { get; set; }

        public IDictionary<string, IEnumerable<string>> AsErrors()
        {
            if (NotFound?.Count == 0 && AlreadyUse?.Count == 0 && CurrentlyRun?.Count == 0)
                return null;
            
            var result = new Dictionary<string, IEnumerable<string>>();

            if (NotFound != null)
                foreach (var notFound in NotFound)
                    result.Add(notFound.Key, new[] { notFound.Value, "NotFound" });

            if (AlreadyUse != null)
                foreach (var alreadyUse in AlreadyUse)
                    result.Add(alreadyUse.Key, new[] { alreadyUse.Value, "AlreadyUse" });

            if (CurrentlyRun != null)
                foreach (var currentlyRun in CurrentlyRun)
                    result.Add(currentlyRun.Key, new[] { currentlyRun.Value, "CurrentlyRun" });

            return result;
        }
    }
}