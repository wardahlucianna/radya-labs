using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricular.Validator
{
    public class UpdateElectivesEntryPeriodValidator : AbstractValidator<UpdateElectivesEntryPeriodRequest>
    {
        public UpdateElectivesEntryPeriodValidator()
        {
            RuleFor(x => x.Electives).NotEmpty();
        }
    }
}
