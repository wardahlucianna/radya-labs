using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterRule;
using FluentValidation;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterRule.Validator
{
    public class CopyExtracurricularRuleFromLastAYValidator : AbstractValidator<CopyExtracurricularRuleFromLastAYRequest>
    {
        public CopyExtracurricularRuleFromLastAYValidator()
        {
            RuleFor(x => x.IdAcademicYear)
                .NotEmpty();

            RuleFor(x => x.IdExtracurricularRule)
                .NotEmpty();
        }
    }
}
