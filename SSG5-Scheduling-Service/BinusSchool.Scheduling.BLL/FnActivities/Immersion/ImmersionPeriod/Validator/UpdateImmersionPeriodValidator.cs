using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.ImmersionPeriod;
using FluentValidation;

namespace BinusSchool.Scheduling.FnActivities.Immersion.ImmersionPeriod.Validator
{
    public class UpdateImmersionPeriodValidator : AbstractValidator<UpdateImmersionPeriodRequest>
    {
        public UpdateImmersionPeriodValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.Semester).NotEmpty();
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.RegistrationStartDate).NotEmpty();
            RuleFor(x => x.RegistrationEndDate).NotEmpty();
        }
    }
}
