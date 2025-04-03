using System;
using System.Collections.Generic;
using BinusSchool.Data.Model.School.FnPeriod.Period;
using FluentValidation;

namespace BinusSchool.School.FnPeriod.Period.Validator
{
    public class UpdatePeriodValidator : AbstractValidator<UpdatePeriodRequest>
    {
        public UpdatePeriodValidator()
        {
            RuleFor(x => x.Id).NotEmpty();

            RuleFor(x => x.Terms)
                .NotEmpty()
                .ForEach(terms => terms.SetValidator(x => new TermValidator(x)));
        }
    }
}
