using System;
using System.Collections.Generic;
using System.Linq;
using BinusSchool.Data.Model.School.FnSubject.Subject;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace BinusSchool.School.FnSubject.Subject.Validator
{
    public class TotalSessionValidator : AbstractValidator<IEnumerable<SessionCollection>>
    {
        public TotalSessionValidator(int maxSession, IStringLocalizer localizer)
        {
            RuleFor(x => x)
                .Must(x =>
                {
                    var total = x.Aggregate(0, (sum, session) => sum += session.Length * session.Count);

                    return total <= maxSession;
                })
                .WithMessage(string.Format(localizer["ExNotEqual"], localizer["TotalSession"], localizer["MaxSession"]));
        }
    }
}
