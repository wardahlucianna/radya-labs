using System;
using Microsoft.Extensions.Localization;

namespace BinusSchool.Common.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string message, IStringLocalizer localizer = null) : 
            base(message ?? (localizer is null ? "Requested resource(s) is not found." : localizer["ExNotFound"])) { }
    }
}
