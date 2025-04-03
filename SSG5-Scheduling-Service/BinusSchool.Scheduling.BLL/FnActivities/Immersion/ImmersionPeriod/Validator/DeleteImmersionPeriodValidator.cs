using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.ImmersionPeriod;
using FluentValidation;

namespace BinusSchool.Scheduling.FnActivities.Immersion.ImmersionPeriod.Validator
{
    public class DeleteImmersionPeriodValidator : AbstractValidator<DeleteImmersionPeriodRequest>
    {
        public DeleteImmersionPeriodValidator()
        {
            RuleFor(x => x.IdImmersionPeriod).NotEmpty();
        }
    }
}
