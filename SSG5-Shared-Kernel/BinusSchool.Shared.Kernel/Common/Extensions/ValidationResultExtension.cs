using System.Linq;
using BinusSchool.Common.Exceptions;
using FluentValidation.Results;
using Microsoft.Extensions.Localization;

namespace BinusSchool.Common.Extensions
{
    public static class GenericExtension
    {
        public static ValidationResult EnsureValid(this ValidationResult validation, bool throwInvalid = true, IStringLocalizer localizer = null)
        {
            if (!validation.IsValid && throwInvalid)
            {
                throw new ModelValidationException(
                    localizer: localizer,
                    failures: validation.Errors
                        .Select(f => new { f.PropertyName,  f.ErrorMessage })
                        .GroupBy(f => f.PropertyName)
                        .ToDictionary(f => f.Key, f => f.Select(x => x.ErrorMessage)));
            }

            return validation;
        }
    }
}