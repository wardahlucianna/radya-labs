using System;
using Microsoft.Extensions.Localization;

namespace BinusSchool.Common.Exceptions
{
    public class UnauthorizeException : Exception
    {
        public UnauthorizeException(string message = null, IStringLocalizer localizer = null) : 
            base(message ?? (localizer is null ? "You are not authorized." : localizer["ExUnauthorize"])) { }
    }
}
