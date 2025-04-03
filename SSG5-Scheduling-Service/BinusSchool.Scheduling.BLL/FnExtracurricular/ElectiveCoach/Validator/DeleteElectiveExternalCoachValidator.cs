using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectiveCoach;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.ElectiveCoach.Validator
{
    public class DeleteElectiveExternalCoachValidator : AbstractValidator<DeleteElectiveExternalCoachRequest>
    {
        public DeleteElectiveExternalCoachValidator()
        {
            RuleFor(x => x.IdExtracurricularExternalCoach).NotEmpty();
        }
    }
}
