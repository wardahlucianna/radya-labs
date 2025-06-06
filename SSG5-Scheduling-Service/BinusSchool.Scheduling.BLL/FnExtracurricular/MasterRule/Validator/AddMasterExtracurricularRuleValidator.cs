﻿using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterRule;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterRule.Validator
{
    public class AddMasterExtracurricularRuleValidator : AbstractValidator<UpdateMasterExtracurricularRuleRequest>
    {
        public AddMasterExtracurricularRuleValidator()
        {
            RuleFor(x => x.IdAcademicYear).NotEmpty();
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.MinEffectives).GreaterThanOrEqualTo(0);
            RuleFor(x => x.MaxEffectives).GreaterThan(0).GreaterThanOrEqualTo(x => x.MinEffectives);
            RuleFor(x => x.Grades).NotEmpty();

            When(x => x.RegistrationStartDate != null, () =>
            {
                RuleFor(x => x.RegistrationEndDate).NotEmpty();
                RuleFor(x => x.RegistrationEndDate).GreaterThan(x => x.RegistrationStartDate).WithMessage("End date must be greater than start date");                
            });         
            RuleFor(x => x.DueDayPayment).NotEmpty();
        }
    }
}
