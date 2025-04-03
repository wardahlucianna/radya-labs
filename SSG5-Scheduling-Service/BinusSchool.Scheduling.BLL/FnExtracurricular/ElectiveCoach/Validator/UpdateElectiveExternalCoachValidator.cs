using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectiveCoach;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.ElectiveCoach.Validator
{   
    public class UpdateElectiveExternalCoachValidator : AbstractValidator<UpdateElectiveExternalCoachRequest>
    {
        public UpdateElectiveExternalCoachValidator()
        {          
            RuleFor(x => x.IdUser).NotEmpty();
            RuleFor(x => x.UserName).NotEmpty();
            RuleFor(x => x.IdExtracurricularExtCoachTaxStatus).NotEmpty();

        }
    }
}
