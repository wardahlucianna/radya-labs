using System;
using Microsoft.Extensions.Localization;

namespace BinusSchool.Common.Exceptions
{
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message = null, IStringLocalizer localizer = null) : 
            base(message ?? (localizer is null ? "You are forbid to access this resource." : localizer["ExForbindden"])) { }
    }
}
