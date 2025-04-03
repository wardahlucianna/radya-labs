using System;
using System.Collections;
using System.Collections.Generic;
using BinusSchool.Common.Model;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace BinusSchool.Common.Comparers
{
    public class JsonDataAssignmentComparer : IEqualityComparer
    {
        private readonly IStringLocalizer _localizer;
        private readonly string _keyOfValueToCompare;

        public JsonDataAssignmentComparer(IStringLocalizer localizer, string keyOfValueToCompare)
        {
            _localizer = localizer;
            _keyOfValueToCompare = keyOfValueToCompare;    
        }

        public new bool Equals(object x, object y)
        {
            var dict = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(y.ToString());

            return dict.TryGetValue(_keyOfValueToCompare, out var compare)
                ? x.Equals(compare.Id)
                : throw new Exception(string.Format(_localizer["ExEmptyProperty"], _keyOfValueToCompare));
        }

        public int GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }
    }
}