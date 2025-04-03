using System;
using System.Collections.Generic;
using System.Linq;
using BinusSchool.Data.Model.School.FnPeriod.Period;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace BinusSchool.School.FnPeriod.Period.Validator
{
    public class AddPeriodValidator : AbstractValidator<AddPeriodRequest>
    {
        public AddPeriodValidator(IServiceProvider provider)
        {
            var localizer = provider.GetService<IStringLocalizer>();

            RuleFor(x => x.IdAcadyear).NotEmpty();

            RuleFor(x => x.IdGrades)
                .NotEmpty()
                .Must(x => x.Distinct().Count() == x.Count())
                .WithMessage(x => string.Format(localizer["ExNotUnique"], nameof(x.IdGrades)))
                .ForEach(ids => ids.ChildRules(id => id.RuleFor(x => x).NotEmpty()));

            RuleFor(x => x.Terms)
                .NotEmpty()
                .ForEach(terms => terms.SetValidator(x => new TermValidator(x)));
        }
    }
}
