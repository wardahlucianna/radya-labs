using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;

namespace BinusSchool.Common.Exceptions
{
    public class ModelValidationException : Exception
    {
        public ModelValidationException(string message = null, IDictionary<string, IEnumerable<string>> failures = null, IStringLocalizer localizer = null) : 
            base(message ?? (localizer is null ? "One or more validation failures have occurred." : localizer["ExValidation"]))
        {
            Failures = failures ?? new Dictionary<string, IEnumerable<string>>();
        }

        public IDictionary<string, IEnumerable<string>> Failures { get; }
    }
}
