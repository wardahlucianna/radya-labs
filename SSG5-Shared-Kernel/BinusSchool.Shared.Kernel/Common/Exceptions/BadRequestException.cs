using System;
using Microsoft.Extensions.Localization;

namespace BinusSchool.Common.Exceptions
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message, IStringLocalizer localizer = null) : 
            base(message ?? (localizer is null ? "Oops. There was someting wrong with your request." : localizer["ExBadRequest"])) { }
    }
}
